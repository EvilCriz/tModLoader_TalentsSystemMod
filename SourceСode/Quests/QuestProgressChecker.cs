using System;
using System.Collections.Generic;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using QM = TalentsSystem.QuestsManager;
using QT = TalentsSystem.Quest.QuestType;
using TS = TalentsSystem.TalentsSystem;

namespace TalentsSystem.Quests;

file static class QPUtils
{
    public static bool IsMyPlayer(Player player) => player.whoAmI != Main.myPlayer;
    public static List<int> GetActualQuestTable(QT qt)
        => TS.GetLocal<QuestSaveInfo>().GetSortedActualQuestTable(qt);
}

[Autoload(Side = ModSide.Client)]
public class QuestProgressCheckerModPlayer : ModPlayer
{
    protected Utils.UtilsUpdaterByTick UpdateHelper = null;
    protected Utils.UtilsUpdaterByTick UpdateInventoryHelper = null;
    protected Utils.UtilsUpdaterByTick UpdateEventsHelper = null;
    protected Item[] LastInventory;
    protected Item[] LastBank4;
    protected int LastTalkNPC = -1;

    public bool IsMyPlayer => QPUtils.IsMyPlayer(Player);

    protected Item[] CopyInventory(Item[] itemsSource)
    {
        Item[] result = new Item[itemsSource.Length];
        for (int i = 0; i < itemsSource.Length; i++)
            result[i] = itemsSource[i];
        return result;
    }

    public override void OnEnterWorld()
    {
        if (IsMyPlayer)
            return;
        //////////////////////
        var questsIdsTable = QPUtils.GetActualQuestTable(QT.QEvent);
        foreach (var questId in questsIdsTable)
            QM.Instance.QuestsTable[questId].QEvent.ResetState();
        LastInventory = CopyInventory(Player.inventory);
        LastBank4 = CopyInventory(Player.bank4.item);
    }

    public override void PostUpdate()
    {
        base.PostUpdate();
        if (IsMyPlayer)
            return;
        //////////////////////
        UpdateInvetory();
        Update();
        //////////////////////
    }
    public override void UpdateDead()
    {
        if (IsMyPlayer)
            return;
        //////////////////////
        base.UpdateDead();
        Update();
    }
    protected List<Action> CreateUpdateActions()
    {
        List<Action> actions = new();
        var table = QM.GetQuestsTable();
        //////////////////
        actions.Add(() =>
        {
            var qIdxs = QPUtils.GetActualQuestTable(QT.QOther);
            foreach (var id in qIdxs)
            {
                var quest = table[id].QOther;
                if (quest.Type == QuestInfoOther.OtherType.PlayerDeath && Player.dead)
                    quest.Invoke();
            }
        });
        actions.Add(() =>
        {
            var qIdxs = QPUtils.GetActualQuestTable(QT.QVisited);
            foreach (var id in qIdxs)
            {
                var quest = table[id].QVisited;
                quest.Invoke(Player);
            }
        });
        actions.Add(() =>
        {
            int talk = Player.talkNPC;
            if (talk != -1 && talk != LastTalkNPC)
            {
                var qIdxs = QPUtils.GetActualQuestTable(QT.QTalkNPC);
                foreach (var id in qIdxs)
                {
                    var quest = table[id].QTalkNPC;
                    quest.Invoke(Main.npc[talk].type);
                }
            }
            LastTalkNPC = talk;
        });
        return actions;
    }
    protected List<Action> CreateUpdateEventsActions()
    {
        List<Action> actions = new();
        var table = QM.GetQuestsTable();
        actions.Add(() =>
        {
            var qIdxs = QPUtils.GetActualQuestTable(QT.QEvent);
            foreach (var id in qIdxs)
                table[id].QEvent.Invoke();
        });
        return actions;
    }
    public void Update()
    {
        if (UpdateHelper is null)
            UpdateHelper = new(delayMin: 60, delayMax: 120, actions: CreateUpdateActions());
        UpdateHelper.Update();
        //////////////////////
        if (UpdateEventsHelper is null)
            UpdateEventsHelper = new(delayMin: 300, delayMax: 300, actions: CreateUpdateEventsActions());
        UpdateEventsHelper.Update();
    }
    public void UpdateInvetory()
    {
        if (LastInventory is null)
        {
            LastInventory = CopyInventory(Player.inventory);
            LastBank4 = CopyInventory(Player.bank4.item);
            return;
        }
        //////////////////////
        if (UpdateInventoryHelper is null)
        {
            List<Action> UpIActions = new();
            UpIActions.Add(() =>
            {
                List<int> updatedInv = UpdateInventory(LastInventory, Player.inventory);
                if (updatedInv.Count > 0)
                {
                    foreach (var id in updatedInv)
                        OnReciveItem(Player.inventory[id].type);
                    LastInventory = CopyInventory(Player.inventory);
                }
            });
            UpIActions.Add(() =>
            {
                List<int> updatedBank4 = UpdateInventory(LastBank4, Player.bank4.item);
                if (updatedBank4.Count > 0)
                {
                    foreach (var id in updatedBank4)
                        OnReciveItem(Player.bank4.item[id].type);
                    LastBank4 = CopyInventory(Player.bank4.item);
                }
            });
            //////////////////////
            UpdateInventoryHelper = new(delayMin: 180, delayMax: 210, UpIActions);
        }
        UpdateInventoryHelper.Update();
        //////////////////////
        List<int> UpdateInventory(Item[] lasts, Item[] invs)
        {
            List<int> result = new();
            for (int i = 0; i < lasts.Length; i++)
            {
                var last = lasts[i];
                var inv = invs[i];
                bool lastEmpty = last.type == ItemID.None;
                bool invEmpty = inv.type == ItemID.None;
                if (lastEmpty != invEmpty)
                {
                    if (!invEmpty)
                        result.Add(i);
                    continue;
                }
                if (lastEmpty)
                    continue;
                if (last.type != inv.type)
                {
                    result.Add(i);
                    continue;
                }
                if (last.stack < inv.stack)
                    result.Add(i);
            }
            return result;
        }   
    }

    public override bool OnPickup(Item item) { OnReciveItem(item.type); return true; }
    public override void PostBuyItem(NPC vendor, Item[] shopInventory, Item item) => OnReciveItem(item.type);
    public void OnReciveItem(int type)
    {
        var table = QM.GetQuestsTable();
        //////////////////////
        var qIdxs = QPUtils.GetActualQuestTable(QT.QItemAnyRecive);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QItemAnyRecive;
            quest.Invoke(type);
        }
        //////////////////////
        qIdxs = QPUtils.GetActualQuestTable(QT.QItemAllRecive);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QItemAllRecive;
            quest.Invoke(type);
        }
    }

    public void OnKillNPC(int whoAmI)
    {
        NPC target = Main.npc[whoAmI];
        //////////////////////
        var table = QM.GetQuestsTable();
        var save = TS.GetLocal<QuestSaveInfo>();
        //////////////////////
        save.QuestInfoKillAnyCount++;
        var qIdxs = QPUtils.GetActualQuestTable(QT.QKillAny);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QKillAny;
            quest.Invoke(save.QuestInfoKillAnyCount);
        }
        //////////////////////
        qIdxs = QPUtils.GetActualQuestTable(QT.QKillAnyType);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QKillAnyType;
            quest.Invoke(target.type);
        }
        //////////////////////
        //if (target.boss)
        //{
            qIdxs = QPUtils.GetActualQuestTable(QT.QKillBossAny);
            foreach (var id in qIdxs)
            {
                var quest = table[id].QKillBossAny;
                quest.Invoke(target);
            }
        //}
        ///////////////////////////
        if (target.townNPC)
        {
            qIdxs = QPUtils.GetActualQuestTable(QT.QKillTownNPC);
            foreach (var id in qIdxs)
            {
                var quest = table[id].QKillTownNPC;
                quest.Invoke(target.type);
            }
        }
    }
}

[Autoload(Side = ModSide.Client)]
public class QuestProgressCheckerGlobalItem : GlobalItem
{
    public override bool? UseItem(Item item, Player player)
    {
        if (QPUtils.IsMyPlayer(player))
            return base.UseItem(item, player);
        ////////////////////////
        var table = QM.GetQuestsTable();
        ////////////////////////
        var qIdxs = QPUtils.GetActualQuestTable(QT.QItemUse);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QItemUse;
            quest.Invoke(item.type);
        }
        ////////////////////////
        return base.UseItem(item, player);
    }
}

[Autoload(Side = ModSide.Client)]
public class QuestProgressCheckerGlobalTile : GlobalTile
{
    public override void KillTile(int i, int j, int type, ref bool fail, ref bool effectOnly, ref bool noItem)
    {
        if (fail || effectOnly || noItem)
            return;
        var save = TS.GetLocal<QuestSaveInfo>();
        if (save is null)
            return;
        ////////////////////////
        Player player = Main.LocalPlayer;
        if (Math.Abs(i - (int)(player.Center.X / 16f)) > 25 ||
            Math.Abs(j - (int)(player.Center.Y / 16f)) > 25)
                return;

        ////////////////////////
        var table = QM.GetQuestsTable();
        ////////////////////////
        var qIdxs = QPUtils.GetActualQuestTable(QT.QDestroyAny);
        foreach (var id in qIdxs)
        {
            var quest = table[id].QDestroyAny;
            quest.Invoke(type);
        }
        ////////////////////////
        qIdxs = QPUtils.GetActualQuestTable(QT.QDestroyCount);
        save.QuestInfoDestroyCount++;
        foreach (var id in qIdxs)
        {
            var quest = table[id].QDestroyCount;
            quest.Invoke(save.QuestInfoDestroyCount);
        }
    }
}

public class QuestProgressCheckerGlobalNPC : GlobalNPC
{
    public override void OnKill(NPC npc)
    {
        int pWhoAmI = npc.lastInteraction;
        if (pWhoAmI == 255)
            return;
        if (Main.dedServ)
            Utils.UtilsModPacketPrimarySourceServer.Send_QuestSyncOnKillNPC(pWhoAmI, npc.whoAmI);
        else
            SyncOnKillNPC(pWhoAmI, npc.whoAmI);
    }
    public static void SyncOnKillNPC(int pWhoAmI, int npcWhoAmI)
    {
        var modPlayer = Main.player[pWhoAmI].GetModPlayer<QuestProgressCheckerModPlayer>();
        modPlayer.OnKillNPC(npcWhoAmI);
    }
}