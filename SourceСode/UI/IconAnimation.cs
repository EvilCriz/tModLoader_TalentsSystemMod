using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using Terraria;
using Terraria.DataStructures;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.UI;

namespace TalentsSystem;

public class IconAnimationInfo
{
    private bool _initialized = false;
    public Asset<Texture2D> Icon { get; protected set; }
    public Rectangle[] Frames { get; protected set; }
    public int FramesCount { get; protected set; }
    public int FrameHeight { get; protected set; }
    public int TicksPerFrame { get; protected set; }
    public int Delay;
    public Color Color = Color.White;

    public bool UseParentDimensions;

    protected IconAnimationInfo(Asset<Texture2D> icon, int framesCount, int ticksPerFrame)
    {
        Icon = icon;
        FramesCount = framesCount;
        TicksPerFrame = ticksPerFrame;
    }

    public virtual void Initialization()
    {
        if (_initialized)
            return;
        _initialized = true;
        if (!Icon.IsLoaded)
            Icon.Wait();
        Texture2D texture = Icon.Value;
        Frames = new Rectangle[FramesCount];
        FrameHeight = texture.Height / FramesCount;
        for (int i = 0; i < FramesCount; i++)
            Frames[i] = new Rectangle(0, i * FrameHeight, texture.Width, FrameHeight);
    }

    public static IconAnimationInfo FromItemID(int itemId)
        => NotServerCreate(() =>
        {
            Main.instance.LoadItem(itemId);
            DrawAnimation animation = Main.itemAnimations[itemId];
            return new(
                TextureAssets.Item[itemId],
                animation?.FrameCount ?? 1,
                animation?.TicksPerFrame ?? 0);
        });

    public static IconAnimationInfo FromVanilaModifiered(
        AssetsManager.VanilaModifiedTexture type,
        int framesCount = 1,
        int ticksPerFrame = 0)
            => NotServerCreate(() => new(
                AssetsManager.Instance.GetAssetVanilaModified(type),
                framesCount,
                ticksPerFrame));

    public static IconAnimationInfo FromTalents(string name, int framesCount = 1, int ticksPerFrame = 0, int delay = 0)
        => NotServerCreate(() => new(
            AssetsManager.Instance.GetOrLoadAssetTalents(name),
            framesCount,
            ticksPerFrame) { Delay = delay });

    protected static IconAnimationInfo NotServerCreate(Func<IconAnimationInfo> creator)
    {
        if (Main.netMode == NetmodeID.Server)
            return null;
        return creator();
    }
}

public class UIIconAnimation : UIElement
{
    public readonly IconAnimationInfo IconInfo;
    protected int CurrentFrame;
    protected int CurrentDelay;
    protected int AnimationTimer;

    public UIIconAnimation(IconAnimationInfo info)
    {
        IconInfo = info;
        IconInfo.Initialization();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (--CurrentDelay > 0)
            return;
        if (++AnimationTimer < IconInfo.TicksPerFrame)
            return;
        AnimationTimer = 0;
        if (++CurrentFrame < IconInfo.FramesCount)
            return;
        CurrentFrame = 0;
        CurrentDelay = IconInfo.Delay;
    }

    protected override void DrawSelf(SpriteBatch spriteBatch)
    {
        if (Parent is not null && IconInfo.UseParentDimensions)
        {
            DrawSelfByParentDimension(spriteBatch);
        }
        else
        {
            DrawSelfBySelfDimension(spriteBatch);
        }
    }

    protected void DrawSelfBySelfDimension(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = Parent.GetInnerDimensions();
        ref Rectangle frame = ref IconInfo.Frames[CurrentFrame];
        Vector2 position = dimensions.Center() - frame.Size() / 2f;

        spriteBatch.Draw(
            IconInfo.Icon.Value,
            position,
            frame,
            IconInfo.Color);
    }

    protected void DrawSelfByParentDimension(SpriteBatch spriteBatch)
    {
        CalculatedStyle dimensions = Parent.GetInnerDimensions();
        ref Rectangle frame = ref IconInfo.Frames[CurrentFrame];

        Vector2 availableSize = new(dimensions.Width, dimensions.Height);
        Vector2 iconSize = frame.Size();

        float scale = Math.Min(
            availableSize.X / iconSize.X,
            availableSize.Y / iconSize.Y);

        Vector2 size = iconSize * scale;
        Vector2 position = dimensions.Center() - size / 2f;

        spriteBatch.Draw(
            IconInfo.Icon.Value,
            position,
            frame,
            IconInfo.Color,
            0f,
            Vector2.Zero,
            scale,
            SpriteEffects.None,
            0f);
    }
}