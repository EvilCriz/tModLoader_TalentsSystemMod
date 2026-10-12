using Ionic.Zlib;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using Terraria;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ModLoader;
using Terraria.UI;
using UIeXtension;
using UIeXtension.Enums;
using UIeXtension.MethodsExtensions;
using static System.Net.Mime.MediaTypeNames;
using LT = Terraria.Localization.LocalizedText;
using TS = TalentsSystem.TalentsSystem;
using UL = TalentsSystem.Utils.UtilsLocalization;

namespace TalentsSystem.Utils;


// This document contains minimal commentary.
// It also avoids repeating explanations already provided in the comments regarding the main menu.
// It is recommended to begin exploring this mod's UI with the UI/Menu.cs file.


/// <summary>
///     Used to automatically scale text to fit the size of the parent element.
///     Automatic scaling occurs in the method: <see cref="UtilsUI.RefreshTextScaleForBranch(UIElement)"/>
/// </summary>
public class UITextAutoScale : UIText
{
    /// <summary>
    ///     A flag indicating whether automatic text scaling to fit the parent's size is required.
    /// </summary>
    public bool AutoScale = true;

    /// <summary>
    ///     Does nothing if <see cref="AutoScale"/> == true.
    /// </summary>
    public float Scale;
    public UITextAutoScale(string text, float scale) : base(text, scale) { Scale = scale; }
}



// This document contains minimal commentary.
// It also avoids repeating explanations already provided in the comments regarding the main menu.
// It is recommended to begin exploring this mod's UI with the UI/Menu.cs file.


public static class UtilsUI
{
    public static UIImage WrapInUIImage(UIElement element, Texture2D backIcon, bool stretch = false, bool center = false)
        => (UIImage)WrapInAuto(element, backIcon, stretch, center);
    public static UIButtonFrame WrapInButtonFrame(UIElement element, bool stretch = false, bool center = false)
        => (UIButtonFrame)WrapInAuto(element, null, stretch, center);

    public static UIElement WrapInAuto(UIElement element, Texture2D backIcon, bool stretch = false, bool center = false)
    {
        UIElement frame;
        if (backIcon is null)
            frame = new UIButtonFrame();
        else
        {
            var backgroundIcon = new UIImage(backIcon);
            backgroundIcon.ScaleToFit = true;
            frame = backgroundIcon;
            frame.SetPadding(10f);
        }
        frame.Append(element);
        element.IgnoresMouseInteraction = true;
        //////////////////////
        if (stretch)
        {
            element.Width = StyleDimension.Fill;
            element.Height = StyleDimension.Fill;
        }
        //////////////////////
        if (center)
        {
            element.HAlign = 0.5f;
            element.VAlign = 0.5f;
        }
        return frame;
    }

    /// <summary>
    ///     Creation of the task panel. 
    ///     Moved to UtilsUI, as the task panel needs to be created in both the main menu and pop-up notifications.
    /// </summary>
    public static UIExStackPanel GetQuestInfoStackPanel(Quest quest, bool showSync, bool tModLoaderStyle = true)
    {
        UIExStackPanel questStackPanel = new UIExStackPanel();
        questStackPanel.StyleDisplay.tModLoaderStyle = tModLoaderStyle;
        questStackPanel.StyleLayout.StackPanel().Spacing.Precent = 0.01f;
        questStackPanel.StyleLayout.Set(UIExOrientation.Horizontal, UIExAlignment.Start, UIExAlignment.Center);
        questStackPanel.Width.Precent = 1f;
        questStackPanel.Height.Pixels = 128f;
        //////////////////////
        bool completed = TS.GetLocal<QuestSaveInfo>().CompletedQuests.Contains(quest.ID);
        AssetsManager.UITextureID backIconType = completed
            ? AssetsManager.UITextureID.TalentFrameMax
            : AssetsManager.UITextureID.TalentFrameCurrent;
        Texture2D backIcon = AssetsManager.Instance.GetAssetUI(backIconType).Value;
        var wrapIcon = CreateFrameIcon(quest.Icon.Value, backIcon, iconSecond: completed);
        wrapIcon.StyleLayoutChild().Margin.Left.Precent = 0.01f;
        questStackPanel.Append(wrapIcon);
        //////////////////////
        string description = quest.GetDescription().Value;
        var wrapDescriptionText = CreateWrapText<UIExElement>(description);
        if (wrapDescriptionText.Data is UITextAutoScale descriptionTextScale)
            descriptionTextScale.Scale = 0.6f;
        questStackPanel.Append(wrapDescriptionText);
        //////////////////////
        wrapDescriptionText.StyleDisplay.tModLoaderStyle = true;
        var style = wrapDescriptionText.StyleLayoutChild();
        style.JustifySelf = UIExAlignment.Stretch;
        style.AlignSelf = UIExAlignment.Stretch;
        style.Margin.Top.Precent = 0.2f;
        style.Margin.Bottom.Precent = 0.2f;
        //////////////////////
        if (showSync)
        {
            var wrapSyncText = CreateWrapText<UIExElement>(quest.Sync ? "⟳" : "⟲");
            wrapSyncText.StyleDisplay.tModLoaderStyle = true;
            style = wrapSyncText.StyleLayoutChild();
            style.Width.Pixels = 64f;
            style.Height.Pixels = 64f;
            questStackPanel.Append(wrapSyncText);
        }
        //////////////////////
        int questExp = quest.Exp;
        var wrapExpText = CreateWrapText<UIExElement>(UL.UI_Quests_Exp.WithFormatArgs(questExp));
        wrapExpText.StyleDisplay.tModLoaderStyle = true;
        wrapExpText.SetPadding(5f);
        style = wrapExpText.StyleLayoutChild();
        style.Width.Pixels = 160f;
        style.Height.Pixels = 64f;
        questStackPanel.Append(wrapExpText);
        ///////////////////////
        return questStackPanel;
    }

    public static UIElement CreateFrameIconByItemID(int iconItemId, Texture2D backgroundIcon, bool iconTopLevel)
    {
        if (Main.instance is null)
            return null;
        ///////////////////////
        Main.instance.LoadItem(iconItemId);
        Texture2D icon = TextureAssets.Item[iconItemId].Value;
        ///////////////////////
        return CreateFrameIcon(icon, backgroundIcon, iconTopLevel);
    }
    public static UIElement CreateFrameIcon(Texture2D icon, Texture2D backgroundIcon, bool iconSecond)
    {
        UIElement frame;
        float size = 64f;
        var uiIcon = new UIImage(icon);
        uiIcon.ScaleToFit = true;

        frame = WrapInAuto(uiIcon, backgroundIcon, stretch: false, center: true);

        var style = frame.StyleLayoutChild();
        style.Width.Pixels = size;
        style.Height.Pixels = size;
        return frame;
    }

    public static UISep AddSeparator(UIExGrid grid, UIExOrientation orientation, int row, int column, int rowSpan = -1, int columnSpan = -1)
    {
        UISep separator = new();
        grid.Append(separator);
        separator.StyleDisplay.tModLoaderStyle = true;
        ////////////////////
        bool vertical = orientation == UIExOrientation.Vertical;
        if (rowSpan == -1)
            rowSpan = vertical ? int.MaxValue : 1;
        if (columnSpan == -1)
            columnSpan = vertical ? 1 : int.MaxValue;
        separator.StyleLayoutChild().Grid().Set(row, column, rowSpan, columnSpan);
        ////////////////////
        return separator;
    }
    public static UISep AddSeparator(UIExStackPanel stackPanel)
    {
        UISep separator = new();
        stackPanel.Append(separator);
        ////////////////////
        var styleDisplay = separator.StyleDisplay;
        styleDisplay.tModLoaderStyle = true;
        var styleTml = styleDisplay.TmlUIPanel();
        ////////////////////
        bool vertical = stackPanel.StyleLayout.Orientation == UIExOrientation.Vertical;
        var style = separator.StyleLayoutChild();
        if (vertical)
        {
            style.Height.Pixels = 5f;
            style.Width.Precent = 1f;
        }
        else
        {
            style.Width.Pixels = 25f;
            style.Height.Precent = 1f;
        }
        ////////////////////
        return separator;
    }
    public static UISep AddSeparator(UIList list, float marginTop = 20f, float marginBottom = 20f)
    {
        UISep separator = new();
        list.Add(separator);
        ////////////////////
        separator.MarginTop = marginTop;
        separator.MarginBottom = marginBottom;
        separator.StyleDisplay.tModLoaderStyle = true;
        separator.Height.Pixels = 5f;
        separator.Width.Precent = 1f;
        ////////////////////
        return separator;
    }

    public static UIButtonFrame CreateWrapTextInFrame(LT text, float scale = 1f)
        => CreateWrapTextInFrame(new(text.Value, scale) { AutoScale = true });
    public static UIButtonFrame CreateWrapTextInFrame(string text, float scale = 1f)
        => CreateWrapTextInFrame(new(text, scale) { AutoScale = true });
    public static UIButtonFrame CreateWrapTextInFrame(UITextAutoScale text)
    {
        PrepareTextsLayout(text);
        UIButtonFrame frame = new();
        frame.Data = text;
        frame.StyleDisplay.PaddingAutoControl = false;
        return UIExLayout.Wrap(text, frame);
    }
    public static TWrap CreateWrapText<TWrap>(UITextAutoScale text) where TWrap : UIExElement, new()
    {
        PrepareTextsLayout(text);
        var wrap = UIExLayout.Wrap<TWrap>(text);
        wrap.Data = text;
        wrap.StyleDisplay.PaddingAutoControl = false;
        return wrap;
    }
    public static TWrap CreateWrapText<TWrap>(LT text, float scale = 1f) 
        where TWrap : UIExElement, new()
            => CreateWrapText<TWrap>(text.Value, scale);
    public static TWrap CreateWrapText<TWrap>(string text, float scale = 1f) 
        where TWrap : UIExElement, new()
            => CreateWrapText<TWrap>(CreateUIText(text, scale));
    public static UITextAutoScale CreateUIText(LT text, float scale = 1f)
        => CreateUIText(text.Value, scale);
    public static UITextAutoScale CreateUIText(string text, float scale = 1f)
    {
        UITextAutoScale uiText = new(text, scale);
        uiText.AutoScale = true;
        PrepareTextsLayout(uiText);
        return uiText;
    }
    public static void SetAutoScale(this UIElement element, bool autoScale)
    {
        if (element is UITextAutoScale scale1)
            scale1.AutoScale = autoScale;
        else if(element is UIExElement el && el.Data is not null && el.Data is UITextAutoScale scale2)
            scale2.AutoScale = autoScale;
    }
    public static void SetWrappedTextAutoScale(this UIExElement wrap, bool autoScale)
    {
        if (wrap.Data is not null && wrap.Data is UITextAutoScale text)
            text.AutoScale = autoScale;
    }
    public static void PrepareTextsLayout(params UIText[] texts)
    {
        foreach (var text in texts)
        {
            text.Width.Set(0f, 1f);
            text.Height.Set(0f, 1f);
            text.TextOriginX = 0.5f;
            text.TextOriginY = 0.5f;
            text.DynamicallyScaleDownToWidth = true;
        }
    }

    /// <summary>
    ///  Signals the need to recalculate automatic text scaling for all <see cref="UITextAutoScale"/> elements(added by this mod) within the provided <see cref="UIState"/>.
    /// </summary>
    /// <param name="element"></param>
    public static void RefreshTextScaleForBranch(UIElement element)
    {
        if (element is UITextAutoScale uiText && uiText.AutoScale && element.Parent is not null)
        { 
            string text = uiText.Text;
            Vector2 size = FontAssets.MouseText.Value.MeasureString(text);
            CalculatedStyle parentSize = element.Parent.GetInnerDimensions();
            float scale = Math.Min(parentSize.Height / size.Y, parentSize.Width / size.X);
            uiText.SetText(text, (scale * uiText.Scale) * 0.75f, false);
        }
        foreach (var child in element.Children)
            RefreshTextScaleForBranch(child);
    }
}
