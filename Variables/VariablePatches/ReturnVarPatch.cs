using System.Reflection;
using BaseLib.Utils.Patching;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Localization;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Hook;
using UltraLib.Variables;

namespace UltraLib.Variables.VariablePatches;

/// <summary>
/// Return dynamic value — core effect patch: sends the card from discard pile back to the hand.
/// </summary>
/// <remarks>
/// 返回动态值——核心效果 Patch：卡牌打出后弃牌堆 → 手牌。
/// <para>
/// 通过 Transpiler 在 GetResultPileTypeAndPositionForCardPlay 末尾插入检查，
/// 若原定去 Discard 且卡牌有 Return 标记，则改为 Hand。
/// 使用 Prepare() 在方法不存在时静默跳过。
/// </para>
/// </remarks>
[HarmonyPatch(typeof(CardModel))]
public static class ReturnVarEffectPatch
{
    // 依次尝试各版本的方法名，兼容不同游戏版本
    static MethodInfo? TargetMethod =
        AccessTools.DeclaredMethod(typeof(CardModel), "GetResultPileTypeForCardPlay")
        ?? AccessTools.DeclaredMethod(typeof(CardModel), "GetResultPileType")
        ?? AccessTools.DeclaredMethod(typeof(CardModel), "GetResultPileTypeAndPositionForCardPlay")
        ?? AccessTools.DeclaredMethod(typeof(CardModel), "GetResultLocationForCardPlay");

    static IEnumerable<MethodBase> TargetMethods()
    {
        if (TargetMethod != null) yield return TargetMethod;
    }

    /// <summary>
    /// If no target method exists, returns false to skip this patch.
    /// </summary>
    /// <remarks>
    /// 如果所有目标方法都不存在，返回 false 跳过此 Patch。
    /// </remarks>
    static bool Prepare()
    {
        if (TargetMethod != null) return true;
        return false;
    }

    [HarmonyTranspiler]
    static List<CodeInstruction> AltDestination(IEnumerable<CodeInstruction> instructions)
    {
        return new InstructionPatcher(instructions)
            .MatchFromEnd(new InstructionMatcher()
                .ldc_i4_3()   // PileType.Discard = 3
            )
            .Insert([
                CodeInstruction.LoadArgument(0),
                CodeInstruction.Call(typeof(ReturnVarEffectPatch), nameof(NormalOrReturn)),
            ]);
    }

    /// <summary>
    /// If the card has a Return marker and is going to the discard pile, sends it to hand instead.
    /// This method is inserted by the transpiler and has lower priority than the Exhaust check.
    /// </summary>
    /// <remarks>
    /// 如果卡牌有 Return 标记且要去弃牌堆，改为去手牌。
    /// 此方法在 Transpiler 插入后，优先级低于 Exhaust 检查。
    /// </remarks>
    static PileType NormalOrReturn(PileType dest, CardModel model)
    {
        if (dest == PileType.Discard && IsReturn(model))
        {
            PlusHooks.Plus_TriggerBeforeCardReturn(model).GetAwaiter().GetResult();
            var final = PileType.Hand;
            PlusHooks.Plus_TriggerAfterCardReturn(model).GetAwaiter().GetResult();
            return final;
        }
        return dest;
    }

    /// <summary>
    /// Returns whether the card has an active return effect.
    /// </summary>
    /// <remarks>
    /// 判断卡牌是否有生效的返回效果。
    /// </remarks>
    public static bool IsReturn(CardModel card)
    {
        var persist = card.DynamicVars.TryGetValue(ReturnVar.Key, out var v) ? v.IntValue : 0;
        return ReturnVar.ReturnCount(card, persist) > 0;
    }
}

/// <summary>
/// Return dynamic value — UI description patch: appends the return-count text to the card description.
/// </summary>
/// <remarks>
/// 返回动态值——UI 描述 Patch：在卡牌描述中追加返回次数文本。
/// </remarks>
[HarmonyPatch]
public static class ReturnVarDescriptionPatch
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
        if (!__instance.DynamicVars.TryGetValue(ReturnVar.Key, out var returnVarObj))
            return;

        int persist = returnVarObj.IntValue;
        int count = ReturnVar.ReturnCount(__instance, persist);
        if (count <= 0) return;

        var loc = new LocString("static_hover_tips", "ULTRALIB-RETURN.card.title");
        loc.Add("ReturnDesc", count.ToString());

        var lines = (__result ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries).ToList();
        lines.Add(loc.GetFormattedText());
        __result = string.Join("\n", lines);
    }
}
