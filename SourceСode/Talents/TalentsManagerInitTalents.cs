using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using TalentsSystem.Content;
using TalentsSystem.Modifiers;
using TalentsSystem.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using static TalentsSystem.Talent;
using IAI = TalentsSystem.IconAnimationInfo;
using LT = Terraria.Localization.LocalizedText;
using TBT = TalentsSystem.TalentsBranchType;
using UL = TalentsSystem.Utils.UtilsLocalization;

namespace TalentsSystem;

public partial class TalentsManager
{
    public Dictionary<TBT, Dictionary<int, Talent>> TalentsTable = null;
    public Dictionary<TBT, Dictionary<int, List<Talent>>> LevelsTalentsTable = null;
    /////////////////////
    public delegate void TalentInitializator(
        int id,
        IAI icon);
    /////////////////////
    private TBT _currentInitBrachType;
    protected TBT CurrentInitBranchType
    {
        get => _currentInitBrachType;
        set
        {
            _currentInitBrachType = value;
            TalentsTable[_currentInitBrachType] = new();
        }
    }
    protected int CurrentInitTalentID;
    protected IAI CurrentInitIcon = null;
    /////////////////////
    public void InitializationTalents()
    {
        TalentInitializator talent = (id, icon) =>
        {
            CurrentInitTalentID = id;
            CurrentInitIcon = icon;
        };
        Func<string, int, IAI> icon2 = (name, heightFrameSize) =>
        {
            if (Main.netMode == NetmodeID.Server)
                return null;
            var asset = AssetsManager.Instance.GetOrLoadAssetTalents(name);
            if (!asset.IsLoaded)
                asset.Wait();
            int height = asset.Height();
            int frames = height / heightFrameSize;
            int delay = frames == 1 ? 0 : Math.Max(frames * 5, 40);
            return IAI.FromTalents(name, frames, 4, delay);
        };
        Func<string, IAI> icon = (name) => icon2(name, 32);



        ////////////////// Warrior

        CurrentInitBranchType = TBT.Melee;


        talent(id: TalentID_W.MaxLife1, icon: icon("MaxLife1"));
        CreateTalentModifierMaxLife(5);

        talent(id: TalentID_W.Armor1, icon: IAI.FromItemID(ItemID.IronChainmail));
        CreateTalentModifierArmor(1f);

        talent(id: TalentID_W.Damage1, icon: IAI.FromItemID(ItemID.IronShortsword));
        CreateTalentModifierDamage(0.01f);


        talent(id: TalentID_W.ExplosionByHeal, icon: IAI.FromItemID(ItemID.BouncyBomb));
        CreateTalentModifierExplosionHeal(damage: 1f, radiusInTiles: 15, cooldown: 3 * 60, Color.Red, SoundID.Item14);


        talent(id: TalentID_W.MaxLife3, icon: icon("MaxLife2"));
        CreateTalentModifierMaxLife(10);

        talent(id: TalentID_W.Armor3, icon: IAI.FromItemID(ItemID.SilverChainmail));
        CreateTalentModifierArmor(1.5f);

        talent(id: TalentID_W.Damage3, icon: IAI.FromItemID(ItemID.SilverShortsword));
        CreateTalentModifierDamage(0.015f);


        talent(id: TalentID_W.MoreHeal, icon: IAI.FromItemID(ItemID.PanicNecklace));
        CreateTalentModifierFactorHealIncrease(0.5f);


        talent(id: TalentID_W.MaxLife5, icon: icon("MaxLife3"));
        CreateTalentModifierMaxLife(15);

        talent(id: TalentID_W.Armor5, icon: IAI.FromItemID(ItemID.PlatinumChainmail));
        CreateTalentModifierArmor(2f);

        talent(id: TalentID_W.Damage5, icon: IAI.FromItemID(ItemID.PlatinumShortsword));
        CreateTalentModifierDamage(0.02f);


        talent(id: TalentID_W.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();


        talent(id: TalentID_W.ImmuneTime, icon: icon("ImmuneTime"));
        CreateTalentModifierImmuneTime(30);

        talent(id: TalentID_W.SicknessTime, icon: IAI.FromItemID(ItemID.HealingPotion));
        CreateTalentModifierBuffTime(BuffID.PotionSickness, -0.1f);

        talent(id: TalentID_W.Knockback, icon: IAI.FromItemID(ItemID.TitanGlove));
        CreateTalentModifierKnokback(0.04f);


        talent(id: TalentID_W.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();


        talent(id: TalentID_W.LifeRegenOutOfCombat, icon: icon("LifeRegenOutOfCombat"));
        CreateTalentModifierLifeRegenOutOfCombat(2, timeInSecond: 15);


        talent(id: TalentID_W.DamageLastTargetWithinRadius, icon: IAI.FromItemID(ItemID.Excalibur));
        CreateTalentModifierDamageLastTargetWithinRadius(0.4f, radiusInTiles: 10);


        talent(id: TalentID_W.DebuffsOnDamage, icon: IAI.FromItemID(ItemID.FlaskofPoison));
        CreateTalentModifierDebuffsOnDamage([BuffID.Poisoned, BuffID.Confused, BuffID.Ichor, BuffID.CursedInferno]);


        talent(id: TalentID_W.SpawnRate, icon: IAI.FromItemID(ItemID.BloodZombieBanner));
        CreateTalentModifierSpawnRate(30f);


        talent(id: TalentID_W.ShadowDodge, icon: IAI.FromItemID(ItemID.HallowedPlateMail));
        CreateTalentModififerShadowDodge(baseCooldownInSec: 25, cooldownReductionInSec: 5);


        talent(id: TalentID_W.Reincarnation, icon: IAI.FromItemID(ItemID.DemonHeart));
        CreateTalentModifierFlag("Reincarnation", m => m.Reincarnation = true, [Lang.GetBuffName(BuffID.PotionSickness)], ActivationType.Active);



        ////////////////// Ranger

        CurrentInitBranchType = TBT.Ranged;


        talent(id: TalentID_R.CritChance1, icon: icon("Crit1"));
        CreateTalentModifierCritChance(1f);

        talent(id: TalentID_R.AttackSpeed1, icon: icon("AttackSpeed1"));
        CreateTalentModifierAttackSpeed(0.01f);

        talent(id: TalentID_R.Damage1, icon: icon("RangeDamage1"));
        CreateTalentModifierDamage(0.01f);
        OverrideHeader("Range");


        talent(id: TalentID_R.AmmoSwitch1, icon: IAI.FromItemID(ItemID.FlamingArrow));
        CreateTalentModifierAmmoSwith(tier: 1);


        talent(id: TalentID_R.CritChance3, icon: icon("Crit2"));
        CreateTalentModifierCritChance(1.5f);

        talent(id: TalentID_R.AttackSpeed3, icon: icon("AttackSpeed2"));
        CreateTalentModifierAttackSpeed(0.015f);

        talent(id: TalentID_R.Damage3, icon: icon("RangeDamage2"));
        CreateTalentModifierDamage(0.015f);
        OverrideHeader("Range");


        talent(id: TalentID_R.AmmoSwitch2, icon: IAI.FromItemID(ItemID.JestersArrow));
        CreateTalentModifierAmmoSwith(tier: 2);


        talent(id: TalentID_R.CritChance5, icon: icon("Crit3"));
        CreateTalentModifierCritChance(2f);

        talent(id: TalentID_R.AttackSpeed5, icon: icon("AttackSpeed3"));
        CreateTalentModifierAttackSpeed(0.02f);

        talent(id: TalentID_R.Damage5, icon: icon("RangeDamage3"));
        CreateTalentModifierDamage(0.02f);
        OverrideHeader("Range");


        talent(id: TalentID_R.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();


        talent(id: TalentID_R.ArmorPenetration, icon: IAI.FromItemID(ItemID.SharkToothNecklace));
        CreateTalentModifierArmorPenetration(2f);

        talent(id: TalentID_R.CritDamage, icon: IAI.FromItemID(ItemID.EyeoftheGolem));
        CreateTalentModifierCritDamage(0.04f);

        talent(id: TalentID_R.Luck, icon: icon("Luck"));
        CreateTalentModifierLuck(0.075f);


        talent(id: TalentID_R.AmmoSwitch3, icon: IAI.FromItemID(ItemID.ChlorophyteArrow));
        CreateTalentModifierAmmoSwith(tier: 3);


        talent(id: TalentID_R.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();


        talent(id: TalentID_R.ChanceNotConsumeAmmo, icon: IAI.FromItemID(ItemID.EndlessMusketPouch));
        CreateTalentModifierChanceNotConsumeAmmo(25f);


        talent(id: TalentID_R.WithoutCollisionProjectile, icon: icon("WithoutCollisionProjectile"));
        CreateTalentModifierWithoutCollisionProjectile();


        talent(id: TalentID_R.CritChanceIsStandingStill, icon: icon("Crit4"));
        CreateTalentModifierCritChanceIsStandingStill(10f);

        talent(id: TalentID_R.AttackSpeedIsStandingStill, icon: icon("AttackSpeed4"));
        CreateTalentModifierAttackSpeedIsStandingStill(0.1f);

        talent(id: TalentID_R.DamageIsStandingStill, icon: icon("RangeDamage4"));
        CreateTalentModifierDamageIsStandingStill(0.1f);


        talent(id: TalentID_R.ExplosionByCrit, icon: IAI.FromItemID(ItemID.ScarabBomb));
        CreateTalentModifierExplosionCrit(damage: 0.25f, radiusInTiles: 5, cooldown: 5 * 60, Color.Brown, SoundID.Item14);


        talent(id: TalentID_R.ProjectileAIM, icon("AIM"));
        CreateTalentModifierProjectileAIM(1f);


        talent(id: TalentID_R.Lifesteal, icon: icon2("Lifesteal", 48));
        CreateTalentModifierLifesteal(0.005f);



        /////////////////////////// TRAVELER

        CurrentInitBranchType = TBT.Travel;


        talent(id: TalentID_T.MoveSpeed, icon: IAI.FromItemID(ItemID.DiamondRing));
        CreateTalentModifierMoveSpeed(0.04f);

        talent(id: TalentID_T.SandStorm, icon: IAI.FromItemID(ItemID.SandstorminaBottle));
        CreateTalentModifierImmitationAccessory(ItemID.SandstorminaBottle, m => m.SandStorm = true);

        talent(id: TalentID_T.NightVisionPotion, icon: IAI.FromItemID(ItemID.NightOwlPotion));
        CreateTalentModifierImmitationBuff(BuffID.NightOwl, m => m.NightVisionPotion = true);

        talent(id: TalentID_T.MiningPotion, icon: IAI.FromItemID(ItemID.MiningPotion));
        CreateTalentModifierImmitationBuff(BuffID.Mining, m => m.MiningPotion = true);

        talent(id: TalentID_T.ShinePotion, icon: IAI.FromItemID(ItemID.ShinePotion));
        CreateTalentModifierImmitationBuff(BuffID.Shine, m => m.ShinePotion = true);

        talent(id: TalentID_T.GillsPotion, icon: IAI.FromItemID(ItemID.GillsPotion));
        CreateTalentModifierImmitationBuff(BuffID.Gills, m => m.GillsPotion = true);

        talent(id: TalentID_T.NoFallDmg, icon: IAI.FromItemID(ItemID.CreativeWings));
        CreateTalentModifierNoFallDmg();

        talent(id: TalentID_T.SpelunkerPotion, icon: IAI.FromItemID(ItemID.SpelunkerPotion));
        CreateTalentModifierImmitationBuff(BuffID.Spelunker, m => m.SpelunkerPotion = true);

        talent(id: TalentID_T.LifeRegen, icon: IAI.FromItemID(ItemID.RegenerationPotion));
        CreateTalentModifierLifeRegen(2);

        talent(id: TalentID_T.ObsidianSkinPotion, icon: IAI.FromItemID(ItemID.ObsidianSkinPotion));
        CreateTalentModifierImmitationBuff(BuffID.ObsidianSkin, m => m.ObsidianSkinPotion = true);

        talent(id: TalentID_T.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();

        talent(id: TalentID_T.ChanceDropGoldOnKill, icon: icon("ChanceDropGoldOnKill"));
        CreateTalentModifierChanceDropGoldOnKill(TalentsSystem.CalamityLoaded ? 4 : 2);

        talent(id: TalentID_T.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();

        talent(id: TalentID_T.Luck, icon: icon("Luck"));
        CreateTalentModifierLuck(0.1f);

        talent(id: TalentID_T.AnkhShield, icon: IAI.FromItemID(ItemID.AnkhShield));
        CreateTalentModifierImmitationAccessory(ItemID.AnkhShield, m => m.AnkhShield = true);

        talent(id: TalentID_T.Charge2, icon: icon("Charge2"));
        CreateTalentModifierChargeTierTravaler();

        talent(id: TalentID_T.ImmoneChaosState, icon: IAI.FromItemID(ItemID.RodOfHarmony));
        CreateTalentModifierImmoneChaosState();

        talent(id: TalentID_T.WingsTime, icon: IAI.FromItemID(ItemID.LongRainbowTrailWings));
        CreateTalentModifierWingsTime(0.25f);

        talent(id: TalentID_T.ExploreMap, icon: IAI.FromItemID(ItemID.TrifoldMap));
        CreateTalentModifierExploreMap();



        ///////////////////////////////// MAGE

        CurrentInitBranchType = TBT.Magic;


        talent(id: TalentID_M.MaxMana1, icon: icon("MaxMana1"));
        CreateTalentModifierMaxMana(5);

        talent(id: TalentID_M.ManaCost1, icon: IAI.FromItemID(ItemID.NaturesGift));
        CreateTalentModifierDecreaseManaCost(-0.01f);

        talent(id: TalentID_M.Damage1, icon: icon("DamageMagic1"));
        CreateTalentModifierDamage(0.01f);
        OverrideHeader("Magic");


        talent(id: TalentID_M.GrapManaStars, IAI.FromItemID(ItemID.CelestialMagnet));
        CreateTalentModifierGrapManaStars(5f);


        talent(id: TalentID_M.MaxMana3, icon: icon("MaxMana2"));
        CreateTalentModifierMaxMana(10);

        talent(id: TalentID_M.ManaCost3, icon: IAI.FromItemID(ItemID.ManaFlower));
        CreateTalentModifierDecreaseManaCost(-0.02f);

        talent(id: TalentID_M.Damage3, icon: icon("DamageMagic2"));
        CreateTalentModifierDamage(0.015f);
        OverrideHeader("Magic");


        talent(id: TalentID_M.MoreMana, icon: icon("MoreMana"));
        CreateTalentModifierFactorManaIncrease(0.5f);


        talent(id: TalentID_M.MaxMana5, icon: icon("MaxMana3"));
        CreateTalentModifierMaxMana(15);

        talent(id: TalentID_M.ManaCost5, icon: IAI.FromItemID(ItemID.MagnetFlower));
        CreateTalentModifierDecreaseManaCost(-0.03f);

        talent(id: TalentID_M.Damage5, icon: icon("DamageMagic3"));
        CreateTalentModifierDamage(0.02f);
        OverrideHeader("Magic");


        talent(id: TalentID_M.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();


        talent(id: TalentID_M.ManaSickness, IAI.FromItemID(ItemID.ManaPotion));
        CreateTalentModifierBuffTime(BuffID.ManaSickness, -0.075f);

        talent(id: TalentID_M.ManaFromSpentMana, icon: IAI.FromItemID(ItemID.ManaCloak));
        CreateTalentModifierFactorManaDecrease(5f, 0.02f);


        talent(id: TalentID_M.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();


        talent(id: TalentID_M.DamageIsMaxMana, icon("DamageMagic4"));
        CreateTalentModifierIsMaxMana(0.1f, timeInSec: 3);


        talent(id: TalentID_M.FullManaRestoration, icon2("FullManaRestoration", 20));
        CreateTalentModifierFlag(
            "FullManaRestoration",
            (m) => m.FullManaRestoration = true,
            [Lang.GetBuffName(BuffID.ManaSickness), 5],
            ActivationType.Active);


        talent(id: TalentID_M.ExplosionByCrit, icon: IAI.FromItemID(ItemID.StickyBomb));
        CreateTalentModifierExplosionCrit(damage: 0.2f, radiusInTiles: 10, cooldown: 15 * 60, Color.Aqua, SoundID.Item20);


        talent(id: TalentID_M.WithoutCollisionProjectile, icon: icon("WithoutCollisionProjectile"));
        CreateTalentModifierWithoutCollisionProjectile();


        talent(id: TalentID_M.ProjectileAIM, icon("AIM"));
        CreateTalentModifierProjectileAIM(2f);


        talent(id: TalentID_M.Lifesteal, icon: icon2("Lifesteal", 48));
        CreateTalentModifierLifesteal(0.005f);



        ////////////// Summoner:

        CurrentInitBranchType = TBT.Summon;


        talent(id: TalentID_S.Damage1, IAI.FromItemID(ItemID.SlimeStaff));
        CreateTalentModifierDamage(0.01f);

        talent(id: TalentID_S.MorePet1, IAI.FromItemID(ItemID.PygmyNecklace));
        CreateTalentModifierMaxPets(1);

        talent(id: TalentID_S.Damage3, IAI.FromItemID(ItemID.FlinxStaff));
        CreateTalentModifierDamage(0.015f);

        talent(id: TalentID_S.StackAttack, IAI.FromItemID(ItemID.SpiderBreastplate));
        CreateTalentModifierStackAttack(baseDamagePerStack: 1, maxStack: 20);

        talent(id: TalentID_S.Damage5, IAI.FromItemID(ItemID.ImpStaff));
        CreateTalentModifierDamage(0.02f);

        talent(id: TalentID_S.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();

        talent(id: TalentID_S.DamageWhips, IAI.FromItemID(ItemID.ThornWhip));
        CreateTalentModifierDamageWhips(0.05f);

        talent(id: TalentID_S.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();

        talent(id: TalentID_S.DamageIsMaxLife, IAI.FromItemID(ItemID.Smolstar));
        CreateTalentModifierIsMaxLife(0.1f, timeInSec: 1);

        talent(id: TalentID_S.MorePet2, IAI.FromItemID(ItemID.NecromanticScroll));
        CreateTalentModifierMaxPets(1);

        talent(id: TalentID_S.AttackSpeedWhips, IAI.FromItemID(ItemID.MaceWhip));
        CreateTalentModifierAttackSpeedWhips(0.05f);

        talent(id: TalentID_S.MorePet3, IAI.FromItemID(ItemID.PapyrusScarab));
        CreateTalentModifierMaxPets(1);

        talent(id: TalentID_S.Lifesteal, icon: icon2("Lifesteal", 48));
        CreateTalentModifierLifesteal(0.002f);



        ///////////// Warlock

        CurrentInitBranchType = TBT.Warlock;


        talent(id: TalentID_WK.DamageWarlockBase, icon("DamageWarlockBase"));
        CreateTalentModifierDamageWarlockBase();

        talent(id: TalentID_WK.HealthCostWhenManaIsLow, icon2("HealthCostWhenManaIsLow", 20));
        CreateTalentModifierFlag("HealthCostWhenManaIsLow", m => m.HealthCostWhenManaIsLow = true, [Lang.GetItemName(ItemID.ManaFlower)], ActivationType.Passive);

        talent(id: TalentID_WK.LifestealIsLowHealth, icon2("Lifesteal2", 48));
        CreateTalentModifierLifestealIsLowHealth(value: 0.025f, healthThresholdBase: 0.15f, healthThresholdStep: 0.05f);

        talent(id: TalentID_WK.ManaHealing, icon("ManaHealing"));
        CreateTalentModifierManaHealing(tier: 1, debuffDurationInSec: 45, manaRegenerationDisabledDurationInSec: 15);

        talent(id: TalentID_WK.DamageIsLowHealth5, icon("DamageIsLowHealth1"));
        CreateTalentModifierDamageIsLowHealth(0.02f, minStatLifePercent: 0.6f);

        talent(id: TalentID_WK.Charge, icon: icon("Charge"));
        CreateTalentModifierChargeTier1();

        talent(id: TalentID_WK.DamageIsLowHealth7, icon("DamageIsLowHealth2"));
        CreateTalentModifierDamageIsLowHealth(0.03f, minStatLifePercent: 0.6f);

        talent(id: TalentID_WK.RodDiscord, icon: IAI.FromItemID(ItemID.RodofDiscord));
        CreateTalentModifierRodDiscord();

        talent(id: TalentID_WK.DamageIsLowHealth9, icon("DamageIsLowHealth3"));
        CreateTalentModifierDamageIsLowHealth(0.1f, minStatLifePercent: 0.2f);

        talent(id: TalentID_WK.HealthRegenMana, icon2("HealthRegenMana", 40));
        CreateTalentModifierFlag("HealthRegenMana", m => m.HealthRegenMana = true, [200], ActivationType.Active);

        talent(id: TalentID_WK.ProjectileAIM, icon("AIM"));
        CreateTalentModifierProjectileAIM(2f);

        talent(id: TalentID_WK.WithoutCollisionProjectile, icon: icon("WithoutCollisionProjectile"));
        CreateTalentModifierWithoutCollisionProjectile();

        talent(id: TalentID_WK.Lifesteal, icon: icon2("Lifesteal", 48));
        CreateTalentModifierLifesteal(0.002f);

        ////////////////////////////////////////////////
        ////////////////////////////////////////////////

        foreach (var branch in Enum.GetValues<TBT>())
        {
            LevelsTalentsTable[branch] = new();
            var table = LevelsTalentsTable[branch];
            foreach (var t in TalentsTable[branch].Values)
            {
                int level = t.Level;
                if (!table.ContainsKey(level))
                    table[level] = new();
                table[level].Add(t);
            }
        }
    }
    protected Talent InitializationTalent(
        string langKey,
        Func<DamageClass, ModifierRefresher> refreshActivatorGetter,
        Func<LT, int, object[]> descriptinoArgsGetter,
        ActivationType activationType = ActivationType.Passive)
    {
        var lt = GetLT(langKey);
        var type = GetDamageClass(CurrentInitBranchType);
        var damageDisplayName = GetDamageDisplayName(CurrentInitBranchType);
        TalentsTable[CurrentInitBranchType][CurrentInitTalentID] = new(
            id: CurrentInitTalentID,
            activateType: activationType,
            branchType: CurrentInitBranchType,
            icon: CurrentInitIcon,
            header: lt.header,
            descriptionGetter: (p) =>
            {
                var args = descriptinoArgsGetter(damageDisplayName, p);
                return args is null || args.Length == 0
                    ? lt.description
                    : lt.description.WithFormatArgs(args);
            },
            refreshActivator: refreshActivatorGetter(type));
        //////////////
        return GetCurrentInitTalent();
    }
    protected Talent GetCurrentInitTalent() => TalentsTable[CurrentInitBranchType][CurrentInitTalentID];

    protected (LT header, LT description) GetLT(string path)
    {
        LT header = UL.GoR_Talents($"{path}.Header");
        LT description = UL.GoR_Talents($"{path}.Description");
        return (header, description);
    }

    protected void OverrideHeader(string postPath)
    {
        ref LT header = ref GetCurrentInitTalent().Header;
        string key = header.Key;
        int index = key.IndexOf("Header");
        if (index == -1)
            return;
        key = key.Remove(index);
        header = Language.GetOrRegister($"{key}OverrideHeaders.{postPath}");
    }


    protected void CreateTalentModifierMaxPets(int value)
        => InitializationTalent(
            "MaxPets",
            type => (m, t, p) => t.StatsModifiersTable[type].MaxPets += value * p,
            (ddn, p) => [value * p]);
    protected void CreateTalentModifierMoveSpeed(float value)
        => InitializationTalent(
            "MoveSpeed",
            type => (m, t, p) => t.StatsModifiersTable[type].MoveSpeed += value * p,
            (ddn, p) => [ToStringNormalize(value * p, 0)]);

    protected void CreateTalentModifierLifeRegen(int value)
        => InitializationTalent(
            "LifeRegen",
            type => (m, t, p) => t.StatsModifiersTable[type].LifeRegen += value * p,
            (ddn, p) => [value * p]);

    protected void CreateTalentModifierLifeRegenOutOfCombat(int value, int timeInSecond)
    {
        var buffType = ModContent.BuffType<BuffOutOfCombat>();
        InitializationTalent(
            "LifeRegenOutOfCombat",
            type => (m, t, p) =>
            {
                StatsModifierByCondtion modif = new();
                modif.LifeRegen = value * p;
                modif.Conditon = (mp) =>
                {
                    Player player = mp.Player;
                    int idx = player.FindBuffIndex(buffType);
                    if (idx == -1)
                        return false;
                    return player.buffTime[idx] >= timeInSecond * 60;
                };
                t.StatsModifiersByCondtionTable[type].Add(modif);
            },
            (ddn, p) => [value * p, timeInSecond]);
    }

    protected void CreateTalentModifierWingsTime(float value)
        => InitializationTalent(
            "WingsTime",
            type => (m, t, p) => t.StatsModifiersTable[type].WingsTime += value * p,
            (ddn, p) => [ToStringNormalize(value * p, 0)]);

    protected void CreateTalentModifierImmuneTime(int value)
        => InitializationTalent(
            "ImmuneTime",
            type => (m, t, p) => t.StatsModifiersTable[type].ImmuneTime += value * p,
            (ddn, p) => [(value * p / 60f).ToStringF()]);

    protected void CreateTalentModifierLuck(float value)
        => InitializationTalent(
            "Luck",
            type => (m, t, p) => t.StatsModifiersTable[type].Luck += value * p,
            (ddn, p) => [(value * p).ToStringF()]);

    protected void CreateTalentModifierMaxLife(int value)
        => InitializationTalent(
            "MaxLife",
            type => (m, t, p) => t.StatsModifiersTable[type].MaxLife += value * p,
            (ddn, p) => [value * p]);

    protected void CreateTalentModifierMaxMana(int value)
        => InitializationTalent(
            "MaxMana",
            type => (m, t, p) => t.StatsModifiersTable[type].MaxMana += value * p,
            (ddn, p) => [value * p]);

    protected void CreateTalentModifierDecreaseManaCost(float value)
        => InitializationTalent(
            "DecreaseManaCost",
            type => (m, t, p) => t.StatsModifiersTable[type].ManaCost += value * p,
            (ddn, p) => [ToStringNormalize(Math.Abs(value * p), 0)]);

    protected void CreateTalentModifierArmor(float value)
        => InitializationTalent(
            "Armor",
            type => (m, t, p) => t.StatsModifiersTable[type].Armor += value * p,
            (ddn, p) => [(value * p).ToStringF(1)]);

    protected void CreateTalentModifierDamageBase(
        string langKey,
        Action<StatsModifier, int> damageModifierChanger,
        Func<int, object[]> descriptinoArgsGetter)
            => InitializationTalent(
                langKey,
                type => (m, t, p) => damageModifierChanger(t.StatsModifiersTable[type], p),
                (ddn, p) => [ddn, .. descriptinoArgsGetter(p)]);

    protected void CreateTalentModifierDamage(float value)
    {
        CreateTalentModifierDamageBase(
            "Damage",
            (m, p) => m.Damage += value * p,
            (p) => [ToStringNormalize(value * p)]);
    }

    protected void CreateTalentModifierDamageLastTargetWithinRadius(float value, int radiusInTiles)
    {
        InitializationTalent(
            "DamageLastTargetWithinRadius",
            type => (m, t, p) =>
            {
                float radius = radiusInTiles * 16f;
                StatsModifierByCondtion modif = new();
                modif.Damage = value * p;
                modif.Conditon = (player) =>
                {
                    if (player.LastTargetWhoAmI == -1)
                        return false;
                    NPC npc = Main.npc[player.LastTargetWhoAmI];
                    if (npc is null || !npc.active)
                        return false;
                    return Vector2.DistanceSquared(npc.Center, player.Player.Center) <= radius * radius;
                };
                t.StatsModifiersByCondtionTable[type].Add(modif);
            },
            (ddn, p) => [ddn, ToStringNormalize(value * p, 0), radiusInTiles]);
    }

    protected void CreateTalentModifierDamageWarlockBase()
    {
        float damageReduction = 0.95f;
        int bamageDamageIncrease = 1;
        int pointsMissingHealth = 13;
        float damageIncrease = 0.004f;
        InitializationTalent(
            "DamageWarlockBase",
            type => (m, t, p) =>
            {
                t.StatsModifiersTable[type].Damage -= damageReduction;
                DynamicStatsModifier modif = new();
                modif.ModifierChanger = (modPlayer, modif2) =>
                {
                    Player player = modPlayer.Player;
                    int missingHealth = player.statLifeMax2 - player.statLife;
                    float missingHealthPercent = (1f - (float)player.statLife / player.statLifeMax2) * 100f;
                    modif2.BaseDamage = bamageDamageIncrease * (missingHealth / (float)(pointsMissingHealth - p + 1));
                    modif2.Damage = damageIncrease * p * missingHealthPercent;
                };
                t.DynamicStatsModifiersTable[type].Add(modif);
            },
            (ddn, p) => [
                ddn, ToStringNormalize(damageReduction, 0),
                bamageDamageIncrease, pointsMissingHealth - p + 1,
                ToStringNormalize(damageIncrease * p, 2), 1]);
    }

    protected void CreateTalentModifierDamageIsLowHealth(float value, float minStatLifePercent)
        => InitializationTalent(
            "DamageIsLowHealth",
            type => (m, t, p) =>
            {
                StatsModifierByCondtion modif = new();
                modif.Damage = value * p;
                modif.Conditon = modPlayer =>
                {
                    Player player = modPlayer.Player;
                    float healthPercent = (float)player.statLife / player.statLifeMax2;
                    return healthPercent <= minStatLifePercent;
                };
                t.StatsModifiersByCondtionTable[type].Add(modif);
            },
            (ddn, p) => [
                ToStringNormalize(value * p, 0), ddn,
                ToStringNormalize(minStatLifePercent, 0)]);

    protected void CreateTalentModifierCritChance(float value)
        => CreateTalentModifierDamageBase(
            "CritChance",
            (m, p) => m.CritChance += value * p,
            (p) => [(value * p).ToStringF(1)]);

    protected void CreateTalentModifierCritDamage(float value)
        => CreateTalentModifierDamageBase(
            "CritDamage",
            (m, p) => m.CritDamage += value * p,
            (p) => [ToStringNormalize(value * p)]);

    protected void CreateTalentModifierAttackSpeed(float value)
        => CreateTalentModifierDamageBase(
            "AttackSpeed",
            (m, p) => m.AttackSpeed += value * p,
            (p) => [ToStringNormalize(value * p)]);

    protected void CreateTalentModifierArmorPenetration(float value)
        => CreateTalentModifierDamageBase(
            "ArmorPenetration",
            (m, p) => m.ArmorPenetration += value * p,
            (p) => [(value * p).ToStringF()]);

    protected void CreateTalentModifierKnokback(float value)
        => CreateTalentModifierDamageBase(
            "Knokback",
            (m, p) => m.Knokback += value * p,
            (p) => [ToStringNormalize(value * p)]);


    protected void CreateTalentModifierByConditionBase(
        string langKey,
        Action<StatsModifierByCondtion, int> damageModifierChanger,
        Func<int, object[]> descriptionArgsGetter)
            => InitializationTalent(
                langKey,
                (type) => (m, t, p) =>
                {
                    StatsModifierByCondtion modif = new();
                    damageModifierChanger(modif, p);
                    t.StatsModifiersByCondtionTable[type].Add(modif);
                },
                (ddn, p) => [ddn, .. descriptionArgsGetter(p)]);

    protected void CreateTalentModifierAttackSpeedWhips(float value)
        => CreateTalentModifierByConditionBase(
            "AttackSpeedWhips",
            (m, p) =>
            {
                m.AttackSpeed = value * p;
                m.ModifyWeaponStatsCondition = (p, i) => i.DamageType.CountsAsClass(DamageClass.SummonMeleeSpeed);
            },
            (p) => [ToStringNormalize(value * p, 1)]);

    protected void CreateTalentModifierDamageWhips(float value)
        => CreateTalentModifierByConditionBase(
            "DamageWhips",
            (m, p) =>
            {
                m.Damage = value * p;
                m.ModifyWeaponStatsCondition = (p, i) => i.DamageType.CountsAsClass(DamageClass.SummonMeleeSpeed);
            },
            (p) => [ToStringNormalize(value * p, 1)]);

    protected void CreateTalentModifierIsMaxMana(float value, int timeInSec)
        => CreateTalentModifierByConditionBase(
            "DamageIsMaxMana",
            (m, p) =>
            {
                m.Conditon = pat => pat.CurrentMaxManaTimer <= timeInSec * 60;
                m.Damage = value * p;
            },
            p => [ToStringNormalize(value * p, 0), timeInSec]);
    protected void CreateTalentModifierIsMaxLife(float value, int timeInSec)
        => CreateTalentModifierByConditionBase(
            "DamageIsMaxLife",
            (m, p) =>
            {
                m.Conditon = pat => pat.CurrentMaxLifeTimer <= timeInSec * 60;
                m.Damage = value * p;
            },
            p => [ToStringNormalize(value * p, 0), timeInSec]);

    protected void CreateTalentModifierIsStandingStill(
        string langKey,
        Action<StatsModifierByCondtion, int> damageModifierChanger,
        Func<int, object[]> descriptionArgsGetter)
            => CreateTalentModifierByConditionBase(
                langKey,
                (m, p) =>
                {
                    m.Conditon = (pat) => pat.Player.velocity.LengthSquared() < 0.01f;
                    damageModifierChanger(m, p);
                },
                descriptionArgsGetter);

    protected void CreateTalentModifierDamageIsStandingStill(float value)
        => CreateTalentModifierIsStandingStill(
            "DamageIsStandingStill",
            (m, p) => m.Damage = value * p,
            (p) => [ToStringNormalize(value * p, 1)]);

    protected void CreateTalentModifierAttackSpeedIsStandingStill(float value)
        => CreateTalentModifierIsStandingStill(
            "AttackSpeedIsStandingStill",
            (m, p) => m.AttackSpeed = value * p,
            (p) => [ToStringNormalize(value * p)]);

    protected void CreateTalentModifierCritChanceIsStandingStill(float value)
        => CreateTalentModifierIsStandingStill(
            "CritChanceIsStandingStill",
            (m, p) => m.CritChance = value * p,
            (p) => [(value * p).ToStringF(1)]);

    protected void CreateTalentModifierStackAttack(int baseDamagePerStack, int maxStack)
        => InitializationTalent(
            "StackAttack",
            type => (m, t, p) =>
            {
                StackAttackModifierHelper h = new(tier: 1, points: p);
                h.Initialization(maxStack);
                t.StackAttackModifierHelperTable[type].Add(h);

                DynamicStatsModifier modif = new();
                modif.ModifierChanger = (player, modif2) => modif2.BaseDamage = player.CurrentStackAttack * (baseDamagePerStack * p);
                t.DynamicStatsModifiersTable[type].Add(modif);
            },
            (ddn, p) => [ddn, baseDamagePerStack, maxStack]);

    protected void CreateTalentModifierLifesteal(float value)
        => InitializationTalent(
            "Lifesteal",
            type => (m, t, p) => t.StatsModifiersTable[type].Lifesteal += value * p,
            (ddn, p) => [ddn, ToStringNormalize(value * p, 4)]);

    protected void CreateTalentModifierLifestealIsLowHealth(float value, float healthThresholdBase, float healthThresholdStep)
    {
        InitializationTalent(
            "LifestealIsLowHealth",
            type => (m, t, p) =>
            {
                StatsModifierByCondtion modif = new();
                modif.Lifesteal = value;
                modif.Conditon = (mp) 
                    => healthThresholdBase + healthThresholdStep * (p - 1) >= (float)mp.Player.statLife / mp.Player.statLifeMax2;
                t.StatsModifiersByCondtionTable[type].Add(modif);
            },
            (ddn, p) => [
                ddn, ToStringNormalize(value, 2),
                ToStringNormalize(healthThresholdBase + healthThresholdStep * (p - 1), 0)]);
    }

    protected void CreateTalentModifierChanceNotConsumeAmmo(float value)
        => InitializationTalent(
            "ChanceNotConsumeAmmo",
            type => (m, t, p) => m.ChanceNotConsumeAmmo += value * p,
            (ddn, p) => [(value * p).ToStringF(0)]);

    protected void CreateTalentModifierWithoutCollisionProjectile()
        => InitializationTalent(
            "WithoutCollisionProjectile",
            type => (m, t, p) => t.WithoutCollisionProjectileTable[type] = true,
            (ddn, p) => [ddn],
            ActivationType.Switchable);

    protected void CreateTalentModifierExplosionBase(
        ExplosionModifiers.ExplosionTrigget explType,
        int tier,
        Action<ExplosionModifiers, int> initializator)
    {
        string langKey = explType switch
        {
            ExplosionModifiers.ExplosionTrigget.HealFactor => "HealExplosion",
            ExplosionModifiers.ExplosionTrigget.ManaFactor => "ManaExplosion",
            ExplosionModifiers.ExplosionTrigget.CritFactor => "ExplosionByCrit",
            _ => throw new Exception($"CreateTalentModifierExplosionBase. Invalid explType = {explType}")
        };

        InitializationTalent(
            langKey,
            (type) => (m, t, p) =>
            {
                var expl = GetExplosionModifiers(p);
                t.ExplosionModifiersTable[type].Add(expl);
            },
            (ddn, p) =>
            {
                var expl = GetExplosionModifiers(p);
                return
                    [ddn,
                    ToStringNormalize(expl.Damage),
                    expl.Radius / 16,
                    (int)(expl.Cooldown / 60f)];
            });
        ExplosionModifiers GetExplosionModifiers(int p)
        {
            ExplosionModifiers expl = new(explType, tier, p);
            initializator(expl, p);
            return expl;
        }
    }

    protected void CreateTalentModifierExplosionHeal(float damage, int radiusInTiles, int cooldown, Color color, SoundStyle sound)
        => CreateTalentModifierExplosionBase(
            ExplosionModifiers.ExplosionTrigget.HealFactor,
            tier: 1,
            (m, p) => m.Initialize(damage * p, radiusInTiles, cooldown, color, sound));

    protected void CreateTalentModifierExplosionMana(float damage, int radiusInTiles, int cooldown, Color color, SoundStyle sound)
        => CreateTalentModifierExplosionBase(
            ExplosionModifiers.ExplosionTrigget.ManaFactor,
            tier: 1,
            (m, p) => m.Initialize(damage * p, radiusInTiles, cooldown, color, sound));

    protected void CreateTalentModifierExplosionCrit(float damage, int radiusInTiles, int cooldown, Color color, SoundStyle sound)
        => CreateTalentModifierExplosionBase(
            ExplosionModifiers.ExplosionTrigget.CritFactor,
            tier: 1,
            (m, p) => m.Initialize(damage * p, radiusInTiles, cooldown, color, sound));


    protected void CreateTalentModifierFactorBase(
        FactorModifier.FactorType factorType,
        FactorModifier.TriggerType triggerType,
        Action<FactorModifier, int> initializator)
    {
        string langKey = factorType switch
        {
            FactorModifier.FactorType.Health => "HealFactor",
            FactorModifier.FactorType.Mana => "ManaFactor",
            _ => throw new Exception($"CreateTalentModifierFactor. Invalid factorType = {factorType}")
        };
        langKey += ".";
        langKey += triggerType switch
        {
            FactorModifier.TriggerType.Increase => "Increase",
            FactorModifier.TriggerType.Decrease => "Decrease",
            _ => throw new Exception($"CreateTalentModifierFactor. Invalid triggetType = {triggerType}")
        };

        var branch = CurrentInitBranchType;
        InitializationTalent(
            langKey,
            type => (m, t, p) =>
            {
                var modifier = GetFactorModifier(p);
                t.FactorModifiersTable[branch].Add(modifier);
            },
            (ddn, p) =>
            {
                var modifier = GetFactorModifier(p);
                string arg1 = modifier.Chance >= 1f ? "" : ToStringNormalize(modifier.Chance);
                string arg2 = ToStringNormalize(modifier.Factor, 0);
                return arg1 == string.Empty ? [arg2] : [arg1, arg2];
            });
        FactorModifier GetFactorModifier(int p)
        {
            FactorModifier expl = new(factorType, triggerType);
            initializator(expl, p);
            return expl;
        }
    }

    protected void CreateTalentModifierFactorHealIncrease(float value)
        => CreateTalentModifierFactorBase(
            FactorModifier.FactorType.Health,
            FactorModifier.TriggerType.Increase,
            (m, p) => m.Initialize(count: 10, factor: value * p, chance: 1f));

    protected void CreateTalentModifierFactorManaIncrease(float value)
        => CreateTalentModifierFactorBase(
            FactorModifier.FactorType.Mana,
            FactorModifier.TriggerType.Increase,
            (m, p) => m.Initialize(count: 10, factor: value * p, chance: 1f));

    protected void CreateTalentModifierFactorManaDecrease(float value, float chance)
        => CreateTalentModifierFactorBase(
            FactorModifier.FactorType.Mana,
            FactorModifier.TriggerType.Decrease,
            (m, p) => m.Initialize(count: 3, factor: value, chance: chance * p));


    protected void CreateTalentModifierBuffTime(int buffID, float value)
    {
        bool increase = value > 0f;
        InitializationTalent(
            increase ? "BuffTimeIncreased" : "BuffTimeReduced",
            (type) => (m, t, p) =>
            {
                var table = m.BuffModifierTable;
                if (!table.ContainsKey(buffID))
                    table[buffID] = 1f;
                table[buffID] = Math.Max(table[buffID] + value * p, 0f);
            },
            (ddn, p) => [Lang.GetBuffName(buffID), ToStringNormalize(Math.Abs(value * p))]);
    }


    protected void CreateTalentModifierExploreMap()
    {
        InitializationTalent(
            "ExploreMap",
            type => (m, t, p) => m.ExploreMap = true,
            (ddn, p) => null,
            ActivationType.Active);
    }


    protected void CreateTalentModifierCharge(
        int tier,
        Action<DashModifiers, int> dushInitializator)
    {
        InitializationTalent(
            "Charge",
            type => (m, t, p) =>
            {
                if (p == 0)
                    return;
                t.DashModifiers.Add(GetCurrentDush(p));
            },
            (ddn, p) =>
            {
                var d = GetCurrentDush(p);
                return
                    [d.Speed,
                    (d.ImmuneTime / 60f).ToStringF(2),
                    (d.Cooldown / 60f).ToStringF(1)];
            },
            ActivationType.Active);
        //////////////////////////////
        DashModifiers GetCurrentDush(int p)
        {
            DashModifiers dash = new(tier, p);
            dushInitializator(dash, p);
            return dash;
        }
    }

    protected void CreateTalentModifierChargeTier1()
        => CreateTalentModifierCharge(tier: 1, (d, p)
            => d.Initialization(speed: 13f, cooldown: 90, immuneTime: 30));

    protected void CreateTalentModifierChargeTierTravaler()
        => CreateTalentModifierCharge(tier: 2, (d, p)
            => d.Initialization(speed: 13f + 2f * p, cooldown: 90, immuneTime: 30 + 5 * p));


    protected void CreateTalentModifierRodDiscord()
        => InitializationTalent(
            "RodDiscord",
            type => (m, t, p) => m.RodDiscord = true,
            (ddn, p) => [Lang.GetItemName(ItemID.RodofDiscord), Lang.GetBuffName(BuffID.ChaosState)],
            ActivationType.Active);

    protected void CreateTalentModifierImmoneChaosState()
        => InitializationTalent(
            "ImmoneChaosState",
            type => (m, t, p) => m.ImmuneChaosState = true,
            (ddn, p) => [Lang.GetBuffName(BuffID.ChaosState)]);


    protected void CreateTalentModifierAmmoSwith(int tier)
        => InitializationTalent(
            "AmmoSwith",
            type => (m, t, p) => t.SwitchAmmoModifiers.Add(new SwitchAmmoModifier(tier, 1)),
            (ddn, p) =>
            {
                var projArrowId = SwitchAmmoModifier.GetArrowProjectileTypeByTier(tier);
                var projBulletId = SwitchAmmoModifier.GetBulletProjectileTypeByTier(tier);
                if (projArrowId == -1 || projBulletId == -1)
                    throw new Exception($"CreateTalentModifierAmmoSwith error: projArrowId = {projArrowId}; projBulletId = {projBulletId}");
                return [Lang.GetProjectileName(projArrowId),
                    tier == 3
                    ? Lang.GetItemName(ItemID.ChlorophyteBullet)
                    : Lang.GetProjectileName(projBulletId)];
            },
            ActivationType.Switchable);


    protected void CreateTalentModifierProjectileAIM(float value)
        => InitializationTalent(
            "ProjectileAIM",
            type => (m, t, p) => t.AIMHomingTurnRateTable[type] = value * p,
            (ddn, p) => [ddn, (value * p).ToStringF(1), TalentsModifers.AIMTargetingRadiusInTiles],
            ActivationType.Switchable);


    protected void CreateTalentModifierFlag(string langKey, Action<TalentsModifers> action, object[] descriptionArgs, ActivationType activationType)
        => InitializationTalent(
            langKey,
            type => (m, t, p) => action(m),
            (ddn, p) => descriptionArgs,
            activationType);
    protected void CreateTalentModifierImmitationAccessory(int itemID, Action<TalentsModifers> action)
        => CreateTalentModifierFlag("AccessorysEffect", action, [Lang.GetItemName(itemID)], ActivationType.Passive);
    protected void CreateTalentModifierImmitationBuff(int buffID, Action<TalentsModifers> action)
        => CreateTalentModifierFlag("BuffsEffect", action, [Lang.GetBuffName(buffID)], ActivationType.Passive);

    protected void CreateTalentModifierNoFallDmg()
        => InitializationTalent(
            "NoFallDmg",
            type => (m, t, p) => m.NoFallDmg = true,
            (ddn, p) => null);

    protected void CreateTalentModifierChanceDropGoldOnKill(int value)
        => InitializationTalent(
            "ChanceDropGoldOnKill",
            type => (m, t, p) => m.ChanceDropGoldOnKill += value * p,
            (ddn, p) => [value * p]);


    protected void CreateTalentModifierDebuffsOnDamage(int[] debuffs)
    {
        InitializationTalent(
            "DebuffsOnDamage",
            type => (m, t, p) => t.DebuffsOnDamageTable[type].AddRange(GetCurrentDebuffs(p)),
            (ddn, p) =>
            {
                var debuffs = GetCurrentDebuffs(p);
                int length = debuffs.Length;
                string args = "";
                for (int i = 0; i < length; i++)
                {
                    args += $"{Lang.GetBuffName(debuffs[i])}";
                    if (i != length - 1)
                        args += '\n';
                }
                return [ddn, TalentsModifers.DebuffsOnDamageTime / 60, args];
            });

        int[] GetCurrentDebuffs(int points)
        {
            int max = Math.Min(points, debuffs.Length);
            int[] result = new int[max];
            for (int i = 0; i < max; i++)
                result[i] = debuffs[i];
            return result;
        }
    }

    protected void CreateTalentModififerShadowDodge(int baseCooldownInSec, int cooldownReductionInSec)
        => InitializationTalent(
            "ShadowDodge",
            type => (m, t, p) => m.ShadowDodgeCooldown = (baseCooldownInSec - cooldownReductionInSec * (p - 1)) * 60,
            (ddn, p) => [baseCooldownInSec - cooldownReductionInSec * (p - 1)]);


    protected void CreateTalentModifierSpawnRate(float value)
    {
        bool increase = value > 1f;
        InitializationTalent(
            "SpawnRate",
            type => (m, t, p) => m.SpawnRate = PrepareValue(p),
            (ddn, p) => [PrepareValue(p).ToStringF(1)],
            ActivationType.Switchable);

        float PrepareValue(int points)
        {
            float result = increase
                ? 1f + (value - 1f) * points
                : 1f - (1f - value) * points;
            return result <= 0f ? 0.1f : result;
        }
    }

    protected void CreateTalentModifierGrapManaStars(float value)
        => InitializationTalent(
            "GrapManaStars",
            type => (m, t, p) =>
            {
                value = 1f + (value - 1f) * p;
                t.ItemsGrapRangeTable[type][ItemID.Star] = value;
                t.ItemsGrapRangeTable[type][ItemID.SoulCake] = value;
                t.ItemsGrapRangeTable[type][ItemID.SugarPlum] = value;
            },
            (ddn, p) => [Lang.GetItemName(ItemID.Star), ToStringNormalize(value * p), Lang.GetItemName(ItemID.CelestialMagnet)]);

    protected void CreateTalentModifierManaHealing(int tier, int debuffDurationInSec, int manaRegenerationDisabledDurationInSec)
        => InitializationTalent(
            "ManaHealing",
            type => (m, t, p) => 
            {
                ManaHealignModifier modif = new(tier, p);
                modif.Initialize(debuffDurationInSec, manaRegenerationDisabledDurationInSec);
                t.ManaHealignModifiers.Add(modif);
            },
            (ddn, p) => [
                ModContent.GetModBuff(ModContent.BuffType<BuffManaHealingCooldown>()).DisplayName,
                debuffDurationInSec,
                manaRegenerationDisabledDurationInSec],
            ActivationType.Active);


    protected string ToStringNormalize(float value, int count = 4)
    {
        string result = (value * 100).ToStringF(count);
        if (result.IndexOf('.') != -1)
            return result.TrimEnd('0').TrimEnd('.');
        return result;
    }
}

public class Talent
{
    public enum ActivationType : byte
    {
        Passive,
        Active,
        Switchable
    }

    public delegate LT DescriptionByPointsTalentGetter(int points);
    public delegate void ModifierRefresher(
        TalentsModifers modifiers,
        TemporaryRefreshModifiers tempModifires,
        int points);

    public readonly int ID;
    public readonly int Level;
    public readonly ActivationType ActivateType;
    public readonly TBT BranchType;
    public readonly IAI Icon;
    public LT Header;
    public readonly DescriptionByPointsTalentGetter DescriptionGetter;
    public readonly ModifierRefresher RefreshActivator;
    public Talent(
        int id,
        ActivationType activateType,
        TBT branchType,
        IAI icon,
        LT header,
        DescriptionByPointsTalentGetter descriptionGetter,
        ModifierRefresher refreshActivator)
    {
        ID = id;
        Level = TalentsManager.Instance.TalentLevelTable[branchType][id];
        ActivateType = activateType;
        BranchType = branchType;
        Icon = icon;
        Header = header;
        DescriptionGetter = descriptionGetter;
        if (refreshActivator is null)
            refreshActivator = (_, _, _) => { };
        RefreshActivator = refreshActivator;
    }
}