using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TalentsSystem.Utils;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using UIeXtension;
using UIeXtension.Styles.TModLoader;

namespace TalentsSystem;

/// <summary>
/// Is absolutely identical to UIExElement. It is used as a marker for applying theme styles.
/// </summary>
public class UISep : UIExElement { }

/// <inheritdoc cref="UISep"/>
public class UIButtonFrame : UIExElement { }

/// <inheritdoc cref="UISep"/>
public class UIConfirmationGrid : UIExGrid { }

/// <inheritdoc cref="UISep"/>
public class UIIconAnimationFrame : UIExElement { }

public class ThemeStyles
{
    public enum UIThemeType : byte
    {
        Terraria,
        Blue,
        Dark,
        Brown,
        Phantom
    }
    protected UIThemeType ThemeType = UIThemeType.Terraria;

    public static ThemeStyles Instance { get; set; } = null;
    public static void Load()
    {
        Instance = new();
        Instance.StylesTable = new();
        Instance.StyleTextTable = new();
        Instance.TerrariaElementWithDefaultStyle = new();
    }
    public static void Unload()
    {
        if (Instance is null)
            return;
        Instance.StylesTable = null;
        Instance.StyleTextTable = null;
        Instance.TerrariaElementWithDefaultStyle = null;
        Instance.StyleDefault = null;
    }
    protected ThemeStyles() { }

    protected Dictionary<Type, StyleTml> StylesTable = null;
    protected Dictionary<Type, StyleText> StyleTextTable = null;
    protected List<Type> TerrariaElementWithDefaultStyle = null;
    protected StyleTml StyleDefault = null;

    public virtual void SwitchTheme(UIThemeType type)
    {
        ThemeType = type;
        StylesTable.Clear();
        StyleTextTable.Clear();
        TerrariaElementWithDefaultStyle.Clear();
        StyleDefault = null;
        ////////////////////
        StyleTml styleButtonFrame = null;
        StyleText styleDefaultText = null;
        ////////////////////
        switch (ThemeType)
        {
            case UIThemeType.Brown:
                StyleDefault = new(new(121, 68, 59), Color.Black, 1f);
                styleButtonFrame = new(StyleDefault.BackgroundColor, Color.DarkGray, 1.5f);

                styleDefaultText = new(new(255, 80, 0));
                break;
            case UIThemeType.Phantom:
                StyleDefault = new(Color.DarkCyan, Color.Aqua, 2f);
                styleButtonFrame = new(StyleDefault, 2f);

                styleDefaultText = new(Color.AliceBlue);
                break;
            case UIThemeType.Dark:
                StyleDefault = new(new(30, 30, 30), Color.Black, 1f);
                styleButtonFrame = new(new(50, 50, 50), Color.Black, 5f);
                break;
            case UIThemeType.Blue:
                StyleDefault = new(Color.DarkSlateBlue, Color.LightSkyBlue, 23f);
                styleButtonFrame = new(Color.DarkSlateBlue, Color.LightSkyBlue, 0.5f);
                break;
        }
        if (StyleDefault is null)
            StyleDefault = new();
        if (styleDefaultText is null)
            styleDefaultText = new(Color.White, 1f);
        //////////////////////////
        StyleTml styleSeparator = new(StyleDefault)
        {
            CornerSize = 4,
            BarSize = 0,
        };
        StyleTml styleConfirmation = new(StyleDefault);
        styleConfirmation.ColorShiftMultiplier = 0.8f;
        styleConfirmation.BackgroundColor.A = 255;
        ///////////////////////////
        if (ThemeType == UIThemeType.Terraria)
        {
            Register<UISep>(styleSeparator, 1f);
            Register<UIConfirmationGrid>(styleConfirmation, 1f);
        }
        else
        {
            Register<UIExElement>(StyleDefault, 0.5f);
            Register<UISep>(styleSeparator, 0.75f, 0.9f);
            Register<UIButtonFrame>(styleButtonFrame, 0.75f);
            Register<UIConfirmationGrid>(styleConfirmation, 5f);
        }
        ///////////////////////////
        Register<UIText>(styleDefaultText);
        Register<UITextAutoScale>(styleDefaultText);
        //Register<UIIconAnimationFrame>(styleConfirmation, 0.7f);
    }

    protected void RegisterTerraria<T>() where T : UIElement
    {
        if (TerrariaElementWithDefaultStyle.Contains(typeof(T)))
            return;
        TerrariaElementWithDefaultStyle.Add(typeof(T));
    }
    protected void Register<T>(StyleText style) where T : UIText
    {
        if(style is not null)
            StyleTextTable[typeof(T)] = style;
    }
    protected void Register<T>(StyleTml style, float alphaMul) 
        where T : UIExElement
        => Register<T>(style, alphaMul, alphaMul);
    protected void Register<T>(StyleTml style, float backgroundAlphaMul, float borderAlphaMul)
        where T : UIExElement
    {
        if (style is null)
            style = new(StyleDefault, 1f);
        /////////////////////////////////////
        style.ApplyShiftColorMultiplier();
        style.ApplyAlphaColorMultiplier(backgroundAlphaMul, borderAlphaMul);
        //////////////////////////////////////
        StylesTable[typeof(T)] = style;
    }

    public void ApplyStylesToBranch(UIElement element)
    {
        if (StyleDefault is null)
            return;

        if (element is UIExElement el && el.StyleDisplay.tModLoaderStyle)
        {
            var style = el.StyleDisplay.TmlUIPanel();
            if (StylesTable.TryGetValue(el.GetType(), out var newStyle))
                style.Copy(newStyle);
            else
                style.Copy(StyleDefault);
        }
        else if (element is UIText text)
        {
            if (StyleTextTable.TryGetValue(text.GetType(), out var newTextStyle))
                newTextStyle.ApplyColor(text);
        }

        foreach (var child in element.Children)
            ApplyStylesToBranch(child);
    }

    public class StyleTml : StyleTmlUIPanel
    {
        public float ColorAlphaMul = 1f;
        public float ColorShiftMultiplier = 1f;

        public StyleTml() { }
        public StyleTml(float colorAlphaMul)
        {
            ColorAlphaMul = colorAlphaMul;
        }
        public StyleTml(StyleTml style) : this(style, 1f) { }
        public StyleTml(Color background, Color border, float colorAlphaMul)
            : this(colorAlphaMul)
        {
            BackgroundColor = background;
            BorderColor = border;
        }
        public StyleTml(StyleTml style, float colorAlphaMul)
            : this(style.BackgroundColor, style.BorderColor, colorAlphaMul) { }

        public void ApplyAlphaColorMultiplier(float backgroundAlphaMul, float borderAlphaMul)
        {
            BackgroundColor = GetColorAlphaMul(BackgroundColor, backgroundAlphaMul);
            BackgroundColor = GetColorAlphaMul(BackgroundColor, ColorAlphaMul);
            BorderColor = GetColorAlphaMul(BorderColor, borderAlphaMul);
            BorderColor = GetColorAlphaMul(BorderColor, ColorAlphaMul);
        }
        public void ApplyShiftColorMultiplier()
        {
            BackgroundColor = GetShiftedСolor(BackgroundColor);
            BorderColor = GetShiftedСolor(BorderColor);
            Color GetShiftedСolor(Color c)
                => new(
                    (byte)Math.Clamp(c.R * ColorShiftMultiplier, 0, 255),
                    (byte)Math.Clamp(c.G * ColorShiftMultiplier, 0, 255),
                    (byte)Math.Clamp(c.B * ColorShiftMultiplier, 0, 255),
                    c.A);
        }
    }

    public class StyleText
    {
        public Color TextColor = Color.White;
        public float AlphaMul = 1;
        public StyleText(Color textColor, float alphaMul)
        {
            TextColor = textColor;
            AlphaMul = alphaMul;
        }
        public StyleText(Color textColor) : this(textColor, 1f) { }
        public void ApplyColor(UIText text)
            => text.TextColor = GetColorAlphaMul(TextColor, AlphaMul);
    }

    internal static Color GetColorAlphaMul(Color c, float mul)
    {
        c.A = (byte)Math.Clamp(c.A * mul, 0, 255);
        return c;
    }
}