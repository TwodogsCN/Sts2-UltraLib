#!/usr/bin/env node
// UltraLib CHM builder
// Converts bilingual Markdown docs into a CHM-compatible HTML help project
// (.hhc table of contents + .hhp project + HTML pages), compiled with hhc.exe.
//
// Usage:  node build-chm.js [--source <docsDir>] [--out <outDir>]
// Then compile the produced .hhp with HTML Help Workshop's hhc.exe:
//   hhc.exe out/UltraLib.hhp   ->   out/UltraLib.chm

const fs = require('fs');
const path = require('path');

// ---- Small, dependency-free Markdown -> HTML subset renderer ---------------
// Handles the constructs used by UltraLib docs: headings, lists (incl. nested
// and task checkboxes), tables, fenced code, blockquotes, hr, bold/italic/code,
// and links. It is intentionally minimal — enough for our documentation set.

function inline(text) {
  return text
    .replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;')
    .replace(/\*\*([^*]+)\*\*/g, '<strong>$1</strong>')
    .replace(/(?<!\*)\*([^*]+)\*(?!\*)/g, '<em>$1</em>')
    .replace(/`([^`]+)`/g, '<code>$1</code>')
    .replace(/\[([^\]]+)\]\(([^)]+)\)/g, (m, txt, href) => {
      let u = href.replace(/\.md$/i, '.html');  // .md -> .html
      u = u.replace(/^(\.\.\/)+/, '');          // CHM output is flat: drop ../ prefixes
      u = u.replace(/^\.\//, '');               // drop ./
      return `<a href="${u}">${inline(txt)}</a>`;
    });
}

function render(md) {
  const lines = md.split(/\r?\n/);
  const out = [];
  let inCode = false, codeBuf = [], i = 0;

  while (i < lines.length) {
    const line = lines[i];

    if (inCode) {
      if (/^\s*```/.test(line)) { inCode = false; out.push(`<pre><code>${codeBuf.join('\n')}</code></pre>`); codeBuf = []; }
      else codeBuf.push(line.replace(/&/g, '&amp;').replace(/</g, '&lt;').replace(/>/g, '&gt;'));
      i++; continue;
    }
    if (/^\s*```/.test(line)) { inCode = true; i++; continue; }

    // horizontal rule
    if (/^\s*---+/.test(line) && !/^\s*\|/.test(line)) { out.push('<hr/>'); i++; continue; }

    // headings
    const h = line.match(/^(#{1,6})\s+(.*)$/);
    if (h) { const lv = h[1].length; out.push(`<h${lv}>${inline(h[2])}</h${lv}>`); i++; continue; }

    // table: consume consecutive rows until blank
    if (/^\s*\|/.test(line)) {
      const rows = [];
      while (i < lines.length && /^\s*\|/.test(lines[i])) { rows.push(lines[i]); i++; }
      out.push(renderTable(rows));
      continue;
    }

    // blockquote
    if (/^\s*>\s?/.test(line)) {
      const q = [];
      while (i < lines.length && /^\s*>\s?/.test(lines[i])) { q.push(lines[i].replace(/^\s*>\s?/, '')); i++; }
      out.push(`<blockquote>${render(q.join('\n'))}</blockquote>`);
      continue;
    }

    // lists (unordered, ordered, nested via indent)
    if (/^\s*[-*+]\s+/.test(line) || /^\s*\d+[.)]\s+/.test(line)) {
      const frag = [];
      while (i < lines.length && (/^\s*[-*+]\s+/.test(lines[i]) || /^\s*\d+[.)]\s+/.test(lines[i]) || /^\s{2,}/.test(lines[i]))) {
        frag.push(lines[i]); i++;
      }
      out.push(renderList(frag));
      continue;
    }

    // blank line
    if (/^\s*$/.test(line)) { i++; continue; }

    // paragraph: gather until blank / block start
    const para = [];
    while (i < lines.length && !/^\s*$/.test(lines[i]) && !/^(#{1,6}\s|```|\s*>\s?|\s*\|)/.test(lines[i])
          && !/^\s*[-*+]\s+/.test(lines[i]) && !/^\s*\d+[.)]\s+/.test(lines[i])) {
      para.push(lines[i]); i++;
    }
    out.push(`<p>${inline(para.join(' '))}</p>`);
  }
  return out.join('\n');
}

function renderList(frag) {
  // Minimal nested-list renderer: depth is inferred from leading indentation.
  const html = [];
  const stack = [];     // stack of open tag kinds ('ul'|'ol')
  const depths = [];    // indent depth corresponding to each open level
  let prevDepth = -1;

  const open = (depth, ordered) => {
    const tag = ordered ? 'ol' : 'ul';
    html.push(`<${tag}>`);
    stack.push(tag);
    depths.push(depth);
  };
  const closeTo = (depth) => {
    while (stack.length && depths[depths.length - 1] > depth) {
      html.push(`</${stack.pop()}>`);
      depths.pop();
    }
  };

  for (const raw of frag) {
    const m = raw.match(/^(\s*)([-*+]|\d+[.)])\s+(.*)$/);
    if (!m) {
      // continuation (wrapped) line -> append to previous item
      const last = html[html.length - 1];
      if (last && last.startsWith('<li>')) html[html.length - 1] = last + ' ' + inline(raw.trim());
      continue;
    }
    const indent = m[1].replace(/\t/g, '  ').length;
    const ordered = /^\d+/.test(m[2]);
    let content = m[3];
    let checked = null;
    const cb = content.match(/^\[( |x|X)\]\s+(.*)$/);
    if (cb) { content = cb[2]; checked = cb[1].toLowerCase() === 'x'; }

    if (indent > prevDepth) open(indent, ordered);
    else if (indent < prevDepth) closeTo(indent - 1);

    const item = checked === null ? inline(content)
      : `<label><input type="checkbox" ${checked ? 'checked' : ''} disabled/> ${inline(content)}</label>`;
    html.push(`<li>${item}</li>`);
    prevDepth = indent;
  }
  while (stack.length) { html.push(`</${stack.pop()}>`); depths.pop(); }
  return html.join('\n');
}

function renderTable(rows) {
  const cells = (r) => r.replace(/^\s*\|/, '').replace(/\|\s*$/, '').split('|').map(s => s.trim());
  const head = cells(rows[0]);
  // skip separator row (index 1)
  const body = rows.slice(2).map(r => `<tr>${cells(r).map(c => `<td>${inline(c)}</td>`).join('')}</tr>`).join('');
  return `<table><thead><tr>${head.map(c => `<th>${inline(c)}</th>`).join('')}</tr></thead><tbody>${body}</tbody></table>`;
}

// ---- Page wrapper + TOC ----------------------------------------------------

function htmlPage(title, bodyHtml, nav) {
  return `<!DOCTYPE html>
<html lang="en"><head><meta charset="utf-8"/>
<title>${title} — UltraLib</title>
<style>
body{font-family:Segoe UI,Arial,sans-serif;margin:0;color:#1a1a1a;}
header{background:#1f2937;color:#fff;padding:14px 22px;font-size:18px;font-weight:600;}
nav.wiki{font-size:12px;padding:8px 22px;border-bottom:1px solid #e5e7eb;}
main{max-width:860px;margin:0 auto;padding:24px 30px 60px;line-height:1.6;}
h1{font-size:28px;border-bottom:2px solid #e5e7eb;padding-bottom:8px;}
h2{font-size:21px;margin-top:30px;}
h3{font-size:17px;margin-top:22px;}
code{background:#f3f4f6;border-radius:3px;padding:1px 5px;font-family:Consolas,monospace;font-size:90%;}
pre{background:#f6f8fa;border:1px solid #e5e7eb;border-radius:6px;padding:12px;overflow:auto;}
pre code{background:none;padding:0;}
table{border-collapse:collapse;width:100%;margin:12px 0;}
th,td{border:1px solid #d1d5db;padding:7px 10px;text-align:left;vertical-align:top;}
th{background:#f3f4f6;}
blockquote{border-left:4px solid #cbd5e1;margin:12px 0;padding:2px 14px;color:#374151;background:#f9fafb;}
a{color:#2563eb;text-decoration:none;}a:hover{text-decoration:underline;}
</style></head>
<body><header>UltraLib — <span style="font-weight:400">StS2 base library docs</span></header>
${nav}<main>${bodyHtml}</main>
</body></html>`;
}

// ---- Main ------------------------------------------------------------------

function main() {
  const args = process.argv.slice(2);
  const get = (flag) => { const j = args.indexOf(flag); return j >= 0 ? args[j + 1] : null; };
  const repoRoot = path.resolve(__dirname, '../../');
  const srcDir = get('--source') ? path.resolve(get('--source')) : path.join(repoRoot, 'docs');
  const outDir = get('--out') ? path.resolve(get('--out')) : path.join(__dirname, 'out');
  fs.mkdirSync(outDir, { recursive: true });

  // Pages to build: (markdownFile, title). Both language variants merged into
  // one CHM with a combined TOC, grouped by topic.
  const pages = [
    ['README.md', 'README', path.join(repoRoot, 'README.md')],
    ['README.zh-CN.md', 'README（中文）', path.join(repoRoot, 'README.zh-CN.md')],
    ['API_INDEX.md', 'API / Feature Index', path.join(srcDir, 'API_INDEX.md')],
    ['API_INDEX.zh-CN.md', '功能 / API 索引（中文）', path.join(srcDir, 'API_INDEX.zh-CN.md')],
    ['CODE_CONVENTIONS.md', 'Code Conventions', path.join(srcDir, 'CODE_CONVENTIONS.md')],
    ['CODE_CONVENTIONS.zh-CN.md', '代码规范（中文）', path.join(srcDir, 'CODE_CONVENTIONS.zh-CN.md')],
    ['DEVELOPMENT_WORKFLOW.md', 'Development Workflow', path.join(srcDir, 'DEVELOPMENT_WORKFLOW.md')],
    ['DEVELOPMENT_WORKFLOW.zh-CN.md', '开发流程（中文）', path.join(srcDir, 'DEVELOPMENT_WORKFLOW.zh-CN.md')],
  ];

  const tocItems = []; // <li> items (flat; hhc supports nesting via <ul>)
  const navLinks = []; // [filename.html, label]
  for (const [fname, title, fpath] of pages) {
    if (!fs.existsSync(fpath)) { console.warn(`skip missing: ${fpath}`); continue; }
    const md = fs.readFileSync(fpath, 'utf8');
    const body = render(md);
    const nav = `<p>` + navLinks.map(([f, t]) => `<a href="${f}">${t}</a>`).join(' · ') + `</p>`;
    fs.writeFileSync(path.join(outDir, fname.replace(/\.md$/, '.html')), htmlPage(title, body, nav));
    tocItems.push(`<LI><OBJECT type="text/sitemap"><param name="Name" value="${esc(title)}"><param name="Local" value="${fname.replace(/\.md$/, '.html')}"></OBJECT></LI>`);
    navLinks.push([fname.replace(/\.md$/, '.html'), title]);
  }

  // .hhc table of contents
  const hhc = `<HTML><HEAD></HEAD><BODY><UL>
<LI><OBJECT type="text/sitemap"><param name="Name" value="UltraLib 文档"><param name="Local" value="README.html"></OBJECT>
<UL>
${tocItems.join('\n')}
</UL></LI>
</UL></BODY></HTML>`;

  // .hhp help project
  const hhp = `[OPTIONS]
Compatibility=1.1 or later
Compiled file=UltraLib.chm
Contents file=UltraLib.hhc
Default topic=README.html
Display compile progress=No
Language=0x804 中文(简体)
Title=UltraLib Documentation

[FILES]
${pages.map(([f]) => f.replace(/\.md$/, '.html')).filter(f => fs.existsSync(path.join(outDir, f))).join('\n')}

[INFOTYPES]
`;

  fs.writeFileSync(path.join(outDir, 'UltraLib.hhc'), hhc);
  fs.writeFileSync(path.join(outDir, 'UltraLib.hhp'), hhp);
  console.log('Built CHM sources in ' + outDir);
  console.log('Next: run hhc.exe out/UltraLib.hhp to produce UltraLib.chm');
}

function esc(s) { return s.replace(/&/g, '&amp;').replace(/</g, '&lt;'); }

main();
