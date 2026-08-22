using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Creatures;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Shared late-patching support for orb hooks.
/// </summary>
/// <remarks>
/// 充能球钩子补丁的共享支撑（延迟补丁）。
/// <para>
/// UltraLib 作为依赖模组总是先于内容模组加载，而 Harmony 的 PatchAll 只会扫描
/// 当时已加载的程序集。若不做延迟补丁，内容模组中后加载的自定义充能球
/// （例如 UltimatePlus 的 MagnetOrb）的 Evoke / Passive 方法将永远补不上钩子，
/// 导致激发/被动时钩子完全不触发。这里订阅 AssemblyLoad 事件，为后加载程序集中的
/// 新充能球类型补上钩子补丁。
/// </para>
/// </remarks>
internal static class LateOrbPatchHelper
{
    private static Harmony? _harmony;

    /// <summary>
    /// Subscribes to <see cref="AppDomain.AssemblyLoad"/> so orb hooks apply to later-loaded mod assemblies.
    /// </summary>
    /// <remarks>
    /// 订阅程序集加载事件，为后加载模组中的自定义充能球补上钩子补丁。
    /// </remarks>
    /// <param name="harmony">The mod's Harmony instance. / 模组的 Harmony 实例。</param>
    public static void Init(Harmony harmony)
    {
        _harmony = harmony;
        AppDomain.CurrentDomain.AssemblyLoad += (_, e) =>
        {
            try
            {
                OrbPassivePatch.PatchLateAssembly(e.LoadedAssembly);
                OrbEvokePatch.PatchLateAssembly(e.LoadedAssembly);
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[UltraLib] 充能球延迟补丁出错: {ex.Message} / orb late patch error: {ex.Message}");
            }
        };
    }

    /// <summary>
    /// The mod's Harmony instance, null until <see cref="Init"/> is called.
    /// </summary>
    /// <remarks>
    /// 模组的 Harmony 实例，调用 <see cref="Init"/> 之前为 null。
    /// </remarks>
    internal static Harmony? Harmony => _harmony;
}

/// <summary>
/// Harmony patch for orb passive triggers: dynamically scans the <c>Passive</c> method
/// of every <c>OrbModel</c> subclass and injects Before/After hooks.
/// </summary>
/// <remarks>
/// 充能球被动触发（Passive）的 Hook Patch。
/// 动态扫描所有 OrbModel 子类的 Passive 方法，注入 Before/After Hook。
/// 在 UltraLib 之后加载的程序集会通过 <see cref="LateOrbPatchHelper"/> 延迟补丁。
/// </remarks>
[HarmonyPatch]
public static class OrbPassivePatch
{
    private static readonly HashSet<MethodBase> PatchedMethods = new();

    internal static void PatchLateAssembly(Assembly assembly)
    {
        foreach (var method in CollectPassiveMethods(new[] { assembly }))
        {
            if (LateOrbPatchHelper.Harmony == null || !PatchedMethods.Add(method))
                continue;
            LateOrbPatchHelper.Harmony.Patch(method,
                postfix: new HarmonyMethod(
                    typeof(OrbPassivePatch).GetMethod(nameof(Postfix), BindingFlags.Public | BindingFlags.Static)!));
        }
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var method in CollectPassiveMethods(AppDomain.CurrentDomain.GetAssemblies()))
        {
            // 只在首次加入时 yield：若延迟路径（PatchLateAssembly）已处理过该方法
            //（例如 Init 订阅后、PatchAll 之前加载的程序集），跳过以免重复打补丁。
            if (PatchedMethods.Add(method))
                yield return method;
        }
    }

    private static IEnumerable<MethodBase> CollectPassiveMethods(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
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
                if (method != null) yield return method;
            }
        }
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
/// 在 UltraLib 之后加载的程序集会通过 <see cref="LateOrbPatchHelper"/> 延迟补丁。
/// </remarks>
[HarmonyPatch]
public static class OrbEvokePatch
{
    private static readonly HashSet<MethodBase> PatchedMethods = new();

    internal static void PatchLateAssembly(Assembly assembly)
    {
        foreach (var method in CollectEvokeMethods(new[] { assembly }))
        {
            if (LateOrbPatchHelper.Harmony == null || !PatchedMethods.Add(method))
                continue;
            LateOrbPatchHelper.Harmony.Patch(method,
                postfix: new HarmonyMethod(
                    typeof(OrbEvokePatch).GetMethod(nameof(Postfix), BindingFlags.Public | BindingFlags.Static)!));
        }
    }

    static IEnumerable<MethodBase> TargetMethods()
    {
        foreach (var method in CollectEvokeMethods(AppDomain.CurrentDomain.GetAssemblies()))
        {
            // 只在首次加入时 yield：若延迟路径（PatchLateAssembly）已处理过该方法
            //（例如 Init 订阅后、PatchAll 之前加载的程序集），跳过以免重复打补丁。
            if (PatchedMethods.Add(method))
                yield return method;
        }
    }

    private static IEnumerable<MethodBase> CollectEvokeMethods(IEnumerable<Assembly> assemblies)
    {
        foreach (var assembly in assemblies)
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
                if (method != null) yield return method;
            }
        }
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
