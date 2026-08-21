# UltraLib Helper —— WinCHM 工程

这里是 UltraLib 的 **WinCHM 离线文档工程**(与在线 Wiki / 仓库 docs 内容同源,但改用 WinCHM 编译、风格更贴近 CHM 阅读体验)。

## 目录结构

```
tools/chm-win/
├─ UltraLibHelper.wcp        # WinCHM 工程文件(用 WinCHM 打开)
├─ code-style.css            # 全局样式(复制按钮、代码框、语法高亮、info-box)
├─ code-copy.js              # 代码复制脚本(复制按钮交互)
└─ data/                     # 页面源(每个子目录一页,HTML)
   ├─ 前言/
   ├─ 检查更新/              # 版本检测 + GitHub/Gitee 下载交互页
   ├─ 简单认识C#/
   ├─ 更新日志/
   ├─ 入门模组开发教学/       # 占位骨架,章节内容后续补充
   ├─ 新Hooks表/             # UltraLib 新增 Hook 总表
   └─ 新Utils表/             # UltraLib 新增 Utils 总表
```

## 如何编译

1. **安装 WinCHM**(https://www.everbsoft.com/winchm.html)。
2. 用 WinCHM 打开 `tools/chm-win/UltraLibHelper.wcp`。
3. 左侧目录树应显示:
   ```
   UltraLib Helper
   ├─ 前言
   ├─ 检查更新
   ├─ 简单认识C#
   ├─ 更新日志
   ├─ 入门模组开发教学
   └─ 新增功能
      ├─ 新Hooks表
      └─ 新Utils表
   ```
4. 点击 **编译(Compile)** 按钮,输出 `UltraLib.chm`(在编译选项中可指定输出位置,默认与工程同目录或按 wcp 设置)。

## 维护规则

- **新增页面**:在 `data/` 下建新子目录写 HTML,然后在 WinCHM 里右键目录树 → 添加文件夹/页面;或直接改 `UltraLibHelper.wcp` 的 `[TOPICS]`(每个页面一组 `TitleList.*.N`,Level 控制层级)。
- **页面风格**:所有页面引用 `../../code-style.css` 与 `../../code-copy.js`(页面位于 `data/<子目录>/` 下,需上溯两级到工程根),代码框用 `class="code-box"` + 复制按钮,说明用 `class="info-box"`。新增页面务必复用这两个文件,保持风格统一。
- **更新日志**:每次发布在 `data/更新日志/更新日志.htm` 中追加,并把 `data/检查更新/检查更新.htm` 里的 `CONFIG.current` 同步为最新版本号。
- 编译产物(UltraLib.chm)由 WinCHM 生成,随发布提供(`tools/chm-win/*.chm` 不入库)。

## 与仓库其它文档的关系

- `docs/`(Markdown)→ 在线 Wiki 内容源
- `tools/chm-win/`(本工程)→ **WinCHM 离线手册**(CHM 的主用工程)

> 建议:以 `docs/` 为内容权威源;需要精修/排版时改这里的 HTML。