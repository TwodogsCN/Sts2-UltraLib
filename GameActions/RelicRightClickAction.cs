using System.Collections;
using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Entities.Multiplayer;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.GameActions.Multiplayer;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Nodes.Relics;
using UltraLib.Hook;

namespace UltraLib.GameActions;

/// <summary>
/// GameAction for right-clicking a relic.
/// </summary>
/// <remarks>
/// 遗物右键操作的 GameAction。
/// <para>
/// 继承自 <see cref="GameAction"/>，确保在联机环境中，
/// 右键 Hook 能通过原版的 Action 队列同步到所有客户端。
/// </para>
/// </remarks>
public sealed class RelicRightClickAction : GameAction
{
    private readonly RelicModel _relic;
    private readonly NRelicInventoryHolder? _holder;

    /// <summary>
    /// Creates a relic right-click action.
    /// </summary>
    /// <remarks>
    /// 创建一个遗物右键 Action。
    /// </remarks>
    /// <param name="relic">被右键点击的遗物。</param>
    /// <param name="holder">遗物持有者 UI 控件（可能为 null）。</param>
    public RelicRightClickAction(RelicModel relic, NRelicInventoryHolder? holder)
    {
        _relic = relic;
        _holder = holder;
    }

    /// <summary>
    /// Creates a relic right-click action without a UI holder reference (resolved on execution).
    /// </summary>
    /// <remarks>
    /// 创建一个遗物右键 Action（不带 UI 控件引用，执行时会自动查找）。
    /// </remarks>
    /// <param name="relic">被右键点击的遗物。</param>
    public RelicRightClickAction(RelicModel relic)
    {
        _relic = relic;
        _holder = null;
    }

    /// <inheritdoc />
    public override ulong OwnerId => _relic?.Owner?.NetId ?? 0UL;

    /// <inheritdoc />
    public override GameActionType ActionType => GameActionType.Any;

    /// <inheritdoc />
    protected override async Task ExecuteAction()
    {
        if (_relic == null)
        {
            return;
        }

        GD.Print($"[UltraLib] 遗物右键 Action 执行: {_relic.Id} / executing relic right-click action: {_relic.Id}");

        try
        {
            // 如果构造时未传入 holder，尝试从场景树中查找
            NRelicInventoryHolder? holder = _holder ?? FindRelicHolder(_relic);

            // 触发遗物右键 Hook
            await PlusHooks.Plus_TriggerRelicRightClick(_relic, holder);

            GD.Print($"[UltraLib] 遗物右键 Action ({_relic.Id}) 执行完毕 / relic right-click action completed: {_relic.Id}");
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 执行 RelicRightClickAction 崩溃: {ex} / RelicRightClickAction execution crashed: {ex}");
        }
    }

    /// <inheritdoc />
    public override INetAction ToNetAction()
    {
        return new NetRelicRightClickAction
        {
            RelicId = _relic?.Id.ToString() ?? string.Empty
        };
    }

    /// <summary>
    /// Recursively finds the UI holder control for the given relic model in the scene tree.
    /// </summary>
    /// <remarks>
    /// 在场景树中递归查找指定遗物模型对应的 UI 控件。
    /// </remarks>
    /// <param name="model">要查找的遗物模型。</param>
    /// <returns>找到的遗物持有者控件，未找到则返回 null。</returns>
    private static NRelicInventoryHolder? FindRelicHolder(RelicModel model)
    {
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null)
        {
            return null;
        }

        try
        {
            return FindRelicHolderRecursive(tree.Root, model);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 查找遗物 UI 节点时异常: {ex.Message} / error finding relic UI node: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Recursively searches the scene tree for the relic holder control.
    /// </summary>
    /// <remarks>
    /// 在场景树中递归查找遗物持有者控件。
    /// </remarks>
    /// <param name="node">当前遍历的节点。</param>
    /// <param name="model">要查找的遗物模型。</param>
    /// <returns>找到的遗物持有者控件，未找到则返回 null。</returns>
    private static NRelicInventoryHolder? FindRelicHolderRecursive(Node node, RelicModel model)
    {
        if (node == null) return null;

        if (node is NRelicInventoryHolder holder && holder.Relic?.Model == model)
        {
            return holder;
        }

        foreach (Node child in node.GetChildren())
        {
            if (child == null) continue;
            NRelicInventoryHolder? found = FindRelicHolderRecursive(child, model);
            if (found != null)
            {
                return found;
            }
        }

        return null;
    }
}

/// <summary>
/// Network serialization structure for the relic right-click action.
/// </summary>
/// <remarks>
/// 遗物右键 Action 的网络序列化结构。
/// 在联机环境中，GameAction 通过此结构序列化并传输到其他客户端。
/// </remarks>
public struct NetRelicRightClickAction : INetAction
{
    /// <summary>
    /// ID of the relic that was right-clicked.
    /// </summary>
    /// <remarks>
    /// 被右键点击的遗物 ID。
    /// </remarks>
    public string RelicId;

    /// <summary>
    /// Deserializes the network data into an actual GameAction.
    /// </summary>
    /// <remarks>
    /// 将网络数据反序列化为实际的 GameAction。
    /// </remarks>
    /// <param name="player">接收此 Action 的玩家。</param>
    /// <returns>对应的 GameAction 实例。</returns>
    public GameAction ToGameAction(Player player)
    {
        RelicModel? relic = FindRelicInPlayer(player, RelicId);
        if (relic == null)
        {
            return new EmptyRelicRightClickAction();
        }

        return new RelicRightClickAction(relic);
    }

    /// <inheritdoc />
    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(RelicId ?? string.Empty);
    }

    /// <inheritdoc />
    public void Deserialize(PacketReader reader)
    {
        RelicId = reader.ReadString();
    }

    /// <summary>
    /// Finds a relic with the given ID on the player.
    /// </summary>
    /// <remarks>
    /// 在玩家身上查找指定 ID 的遗物。
    /// </remarks>
    private static RelicModel? FindRelicInPlayer(Player player, string relicId)
    {
        if (player == null || string.IsNullOrEmpty(relicId))
        {
            return null;
        }

        foreach (RelicModel relic in EnumerateRelics(player))
        {
            if (relic != null && string.Equals(relic.Id.ToString(), relicId, StringComparison.Ordinal))
            {
                return relic;
            }
        }

        return null;
    }

    /// <summary>
    /// Enumerates all relics owned by the player (via reflection over enumerable properties).
    /// </summary>
    /// <remarks>
    /// 枚举玩家拥有的所有遗物（通过反射遍历所有可枚举的属性）。
    /// </remarks>
    private static IEnumerable EnumerateRelics(Player player)
    {
        foreach (PropertyInfo prop in player.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (prop == null || !typeof(IEnumerable).IsAssignableFrom(prop.PropertyType))
            {
                continue;
            }

            if (prop.GetValue(player) is not IEnumerable enumerable)
            {
                continue;
            }

            foreach (object? item in enumerable)
            {
                if (item is RelicModel relic)
                {
                    yield return relic;
                }
            }
        }
    }
}

/// <summary>
/// Empty relic right-click action, used as a degraded fallback when network deserialization fails.
/// </summary>
/// <remarks>
/// 空的遗物右键 Action，用于网络反序列化失败时的降级处理。
/// </remarks>
internal sealed class EmptyRelicRightClickAction : GameAction
{
    /// <inheritdoc />
    public override ulong OwnerId => 0UL;

    /// <inheritdoc />
    public override GameActionType ActionType => GameActionType.Any;

    /// <inheritdoc />
    protected override Task ExecuteAction() => Task.CompletedTask;

    /// <inheritdoc />
    public override INetAction ToNetAction()
    {
        return new NetRelicRightClickAction { RelicId = string.Empty };
    }
}
