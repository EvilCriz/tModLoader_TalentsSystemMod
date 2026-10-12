using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TalentsSystem;

[Autoload(Side = ModSide.Client)]
public class QuestSaveInfo : ModPlayer
{
    ////////////////////
    public int Level = 0;
    public int Exp = 0;
    ////////////////////
    public List<int> CompletedQuests = new();
    public List<int> IncompletedQuests = new();
    protected Dictionary<Quest.QuestType, List<int>> SortedActualQuestTable = new();
    protected List<(Quest.QuestType, int)> LastCompletedQuests = new();
    ////////////////////
    public int QuestInfoKillAnyCount;
    public int QuestInfoDestroyCount;
    ////////////////////
    public override void SaveData(TagCompound tag)
    {
        UpdateLastQuestComplete();
        ////////////////////
        tag[nameof(Level)] = Level;
        tag[nameof(Exp)] = Exp;
        ////////////////////
        foreach (var id in CompletedQuests)
            tag[$"QID{id}"] = true;
        ////////////////////
        tag[$"{nameof(QuestInfoKillAnyCount)}"] = QuestInfoKillAnyCount;
        tag[$"{nameof(QuestInfoDestroyCount)}"] = QuestInfoDestroyCount;
    }
    public override void LoadData(TagCompound tag)
    {
        Level = tag.GetInt(nameof(Level));
        Exp = tag.GetInt(nameof(Exp));
        ////////////////////
        LastCompletedQuests.Clear();
        var qm = QuestsManager.Instance;
        var all = qm.AllQuestsArray;
        foreach (var qid in all)
            if (tag.TryGet($"QID{qid}", out bool val))
                if (val) CompletedQuests.Add(qid); else IncompletedQuests.Add(qid);
            else
                IncompletedQuests.Add(qid);
        ////////////////////
        UpdateSortedActualQuestTable(qm);
        ////////////////////
        QuestInfoKillAnyCount = tag.GetInt($"{nameof(QuestInfoKillAnyCount)}");
        QuestInfoDestroyCount = tag.GetInt($"{nameof(QuestInfoDestroyCount)}");
    }

    public override void PostUpdate()
    {
        UpdateLastQuestComplete();
    }
    public void UpdateLastQuestComplete()
    {
        foreach (var last in LastCompletedQuests)
        {
            (Quest.QuestType type, int questId) quest = last;
            SortedActualQuestTable[quest.type].Remove(quest.questId);
        }
        if(LastCompletedQuests.Count > 0)
            UIMenuSystem.SafeUpdate();
        LastCompletedQuests.Clear();
    }
    public void UpdateSortedActualQuestTable(QuestsManager questManager)
    {
        foreach (var key in questManager.SortedQuestTable.Keys)
            SortedActualQuestTable[key] = new();
        foreach (var questId in CompletedQuests)
        {
            var quest = questManager.QuestsTable[questId];
            if (quest.Sync)
                SortedActualQuestTable[quest.Type].Add(quest.ID);
        }
        foreach (var questId in IncompletedQuests)
        {
            var quest = questManager.QuestsTable[questId];
            SortedActualQuestTable[quest.Type].Add(quest.ID);
        }
    }

    public void CompleteQuest(int questId, bool handle = false)
    {
        if (CompletedQuests is null)
            return;
        if (Main.netMode == NetmodeID.Server)
            return;
        /////////////////
        var qm = QuestsManager.Instance;
        var quest = qm.QuestsTable[questId];
        if (!handle && quest.Sync)
            Utils.UtilsModPacketPrimarySourceClient.Send_QuestComplete(questId);
        if (CompletedQuests.Contains(questId))
            return;
        if (!qm.QuestExpTable.ContainsKey(questId))
            return;
        /////////////////
        CompletedQuests.Add(questId);
        if (IncompletedQuests.Contains(questId))
            IncompletedQuests.Remove(questId);
        if (!quest.Sync)
        {
            var type = quest.Type;
            if (SortedActualQuestTable.ContainsKey(type))
            {
                var questIndexes = SortedActualQuestTable[type];
                if (questIndexes.Contains(questId))
                    LastCompletedQuests.Add((type, questId));
            }
        }
        //////////////////
        UINotificationSystem.AddQuestCompleteNotification(questId);
        /////////////////
        int lastLevel = Level;
        if (!IsMaxLevel(Level))
        {
            int questExp = qm.QuestExpTable[questId];
            Exp += questExp;
            int needEx = GetNeedExp(Level);
            while (Exp >= needEx)
            {
                if (IsMaxLevel(++Level))
                {
                    SetMaxLevel();
                    break;
                }
                Exp -= needEx;
                needEx = GetNeedExp(Level);
                UINotificationSystem.AddLevelUpNotification();
            }
            UIMenuSystem.SafeUpdate();
        }
        ////////////////////
        SoundEngine.PlaySound(
            Level > lastLevel
            ? SoundID.AchievementComplete
            : SoundID.ResearchComplete);
    }

    public int GetMaxLevel() => QuestsManager.Instance.LevelExpArray.Count;
    public int GetMaxLevelExp() => GetNeedExp(GetMaxLevel() - 1);
    public bool IsMaxLevel(int level) => level >= QuestsManager.Instance.LevelExpArray.Count;
    public void SetMaxLevel()
    {
        Exp = GetMaxLevelExp();
        Level = GetMaxLevel();
    }
    public int GetNeedExp(int level)
    {
        var qm = QuestsManager.Instance;
        if (qm.LevelExpArray is null)
            return -1;
        /////////////////
        level = (int)MathHelper.Max(0, level);
        level = (int)MathHelper.Min(level, qm.LevelExpArray.Count - 1);
        return qm.LevelExpArray[level];
    }
    public List<int> GetSortedActualQuestTable(Quest.QuestType type)
    {
        if (SortedActualQuestTable.ContainsKey(type))
            return SortedActualQuestTable[type];
        return new();
    }
}