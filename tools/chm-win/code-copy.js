/* ============================================
    PowerModel Code Script (Cookie Theme Support)
    Version: 1.2
    Encoding: UTF-8
    Compatible: IE6+ (Tested in CHM)
   ============================================ */

/**
 * [修改] 自动加载并应用用户保存的主题样式 (通过 Cookie)
 */
/* ============================================
    PowerModel Code Script (UserData Support)
    Compatible: IE6+ (Special for CHM)
   ============================================ */
// 下面是你原本的 copyCode 等函数...

/**
 * 复制代码到剪贴板
 * @param {HTMLElement} btn - 点击的按钮元素
 */
function copyCode(btn) {
    var pre = btn.parentNode.getElementsByTagName("pre")[0];
    var codeText = pre.innerText || pre.textContent;
    
    var textArea = document.createElement("textarea");
    textArea.value = codeText;
    document.body.appendChild(textArea);
    textArea.select();
    document.execCommand("copy");
    document.body.removeChild(textArea);
    
    // 使用 Unicode 编码确保 CHM 内部不乱码
    btn.innerHTML = "\u5df2\u590d\u5236"; // "已复制"
    window.setTimeout(function() {
        btn.innerHTML = "\u590d\u5236"; // "复制"
    }, 2000);
}

/**
 * 复制代码（替代版本，带成功回调）
 */
function copyCodeEx(btn, successText, delay) {
    successText = successText || "\u5df2\u590d\u5236";
    delay = delay || 2000;
    
    var pre = btn.parentNode.getElementsByTagName("pre")[0];
    var codeText = pre.innerText || pre.textContent;
    
    var textArea = document.createElement("textarea");
    textArea.value = codeText;
    document.body.appendChild(textArea);
    textArea.select();
    
    var success = document.execCommand("copy");
    document.body.removeChild(textArea);
    
    if (success) {
        var originalText = btn.innerHTML;
        btn.innerHTML = successText;
        window.setTimeout(function() {
            btn.innerHTML = originalText;
        }, delay);
    }
    
    return success;
}