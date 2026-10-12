using System;
using System.Collections.Generic;
using System.Reflection;
using TalentsSystem.Utils;
using Terraria.Localization;
using Terraria.ModLoader;

namespace TalentsSystem;

public enum TalentsBranchType : byte
{
    Melee,
    Ranged,
    Magic,
    Summon,
    Travel,
    Warlock
}

public static class TalentID_W
{
    [Level(level: 1, points: 4)]
    public const int MaxLife1 = 10; // 1-4
    [Level(1)]
    public const int Armor1 = 11;
    [Level(1)]
    public const int Damage1 = 12;

    [Level(2, points: 1)]
    public const int ExplosionByHeal = 20; // 5

    [Level(level: 3, points: 4)]
    public const int MaxLife3 = 30; // 6-9
    [Level(3)]
    public const int Armor3 = 31;
    [Level(3)]
    public const int Damage3 = 32;

    [Level(4, points: 1)]
    public const int MoreHeal = 40; // 10

    [Level(level: 5, points: 4)]
    public const int MaxLife5 = 50; // 11-14
    [Level(5)]
    public const int Armor5 = 51;
    [Level(5)]
    public const int Damage5 = 52;

    [Level(level: 6, points: 1)]
    public const int Charge = 150; // 15

    [Level(level: 7, points: 4)]
    public const int ImmuneTime = 70; // 16-19
    [Level(7)]
    public const int SicknessTime = 71;
    [Level(7)]
    public const int Knockback = 72;

    [Level(level: 8, points: 1)]
    public const int RodDiscord = 80; // 20

    [Level(level: 9, points: 4)]
    public const int LifeRegenOutOfCombat = 90; // 21-24

    [Level(level: 10, points: 1)]
    public const int DamageLastTargetWithinRadius = 100; // 25

    [Level(level: 11, points: 4)]
    public const int DebuffsOnDamage = 110; // 26-29

    [Level(level: 12, points: 1)]
    public const int SpawnRate = 120; // 30

    [Level(level: 13, points: 4)] // 31-34
    public const int ShadowDodge = 130;

    [Level(level: 14, points: 1)]
    public const int Reincarnation = 140; // 35
}

public static class TalentID_R
{
    [Level(level: 1, points: 4)]
    public const int CritChance1 = 10; // 1-4
    [Level(1)]
    public const int AttackSpeed1 = 11;
    [Level(1)]
    public const int Damage1 = 12;

    [Level(level: 2, points: 1)]
    public const int AmmoSwitch1 = 20; // 5

    [Level(level: 3, points: 4)]
    public const int CritChance3 = 30; // 6-9
    [Level(3)]
    public const int AttackSpeed3 = 31;
    [Level(3)]
    public const int Damage3 = 32;

    [Level(level: 4, points: 1)]
    public const int AmmoSwitch2 = 40; // 10

    [Level(level: 5, points: 4)]
    public const int CritChance5 = 50; // 11-14
    [Level(5)]
    public const int AttackSpeed5 = 51;
    [Level(5)]
    public const int Damage5 = 52;

    [Level(level: 6, points: 1)]
    public const int Charge = 60; // 15

    [Level(level: 7, points: 3)]
    public const int ArmorPenetration = 70; // 16-18
    [Level(7)]
    public const int CritDamage = 71;
    [Level(7)]
    public const int Luck = 72;

    [Level(level: 8, points: 1)]
    public const int AmmoSwitch3 = 80; // 19

    [Level(level: 9, points: 1)]
    public const int RodDiscord = 90; // 20

    [Level(level: 10, points: 4)]
    public const int ChanceNotConsumeAmmo = 100; // 21-24

    [Level(level: 11, points: 1)] 
    public const int WithoutCollisionProjectile = 110; // 25

    [Level(level: 12, points: 4)]
    public const int CritChanceIsStandingStill = 120; // 26-29
    [Level(12)]
    public const int AttackSpeedIsStandingStill = 121;
    [Level(12)]
    public const int DamageIsStandingStill = 122;

    [Level(level: 13, points: 1)]
    public const int ExplosionByCrit = 130; // 30

    [Level(level: 14, points: 4)]
    public const int ProjectileAIM = 140; // 31-34;

    [Level(level: 15, points: 1)]
    public const int Lifesteal = 150; // 35
}

public static class TalentID_M
{
    [Level(level: 1, points: 4)]
    public const int MaxMana1 = 10; // 1-4
    [Level(1)]
    public const int ManaCost1 = 11;
    [Level(1)]
    public const int Damage1 = 12;

    [Level(level: 2, points: 1)]
    public const int GrapManaStars = 20; // 5

    [Level(level: 3, points: 4)]
    public const int MaxMana3 = 30; // 6-9
    [Level(3)]
    public const int ManaCost3 = 31;
    [Level(3)]
    public const int Damage3 = 32;

    [Level(level: 4, points: 1)]
    public const int MoreMana = 40; // 10

    [Level(level: 5, points: 4)]
    public const int MaxMana5 = 50; // 11-14
    [Level(5)]
    public const int ManaCost5 = 51;
    [Level(5)]
    public const int Damage5 = 52;

    [Level(level: 6, points: 1)]
    public const int Charge = 60; // 15

    [Level(level: 7, points: 4)]
    public const int ManaSickness = 70; // 16-19
    [Level(7)]
    public const int ManaFromSpentMana  = 71;

    [Level(level: 8, points: 1)]
    public const int RodDiscord = 80; // 20

    [Level(level: 9, points: 4)]
    public const int DamageIsMaxMana = 90; // 21-24

    [Level(level: 10, points: 1)]
    public const int FullManaRestoration = 100; // 25

    [Level(level: 11, points: 4)]
    public const int ExplosionByCrit = 110; // 26-29

    [Level(level: 12, points: 1)] // 30
    public const int WithoutCollisionProjectile = 120;

    [Level(level: 13, points: 4)]
    public const int ProjectileAIM = 130; // 31-34

    [Level(level: 14, points: 1)]
    public const int Lifesteal = 150; // 35
}

public static class TalentID_S
{
    [Level(level: 1, points: 4)]
    public const int Damage1 = 10; // 1-4

    [Level(level: 2, points: 1)]
    public const int MorePet1 = 20; // 5

    [Level(level: 3, points: 4)]
    public const int Damage3 = 30; // 6-9

    [Level(level: 4, points: 1)]
    public const int StackAttack = 40; // 10

    [Level(level: 5, points: 4)]
    public const int Damage5 = 50; // 11-14

    [Level(level: 6, points: 1)]
    public const int Charge = 60; // 15

    [Level(level: 7, points: 4)]
    public const int DamageWhips = 70; // 16-19

    [Level(level: 8, points: 1)]
    public const int RodDiscord = 80; // 20

    [Level(level: 9, points: 4)]
    public const int DamageIsMaxLife = 90; // 21-24

    [Level(level: 10, points: 1)]
    public const int MorePet2 = 100; // 25

    [Level(level: 11, points: 4)]
    public const int AttackSpeedWhips = 110; // 26-29

    [Level(level: 12, points: 1)]
    public const int MorePet3 = 120; // 30

    [Level(level: 14, points: 5)]
    public const int Lifesteal = 140; // 31-35
}

public static class TalentID_T
{
    [Level(level: 1, points: 4)]
    public const int MoveSpeed = 10; // 1-4

    [Level(level: 2, points: 1)]
    public const int SandStorm = 20; // 5

    [Level(level: 3, points: 1)]
    public const int NightVisionPotion = 30; // 6

    [Level(level: 4, points: 1)]
    public const int MiningPotion = 40; // 7

    [Level(level: 5, points: 1)]
    public const int ShinePotion = 50; // 8

    [Level(level: 6, points: 1)]
    public const int GillsPotion = 60; // 9

    [Level(level: 7, points: 1)]
    public const int NoFallDmg = 70; // 10

    [Level(level: 8, points: 1)]
    public const int SpelunkerPotion = 80; // 11

    [Level(level: 9, points: 2)]
    public const int LifeRegen = 90; // 12-13

    [Level(level: 10, points: 1)]
    public const int ObsidianSkinPotion = 100; // 14

    [Level(level: 11, points: 1)]
    public const int Charge = 110; // 15

    [Level(level: 12, points: 4)]
    public const int ChanceDropGoldOnKill = 120; // 16-19

    [Level(level: 13, points: 1)]
    public const int RodDiscord = 130; // 20

    [Level(level: 14, points: 4)]
    public const int Luck = 140; // 21-24

    [Level(level: 15, points: 1)]
    public const int AnkhShield = 150; // 25

    [Level(level: 16, points: 4)]
    public const int Charge2 = 160; // 26-29

    [Level(level: 17, points: 1)]
    public const int ImmoneChaosState = 170; // 30

    [Level(level: 18, points: 4)]
    public const int WingsTime = 180; // 31-34

    [Level(level: 19, points: 1)]
    public const int ExploreMap = 190; // 35
}

public static class TalentID_WK
{
    [Level(level: 1, points: 4)]
    public const int DamageWarlockBase = 10; // 1-4

    [Level(level: 2, points: 1)]
    public const int HealthCostWhenManaIsLow = 20; // 5

    [Level(level: 3, points: 4)]
    public const int LifestealIsLowHealth = 30; // 6-9

    [Level(level: 4, points: 1)]
    public const int ManaHealing = 40; // 10

    [Level(level: 5, points: 4)]
    public const int DamageIsLowHealth5 = 50; // 16-19

    [Level(level: 6, points: 1)]
    public const int Charge = 60; // 15

    [Level(level: 7, points: 4)]
    public const int DamageIsLowHealth7 = 70; // 16-19

    [Level(level: 8, points: 1)]
    public const int RodDiscord = 80; // 20

    [Level(level: 9, points: 4)]
    public const int DamageIsLowHealth9 = 90; // 21-24

    [Level(level: 10, points: 1)]
    public const int HealthRegenMana = 100; // 25

    [Level(level: 11, points: 4)]
    public const int ProjectileAIM = 110; // 26-29

    [Level(level: 12, points: 1)]
    public const int WithoutCollisionProjectile = 120; // 30

    [Level(level: 13, points: 5)]
    public const int Lifesteal = 130; // 31-35
}

public partial class TalentsManager
{
    public static TalentsManager Instance { get; set; } = null;
    protected TalentsManager() { }
    ///////////////////////////
    private TalentsBranchType[] _talentsBranchTypes = null;
    private byte[] _talentsBranchTypesInByte = null;
    private Dictionary<TalentsBranchType, DamageClass> _talentBranchDamageTable = null;
    private DamageClass[] _talentBranchDamageClasses = null;
    ///////////////////////////
    /// <summary>
    ///     Talent branch + dictionary: talent ID >> the section to which the talent belongs.
    /// </summary>
    public Dictionary<TalentsBranchType, Dictionary<int, int>> TalentLevelTable = null;
    /// <summary>
    ///     Talent branch + dictionary: talent ID >> maximum number of talent points.
    /// </summary>
    public Dictionary<TalentsBranchType, Dictionary<int, int>> TalentMaxPointsTable = null;
    public static void Load()
    {
        Instance = new();
        Instance.TalentLevelTable = new();
        Instance.TalentMaxPointsTable = new();
        Instance.InitializationBranchesInfo();
        Instance.TalentsTable = new();
        Instance.LevelsTalentsTable = new();
        Instance.InitializationTalents();
    }
    public static void Unload()
    {
        if (Instance is null)
            return;
        Instance.TalentLevelTable = null;
        Instance.TalentMaxPointsTable = null;
        Instance.TalentsTable = null;
        Instance.LevelsTalentsTable = null;
        Instance._talentsBranchTypes = null;
        Instance._talentsBranchTypesInByte = null;
        Instance._talentBranchDamageTable = null;
        Instance._talentBranchDamageClasses = null;
        Instance = null;
    }
    public void InitializationBranchesInfo()
    {
        FillTalentsTables(typeof(TalentID_W), TalentsBranchType.Melee);
        FillTalentsTables(typeof(TalentID_R), TalentsBranchType.Ranged);
        FillTalentsTables(typeof(TalentID_M), TalentsBranchType.Magic);
        FillTalentsTables(typeof(TalentID_S), TalentsBranchType.Summon);
        FillTalentsTables(typeof(TalentID_WK), TalentsBranchType.Warlock);
        FillTalentsTables(typeof(TalentID_T), TalentsBranchType.Travel);
        void FillTalentsTables(Type type, TalentsBranchType branchType)
        {
            Dictionary<int, LevelAttribute> talentsAttributeTable = new();
            foreach (var field in type.GetFields())
            {
                var levelAttribute = field.GetCustomAttribute<LevelAttribute>();
                int id = (int)field.GetRawConstantValue()!;
                if (levelAttribute is null || levelAttribute.Level <= 0)
                    throw new Exception($"InitializationBranchesInfo Exception. ID: {id} levelAttribute is null or level <= 0");
                talentsAttributeTable[id] = levelAttribute;
            }
            /////////////////////
            List<LevelAttribute> actualAttr = new();
            foreach (var attr in talentsAttributeTable.Values)
                if (attr.Points != -1)
                    actualAttr.Add(attr);
            ////////////////////
            foreach (var attr in talentsAttributeTable.Values)
                foreach (var actAttr in actualAttr)
                    if (attr.Level == actAttr.Level)
                        attr.Points = actAttr.Points;
            ////////////////////
            TalentLevelTable[branchType] = new();
            TalentMaxPointsTable[branchType] = new();
            foreach (var table in talentsAttributeTable)
            {
                TalentLevelTable[branchType][table.Key] = table.Value.Level;
                TalentMaxPointsTable[branchType][table.Key] = table.Value.Points;
            }
        }
    }

    public static TalentsBranchType[] GetTBT()
    {
        if(Instance._talentsBranchTypes is null)
            Instance._talentsBranchTypes = Enum.GetValues<TalentsBranchType>();
        return Instance._talentsBranchTypes;
    }
    public static byte[] GetTBTByte()
    {
        if (Instance._talentsBranchTypesInByte is null)
        {
            var tbtArray = GetTBT();
            Instance._talentsBranchTypesInByte = new byte[tbtArray.Length];
            for (int i = 0; i < tbtArray.Length; i++)
                Instance._talentsBranchTypesInByte[i] = (byte)tbtArray[i];
        }
        return Instance._talentsBranchTypesInByte;
    }
    private static void InitializeDamageTable()
    {
        if (Instance._talentBranchDamageTable is null)
        {
            Instance._talentBranchDamageTable = new();
            ////////
            Append(TalentsBranchType.Melee, DamageClass.Melee);
            Append(TalentsBranchType.Ranged, DamageClass.Ranged);
            Append(TalentsBranchType.Magic, DamageClass.Magic);
            Append(TalentsBranchType.Summon, DamageClass.Summon);
            Append(TalentsBranchType.Warlock, ModContent.GetInstance<WarlockDamageClass>());
            Append(TalentsBranchType.Travel, DamageClass.Generic);
            ////////
            void Append(TalentsBranchType type, DamageClass dmg)
                => Instance._talentBranchDamageTable[type] = dmg;
        }
    }
    public static DamageClass GetDamageClass(TalentsBranchType tbt)
    {
        InitializeDamageTable();
        return Instance._talentBranchDamageTable[tbt];
    }

    public static LocalizedText GetDamageDisplayName(TalentsBranchType tbt)
        => tbt switch
        {
            TalentsBranchType.Melee => UtilsLocalization.Melee,
            TalentsBranchType.Ranged => UtilsLocalization.Ranger,
            TalentsBranchType.Magic => UtilsLocalization.Mage,
            TalentsBranchType.Warlock => UtilsLocalization.Mage,
            TalentsBranchType.Summon => UtilsLocalization.Summoner,
            _ => LocalizedText.Empty
        };

    public static DamageClass[] GetDamageClasses()
    {
        if (Instance._talentBranchDamageClasses is null)
        {
            InitializeDamageTable();
            Instance._talentBranchDamageClasses = new DamageClass[Instance._talentBranchDamageTable.Count];
            int i = 0;
            foreach (var dmg in Instance._talentBranchDamageTable.Values)
                Instance._talentBranchDamageClasses[i++] = dmg;
        }
        return Instance._talentBranchDamageClasses;
    }
}
