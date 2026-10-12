using Terraria;
using Terraria.GameContent.Events;
using TBT = TalentsSystem.TalentsBranchType;

namespace TalentsSystem;

/// <summary>
///     Event execution and synchronization: changing talents/talent trees, completing quests
///     Synchronization of these events across all parties
/// </summary>
public static class EventsManager
{
    public static void OnBranchActivated(TBT branch)
        => OnBranchActivated(Main.LocalPlayer, branch, false);
    public static void OnBranchActivated(Player player, TBT branch, bool handle)
    {
        if (!handle)
            Utils.UtilsModPacketPrimarySourceClient.Send_OnBranchActivated(branch);
        var save = player.GetModPlayer<TalentsSaveInfo>();
        save.CurrentBranch = branch;
        var talents = player.GetModPlayer<TalentsApplyModPlayer>();
        talents.RefreshModifiers();
        ModDebug.Write($"OnBranchActivated -> player = {player.whoAmI}; branch = {branch}; handle: {handle}", [DMT.EvtMgr]);
    }

    public static void OnBranchReset(TBT branch)
        => OnBranchReset(Main.LocalPlayer, branch, false);
    public static void OnBranchReset(Player player, TBT branch, bool handle) 
    {
        if (!handle)
            Utils.UtilsModPacketPrimarySourceClient.Send_OnBranchReset(branch);
        var save = player.GetModPlayer<TalentsSaveInfo>();
        save.ResetBranch(branch);
        if (player == Main.LocalPlayer)
            UIMenuSystem.SafeUpdate();
        ModDebug.Write($"OnBranchReset -> player = {player.whoAmI}; branch = {branch}; handle: {handle}", [DMT.EvtMgr]);
    }

    public static void OnTalentLevelIncreased(Talent talent)
        => OnTalentLevelIncreased(Main.LocalPlayer, talent.BranchType, talent.ID, false);
    public static void OnTalentLevelIncreased(Player player, TBT branch, int talentId, bool handle) 
    {
        if (!handle)
            Utils.UtilsModPacketPrimarySourceClient.Send_OnTalentLevelIncreased(branch, talentId);
        var save = player.GetModPlayer<TalentsSaveInfo>();
        save.AddTalentPoint(branch, talentId);
        ModDebug.Write($"OnTalentLevelIncreased -> player = {player.whoAmI}; branch = {branch}; talentId: {talentId}; handle: {handle}", [DMT.EvtMgr]);
    }


    public static void DD2EventOnWin(On_DD2Event.orig_WinInvasionInternal orig)
    {
        orig();

        int tier = 1;
        if (DD2Event.DownedInvasionT3)
            tier = 3;
        else if (DD2Event.DownedInvasionT2)
            tier = 2;
        if (TalentsSystem.IsSingle())
        {
            SyncDD2EventOnWin(tier);
        }
        else if (TalentsSystem.IsServer())
        {
            Utils.UtilsModPacketPrimarySourceServer.Send_SyncDD2EventOnWin(tier);
        }
    }

    public static void SyncDD2EventOnWin(int tier)
    {
        var table = QuestsManager.GetQuestsTable();
        var qIdxs = TalentsSystem.GetLocal<QuestSaveInfo>().GetSortedActualQuestTable(Quest.QuestType.QEventOldArmy);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QEventOldArmy;
            quest.Invoke(tier);
        }
    }
}