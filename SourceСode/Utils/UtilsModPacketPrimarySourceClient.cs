using System;
using System.Collections.Generic;
using System.IO;
using Terraria;
using Terraria.ModLoader;
using TBT = TalentsSystem.TalentsBranchType;
using TS = TalentsSystem.TalentsSystem;

namespace TalentsSystem.Utils;

public class UtilsModPacketPrimarySourceClient
{
    public enum PacketType : byte
    {
        Unknown,
        QuestComplete,
        OnBranchActivated,
        OnBranchReset,
        OnTalentLevelIncreased,
        OnPlayerConnected,
        ProcessTriggers,
        PostHurt,
        OnHitNPC
    }

    public enum ProcessTriggersType : byte
    {
        ChargeKeybind,
        SwithAmmoKeybind,
        SwithSandStormKeybind,
        SwithWithoutCollisionProjectileKeybind,
        SwithProjectileAIMKeybind,
        ReincarnationKeybind,
        SpawnRateKeybind,
        FullManaRestorationKeybind,
        ManaHealingKeybind,
        HealthRegenManaKeybind
    }

    public void Handle(BinaryReader reader, int whoAmI)
    {
        Action serverAction = null;
        Action clientAction = null;
        bool activateClientActionOnServer = false;
        ////////////////////
        TBT branch;
        ////////////////////
        if(!TS.IsServer()) 
            whoAmI = reader.ReadInt32();
        TalentsSaveInfo mp1 = GetPlayer().GetModPlayer<TalentsSaveInfo>();
        TalentsApplyModPlayer mp2 = GetPlayer().GetModPlayer<TalentsApplyModPlayer>();
        ////////////////////
        var type = (PacketType)reader.ReadByte();
        ModDebug.Write($"\nPreHandle -> whoAmI = {whoAmI}; type = {type}", [DMT.MPSC]);
        switch (type)
        {
            case PacketType.QuestComplete:
                int questId = reader.ReadInt32();
                serverAction = () => Send_QuestComplete(questId, whoAmI);
                clientAction = () => TS.GetLocal<QuestSaveInfo>().CompleteQuest(questId, handle: true);
                ModDebug.Write($"Handle: QuestComplete -> questId = {questId}", [DMT.MPSC]);
                break;
            case PacketType.OnBranchActivated:
                branch = ReadBranch();
                serverAction = () => Send_OnBranchActivated(branch, whoAmI);
                clientAction = () => EventsManager.OnBranchActivated(GetPlayer(), branch, handle: true);
                activateClientActionOnServer = true;
                ModDebug.Write($"Handle: OnBranchActivated -> branch = {branch}", [DMT.MPSC]);
                break;
            case PacketType.OnBranchReset:
                branch = ReadBranch();
                serverAction = () => Send_OnBranchReset(branch, whoAmI);
                clientAction = () => EventsManager.OnBranchReset(GetPlayer(), branch, true);
                activateClientActionOnServer = true;
                ModDebug.Write($"Handle: OnBranchReset -> branch = {branch}", [DMT.MPSC]);
                break;
            case PacketType.OnTalentLevelIncreased:
                branch = ReadBranch();
                int talentId = reader.ReadInt32();
                serverAction = () => Send_OnTalentLevelIncreased(branch, talentId, whoAmI);
                clientAction = () => EventsManager.OnTalentLevelIncreased(GetPlayer(), branch, talentId, handle: true);
                activateClientActionOnServer = true;
                ModDebug.Write($"Handle: OnTalentLevelIncreased -> branch = {branch}; talentId = {talentId}", [DMT.MPSC]);
                break;
            case PacketType.OnPlayerConnected:
                branch = ReadBranch();
                ///////////////////////
                int count = reader.ReadInt32();
                ModDebug.Write($"Handle: OnPlayerConnected -> branch = {branch}; count = {count}", [DMT.MPSC]);
                Dictionary<TBT, Dictionary<int, int>> talentPointsTable = new(count);
                for (int i = 0; i < count; i++)
                {
                    var innerBranch = ReadBranch();
                    int innerCount = reader.ReadInt32();
                    talentPointsTable[innerBranch] = new(innerCount);
                    ModDebug.Write($"Handle: OnPlayerConnected -> innerBranch = {innerBranch}; innerCount = {innerCount}", [DMT.MPSC], showInChatOverride: false);
                    for (int j = 0; j < innerCount; j++)
                    { 
                        int key = reader.ReadInt32();
                        int value = reader.ReadInt32();
                        talentPointsTable[innerBranch][key] = value;
                        ModDebug.Write($"Handle: OnPlayerConnected -> talentID = {key}; points: {value}", [DMT.MPSC], showInChatOverride: false);
                    }
                }
                
                serverAction = () => Send_OnPlayerConnected(mp1);
                clientAction = () => mp1.SetTalentsPointsForConnect(branch, talentPointsTable);
                activateClientActionOnServer = true;
                break;
            case PacketType.ProcessTriggers:
                mp2 = GetPlayer().GetModPlayer<TalentsApplyModPlayer>();
                ProcessTriggersType ptType = (ProcessTriggersType)reader.ReadByte();
                serverAction = () => mp2.SyncProсessTrigger(ptType, reader);
                ModDebug.Write($"Handle: ProcessTriggers -> type = {ptType}", [DMT.MPSC]);
                break;
            case PacketType.PostHurt:
                serverAction = () => Send_PostHurt(whoAmI);
                clientAction = () => mp2.SyncPostHurt();
                activateClientActionOnServer = true;
                ModDebug.Write($"Handle: PostHurt", [DMT.MPSC]);
                break;
            case PacketType.OnHitNPC:
                int targetWhoAmI = reader.ReadInt32();
                bool currentDmgClass = reader.ReadBoolean();
                if (TS.IsServer())
                {
                    bool crit = reader.ReadBoolean();
                    int damageDone = reader.ReadInt32();
                    mp2.SyncOnHitNPC_AllSides(Main.npc[targetWhoAmI], currentDmgClass);
                    mp2.SyncOnHitNPC_SingleServerSides(Main.npc[targetWhoAmI], currentDmgClass, crit, damageDone);
                    Send(
                        PacketType.OnHitNPC, 
                        w =>
                        {
                            w.Write(targetWhoAmI);
                            w.Write(currentDmgClass);
                        }, 
                        whoAmI);
                    ModDebug.Write($"Handle: OnHitNPC -> targetWhoAmI = {targetWhoAmI}; currentDmgClass = {currentDmgClass}; crit = {crit}; damageDone = {damageDone}", [DMT.MPSC]);
                }
                else
                {
                    mp2.SyncOnHitNPC_AllSides(Main.npc[targetWhoAmI], currentDmgClass);
                }
                break;
        }
        ////////////////////
        if (TS.IsServer())
        {
            if (activateClientActionOnServer && clientAction is not null)
            {
                ModDebug.Write($"Handle: clientAction Invoke", [DMT.MPSC]);
                clientAction();
            }
            if (serverAction is not null)
            {
                ModDebug.Write($"Handle: serverAction Invoke", [DMT.MPSC]);
                serverAction();
            }
        }
        else if (clientAction is not null)
        {
            ModDebug.Write($"Handle: clientAction Invoke", [DMT.MPSC]);
            clientAction();
        }

        TBT ReadBranch() => (TBT)reader.ReadByte();
        Player GetPlayer() => Main.player[whoAmI];
    }

    public static void Send(PacketType type, Action<ModPacket> writer, int whoAmI)
    {
        if (TS.IsSingle())
            return;
        ModDebug.Write($"\nSend -> type = {type}; whoAmI = {whoAmI};", [DMT.MPSC]);
        ////////////////////
        ModPacket packet = TS.Instance.GetPacket();
        packet.Write((byte)TS.PacketPrimarySourceType.Client);
        if (TS.IsServer())
            packet.Write(whoAmI);
        packet.Write((byte)type);
        writer(packet);
        packet.Send(-1, TS.IsServer() ? whoAmI : -1);
    }
    public static void Send_QuestComplete(int questId, int whoAmI = -1)
    {
        Send(PacketType.QuestComplete, (w) => w.Write(questId), whoAmI);
        ModDebug.Write($"Send_QuestComplete -> questId = {questId};", [DMT.MPSC]);
    }
    public static void Send_OnBranchActivated(TBT branch, int whoAmI = -1)
    {
        Send(PacketType.OnBranchActivated, (w) => w.Write((byte)branch), whoAmI);
        ModDebug.Write($"Send_OnBranchActivated -> branch = {branch};", [DMT.MPSC]);
    }
    public static void Send_OnBranchReset(TBT branch, int whoAmI = -1)
    {
        Send(PacketType.OnBranchReset, (w) => w.Write((byte)branch), whoAmI);
        ModDebug.Write($"Send_OnBranchReset -> branch = {branch};", [DMT.MPSC]);
    }

    public static void Send_OnTalentLevelIncreased(TBT branch, int talentId, int whoAmI = -1)
    {
        Send(
            PacketType.OnTalentLevelIncreased,
            (w) =>
            {
                w.Write((byte)branch);
                w.Write(talentId);
            },
            whoAmI);
        ModDebug.Write($"Send_OnTalentLevelIncreased -> branch = {branch}; talentId = {talentId}", [DMT.MPSC]);
    }

    public static void Send_OnPlayerConnected(TalentsSaveInfo save)
    {
        Send(PacketType.OnPlayerConnected,
            (w) => 
            {
                var currentBranch = save.CurrentBranch;
                w.Write((byte)currentBranch);
                //////////////////////
                var talentPointsTable = save.GetTalentsPointsForConnect();
                int count = talentPointsTable.Count;
                ModDebug.Write($"Send_OnPlayerConnected -> currentBranch = {currentBranch}; count = {count}", [DMT.MPSC]);
                w.Write(count);
                foreach (var table in talentPointsTable)
                {
                    TBT key = table.Key;
                    int innerCount = talentPointsTable[key].Keys.Count;
                    w.Write((byte)key);
                    w.Write(innerCount);
                    ModDebug.Write($"Send_OnPlayerConnected -> branch = {key}; innerCount = {innerCount}", [DMT.MPSC], showInChatOverride: false);
                    foreach (var info in talentPointsTable[key])
                    {
                        w.Write(info.Key);
                        w.Write(info.Value);
                        ModDebug.Write($"Send_OnPlayerConnected -> talentId = {info.Key}; points = {info.Value}", [DMT.MPSC], showInChatOverride: false);
                    }
                }
            },
            save.Player.whoAmI);
    }

    public static void Send_ProcessTriggers(ProcessTriggersType type, Action<ModPacket> postWriterArgs = null)
    {
        Send(
            PacketType.ProcessTriggers,
            w =>
            {
                w.Write((byte)type);
                if (postWriterArgs is not null)
                    postWriterArgs(w);
            },
            -1);
        ModDebug.Write($"Send_ProcessTriggers -> type = {type};", [DMT.MPSC]);
    }

    public static void Send_PostHurt(int whoAmI)
    {
        Send(PacketType.PostHurt, w => { }, whoAmI);
        ModDebug.Write($"Send_ProcessTriggers", [DMT.MPSC]);
    }

    public static void Send_SyncOnHitNPC(int whoAmI, int targetWhoAmI, bool currentDmgClass, bool crit, int damageDone)
    {
        Send(
            PacketType.OnHitNPC, w => 
            {
                w.Write(targetWhoAmI);
                w.Write(currentDmgClass);
                w.Write(crit);
                w.Write(damageDone);
            }, 
            whoAmI);
        ModDebug.Write($"Send_SyncOnHitNPC -> targetWhoAmI = {targetWhoAmI}; crit = {crit}; currentDmgClass = {currentDmgClass}; damageDone = {damageDone}", [DMT.MPSC]);
    }
}