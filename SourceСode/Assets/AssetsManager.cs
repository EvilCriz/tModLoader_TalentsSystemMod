using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using Terraria.ModLoader;

namespace TalentsSystem;
public class AssetsManager
{
    public static AssetsManager Instance { get; set; } = null;
    protected AssetsManager() { }
    public enum UITextureID : byte
    {
        TalentFrameMax,
        TalentFrameCurrent,
        TalentFrameClose,
        LevelUpFrame,
        SectionTalentFrameMax,
        SectionTalentFrameCurrent,
        SectionTalentFrameClose
    }
    public enum VanilaFirtFrameTexture : byte
    {
        SoulofLight,
        SoulofNight,
        SoulofFlight
    }
    public enum VanilaModifiedTexture : byte
    {
        EoCShieldOrange,
    }
    public const string ModAssetsPath = $"{nameof(TalentsSystem)}/Assets";
    public const string ModAssetsUIPath = $"{ModAssetsPath}/UI";
    public const string ModAssetsVanilaPath = $"{ModAssetsPath}/Vanila";
    public const string ModAssetsTalentsPath = $"{ModAssetsPath}/Talents";
    protected Dictionary<UITextureID, Asset<Texture2D>> AssetsUI = null;
    /// <summary>
    ///     Vanilla Terraria textures—sprites cropped to the first frame.
    /// </summary>
    protected Dictionary<VanilaFirtFrameTexture, Asset<Texture2D>> AssetsVanilaFirstFrame = null;
    /// <summary>
    ///     Vanilla Terraria textures—with minor changes (relative to the effort involved)
    /// </summary>
    protected Dictionary<VanilaModifiedTexture, Asset<Texture2D>> AssetsVanilaModifier = null;
    protected Dictionary<string, Asset<Texture2D>> AssetsTalents = null;
    public static void Load()
    {
        Instance = new();
        Instance.AssetsUI = LoadAssetsTexture2D<UITextureID>(ModAssetsUIPath);
        Instance.AssetsVanilaFirstFrame = LoadAssetsTexture2D<VanilaFirtFrameTexture>(ModAssetsVanilaPath);
        Instance.AssetsVanilaModifier = LoadAssetsTexture2D<VanilaModifiedTexture>(ModAssetsVanilaPath);
        Instance.AssetsTalents = new();

        // The names of the textures loaded using this method match the names of their corresponding enum constants.
        Dictionary<T, Asset<Texture2D>> LoadAssetsTexture2D<T>(string path) where T : Enum
        {
            Dictionary<T, Asset<Texture2D>> assets = new();
            foreach (var field in typeof(T).GetFields())
            {
                if (!field.IsLiteral)
                    continue;
                T value = (T)field.GetRawConstantValue()!;
                assets[value] = ModContent.Request<Texture2D>($"{path}/{field.Name}");
            }
            return assets;
        }
    }
    public static void Unload()
    {
        if (Instance is null)
            return;
        Instance.AssetsUI = null;
        Instance.AssetsVanilaFirstFrame = null;
        Instance.AssetsVanilaModifier = null;
        Instance.AssetsTalents = null;
        Instance = null;
    }
    public Asset<Texture2D> GetAssetUI(UITextureID textureID) => AssetsUI[textureID];

    public Asset<Texture2D> GetAssetVanilaFirstFrame(VanilaFirtFrameTexture type)
        => AssetsVanilaFirstFrame[type];
    public Asset<Texture2D> GetAssetVanilaModified(VanilaModifiedTexture type)
        => AssetsVanilaModifier[type];

    public Asset<Texture2D> GetOrLoadAssetTalents(string name)
    {
        if (AssetsTalents.ContainsKey(name))
            return AssetsTalents[name];
        AssetsTalents[name] = ModContent.Request<Texture2D>($"{ModAssetsTalentsPath}/{name}");
        return AssetsTalents[name];
    }
}