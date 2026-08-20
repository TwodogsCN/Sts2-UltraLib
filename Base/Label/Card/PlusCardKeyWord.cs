using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace UltraLib.Base.Label.Card;

/// <summary>
/// Defines UltraLib's custom card keywords (keywords).
/// </summary>
/// <remarks>
/// UltraLib 自定义卡牌关键词（Keyword）定义。
/// <para>
/// 每个静态字段使用 <see cref="CustomEnumAttribute"/> 注册到游戏的本地化系统，
/// 使用 <see cref="KeywordPropertiesAttribute"/> 控制关键词在卡牌描述中的位置。
/// </para>
/// </remarks>
public class PlusCardKeyWord
{
    /// <summary>
    /// Isomorphism — when there are two cards with this keyword in hand and one card
    /// between them, the middle card is auto-played. Shown after the card name.
    /// </summary>
    /// <remarks>
    /// 同构（Isomorphism）—— 当手牌中存在两张带有此关键词的卡牌间隔一张时，
    /// 自动打出中间的那张卡牌。显示在卡牌名称之后。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Isomorphism;

    /// <summary>
    /// CastWhenDrawn — when a card with this keyword is drawn, it is auto-played.
    /// Shown before the card name.
    /// </summary>
    /// <remarks>
    /// 抽牌时施放（CastWhenDrawn）—— 当抽到带有此关键词的卡牌时，自动将其打出。
    /// 显示在卡牌名称之前。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword CastWhenDrawn;

    /// <summary>
    /// Dull — the card cannot be discarded on your turn. Shown before the card name.
    /// </summary>
    /// <remarks>
    /// 迟钝（Dull）—— 该牌在你的回合无法被丢弃。显示在卡牌名称之前。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Dull;

    /// <summary>
    /// Hunger — after being used, it cannot be used again unless a non-minion enemy dies.
    /// Shown before the card name.
    /// </summary>
    /// <remarks>
    /// 饥饿（Hunger）—— 使用后不可再次使用，除非有非爪牙敌方单位死亡。
    /// 显示在卡牌名称之前。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Hunger;

    /// <summary>
    /// InTest — marks a card that is still in testing. Shown after the card name.
    /// </summary>
    /// <remarks>
    /// 测试中（InTest）—— 标记仍在测试阶段的卡牌。显示在卡牌名称之后。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword InTest;

    /// <summary>
    /// DiscardWhenDrawn — when a card with this keyword is drawn, it is auto-discarded.
    /// Shown after the card name.
    /// </summary>
    /// <remarks>
    /// 抽牌时丢弃（DiscardWhenDrawn）—— 当抽到带有此关键词的卡牌时，自动将其丢弃。
    /// 显示在卡牌名称之后。
    /// </remarks>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword DiscardWhenDrawn;
}
