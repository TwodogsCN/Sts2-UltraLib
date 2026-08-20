using System.Reflection;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Harmony patch for orb passive triggers: dynamically scans the <c>Passive</c> method
/// of every <c>OrbModel</c> subclass and injects Before/After hooks.
/// </summary>
/// <remarks>
/// 充能球被动触发（Passive）的 Hook Patch。
/// 动态扫描所有 OrbModel 子类的 Passive 方法，注入 Before/After Hook。
/// </remarks>
[HarmonyPatch]
public static class OrbPassivePatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        var targetMethods = new List<MethodBase>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = assembly.FullName.ToLower();
            if (name.Contains("system") || name.Contains("mscorlib") ||
                name.Contains("steamworks") || name.Contains("godot") ||
                name.Contains("unity"))
                continue;

            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
            catch { continue; }

            foreach (var type in types.Where(t => t.IsSubclassOf(typeof(OrbModel)) && !t.IsAbstract))
            {
                var method = AccessTools.Method(type, "Passive");
                if (method != null) targetMethods.Add(method);
            }
        }
        return targetMethods.Distinct();
    }

    [HarmonyPostfix]
    public static async Task Postfix(Task __result, PlayerChoiceContext choiceContext, Creature? target, OrbModel __instance)
    {
        await PlusHooks.Plus_TriggerBeforeOrbPassive(choiceContext, target, __instance);
        await __result;
        await PlusHooks.Plus_TriggerAfterOrbPassive(choiceContext, target, __instance);
    }
}

/// <summary>
/// Harmony patch for orb evoke triggers: dynamically scans the <c>Evoke</c> method
/// of every <c>OrbModel</c> subclass and injects Before/After hooks.
/// </summary>
/// <remarks>
/// 充能球激发（Evoke）的 Hook Patch。
/// 动态扫描所有 OrbModel 子类的 Evoke 方法，注入 Before/After Hook。
/// </remarks>
[HarmonyPatch]
public static class OrbEvokePatch
{
    static IEnumerable<MethodBase> TargetMethods()
    {
        var targetMethods = new List<MethodBase>();
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            string name = assembly.FullName.ToLower();
            if (name.Contains("system") || name.Contains("mscorlib") ||
                name.Contains("steamworks") || name.Contains("godot") ||
                name.Contains("unity"))
                continue;

            Type[] types;
            try { types = assembly.GetTypes(); }
            catch (ReflectionTypeLoadException e) { types = e.Types.Where(t => t != null).ToArray(); }
            catch { continue; }

            foreach (var type in types.Where(t => t.IsSubclassOf(typeof(OrbModel)) && !t.IsAbstract))
            {
                var method = AccessTools.Method(type, "Evoke");
                if (method != null) targetMethods.Add(method);
            }
        }
        return targetMethods.Distinct();
    }

    [HarmonyPostfix]
    public static void Postfix(ref Task<IEnumerable<Creature>> __result, PlayerChoiceContext playerChoiceContext, OrbModel __instance)
    {
        __result = InterceptEvokeAsync(__result, playerChoiceContext, __instance);
    }

    private static async Task<IEnumerable<Creature>> InterceptEvokeAsync(
        Task<IEnumerable<Creature>> originalTask, PlayerChoiceContext ctx, OrbModel orb)
    {
        await PlusHooks.Plus_TriggerBeforeOrbEvoke(ctx, orb);
        var result = await originalTask;
        await PlusHooks.Plus_TriggerAfterOrbEvokeRemoved(ctx, orb);
        return result;
    }
}
