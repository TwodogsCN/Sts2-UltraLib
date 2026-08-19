using System.Reflection;
using Godot;
using HarmonyLib;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using UltraLib.Base.Abstract;

namespace UltraLib.Hook.HookPatches;

/// <summary>
/// 充能遗物 UI 显示 Patch。
/// <para>
/// 为具有 <see cref="PlusChargeRelic.UseChargeBarDisplay"/> 的充能遗物，
/// 在遗物图标下方渲染一条充能进度条，以可视化方式显示当前充能状态。
/// </para>
/// <para>
/// 进度条采用尖塔复古风格，颜色随充能层数变化：
/// 未满时绿色渐变，过载时金色→血红→邪能紫→幽冥蓝。
/// </para>
/// </summary>
[HarmonyPatch(typeof(NRelicInventoryHolder))]
public static class ChargeRelicUiPatch
{
    private const string GridContainerName = "PlusChargeGridContainer";
    private const string BackgroundPanelName = "PlusChargeBgPanel";
    private const string StyleBoxStorageKey = "PlusCustomStyleBox";

    private const float TotalWidth = 58f;
    private const float TotalHeight = 5f;
    private const float PanelWidth = TotalWidth + 2;
    private const float PanelHeight = TotalHeight + 2;

    private static readonly Color ColorEmptySlot = new(0.38f, 0.36f, 0.34f, 0.85f);
    private static readonly Color ColorBorderNormal = new(0.14f, 0.11f, 0.08f, 1.0f);
    private static readonly Color ColorDefault = new(0.16f, 0.62f, 0.26f);
    private static readonly Color ColorFullBase = new(0.35f, 0.75f, 0.42f);

    private static readonly Color[] OverchargeColors =
    [
        new(0.78f, 0.46f, 0.12f),  // 古朴暗金
        new(0.62f, 0.16f, 0.16f),  // 仪式血红
        new(0.42f, 0.16f, 0.62f),  // 邪能虚空紫
        new(0.12f, 0.42f, 0.62f),  // 幽冥深蓝
    ];

    private static readonly FieldInfo RelicField = typeof(NRelicInventoryHolder)
        .GetField("_relic", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly FieldInfo AmountLabelField = typeof(NRelicInventoryHolder)
        .GetField("_amountLabel", BindingFlags.NonPublic | BindingFlags.Instance);

    private static readonly MethodInfo RefreshAmountMethod = typeof(NRelicInventoryHolder)
        .GetMethod("RefreshAmount", BindingFlags.NonPublic | BindingFlags.Instance);

    /// <summary>
    /// 在遗物模型变更时创建/更新充能条背景容器。
    /// </summary>
    [HarmonyPatch("OnModelChanged")]
    [HarmonyPostfix]
    public static void OnModelChanged_Postfix(NRelicInventoryHolder __instance, RelicModel? newModel)
    {
        RemoveOldPanel(__instance);

        if (newModel is PlusChargeRelic chargeRelic && chargeRelic.UseChargeBarDisplay)
            CreateChargeBarPanel(__instance, chargeRelic);
    }

    /// <summary>
    /// 在 _Ready 后执行一帧，确保读档后充能条与保存的值同步。
    /// </summary>
    [HarmonyPatch("_Ready")]
    [HarmonyPostfix]
    public static void Ready_Postfix(NRelicInventoryHolder __instance)
    {
        if (RelicField?.GetValue(__instance) is not NRelic nr ||
            nr.Model is not PlusChargeRelic chargeRelic ||
            !chargeRelic.UseChargeBarDisplay)
            return;

        var tree = __instance.GetTree();
        if (tree == null) return;

        // 延迟一帧确保所有属性已恢复
        Action? onFrame = null;
        onFrame = () =>
        {
            tree.ProcessFrame -= onFrame;
            if (!GodotObject.IsInstanceValid(__instance)) return;

            RefreshAmountMethod?.Invoke(__instance, null);
        };
        tree.ProcessFrame += onFrame;
    }

    /// <summary>
    /// 在 RefreshAmount 后更新充能条的每个格子。
    /// </summary>
    [HarmonyPatch("RefreshAmount")]
    [HarmonyPostfix]
    public static void RefreshAmount_Postfix(NRelicInventoryHolder __instance)
    {
        if (__instance == null) return;
        if (RelicField?.GetValue(__instance) is not Control relicNode ||
            AmountLabelField?.GetValue(__instance) is not Control amountLabel)
            return;

        var bgPanel = relicNode.GetNodeOrNull<PanelContainer>(BackgroundPanelName);

        if (relicNode is NRelic nRelic && nRelic.Model is PlusChargeRelic chargeRelic && chargeRelic.UseChargeBarDisplay)
        {
            RenderChargeBar(relicNode, bgPanel, amountLabel, chargeRelic);
        }
        else
        {
            if (bgPanel != null) bgPanel.Visible = false;
            if (amountLabel != null && relicNode is NRelic nr && nr.Model != null)
            {
                var run = MegaCrit.Sts2.Core.Runs.RunManager.Instance;
                amountLabel.Visible = nr.Model.ShowCounter && run != null && run.IsInProgress;
            }
        }
    }

    // ==========================================
    // 内部实现
    // ==========================================

    private static void RemoveOldPanel(NRelicInventoryHolder __instance)
    {
        if (RelicField?.GetValue(__instance) is not Control relicNode) return;

        var oldPanel = relicNode.GetNodeOrNull<PanelContainer>(BackgroundPanelName);
        if (oldPanel != null)
        {
            if (oldPanel.HasMeta(StyleBoxStorageKey))
            {
                if (oldPanel.GetMeta(StyleBoxStorageKey).AsGodotObject() is IDisposable sb)
                    sb.Dispose();
            }
            relicNode.RemoveChild(oldPanel);
            oldPanel.QueueFree();
        }
    }

    private static void CreateChargeBarPanel(NRelicInventoryHolder __instance, PlusChargeRelic chargeRelic)
    {
        if (RelicField?.GetValue(__instance) is not Control relicNode) return;

        var bgPanel = new PanelContainer
        {
            Name = BackgroundPanelName,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(PanelWidth, PanelHeight),
            Size = new Vector2(PanelWidth, PanelHeight),
        };

        var styleBox = new StyleBoxFlat
        {
            DrawCenter = true,
            BgColor = ColorBorderNormal,
            BorderWidthLeft = 1,
            BorderWidthRight = 1,
            BorderWidthTop = 1,
            BorderWidthBottom = 1,
            BorderColor = ColorBorderNormal,
            CornerRadiusTopLeft = 1,
            CornerRadiusTopRight = 1,
            CornerRadiusBottomLeft = 1,
            CornerRadiusBottomRight = 1,
        };

        bgPanel.AddThemeStyleboxOverride("panel", styleBox);
        bgPanel.SetMeta(StyleBoxStorageKey, styleBox);

        var hBox = new HBoxContainer
        {
            Name = GridContainerName,
            Alignment = BoxContainer.AlignmentMode.Center,
            MouseFilter = Control.MouseFilterEnum.Ignore,
            CustomMinimumSize = new Vector2(TotalWidth, TotalHeight),
            SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter,
            SizeFlagsVertical = Control.SizeFlags.ShrinkCenter,
        };
        hBox.AddThemeConstantOverride("separation", 1);

        bgPanel.AddChild(hBox);
        relicNode.AddChild(bgPanel);

        bgPanel.Position = new Vector2(
            (relicNode.Size.X - PanelWidth) / 2f,
            relicNode.Size.Y - PanelHeight);
    }

    private static void RenderChargeBar(Control relicNode, PanelContainer? bgPanel,
        Control amountLabel, PlusChargeRelic chargeRelic)
    {
        amountLabel.Visible = false;
        if (bgPanel == null) return;

        bgPanel.Visible = chargeRelic.ShowCounter;
        if (!bgPanel.Visible) return;

        bgPanel.Position = new Vector2(
            (relicNode.Size.X - PanelWidth) / 2f,
            relicNode.Size.Y - PanelHeight);

        var gridContainer = bgPanel.GetNodeOrNull<HBoxContainer>(GridContainerName);
        if (gridContainer == null) return;

        int current = chargeRelic.NowCharge;
        int max = chargeRelic.TotalCharge;
        if (max <= 0) return;

        // 更新边框颜色（充满时发光）
        if (bgPanel.HasMeta(StyleBoxStorageKey) &&
            bgPanel.GetMeta(StyleBoxStorageKey).AsGodotObject() is StyleBoxFlat sb)
        {
            if (current >= max)
            {
                sb.ShadowColor = new Color(0.85f, 0.70f, 0.35f, 0.75f);
                sb.ShadowSize = 1;
                sb.ShadowOffset = Vector2.Zero;
            }
            else
            {
                sb.ShadowSize = 0;
            }
        }

        // 同步格子数量
        while (gridContainer.GetChildCount() > max)
        {
            var child = gridContainer.GetChild(gridContainer.GetChildCount() - 1);
            gridContainer.RemoveChild(child);
            child.CallDeferred("queue_free");
        }
        while (gridContainer.GetChildCount() < max)
        {
            var cell = new TextureRect
            {
                MouseFilter = Control.MouseFilterEnum.Ignore,
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
                ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            };
            gridContainer.AddChild(cell);
        }

        // 填充每个格子
        for (int i = 0; i < max; i++)
        {
            if (gridContainer.GetChild(i) is not TextureRect cell) continue;

            // 复用或创建渐变纹理
            GradientTexture2D gradTex;
            Gradient gradient;
            if (cell.Texture is GradientTexture2D existing && existing.Gradient != null)
            {
                gradTex = existing;
                gradient = existing.Gradient;
            }
            else
            {
                gradient = new Gradient();
                gradTex = new GradientTexture2D
                {
                    Gradient = gradient,
                    Width = 16,
                    Height = 16,
                    Fill = GradientTexture2D.FillEnum.Linear,
                };
                cell.Texture = gradTex;
            }

            bool isEmpty = current == 0;
            Color baseColor = ColorEmptySlot;

            if (!isEmpty)
            {
                int cellLayer = -1;
                while (current > (cellLayer * max) + i)
                    cellLayer++;

                if (cellLayer == 0 && current < max && i >= current)
                    isEmpty = true;
                else
                    baseColor = GetOverchargeColor(cellLayer);
            }

            if (isEmpty)
            {
                gradTex.FillFrom = new Vector2(0.1f, 0.0f);
                gradTex.FillTo = new Vector2(0.9f, 1.0f);
                gradient.SetColor(0, ColorEmptySlot);
                gradient.SetColor(1, ColorEmptySlot.Darkened(0.35f));
            }
            else
            {
                float progressLeft = i / (float)max;
                float cellRatio = (progressLeft + (i + 1) / (float)max) / 2f;
                float glow = cellRatio * 0.28f;

                var leftTop = new Color(
                    Mathf.Min(1f, baseColor.R + glow + 0.15f),
                    Mathf.Min(1f, baseColor.G + glow + 0.15f),
                    Mathf.Min(1f, baseColor.B + glow + 0.15f), baseColor.A);
                var rightBottom = new Color(
                    Mathf.Min(1f, baseColor.R + glow),
                    Mathf.Min(1f, baseColor.G + glow),
                    Mathf.Min(1f, baseColor.B + glow), baseColor.A).Darkened(0.25f);

                gradTex.FillFrom = new Vector2(0.0f, 0.0f);
                gradTex.FillTo = new Vector2(1.0f, 1.0f);
                gradient.SetColor(0, leftTop);
                gradient.SetColor(1, rightBottom);
            }
        }
    }

    private static Color GetOverchargeColor(int layer)
    {
        if (layer < 0) return ColorEmptySlot;
        if (layer == 0) return ColorDefault;
        if (layer == 1) return ColorFullBase;
        return OverchargeColors[(layer - 2) % OverchargeColors.Length];
    }
}
