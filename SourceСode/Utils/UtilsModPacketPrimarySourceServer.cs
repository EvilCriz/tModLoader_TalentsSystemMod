using Microsoft.Xna.Framework;
using System;
using System.IO;
using TalentsSystem.Quests;
using Terraria;
using Terraria.ModLoader;
using static TalentsSystem.Utils.UtilsModPacketPrimarySourceClient;
using TS = TalentsSystem.TalentsSystem;

namespace TalentsSystem.Utils;
public class UtilsModPacketPrimarySourceServer
{
    public enum PacketType : byte
    {
        SyncDD2EventOnWin,
        QuestOnKillNPC,
        SyncProgressTrigger,
        SyncSetStat,
        SyncShowCombatText,
        UpdateBuffTime,
        PotionDelay,
        SyncClearBuff,
        SyncActiveTalentActivated,
        SyncCreateExplosionVisual,
        SetParamsGlobalProjectile
    }
    public enum StatType : byte
    { 
        Health,
        Mana
    }
    public enum ActiveTalentType : byte
    {
        Reincarnation,
        FullManaRestoration,
        ManaHealing,
        HealthRegenMana
    }
    public void Handle(BinaryReader reader)
    { 
        PacketType type = (PacketType)reader.ReadByte();
        int pCausingWhoAmI = reader.ReadInt32();
        int npcWhoAmI;
        StatType statType;
        int statValue;
        int buffType;
        ModDebug.Write($"\nPreHandle -> type = {type}; pCausingWhoAmI = {pCausingWhoAmI}", [DMT.MPSS]);
        switch (type)
        {
            case PacketType.SyncDD2EventOnWin:
                int tier = reader.ReadInt32();
                EventsManager.SyncDD2EventOnWin(tier);
                break;
            case PacketType.QuestOnKillNPC:
                npcWhoAmI = reader.ReadInt32();
                QuestProgressCheckerGlobalNPC.SyncOnKillNPC(pCausingWhoAmI, npcWhoAmI);
                ModDebug.Write($"QuestOnKillNPC -> npcWhoAmI = {npcWhoAmI}", [DMT.MPSS]);
                break;
            case PacketType.SyncProgressTrigger:
                ProcessTriggersType ptType = (ProcessTriggersType)reader.ReadByte();
                GetMP().SyncProсessTrigger(ptType, reader);
                ModDebug.Write($"Handle: ProcessTriggers -> type = {ptType}", [DMT.MPSS]);
                break;
            case PacketType.SyncSetStat:
                statType = (StatType)reader.ReadByte(); 
                statValue = reader.ReadInt32();
                GetMP().SyncSetStat(statType, statValue);
                ModDebug.Write($"Handle: SetStat -> statType = {statType}; statValue = {statValue}", [DMT.MPSS]);
                break;
            case PacketType.SyncShowCombatText:
                statType = (StatType)reader.ReadByte();
                statValue = reader.ReadInt32();
                GetMP().SyncShowCombatText(statType, statValue);
                ModDebug.Write($"Handle: ShowCombatText -> statValue = {statValue}; statValue = {statValue}", [DMT.MPSS]);
                break;
            case PacketType.UpdateBuffTime:
                buffType = reader.ReadInt32();
                int buffTime = reader.ReadInt32();
                int id = GetP().FindBuffIndex(buffType);
                if (id != -1)
                {
                    GetP().buffTime[id] = buffTime;
                }
                ModDebug.Write($"Handle: UpdateBuffTime -> buffType = {buffType}; buffTime = {buffTime}; id = {id}", [DMT.MPSS]);
                break;
            case PacketType.PotionDelay:
                int potionDelay = reader.ReadInt32();
                GetP().potionDelay = potionDelay;
                ModDebug.Write($"Handle: PotionDelay -> potionDelay = {potionDelay}", [DMT.MPSS]);
                break;
            case PacketType.SyncClearBuff:
                buffType = reader.ReadInt32();
                GetMP().SyncClearBuff(buffType);
                ModDebug.Write($"Handle: SyncClearBuff -> buffType = {buffType}", [DMT.MPSS]);
                break;
            case PacketType.SyncActiveTalentActivated:
                ActiveTalentType activeTalentType = (ActiveTalentType)reader.ReadByte();
                switch (activeTalentType)
                {
                    case ActiveTalentType.Reincarnation:
                        GetMP().SyncActivateReincarnation();
                        break;
                    case ActiveTalentType.FullManaRestoration:
                        GetMP().SyncActivateFullManaRestoration();
                        break;
                    case ActiveTalentType.ManaHealing:
                        GetMP().SyncActivateManaHealing();
                        break;
                    case ActiveTalentType.HealthRegenMana:
                        GetMP().SyncActivateHealthRegenMana();
                        break;
                }
                ModDebug.Write($"Handle: SyncActiveTalentActivated -> activeTalentType = {activeTalentType}", [DMT.MPSS]);
                break;
            case PacketType.SyncCreateExplosionVisual:
                float posX = reader.ReadSingle();
                float posY = reader.ReadSingle();
                GetMP().SyncCreateExplosionVisual(new Vector2(posX, posY));
                ModDebug.Write($"Handle: SyncCreateExplosionVisual -> posX = {posX}; posY = {posY}", [DMT.MPSS]);
                break;
            case PacketType.SetParamsGlobalProjectile:
                int projWhoAmI = reader.ReadInt32();
                Main.projectile[projWhoAmI].GetGlobalProjectile<TalentApplyGlobalProjectile>().SetParams(GetP());
                ModDebug.Write($"Handle: SetParamsGlobalProjectile -> projWhoAmI = {projWhoAmI}", [DMT.MPSS]);
                break;
        }

        Player GetP() => Main.player[pCausingWhoAmI];
        TalentsApplyModPlayer GetMP() => GetP().GetModPlayer<TalentsApplyModPlayer>();
    }

    public static void Send(PacketType type, Action<ModPacket> writer, int pCausingWhoAmI, int toClient = -1)
    {
        if (!Main.dedServ)
            return;
        var packet = TS.Instance.GetPacket();
        packet.Write((byte)TS.PacketPrimarySourceType.Server);
        packet.Write((byte)type);
        packet.Write(pCausingWhoAmI);
        ModDebug.Write($"\nSend -> type = {type}; pCausingWhoAmI = {pCausingWhoAmI}", [DMT.MPSS]);
        writer(packet);
        packet.Send(toClient, -1);
    }

    public static void Send_SyncDD2EventOnWin(int tier)
    {
        Send(PacketType.SyncDD2EventOnWin, w => w.Write(tier), Main.maxPlayers);
        ModDebug.Write($"Send_ServerDD2EventOnWin -> pCausingWhoAmI = {Main.maxPlayers}; tier = {tier}", [DMT.MPSS]);
    }

    public static void Send_QuestSyncOnKillNPC(int pCausingWhoAmI, int npcWhoAmI)
    {
        Send(PacketType.QuestOnKillNPC, w => w.Write(npcWhoAmI), pCausingWhoAmI, toClient: pCausingWhoAmI);
        ModDebug.Write($"Send_QuestSyncOnKillNPC -> pCausingWhoAmI = {pCausingWhoAmI}; pCausingWhoAmI = {pCausingWhoAmI}", [DMT.MPSS]);
    }

    public static void Send_SyncProgressTrigger(int pCausingWhoAmI, ProcessTriggersType type, Action<ModPacket> writer)
    {
        Send(
            PacketType.SyncProgressTrigger, w =>
            {
                w.Write((byte)type);
                writer(w);
            }, 
            pCausingWhoAmI);
        ModDebug.Write($"Send_SyncProgressTrigger -> pCausingWhoAmI = {pCausingWhoAmI}; type = {type}", [DMT.MPSS]);
    }

    public static void Send_SyncSetStat(int pCausingWhoAmI, StatType type, int statValue)
    {
        Send(
            PacketType.SyncSetStat, w => 
            {
                w.Write((byte)type);
                w.Write(statValue);
            }, 
            pCausingWhoAmI);
        ModDebug.Write($"Send_SyncSetStat -> pCausingWhoAmI = {pCausingWhoAmI}; type = {type}; statValue = {statValue}", [DMT.MPSS]);
    }

    public static void Send_SyncShowCombatText(int pCausingWhoAmI, StatType type, int statValue)
    {
        Send(
            PacketType.SyncShowCombatText, w =>
            {
                w.Write((byte)type);
                w.Write(statValue);
            },
            pCausingWhoAmI);
        ModDebug.Write($"Send_SyncShowCombatText -> pCausingWhoAmI = {pCausingWhoAmI}; type = {type}; statValue = {statValue}", [DMT.MPSS]);
    }

    public static void Send_UpdateBuffTime(int pCausingWhoAmI, int buffType, int buffTime)
    {
        Send(PacketType.UpdateBuffTime, w =>
        {
            w.Write(buffType);
            w.Write(buffTime);
        }, 
        pCausingWhoAmI);
        ModDebug.Write($"Send_UpdateBuffTime -> pCausingWhoAmI = {pCausingWhoAmI}; buffType = {buffType}; buffTime = {buffTime}", [DMT.MPSS]);
    }
    public static void Send_PotionDelay(int pCausingWhoAmI, int delay)
    {
        Send(PacketType.PotionDelay, w => w.Write(delay), pCausingWhoAmI);
        ModDebug.Write($"Send_PotionDelay -> pCausingWhoAmI = {pCausingWhoAmI}; delay = {delay}", [DMT.MPSS]);
    }

    public static void Send_SyncActiveTalentActivated(int pCausingWhoAmI, ActiveTalentType type)
    {
        Send(PacketType.SyncActiveTalentActivated, w => w.Write((byte)type), pCausingWhoAmI);
        ModDebug.Write($"Send_SyncActiveTalentActivated -> pCausingWhoAmI = {pCausingWhoAmI}; delay = {type}", [DMT.MPSS]);
    }

    public static void Send_SyncClearBuff(int pCausingWhoAmI, int buffType)
    {
        Send(PacketType.SyncClearBuff, w => w.Write(buffType), pCausingWhoAmI);
        ModDebug.Write($"Send_SyncActiveTalentActivated -> pCausingWhoAmI = {pCausingWhoAmI}; buffType = {buffType}", [DMT.MPSS]);
    }

    public static void Send_SyncCreateExplosionVisual(int pCausingWhoAmI, Vector2 position)
    {
        Send(PacketType.SyncCreateExplosionVisual, w =>
        {
            w.Write(position.X);
            w.Write(position.Y);
        },
        pCausingWhoAmI);
        ModDebug.Write($"Send_SyncCreateExplosionVisual -> pCausingWhoAmI = {pCausingWhoAmI}; position = {position}", [DMT.MPSS]);
    }

    public static void Send_SetParamsGlobalProjectile(int pCausingWhoAmI, int projWhoAmI)
    {
        Send(PacketType.SetParamsGlobalProjectile, w => w.Write(projWhoAmI), pCausingWhoAmI);
        ModDebug.Write($"Send_SyncCreateExplosionVisual -> pCausingWhoAmI = {pCausingWhoAmI}; projWhoAmI = {projWhoAmI}", [DMT.MPSS]);
    }
}
