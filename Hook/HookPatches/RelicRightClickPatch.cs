using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Context;
using MegaCrit.Sts2.Core.ControllerInput;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Multiplayer.Game;
using MegaCrit.Sts2.Core.Nodes.Combat;
using MegaCrit.Sts2.Core.Nodes.GodotExtensions;
using MegaCrit.Sts2.Core.Nodes.Relics;
using MegaCrit.Sts2.Core.Runs;
using UltraLib.GameActions;
using UltraLib.Net;
using UltraLib.Base.Utils;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// Full Harmony patch for relic right-click: connects right-click input after
/// <c>NRelicInventoryHolder._Ready</c> and enqueues a GameAction for online sync.
/// </summary>
/// <remarks>
/// 遗物右键点击的完整 Hook Patch。
/// 在 NRelicInventoryHolder._Ready 后连接右键输入信号；
/// 检测右键/取消键，通过 GameAction 入队保证联机同步。
/// </remarks>
[HarmonyPatch(typeof(NRelicInventoryHolder), "_Ready", MethodType.Normal)]
public static class RelicRightClickPatch
{
    private const string ConnectedMetaKey = "UltraLib_RightClickConnected";

    [HarmonyPostfix]
    public static void Postfix(NRelicInventoryHolder __instance)
    {
        TryConnectRightClick(__instance);
    }

    private static void TryConnectRightClick(NRelicInventoryHolder relicHolder)
    {
        if (relicHolder == null || relicHolder.HasMeta(ConnectedMetaKey))
            return;

        relicHolder.SetMeta(ConnectedMetaKey, true);
        relicHolder.Connect(NClickableControl.SignalName.GuiInput,
            Callable.From<InputEvent>(e => HandleRelicRightClick(relicHolder, e)));
    }

    private static void HandleRelicRightClick(NRelicInventoryHolder relicHolder, InputEvent inputEvent)
    {
        if (relicHolder.GetViewport().IsInputHandled())
            return;
        if (NTargetManager.Instance?.IsInSelection == true)
            return;

        RelicModel? model = relicHolder.Relic?.Model;
        if (!relicHolder.Visible || model == null)
            return;

        if (!IsRightClickOrCancel(relicHolder, inputEvent))
            return;

        var owner = model.Owner;
        var me = owner != null ? LocalContext.GetMe(owner.RunState) : null;
        if (owner == null || me == null || owner.NetId != me.NetId)
            return;

        relicHolder.GetViewport().SetInputAsHandled();
        ProcessRelicRightClick(model, relicHolder);
    }

    private static void ProcessRelicRightClick(RelicModel model, NRelicInventoryHolder holder)
    {
        var action = new RelicRightClickAction(model, holder);

        if (ActionQueueHelper.TryEnqueue(action))
        {
            if (IsMultiplayer())
                RelicRightClickSyncNet.TrySendRightClick(model);
            return;
        }

        if (IsMultiplayer())
            GD.PrintErr("[UltraLib] RelicRightClickAction enqueue failed (multiplayer), firing hook directly / RelicRightClickAction 入队失败（联机），直接触发 hook");

        _ = PlusHooks.Plus_TriggerRelicRightClick(model, holder);
    }

    private static bool IsMultiplayer()
        => RunManager.Instance?.NetService?.Type.IsMultiplayer() == true;

    private static bool IsRightClickOrCancel(NRelicInventoryHolder relicHolder, InputEvent inputEvent)
    {
        if (inputEvent is InputEventMouseButton { ButtonIndex: MouseButton.Right } mouseButton)
            return mouseButton.IsReleased();
        if (inputEvent is InputEventAction actionEvent && relicHolder.HasFocus())
            return actionEvent.Action == MegaInput.cancel && actionEvent.IsPressed();
        return false;
    }
}
