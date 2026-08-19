using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes.Relics;
using UltraLib.Base.Utils;

namespace UltraLib.Base.Scripts;

/// <summary>
/// 遗物选择器配置参数。
/// </summary>
/// <param name="HeaderText">选择界面标题。</param>
/// <param name="MinSelect">最少可选数量。</param>
/// <param name="MaxSelect">最多可选数量。</param>
/// <param name="RequireManualConfirmation">是否需要手动确认。</param>
public record RelicSelectorPrefs(string HeaderText, int MinSelect = 1, int MaxSelect = 1, bool RequireManualConfirmation = true);

/// <summary>
/// 简单的遗物选择界面控制器。
/// <para>
/// 创建一个 Godot 场景弹窗，以网格形式展示遗物供玩家选择。
/// 依赖场景文件 <c>res://Base/Scenes/screens/simple_relic_select_screen.tscn</c>。
/// </para>
/// </summary>
public class NSimpleRelicSelectScreen
{
    private const string ScenePath = "res://Base/Scenes/screens/simple_relic_select_screen.tscn";

    private Node _rootNode;
    private GridContainer _gridContainer;
    private RichTextLabel _headerLabel;
    private Button _confirmButton;

    private IReadOnlyList<RelicModel> _relics;
    private RelicSelectorPrefs _prefs;
    private Player _playerContext;

    private readonly List<RelicModel> _selectedRelics = new();
    private readonly Dictionary<RelicModel, Control> _relicHolders = new();
    private readonly Dictionary<RelicModel, Panel> _relicBgPanels = new();

    private readonly TaskCompletionSource<IEnumerable<RelicModel>> _tcs = new();
    private Control _currentTipInstance;

    /// <summary>
    /// 创建遗物选择界面并返回玩家选择结果。
    /// </summary>
    /// <param name="relics">可供选择的遗物列表。</param>
    /// <param name="prefs">选择器配置。</param>
    /// <param name="player">进行选择的玩家。</param>
    /// <returns>玩家选中的遗物列表。</returns>
    public static Task<IEnumerable<RelicModel>> Create(IReadOnlyList<RelicModel> relics, RelicSelectorPrefs prefs, Player player)
    {
        var controller = new NSimpleRelicSelectScreen
        {
            _relics = relics.Select(r => r.ToMutable()).ToList(),
            _prefs = prefs,
            _playerContext = player
        };
        controller.InitializeScreen();
        return controller._tcs.Task;
    }

    private void InitializeScreen()
    {
        try
        {
            var scene = GD.Load<PackedScene>(ScenePath);
            _rootNode = scene.Instantiate();

            _gridContainer = _rootNode.GetNode<GridContainer>("Panel/ScrollContainer/GridContainer");
            _headerLabel = _rootNode.GetNode<RichTextLabel>("Panel/HeaderLabel");
            _confirmButton = _rootNode.GetNode<Button>("Panel/ConfirmButton");

            _headerLabel.Text = $"[center]{_prefs.HeaderText}[/center]";
            _confirmButton.Pressed += OnConfirmButtonPressed;

            PopulateRelicGrid();
            UpdateConfirmButtonState();

            var tree = (SceneTree)Engine.GetMainLoop();
            tree.Root.AddChild(_rootNode);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] Relic selection screen init failed: {ex}");
            _tcs.TrySetException(ex);
        }
    }

    private void PopulateRelicGrid()
    {
        foreach (Node child in _gridContainer.GetChildren())
            child.QueueFree();

        foreach (var relic in _relics)
        {
            var holder = CreateRelicHolderNode(relic);
            if (holder == null) continue;

            var panel = new Panel();
            _relicBgPanels[relic] = panel;
            panel.AddChild(holder);

            // 点击选择
            var clickCatcher = new Button
            {
                Flat = true,
                MouseFilter = Control.MouseFilterEnum.Pass,
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
            };
            clickCatcher.Pressed += () => OnRelicClicked(relic);

            // 悬浮提示
            clickCatcher.MouseEntered += () => ShowNativeHoverTip(relic, holder);
            clickCatcher.MouseExited += HideNativeHoverTip;

            var margin = new MarginContainer();
            margin.AddChild(panel);
            margin.AddChild(clickCatcher);
            _gridContainer.AddChild(margin);
        }
    }

    private Control CreateRelicHolderNode(RelicModel relic)
    {
        try
        {
            var scene = GD.Load<PackedScene>("res://scenes/ui/relic_inventory_holder.tscn");
            var holder = scene.Instantiate<Control>();
            // 设置遗物数据（通过 Reflection 或公开方法）
            var relicNode = holder.GetNode<NRelic>("%Relic");
            if (relicNode != null)
            {
                var modelField = relicNode.GetType().GetField("Model",
                    System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (modelField != null)
                    modelField.SetValue(relicNode, relic);
                else
                {
                    var modelProp = relicNode.GetType().GetProperty("Model",
                        System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    modelProp?.SetValue(relicNode, relic);
                }
            }

            _relicHolders[relic] = holder;
            return holder;
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] Create relic holder failed: {ex}");
            return null;
        }
    }

    private void OnRelicClicked(RelicModel relic)
    {
        if (_selectedRelics.Contains(relic))
            _selectedRelics.Remove(relic);
        else
            _selectedRelics.Add(relic);

        UpdateRelicVisualState(relic, _selectedRelics.Contains(relic));
        UpdateConfirmButtonState();

        if (!_prefs.RequireManualConfirmation && _selectedRelics.Count >= _prefs.MinSelect)
            CompleteSelection();
    }

    private void UpdateRelicVisualState(RelicModel relic, bool isSelected)
    {
        if (!_relicBgPanels.TryGetValue(relic, out var panel)) return;

        var style = panel.GetThemeStylebox("panel").Duplicate() as StyleBoxFlat;
        if (style == null) return;

        if (isSelected)
        {
            style.BorderColor = new Color(0.3f, 0.76f, 1f, 1f);
            style.BgColor = new Color(0.2f, 0.4f, 0.6f, 0.4f);
        }
        else
        {
            style.BorderColor = Colors.Transparent;
            style.BgColor = new Color(0, 0, 0, 0.3f);
        }
        panel.AddThemeStyleboxOverride("panel", style);
    }

    private void ShowNativeHoverTip(RelicModel relic, Control sourceControl)
    {
        if (_currentTipInstance != null) return;

        try
        {
            var tipScene = PreloadManager.Cache.GetScene("res://scenes/ui/hover_tip.tscn");
            if (tipScene == null) return;

            _currentTipInstance = tipScene.Instantiate<Control>(PackedScene.GenEditState.Disabled);
            _rootNode.AddChild(_currentTipInstance);

            var titleLabel = _currentTipInstance.GetNode<MegaLabel>("%Title");
            var descLabel = _currentTipInstance.GetNode<MegaRichTextLabel>("%Description");
            var iconRect = _currentTipInstance.GetNode<TextureRect>("%Icon");

            titleLabel.SetTextAutoSize(relic.HoverTip.Title);

            string description = relic.DynamicDescription != null
                ? LocStringHelper.ToFormattedString(relic.DynamicDescription)
                : relic.HoverTip.Description;
            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descLabel.Text = description ?? "";

            if (relic.Icon != null)
            {
                iconRect.Texture = relic.Icon;
                iconRect.Visible = true;
            }
            else
                iconRect.Visible = false;

            _currentTipInstance.ResetSize();

            Vector2 relicPos = sourceControl.GlobalPosition;
            Vector2 relicSize = sourceControl.Size;
            float startX = relicPos.X + (relicSize.X / 2f) - (_currentTipInstance.Size.X / 2f);
            float startY = relicPos.Y + relicSize.Y + 10f;

            var tree = (SceneTree)Engine.GetMainLoop();
            if (startY + _currentTipInstance.Size.Y > tree.Root.Size.Y)
                startY = relicPos.Y - _currentTipInstance.Size.Y - 10f;

            _currentTipInstance.GlobalPosition = new Vector2(startX, startY);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] Show relic tip failed: {ex}");
        }
    }

    private void HideNativeHoverTip()
    {
        if (_currentTipInstance != null && GodotObject.IsInstanceValid(_currentTipInstance))
        {
            _currentTipInstance.QueueFree();
            _currentTipInstance = null;
        }
    }

    private void UpdateConfirmButtonState()
    {
        int count = _selectedRelics.Count;
        _confirmButton.Disabled = count < _prefs.MinSelect || count > _prefs.MaxSelect;
    }

    private void OnConfirmButtonPressed() => CompleteSelection();

    private void CompleteSelection()
    {
        if (_tcs.Task.IsCompleted) return;
        _tcs.TrySetResult(_selectedRelics.ToList());
        if (GodotObject.IsInstanceValid(_rootNode))
            _rootNode.QueueFree();
    }
}
