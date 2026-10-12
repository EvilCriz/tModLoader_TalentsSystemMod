using log4net.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace TalentsSystem;

/// <summary>
/// Attribute for quest and talent IDs.
/// For quests: specifies the level to which the quest belongs. The "Points" value is ignored.
/// For talents: specifies the section to which the talent belongs. 
/// "Points" indicates the number of talent points assigned to each skill in the section 
/// (only the first attribute in each section must have a value other than -1; otherwise, the value will be assigned randomly).
/// </summary>
[AttributeUsage(AttributeTargets.Field)]
public class LevelAttribute : Attribute
{
    public int Level { get; }
    public int Points;
    public LevelAttribute(int level, int points = -1)
    {
        Level = level;
        Points = points;
    }
}

public static class QuestID
{
    [Level(1)]
    public const int Wood = 10;
    [Level(1)]
    public const int Death = 11;


    [Level(2)]
    public const int AnyEnemy = 20;
    [Level(2)]
    public const int CopperCoin = 21;
    [Level(2)]
    public const int Stone = 22;


    [Level(3)]
    public const int Workbench = 30;
    [Level(3)]
    public const int Bomb = 31;


    [Level(4)]
    public const int Stove = 40;
    [Level(4)]
    public const int Iron = 41;
    [Level(4)]
    public const int Anvil = 42;

    
    [Level(5)]
    public const int LesserHealPotion = 50;
    [Level(5)]
    public const int LesserManaPotion = 51;
    [Level(5)]
    public const int ObjDestroy_100 = 52;


    [Level(6)]
    public const int Gold = 60;
    [Level(6)]
    public const int GoldCoin = 61;
    [Level(6)]
    public const int MinersHelmet = 62;
    [Level(6)]
    public const int TravelingMerchant = 63;


    [Level(7)]
    public const int LifeCrystal = 70;
    [Level(7)]
    public const int ManaCrystal = 71;
    [Level(7)]
    public const int Graveyard = 72;


    [Level(8)]
    public const int Mirror = 80;
    [Level(8)]
    public const int BootsHermes = 81;
    [Level(8)]
    public const int HealPotion = 82;
    [Level(8)]
    public const int ManaPotion = 83;
    [Level(8)]
    public const int SkeletonMerchant = 84;


    [Level(9)]
    public const int ShadowSphere = 90;
    [Level(9)]
    public const int SporeJungle = 91;
    [Level(9)]
    public const int ObjDestroy_2000 = 92;


    [Level(10)]
    public const int SlimesKing = 100;
    [Level(10)]
    public const int GoblinsDefeat = 101;
    [Level(10)]
    public const int MaxLifeCrystal = 102;
    [Level(10)]
    public const int MaxManaCrystal = 103;
    [Level(10)]
    public const int MiniShark = 104;


    [Level(11)]
    public const int EyeCthulhu = 110;
    [Level(11)]
    public const int BloodMoon = 111;
    [Level(11)]
    public const int PurificationPowder = 112;


    [Level(12)]
    public const int DownedBoss2 = 120;
    [Level(12)]
    public const int MoltenPickaxe = 121;
    [Level(12)]
    public const int HellForge = 122;
    

    [Level(13)]
    public const int OldArmy1 = 130;
    [Level(13)]
    public const int Meteorite = 131;


    [Level(14)]
    public const int QueenBee = 140;
    [Level(14)]
    public const int Shimmer = 141;


    [Level(15)]
    public const int DowndeSkeletron = 150;


    [Level(16)]
    public const int GoldKey = 160;
    [Level(16)]
    public const int Bones250 = 161;
    [Level(16)]
    public const int VoidBag = 162;


    [Level(17)]
    public const int AlchemyTable = 170;
    [Level(17)]
    public const int BewitchingTable = 171;
    [Level(17)]
    public const int Mechanic = 172;


    [Level(18)]
    public const int KillClothier = 180;
    [Level(18)]
    public const int ShadowKey = 181;
    [Level(18)]
    public const int ShadowChest = 182;


    [Level(19)]
    public const int ObjDestroy_10_000 = 190;
    [Level(19)]
    public const int NightsEdge = 191;
    [Level(19)]
    public const int TerrasparkBoots = 192;


    [Level(20)]
    public const int WallofFlesh = 200;


    [Level(21)]
    public const int SoulofLight = 210;
    [Level(21)]
    public const int SoulofNight = 211;
    [Level(21)]
    public const int CobaltPickaxe = 212;
    [Level(21)]
    public const int CrystalShard = 213;
    [Level(21)]
    public const int DemonAltar = 214;


    [Level(22)]
    public const int Wizard = 220;
    [Level(22)]
    public const int GreaterHealingPotion = 221;
    [Level(22)]
    public const int GreaterManaPotion = 222;
    [Level(22)]
    public const int PlatinumCoin = 223;


    [Level(23)]
    public const int Mimic = 230;
    [Level(23)]
    public const int Unicorn = 231;
    [Level(23)]
    public const int FloatyGross = 232;
    [Level(23)]
    public const int MythrilAnvil = 233;
    [Level(23)]
    public const int AdamantiteForge = 234;


    [Level(24)]
    public const int BigMimic = 240;
    [Level(24)]
    public const int SoulofFlight = 241;
    [Level(24)]
    public const int Wings = 242;
    [Level(24)]
    public const int Pirates = 243;


    [Level(25)]
    public const int HallowedPlateMail = 250;
    [Level(25)]
    public const int Clentaminator = 251;
    [Level(25)]
    public const int LifeFruit = 252;
    [Level(25)]
    public const int OldArmy2 = 253;


    [Level(26)]
    public const int LifeFruitMax = 260;
    [Level(26)]
    public const int Destroyer = 261;
    [Level(26)]
    public const int Twins = 262;
    [Level(26)]
    public const int SkeletronPrime = 263;


    [Level(27)]
    public const int PickaxeAxe = 270;
    [Level(27)]
    public const int AvengerEmblem = 271;
    [Level(27)]
    public const int ChlorophyteOre = 272;


    [Level(28)]
    public const int Plantera = 280;

    [Level(29)]
    public const int SolarEclipse = 290;
    [Level(29)]
    public const int OldArmy3 = 291;

    [Level(30)]
    public const int PumpkinMoon = 300;

    [Level(31)]
    public const int FrostMoon = 310;
    [Level(31)]
    public const int SnowLegion = 311;

    [Level(32)]
    public const int Golem = 320;

    [Level(33)]
    public const int MartianMadness = 330;

    [Level(34)]
    public const int Cultist = 340;

    [Level(35)]
    public const int Сelestial = 350;
}

public partial class QuestsManager
{
    public static QuestsManager Instance { get; set; } = null;
    ////////////////////
    public const float QuestRateExp = 1.2f;
    ////////////////////
    /// <summary>
    ///     List: level – XP required to reach the next level
    /// </summary>
    public List<int> LevelExpArray = null;
    /// <summary>
    ///     Table: quest id - quest exp
    /// </summary>
    public Dictionary<int, int> QuestExpTable = null;
    public List<int> AllQuestsArray = null;
    /// <summary>
    ///     Quests sorted by type. Used to optimize the verification of task completion.
    /// </summary>
    public Dictionary<Quest.QuestType, List<int>> SortedQuestTable = null;
    ////////////////////
    protected QuestsManager() { }
    public static void Load()
    {
        Instance = new();
        Instance.InitializationLevelExpTables();
        ////////////////////
        Instance.AllQuestsArray = new();
        Instance.SortedQuestTable = new();
        ////////////////////
        Instance.InitializationQuests();
        ////////////////////
        //Debug();
    }
    public static void Unload()
    {
        if (Instance is null)
            return;
        Instance.LevelExpArray = null;
        Instance.AllQuestsArray = null;
        ////////////////////
        Instance.SortedQuestTable = null;
        Instance.QuestsTable = null;
        ////////////////////
        Instance = null;
    }

    /// <summary>
    ///     An algorithm for automatically determining the amount of experience awarded for each quest and the experience required for each level.
    ///     The algorithm ensures that experience values ​​for quests of different levels do not repeat and always increase by the minimum possible increment.
    /// </summary>
    public void InitializationLevelExpTables()
    {
        LevelExpArray = new();
        QuestExpTable = new();
        ///////////////////////
        Dictionary<int, int> questLevels = new();
        foreach (var field in typeof(QuestID).GetFields())
        {
            var levelAttribute = field.GetCustomAttribute<LevelAttribute>();
            if (levelAttribute is null || levelAttribute.Level <= 0)
                continue;
            int id = (int)field.GetRawConstantValue()!;
            questLevels.Add(id, levelAttribute.Level);
        }
        //////////////////////////////
        Dictionary<int, int> questCountByLevel = new();
        foreach (int level in questLevels.Values)
        {
            questCountByLevel.TryGetValue(level, out int count);
            questCountByLevel[level] = count + 1;
        }
        ////////////////////
        List<int> sortedLevels = questLevels
            .Values
            .Distinct()
            .OrderBy(x => x)
            .ToList();

        ////////////////////

        int previousQuestExperience = 0;
        int previousLevelExperience = 0;
        foreach (int level in sortedLevels)
        {
            int questCount = questCountByLevel[level];
            int questExperience = Math.Max(
                previousQuestExperience + 1,
                previousLevelExperience / questCount + 1);
            int levelExperience = questExperience * questCount;
            ////////////////////////
            previousQuestExperience = questExperience;
            previousLevelExperience = levelExperience;
            /////////////////////////
            LevelExpArray.Add(levelExperience);
            int finalQuestExperience = (int)Math.Round(questExperience * QuestRateExp + 0.0001f);
            foreach (var quest in questLevels)
                if (quest.Value == level)
                    QuestExpTable[quest.Key] = finalQuestExperience;
        }
    }

//    public void Debug()
//    {
//        var modInst = TalentsSystem.Instance;
//        if (modInst is null)
//            return;
//        ////////////////////
//        List<int> visitedQIDs = new();
//        bool isThrow = false;
//        //////////////////////////
//        void PrintError(string msg)
//        {
//            isThrow = true;
//            modInst.Logger.Error($"[{modInst.DisplayName}][QuestLoader]: ");
//        }
//        //////////////////////////
//        visitedQIDs.Clear();
//        foreach (var val in QuestsTable.Values)
//        {
//            int id = val.ID;
//            if (visitedQIDs.Contains(id))
//                PrintError($"QuestsTable -> Dubbing ids: {id}");
//            else
//                visitedQIDs.Add(id);
//        }
//        foreach (var pair in QuestsTable)
//        {
//            var key = pair.Key;
//            var val = pair.Value;
//            if (key != val.ID)
//                PrintError($"QuestsTable -> ID mismatch:\tkey: {key}; id: {val.ID}");
//        }
//        //////////////////////////
//        foreach (var key in SortedQuestTable.Keys)
//        {
//            var list = SortedQuestTable[key];
//            foreach (var id in list)
//                if (!QuestsTable.ContainsKey(id))
//                    PrintError($"QuestsTable (SortedQuestTable) -> ID not found: {id}");
//        }
//        visitedQIDs.Clear();
//        foreach (var key in SortedQuestTable.Keys)
//        {
//            var list = SortedQuestTable[key];
//            foreach (var id in list)
//                if (visitedQIDs.Contains(id))
//                    PrintError($"SortedQuestTable -> Dubbing ids: {id}");
//                else
//                    visitedQIDs.Add(id);
//        }
//        ////////////////////
//#pragma warning disable CS0162
//        if (QuestRateExp == 1f)
//        {
//            int totalXp1 = 0, totalXp2 = 0;
//            foreach (var xp in LevelExpArray)
//                totalXp1 += xp;
//            foreach (var xp in QuestExpTable.Values)
//                totalXp2 += xp;
//            if (totalXp1 != totalXp2)
//                PrintError($"ExpError -> totalXp1[{totalXp1}] is not equal totalXp2[{totalXp2}]");
//        }
//#pragma warning restore CS0162
//        /////////////////////////
//        if (isThrow)
//            throw new Exception("QuestLoader error. Check Logs");
//    }
}