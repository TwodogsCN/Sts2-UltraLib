using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Modding;
using UltraLib.Hook.HookPatches;

namespace UltraLib.UltraLibCode;

/// <summary>
/// Main entry point of the UltraLib mod.
/// </summary>
/// <remarks>
/// UltraLib 模组主入口。
/// <para>
/// 框架职责：
/// <list type="bullet">
///   <item>初始化 Harmony 实例并 Patch 所有被 [HarmonyPatch] 标记的方法。</item>
///   <item>提供全局日志记录器供库内部使用。</item>
///   <item>注册模组元数据（模组 ID、依赖等）由 <c>UltraLib.json</c> 管理。</item>
/// </list>
/// </para>
/// </remarks>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    /// <summary>
    /// Unique identifier of the mod.
    /// </summary>
    /// <remarks>
    /// 模组唯一标识符。当前用途：Harmony 实例 ID 和日志前缀。
    /// </remarks>
    public const string ModId = "UltraLib";

    /// <summary>
    /// Global logger for internal use by UltraLib modules.
    /// </summary>
    /// <remarks>
    /// 全局日志记录器，供 UltraLib 内部各模块使用。
    /// </remarks>
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    /// <summary>
    /// Mod initialization entry, called automatically by the game on load.
    /// </summary>
    /// <remarks>
    /// 模组初始化入口，由游戏在加载时自动调用。
    /// <para>
    /// 初始化流程：
    /// 1. 创建 Harmony 实例（ID = "UltraLib"）。
    /// 2. 执行 PatchAll 扫描本程序集中所有 [HarmonyPatch] 标记的类。
    /// 3. Patch 失败时仅记录警告，不会导致整个模组崩溃。
    /// </para>
    /// </remarks>
    public static void Initialize()
    {
        // 创建 Harmony 实例，所有 Patch 类将自动被扫描
        Harmony harmony = new(ModId);

        try
        {
            // 注册充能球钩子的延迟补丁：
            // UltraLib 作为依赖模组先于内容模组加载，此时内容模组里的自定义充能球
            // （如 UltimatePlus 的 MagnetOrb）尚未加载，PatchAll 扫描不到它们的
            // Evoke/Passive 方法。这里订阅 AssemblyLoad，在后加载的程序集中出现
            // 新的充能球类型时补上钩子补丁。
            LateOrbPatchHelper.Init(harmony);

            // 批量 Patch 本程序集中所有带有 [HarmonyPatch] 的静态类
            // 显式传入 Assembly 确保只扫描 UltraLib 自身的 Patch
            harmony.PatchAll(typeof(MainFile).Assembly);
        }
        catch (System.Exception ex)
        {
            // 某个 Patch 失败时记录警告但不阻塞初始化
            GD.PrintErr($"[{ModId}] 部分 Patch 注入失败: {ex.Message} / some patches failed to apply: {ex.Message}");
        }

        GD.Print($"[{ModId}] 模组初始化完成！ / mod initialization complete.");
    }
}
