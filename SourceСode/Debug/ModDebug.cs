using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Terraria;

namespace TalentsSystem;

/// <summary>
/// Debug Message Type
/// </summary>
public enum DMT : byte
{
    /// <summary>
    /// Event Manager
    /// </summary>
    EvtMgr,
    /// <summary>
    /// UtilsModPacketPrimarySourceClient
    /// </summary>
    MPSC,
    /// <summary>
    /// UtilsModPacketPrimarySourceServer
    /// </summary>
    MPSS
}

public static class ModDebug
{
#if DEBUG
    public static string LogFilesPath = string.Empty;
    public static bool ShowInChat = true;
    private static Dictionary<DMT, bool> Enabled;
    private static Dictionary<DMT, StreamWriter> ClientWriters;
    private static Dictionary<DMT, StreamWriter> ServerWriters;
    private static StreamWriter ClientAllWriter;
    private static StreamWriter ServerAllWriter;
#endif

    [Conditional("DEBUG")]
    public static void LoadOrReload()
    {
#if DEBUG
        Enabled = new();
        /////////////
        foreach (var dlt in Enum.GetValues<DMT>())
            Enabled[dlt] = true;
        /////////////
        if (LogFilesPath == string.Empty)
        {
            LogFilesPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            LogFilesPath = Path.Combine(LogFilesPath, "My Games", "Terraria", "tModLoader", "CommonLogs");
        }
        if(!Directory.Exists(LogFilesPath))
            Directory.CreateDirectory(LogFilesPath);
        /////////////
        Unload();
        /////////////
        if (Main.dedServ)
        {
            ServerWriters = new();
            ServerAllWriter = new StreamWriter(Path.Combine(LogFilesPath, $"1all_server.log"), append: false);
        }
        else
        {
            ClientWriters = new();
            ClientAllWriter = new StreamWriter(Path.Combine(LogFilesPath, $"1all_client.log"), append: false);
        }
        foreach (var dlt in Enum.GetValues<DMT>())
        {
            if (Main.dedServ)
            {
                string path2 = Path.Combine(LogFilesPath, $"{dlt}_server.log");
                ServerWriters[dlt] = new StreamWriter(path2, append: false);
            }
            else
            {
                string path1 = Path.Combine(LogFilesPath, $"{dlt}_client.log");
                ClientWriters[dlt] = new StreamWriter(path1, append: false);
            }
        }
#endif
    }

    [Conditional("DEBUG")]
    public static void Unload()
    {
#if DEBUG
        if (ClientWriters is not null)
            foreach (var w in ClientWriters.Values)
                w?.Dispose();
        if (ServerWriters is not null)
            foreach (var w in ServerWriters.Values)
                w?.Dispose();
        ClientAllWriter?.Dispose();
        ServerAllWriter?.Dispose();
#endif
    }

    [Conditional("DEBUG")]
    public static void Write(string message, DMT[] types, bool? showInChatOverride = null)
    {
#if DEBUG
        var types2 = types.Distinct();
        string time = DateTime.Now.ToString("T");
        string message2 = $"[{time}]: {message}";
        ////////////
        foreach (var type in types2)
        {
            if (Main.dedServ)
            {
                ServerWriters[type]?.WriteLine(message2);
                ServerWriters[type]?.Flush();
            }
            else
            {
                ClientWriters[type]?.WriteLine(message2);
                ClientWriters[type]?.Flush();
            }
        }
        ////////////
        string strTypes = string.Empty;
        foreach (var type in types2)
            strTypes += $"{type}/";
        strTypes = strTypes.TrimEnd('/');
        message2 = $"[{time}][{strTypes}]: {message}";
        if (Main.dedServ)
        {
            ServerAllWriter?.WriteLine(message2);
            ServerAllWriter?.Flush();
        }
        else
        {
            ClientAllWriter?.WriteLine(message2);
            ClientAllWriter?.Flush();
        }
        ////////////
        if (showInChatOverride is null)
            showInChatOverride = ShowInChat;
        if ((bool)showInChatOverride)
        {
            if (Main.dedServ)
                Console.WriteLine(message);
            else if(Main.LocalPlayer.active)
                Main.NewText(message, Color.DarkOrange);
        }
#endif
    }
}