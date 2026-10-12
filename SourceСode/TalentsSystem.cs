using System.IO;
using Terraria;
using Terraria.GameContent.Events;
using Terraria.ID;
using Terraria.ModLoader;

namespace TalentsSystem;
public class TalentsSystem : Mod
{
    public static TalentsSystem Instance = null;

    public enum PacketPrimarySourceType : byte
    { 
        Client,
        Server
    }
    /// <summary>
    ///     An instance of a class for exchanging network messages
    /// </summary>
    protected Utils.UtilsModPacketPrimarySourceClient UtilsModPacketPrimarySourceClient = null;
    protected Utils.UtilsModPacketPrimarySourceServer UtilsModPacketPrimarySourceServer = null;
    public static bool CalamityLoaded { get; protected set; } = false;

    public override void Load()
    {
        base.Load();
        ModDebug.LoadOrReload();
        Instance = this;
        ////////////////////////
        CalamityLoaded = ModLoader.HasMod("CalamityMod");
        ////////////////////////
        if (!Main.dedServ)
        {
            // Loading asset, quest, and UI style tables
            // The server is unaware of these tables
            // For quests (during quest execution), the server only transmits the quest ID
            AssetsManager.Load();
            QuestsManager.Load();
            ThemeStyles.Load();
        }
        ////////////////////////

        // Loading talent tables. All network parties are aware of them.
        TalentsManager.Load();
        UtilsModPacketPrimarySourceClient = new();
        UtilsModPacketPrimarySourceServer = new();
        ////////////////////////
        On_DD2Event.WinInvasionInternal += EventsManager.DD2EventOnWin;
    }
    public override void Unload()
    {
        base.Unload();
        Instance = null;
        ////////////////////////
        CalamityLoaded = false;
        ////////////////////////
        if (!Main.dedServ)
        {
            QuestsManager.Unload();
            AssetsManager.Unload();
            ThemeStyles.Unload();
        }
        ////////////////////////
        TalentsManager.Unload();
        UtilsModPacketPrimarySourceClient = null;
        UtilsModPacketPrimarySourceServer = null;
        ////////////////////////
        On_DD2Event.WinInvasionInternal -= EventsManager.DD2EventOnWin;
        ////////////////////////
        ModDebug.Unload();
    }

    public override void HandlePacket(BinaryReader reader, int whoAmI)
    {
        PacketPrimarySourceType type = (PacketPrimarySourceType)reader.ReadByte();
        switch (type)
        {
            case PacketPrimarySourceType.Client:
                UtilsModPacketPrimarySourceClient.Handle(reader, whoAmI);
                break;
            case PacketPrimarySourceType.Server:
                UtilsModPacketPrimarySourceServer.Handle(reader);
                break;
        }
        
    }

    // DEBUG:
    //public static void LogError(string msg)
    //{
    //    if (Instance is null)
    //        return;
    //    Instance.Logger.Error(msg);
    //}

    public static string GetDisplayName()
        => Instance is null ? string.Empty : Instance.DisplayName;

    /// <summary>
    ///     Checks whether the local player is active. 
    ///     Used for correct UI loading.
    /// </summary>
    /// <returns></returns>
    public static bool IsPlayerReady()
        => Instance is not null &&
        Main.LocalPlayer is not null &&
        Main.LocalPlayer.active;
    /// <summary>
    ///     Inverted check <see cref="IsPlayerReady"/>
    /// </summary>
    /// <returns></returns>
    public static bool IsPlayerNotReady() => !IsPlayerReady();

    /// <summary>
    ///     Returns the specified ModPlayer instance for the local player.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    public static T GetLocal<T>() where T : ModPlayer
        => IsPlayerReady() ? Main.LocalPlayer.GetModPlayer<T>() : null;

    public static bool IsSingle() => IsNetMode(NetmodeID.SinglePlayer);
    public static bool IsMultiplayer() => IsNetMode(NetmodeID.MultiplayerClient);

    /// <summary>
    ///     Abbreviated name of method <see cref="IsMultiplayer()"/>
    /// </summary>
    /// <returns></returns>
    public static bool IsMC() => IsMultiplayer();

    /// <summary>
    ///    Shortened check <see cref="Main.netMode"/> != <see cref="NetmodeID.MultiplayerClient"/>
    /// </summary>
    /// <returns></returns>
    public static bool IsNotMC() => !IsMC();
    public static bool IsServer() => IsNetMode(NetmodeID.Server);
    public static bool IsNetMode(int id) => Main.netMode == id;
}