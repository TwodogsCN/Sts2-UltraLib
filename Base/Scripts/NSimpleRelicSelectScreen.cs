using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Players;
using MegaCrit.Sts2.Core.Models;
using UltraLib.Base.Utils;

namespace UltraLib.Base.Scripts;

/// <summary>
/// Configuration parameters for the relic selector.
/// </summary>
/// <param name="HeaderText">选择界面标题。</param>
/// <param name="MinSelect">最少可选数量。</param>
/// <param name="MaxSelect">最多可选数量。</param>
/// <param name="RequireManualConfirmation">是否需要手动确认。</param>
public record RelicSelectorPrefs(string HeaderText, int MinSelect = 1, int MaxSelect = 1, bool RequireManualConfirmation = true);

/// <summary>
/// Controller for the simple relic-selection screen.
/// </summary>
/// <remarks>
/// 简单的遗物选择界面控制器。
/// <para>
/// 弹出一个全屏遮罩窗口，以网格形式展示遗物供玩家选择。
/// 节点完全用代码构建（不依赖游戏内场景），支持多选/单选替换/悬浮提示。
/// </para>
/// </remarks>
public class NSimpleRelicSelectScreen
{
    private const string ScenePath = "res://Base/Scenes/screens/simple_relic_select_screen.tscn";

    private Node _rootNode = null!;
    private GridContainer _gridContainer = null!;
    private RichTextLabel _headerLabel = null!;
    private Button _confirmButton = null!;

    private IReadOnlyList<RelicModel> _relics = null!;
    private RelicSelectorPrefs _prefs = null!;
    private Player _playerContext = null!;

    private readonly List<RelicModel> _selectedRelics = new();
    private readonly Dictionary<RelicModel, Control> _relicHolders = new();
    private readonly Dictionary<RelicModel, Panel> _relicBgPanels = new();

    private readonly TaskCompletionSource<IEnumerable<RelicModel>> _tcs = new();
    private Control? _currentTipInstance;

    /// <summary>
    /// Creates the relic-selection screen and returns the player's selection.
    /// </summary>
    /// <remarks>
    /// 创建遗物选择界面并返回玩家选择结果。
    /// </remarks>
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
            GD.PrintErr($"[UltraLib] 遗物选择界面初始化失败: {ex} / relic selection screen init failed: {ex}");
            _tcs.TrySetException(ex);
        }
    }

    private void PopulateRelicGrid()
    {
        foreach (Node child in _gridContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var relic in _relics)
        {
            var holderNode = CreateRelicHolderNode(relic);
            if (holderNode == null) continue;

            _gridContainer.AddChild(holderNode);

            _relicHolders[relic] = holderNode;
            var bgPanel = holderNode.GetNode<Panel>("BgPanel");
            if (bgPanel != null) _relicBgPanels[relic] = bgPanel;

            if (holderNode is Control controlHolder)
            {
                controlHolder.GuiInput += (InputEvent @event) =>
                {
                    if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                    {
                        HandleRelicSelection(relic);
                    }
                };
            }
        }
    }

    private Control? CreateRelicHolderNode(RelicModel relic)
    {
        var holderContainer = new Control
        {
            Name = "RelicHolder",
            CustomMinimumSize = new Vector2(120, 120),
            MouseFilter = Control.MouseFilterEnum.Stop
        };

        var bgPanel = new Panel
        {
            Name = "BgPanel",
            Size = new Vector2(110, 110),
            Position = new Vector2(5, 5)
        };

        var defaultStyle = new StyleBoxFlat
        {
            BgColor = new Color(0, 0, 0, 0.3f),
            BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3,
            BorderColor = Colors.Transparent,
            CornerRadiusTopLeft = 8, CornerRadiusTopRight = 8,
            CornerRadiusBottomLeft = 8, CornerRadiusBottomRight = 8
        };
        bgPanel.AddThemeStyleboxOverride("panel", defaultStyle);
        holderContainer.AddChild(bgPanel);

        var textureRect = new TextureRect
        {
            Name = "IconTexture",
            Size = new Vector2(100, 100),
            Position = new Vector2(10, 10),
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered
        };

        Texture2D? finalTexture = null;
        if (relic != null && relic.Icon != null) finalTexture = relic.Icon;
        else if (relic != null && !string.IsNullOrEmpty(relic.IconPath) && ResourceLoader.Exists(relic.IconPath))
            finalTexture = GD.Load<Texture2D>(relic.IconPath);

        if (finalTexture != null)
        {
            textureRect.Texture = finalTexture;
            holderContainer.AddChild(textureRect);
        }
        else
        {
            var label = new Godot.Label
            {
                Text = relic?.Id?.Entry?.Substring(0, Math.Min(4, relic.Id.Entry.Length)).ToUpper() ?? "RELC",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Size = new Vector2(100, 100)
            };
            label.AddThemeColorOverride("font_color", Colors.Gold);
            holderContainer.AddChild(label);
        }

        // 绑定悬浮事件
        if (relic != null)
        {
            holderContainer.MouseEntered += () =>
            {
                ShowNativeHoverTip(relic, holderContainer);
                ApplyHoverEffect(holderContainer, true);
            };

            holderContainer.MouseExited += () =>
            {
                HideNativeHoverTip();
                ApplyHoverEffect(holderContainer, false);
            };
        }

        return holderContainer;
    }

    /// <summary>
    /// 统一的选中/取消选中处理逻辑。
    /// </summary>
    private void HandleRelicSelection(RelicModel relic)
    {
        if (_selectedRelics.Contains(relic))
        {
            // 取消选中
            _selectedRelics.Remove(relic);
            UpdateRelicVisualState(relic, isSelected: false);
        }
        else
        {
            // 尝试选中
            if (_selectedRelics.Count < _prefs.MaxSelect)
            {
                _selectedRelics.Add(relic);
                UpdateRelicVisualState(relic, isSelected: true);
            }
            else if (_prefs.MaxSelect == 1 && _selectedRelics.Count == 1)
            {
                // 单选替换：关掉旧的，开启新的
                var oldRelic = _selectedRelics[0];
                _selectedRelics.Clear();
                UpdateRelicVisualState(oldRelic, isSelected: false);

                _selectedRelics.Add(relic);
                UpdateRelicVisualState(relic, isSelected: true);
            }
        }

        UpdateConfirmButtonState();

        // 如果不需要手动确认且满足条件，自动完成
        if (!_prefs.RequireManualConfirmation && _selectedRelics.Count >= _prefs.MinSelect)
        {
            CompleteSelection();
        }
    }

    private void UpdateRelicVisualState(RelicModel relic, bool isSelected)
    {
        // 1. 处理边框
        if (_relicBgPanels.TryGetValue(relic, out var panel))
        {
            SetSelectionBorder(panel, isSelected);
        }

        // 2. 处理缩放（选中时也保持放大效果）
        if (_relicHolders.TryGetValue(relic, out var control))
        {
            var targetScale = isSelected ? new Vector2(1.15f, 1.15f) : new Vector2(1.0f, 1.0f);
            var tween = control.CreateTween();
            tween.TweenProperty(control, "scale", targetScale, 0.15).SetEase(Tween.EaseType.Out);
        }
    }

    private void ApplyHoverEffect(Control control, bool isHovering)
    {
        // 如果已经选中，悬浮时不再重复放大
        if (_selectedRelics.Any(r => _relicHolders[r] == control)) return;

        var targetScale = isHovering ? new Vector2(1.15f, 1.15f) : new Vector2(1.0f, 1.0f);
        var tween = control.CreateTween();
        tween.TweenProperty(control, "scale", targetScale, 0.15).SetEase(Tween.EaseType.Out);
    }

    private void SetSelectionBorder(Panel panel, bool isSelected)
    {
        var style = panel.GetThemeStylebox("panel").Duplicate() as StyleBoxFlat;
        if (style == null) return;

        if (isSelected)
        {
            style.BorderColor = new Color(0.3f, 0.76f, 1f, 1f); // 选中蓝
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

            // 填充数据
            var titleLabel = _currentTipInstance.GetNode<MegaLabel>("%Title");
            var descLabel = _currentTipInstance.GetNode<MegaRichTextLabel>("%Description");
            var iconRect = _currentTipInstance.GetNode<TextureRect>("%Icon");

            if (!string.IsNullOrEmpty(relic.HoverTip.Title))
            {
                titleLabel.SetTextAutoSize(relic.HoverTip.Title);
                titleLabel.Visible = true;
            }
            else
            {
                titleLabel.Visible = false;
            }

            string description = relic.DynamicDescription != null
                ? LocStringHelper.ToFormattedString(relic.DynamicDescription)
                : relic.HoverTip.Description ?? "";

            descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            descLabel.Text = description;

            if (relic.Icon != null)
            {
                iconRect.Texture = relic.Icon;
                iconRect.Visible = true;
            }
            else
            {
                iconRect.Visible = false;
            }

            // 先刷新尺寸，再计算固定位置
            _currentTipInstance.ResetSize();

            // 获取遗物容器的全局位置和大小
            Vector2 relicGlobalPos = sourceControl.GlobalPosition;
            Vector2 relicSize = sourceControl.Size;

            // 提示框起始位置：遗物底部 + 10 像素间距，水平居中对齐
            float startX = relicGlobalPos.X + (relicSize.X / 2f) - (_currentTipInstance.Size.X / 2f);
            float startY = relicGlobalPos.Y + relicSize.Y + 10f;

            // 边界检查：超出屏幕底部则显示在遗物上方
            var tree = (SceneTree)Engine.GetMainLoop();
            if (startY + _currentTipInstance.Size.Y > tree.Root.Size.Y)
            {
                startY = relicGlobalPos.Y - _currentTipInstance.Size.Y - 10f;
            }

            _currentTipInstance.GlobalPosition = new Vector2(startX, startY);
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[UltraLib] 显示原生遗物提示框失败: {ex} / failed to show native relic hover tip: {ex}");
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
        // 确保只返回一次结果
        if (_tcs.Task.IsCompleted) return;

        _tcs.TrySetResult(_selectedRelics.ToList());
        if (GodotObject.IsInstanceValid(_rootNode))
        {
            _rootNode.QueueFree();
        }
    }
}
