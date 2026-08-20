using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Hook;
using UltraLib.Base.Utils;
using UltraLib.Variables;

namespace UltraLib.Variables.VariablePatches;

/// <summary>
/// Empower dynamic value — core effect patch: applies the power to the target when the card is played.
/// </summary>
/// <remarks>
/// 赋能动态值——核心效果 Patch：打出卡牌时为指定目标施加能力。
/// </remarks>
[HarmonyPatch(typeof(CardModel), nameof(CardModel.OnPlayWrapper))]
public static class EmpowerVarEffectPatch
{
    [HarmonyPostfix]
    public static void Postfix(CardModel __instance, PlayerChoiceContext choiceContext)
    {
        if (__instance.Owner?.Creature == null) return;

        foreach (var emp in __instance.DynamicVars.Values.OfType<EmpowerVar>())
            ApplyEmpower(__instance, emp);
    }

    private static void ApplyEmpower(CardModel card, EmpowerVar ev)
    {
        var applier = card.Owner.Creature;
        var targets = new List<Creature>();

        switch (card.TargetType)
        {
            case TargetType.Self:
                targets.Add(applier);
                break;
            case TargetType.AnyPlayer:
            case TargetType.AnyAlly:
            case TargetType.AnyEnemy:
                if (card.CurrentTarget != null) targets.Add(card.CurrentTarget);
                break;
            case TargetType.AllEnemies:
                targets.AddRange(card.CombatState.Enemies.Where(e => e.IsHittable));
                break;
            case TargetType.AllAllies:
                targets.AddRange(card.CombatState.Allies.Where(a => a.IsAlive));
                break;
        }

        PlusHooks.Plus_TriggerBeforeCardEmpower(card, ev, targets).GetAwaiter().GetResult();

        foreach (var t in targets)
            _ = PowerHelper.Apply(new ThrowingPlayerChoiceContext(),ev.Power, t, ev.PowerCount, applier, card);

        PlusHooks.Plus_TriggerAfterCardEmpower(card, ev, targets).GetAwaiter().GetResult();
    }
}

/// <summary>
/// Empower dynamic value — HoverTip patch: inserts the power's tip into the card's hover tips.
/// </summary>
/// <remarks>
/// 赋能动态值——HoverTip Patch：在卡牌悬浮提示中插入能力的提示。
/// </remarks>
[HarmonyPatch(typeof(CardModel), "get_HoverTips")]
public static class EmpowerVarHoverTipPatch
{
    [HarmonyPostfix]
    public static IEnumerable<IHoverTip> Postfix(IEnumerable<IHoverTip> __result, CardModel __instance)
    {
        var list = __result?.ToList() ?? new List<IHoverTip>();
        foreach (var emp in __instance.DynamicVars.Values.OfType<EmpowerVar>())
        {
            var tip = PowerHelper.GetPowerTip(emp.Power);
            if (tip != null) list.Insert(0, tip);
        }
        return list.Distinct();
    }
}

/// <summary>
/// Empower dynamic value — description patch: appends the empower text to the card description.
/// </summary>
/// <remarks>
/// 赋能动态值——描述文本 Patch：在卡牌描述中追加赋能文本。
/// </remarks>
[HarmonyPatch]
public static class EmpowerVarDescriptionPatch
{
    static MethodBase? TargetMethod()
    {
        return typeof(CardModel).GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == "GetDescriptionForPile" &&
                                 m.GetParameters().Length == 3 &&
                                 m.GetParameters()[0].ParameterType == typeof(PileType));
    }

    [HarmonyPostfix]
    public static void Postfix(ref string __result, CardModel __instance)
    {
        if (!__instance.DynamicVars.Values.OfType<EmpowerVar>().Any()) return;

        var lines = (__result ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        foreach (var emp in __instance.DynamicVars.Values.OfType<EmpowerVar>())
        {
            var loc = new LocString("static_hover_tips", "ULTRALIB-EMPOWER.card.title");
            loc.Add("EmpowerDesc", $"{emp.PowerCount} [gold]{emp.Power.Title.GetFormattedText()}[/gold]");
            lines.Add(loc.GetFormattedText());
        }
        __result = string.Join("\n", lines);
    }
}
