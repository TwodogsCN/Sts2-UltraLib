using MegaCrit.Sts2.Core.Localization;

namespace UltraLib.Base.Utils;

/// <summary>
/// <see cref="LocString"/> 本地化字符串辅助工具。
/// </summary>
public static class LocStringHelper
{
    /// <summary>
    /// 将 <see cref="LocString"/> 解析为格式化后的纯文本字符串。
    /// </summary>
    /// <param name="locString">要解析的本地化字符串。</param>
    /// <returns>格式化后的文本。</returns>
    public static string ToFormattedString(LocString locString)
        => locString.GetFormattedText();
}
