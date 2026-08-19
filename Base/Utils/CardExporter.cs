using Godot;
using MegaCrit.Sts2.addons.mega_text;
using MegaCrit.Sts2.Core.Assets;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.Models;
using MegaCrit.Sts2.Core.Nodes;
using MegaCrit.Sts2.Core.Nodes.Cards;
using MegaCrit.Sts2.Core.Nodes.Rooms;

namespace UltraLib.Base.Utils;

public partial class CardExporter : Node
{
    public static async Task<Error> RenderCardToImage(
        CardModel cardModel,
        string savePath = "")
    {
        // 0. 路径处理（建议使用 ID 避免特殊字符导致保存失败）
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = $"user://{cardModel.Title}.png";
        }

        // 1. 获取父节点逻辑 (保持不变)
        Node? parentNode = NCombatRoom.Instance ?? (Node?)NGame.Instance;
        if (parentNode == null) return Error.Failed;

        // 2. 环境搭建
        Vector2I viewportSize = new Vector2I(512, 512);
        SubViewportContainer container = new SubViewportContainer();
        SubViewport subViewport = new SubViewport();
        subViewport.Size = viewportSize;
        subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        subViewport.TransparentBg = true;

        container.AddChild(subViewport);
        parentNode.AddChild(container);

        // 3. 实例化卡牌
        NCard? nCard = NCard.Create(cardModel);
        if (nCard == null)
        {
            container.QueueFree();
            return Error.Failed;
        }

        // *** 关键修改：先添加到 SubViewport，让它“活”过来 ***
        subViewport.AddChild(nCard);
        nCard.Position = (Vector2)viewportSize / 2f;
        
        var unplayableIcon = nCard.GetNodeOrNull<Control>("%UnplayableEnergyIcon");
        if (unplayableIcon != null)
        {
            unplayableIcon.QueueFree(); 
        }
        
        nCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);

        // 4. 【核心修复】强制同步与深度等待
        // 给异步加载和本地化注入留出充足时间
        await Task.Delay(150);

        // 至少等待 5-10 帧。StS2 的一些文本组件刷新频率较低
        for (int i = 0; i < 10; i++)
        {
            if (!GodotObject.IsInstanceValid(nCard)) break;
            await parentNode.ToSignal(parentNode.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        // 5. 抓取图像
        Image image = subViewport.GetTexture().GetImage();
        if (image == null || image.IsEmpty())
        {
            nCard.QueueFree();
            container.QueueFree();
            return Error.Failed;
        }

        // 6. 裁剪逻辑 (保持不变)
        Rect2I usedRect = image.GetUsedRect();
        if (usedRect.Area == 0)
        {
            GD.PrintErr($"警告: {cardModel.Id} 渲染出空白，请检查资源路径");
        }

        Rect2I marginRect = usedRect.Grow(10);
        Rect2I finalRect = marginRect.Intersection(new Rect2I(0, 0, image.GetWidth(), image.GetHeight()));
        Image croppedImage = image.GetRegion(finalRect);

        // 7. 保存与清理
        Error error = croppedImage.SavePng(savePath);
        if (error == Error.Ok)
        {
            GD.Print($"✓ [{cardModel.Id}] 保存成功");
        }

        nCard.QueueFree();
        container.QueueFree();

        return error;
    }

    /// <summary>
    /// 生成带有 Hover Tips 的卡牌图像（修正了之前版本中顶部裁剪过严导致的金边特效和文本 Shader 被切掉的问题）
    /// </summary>
    /// <param name="cardModel"></param>
    /// <param name="savePath"></param>
    /// <returns></returns>
    public static async Task<Error> RenderCardWithHoverTipsToImage(
        CardModel cardModel,
        string savePath = "")
    {
        if (string.IsNullOrEmpty(savePath))
        {
            savePath = $"user://{cardModel.Title}_WithTips.png";
        }

        Node? parentNode = NCombatRoom.Instance ?? (Node?)NGame.Instance;
        if (parentNode == null) return Error.Failed;

        Vector2I viewportSize = new Vector2I(2560, 1440);
        SubViewportContainer container = new SubViewportContainer();
        SubViewport subViewport = new SubViewport();
        subViewport.Size = viewportSize;
        subViewport.RenderTargetUpdateMode = SubViewport.UpdateMode.Always;
        subViewport.TransparentBg = true;

        container.AddChild(subViewport);
        parentNode.AddChild(container);

        HBoxContainer rootLayout = new HBoxContainer();
        rootLayout.Size = viewportSize;
        rootLayout.Alignment = BoxContainer.AlignmentMode.Center;
        rootLayout.AddThemeConstantOverride("separation", 35);
        subViewport.AddChild(rootLayout);

        NCard? nCard = NCard.Create(cardModel);
        if (nCard == null)
        {
            container.QueueFree();
            return Error.Failed;
        }

        Control cardWrapper = new Control();
        cardWrapper.CustomMinimumSize = new Vector2(270, 460);
        cardWrapper.AddChild(nCard);
        rootLayout.AddChild(cardWrapper);

        nCard.Position = new Vector2(
            cardWrapper.CustomMinimumSize.X / 2f,
            cardWrapper.CustomMinimumSize.Y / 2f
        );
        
        var unplayableIcon = nCard.GetNodeOrNull<Control>("%UnplayableEnergyIcon");
        if (unplayableIcon != null)
        {
            unplayableIcon.QueueFree(); 
        }
        
        nCard.UpdateVisuals(PileType.Hand, CardPreviewMode.Normal);

        HBoxContainer tipsGroupHBox = new HBoxContainer();
        tipsGroupHBox.Alignment = BoxContainer.AlignmentMode.Begin;
        tipsGroupHBox.AddThemeConstantOverride("separation", 7);//修改列表间距，避免过于分散

        VBoxContainer cardsColumn = new VBoxContainer();
        cardsColumn.Alignment = BoxContainer.AlignmentMode.Begin;
        cardsColumn.AddThemeConstantOverride("separation", 10);

        VBoxContainer textsColumn = new VBoxContainer();
        textsColumn.Alignment = BoxContainer.AlignmentMode.Begin;
        textsColumn.AddThemeConstantOverride("separation", 8);

        tipsGroupHBox.AddChild(cardsColumn);
        tipsGroupHBox.AddChild(textsColumn);

        Control tipsWrapperCase = new Control();
        tipsWrapperCase.CustomMinimumSize = new Vector2(750, 900);
        tipsWrapperCase.AddChild(tipsGroupHBox);
        rootLayout.AddChild(tipsWrapperCase);

        var hoverTips = cardModel.HoverTips;

        if (hoverTips != null && hoverTips.Count() > 0)
        {
            Control topSpacer = new Control();
            topSpacer.CustomMinimumSize = new Vector2(0, 18);
            textsColumn.AddChild(topSpacer);
        }

        if (hoverTips != null)
        {
            foreach (var tip in hoverTips)
            {
                if (tip == null) continue;

                // 分流：普通词条说明（右列）
                if (tip is MegaCrit.Sts2.Core.HoverTips.HoverTip hoverTip)
                {
                    Control tipControl = PreloadManager.Cache.GetScene("res://scenes/ui/hover_tip.tscn")
                        .Instantiate<Control>(PackedScene.GenEditState.Disabled);

                    textsColumn.AddChild(tipControl);

                    MegaLabel titleLabel = tipControl.GetNode<MegaLabel>("%Title");
                    if (hoverTip.Title == null)
                        titleLabel.Visible = false;
                    else
                        titleLabel.SetTextAutoSize(hoverTip.Title);

                    MegaRichTextLabel descLabel = tipControl.GetNode<MegaRichTextLabel>("%Description");
                    descLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
                    descLabel.Text = hoverTip.Description;

                    TextureRect iconRect = tipControl.GetNode<TextureRect>("%Icon");
                    if (hoverTip.Icon != null)
                    {
                        iconRect.Texture = hoverTip.Icon;
                        iconRect.Visible = true;
                    }
                    else
                        iconRect.Visible = false;

                    if (hoverTip.IsDebuff)
                    {
                        CanvasItem bgNode = tipControl.GetNode<CanvasItem>("%Bg");
                        bgNode.Material = PreloadManager.Cache.GetMaterial("res://materials/ui/hover_tip_debuff.tres");
                    }

                    tipControl.ResetSize();
                }
                // 分流：衍生卡（左列）
                else if (tip is MegaCrit.Sts2.Core.HoverTips.CardHoverTip cardHoverTip)
                {
                    Control cardTipControl = PreloadManager.Cache.GetScene("res://scenes/ui/card_hover_tip.tscn")
                        .Instantiate<Control>(PackedScene.GenEditState.Disabled);

                    Control cardBox = new Control();
                    // 采用你修改后的紧凑适配大小 (220, 320)
                    cardBox.CustomMinimumSize = new Vector2(220, 320);
                    cardBox.AddChild(cardTipControl);
                    cardsColumn.AddChild(cardBox);

                    NCard tipCardNode = cardTipControl.GetNode<NCard>("%Card");
                    tipCardNode.Model = cardHoverTip.Card;
                    tipCardNode.UpdateVisuals(PileType.Deck, CardPreviewMode.Normal);

                    cardTipControl.Size = cardBox.CustomMinimumSize;
                    cardTipControl.Position = Vector2.Zero;

                    tipCardNode.Position = new Vector2(
                        cardBox.CustomMinimumSize.X / 2f,
                        (cardBox.CustomMinimumSize.Y / 2f) + 20f
                    );
                }
            }
        }

        await Task.Delay(400);

        for (int i = 0; i < 35; i++)
        {
            if (!GodotObject.IsInstanceValid(nCard)) break;
            await parentNode.ToSignal(parentNode.GetTree(), SceneTree.SignalName.ProcessFrame);
        }

        Image image = subViewport.GetTexture().GetImage();
        if (image == null || image.IsEmpty())
        {
            container.QueueFree();
            return Error.Failed;
        }

        Rect2I usedRect = image.GetUsedRect();
        if (usedRect.Area == 0)
        {
            GD.PrintErr($"警告: {cardModel.Id} 渲染出空白");
        }

        Rect2I marginRect = usedRect.Grow(90);
        Rect2I finalRect = marginRect.Intersection(new Rect2I(0, 0, image.GetWidth(), image.GetHeight()));
        Image croppedImage = image.GetRegion(finalRect);

        Error error = croppedImage.SavePng(savePath);
        if (error == Error.Ok)
        {
            GD.Print($"✓ [{cardModel.Id}] 顶端向上延展对齐修正完毕，图像已安全保存: {savePath}");
        }

        container.QueueFree();
        return error;
    }
}