using System.Reflection;
using Godot;
using MegaCrit.Sts2.Core.Combat;
using MegaCrit.Sts2.Core.GameActions;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Runs;

namespace UltraLib.Base.Utils;

/// <summary>
/// Helper for enqueueing GameActions into the action queue.
/// </summary>
/// <remarks>
/// GameAction 队列入队辅助工具。
/// 优先走 ActionQueueSynchronizer，单机回退到反射寻找 Enqueue 方法。
/// </remarks>
public static class ActionQueueHelper
{
    public static bool TryEnqueue(GameAction action)
    {
        if (action == null) return false;

        RunManager? runManager = RunManager.Instance;
        if (runManager?.ActionQueueSynchronizer != null)
        {
            try
            {
                runManager.ActionQueueSynchronizer.RequestEnqueue(action);
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[UltraLib] ActionQueueHelper RequestEnqueue failed: {ex} / 入队请求失败: {ex}");
            }
        }

        if (runManager?.NetService != null && runManager.NetService.Type.IsMultiplayer())
            return false;

        CombatManager combatManager = CombatManager.Instance;
        if (combatManager == null) return false;

        foreach (object candidate in CollectCandidates(combatManager, 2))
        {
            MethodInfo? enqueueMethod = FindEnqueueMethod(candidate.GetType());
            if (enqueueMethod == null) continue;

            try
            {
                enqueueMethod.Invoke(candidate, new object[] { action });
                return true;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[UltraLib] ActionQueueHelper fallback failed on {candidate.GetType().Name}: {ex} / 反射入队回退失败于 {candidate.GetType().Name}: {ex}");
            }
        }

        GD.PrintErr("[UltraLib] ActionQueueHelper: no enqueue method found / 找不到入队方法");
        return false;
    }

    private static List<object> CollectCandidates(object root, int maxDepth)
    {
        var results = new List<object>();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        var queue = new Queue<(object, int)>();
        queue.Enqueue((root, 0));

        while (queue.Count > 0)
        {
            var (obj, depth) = queue.Dequeue();
            if (!visited.Add(obj)) continue;
            results.Add(obj);
            if (depth >= maxDepth || IsSimpleType(obj.GetType())) continue;

            foreach (var member in obj.GetType().GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                object? value = member switch
                {
                    FieldInfo f => f.GetValue(obj),
                    PropertyInfo p when p.GetIndexParameters().Length == 0 => p.GetValue(obj),
                    _ => null
                };
                if (value == null || IsSimpleType(value.GetType())) continue;
                queue.Enqueue((value, depth + 1));
            }
        }
        return results;
    }

    private static bool IsSimpleType(Type type)
        => type.IsPrimitive || type.IsEnum || type == typeof(string) || type == typeof(decimal)
           || type == typeof(DateTime) || type == typeof(TimeSpan) || type == typeof(Guid)
           || (type.Namespace?.StartsWith("System", StringComparison.Ordinal) == true);

    private sealed class ReferenceEqualityComparer : IEqualityComparer<object>
    {
        public static readonly ReferenceEqualityComparer Instance = new();
        public new bool Equals(object? x, object? y) => ReferenceEquals(x, y);
        public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
    }

    private static MethodInfo? FindEnqueueMethod(Type type)
        => type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
            .FirstOrDefault(m => IsEnqueueLikeName(m.Name)
                                 && m.GetParameters().Length == 1
                                 && typeof(GameAction).IsAssignableFrom(m.GetParameters()[0].ParameterType));

    private static bool IsEnqueueLikeName(string name)
        => name.Contains("Enqueue", StringComparison.OrdinalIgnoreCase)
           || name.Contains("AddAction", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Queue", StringComparison.OrdinalIgnoreCase)
           || name.Contains("Request", StringComparison.OrdinalIgnoreCase);
}
