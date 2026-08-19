using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;

namespace UltraLib.Base.Label.Card;

/// <summary>
/// UltraLib 自定义卡牌关键词（Keyword）定义。
/// <para>
/// 每个静态字段使用 <see cref="CustomEnumAttribute"/> 注册到游戏的本地化系统，
/// 使用 <see cref="KeywordPropertiesAttribute"/> 控制关键词在卡牌描述中的位置。
/// </para>
/// </summary>
public class PlusCardKeyWord
{
    /// <summary>
    /// 同构（Isomorphism）—— 当手牌中存在两张带有此关键词的卡牌间隔一张时，
    /// 自动打出中间的那张卡牌。
    /// 显示在卡牌名称之后。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword Isomorphism;

    /// <summary>
    /// 抽牌时施放（CastWhenDrawn）—— 当抽到带有此关键词的卡牌时，
    /// 自动将其打出。
    /// 显示在卡牌名称之前。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword CastWhenDrawn;

    /// <summary>
    /// 迟钝（Dull）—— 该牌在你的回合无法被丢弃。
    /// 显示在卡牌名称之前。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Dull;

    /// <summary>
    /// 饥饿（Hunger）—— 使用后不可再次使用 除非有非爪牙敌方单位死亡。
    /// 显示在卡牌名称之前。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Hunger;

    /// <summary>
    /// 测试中（InTest）—— 标记仍在测试阶段的卡牌。
    /// 显示在卡牌名称之后。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword InTest;

    /// <summary>
    /// 抽牌时丢弃（DiscardWhenDrawn）—— 当抽到带有此关键词的卡牌时，
    /// 自动将其丢弃。
    /// 显示在卡牌名称之后。
    /// </summary>
    [CustomEnum]
    [KeywordProperties(AutoKeywordPosition.After)]
    public static CardKeyword DiscardWhenDrawn;
}
