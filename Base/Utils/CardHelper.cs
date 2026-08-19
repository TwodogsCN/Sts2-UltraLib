using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.Commands;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Helpers;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.CommonUi;
using MegaCrit.Sts2.Core.Nodes.Rooms;
using MegaCrit.Sts2.Core.Nodes.Vfx;
using MegaCrit.Sts2.Core.TestSupport;
using UltraLib.Variables;

namespace UltraLib.Base.Utils;

/// <summary>
/// 卡牌操作辅助工具，封装 CardCmd / CardPileCmd 的常用操作。
/// </summary>
public static class CardHelper
{
    /// <summary>
    /// 带有预览的生成卡牌到战斗方法
    /// </summary>
    public static async Task PreviewAddGeneratedCardToCombat(CardModel card, PileType pile, Player? player, CardPilePosition position,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        CardCmd.PreviewCardPileAdd(await AddGeneratedCardToCombat(card, pile, player, position,
            style));
    }

    /// <summary>
    /// 预览一个牌堆添加的结果
    /// </summary>
    public static void PreviewCardPileAddResult(CardPileAddResult result)
    {
        CardCmd.PreviewCardPileAdd(result);
    }

    /// <summary>
    /// 消耗一个牌或一个列表的牌
    /// </summary>
    public static async Task Exhaust(List<CardModel> cardList, PlayerChoiceContext? playerChoiceContext = null)
    {
        playerChoiceContext ??= new BlockingPlayerChoiceContext();
        foreach (var card in cardList)
        {
            await CardCmd.Exhaust(playerChoiceContext, card);
        }
    }

    /// <summary>
    /// 消耗一个牌或一个列表的牌
    /// </summary>
    public static async Task Exhaust(CardModel card, PlayerChoiceContext? playerChoiceContext = null)
    {
        playerChoiceContext ??= new BlockingPlayerChoiceContext();
        await CardCmd.Exhaust(playerChoiceContext, card);
    }

    /// <summary>
    /// 升级一个牌或一个列表的牌
    /// </summary>
    public static void Upgrade(CardModel card)
    {
        CardCmd.Upgrade(card);
    }

    /// <summary>
    /// 降级一个牌或一个列表的牌
    /// </summary>
    public static void Downgrade(List<CardModel> cardList)
    {
        foreach (var card in cardList)
        {
            CardCmd.Downgrade(card);
        }
    }

    /// <summary>
    /// 降级一个牌或一个列表的牌
    /// </summary>
    public static void Downgrade(CardModel card)
    {
        CardCmd.Downgrade(card);
    }

    /// <summary>
    /// 升级一个牌或一个列表的牌
    /// </summary>
    public static void Upgrade(List<CardModel> cardList)
    {
        foreach (var card in cardList)
        {
            CardCmd.Upgrade(card);
        }
    }

    /// <summary>
    /// 生成卡牌进入战斗 触发生成卡牌Hook
    /// 返回CardPileAddResult
    /// </summary>
    public static async Task<CardPileAddResult> AddGeneratedCardToCombat(CardModel card, PileType pile, Player? player, CardPilePosition position,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        return await CardPileCmd.AddGeneratedCardToCombat(card, pile, player, position);
    }

    /// <summary>
    /// 将指定卡添加至卡堆 带预览设置
    /// 不能触发生成卡牌Hook
    /// </summary>
    public static async Task AddToPile(CardModel card, PileType pile, CardPilePosition position, bool isPreview = true,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        if (isPreview)
            CardCmd.PreviewCardPileAdd(await CardPileHelper.AddToPile(card, pile, position));
        else
            await CardPileHelper.AddToPile(card, pile, position);
    }

    /// <summary>
    /// 将指定卡牌列表添加至卡堆 带预览设置
    /// 不能触发生成卡牌Hook
    /// </summary>
    public static async Task AddToPile(List<CardModel> cardList, PileType pile, CardPilePosition position,
        bool isPreview = true,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        foreach (var card in cardList)
        {
            if (isPreview)
                CardCmd.PreviewCardPileAdd(await CardPileHelper.AddToPile(card, pile, position));
            else
                await CardPileHelper.AddToPile(card, pile, position);
        }
    }

    /// <summary>
    /// 传入卡牌 返回一个原版模型 如需使用需要ToMutable()
    /// </summary>
    public static CardModel GetModelDb(CardModel cardModel)
    {
        var type = cardModel.GetType();
        var method = typeof(ModelDb).GetMethod("Card", Type.EmptyTypes);
        var genericMethod = method.MakeGenericMethod(type);
        return (CardModel)genericMethod.Invoke(null, null);
    }

    /// <summary>
    /// 克隆一个卡牌 返回复制的卡
    /// </summary>
    public static CardModel Clone(CardModel cardModel)
    {
        CardModel card = cardModel.CreateClone();
        return card;
    }

    /// <summary>
    /// 克隆一个卡牌的原始版本 返回原始版本的复制卡
    /// </summary>
    public static CardModel CloneOrigin(CardModel cardModel, Player player, ICombatState combatState)
    {
        CardModel card = combatState.CreateCard(GetModelDb(cardModel), player);
        return card;
    }

    /// <summary>
    /// 给一个卡牌添加关键词
    /// </summary>
    public static void ApplyKeyword(CardModel card, params CardKeyword[] keywordsList)
    {
        CardCmd.ApplyKeyword(card, keywordsList);
    }

    /// <summary>
    /// 给一列表卡牌添加关键词
    /// </summary>
    public static void ApplyKeyword(List<CardModel> cardList, params CardKeyword[] keywordsList)
    {
        foreach (var card in cardList)
        {
            CardCmd.ApplyKeyword(card, keywordsList);
        }
    }

    /// <summary>
    /// 给一张卡牌移除关键词
    /// </summary>
    public static void RemoveKeyword(CardModel card, params CardKeyword[] keywordsList)
    {
        CardCmd.RemoveKeyword(card, keywordsList);
    }

    /// <summary>
    /// 给一列表卡牌移除关键词
    /// </summary>
    public static void RemoveKeyword(List<CardModel> cardList, params CardKeyword[] keywordsList)
    {
        foreach (var card in cardList)
        {
            CardCmd.RemoveKeyword(card, keywordsList);
        }
    }

    /// <summary>
    /// 给指定卡添加返回效果
    /// </summary>
    public static void AddReturnVar(this CardModel card, decimal value)
    {
        var varsDict = GetVarsDict(card);
        if (varsDict == null) return;

        decimal finalValue = value;

        if (varsDict.Contains(ReturnVar.Key))
        {
            var existing = varsDict[ReturnVar.Key];
            finalValue += GetNumericValueFromVar(existing);
        }

        varsDict[ReturnVar.Key] = new ReturnVar(finalValue);

        card.RefreshHoverTips();
    }

    /// <summary>
    /// 给指定卡添加赋能效果
    /// </summary>
    public static void AddEmpowerVar(this CardModel card, PowerModel power, decimal value)
    {
        var varsDict = GetVarsDict(card);
        if (varsDict == null) return;

        varsDict[EmpowerVar.Key] = new EmpowerVar(power, value);

        card.RefreshHoverTips();
    }

    private static System.Collections.IDictionary GetVarsDict(CardModel card)
    {
        var varsField = card.DynamicVars.GetType().GetField("_vars", BindingFlags.NonPublic | BindingFlags.Instance);
        return varsField?.GetValue(card.DynamicVars) as System.Collections.IDictionary;
    }

    /// <summary>
    /// 移除卡牌的返回
    /// </summary>
    public static bool RemoveReturnVar(this CardModel card)
    {
        var dict = GetVarsDict(card);
        if (dict == null) return false;
        dict.Remove(ReturnVar.Key);
        return true;
    }

    /// <summary>
    /// 移除卡牌的赋能
    /// </summary>
    public static bool RemoveEmpowerVar(this CardModel card)
    {
        var dict = GetVarsDict(card);
        if (dict == null) return false;
        dict.Remove(EmpowerVar.Key);
        return true;
    }

    private static decimal GetNumericValueFromVar(object varObj)
    {
        if (varObj == null) return 0m;

        // 针对你模组变量的快速判断
        if (varObj is ReturnVar rv) return rv.IntValue;

        // 针对原版 DynamicVar 的处理
        if (varObj is MegaCrit.Sts2.Core.Localization.DynamicVars.DynamicVar dVar)
            return Convert.ToDecimal(dVar.IntValue);

        return decimal.TryParse(varObj.ToString(), out var d) ? d : 0m;
    }

    /// <summary>
    /// 刷新一个卡牌的悬浮提示
    /// </summary>
    public static void RefreshHoverTips(this CardModel card)
    {
        // 查找当前卡牌对应的场景节点（Node）
        var node = NCard.FindOnTable(card);
        if (node != null)
        {
            // 获取当前牌堆类型，默认为 None
            var pileType = card.Pile?.Type ?? PileType.None;

            // 调用游戏原生的视觉更新方法，这会触发 HoverTips 的重新读取
            node.UpdateVisuals(pileType, CardPreviewMode.Normal);
        }
    }

    /// <summary>
    /// 自动打出一张卡牌，会传入choiceContext
    /// </summary>
    public static async Task AutoPlay(PlayerChoiceContext choiceContext, CardModel card, ICombatState combatState, bool skipX = true)
    {
        await CardCmd.AutoPlay(choiceContext, card, GetAutoTarget(card, combatState), skipXCapture: skipX);
    }

    /// <summary>
    /// 自动打出一张牌，不用传入choiceContext
    /// 会用new BlockingPlayerChoiceContext()代替
    /// </summary>
    public static async Task AutoPlay(CardModel card, ICombatState combatState, bool skipX = true)
    {
        await CardCmd.AutoPlay(new BlockingPlayerChoiceContext(), card, GetAutoTarget(card, combatState), skipXCapture: skipX);
    }

    /// <summary>
    /// 获取一个卡牌可以指定的随机目标
    /// </summary>
    public static Creature? GetAutoTarget(CardModel card, ICombatState combatState)
    {
        if (card == null || combatState == null) return null;

        var owner = card.Owner;

        var rng = owner.RunState.Rng.CombatTargets;

        return card.TargetType switch
        {
            TargetType.AnyEnemy => combatState.HittableEnemies.FirstOrDefault(),

            TargetType.AnyAlly => rng.NextItem(combatState.Allies.Where(c =>
                c.IsAlive &&
                c != owner.Creature
            )),

            TargetType.AnyPlayer => owner.Creature,

            TargetType.AllEnemies => null,
            TargetType.AllAllies => null,

            _ => null
        };
    }

    /// <summary>
    /// 设置卡牌的类型
    /// </summary>
    public static void SetCardType(this CardModel card, CardType newType)
    {
        if (card == null) return;
        Traverse.Create(card).Field("<Type>k__BackingField").SetValue(newType);
    }

    /// <summary>
    /// 弃掉卡牌，需传入choiceContext
    /// </summary>
    public static async Task Discard(PlayerChoiceContext choiceContext, IEnumerable<CardModel> card)
    {
        await CardCmd.Discard(choiceContext, card);
    }

    /// <summary>
    /// 弃掉卡牌，需传入choiceContext
    /// </summary>
    public static async Task Discard(PlayerChoiceContext choiceContext, CardModel card)
    {
        await CardCmd.Discard(choiceContext, card);
    }

    /// <summary>
    /// 预览一张卡牌（出现在中央再飞回卡堆）
    /// </summary>
    public static TaskCompletionSource? Preview(CardModel card, float time = 1.2f, CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        return CardCmd.Preview(card, time, style);
    }

    /// <summary>
    /// 给一张卡牌添加指定量的附魔
    /// </summary>
    public static T? Enchant<T>(CardModel card, Decimal amount) where T : EnchantmentModel
    {
        return CardCmd.Enchant(ModelDb.Enchantment<T>().ToMutable(), card, amount) as T;
    }

    /// <summary>
    /// 给一张卡牌添加指定量的附魔
    /// </summary>
    public static EnchantmentModel? Enchant(EnchantmentModel enchantment, CardModel card, Decimal amount)
    {
        return CardCmd.Enchant(enchantment.ToMutable(), card, amount);
    }

    /// <summary>
    /// 创建一个卡牌
    /// </summary>
    public static T CreateCard<T>(ICombatState combatState, Player player) where T : CardModel
    {
        return (T)combatState.CreateCard(ModelDb.Card<T>(), player);
    }

    /// <summary>
    /// 以card为基础创建一个card
    /// </summary>
    public static CardModel CreateCard(CardModel canonicalCard, ICombatState combatState, Player player)
    {
        return combatState.CreateCard(canonicalCard, player);
    }

    /// <summary>
    /// 把目标卡牌转变成指定卡牌的最初版本
    /// </summary>
    public static async Task<CardPileAddResult?> TransformTo<T>(CardModel card) where T : CardModel
    {
        return await CardCmd.TransformTo<T>(card);
    }

    /// <summary>
    /// 把目标卡牌变化成目标卡牌
    /// </summary>
    public static async Task<CardPileAddResult?> Transform(
        CardModel original,
        CardModel replacement,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        return new CardPileAddResult?((await CardCmd.Transform(new CardTransformation(original, replacement).Yield(), null, style)).FirstOrDefault());
    }

    /// <summary>
    /// 带预览的变化
    /// 把目标卡牌变化成目标卡牌
    /// </summary>
    public static async Task PreviewTransform(
        CardModel original,
        CardModel replacement,
        CardPreviewStyle style = CardPreviewStyle.HorizontalLayout)
    {
        PreviewCardPileAddResult((await CardCmd.Transform(new CardTransformation(original, replacement).Yield(), null, style)).FirstOrDefault());
    }

    /// <summary>
    /// 拥有锻造一般的预览效果
    /// </summary>
    public static void PreviewSovereignBlade(IReadOnlyCollection<CardModel> cards)
    {
        if (TestMode.IsOn || !LocalContext.IsMine(cards.First()))
            return;
        List<CardModel> list1 = cards.Where(c => c.Pile.Type == PileType.Hand).ToList();
        List<CardModel> list2 = cards.Where(c => c.Pile.Type != PileType.Hand).ToList();
        foreach (CardModel card in list1)
            NRun.Instance.GlobalUi.AboveTopBarVfxContainer.AddChildSafely(NCardSmithVfx.Create(NCombatRoom.Instance.Ui.Hand.GetCard(card), false));
        if (list2.Count == 0)
            return;
        NRun.Instance.GlobalUi.CardPreviewContainer.AddChildSafely(NCardSmithVfx.Create(list2, false));
    }

    /// <summary>
    /// 拥有锻造一般的预览效果
    /// </summary>
    public static void PreviewSovereignBlade(CardModel card)
    {
        List<CardModel> cards =
        [
            card
        ];
        PreviewSovereignBlade(cards);
    }
}
