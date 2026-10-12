using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;

using TBT = TalentsSystem.TalentsBranchType;

namespace TalentsSystem;

public class TalentsSaveInfo : ModPlayer
{
    public TBT CurrentBranch = TBT.Travel;
    protected Dictionary<TBT, Dictionary<int, int>> TalentsPointsTable = new();

    /////////////////
    public override void SaveData(TagCompound tag)
    {
        tag[nameof(CurrentBranch)] = (byte)CurrentBranch;
        foreach (var tbt in Enum.GetValues<TBT>())
        {
            var tbtName = Enum.GetName(tbt);
            if (!TalentsPointsTable.ContainsKey(tbt))
                TalentsPointsTable[tbt] = new();
            var table = TalentsPointsTable[tbt];
            foreach (var pair in table)
                tag[$"{tbtName}{pair.Key}"] = pair.Value;
        }
    }
    public override void LoadData(TagCompound tag)
    {
        CurrentBranch = (TBT)tag.GetByte(nameof(CurrentBranch));
        foreach (var tbt in Enum.GetValues<TBT>())
        {
            var tbtName = Enum.GetName(tbt);
            TalentsPointsTable[tbt] = new();
            var table = TalentsPointsTable[tbt];
            foreach (var id in TalentsManager.Instance.TalentLevelTable[tbt].Keys)
                table[id] = tag.GetInt($"{tbtName}{id}");
        }
    }
    public override void OnEnterWorld()
    {
        Utils.UtilsModPacketPrimarySourceClient.Send_OnPlayerConnected(this);
    }
    
    public Dictionary<TBT, Dictionary<int, int>> GetTalentsPointsForConnect()
    {
        Dictionary<TBT, Dictionary<int, int>> result = new();
        foreach (var table in TalentsPointsTable)
        {
            TBT key = table.Key;
            result[key] = new();
            foreach (var info in TalentsPointsTable[key])
                if (info.Value > 0)
                    result[key][info.Key] = info.Value;
            if (result[key].Count == 0)
                result.Remove(key);
        }
        return result;
    }
    public void SetTalentsPointsForConnect(TBT currentBranch, Dictionary<TBT, Dictionary<int, int>> syncTable)
    {
        CurrentBranch = currentBranch;
        ///////////////////
        foreach (var tbt in Enum.GetValues<TBT>())
        {
            TalentsPointsTable[tbt] = new();
            foreach (var id in TalentsManager.Instance.TalentLevelTable[tbt].Keys)
                TalentsPointsTable[tbt][id] = 0;
        }
        //////////////////
        foreach (var table in syncTable)
        {
            TBT key = table.Key;
            foreach (var info in syncTable[key])
                TalentsPointsTable[key][info.Key] = info.Value;
        }
        /////////////////
        Player.GetModPlayer<TalentsApplyModPlayer>().OnEnterWorld();
    }

    public int GetActualPoints(int talentId) => GetActualPoints(CurrentBranch, talentId);
    public int GetActualPoints(TBT branch, int talentId)
    {
        return TalentsPointsTable[branch][talentId];
    }
    public int GetSpentPointsBranch() => GetSpentPointsBranch(CurrentBranch);
    public int GetSpentPointsBranch(TBT branch)
    {
        var table = TalentsPointsTable[branch].Values;
        int result = 0;
        foreach (var points in table)
            result += points;
        return result;
    }
    public int GetFreePointsBranch() => GetFreePointsBranch(CurrentBranch);
    public int GetFreePointsBranch(TBT branch)
        => Player.GetModPlayer<QuestSaveInfo>().Level - GetSpentPointsBranch(branch);

    public void AddTalentPoint(TBT branch, int talentId)
    {
        TalentsPointsTable[branch][talentId]++;
        Player.GetModPlayer<TalentsApplyModPlayer>().RefreshModifiers();
    }
    public void ResetBranch(TBT branch)
    {
        foreach (var key in TalentsPointsTable[branch].Keys)
            TalentsPointsTable[branch][key] = 0;
        Player.GetModPlayer<TalentsApplyModPlayer>().RefreshModifiers();
    }
}