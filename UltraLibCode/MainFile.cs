using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Modding;

namespace UltraLib.UltraLibCode;

/// <summary>
/// UltraLib 模组主入口。
/// <para>
/// 框架职责：
/// <list type="bullet">
///   <item>初始化 Harmony 实例并 Patch 所有被 [HarmonyPatch] 标记的方法。</item>
///   <item>提供全局日志记录器供库内部使用。</item>
///   <item>注册模组元数据（模组 ID、依赖等）由 <c>UltraLib.json</c> 管理。</item>
/// </list>
/// </para>
/// </summary>
[ModInitializer(nameof(Initialize))]
public partial class MainFile : Node
{
    /// <summary>
    /// 模组唯一标识符。
    /// <para>当前用途：Harmony 实例 ID 和日志前缀。</para>
    /// </summary>
    public const string ModId = "UltraLib";

    /// <summary>
    /// 全局日志记录器，供 UltraLib 内部各模块使用。
    /// </summary>
    public static MegaCrit.Sts2.Core.Logging.Logger Logger { get; } =
        new(ModId, MegaCrit.Sts2.Core.Logging.LogType.Generic);

    /// <summary>
    /// 模组初始化入口，由游戏在加载时自动调用。
    /// <para>
    /// 初始化流程：
    /// 1. 创建 Harmony 实例（ID = "UltraLib"）。
    /// 2. 执行 PatchAll 扫描本程序集中所有 [HarmonyPatch] 标记的类。
    /// 3. Patch 失败时仅记录警告，不会导致整个模组崩溃。
    /// </para>
    /// </summary>
    public static void Initialize()
    {
        // 创建 Harmony 实例，所有 Patch 类将自动被扫描
        Harmony harmony = new(ModId);

        try
        {
            // 批量 Patch 本程序集中所有带有 [HarmonyPatch] 的静态类
            // 显式传入 Assembly 确保只扫描 UltraLib 自身的 Patch
            harmony.PatchAll(typeof(MainFile).Assembly);
        }
        catch (System.Exception ex)
        {
            // 某个 Patch 失败时记录警告但不阻塞初始化
            Log.Warn($"[{ModId}] 部分 Patch 注入失败: {ex.Message}");
        }

        Log.Info($"[{ModId}] 模组初始化完成！");
    }
}
