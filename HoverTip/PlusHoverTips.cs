using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Localization.DynamicVars;

namespace UltraLib.HoverTip;

/// <summary>
/// UltraLib's predefined hover-tip enum.
/// </summary>
/// <remarks>
/// UltraLib 预定义的悬停提示（HoverTip）枚举。
/// <para>
/// 每个枚举值对应本地化键 <c>static_hover_tips/ULTRALIB-{Slugified名称}.base.title</c>
/// 和 <c>static_hover_tips/ULTRALIB-{Slugified名称}.base.description</c>。
/// </para>
/// </remarks>
public enum PlusHoverTip
{
    /// <summary>
    /// Return — the card returns to the hand after being used.
    /// </summary>
    /// <remarks>
    /// 返回 —— 卡牌使用后返回手牌。
    /// </remarks>
    Return = 0,

    /// <summary>
    /// Empower — injects a specific ability into the card.
    /// </summary>
    /// <remarks>
    /// 注能 —— 为卡牌注入特定能力。
    /// </remarks>
    Empower = 1,

    /// <summary>
    /// Move — the card moves between the hand and the deck.
    /// </summary>
    /// <remarks>
    /// 移动 —— 卡牌在手牌/牌组间移动。
    /// </remarks>
    Move = 2,

    /// <summary>
    /// Stolen — gold or a card is stolen.
    /// </summary>
    /// <remarks>
    /// 被偷 —— 金币或卡牌被偷取。
    /// </remarks>
    Stolen = 3,

    /// <summary>
    /// Rewind — time-reversal style effects.
    /// </summary>
    /// <remarks>
    /// 回溯 —— 时间倒流类的效果。
    /// </remarks>
    Rewind = 4,

    /// <summary>
    /// Hp — hints related to hit points.
    /// </summary>
    /// <remarks>
    /// 生命 —— 与生命值相关的提示。
    /// </remarks>
    Hp = 5,

    /// <summary>
    /// Powers — hints related to powers.
    /// </summary>
    /// <remarks>
    /// 能力 —— 与能力相关的提示。
    /// </remarks>
    Powers = 6,

    /// <summary>
    /// OnlyPower — shows only power-related hints.
    /// </summary>
    /// <remarks>
    /// 仅能力 —— 仅显示能力相关的提示。
    /// </remarks>
    OnlyPower = 7,

    /// <summary>
    /// Charge — hints related to relic charging.
    /// </summary>
    /// <remarks>
    /// 充能 —— 遗物充能相关的提示。
    /// </remarks>
    Charge = 8,

    /// <summary>
    /// Set — hints related to sets.
    /// </summary>
    /// <remarks>
    /// 套装 —— 与套装相关的提示。
    /// </remarks>
    Set = 9,
}

/// <summary>
/// UltraLib hover-tip factory, used to create the predefined static hover tips.
/// </summary>
/// <remarks>
/// UltraLib 悬停提示工厂，用于创建预定义的静态悬停提示。
/// <para>
/// 所有提示的本地化键统一以 <c>ULTRALIB-</c> 为前缀，
/// 与 <c>UltraLib.json</c> 中的模组 ID 保持一致。
/// </para>
/// </remarks>
public static class PlusHoverTipFactory
{
    /// <summary>
    /// Creates a localization string pointing to a static localization entry (<c>static_hover_tips</c>).
    /// </summary>
    /// <remarks>
    /// 创建一个指向静态本地化条目（<c>static_hover_tips</c>）的本地化字符串。
    /// </remarks>
    /// <param name="entry">本地化条目键名。</param>
    /// <returns>对应的本地化字符串。</returns>
    private static LocString L10NStatic(string entry) => new LocString("static_hover_tips", entry);

    /// <summary>
    /// Creates a predefined static hover tip, injecting dynamic variables into its title and description.
    /// </summary>
    /// <remarks>
    /// 创建一个预设的静态悬停提示，并为其标题和描述注入动态变量。
    /// </remarks>
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
