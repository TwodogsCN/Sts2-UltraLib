using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Gold;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Cards.Holders;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Rooms;
using MegaCrit.Sts2.Core.Runs;
using UltraLib.Variables;

namespace UltraLib.Hook;

/// <summary>
/// Contract for implementing UltraLib's hook system.
/// </summary>
/// <remarks>
/// UltraLib 钩子系统的实现契约。
/// <para>
/// 任何实现 <see cref="IPlusHooks"/> 的模型（如片选基类 <c>PlusRelicModel</c>、
/// 能力基类、单例）在被收集为监听者后，会自动收到这些钩子调用。全部成员都有
/// 默认实现，订阅者只需覆写自己关心的部分。
/// </para>
/// </remarks>
public interface IPlusHooks
{
    // ==========================================
    // Room Hooks / 房间钩子
    // ==========================================

    /// <summary>
    /// Fired after a random room type has been rolled for the map point.
    /// </summary>
    /// <remarks>
    /// 随机房间 Roll 点完成后触发。此时本局已确定该地图点将生成哪种
    /// <see cref="RoomType"/>，可在此根据结果调整房间相关逻辑。
    /// </remarks>
    Task Plus_AfterRandomRoomRolled(RoomType roomType) => Task.CompletedTask;

    // ==========================================
    // Hand Hooks / 手牌钩子
    // ==========================================

    /// <summary>
    /// Fired after a card has been moved into/from the hand pile.
    /// </summary>
    /// <remarks>
    /// 手牌堆发生移动后触发。可用于响应卡牌进入/离开手牌（例如同构检测依赖此钩子）。
    /// </remarks>
    Task Plus_AfterHandPileMoved(CardModel card) => Task.CompletedTask;

    /// <summary>
    /// Fired before a card is moved into/from the hand pile.
    /// </summary>
    /// <remarks>
    /// 手牌堆发生移动前触发。可用于在移动生效前阻止或修改该操作。
    /// </remarks>
    Task Plus_BeforeHandPileMoved(CardModel card) => Task.CompletedTask;

    // ==========================================
    // Card Hooks / 卡牌钩子
    // ==========================================

    /// <summary>
    /// Modifies a card's rose value (pipeline, set/replace).
    /// </summary>
    /// <remarks>
    /// 修改卡牌的玫瑰数值（管线型，可替换当前值）。
    /// </remarks>
    decimal Plus_ModifyRoseCard(decimal amount, Player player, CardModel card) => amount;

    /// <summary>
    /// Multiplies a card's rose value (multiplicative modifier).
    /// </summary>
    /// <remarks>
    /// 卡牌玫瑰数值的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyRoseCardMultiplicative(decimal amount, Player player, CardModel card) => 1m;

    /// <summary>
    /// Adds a delta to a card's rose value (additive modifier).
    /// </summary>
    /// <remarks>
    /// 卡牌玫瑰数值的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyRoseCardAddictive(decimal amount, Player player, CardModel card) => 0m;

    // ==========================================
    // Power Hooks / 能力钩子
    // ==========================================

    /// <summary>
    /// Fired when a power is right-clicked.
    /// </summary>
    /// <remarks>
    /// 能力被右键点击时触发。
    /// </remarks>
    Task Plus_PowerRightClick(PowerModel powerModel, NPower holder) => Task.CompletedTask;

    // ==========================================
    // Relic Hooks / 遗物钩子
    // ==========================================

    /// <summary>
    /// Fired when a relic is right-clicked.
    /// </summary>
    /// <remarks>
    /// 遗物被右键点击时触发。
    /// </remarks>
    Task Plus_RelicRightClick(RelicModel relicModel, NRelicInventoryHolder? holder) => Task.CompletedTask;

    /// <summary>
    /// Fired after a relic has been obtained.
    /// </summary>
    /// <remarks>
    /// 遗物获得后触发。
    /// </remarks>
    Task Plus_AfterRelicObtain(IRunState runState, RelicModel relicModel, Player holder) => Task.CompletedTask;

    /// <summary>
    /// Fired before a relic is obtained.
    /// </summary>
    /// <remarks>
    /// 遗物获得前触发。
    /// </remarks>
    Task Plus_BeforeRelicObtain(IRunState runState, RelicModel relicModel, Player holder) => Task.CompletedTask;

    /// <summary>
    /// Fired when a card is right-clicked.
    /// </summary>
    /// <remarks>
    /// 卡牌被右键点击时触发。
    /// </remarks>
    Task Plus_CardRightClick(CardModel relicModel, NCardHolder holder) => Task.CompletedTask;

    // ==========================================
    // Charge Hooks / 充能钩子
    // ==========================================

    /// <summary>
    /// Modifies the max charge of a relic (pipeline, set/replace).
    /// </summary>
    /// <remarks>
    /// 修改遗物最大充能值（管线型）。
    /// </remarks>
    decimal Plus_ModifyMaxCharge(decimal amount, Player player, RelicModel relic) => amount;

    /// <summary>
    /// Multiplies the max charge of a relic (multiplicative).
    /// </summary>
    /// <remarks>
    /// 遗物最大充能的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyMaxChargeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;

    /// <summary>
    /// Adds a delta to the max charge of a relic (additive).
    /// </summary>
    /// <remarks>
    /// 遗物最大充能的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyMaxChargeAddictive(decimal amount, Player player, RelicModel relic) => 0m;

    /// <summary>
    /// Modifies the charge upgrade amount of a relic (pipeline).
    /// </summary>
    /// <remarks>
    /// 修改遗物充能升级量（管线型）。
    /// </remarks>
    decimal Plus_ModifyChargeUpgrade(decimal amount, Player player, RelicModel relic) => amount;

    /// <summary>
    /// Multiplies the charge upgrade amount of a relic (multiplicative).
    /// </summary>
    /// <remarks>
    /// 遗物充能升级量的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyChargeUpgradeMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;

    /// <summary>
    /// Adds a delta to the charge upgrade amount of a relic (additive).
    /// </summary>
    /// <remarks>
    /// 遗物充能升级量的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyChargeUpgradeAddictive(decimal amount, Player player, RelicModel relic) => 0m;

    /// <summary>
    /// Modifies the charge spend amount of a relic (pipeline).
    /// </summary>
    /// <remarks>
    /// 修改遗物消耗充能量（管线型）。
    /// </remarks>
    decimal Plus_ModifyChargeSpend(decimal amount, Player player, RelicModel relic) => amount;

    /// <summary>
    /// Multiplies the charge spend amount of a relic (multiplicative).
    /// </summary>
    /// <remarks>
    /// 遗物消耗充能量的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyChargeSpendMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;

    /// <summary>
    /// Adds a delta to the charge spend amount of a relic (additive).
    /// </summary>
    /// <remarks>
    /// 遗物消耗充能量的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyChargeSpendAddictive(decimal amount, Player player, RelicModel relic) => 0m;

    /// <summary>
    /// Fired after charge has been spent on a relic.
    /// </summary>
    /// <remarks>
    /// 遗物消耗充能后触发。
    /// </remarks>
    Task Plus_AfterChargeSpend(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired after a relic has gained charge.
    /// </summary>
    /// <remarks>
    /// 遗物获得充能后触发。
    /// </remarks>
    Task Plus_AfterChargeGain(decimal amount, Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired when a relic becomes fully charged.
    /// </summary>
    /// <remarks>
    /// 遗物充能充满时触发。
    /// </remarks>
    Task Plus_OnChargeFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired when a relic is no longer fully charged.
    /// </summary>
    /// <remarks>
    /// 遗物不再处于充满状态时触发。
    /// </remarks>
    Task Plus_OnChargeNoLongerFullyCharged(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired when a relic's charge amount changes.
    /// </summary>
    /// <remarks>
    /// 遗物充能数值发生变化时触发。
    /// </remarks>
    Task Plus_OnChargeChanged(int oldCharge, int newCharge, Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired after a relic's charge effect has been applied once.
    /// </summary>
    /// <remarks>
    /// 遗物充能效果生效（单次）后触发。
    /// </remarks>
    Task Plus_AfterChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired after a relic's charge effect has been applied to its full extent.
    /// </summary>
    /// <remarks>
    /// 遗物充能效果完全生效后触发。
    /// </remarks>
    Task Plus_AfterChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired before a relic's charge effect is applied once.
    /// </summary>
    /// <remarks>
    /// 遗物充能效果生效（单次）前触发。
    /// </remarks>
    Task Plus_BeforeChargeEffected(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Fired before a relic's charge effect is applied to its full extent.
    /// </summary>
    /// <remarks>
    /// 遗物充能效果完全生效前触发。
    /// </remarks>
    Task Plus_BeforeChargeTotallyEffected(Player player, RelicModel relic) => Task.CompletedTask;

    /// <summary>
    /// Modifies how many times a relic's charge effect repeats (pipeline).
    /// </summary>
    /// <remarks>
    /// 修改遗物充能效果的重复次数（管线型）。
    /// </remarks>
    decimal Plus_ModifyChargeRepeatTimes(decimal amount, Player player, RelicModel relic) => amount;

    /// <summary>
    /// Multiplies a relic's charge effect repeat count (multiplicative).
    /// </summary>
    /// <remarks>
    /// 遗物充能效果重复次数的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyChargeRepeatTimesMultiplicative(decimal amount, Player player, RelicModel relic) => 1m;

    /// <summary>
    /// Adds a delta to a relic's charge effect repeat count (additive).
    /// </summary>
    /// <remarks>
    /// 遗物充能效果重复次数的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyChargeRepeatTimesAddictive(decimal amount, Player player, RelicModel relic) => 0m;

    // ==========================================
    // Gold Hooks / 金币钩子
    // ==========================================

    /// <summary>
    /// Modifies the gold loss amount (pipeline).
    /// </summary>
    /// <remarks>
    /// 修改金币损失量（管线型），可替换最终损失值。
    /// </remarks>
    decimal Plus_ModifyGoldLoss(decimal amount, Player player, GoldLossType goldLossType) => amount;

    /// <summary>
    /// Multiplies the gold loss amount (multiplicative).
    /// </summary>
    /// <remarks>
    /// 金币损失量的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyGoldLossMultiplicative(decimal amount, Player player, GoldLossType goldLossType) => 1m;

    /// <summary>
    /// Adds a delta to the gold loss amount (additive).
    /// </summary>
    /// <remarks>
    /// 金币损失量的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyGoldLossAddictive(decimal amount, Player player, GoldLossType goldLossType) => 0m;

    /// <summary>
    /// Modifies the gold gain amount (pipeline).
    /// </summary>
    /// <remarks>
    /// 修改金币获得量（管线型），可替换最终获得值。
    /// </remarks>
    decimal Plus_ModifyGoldGain(decimal amount, Player player, bool wasStolenBack) => amount;

    /// <summary>
    /// Multiplies the gold gain amount (multiplicative).
    /// </summary>
    /// <remarks>
    /// 金币获得量的乘算型修改器（恒等元 1m）。
    /// </remarks>
    decimal Plus_ModifyGoldGainMultiplicative(decimal amount, Player player, bool wasStolenBack) => 1m;

    /// <summary>
    /// Adds a delta to the gold gain amount (additive).
    /// </summary>
    /// <remarks>
    /// 金币获得量的加算型修改器（恒等元 0m）。
    /// </remarks>
    decimal Plus_ModifyGoldGainAddictive(decimal amount, Player player, bool wasStolenBack) => 0m;

    // ==========================================
    // Orb Hooks / 充能球钩子
    // ==========================================

    /// <summary>
    /// Fired after an orb has been evoked and removed.
    /// </summary>
    /// <remarks>
    /// 充能球被激发并移除后触发。
    /// </remarks>
    Task Plus_AfterOrbEvokeRemoved(PlayerChoiceContext choiceContext, OrbModel orb) => Task.CompletedTask;

    /// <summary>
    /// Fired before an orb is evoked.
    /// </summary>
    /// <remarks>
    /// 充能球被激发前触发。
    /// </remarks>
    Task Plus_BeforeOrbEvoke(PlayerChoiceContext choiceContext, OrbModel orb) => Task.CompletedTask;

    /// <summary>
    /// Fired before an orb's passive effect is applied.
    /// </summary>
    /// <remarks>
    /// 充能球被动效果生效前触发。
    /// </remarks>
    Task Plus_BeforeOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb) => Task.CompletedTask;

    /// <summary>
    /// Fired after an orb's passive effect is applied.
    /// </summary>
    /// <remarks>
    /// 充能球被动效果生效后触发。
    /// </remarks>
    Task Plus_AfterOrbPassive(PlayerChoiceContext choiceContext, Creature? creature, OrbModel orb) => Task.CompletedTask;

    // ==========================================
    // PlusCardKeyWords Hooks / 卡牌关键词钩子
    // ==========================================

    /// <summary>
    /// Fired before the isomorphism logic auto-plays the middle card.
    /// </summary>
    /// <remarks>
    /// 同构逻辑自动打出中间卡牌前触发。
    /// </remarks>
    Task Plus_BeforeIsomorphism(CardModel card) => Task.CompletedTask;

    /// <summary>
    /// Fired after the isomorphism logic auto-plays the middle card.
    /// </summary>
    /// <remarks>
    /// 同构逻辑自动打出中间卡牌后触发。
    /// </remarks>
    Task Plus_AfterIsomorphism(CardModel card) => Task.CompletedTask;

    /// <summary>
    /// Fired before a CastWhenDrawn card is auto-played.
    /// </summary>
    /// <remarks>
    /// 抽牌时施放（CastWhenDrawn）卡牌被自动打出前触发。
    /// </remarks>
    Task Plus_BeforeCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card) => Task.CompletedTask;

    /// <summary>
    /// Fired after a CastWhenDrawn card is auto-played.
    /// </summary>
    /// <remarks>
    /// 抽牌时施放（CastWhenDrawn）卡牌被自动打出后触发。
    /// </remarks>
    Task Plus_AfterCastWhenDrawn(PlayerChoiceContext choiceContext, CardModel card) => Task.CompletedTask;

    // ==========================================
    // PlusDynamicVars Hooks / 动态变量钩子
    // ==========================================

    /// <summary>
    /// Fired before a card is returned.
    /// </summary>
    /// <remarks>
    /// 卡牌返回（回收）前触发。
    /// </remarks>
    Task Plus_BeforeCardReturn(CardModel card) => Task.CompletedTask;

    /// <summary>
    /// Fired after a card is returned.
    /// </summary>
    /// <remarks>
    /// 卡牌返回（回收）后触发。
    /// </remarks>
    Task Plus_AfterCardReturn(CardModel card) => Task.CompletedTask;

    // ==========================================
    // Empower Hooks / 赋能钩子
    // ==========================================

    /// <summary>
    /// Fired before a card is empowered for the given targets.
    /// </summary>
    /// <remarks>
    /// 卡牌对指定目标施加赋能前触发。
    /// </remarks>
    Task Plus_BeforeCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets) => Task.CompletedTask;

    /// <summary>
    /// Fired after a card is empowered for the given targets.
    /// </summary>
    /// <remarks>
    /// 卡牌对指定目标施加赋能后触发。
    /// </remarks>
    Task Plus_AfterCardEmpower(CardModel card, EmpowerVar empowerVar, List<Creature> targets) => Task.CompletedTask;
}
