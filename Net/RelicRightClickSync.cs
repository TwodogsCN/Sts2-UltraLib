using System.Collections;
using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Logging;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Multiplayer.Serialization;
using MegaCrit.Sts2.Core.Multiplayer.Transport;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Hook;

namespace UltraLib.Net;

public struct RelicRightClickMessage : INetMessage
{
    public bool ShouldBroadcast => true;
    public NetTransferMode Mode => NetTransferMode.Reliable;
    public LogLevel LogLevel => LogLevel.Info;
    public bool ShouldBuffer => false;

    public string RelicId;
    public string OwnerKeyName;
    public string OwnerKeyValue;

    public void Serialize(PacketWriter writer)
    {
        writer.WriteString(RelicId ?? string.Empty);
        writer.WriteString(OwnerKeyName ?? string.Empty);
        writer.WriteString(OwnerKeyValue ?? string.Empty);
    }

    public void Deserialize(PacketReader reader)
    {
        RelicId = reader.ReadString();
        OwnerKeyName = reader.ReadString();
        OwnerKeyValue = reader.ReadString();
    }
}

internal static class RelicRightClickSyncNet
{
    private static INetGameService? _registeredOn;

    internal static void RegisterHandlers(INetGameService net)
    {
        if (net == null || !net.Type.IsMultiplayer() || _registeredOn == net)
            return;
        _registeredOn = net;
        net.RegisterMessageHandler<RelicRightClickMessage>(OnRelicRightClickMessage);
        Log.Info("[UltraLib] RelicRightClickSyncNet handlers registered");
    }

    internal static bool TrySendRightClick(RelicModel model)
    {
        INetGameService? net = RunManager.Instance?.NetService;
        if (net == null || !net.Type.IsMultiplayer()) return false;

        GetOwnerKey(model, out string ownerKeyName, out string ownerKeyValue);
        var msg = new RelicRightClickMessage
        {
            RelicId = model?.Id.ToString() ?? string.Empty,
            OwnerKeyName = ownerKeyName,
            OwnerKeyValue = ownerKeyValue
        };
        try
        {
            net.SendMessage(msg);
            return true;
        }
        catch (Exception ex)
        {
            Log.Warn("[UltraLib] Send relic right click failed: " + ex);
            return false;
        }
    }

    private static void OnRelicRightClickMessage(RelicRightClickMessage msg, ulong senderId)
    {
        try
        {
            if (IsLocalSender(senderId)) return;
            if (!IsHost()) return;

            RelicModel? model = FindRelicFromMessage(msg);
            if (model == null)
            {
                Log.Warn("[UltraLib] Relic right click sync: relic not found");
                return;
            }

            NRelicInventoryHolder? holder = FindRelicHolder(model);
            _ = PlusHooks.Plus_TriggerRelicRightClick(model, holder);
        }
        catch (Exception ex)
        {
            Log.Warn("[UltraLib] Relic right click handler failed: " + ex);
        }
    }

    private static bool IsHost()
    {
        return RunManager.Instance?.NetService?.Type == NetGameType.Host;
    }

    private static RelicModel? FindRelicFromMessage(RelicRightClickMessage msg)
    {
        IRunState? runState = RunManager.Instance?.DebugOnlyGetState();
        if (runState == null || string.IsNullOrEmpty(msg.RelicId)) return null;

        Player? owner = FindPlayer(runState, msg.OwnerKeyName, msg.OwnerKeyValue);
        if (owner != null)
        {
            RelicModel? relic = FindRelicInPlayer(owner, msg.RelicId);
            if (relic != null) return relic;
        }

        RelicModel? uniqueMatch = null;
        foreach (Player player in EnumeratePlayers(runState))
        {
            RelicModel? relic = FindRelicInPlayer(player, msg.RelicId);
            if (relic == null) continue;
            if (uniqueMatch != null) return null;
            uniqueMatch = relic;
        }
        return uniqueMatch;
    }

    private static Player? FindPlayer(IRunState runState, string keyName, string keyValue)
    {
        if (string.IsNullOrEmpty(keyName) || string.IsNullOrEmpty(keyValue)) return null;
        foreach (Player player in EnumeratePlayers(runState))
        {
            var prop = player.GetType().GetProperty(keyName, BindingFlags.Public | BindingFlags.Instance);
            if (prop == null) continue;
            object? value = prop.GetValue(player);
            if (value != null && string.Equals(value.ToString(), keyValue, StringComparison.Ordinal))
                return player;
        }
        return null;
    }

    private static IEnumerable<Player> EnumeratePlayers(IRunState runState)
    {
        foreach (var prop in runState.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) continue;
            if (prop.GetValue(runState) is not IEnumerable enumerable) continue;
            foreach (object? item in enumerable)
                if (item is Player player) yield return player;
        }
    }

    private static RelicModel? FindRelicInPlayer(Player player, string relicId)
    {
        foreach (RelicModel relic in EnumerateRelics(player))
            if (string.Equals(relic.Id.ToString(), relicId, StringComparison.Ordinal))
                return relic;
        return null;
    }

    private static IEnumerable<RelicModel> EnumerateRelics(Player player)
    {
        foreach (var prop in player.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!typeof(IEnumerable).IsAssignableFrom(prop.PropertyType)) continue;
            if (prop.GetValue(player) is not IEnumerable enumerable) continue;
            foreach (object? item in enumerable)
                if (item is RelicModel relic) yield return relic;
        }
    }

    private static void GetOwnerKey(RelicModel model, out string keyName, out string keyValue)
    {
        keyName = string.Empty; keyValue = string.Empty;
        object? owner = GetPropertyValue(model, "Owner") ?? GetPropertyValue(model, "Player");
        if (owner == null) return;

        foreach (string candidate in new[] { "PlayerId", "Id", "PlayerIndex", "Index", "NetId" })
        {
            object? value = GetPropertyValue(owner, candidate);
            if (value == null) continue;
            keyName = candidate;
            keyValue = value.ToString() ?? string.Empty;
            if (!string.IsNullOrEmpty(keyValue)) return;
        }
    }

    private static object? GetPropertyValue(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null) return null;
        try { return prop.GetValue(target); } catch { return null; }
    }

    internal static NRelicInventoryHolder? FindRelicHolder(RelicModel model)
    {
        if (model == null) return null;
        if (Engine.GetMainLoop() is not SceneTree tree || tree.Root == null) return null;
        return FindRelicHolderRecursive(tree.Root, model);
    }

    private static NRelicInventoryHolder? FindRelicHolderRecursive(Node node, RelicModel model)
    {
        if (node == null) return null;
        if (node is NRelicInventoryHolder holder && holder.Relic?.Model == model) return holder;
        foreach (Node child in node.GetChildren())
        {
            if (child == null) continue;
            var found = FindRelicHolderRecursive(child, model);
            if (found != null) return found;
        }
        return null;
    }

    private static bool IsLocalSender(ulong senderId)
    {
        var net = RunManager.Instance?.NetService;
        if (net == null) return false;
        ulong? localId = TryGetUlongProperty(net, "LocalPeerId")
                      ?? TryGetUlongProperty(net, "LocalClientId")
                      ?? TryGetUlongProperty(net, "PeerId");
        return localId.HasValue && localId.Value == senderId;
    }

    private static ulong? TryGetUlongProperty(object target, string propertyName)
    {
        var prop = target.GetType().GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
        if (prop == null) return null;
        object? value = prop.GetValue(target);
        return value switch
        {
            ulong u => u,
            long l when l >= 0 => (ulong)l,
            int i when i >= 0 => (ulong)i,
            _ => null
        };
    }
}

[HarmonyPatch(typeof(RunManager))]
internal static class RunManagerRelicRightClickRegisterHandlersPatch
{
    [HarmonyPatch("InitializeShared")]
    [HarmonyPostfix]
    private static void InitPostfix() => TryRegister();

    [HarmonyPatch("SetUpNewMultiplayer")]
    [HarmonyPostfix]
    private static void NewMpPostfix() => TryRegister();

    [HarmonyPatch("SetUpSavedMultiplayer")]
    [HarmonyPostfix]
    private static void SavedMpPostfix() => TryRegister();

    private static void TryRegister()
    {
        var net = RunManager.Instance?.NetService;
        if (net != null) RelicRightClickSyncNet.RegisterHandlers(net);
    }
}
