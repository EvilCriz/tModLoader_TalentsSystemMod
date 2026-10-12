using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.ModLoader;

using TBT = TalentsSystem.TalentsBranchType;

namespace TalentsSystem.Modifiers;

public class TalentsModifers
{
    /// By TemporaryRefreshModifiers
    public StatsModifier StatsModifier;
    public List<StatsModifierByCondtion> StatsModifiersByCondtion;
    public List<DynamicStatsModifier> DynamicStatsModifiers;
    public ExplosionModifiers ExplosionModifier;
    public StackAttackModifierHelper StackAttackModifierHelper;
    public DashModifiers DashModifier;
    public SwitchAmmoModifier SwitchAmmoModifier;
    public ManaHealignModifier ManaHealignModifier;
    public List<FactorModifier> FactorModifiers;
    public List<int> DebuffsOnDamage;
    public const int DebuffsOnDamageTime = 10 * 60;
    public Dictionary<int, float> ItemsGrapRangeTable;
    //////////// Common Tables
    public Dictionary<int, float> BuffModifierTable = new();
    //////////// Active Skill Flags
    public bool RodDiscord;
    public bool ReinforcedRodDiscord;
    public bool SandStorm;
    public bool NightVisionPotion;
    public bool MiningPotion;
    public bool ShinePotion;
    public bool GillsPotion;
    public bool SpelunkerPotion;
    public bool ObsidianSkinPotion;
    public bool NoFallDmg;
    public bool AnkhShield;
    public bool ImmuneChaosState;
    public bool ExploreMap;
    public bool WithoutCollisionProjectile;
    public bool Reincarnation;
    public bool FullManaRestoration;
    public bool HealthCostWhenManaIsLow;
    public bool HealthRegenMana;
    /////////// Common
    public int ChanceDropGoldOnKill;
    public float ChanceNotConsumeAmmo;
    public float AIMHomingTurnRate;
    public const int AIMTargetingRadiusInTiles = 25;
    public int ShadowDodgeCooldown;
    public float SpawnRate = 0f;
    public float ItemsGrapRange = 1f;

    public void RefreshModifiers(TalentsSaveInfo save, TalentsApplyModPlayer modPlayer)
    {
        bool empty = save is null || modPlayer is null;
        TemporaryRefreshModifiers temp = new();
        TBT currentBranch = TBT.Travel;
        if(!empty)
        {
            currentBranch = save.CurrentBranch;
            var table = TalentsManager.Instance.TalentsTable[currentBranch];
            ///////////////////
            foreach (var talent in table.Values)
            {
                var points = save.GetActualPoints(talent.ID);
                if (points > 0)
                    talent.RefreshActivator(this, temp, points);
            }
        }
        ///////////////////
        DamageClass type = TalentsManager.GetDamageClass(currentBranch);
        StatsModifier = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.StatsModifiersTable, type);
        StatsModifiersByCondtion = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.StatsModifiersByCondtionTable, type);
        DynamicStatsModifiers = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.DynamicStatsModifiersTable, type);
        ExplosionModifier = TemporaryRefreshModifiers.ExtractMaxTierModifier(empty, ref temp.ExplosionModifiersTable, type, new(0, 0, 0));
        StackAttackModifierHelper = TemporaryRefreshModifiers.ExtractMaxTierModifier(empty, ref temp.StackAttackModifierHelperTable, type, new(0, 0));
        FactorModifiers = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.FactorModifiersTable, currentBranch);
        DashModifier = TemporaryRefreshModifiers.ExtractMaxTierModifier(empty, ref temp.DashModifiers, type, new(0, 0));
        SwitchAmmoModifier = TemporaryRefreshModifiers.ExtractMaxTierModifier(empty, ref temp.SwitchAmmoModifiers, type, new(0, 0));
        ManaHealignModifier = TemporaryRefreshModifiers.ExtractMaxTierModifier(empty, ref temp.ManaHealignModifiers, type, new(0, 0));
        DebuffsOnDamage = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.DebuffsOnDamageTable, type);
        ItemsGrapRangeTable = TemporaryRefreshModifiers.ExtractCurrentBranchModifier(empty, ref temp.ItemsGrapRangeTable, type);
        if (!empty)
        {
            WithoutCollisionProjectile = temp.WithoutCollisionProjectileTable[type];
            AIMHomingTurnRate = temp.AIMHomingTurnRateTable[type];
            SwitchAmmoModifier.CurrentArrow = (byte)Math.Min(SwitchAmmoModifier.Tier, modPlayer.LastCurrentArrow);
            SwitchAmmoModifier.CurrentBullet = (byte)Math.Min(SwitchAmmoModifier.Tier, modPlayer.LastCurrentBullet);
            foreach (var buff in BuffModifierTable.Keys)
                modPlayer.BuffDurationTable[buff] = 0;
        }
        temp = null;
    }

    public static TalentsModifers Empty()
    {
        TalentsModifers modifier = new();
        modifier.RefreshModifiers(null, null);
        return modifier;
    }
}



public class StatsModifier
{
    public float BaseDamage;
    public float Damage;
    public float CritChance;
    public float CritDamage;
    public float AttackSpeed;
    public float ArmorPenetration;
    public float Knokback;
    public float Armor;
    public float Luck;
    public int MaxLife;
    public int MaxMana;
    public float ManaCost;
    public int ImmuneTime;
    public float MoveSpeed;
    public int LifeRegen;
    public float WingsTime;
    public int MaxPets;
    public float Lifesteal;

    public static StatsModifier GetFinal(TalentsApplyModPlayer player, TalentsModifers modifiers)
    {
        StatsModifier result = new();
        ///////////////////
        result.ApplyOtherStats(modifiers.StatsModifier);
        foreach (var modif in modifiers.StatsModifiersByCondtion)
            if (modif.Conditon(player))
                result.ApplyOtherStats(modif);
        foreach (var modif in modifiers.DynamicStatsModifiers)
            result.ApplyOtherStats(modif.GetModifier(player));
        ///////////////////
        result.Armor = (int)Math.Round(result.Armor);
        ///////////////////
        return result;
    }

    protected void ApplyOtherStats(StatsModifier modifier)
    {
        BaseDamage += modifier.BaseDamage;
        Damage += modifier.Damage;
        CritChance += modifier.CritChance;
        CritDamage += modifier.CritDamage;
        AttackSpeed += modifier.AttackSpeed;
        ArmorPenetration += modifier.ArmorPenetration;
        Knokback += modifier.Knokback;
        Armor += modifier.Armor;
        Luck += modifier.Luck;
        MaxLife += modifier.MaxLife;
        MaxMana += modifier.MaxMana;
        ManaCost += modifier.ManaCost;
        ImmuneTime += modifier.ImmuneTime;
        MoveSpeed += modifier.MoveSpeed;
        LifeRegen += modifier.LifeRegen;
        WingsTime += modifier.WingsTime;
        MaxPets += modifier.MaxPets;
        Lifesteal += modifier.Lifesteal;
    }
}

public class StatsModifierByCondtion : StatsModifier
{
    public Func<TalentsApplyModPlayer, bool> Conditon = (player) => false;
    public Func<TalentsApplyModPlayer, Item, bool> ModifyWeaponStatsCondition = (player, item) => false;
}

public class DynamicStatsModifier
{
    public delegate void DynamicModifierChanger(TalentsApplyModPlayer player, StatsModifier modifier);
    protected StatsModifier Modifier { get; set; } = new();
    public DynamicModifierChanger ModifierChanger { protected get; set; } = (_, _) => { };
    public StatsModifier GetModifier(TalentsApplyModPlayer player) 
    {
        ModifierChanger(player, Modifier);
        return Modifier;
    }
}



public class FactorModifier
{
    public enum FactorType : byte
    {
        Health,
        Mana
    }
    public enum TriggerType : byte
    {
        Increase,
        Decrease
    }
    public FactorType Type { get; protected set; }
    public TriggerType Trigger { get; protected set; }
    public int Count { get; protected set; }
    public float Factor { get; protected set; }
    public float Chance { get; protected set; }

    public FactorModifier(FactorType factorType, TriggerType triggerType)
    {
        Type = factorType;
        Trigger = triggerType;
    }
    public void Initialize(int count, float factor, float chance)
    {
        Count = count;
        Factor = factor;
        Chance = chance;
    }
}



public abstract class TierModifiers
{
    public readonly bool IsReady;
    public readonly int Tier;
    public readonly int Points;
    public TierModifiers(int tier, int points)
    {
        Points = points;
        Tier = tier;
        IsReady = Tier > 0;
    }
}

public class ManaHealignModifier : TierModifiers
{
    public int DebuffDurationInSec { get; protected set; }
    public int ManaRegenerationDisabledDurationInSec { get; protected set; }
    public ManaHealignModifier(int tier, int points) : base(tier, points) { }
    public void Initialize(int debuffDurationInSec, int manaRegenerationDisabledDurationInSec)
    { 
        DebuffDurationInSec = debuffDurationInSec;
        ManaRegenerationDisabledDurationInSec = manaRegenerationDisabledDurationInSec;
    }
}

public class SwitchAmmoModifier : TierModifiers
{
    public enum ArrowType : byte
    {
        Default = 0,
        ArrowFlaming,
        ArrowJester,
        ArrowChlorophyte,
        Max
    }
    public enum BulletType : byte
    { 
        Default,
        BulletMeteor,
        BulletCrystal,
        BulletChlorophyte,
        Max
    }
    public byte CurrentArrow;
    public byte CurrentBullet;

    public SwitchAmmoModifier(int tier, int points) : base(tier, points) { }

    public void NextArrow() => Next(ref CurrentArrow, (byte)ArrowType.Max);
    public void NextBullet() => Next(ref CurrentBullet, (byte)BulletType.Max);
    protected void Next(ref byte current, byte max)
    {
        if (++current > Tier || current == max)
            current = 0;
    }
    public static int GetArrowProjectileTypeByTier(int tier)
    {
        var type = (ArrowType)tier;
        return type switch
        {
            ArrowType.ArrowFlaming => ProjectileID.FireArrow,
            ArrowType.ArrowJester => ProjectileID.JestersArrow,
            ArrowType.ArrowChlorophyte => ProjectileID.ChlorophyteArrow,
            _ => -1
        };
    }
    public int GetArrowProjectileType() => GetArrowProjectileTypeByTier(CurrentArrow);
    public static int GetBulletProjectileTypeByTier(int tier)
    {
        var type = (BulletType)tier;
        return type switch
        {
            BulletType.BulletMeteor => ProjectileID.MeteorShot,
            BulletType.BulletCrystal => ProjectileID.CrystalBullet,
            BulletType.BulletChlorophyte => ProjectileID.ChlorophyteBullet,
            _ => -1
        };
    }
    public int GetBulletProjectileType() => GetBulletProjectileTypeByTier(CurrentBullet);
}

public class ExplosionModifiers : TierModifiers
{
    public enum ExplosionTrigget
    { 
        HealFactor,
        ManaFactor,
        CritFactor
    }
    public ExplosionTrigget Trigget { get; protected set; }
    public float Damage { get; protected set; }
    public int Radius { get; protected set; }
    public int Cooldown { get; protected set; }
    public Color Color { get; protected set; }
    public SoundStyle Sound { get; protected set; }
    public ExplosionModifiers(ExplosionTrigget trigget, int tier, int points) : base(tier, points) { Trigget = trigget; }
    public void Initialize(float damage, int radiusInTiles, int cooldown, Color color, SoundStyle sound)
    {
        Damage = damage;
        Radius = radiusInTiles * 16;
        Cooldown = cooldown;
        Color = color;
        Sound = sound;

        //SoundID.Item8
        //SoundID.Item9
        //SoundID.Item20 +
        //SoundID.Item21 +
        //SoundID.Item43
        //SoundID.Item72
        //SoundID.Item74 + 
        //SoundID.Item75 +
        //SoundID.Item113
    }
}

public class DashModifiers : TierModifiers
{
    public float Speed { get; protected set; }
    public int Cooldown { get; protected set; }
    public int ImmuneTime { get; protected set; }
    public DashModifiers(int tier, int points) : base(tier, points) { }
    public void Initialization(float speed, int cooldown, int immuneTime)
    {
        Speed = speed;
        Cooldown = cooldown;
        ImmuneTime = immuneTime;
    }
}

public class StackAttackModifierHelper : TierModifiers
{
    public int MaxStack { get; protected set; }
    public StackAttackModifierHelper(int tier, int points) : base(tier, points) { }
    public void Initialization(int maxStack) => MaxStack = maxStack;
}


public class TemporaryRefreshModifiers
{
    public Dictionary<DamageClass, StatsModifier> StatsModifiersTable;
    public Dictionary<DamageClass, List<StatsModifierByCondtion>> StatsModifiersByCondtionTable;
    public Dictionary<DamageClass, List<DynamicStatsModifier>> DynamicStatsModifiersTable;
    public Dictionary<DamageClass, List<ExplosionModifiers>> ExplosionModifiersTable;
    public Dictionary<DamageClass, List<StackAttackModifierHelper>> StackAttackModifierHelperTable;
    public Dictionary<DamageClass, bool> WithoutCollisionProjectileTable;
    public Dictionary<DamageClass, float> AIMHomingTurnRateTable;
    public Dictionary<TBT, List<FactorModifier>> FactorModifiersTable;
    public List<DashModifiers> DashModifiers = new();
    public List<SwitchAmmoModifier> SwitchAmmoModifiers = new();
    public List<ManaHealignModifier> ManaHealignModifiers = new();
    public Dictionary<DamageClass, List<int>> DebuffsOnDamageTable;
    public Dictionary<DamageClass, Dictionary<int, float>> ItemsGrapRangeTable;

    public TemporaryRefreshModifiers()
    {
        InitializeTable(ref StatsModifiersTable);
        InitializeTable(ref StatsModifiersByCondtionTable);
        InitializeTable(ref DynamicStatsModifiersTable);
        InitializeTable(ref ExplosionModifiersTable);
        InitializeTable(ref StackAttackModifierHelperTable);
        InitializeTable(ref FactorModifiersTable);
        InitializeTable(ref DebuffsOnDamageTable);
        InitializeTable(ref ItemsGrapRangeTable);
        InitilizeTableDefault(ref WithoutCollisionProjectileTable);
        InitilizeTableDefault(ref AIMHomingTurnRateTable);
    }

    protected void InitializeTable<T>(ref Dictionary<DamageClass, T> table) where T : new()
    {
        table = new();
        var damages = TalentsManager.GetDamageClasses();
        foreach(var dmg in damages)
            table[dmg] = new();
    }
    protected void InitilizeTableDefault<T>(ref Dictionary<DamageClass, T> table)
    {
        table = new();
        var damages = TalentsManager.GetDamageClasses();
        foreach (var dmg in damages)
            table[dmg] = default;
    }
    protected void InitializeTable<T>(ref Dictionary<TBT, T> table) where T : new()
    {
        table = new();
        var types = TalentsManager.GetTBT();
        foreach (var type in types)
            table[type] = new();
    }

    public static T ExtractMaxTierModifier<T>(bool empty, ref Dictionary<DamageClass, List<T>> modifiersTable, DamageClass type,T _default) 
        where T : TierModifiers
    {
        var modifiers = ExtractCurrentBranchModifier(empty, ref modifiersTable, type);
        return ExtractMaxTierModifier(empty, ref modifiers, type, _default);
    }
    public static T ExtractMaxTierModifier<T>(bool empty, ref List<T> modifiers, DamageClass type, T _default)
        where T : TierModifiers
    {
        T result = null;
        if (!empty)
        {
            int maxTier = 0;
            int maxPoints = 0;
            foreach (var modifier in modifiers)
            {
                if (modifier.Tier < maxTier)
                    continue;
                maxTier = modifier.Tier;
                if (modifier.Points < maxPoints)
                    continue;
                maxPoints = modifier.Points;
                result = modifier;
            }
        }
        modifiers = null;
        if (result is null)
            result = _default;
        return result;
    }

    public static TVal ExtractCurrentBranchModifier<TKey, TVal>(bool empty, ref Dictionary<TKey, TVal> table, TKey key)
            where TVal : class, new()
    {
        var result = !empty && table.ContainsKey(key) ? table[key] : new();
        table = null;
        return result;
    }
}