using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace UltraLib.HoverTip;

/// <summary>
/// UltraLib 预定义的悬停提示（HoverTip）枚举。
/// <para>
/// 每个枚举值对应本地化键 <c>static_hover_tips/ULTRALIB-{Slugified名称}.base.title</c>
/// 和 <c>static_hover_tips/ULTRALIB-{Slugified名称}.base.description</c>。
/// </para>
/// </summary>
public enum PlusHoverTip
{
    /// <summary>
    /// 返回 —— 卡牌使用后返回手牌。
    /// </summary>
    Return = 0,

    /// <summary>
    /// 注能 —— 为卡牌注入特定能力。
    /// </summary>
    Empower = 1,

    /// <summary>
    /// 移动 —— 卡牌在手牌/牌组间移动。
    /// </summary>
    Move = 2,

    /// <summary>
    /// 被偷 —— 金币或卡牌被偷取。
    /// </summary>
    Stolen = 3,

    /// <summary>
    /// 回溯 —— 时间倒流类的效果。
    /// </summary>
    Rewind = 4,

    /// <summary>
    /// 生命 —— 与生命值相关的提示。
    /// </summary>
    Hp = 5,

    /// <summary>
    /// 能力 —— 与能力相关的提示。
    /// </summary>
    Powers = 6,

    /// <summary>
    /// 仅能力 —— 仅显示能力相关的提示。
    /// </summary>
    OnlyPower = 7,

    /// <summary>
    /// 充能 —— 遗物充能相关的提示。
    /// </summary>
    Charge = 8,

    /// <summary>
    /// 套装 —— 与套装相关的提示。
    /// </summary>
    Set = 9,
}

/// <summary>
/// UltraLib 悬停提示工厂，用于创建预定义的静态悬停提示。
/// <para>
/// 所有提示的本地化键统一以 <c>ULTRALIB-</c> 为前缀，
/// 与 <c>UltraLib.json</c> 中的模组 ID 保持一致。
/// </para>
/// </summary>
public static class PlusHoverTipFactory
{
    /// <summary>
    /// 创建一个指向静态本地化条目（<c>static_hover_tips</c>）的本地化字符串。
    /// </summary>
    /// <param name="entry">本地化条目键名。</param>
    /// <returns>对应的本地化字符串。</returns>
    private static LocString L10NStatic(string entry) => new LocString("static_hover_tips", entry);

    /// <summary>
    /// 创建一个预设的静态悬停提示，并为其标题和描述注入动态变量。
    /// </summary>
    /// <param name="tip">要创建的悬停提示类型（枚举）。</param>
    /// <param name="vars">要注入到标题和描述中的动态变量。</param>
    /// <returns>生成的悬停提示实例。</returns>
    public static IHoverTip Static(PlusHoverTip tip, params DynamicVar[] vars)
    {
        // 将枚举名称转换为 URL 友好的 slug 格式（如 "Return" → "return"）
        string str = StringHelper.Slugify(tip.ToString());

        // 构建本地化键：static_hover_tips/ULTRALIB-{slug}.base.title
        LocString title = L10NStatic("ULTRALIB-" + str + ".base.title");

        // 构建本地化键：static_hover_tips/ULTRALIB-{slug}.base.description
        LocString description = L10NStatic("ULTRALIB-" + str + ".base.description");

        // 为标题和描述注入动态变量
        foreach (DynamicVar var in vars)
        {
            title.Add(var);
            description.Add(var);
        }

        return new MegaCrit.Sts2.Core.HoverTips.HoverTip(title, description);
    }
}
