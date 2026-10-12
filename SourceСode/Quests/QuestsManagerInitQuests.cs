using Microsoft.Xna.Framework.Graphics;
using ReLogic.Content;
using System;
using System.Collections.Generic;
using System.Reflection;
using Terraria;
using Terraria.GameContent;
using Terraria.ID;
using Terraria.Localization;
using LT = Terraria.Localization.LocalizedText;
using TS = TalentsSystem.TalentsSystem;
using UL = TalentsSystem.Utils.UtilsLocalization;

namespace TalentsSystem;
public partial class QuestsManager
{
    delegate void QuestInitializator(
            int id,
            Func<int, LT> lang,
            int? icon,
            bool sync);
    //////////////////////
    public Dictionary<int, Quest> QuestsTable = null;
    public static Dictionary<int, Quest> GetQuestsTable() => Instance.QuestsTable;
    //////////////////////
    protected int CurrentInitQuest = 0;
    protected bool CurrentInitQuestSync;
    protected int[] CurrentInitTypes = null;
    protected Condition CurrentInitCondition = null;
    protected int? CurrentIconItemId;
    //////////////////////
    protected Func<int, LT> InitLangConverter = null;
    //////////////////////
    protected void InitializationQuests()
    {
        QuestsTable = new();
        /////////////////////
        InitLangConverter = (type) => LT.Empty;
        //////////////////////
        Action<int[]> typesSetter = (array) => { CurrentInitTypes = array; };
        QuestInitializator quest = (id, lang, icon, sync) =>
        {
            CurrentInitTypes = null;
            CurrentInitCondition = null;
            CurrentInitQuest = id;
            CurrentIconItemId = icon;
            CurrentInitQuestSync = sync;
            InitLangConverter = lang;
        };
        //////////////////////


        quest(id: QuestID.Wood, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.Wood]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.Death, lang: null, icon: ItemID.RichGravestone3, sync: false);
        CreateQuestOther(QuestInfoOther.OtherType.PlayerDeath, UL.Quest_Other_PlayerDeath);
        QuestsTable[CurrentInitQuest].QOther.PlayerInvoke = (p) => p.dead;

        //////////////////////
        
        quest(id: QuestID.AnyEnemy, null, icon: ItemID.ZombieBanner, sync: false);
        CreateQuestKillAny(count: 1);
        //////////////////////
        quest(id: QuestID.CopperCoin, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.CopperCoin]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.Stone, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.StoneBlock]);
        CreateQuestItemAnyReceive(count: 1);

        //////////////////////

        quest(id: QuestID.Workbench, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.WorkBench]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.Bomb, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.Bomb]);
        CreateQuestItemAnyReceive(count: 1);

        //////////////////////

        quest(id: QuestID.Stove, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.Furnace]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.Iron, lang: Lang.GetItemName, icon: ItemID.IronBar, sync: true);
        typesSetter([ItemID.IronOre, ItemID.LeadOre]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.Anvil, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.IronAnvil, ItemID.LeadAnvil]);
        CreateQuestItemAnyReceive(count: 1);

        //////////////////////

        quest(id: QuestID.LesserHealPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.LesserHealingPotion]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.LesserManaPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.LesserManaPotion]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.ObjDestroy_100, lang: null, icon: ItemID.DirtWall, sync: false);
        CreateQuestDestroyCount(count: 100);

        //////////////////////

        quest(id: QuestID.Gold, lang: Lang.GetItemName, icon: ItemID.GoldBar, sync: true);
        typesSetter([ItemID.GoldOre, ItemID.PlatinumOre]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.GoldCoin, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.GoldCoin]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.MinersHelmet, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.MiningHelmet]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.TravelingMerchant, lang: Lang.GetNPCName, icon: ItemID.PeddlersHat, sync: false);
        typesSetter([NPCID.TravellingMerchant]);
        CreateQuestTalkNPC();

        //////////////////////

        quest(id: QuestID.LifeCrystal, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.LifeCrystal]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.ManaCrystal, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.ManaCrystal]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.Graveyard, lang: null, icon: ItemID.Tombstone, sync: false);
        CreateQuestVisited(Condition.InGraveyard);

        //////////////////////

        quest(id: QuestID.Mirror, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.MagicMirror, ItemID.IceMirror]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.BootsHermes, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.HermesBoots, ItemID.SailfishBoots, ItemID.FlurryBoots, ItemID.SandBoots]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.HealPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.HealingPotion]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.ManaPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.ManaPotion]);
        CreateQuestItemUse();
        //////////////////////
        quest(id: QuestID.SkeletonMerchant, lang: Lang.GetNPCName, icon: ItemID.Radar, sync: false);
        typesSetter([NPCID.SkeletonMerchant]);
        CreateQuestTalkNPC();

        //////////////////////

        quest(id: QuestID.ShadowSphere, lang: null, icon: ItemID.ShadowOrb, sync: false);
        typesSetter([TileID.ShadowOrbs]);
        CreateQuestDestroyAny([Lang.GetItemName(ItemID.ShadowOrb)]);
        //////////////////////
        quest(id: QuestID.SporeJungle, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.JungleSpores]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.ObjDestroy_2000, lang: null, icon: ItemID.StoneWall, sync: false);
        CreateQuestDestroyCount(count: 2000);

        //////////////////////

        quest(id: QuestID.SlimesKing, lang: Lang.GetNPCName, icon: ItemID.KingSlimeBossBag, sync: true);
        typesSetter([NPCID.KingSlime]);
        CreateQuestKillBossAny();
        //////////////////////
        quest(id: QuestID.GoblinsDefeat, lang: null, icon: ItemID.GoblinBattleStandard, sync: false);
        CreateQuestEvent(() => Main.invasionType == InvasionID.GoblinArmy, "Bestiary_Invasions.Goblins");
        //////////////////////
        quest(id: QuestID.MaxLifeCrystal, lang: Lang.GetItemName, icon: ItemID.AegisCrystal, sync: false);
        typesSetter([ItemID.LifeCrystal]);
        CreateQuestItemUseMax();
        GetCurrentInitQuest().QItemUse.InvokeCondition = (_) =>
        {
            Player player = Main.LocalPlayer;
            if (player is null)
                return false;
            return player.ConsumedLifeCrystals == Player.LifeCrystalMax - 1;
        };
        //////////////////////
        quest(id: QuestID.MaxManaCrystal, lang: Lang.GetItemName, icon: ItemID.ArcaneCrystal, sync: false);
        typesSetter([ItemID.ManaCrystal]);
        CreateQuestItemUseMax();
        GetCurrentInitQuest().QItemUse.InvokeCondition = (_) =>
        {
            Player player = Main.LocalPlayer;
            if (player is null)
                return false;
            return player.ConsumedManaCrystals == Player.ManaCrystalMax - 1;
        };
        //////////////////////
        quest(id: QuestID.MiniShark, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.Minishark]);
        CreateQuestItemAnyReceive(count: 1);

        //////////////////////

        quest(id: QuestID.EyeCthulhu, lang: Lang.GetNPCName, icon: ItemID.EyeOfCthulhuBossBag, sync: true);
        typesSetter([NPCID.EyeofCthulhu]);
        CreateQuestKillBossAny();
        //////////////////////
        quest(id: QuestID.BloodMoon, lang: null, icon: ItemID.BloodZombieBanner, sync: false);
        CreateQuestEvent(() => Main.bloodMoon, "Bestiary_Events.BloodMoon");
        /////////////////////
        quest(id: QuestID.PurificationPowder, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.PurificationPowder]);
        CreateQuestItemUse();

        //////////////////////

        
        if (TS.CalamityLoaded)
        {
            quest(id: QuestID.DownedBoss2, lang: Lang.GetItemName, icon: ItemID.BrainOfCthulhuBossBag, sync: true);
            typesSetter([ItemID.TissueSample, ItemID.ShadowScale]);
            CreateQuestItemAnyReceive(1);
        }
        else
        {
            quest(id: QuestID.DownedBoss2, lang: Lang.GetNPCName, icon: ItemID.BrainOfCthulhuBossBag, sync: true);
            typesSetter([NPCID.BrainofCthulhu, NPCID.EaterofWorldsHead, NPCID.EaterofWorldsBody, NPCID.EaterofWorldsTail]);
            CreateQuestKillBossAny();
            GetCurrentInitQuest().QKillBossAny.InvokeCondition = (target) =>
            {
                if (target.type != NPCID.EaterofWorldsHead &&
                    target.type != NPCID.EaterofWorldsBody &&
                    target.type != NPCID.EaterofWorldsTail)
                    return true;
                ////////////////////////////////
                foreach (NPC other in Main.npc)
                {
                    if (!other.active || other.whoAmI == target.whoAmI)
                        continue;
                    if (other.type == NPCID.EaterofWorldsHead ||
                        other.type == NPCID.EaterofWorldsBody ||
                        other.type == NPCID.EaterofWorldsTail)
                        return false;
                }
                return true;
            };
            GetCurrentInitQuest().OverrideDescriptionArgsGetter = ()
                => [$"{Lang.GetNPCName(NPCID.EaterofWorldsHead)}; {Lang.GetNPCName(NPCID.BrainofCthulhu)}"];
        }
        //////////////////////
        quest(id: QuestID.MoltenPickaxe, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.MoltenPickaxe]);
        CreateQuestItemAnyReceive(count: 1);
        //////////////////////
        quest(id: QuestID.HellForge, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.Hellforge]);
        CreateQuestItemAnyReceive(count: 1);

        //////////////////////

        quest(id: QuestID.OldArmy1, lang: null, icon: ItemID.DD2ElderCrystalStand, sync: false);
        CreateQuestEvent_OldArmy(tier: 1);
        //////////////////////
        quest(id: QuestID.Meteorite, lang: null, icon: ItemID.MeteoriteBar, sync: false);
        CreateQuestVisited(Condition.InMeteor);

        //////////////////////

        quest(id: QuestID.QueenBee, lang: Lang.GetNPCName, icon: ItemID.QueenBeeBossBag, sync: true);
        typesSetter([NPCID.QueenBee]);
        CreateQuestKillBossAny();
        //////////////////////
        quest(id: QuestID.Shimmer, lang: null, icon: ItemID.ShimmerBlock, sync: false);
        CreateQuestVisited(
            new Condition(
                UL.Condtions_InShimmeringLake,
                () => Main.LocalPlayer.shimmering));

        //////////////////////

        quest(id: QuestID.DowndeSkeletron, lang: Lang.GetNPCName, icon: ItemID.SkeletronBossBag, sync: true);
        typesSetter([NPCID.SkeletronHead]);
        CreateQuestKillBossAny();

        //////////////////////

        quest(id: QuestID.GoldKey, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.GoldenKey]);
        CreateQuestItemAnyReceive(1);
        //////////////////////
        quest(id: QuestID.Bones250, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.Bone]);
        CreateQuestItemAnyReceive(250);
        //////////////////////
        quest(id: QuestID.VoidBag, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.VoidLens, ItemID.ClosedVoidBag]);
        CreateQuestItemAnyReceive(1);

        //////////////////////
        
        quest(id: QuestID.AlchemyTable, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.AlchemyTable]);
        CreateQuestItemAnyReceive(1);
        //////////////////////
        quest(id: QuestID.BewitchingTable, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.BewitchingTable]);
        CreateQuestItemAnyReceive(1);
        //////////////////////
        quest(id: QuestID.Mechanic, lang: Lang.GetNPCName, icon: ItemID.Wire, sync: false);
        typesSetter([NPCID.Mechanic]);
        CreateQuestTalkNPC();

        ///////////////////////

        quest(id: QuestID.KillClothier, lang: Lang.GetNPCName, icon: ItemID.ClothierVoodooDoll, sync: true);
        typesSetter([NPCID.Clothier]);
        CreateQuestKillTownNPC();
        //////////////////////
        quest(id: QuestID.ShadowKey, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.ShadowKey]);
        CreateQuestItemAnyReceive(1);
        //////////////////////
        quest(id: QuestID.ShadowChest, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.HellwingBow, ItemID.Sunfury, ItemID.FlowerofFire]);
        CreateQuestItemAnyReceive(1);

        ////////////////////////

        quest(id: QuestID.ObjDestroy_10_000, lang: null, icon: ItemID.BlueSlabWall, sync: false);
        CreateQuestDestroyCount(count: 10000);
        //////////////////////
        quest(id: QuestID.NightsEdge, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.NightsEdge]);
        CreateQuestItemAnyReceive(1);
        //////////////////////
        quest(id: QuestID.TerrasparkBoots, lang: Lang.GetItemName, icon: null, sync: true);
        typesSetter([ItemID.TerrasparkBoots]);
        CreateQuestItemAnyReceive(1);

        /////////////////////////

        quest(id: QuestID.WallofFlesh, lang: Lang.GetNPCName, icon: ItemID.WallOfFleshBossBag, sync: true);
        typesSetter([NPCID.WallofFlesh]);
        CreateQuestKillBossAny();

        ///////////////////////////

        quest(id: QuestID.SoulofLight, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.SoulofLight]);
        CreateQuestItemAnyReceive(15);
        GetCurrentInitQuest().Icon = AssetsManager.Instance.GetAssetVanilaFirstFrame(AssetsManager.VanilaFirtFrameTexture.SoulofLight);
        //////////////////////////
        quest(id: QuestID.SoulofNight, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.SoulofNight]);
        CreateQuestItemAnyReceive(15);
        GetCurrentInitQuest().Icon = AssetsManager.Instance.GetAssetVanilaFirstFrame(AssetsManager.VanilaFirtFrameTexture.SoulofNight);
        /////////////////////////
        quest(id: QuestID.CobaltPickaxe, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.CobaltPickaxe, ItemID.PalladiumPickaxe]);
        CreateQuestItemAnyReceive(1);
        ////////////////////////
        quest(id: QuestID.CrystalShard, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.CrystalShard]);
        CreateQuestItemAnyReceive(10);
        ////////////////////////
        quest(id: QuestID.DemonAltar, lang: Lang.GetItemName, icon: ItemID.Pwnhammer, sync: true);
        typesSetter([TileID.DemonAltar]);
        CreateQuestDestroyAny([Language.GetTextValue(WorldGen.crimson ? "MapObject.CrimsonAltar" : "MapObject.DemonAltar")]);

        ////////////////////////

        quest(id: QuestID.Wizard, lang: Lang.GetNPCName, icon: ItemID.CrystalBall, sync: false);
        typesSetter([NPCID.Wizard]);
        CreateQuestTalkNPC();
        ////////////////////////
        quest(id: QuestID.GreaterHealingPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.GreaterHealingPotion]);
        CreateQuestItemUse();
        ////////////////////////
        quest(id: QuestID.GreaterManaPotion, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.GreaterManaPotion]);
        CreateQuestItemUse();
        ////////////////////////
        quest(id: QuestID.PlatinumCoin, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.PlatinumCoin]);
        CreateQuestItemAnyReceive(1);

        ////////////////////////

        quest(id: QuestID.Mimic, lang: Lang.GetNPCName, icon: ItemID.GoldChest, sync: true);
        typesSetter([NPCID.Mimic, NPCID.IceMimic, NPCID.PresentMimic]);
        CreateQuestKillAnyType();
        GetCurrentInitQuest().OverrideDescriptionArgsGetter = () => [Lang.GetNPCName(NPCID.Mimic)];
        ////////////////////////
        quest(id: QuestID.Unicorn, lang: Lang.GetNPCName, icon: ItemID.BlessedApple, sync: true);
        typesSetter([NPCID.Unicorn]);
        CreateQuestKillAnyType();
        ////////////////////////
        quest(id: QuestID.FloatyGross, lang: Lang.GetNPCName, icon: ItemID.CursedFlame, sync: true);
        //typesSetter([NPCID.FloatyGross, NPCID.SeekerHead, NPCID.SeekerBody, NPCID.SeekerTail]);
        typesSetter([NPCID.FloatyGross, NPCID.Clinger]);
        CreateQuestKillAnyType();
        //GetCurrentInitQuest().OverrideDescriptionArgsGetter = () => [$"{Lang.GetNPCName(NPCID.FloatyGross)}; {Lang.GetNPCName(NPCID.SeekerHead)}"];
        ////////////////////////
        quest(id: QuestID.MythrilAnvil, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.MythrilAnvil, ItemID.OrichalcumAnvil]);
        CreateQuestItemAnyReceive(1);
        ////////////////////////
        quest(id: QuestID.AdamantiteForge, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.AdamantiteForge, ItemID.TitaniumForge]);
        CreateQuestItemAnyReceive(1);

        ////////////////////////

        quest(id: QuestID.BigMimic, lang: Lang.GetNPCName, icon: ItemID.CorruptionChest, sync: true);
        typesSetter([NPCID.BigMimicCorruption, NPCID.BigMimicCrimson, NPCID.BigMimicHallow]);
        CreateQuestKillAnyType();
        ////////////////////////
        quest(id: QuestID.SoulofFlight, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.SoulofFlight]);
        CreateQuestItemAnyReceive(1);
        GetCurrentInitQuest().Icon = AssetsManager.Instance.GetAssetVanilaFirstFrame(AssetsManager.VanilaFirtFrameTexture.SoulofFlight);
        ////////////////////////
        quest(id: QuestID.Wings, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.DemonWings, ItemID.AngelWings]);
        CreateQuestItemAnyReceive(1);
        ///////////////////////
        quest(id: QuestID.Pirates, lang: null, icon: ItemID.PirateMap, sync: false);
        CreateQuestEvent(() => Main.invasionType == InvasionID.PirateInvasion, "Bestiary_Invasions.Pirates");

        ////////////////////////

        quest(id: QuestID.HallowedPlateMail, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.HallowedPlateMail, ItemID.AncientHallowedPlateMail]);
        CreateQuestItemAnyReceive(1);
        ////////////////////////
        quest(id: QuestID.Clentaminator, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.Clentaminator]);
        CreateQuestItemUse();
        ////////////////////////
        quest(id: QuestID.LifeFruit, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.LifeFruit]);
        CreateQuestItemUse();
        ////////////////////////
        quest(id: QuestID.OldArmy2, lang: null, icon: ItemID.DD2ElderCrystalStand, sync: false);
        CreateQuestEvent_OldArmy(tier: 2);

        ////////////////////////

        quest(id: QuestID.LifeFruitMax, lang: Lang.GetItemName, icon: ItemID.AegisFruit, sync: false);
        typesSetter([ItemID.LifeFruit]);
        CreateQuestItemUseMax();
        GetCurrentInitQuest().QItemUse.InvokeCondition = (_) =>
        {
            Player player = Main.LocalPlayer;
            if (player is null)
                return false;
            return player.ConsumedLifeFruit == Player.LifeFruitMax - 1;
        };
        ////////////////////////
        quest(id: QuestID.Destroyer, lang: Lang.GetNPCName, icon: ItemID.DestroyerBossBag, sync: true);
        typesSetter([NPCID.TheDestroyer]);
        CreateQuestKillBossAny();
        ////////////////////////
        quest(id: QuestID.Twins, lang: Lang.GetNPCName, icon: ItemID.TwinsBossBag, sync: true);
        typesSetter([NPCID.Retinazer, NPCID.Spazmatism]);
        CreateQuestKillBossAny();
        GetCurrentInitQuest().QKillBossAny.InvokeCondition = (target) =>
        {
            if (target.type != NPCID.Retinazer &&
                target.type != NPCID.Spazmatism)
                return true;
            ////////////////////////////////
            foreach (NPC other in Main.npc)
            {
                if (!other.active || other.whoAmI == target.whoAmI)
                    continue;
                if (other.type == NPCID.Retinazer ||
                    other.type == NPCID.Spazmatism)
                    return false;
            }
            return true;
        };
        GetCurrentInitQuest().OverrideDescriptionArgsGetter = () => [Language.GetTextValue("Enemies.TheTwins")];
        ////////////////////////
        quest(id: QuestID.SkeletronPrime, lang: Lang.GetNPCName, icon: ItemID.SkeletronPrimeBossBag, sync: true);
        typesSetter([NPCID.SkeletronPrime]);
        CreateQuestKillBossAny();

        ////////////////////////

        quest(id: QuestID.PickaxeAxe, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.PickaxeAxe, ItemID.Drax]);
        CreateQuestItemAnyReceive(1);
        ////////////////////////
        quest(id: QuestID.AvengerEmblem, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.AvengerEmblem]);
        CreateQuestItemAnyReceive(1);
        ////////////////////////
        quest(id: QuestID.ChlorophyteOre, lang: Lang.GetItemName, icon: ItemID.ChlorophyteBar, sync: false);
        typesSetter([ItemID.ChlorophyteOre]);
        CreateQuestItemAnyReceive(1);

        ////////////////////////
        
        quest(id: QuestID.Plantera, lang: Lang.GetNPCName, icon: ItemID.PlanteraBossBag, sync: true);
        typesSetter([NPCID.Plantera]);
        CreateQuestKillBossAny();

        ////////////////////////

        quest(id: QuestID.SolarEclipse, lang: null, icon: ItemID.SolarTable, sync: false);
        CreateQuestEvent(() => Main.eclipse, "LegacyMisc.20");
        //////////
        quest(id: QuestID.OldArmy3, lang: null, icon: ItemID.DD2ElderCrystalStand, sync: false);
        CreateQuestEvent_OldArmy(tier: 3);

        ////////////////////////

        quest(id: QuestID.PumpkinMoon, lang: null, icon: ItemID.PumpkinMoonMedallion, sync: false);
        CreateQuestEvent(() => Main.pumpkinMoon, "Bestiary_Invasions.PumpkinMoon");

        ////////////////////////
        
        quest(id: QuestID.FrostMoon, lang: null, icon: ItemID.NaughtyPresent, sync: false);
        CreateQuestEvent(() => Main.snowMoon, "Bestiary_Invasions.FrostMoon");
        ////////////////////////
        quest(id: QuestID.SnowLegion, lang: null, icon: ItemID.SnowGlobe, sync: false);
        CreateQuestEvent(() => Main.invasionType == InvasionID.SnowLegion, "Bestiary_Invasions.FrostLegion");

        ////////////////////////

        quest(id: QuestID.Golem, lang: Lang.GetNPCName, icon: ItemID.GolemBossBag, sync: true);
        typesSetter([NPCID.Golem]);
        CreateQuestKillBossAny();

        ////////////////////////
        
        quest(id: QuestID.MartianMadness, lang: null, icon: ItemID.CosmicCarKey, sync: false);
        CreateQuestEvent(() => Main.invasionType == InvasionID.MartianMadness, "Bestiary_Invasions.Martian");

        ///////////////////////

        quest(id: QuestID.Cultist, lang: Lang.GetNPCName, icon: ItemID.CultistBossBag, sync: true);
        typesSetter([NPCID.CultistBoss]);
        CreateQuestKillBossAny();
        ///////////////////////
        //quest(id: QuestID.ObjDestroy_25_000, lang: null, icon: ItemID.LunarBrickWall, sync: false);
        //CreateQuestDestroyCount(count: 25000);

        /////////////////////////////////

        quest(id: QuestID.Сelestial, lang: Lang.GetItemName, icon: null, sync: false);
        typesSetter([ItemID.FragmentNebula, ItemID.FragmentSolar, ItemID.FragmentStardust, ItemID.FragmentVortex]);
        CreateQuestItemAllReceive(15);
        GetCurrentInitQuest().OverrideDescriptionArgsGetter = () => [Language.GetTextValue("LegacyMisc.51"), 15];

        /////////////////////////////////

        foreach (var q in QuestsTable)
            q.Value.Initialization();

        ////////////////////////////////

        CurrentInitTypes = null;
        CurrentInitCondition = null;
        InitLangConverter = null;
    }
    protected Quest InitializationQuest()
    {
        LT[] args;
        if (InitLangConverter is not null)
        {
            int l = CurrentInitTypes is null ? 0 : CurrentInitTypes.Length;
            args = new LT[l];
            for (int i = 0; i < l; i++)
                args[i] = InitLangConverter(CurrentInitTypes[i]);
        }
        else
            args = new LT[0];
        //////////////////////
        if (CurrentIconItemId is null)
        {
            if (CurrentInitTypes is null || CurrentInitTypes.Length < 1)
                throw new Exception($"[{TS.GetDisplayName()}] QuestCreater -> CurrentIconItemId is null");
            CurrentIconItemId = CurrentInitTypes[0];
        }
        //////////////////////
        AllQuestsArray.Add(CurrentInitQuest);
        QuestsTable[CurrentInitQuest] =
            new Quest(
                CurrentInitQuest,
                (int)CurrentIconItemId,
                CurrentInitQuestSync,
                args);
        return GetCurrentInitQuest();
    }
    protected Quest GetCurrentInitQuest() => QuestsTable[CurrentInitQuest];

    protected void SetQuestDescriptionByCount(int count, LT lt, LT ltCounter)
    {
        var quest = GetCurrentInitQuest();
        if (count < 2)
            quest.Description = lt;
        else
        {
            quest.Description = ltCounter;
            quest.AddDescriptionArgs(count);
        }
    }

    protected void CreateQuestItemAnyReceive(int count)
    {
        QuestInfoItemAnyReceive q = new(CurrentInitQuest);
        q.Count = count;
        q.Types = CurrentInitTypes;
        InitializationQuest().QItemAnyRecive = q;

        SetQuestDescriptionByCount(count, UL.Quest_Items_Any_Receive, UL.Quest_Items_Any_Receive_Count);
    }
    protected void CreateQuestItemAllReceive(int count)
    {
        QuestInfoItemAllReceive q = new(CurrentInitQuest);
        q.Count = count;
        q.Types = CurrentInitTypes;
        InitializationQuest().QItemAllRecive = q;

        SetQuestDescriptionByCount(count, UL.Quest_Items_All_Receive, UL.Quest_Items_All_Receive_Count);
    }
    protected void CreateQuestItemUse_OverrideDescription(LT description)
    {
        QuestInfoItemUse q = new(CurrentInitQuest);
        q.Types = CurrentInitTypes;
        var quest = InitializationQuest();
        quest.QItemUse = q;
        quest.Description = description;
    }
    protected void CreateQuestItemUse()
        => CreateQuestItemUse_OverrideDescription(UL.Quest_Items_Use);
    protected void CreateQuestItemUseMax()
        => CreateQuestItemUse_OverrideDescription(UL.Quest_Items_Use_Max);
    protected void CreateQuestKillAny(int count)
    {
        QuestInfoKillAny q = new(CurrentInitQuest);
        q.Count = count;
        InitializationQuest().QKillAny = q;

        SetQuestDescriptionByCount(count, UL.Quest_Kill_Any, UL.Quest_Kill_Any_Count);
    }
    protected void CreateQuestKillAnyType()
    { 
        QuestInfoKillAnyType q = new(CurrentInitQuest);
        q.Types = CurrentInitTypes;
        var quest = InitializationQuest();
        quest.QKillAnyType = q;
        quest.Description = UL.Quest_Kill_Any_Type;
    }
    protected void CreateQuestKillBossAny()
    {
        QuestInfoKillBossAny q = new(CurrentInitQuest);
        q.Types = CurrentInitTypes;
        var quest = InitializationQuest();
        quest.QKillBossAny = q;
        quest.Description = UL.Quest_Kill_Boss;
    }
    protected void CreateQuestKillTownNPC()
    {
        QuestInfoKillTownNPC q = new(CurrentInitQuest);
        q.Type = CurrentInitTypes[0];
        var quest = InitializationQuest();
        quest.QKillTownNPC = q;
        quest.Description = UL.Quest_Kill_TownNPC;
    }
    protected void CreateQuestTalkNPC()
    {
        QuestInfoTalkNPC q = new(CurrentInitQuest);
        q.Type = CurrentInitTypes[0];
        var quest = InitializationQuest();
        quest.QTalkNPC = q;
        quest.Description = UL.Quest_TalkNPC;
    }
    protected void CreateQuestDestroyCount(int count)
    {
        QuestInfoDestroyCount q = new(CurrentInitQuest);
        q.Count = count;
        InitializationQuest().QDestroyCount = q;

        SetQuestDescriptionByCount(count, UL.Quest_Destroy_AnyObject, UL.Quest_Destroy_AnyObject_Count);
    }
    protected void CreateQuestDestroyAny(object[] descriptionArgs)
    {
        QuestInfoDestroyAny q = new(CurrentInitQuest);
        q.Types = CurrentInitTypes;
        var quest = InitializationQuest();
        quest.QDestroyAny = q;
        quest.Description = UL.Quest_Destroy_Object;
        quest.AddDescriptionArgs(descriptionArgs);
    }
    protected void CreateQuestEvent(
        Func<bool> invokeCondition, 
        string eventLangKey) 
    {
        QuestInfoEvent q = new(CurrentInitQuest, invokeCondition);
        var quest = InitializationQuest();
        quest.QEvent = q;

        quest.Description = UL.Quest_Events_EndEvent;
        quest.AddDescriptionArgs(Language.GetText(eventLangKey));
    }
    protected void CreateQuestEvent_OldArmy(int tier)
    {
        QuestInfoEventOldArmy q = new(CurrentInitQuest, tier);
        var quest = InitializationQuest();
        quest.QEventOldArmy = q;

        quest.Description = UL.Quest_Events_EndEventOldArmy;
        quest.AddDescriptionArgs([Language.GetText("Bestiary_Invasions.OldOnesArmy"), tier]);
    }
    protected void CreateQuestVisited(Condition condition)
    {
        QuestInfoVisited q = new(CurrentInitQuest, condition);
        var quest = InitializationQuest();
        quest.QVisited = q;
        quest.Description = UL.Quest_Visited_Zone;
        quest.AddDescriptionArgs(condition.Description);
    }
    protected void CreateQuestOther(QuestInfoOther.OtherType type, LT description)
    {
        var quest = InitializationQuest();
        quest.QOther = new(CurrentInitQuest, type);
        quest.Description = description;
    }
}

public class Quest
{
    public enum QuestType
    {
        QKillAny,
        QKillAnyType,
        QKillBossAny,
        QKillTownNPC,
        ////////////////////
        QTalkNPC,
        ////////////////////
        QItemAnyRecive,
        QItemAllRecive,
        QItemUse,
        ////////////////////
        QDestroyCount,
        QDestroyAny,
        ////////////////////
        QVisited,
        ////////////////////
        QEvent,
        QEventOldArmy,
        ////////////////////
        QOther,
    }

    public readonly int ID;
    public readonly int Exp;
    public readonly bool Sync;
    public QuestType Type { get; protected set; }

    ////////////////////

    public Func<LT> OverrideDescriptionGetter { protected get; set; }
    public Func<object[]> OverrideDescriptionArgsGetter { protected get; set; }
    public LT Description { protected get; set; }
    protected object[] DescriptionArgs;
    protected object[] DescriptionAdditionArgs;

    ////////////////////

    public Asset<Texture2D> Icon;

    ////////////////////

    public QuestInfoKillAny QKillAny = null;
    public QuestInfoKillAnyType QKillAnyType = null;
    public QuestInfoKillBossAny QKillBossAny = null;
    public QuestInfoKillTownNPC QKillTownNPC = null;
    ////////////////////
    public QuestInfoTalkNPC QTalkNPC = null;
    ////////////////////
    public QuestInfoItemAnyReceive QItemAnyRecive = null;
    public QuestInfoItemAllReceive QItemAllRecive = null;
    public QuestInfoItemUse QItemUse = null;
    ////////////////////
    public QuestInfoDestroyCount QDestroyCount = null;
    public QuestInfoDestroyAny QDestroyAny = null;
    ////////////////////
    public QuestInfoVisited QVisited = null;
    ////////////////////
    public QuestInfoEvent QEvent = null;
    public QuestInfoEventOldArmy QEventOldArmy = null;
    ////////////////////
    public QuestInfoOther QOther = null;

    ////////////////////

    public Quest(int id, int iconItemId, bool sync, object[] descriptionArgs)
    {
        ID = id;
        Exp = QuestsManager.Instance.QuestExpTable[id];
        Sync = sync;
        DescriptionArgs = descriptionArgs;
        if (Main.netMode != NetmodeID.Server && iconItemId != -1)
        {
            Main.instance.LoadItem(iconItemId);
            Icon = TextureAssets.Item[iconItemId];
        }
    }
    public LT GetDescription()
    {
        if (OverrideDescriptionGetter is not null)
            return OverrideDescriptionGetter();
        else if(OverrideDescriptionArgsGetter is not null)
            return Description.WithFormatArgs(OverrideDescriptionArgsGetter());
        else
            return Description.WithFormatArgs(DescriptionArgs);
    }
    public void Initialization()
    {
        if (DescriptionArgs is null)
            DescriptionArgs = new object[0];
        if(DescriptionAdditionArgs is null)
            DescriptionAdditionArgs = new object[0];
        ////////////////////
        int length1 = DescriptionArgs.Length;
        int length2 = DescriptionAdditionArgs.Length;
        string firstArg = string.Empty;
        for (int i = 0; i < length1; i++)
            firstArg += DescriptionArgs[i].ToString() + "; ";
        firstArg = firstArg.TrimEnd(' ').TrimEnd(';');
        ////////////////////
        length1 = firstArg == string.Empty ? 0 : 1;
        DescriptionArgs = new object[length1 + length2];
        if (length1 == 0)
            DescriptionArgs = DescriptionAdditionArgs;
        else
        {
            DescriptionArgs[0] = firstArg;
            for (int i = 0; i < length2; i++)
                DescriptionArgs[i + 1] = DescriptionAdditionArgs[i];
        }
        DescriptionAdditionArgs = null;

        ////////////////

        foreach (QuestType questType in Enum.GetValues<QuestType>())
        {
            FieldInfo field = this.GetType().GetField(
                questType.ToString(),
                BindingFlags.Instance |
                BindingFlags.Public |
                BindingFlags.NonPublic);
            ////////////////////
            if (field?.GetValue(this) is not null)
            {
                Type = questType;
                var table = QuestsManager.Instance.SortedQuestTable;
                if (!table.ContainsKey(Type))
                    table[Type] = new();
                table[Type].Add(ID);
                return;
            }
        }
    }

    public void AddDescriptionArgs(params object[] args)
    {
        int length1 = DescriptionAdditionArgs is null ? 0 : DescriptionAdditionArgs.Length;
        int length2 = args.Length;
        object[] updArgs = new object[length1 + length2];
        for(int i = 0; i < length1; i++)
            updArgs[i] = DescriptionAdditionArgs[i];
        for (int i = 0; i < length2; i++)
            updArgs[i + length1] = args[i];
        DescriptionAdditionArgs = updArgs;
    }
 }

public abstract class QuestInfoBase
{
    public int ID;    
    public QuestInfoBase(int id)
        => ID = id;
    protected void Complete()
    {
        var save = TS.GetLocal<QuestSaveInfo>();
        if (save is not null)
            save.CompleteQuest(ID);
    }

    protected bool IsContainsType(int[] types, int type)
    {
        if (ContainerEmpty(types))
            return false;
        ////////////////////
        if (types is not null)
            foreach (var t in types)
                if (t == type)
                    return true;
        ////////////////////
        return false;
    }
    protected bool ContainerEmpty(int[] types)
        => types is null || types.Length == 0;
}

public class QuestInfoTalkNPC : QuestInfoBase
{
    public int Type;
    public QuestInfoTalkNPC(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (Type == type)
            Complete();
    }
}

public class QuestInfoKillAny : QuestInfoBase
{
    public int Count = 0;
    public QuestInfoKillAny(int id) : base(id) { }
    public void Invoke(int currentCount)
    {
        if (Count <= currentCount)
            Complete();
    }
}
public class QuestInfoKillAnyType : QuestInfoBase
{
    public Func<int, bool> InvokeCondition = (_) => true;
    public int[] Types;
    public QuestInfoKillAnyType(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (!InvokeCondition(type))
            return;
        if (!IsContainsType(Types, type))
            return;
        Complete();
    }
}
public class QuestInfoKillBossAny : QuestInfoBase
{
    public Func<NPC, bool> InvokeCondition = (_) => true;
    public int[] Types;
    public QuestInfoKillBossAny(int id) : base(id) { }
    public void Invoke(NPC target)
    {
        if (!InvokeCondition(target))
            return;
        if (!IsContainsType(Types, target.type))
            return;
        Complete();
    }
}

public class QuestInfoKillTownNPC : QuestInfoBase
{
    public int Type;
    public QuestInfoKillTownNPC(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (Type == type)
            Complete();
    }
}

public abstract class QuestInfoItemBase : QuestInfoBase
{
    public QuestInfoItemBase(int id) : base(id) { }
    protected bool IsPlayerAcitve()
        => Main.LocalPlayer is not null && Main.LocalPlayer.active;
    protected int GetItemCount(int type)
    {
        if (!IsPlayerAcitve())
            return 0;
        int itemCount = 0;
        foreach (Item item in Main.LocalPlayer.inventory)
            if (item.type == type)
                itemCount += item.stack;
        foreach (Item item in Main.LocalPlayer.bank4.item)
            if (item.type == type)
                itemCount += item.stack;
        return itemCount;
    }
}
public class QuestInfoItemAnyReceive : QuestInfoItemBase
{
    public Func<int, bool> InvokeCondition = (_) => true;
    public int Count = 0;
    public int[] Types;
    public QuestInfoItemAnyReceive(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (!InvokeCondition(type))
            return;
        if (!IsContainsType(Types, type))
            return;
        ////////////////////
        if (Count <= 1)
            Complete();
        else if (Main.LocalPlayer is not null && GetItemCount(type) >= Count)
            Complete();
    }
}
public class QuestInfoItemAllReceive : QuestInfoItemBase
{
    public Func<int, bool> InvokeCondition = (_) => true;
    public int Count = 0;
    public int[] Types;
    public QuestInfoItemAllReceive(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (!InvokeCondition(type))
            return;
        if (!IsContainsType(Types, type))
            return;
        ////////////////////
        int[] itemsCounts = new int[Types.Length];
        for (int i = 0; i < Types.Length; i++)
            itemsCounts[i] = GetItemCount(Types[i]);
        bool enout = true;
        foreach (var c in itemsCounts)
            if (c < Count)
            {
                enout = false;
                break;
            }
        if (enout)
            Complete();
    }
}
public class QuestInfoItemUse : QuestInfoItemBase
{
    public Func<int, bool> InvokeCondition = (_) => true;
    public int[] Types;
    public QuestInfoItemUse(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (!InvokeCondition(type))
            return;
        if (!IsContainsType(Types, type))
            return;
        Complete();
    }
}

public class QuestInfoDestroyCount : QuestInfoBase
{
    public int Count = 0;
    public QuestInfoDestroyCount(int id) : base(id) { }
    public void Invoke(int currentCount)
    {
        if (currentCount >= Count)
            Complete();
    }
}

public class QuestInfoDestroyAny : QuestInfoBase
{
    public int[] Types;
    public QuestInfoDestroyAny(int id) : base(id) { }
    public void Invoke(int type)
    {
        if (!IsContainsType(Types, type))
            return;
        Complete();
    }
}

public class QuestInfoVisited : QuestInfoBase
{
    public Func<Player, bool> InvokeCondition = (_) => true;
    public Condition VisitedCondtion = null;
    public QuestInfoVisited(int id, Condition visitedCondtion) : base(id) => VisitedCondtion = visitedCondtion;
    public void Invoke(Player player)
    {
        if (VisitedCondtion.IsMet() && InvokeCondition(player))
            Complete();
    }
}

public class QuestInfoEvent : QuestInfoBase
{
    protected readonly Func<bool> InvokeCondition;
    protected bool OldState;
    public QuestInfoEvent(int id, Func<bool> invokeCondition) : base(id)
    {
        InvokeCondition = invokeCondition;
    }
    public void ResetState() => OldState = false;
    public void Invoke()
    {
        bool current = InvokeCondition();
        if (OldState && !current)
            Complete();
        OldState = current;
    }
}
public class QuestInfoEventOldArmy : QuestInfoBase
{ 
    public readonly int Tier = 0;
    public QuestInfoEventOldArmy(int id, int tier) : base(id) => Tier = tier;
    public void Invoke(int tier)
    {
        if (tier >= Tier)
            Complete();
    }
}

public class QuestInfoOther : QuestInfoBase
{
    public enum OtherType
    { 
        PlayerDeath,
    }
    public Func<Player, bool> PlayerInvoke = (p) => false;
    public OtherType Type;
    public QuestInfoOther(int id, OtherType type) : base(id) { Type = type; }
    public void Invoke()
    {
        if (Main.LocalPlayer is null)
            return;
        ////////////////////
        if (Type == OtherType.PlayerDeath && PlayerInvoke(Main.LocalPlayer))
        {
            Complete();
            return;
        }
    }
}