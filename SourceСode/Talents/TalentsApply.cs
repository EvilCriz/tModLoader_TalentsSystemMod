using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.IO;
using TalentsSystem.Content;
using TalentsSystem.Modifiers;
using Terraria;
using Terraria.Audio;
using Terraria.DataStructures;
using Terraria.GameInput;
using Terraria.ID;
using Terraria.ModLoader;
using MPC = TalentsSystem.Utils.UtilsModPacketPrimarySourceClient;
using MPS = TalentsSystem.Utils.UtilsModPacketPrimarySourceServer;
using PTT = TalentsSystem.Utils.UtilsModPacketPrimarySourceClient.ProcessTriggersType;
using TS = TalentsSystem.TalentsSystem;

namespace TalentsSystem;

public class TalentsApplyModPlayer : ModPlayer
{
    private TalentsModifers _modifiers = null;
    /// <summary>
    ///     Talent modifiers.
    ///     This value is always synchronized on both sides.
    /// </summary>
    public TalentsModifers Modifiers
    {
        get
        {
            if (_modifiers is null)
                _modifiers = TalentsModifers.Empty();
            return _modifiers;
        }
        protected set => _modifiers = value;
    }
    private StatsModifier _currentStatsModifier = null;
    public StatsModifier CurrentStatsModififer
    {
        get
        {
            if (_currentStatsModifier is null)
                _currentStatsModifier = StatsModifier.GetFinal(this, Modifiers);
            return _currentStatsModifier;
        }
        protected set { _currentStatsModifier = value; }
    }
    public DamageClass CurrentDamageClass { get; protected set; } = DamageClass.Default;
    //////////////////////
    public Dictionary<int, int> BuffDurationTable = new();
    //////////////////////
    protected int LastMana;
    protected int LastManaMax;
    protected int LastHeath;
    protected int LastHeathMax;
    public int LastTargetWhoAmI = -1;
    //////////////////////
    protected int CurrentDashCooldown;
    private int _currentExplosionCooldown;
    public int CurrentExplosionCooldown { get => _currentExplosionCooldown; protected set => _currentExplosionCooldown = value; }
    protected int CurrentShadowDodgeCooldown;
    private bool _lastShadowDodge;
    private int _currentMaxManaTimer;
    private int _currentMaxLifeTimer;
    public int CurrentMaxManaTimer { get => _currentMaxManaTimer; protected set => _currentMaxManaTimer = value; }
    public int CurrentMaxLifeTimer { get => _currentMaxLifeTimer; protected set => _currentMaxLifeTimer = value; }
    //////////////////////
    public int CurrentAccumulatedLifestealDamage = 0;
    //////////////////////
    public bool CurrentBindSandStorm = true;
    public bool CurrentBindAIMProjectile = true;
    public bool CurrentBindWithoutCollisionProjectile = true;
    public bool CurrentBindEditSpawnRate = false;
    //////////////////////
    public int CurrentStackAttack;
    //////////////////////
    public byte LastCurrentArrow;
    public byte LastCurrentBullet;
    /////////////////////
    private bool _reincarnationActivated;
    private bool _fullManaRestorationActivated;
    private bool _manaHealingActivated;
    private bool _healthRegenManaActivated;
    /////////////////////
    protected bool _lastDirectionRight;

    /// <summary>
    ///     Called on all network sides.
    /// </summary>
    public void RefreshModifiers()
    {
        ModDebug.Write($"RefreshModifiers -> Player.whoAmI = {Player.whoAmI}", [DMT.MPSC]);
        var save = Player.GetModPlayer<TalentsSaveInfo>();
        if (save is null)
            return;
        //////////////////
        BuffDurationTable.Clear();
        UpdateOutOfCombat(clearTime: false, clear: true);
        //////////////////
        if (Modifiers is not null)
        {
            if (Modifiers.SwitchAmmoModifier.Tier > 0)
            {
                LastCurrentArrow = Modifiers.SwitchAmmoModifier.CurrentArrow;
                LastCurrentBullet = Modifiers.SwitchAmmoModifier.CurrentBullet;
            }
            if (Player.HasBuff(BuffID.ShadowDodge))
                Player.ClearBuff(BuffID.ShadowDodge);
        }
        CurrentDamageClass = TalentsManager.GetDamageClass(save.CurrentBranch);
        if (CurrentDamageClass == ModContent.GetInstance<WarlockDamageClass>())
            CurrentDamageClass = DamageClass.Magic;
        Modifiers = new();
        Modifiers.RefreshModifiers(save, this);
    }

    /// <summary>
    ///    This method is called upon entering the game on all network sides - at the moment the player's talent points are synchronized.
    /// </summary>
    public override void OnEnterWorld()
    {
        Reset();
        RefreshModifiers();
        UpdateTimers(respawn: true);
        CurrentStackAttack = 0;
    }
    public override void OnRespawn()
    {
        UpdateTimers(respawn: true);
        Reset();
        CurrentStackAttack = 0;
    }
    public void Reset()
    {
        LastMana = Player.statMana;
        LastManaMax = Player.statManaMax2;
        LastHeath = Player.statLife;
        LastHeathMax = Player.statLifeMax2;
    }
    public override void ResetEffects()
    {
        UpdateTimers(respawn: false);
        _currentStatsModifier = null;
    }


    public void UpdateTimers(bool respawn)
    {
        UpdateMaxStatTimer(ref _currentMaxManaTimer, Player.statMana, Player.statManaMax2);
        UpdateMaxStatTimer(ref _currentMaxLifeTimer, Player.statLife, Player.statLifeMax2);
        UpdateCooldown(ref _currentExplosionCooldown);
        UpdateCooldown(ref CurrentShadowDodgeCooldown);
        UpdateCooldown(ref CurrentDashCooldown);
        if (respawn)
        {
            _lastShadowDodge = false;
        }
        void UpdateMaxStatTimer(ref int timer, int stat, int maxStat)
        {
            if (stat >= maxStat)
                timer = 0;
            else
                timer++;
        }
        void UpdateCooldown(ref int cooldown)
        {
            if (cooldown > 0)
                cooldown = respawn ? 0 : cooldown - 1;
        }
    }

    public override void PreUpdateBuffs()
    {
        ApplyFlag(Modifiers.NightVisionPotion, ref Player.nightVision);
        ApplyFlag(Modifiers.GillsPotion, ref Player.gills);
        ApplyFlag(Modifiers.SpelunkerPotion, ref Player.findTreasure);
        ApplyFlag(Modifiers.NoFallDmg, ref Player.noFallDmg);
        ///////////////////////////
        ApplyFlag2(Modifiers.SandStorm && CurrentBindSandStorm, () => Player.GetJumpState<SandstormInABottleJump>().Enable());
        ApplyFlag2(Modifiers.MiningPotion, () => Player.pickSpeed -= 0.25f);
        ApplyFlag2(Modifiers.ShinePotion, () => Lighting.AddLight(Player.Center, 0.8f, 0.95f, 1f));
        ApplyFlag2(Modifiers.ObsidianSkinPotion, () =>
        {
            Player.lavaImmune = true;
            Player.fireWalk = true;
            Player.buffImmune[BuffID.OnFire] = true;
        });
        ApplyFlag2(Modifiers.AnkhShield, () =>
        {
            Player.noKnockback = true;

            Player.buffImmune[BuffID.Bleeding] = true;
            Player.buffImmune[BuffID.BrokenArmor] = true;
            Player.buffImmune[BuffID.Burning] = true;
            Player.buffImmune[BuffID.Confused] = true;
            Player.buffImmune[BuffID.Cursed] = true;
            Player.buffImmune[BuffID.Darkness] = true;
            Player.buffImmune[BuffID.Poisoned] = true;
            Player.buffImmune[BuffID.Silenced] = true;
            Player.buffImmune[BuffID.Slow] = true;
            Player.buffImmune[BuffID.Weak] = true;
            Player.buffImmune[BuffID.Chilled] = true;
            Player.buffImmune[BuffID.Stoned] = true;
        });
        void ApplyFlag(bool flag, ref bool stat)
        {
            if (flag)
                stat = true;
        }
        void ApplyFlag2(bool flag, Action action)
        {
            if (flag)
                action();
        }
        ///////////////////////////
        Player.statLifeMax2 += CurrentStatsModififer.MaxLife;
        Player.statManaMax2 += CurrentStatsModififer.MaxMana;
        Player.statDefense += (int)CurrentStatsModififer.Armor;
        if (Player.wingTimeMax > 0)
            Player.wingTimeMax = (int)(Player.wingTimeMax * (1f + CurrentStatsModififer.WingsTime));
        Player.maxMinions += CurrentStatsModififer.MaxPets;
    }
    public override void PostUpdateBuffs()
    {
        int sdCooldown = Modifiers.ShadowDodgeCooldown;
        if (sdCooldown > 0)
        {
            bool hasBf = Player.HasBuff(BuffID.ShadowDodge);
            if (_lastShadowDodge && !hasBf)
            {
                _lastShadowDodge = false;
                CurrentShadowDodgeCooldown = sdCooldown;
            }

            Player.onHitDodge = true;
            if (CurrentShadowDodgeCooldown <= 0)
            {
                _lastShadowDodge = true;
                CurrentShadowDodgeCooldown = int.MaxValue;
                Player.AddBuff(BuffID.ShadowDodge, sdCooldown * 1000, quiet: true);
            }
        }
        ////////////////////////////
        UpdateOutOfCombat(clearTime: false, clear: false);
        ////////////////////////////
        if (Player == Main.LocalPlayer)
        {
            var spawnRateBuffType = ModContent.BuffType<BuffSpawnRate>();
            if (Modifiers.SpawnRate != 0f && CurrentBindEditSpawnRate && !Player.HasBuff(spawnRateBuffType))
                Player.AddBuff(spawnRateBuffType, 60, quiet: true);
            else if ((Modifiers.SpawnRate == 0f || !CurrentBindEditSpawnRate) && Player.HasBuff(spawnRateBuffType))
                SyncClearBuff(spawnRateBuffType);
        }
        ////////////////////////////
        if (Player == Main.LocalPlayer)
        {
            foreach (var buffModif in Modifiers.BuffModifierTable)
            {
                var type = buffModif.Key;
                int id = Player.FindBuffIndex(type);
                bool contains;
                if (id == -1)
                {
                    contains = BuffDurationTable.ContainsKey(type);
                    if (contains)
                        BuffDurationTable.Remove(type);
                    continue;
                }
                ///////
                int time = Player.buffTime[id];
                if (time < 5)
                    continue;
                ///////
                contains = BuffDurationTable.ContainsKey(type);
                int lastTime = contains ? BuffDurationTable[type] : 0;
                if (lastTime >= time)
                    continue;
                ///////
                int delta = time - lastTime;
                if (delta < 5)
                    continue;
                ///////
                Player.buffTime[id] = (int)Math.Clamp(time - delta * (1f - buffModif.Value), 3, int.MaxValue);
                BuffDurationTable[type] = Player.buffTime[id];
                ///////
                if (id == BuffID.PotionSickness)
                {
                    Player.potionDelay = Player.buffTime[id];
                }
            }

            //foreach (var buff in BuffDurationTable)
            //{
            //    int type = buff.Key;
            //    int id = Player.FindBuffIndex(type);
            //    if (id == -1)
            //    {
            //        BuffDurationTable[type] = 0;
            //        continue;
            //    }
            //    int diff = Player.buffTime[id] - buff.Value;
            //    if (diff <= 0)
            //        continue;
            //    Player.buffTime[id] = (int)Math.Clamp(Player.buffTime[id] * Modifiers.BuffModifierTable[type], 1, int.MaxValue);
            //    BuffDurationTable[type] = Player.buffTime[id];
            //    if (TS.IsServer())
            //    {
            //        ModDebug.Write($"BuffDurationChange: type = {type}; diff = {diff}; buff.Value = {buff.Value}; Modifiers.BuffModifierTable[type] = {Modifiers.BuffModifierTable[type]}", [DMT.MPSS]);
            //        MPS.Send_UpdateBuffTime(Player.whoAmI, type, Player.buffTime[id]);
            //    }
            //    if (id == BuffID.PotionSickness)
            //    {
            //        Player.potionDelay = Player.buffTime[id];
            //        if (TS.IsServer())
            //        {
            //            MPS.Send_PotionDelay(Player.whoAmI, Player.potionDelay);
            //        }
            //    }
            //}
        }
    }
    public override void UpdateEquips()
    {
        var dmgType = CurrentDamageClass;
        Player.GetDamage(dmgType).Base += CurrentStatsModififer.BaseDamage;
        Player.GetDamage(dmgType) += CurrentStatsModififer.Damage;
        Player.GetCritChance(dmgType) += CurrentStatsModififer.CritChance;
        Player.GetArmorPenetration(dmgType) += CurrentStatsModififer.ArmorPenetration;
        Player.GetAttackSpeed(dmgType) += CurrentStatsModififer.AttackSpeed;
        Player.GetKnockback(dmgType) += CurrentStatsModififer.Knokback;
    }
    public override void PostUpdate()
    {
        if (Player == Main.LocalPlayer)
        {
            if (Modifiers.ImmuneChaosState && Player.statLife == Player.statLifeMax2)
                Player.buffImmune[BuffID.ChaosState] = true;
        }
        if (TS.IsNotMC())
        {
            if (_reincarnationActivated)
                ActivateReincarnation();
            if (_fullManaRestorationActivated)
                ActivateFullManaRestoration();
            if (_manaHealingActivated)
                ActivateManaHealing();
            if (_healthRegenManaActivated)
                ActivateHealthRegenMana();
            //////////////////////////
            if (Player.statLife >= Player.statLifeMax2)
            {
                CurrentAccumulatedLifestealDamage = 0;
            }
            else if (CurrentAccumulatedLifestealDamage > 0)
            {
                int delta = (int)(CurrentAccumulatedLifestealDamage * CurrentStatsModififer.Lifesteal);
                if (delta > 0)
                {
                    CurrentAccumulatedLifestealDamage = 0;
                    SyncAddToStatLife(delta, showCombatText: true);
                }
            }
            //////////////////////////
            if (LastManaMax == Player.statManaMax2 && LastHeathMax == Player.statLifeMax2)
            {
                ApplyFactor(Player.statLife, Player.statLifeMax2, ref LastHeath, FactorModifier.FactorType.Health);
                ApplyFactor(Player.statMana, Player.statManaMax2, ref LastMana, FactorModifier.FactorType.Mana);
                void ApplyFactor(int stat, int maxStat, ref int lastStat, FactorModifier.FactorType type)
                {
                    int delta = stat - lastStat;
                    int absDelta = Math.Abs(delta);
                    if (absDelta < 3)
                        return;
                    ////////
                    float modifier = 1f;
                    foreach (var factor in Modifiers.FactorModifiers)
                    {
                        if (factor.Type != type)
                            continue;
                        bool increase = factor.Trigger == FactorModifier.TriggerType.Increase;
                        if (increase && delta < 0)
                            continue;
                        if (!increase && delta > 0)
                            continue;
                        if (absDelta < factor.Count)
                            continue;
                        if (factor.Chance < 1f && Player.RollLuck(100) > factor.Chance * 100f)
                            continue;
                        modifier += factor.Factor;
                    }
                    if (modifier != 1f)
                    {
                        absDelta = (int)(absDelta * modifier) - absDelta;
                        if (type == FactorModifier.FactorType.Health)
                            SyncAddToStatLife(absDelta, showCombatText: true);
                        else if (type == FactorModifier.FactorType.Mana)
                            SyncAddToStatMana(absDelta, showCombatText: true);
                    }
                }
                ///////////////////////////
                if (Modifiers.ExplosionModifier.IsReady && CurrentExplosionCooldown <= 0)
                {
                    TryCreateExplostion(ExplosionModifiers.ExplosionTrigget.HealFactor, Player.statLife, LastHeath);
                    TryCreateExplostion(ExplosionModifiers.ExplosionTrigget.ManaFactor, Player.statMana, LastMana);
                    void TryCreateExplostion(ExplosionModifiers.ExplosionTrigget type, int stat, int lastStat)
                    {
                        if (Modifiers.ExplosionModifier.Trigget != type)
                            return;
                        int diff = stat - lastStat;
                        if (diff < 10)
                            return;
                        SyncCreateExplosion(diff, Player.Center);
                    }
                }
            }
        }
        ////////////////////////////
        Reset();
    }
    public override void UpdateLifeRegen()
    {
        Player.lifeRegen += CurrentStatsModififer.LifeRegen * 2;
    }
    public override void PostUpdateRunSpeeds()
    {
        float ms = 1f + CurrentStatsModififer.MoveSpeed;
        Player.maxRunSpeed *= ms;
        Player.runAcceleration *= ms;
    }

    public override void ModifyWeaponDamage(Item item, ref StatModifier damage)
    {
        if (!CountsAsClass(item.DamageType))
            return;
        foreach (var modifier in Modifiers.StatsModifiersByCondtion)
            if (modifier.ModifyWeaponStatsCondition(this, item))
            {
                damage.Base += CurrentStatsModififer.BaseDamage;
                damage += CurrentStatsModififer.Damage;
            }
    }
    public override float UseSpeedMultiplier(Item item)
    {
        if (!CountsAsClass(item.DamageType))
            return 1f;
        float mul = 1f;
        foreach (var modifier in Modifiers.StatsModifiersByCondtion)
            if (modifier.ModifyWeaponStatsCondition(this, item))
                mul += modifier.AttackSpeed;
        return mul;
    }
    public override void ModifyManaCost(Item item, ref float reduce, ref float mult)
    {
        mult += CurrentStatsModififer.ManaCost;
    }
    public override void OnMissingMana(Item item, int neededMana)
    {
        if (Modifiers.HealthCostWhenManaIsLow && Player.statLife > neededMana)
        {
            Player.statLife -= neededMana;
            Player.statMana += neededMana;
            if (TS.IsNotMC())
                SyncShowCombatText(MPS.StatType.Health, -neededMana);
        }
    }
    /// <summary>
    /// <inheritdoc/>
    /// <para>
    ///     Testing is required.
    ///     There is no precise understanding of how ammunition consumption synchronization works in tModLoader.
    /// </para>
    /// </summary>
    public override bool CanConsumeAmmo(Item weapon, Item ammo)
    {
        if (Modifiers.ChanceNotConsumeAmmo <= 0f)
            return true;
        if (Modifiers.ChanceNotConsumeAmmo >= 100f)
            return false;
        return Player.RollLuck(100) < Modifiers.ChanceNotConsumeAmmo;
    }
    public override void ModifyLuck(ref float luck)
    {
        luck += CurrentStatsModififer.Luck;
    }
    public override void ModifyHitNPC(NPC target, ref NPC.HitModifiers modifiers)
    {
        if (CountsAsClass(modifiers.DamageType))
            modifiers.CritDamage *= (1f + CurrentStatsModififer.CritDamage);
    }
    
    public override void PostHurt(Player.HurtInfo info)
    {
        SyncPostHurt();
        if (TS.IsMC())
        {
            MPC.Send_PostHurt(Player.whoAmI);
        }
    }

    /// <summary>
    ///     Called on all network sides.
    /// </summary>
    public void SyncPostHurt()
    {
        UpdateOutOfCombat(clearTime: true, clear: false);
        Player.immuneTime += CurrentStatsModififer.ImmuneTime;
    }

    public override void OnHitNPC(NPC target, NPC.HitInfo hit, int damageDone)
    {
        bool currentDmgClass = CountsAsClass(hit.DamageType);
        if (Player == Main.LocalPlayer)
        {
            SyncOnHitNPC_AllSides(target, currentDmgClass);
        }
        if(TS.IsSingle())
        {
            SyncOnHitNPC_SingleServerSides(target, currentDmgClass, hit.Crit, damageDone);
        }
        else
        {
            MPC.Send_SyncOnHitNPC(Player.whoAmI, target.whoAmI, currentDmgClass, hit.Crit, damageDone);
        }
    }

    /// <summary>
    ///     Called on all network sides.
    /// </summary>
    public void SyncOnHitNPC_AllSides(NPC target, bool currentDmgClass)
    {
        UpdateOutOfCombat(clearTime: true, clear: false);
        ///////////////////////////
        if (currentDmgClass)
        {
            var stackModif = Modifiers.StackAttackModifierHelper;
            if (stackModif.IsReady && ++CurrentStackAttack > stackModif.MaxStack)
                CurrentStackAttack = 0;
            ///////////////////////////
            if (target.active)
            {
                if (Modifiers.DebuffsOnDamage.Count > 0)
                    foreach (var debuff in Modifiers.DebuffsOnDamage)
                        target.AddBuff(debuff, TalentsModifers.DebuffsOnDamageTime);
            }
        }
    }

    /// <summary>
    ///     Called only in single-player mode or on the server side.
    /// </summary>
    public void SyncOnHitNPC_SingleServerSides(NPC target, bool currentDmgClass, bool crit, int damageDone)
    {
        ///////////////////////////
        if (currentDmgClass)
        {
            if (crit &&
            CurrentExplosionCooldown <= 0 &&
            Modifiers.ExplosionModifier.IsReady &&
            Modifiers.ExplosionModifier.Trigget == ExplosionModifiers.ExplosionTrigget.CritFactor)
            {
                SyncCreateExplosion(damageDone, target.Center);
            }
            ///////////////////////////
            if (CurrentStatsModififer.Lifesteal > 0.00001f)
            {
                CurrentAccumulatedLifestealDamage += damageDone;
                ModDebug.Write($"SyncOnHitNPC_SingleServerSides -> CurrentAccumulatedLifestealDamage = {CurrentAccumulatedLifestealDamage}; CurrentStatsModififer.Lifesteal = {CurrentStatsModififer.Lifesteal}", [DMT.MPSC]);
            }
        }
        ///////////////////////////
        if (!target.active)
        {
            if (!target.SpawnedFromStatue &&
            Modifiers.ChanceDropGoldOnKill > 0 &&
            Player.RollLuck(100) < Modifiers.ChanceDropGoldOnKill)
            {
                int itemIndex = Item.NewItem(
                    target.GetSource_Death(),
                    target.getRect(),
                    ItemID.GoldCoin,
                    1);
                ///////////////////////////
                if (TS.IsServer() && itemIndex >= 0)
                    NetMessage.SendData(MessageID.SyncItem, -1, -1, null, itemIndex, 1f);
            }
        }
    }

    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        // Call the SyncProcessTrigger method in single-player mode or on the server
        // The server calls the SyncProcessTrigger method for remote clients internally within the method itself
        void SyncJustPressed(PTT type, Action<ModPacket> writerArgs = null)
        {
            if (TS.IsSingle())
                SyncProсessTrigger(type, null);
            else
                MPC.Send_ProcessTriggers(type, writerArgs);
        }
        ///////////////////////////
        if (KeybindSystem.ChargeKeybind.JustPressed)
        {
            if (Player.controlRight)
                _lastDirectionRight = true;
            else if (Player.controlLeft)
                _lastDirectionRight = false;
            else
                _lastDirectionRight = Player.direction > 0;
            /////////////////
            SyncJustPressed(PTT.ChargeKeybind, writerArgs: w => w.Write(_lastDirectionRight));
        }
        ///////////////////////////
        if (KeybindSystem.RodDiscordKeybind.JustReleased && Modifiers.RodDiscord && !Player.chaosState)
        {
            Vector2 teleportPoisition = default(Vector2);
            teleportPoisition.X = Main.mouseX + Main.screenPosition.X;
            if (Player.gravDir == 1f)
                teleportPoisition.Y = Main.mouseY + Main.screenPosition.Y - Player.height;
            else
                teleportPoisition.Y = Main.screenPosition.Y + Main.screenHeight - Main.mouseY;
            teleportPoisition.X -= Player.width / 2;
            /////////////////////////////
            Player.LimitPointToPlayerReachableArea(ref teleportPoisition);
            if (!(teleportPoisition.X > 50f) || 
                !(teleportPoisition.X < Main.maxTilesX * 16f - 50f) || 
                !(teleportPoisition.Y > 50f) || 
                !(teleportPoisition.Y < Main.maxTilesY * 16f - 50f))
                    return;
            ////////////////////////////
            int pI = (int)(teleportPoisition.X / 16f);
            int pJ = (int)(teleportPoisition.Y / 16f);
            if ((Main.tile[pI, pJ].WallType == WallID.LihzahrdBrickUnsafe && 
                !NPC.downedPlantBoss && 
                (Main.remixWorld || pJ > Main.worldSurface)) || 
                Collision.SolidCollision(teleportPoisition, Player.width, Player.height))
                    return;
            //////////////////////////////
            Player.Teleport(teleportPoisition, TeleportationStyleID.RodOfDiscord);
            NetMessage.SendData(MessageID.TeleportEntity, -1, -1, null, 0, Player.whoAmI, teleportPoisition.X, teleportPoisition.Y, 1);
            Player.AddBuff(BuffID.ChaosState, 360, quiet: true);
        }
        ///////////////////////////
        if (KeybindSystem.SwithAmmoKeybind.JustPressed)
        {
            SyncJustPressed(PTT.SwithAmmoKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.SwithSandStormKeybind.JustPressed)
        {
            SyncJustPressed(PTT.SwithSandStormKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.ExploreMapKeybind.JustPressed && Modifiers.ExploreMap)
        {
            for (int x = 0; x < Main.maxTilesX; x++)
                for (int y = 0; y < Main.maxTilesY; y++)
                    if(WorldGen.InWorld(x, y, 0))
						if(Main.netMode == NetmodeID.SinglePlayer || Main.sectionManager.TileLoaded(x, y))
							Main.Map.Update(x, y, 255);
            Main.refreshMap = true;
        }
        ///////////////////////////
        if (KeybindSystem.SwithWithoutCollisionProjectileKeybind.JustPressed)
        {
            SyncJustPressed(PTT.SwithWithoutCollisionProjectileKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.SwithProjectileAIMKeybind.JustPressed)
        {
            SyncJustPressed(PTT.SwithProjectileAIMKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.ReincarnationKeybind.JustPressed)
        {
            SyncJustPressed(PTT.ReincarnationKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.SpawnRateKeybind.JustPressed)
        {
            SyncJustPressed(PTT.SpawnRateKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.FullManaRestorationKeybind.JustPressed)
        {
            SyncJustPressed(PTT.FullManaRestorationKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.ManaHealingKeybind.JustPressed)
        {
            SyncJustPressed(PTT.ManaHealingKeybind);
        }
        ///////////////////////////
        if (KeybindSystem.HealthRegenManaKeybind.JustPressed)
        {
            SyncJustPressed(PTT.HealthRegenManaKeybind);
        }
    }

    protected bool SyncActivateTalentChecker(PTT type)
        => type switch
        {
            PTT.ChargeKeybind => Modifiers.DashModifier.IsReady && CurrentDashCooldown <= 0,
            PTT.SwithAmmoKeybind => Modifiers.SwitchAmmoModifier.IsReady,
            PTT.SwithSandStormKeybind => Modifiers.SandStorm,
            PTT.SwithWithoutCollisionProjectileKeybind => Modifiers.WithoutCollisionProjectile,
            PTT.SwithProjectileAIMKeybind => Modifiers.AIMHomingTurnRate > 0f,
            PTT.ReincarnationKeybind => Modifiers.Reincarnation && !Player.HasBuff(BuffID.PotionSickness) && Player.potionDelay <= 0,
            PTT.SpawnRateKeybind => Modifiers.SpawnRate != 0f,
            PTT.FullManaRestorationKeybind => Modifiers.FullManaRestoration && !Player.HasBuff(BuffID.ManaSickness),
            PTT.ManaHealingKeybind => Modifiers.ManaHealignModifier.IsReady && !Player.HasBuff(ModContent.BuffType<BuffManaHealingCooldown>()),
            PTT.HealthRegenManaKeybind => Modifiers.HealthRegenMana,
            _ => false
        };

    /// <summary>
    ///     Always called in single-player mode and on the server side.
    ///     In multiplayer mode, the server independently decides whether to call this method for remote clients.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="reader"></param>
    public void SyncProсessTrigger(PTT type, BinaryReader reader)
    {
        // Remote clients must not perform readiness checks themselves
        bool isReadiness = TS.IsMC() || SyncActivateTalentChecker(type);
        ModDebug.Write($"SyncProgressTrigger: -> isReadiness = {isReadiness}; type = {type}; IsServer = {TS.IsServer()}", [DMT.MPSC]);

        // The actions will be performed only if isReadiness == true.
        Action safeAction = null;

        // Simplified data transmission from the server to remote clients
        // (re-invoking this method on remote clients with different BinaryReader data).
        void Send(Action<ModPacket> writer)
        {
            if (TS.IsSingle())
                return;
            MPS.Send_SyncProgressTrigger(Player.whoAmI, type, writer);
        }

        switch (type)
        {
            case PTT.ChargeKeybind:
                bool directionRight = false;
                float velocityX = 0;
                if (TS.IsSingle())
                {
                    // A single client knows the direction of the jerk by the time this method is called.
                    directionRight = _lastDirectionRight;
                }
                else if (TS.IsServer())
                {
                    // The client sent the dash direction to the server.
                    directionRight = reader.ReadBoolean();
                }
                else
                {
                    // The server sends the new speed and calculated direction to the remote client.
                    velocityX = reader.ReadSingle();
                }

                safeAction = () =>
                {
                    var modif = Modifiers.DashModifier;
                    if (modif.ImmuneTime > 0)
                    {
                        Player.immune = true;
                        Player.immuneTime += modif.ImmuneTime;
                    }
                    //////////
                    if (TS.IsNotMC())
                    {
                        CurrentDashCooldown = modif.Cooldown;
                        Player.velocity.X += modif.Speed * (directionRight ? 1f : -1f);
                        Send(w => w.Write(Player.velocity.X));
                    }
                    else
                    {
                        Player.velocity.X = velocityX;
                    }
                };
                ModDebug.Write($"ChargeKeybind -> directionRight = {directionRight}; Player.velocity.X = {Player.velocity.X}", [DMT.MPSC]);
                break;
            case PTT.SwithAmmoKeybind:
                byte currentArrow = 0;
                byte currentBullet = 0;
                if (TS.IsMC())
                {
                    currentArrow = reader.ReadByte();
                    currentBullet = reader.ReadByte();
                }
                var modif = Modifiers.SwitchAmmoModifier;
                safeAction = () =>
                {
                    if (TS.IsNotMC())
                    {
                        modif.NextArrow();
                        modif.NextBullet();
                        Send(w =>
                        {
                            w.Write(modif.CurrentArrow);
                            w.Write(modif.CurrentBullet);
                        });
                    }
                    else
                    {
                        modif.CurrentArrow = currentArrow;
                        modif.CurrentBullet = currentBullet;
                    }
                    if (!TS.IsServer())
                    {
                        SoundEngine.PlaySound(SoundID.Item39, Player.Center);
                    }
                };
                ModDebug.Write($"SwithAmmoKeybind -> CurrentArrow = {modif.CurrentArrow}; CurrentBullet = {modif.CurrentBullet}", [DMT.MPSC]);
                break;
            case PTT.SwithSandStormKeybind:
                bool currentBindSandStorm = false;
                if (TS.IsMC())
                {
                    currentBindSandStorm = reader.ReadBoolean();
                }
                safeAction = () =>
                {
                    if (TS.IsNotMC())
                    {
                        CurrentBindSandStorm = !CurrentBindSandStorm;
                        Send(w => w.Write(CurrentBindSandStorm));
                    }
                    else
                    {
                        CurrentBindSandStorm = currentBindSandStorm;
                    }
                };
                ModDebug.Write($"SwithSandStormKeybind -> CurrentStateSandStorm = {CurrentBindSandStorm}", [DMT.MPSC]);
                break;
            case PTT.SwithWithoutCollisionProjectileKeybind:
                bool currentBindWithoutCollisionProjectile = false;
                if (TS.IsMC())
                {
                    currentBindWithoutCollisionProjectile = reader.ReadBoolean();
                }
                safeAction = () =>
                {
                    if (TS.IsNotMC())
                    {
                        CurrentBindWithoutCollisionProjectile = !CurrentBindWithoutCollisionProjectile;
                        Send(w => w.Write(CurrentBindWithoutCollisionProjectile));
                    }
                    else
                    {
                        CurrentBindWithoutCollisionProjectile = currentBindWithoutCollisionProjectile;
                    }
                };
                ModDebug.Write($"SwithWithoutCollisionProjectileKeybind -> CurrentBindWithoutCollisionProjectile = {CurrentBindWithoutCollisionProjectile}", [DMT.MPSC]);
                break;
            case PTT.SwithProjectileAIMKeybind:
                bool currentBindAIMProjectile = false;
                if (TS.IsMC())
                {
                    currentBindAIMProjectile = reader.ReadBoolean();
                }
                safeAction = () =>
                {
                    if (TS.IsNotMC())
                    {
                        CurrentBindAIMProjectile = !CurrentBindAIMProjectile;
                        Send(w => w.Write(CurrentBindAIMProjectile));
                    }
                    else
                    {
                        CurrentBindAIMProjectile = currentBindAIMProjectile;
                    }
                };
                ModDebug.Write($"SwithProjectileAIMKeybind -> CurrentBindAIMProjectile = {CurrentBindAIMProjectile}", [DMT.MPSC]);
                break;
            case PTT.ReincarnationKeybind:
                if (TS.IsNotMC())
                {
                    safeAction = () => _reincarnationActivated = true;
                }
                ModDebug.Write($"ReincarnationKeybind", [DMT.MPSC]);
                break;
            case PTT.SpawnRateKeybind:
                bool currentBindEditSpawnRate = false;
                if (TS.IsMC())
                {
                    currentBindEditSpawnRate = reader.ReadBoolean();
                }
                safeAction = () =>
                {
                    if (TS.IsNotMC())
                    {
                        CurrentBindEditSpawnRate = !CurrentBindEditSpawnRate;
                        Send(w => w.Write(CurrentBindEditSpawnRate));
                    }
                    else
                    { 
                        CurrentBindEditSpawnRate = currentBindEditSpawnRate;
                    }
                };
                ModDebug.Write($"SpawnRateKeybind -> CurrentBindEditSpawnRate = {CurrentBindEditSpawnRate}", [DMT.MPSC]);
                break;
            case PTT.FullManaRestorationKeybind:
                if (TS.IsNotMC())
                {
                    safeAction = () => _fullManaRestorationActivated = true;
                }
                ModDebug.Write($"FullManaRestorationKeybind", [DMT.MPSC]);
                break;
            case PTT.ManaHealingKeybind:
                if (TS.IsNotMC())
                {
                    safeAction = () => _manaHealingActivated = true;
                }
                ModDebug.Write($"ManaHealingKeybind", [DMT.MPSC]);
                break;
            case PTT.HealthRegenManaKeybind:
                if (TS.IsNotMC())
                {
                    safeAction = () => _healthRegenManaActivated = true;
                }
                ModDebug.Write($"HealthRegenManaKeybind", [DMT.MPSC]);
                break;
        }
        ////////////////////////
        if (isReadiness && safeAction is not null)
            safeAction();
    }

    public void Send_SyncActiveTalentActivated(MPS.ActiveTalentType type) => MPS.Send_SyncActiveTalentActivated(Player.whoAmI, type);

    /// <summary>
    ///     Called in single-player or on the server side.
    /// </summary>
    public void ActivateReincarnation()
    {
        _reincarnationActivated = false;
        LastHeath = 1;
        SyncSetStat(MPS.StatType.Health, Player.statLifeMax2);
        SyncShowCombatText(MPS.StatType.Health, Player.statLifeMax2 - 1);
        SyncActivateReincarnation();
        ModDebug.Write($"ActivateReincarnation -> newStatLife = {Player.statLifeMax2}", [DMT.MPSS]);
    }
    /// <summary>
    ///     Can be called on either side
    /// </summary>
    /// <param name="newStatLife"></param>
    public void SyncActivateReincarnation()
    {
        Player.AddBuff(BuffID.PotionSickness, 60 * 60, quiet: true);
        Player.potionDelay = 60 * 60;
        if (TS.IsServer())
        {
            Send_SyncActiveTalentActivated(MPS.ActiveTalentType.Reincarnation);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.Item103, Player.Center);
        }
        ModDebug.Write($"SyncActivateReincarnation", [DMT.MPSS]);
    }

    /// <summary>
    ///     Called in single-player or on the server side.
    /// </summary>
    public void ActivateFullManaRestoration()
    {
        _fullManaRestorationActivated = false;
        if (Player.statMana == Player.statLifeMax2)
            return;
        SyncAddToStatMana(Player.statManaMax2, showCombatText: true);
        SyncActivateFullManaRestoration();
        ModDebug.Write($"SyncActivateReincarnation -> statManaMax2 = {Player.statManaMax2}", [DMT.MPSS]);
    }
    /// <summary>
    ///     Can be called on either side
    /// </summary>
    public void SyncActivateFullManaRestoration()
    {
        Player.AddBuff(BuffID.ManaSickness, 5 * 60, quiet: true);
        if (TS.IsServer())
        {
            Send_SyncActiveTalentActivated(MPS.ActiveTalentType.FullManaRestoration);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.Item100, Player.Center);
        }
        ModDebug.Write($"SyncActivateReincarnation -> statManaMax2 = {Player.statManaMax2}", [DMT.MPSS]);
    }

    /// <summary>
    ///     Called in single-player or on the server side.
    /// </summary>
    public void ActivateManaHealing()
    {
        _manaHealingActivated = false;
        if (Player.statMana == 0)
            return;

        ModDebug.Write($"PRE_SyncActivateReincarnation -> statLife = {Player.statLife}; statMana = {Player.statMana}", [DMT.MPSS]);
        SyncAddToStatLife(Player.statMana, showCombatText: false);
        SyncAddToStatMana(-Player.statMana, showCombatText: true);
        SyncActivateManaHealing();
        ModDebug.Write($"POST_SyncActivateReincarnation -> statLife = {Player.statLife}; statMana = {Player.statMana}", [DMT.MPSS]);
    }

    /// <summary>
    ///     Can be called on either side
    /// </summary>
    public void SyncActivateManaHealing()
    {
        Player.AddBuff(ModContent.BuffType<BuffManaHealingCooldown>(), Modifiers.ManaHealignModifier.DebuffDurationInSec * 60, quiet: true);
        if (TS.IsServer())
        {
            Send_SyncActiveTalentActivated(MPS.ActiveTalentType.ManaHealing);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.Item103, Player.Center);
        }
        ModDebug.Write($"SyncActivateManaHealing", [DMT.MPSS]);
    }

    /// <summary>
    ///     Called in single-player or on the server side.
    /// </summary>
    public void ActivateHealthRegenMana()
    {
        _healthRegenManaActivated = false;
        if (Player.statLife < 200)
            return;
        ModDebug.Write($"PRE_ActivateHealthRegenMana -> statLife = {Player.statLife}; statMana = {Player.statMana}; statManaMax2 = {Player.statManaMax2}", [DMT.MPSS]);
        SyncAddToStatLife(-(Player.statLife - 1), showCombatText: true);
        SyncAddToStatMana(Player.statManaMax2, showCombatText: false);
        SyncActivateHealthRegenMana();
        ModDebug.Write($"POST_ActivateHealthRegenMana -> statLife = {Player.statLife}; statMana = {Player.statMana};", [DMT.MPSS]);
    }

    /// <summary>
    ///     Can be called on either side
    /// </summary>
    public void SyncActivateHealthRegenMana()
    {
        if (TS.IsServer())
        {
            Send_SyncActiveTalentActivated(MPS.ActiveTalentType.HealthRegenMana);
        }
        else
        {
            SoundEngine.PlaySound(SoundID.Item103, Player.Center);
        }
        ModDebug.Write($"SyncActivateHealthRegenMana", [DMT.MPSS]);
    }

    /// <summary>
    ///     This method should only be called in single-player mode or on the server side.
    /// </summary>
    public void SyncCreateExplosion(int baseDamage, Vector2 position)
    {
		CurrentExplosionCooldown = Modifiers.ExplosionModifier.Cooldown;
        ///////////////////////////
        float knockback = Player.GetTotalKnockback(CurrentDamageClass).ApplyTo(12f);
        float critChance = Player.GetTotalCritChance(CurrentDamageClass);
        int damage = (int)Math.Round(baseDamage * Modifiers.ExplosionModifier.Damage);
        damage = (int)Player.GetTotalDamage(CurrentDamageClass).ApplyTo(damage);
        float radius = Modifiers.ExplosionModifier.Radius;
        float radiusSquared = radius * radius;
        ///////////////////////////
        foreach (NPC npc in Main.ActiveNPCs)
        {
            if (!npc.CanBeChasedBy(Player))
                continue;
            if (Vector2.DistanceSquared(position, npc.Center) > radiusSquared)
                continue;
            int direction = npc.Center.X >= position.X ? 1 : -1;
            bool crit = Player.RollLuck(100) < critChance;
            Player.ApplyDamageToNPC(
                npc,
                damage,
                knockback,
                direction,
                crit,
                CurrentDamageClass,
                damageVariation: true);
        }
        ///////////////////////////
        if (TS.IsSingle())
            SyncCreateExplosionVisual(position);
        else
            MPS.Send_SyncCreateExplosionVisual(Player.whoAmI, position);
    }

    /// <summary>
    ///     This method is called automatically within method <see cref="SyncCreateExplosion(int, Vector2)"/> in single-player mode or by the server side.
    /// </summary>
    /// <param name="position"></param>
    public void SyncCreateExplosionVisual(Vector2 position)
    {
        const float DustDensity = 120f;
        var modif = Modifiers.ExplosionModifier;
        Color color = modif.Color;
        float radius = modif.Radius;
        /////////////////////
        for (int i = 0; i < 20; i++)
        {
            Vector2 velocity = Main.rand.NextVector2Circular(3f, 3f);

            int dust = Dust.NewDustPerfect(
                position,
                DustID.Torch,
                velocity,
                0,
                Modifiers.ExplosionModifier.Color,
                Main.rand.NextFloat(1.5f, 2.5f)
            ).dustIndex;

            Main.dust[dust].noGravity = true;
        }
        ///////////////////////
        int cloudDustCount = (int)(MathF.PI * radius * radius / DustDensity);
        for (int i = 0; i < cloudDustCount; i++)
        {
            Vector2 offset = Main.rand.NextVector2Circular(radius, radius);
            if (offset.LengthSquared() > radius * radius)
            {
                i--;
                continue;
            }
            Vector2 velocity = Main.rand.NextVector2Circular(2f, 2f);
            int dust = Dust.NewDustPerfect(
                position + offset,
                DustID.Smoke,
                velocity,
                100,
                color,
                Main.rand.NextFloat(1f, 1.8f)
            ).dustIndex;
            Main.dust[dust].noGravity = true;
        }
        /////////////////////
        SoundEngine.PlaySound(modif.Sound, position);
    }

    /// <inheritdoc cref="SyncAddToStat(MPS.StatType, int, bool)"/>
    public void SyncAddToStatLife(int value, bool showCombatText) 
        => SyncAddToStat(MPS.StatType.Health, value, showCombatText);

    /// <inheritdoc cref="SyncAddToStat(MPS.StatType, int, bool)"/>
    public void SyncAddToStatMana(int value, bool showCombatText) 
        => SyncAddToStat(MPS.StatType.Mana, value, showCombatText);

    /// <summary>
    ///     This method should only be called in single-player mode or on the server side.
    ///     You are responsible for ensuring that the maximum characteristic has been calculated by the time this method is called.
    /// </summary>
    public void SyncAddToStat(
        MPS.StatType type, 
        int value, 
        bool showCombatText)
    {
        if (Player.dead)
            return;
        int stat;
        int statMax;
        switch (type)
        {
            case MPS.StatType.Health:
                stat = Player.statLife;
                statMax = Player.statLifeMax2;
                break;
            case MPS.StatType.Mana:
                stat = Player.statMana;
                statMax = Player.statManaMax2;
                break;
            default:
                return;
        }

        int lastStat = stat;
        int delta = stat;
        stat += value;
        stat = Math.Clamp(stat, 0, statMax);
        if (lastStat == stat)
            return;

        SyncSetStat(type, stat);
        if (showCombatText)
            SyncShowCombatText(type, stat - delta);
    }
    /// <summary>
    ///     This method should only be called in single-player mode or on the server side.
    /// </summary>
    public void SyncShowCombatText(MPS.StatType type, int value)
    {
        if (TS.IsServer())
        {
            MPS.Send_SyncShowCombatText(Player.whoAmI, type, value);
            return;
        }

        Color? color = type switch
        {
            MPS.StatType.Health => value > 0 ? CombatText.HealLife : Color.DarkRed,
            MPS.StatType.Mana => value > 0 ? CombatText.HealMana : new(27, 1, 56),
            _ => null
        };
        if (color is null)
            return;

        Rectangle cmtRect = new((int)Player.position.X, (int)Player.position.Y, Player.width, Player.height);
        CombatText.NewText(cmtRect, (Color)color, value);
    }

    /// <summary>
    ///     This method should only be called in single-player mode or on the server side.
    /// </summary>
    public void SyncSetStat(MPS.StatType type, int value)
    {
        switch (type)
        {
            case MPS.StatType.Health:
                Player.statLife = value;
                break;
            case MPS.StatType.Mana:
                Player.statMana = value;
                break;
        }
        if (TS.IsServer())
            MPS.Send_SyncSetStat(Player.whoAmI, type, value);
    }

    public void SyncClearBuff(int buffType)
    {
        Player.ClearBuff(buffType);
        if (TS.IsServer())
        {
            MPS.Send_SyncClearBuff(Player.whoAmI, buffType);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="clearTime"></param>
    /// <param name="clear"></param>
    public void UpdateOutOfCombat(bool clearTime, bool clear)
    {
        if (CurrentDamageClass != DamageClass.Melee)
            clear = true;

        int buffType = ModContent.BuffType<BuffOutOfCombat>();
        int index = Player.FindBuffIndex(buffType);
        if (index != -1)
        {
            if (clear)
                Player.ClearBuff(buffType);
            else if (clearTime)
            {
                Player.buffTime[index] = 4;
            }
        }
        else if (!clear)
            Player.AddBuff(buffType, 4, quiet: true);
    }

    public bool CountsAsClass(DamageClass type)
        => CurrentDamageClass.CountsAsClass(type) || type.CountsAsClass(CurrentDamageClass);
}

public class TalentApplyGlobalProjectile : GlobalProjectile
{
    public int TargetType = -2;
    public float HomingTurnRate;
    public bool WithoutCollisionProjectile;
    public override bool InstancePerEntity => true;
    public override void AI(Projectile projectile)
    {
        if (TargetType == -2)
            return;
        else if (TargetType == -1)
        {
            FintClossestTarget(projectile);
            if (TargetType == -1)
                return;
        }
        NPC target = Main.npc[TargetType];
        if (!target.CanBeChasedBy())
        {
            TargetType = -2;
            return;
        }
        float maxRadiusPixels = TalentsModifers.AIMTargetingRadiusInTiles * 16f;
        float radiusSquared = maxRadiusPixels * maxRadiusPixels;
        float distanceSquired = Vector2.DistanceSquared(projectile.Center, target.Center);
        if (!CheckDistanceAndCollision(projectile, target, distanceSquired, radiusSquared))
        {
            TargetType = -1;
            return;
        }
        ///////////////////////
        float speed = projectile.velocity.Length();
        if (speed <= 0.001f)
            return;
        ////////////////////////
        Vector2 currentDirection = projectile.velocity / speed;
        Vector2 targetDirection = (target.Center - projectile.Center).SafeNormalize(currentDirection);
        /////////////////////////
        float angleDifference = MathHelper.WrapAngle(
            targetDirection.ToRotation() - currentDirection.ToRotation());
        float maxTurnAngle = MathHelper.ToRadians(HomingTurnRate);
        float turnAngle = MathHelper.Clamp(angleDifference, -maxTurnAngle, maxTurnAngle);
        /////////////////////////
        projectile.velocity = currentDirection.RotatedBy(turnAngle) * speed;
        projectile.rotation = projectile.velocity.ToRotation() + MathHelper.PiOver2;
    }
    public override void OnHitNPC(Projectile projectile, NPC target, NPC.HitInfo hit, int damageDone)
    {
        TargetType = -2;
    }
    public override bool TileCollideStyle(
        Projectile projectile,
        ref int width,
        ref int height,
        ref bool fallThrough,
        ref Vector2 hitboxCenterFrac)
    {
        if (WithoutCollisionProjectile)
        {
            fallThrough = false;
            return false;
        }
        return true;
    }
    public override void OnSpawn(Projectile projectile, IEntitySource source)
    {
        if (source is IEntitySource_WithStatsFromItem itemSource)
        {
            Item item = itemSource.Item;
            if (item.stack == 1)
            {
                var player = itemSource.Player;
                var modPlayer = player.GetModPlayer<TalentsApplyModPlayer>();
                if (modPlayer.CountsAsClass(projectile.DamageType))
                {
                    SetParams(player);
                    if (TS.IsServer())
                    {
                        MPS.Send_SetParamsGlobalProjectile(player.whoAmI, projectile.whoAmI);
                    }
                }
            }
        }
    }

    public void SetParams(Player player)
    {
        var modPlayer = player.GetModPlayer<TalentsApplyModPlayer>();
        var modifiers = modPlayer.Modifiers;
        HomingTurnRate = modifiers.AIMHomingTurnRate;
        WithoutCollisionProjectile = modPlayer.CurrentBindWithoutCollisionProjectile && modifiers.WithoutCollisionProjectile;
        TargetType = modPlayer.CurrentBindAIMProjectile && HomingTurnRate > 0 ? -1 : -2;
    }

    protected bool CheckDistanceAndCollision(Projectile projectile, NPC target, float distanceSquared, float distanceSquaredMax)
    {
        if (distanceSquared >= distanceSquaredMax)
            return false;
        if (!WithoutCollisionProjectile && !Collision.CanHit(
            projectile.Center, projectile.width / 2, projectile.height / 2,
            target.Center, target.width / 2, target.height / 2))
                return false;
        return true;
    }
    protected void FintClossestTarget(Projectile projectile)
    {
        float maxRadiusPixels = TalentsModifers.AIMTargetingRadiusInTiles * 16f;
        float closestDistanceSquared = maxRadiusPixels * maxRadiusPixels;
        foreach (NPC target in Main.npc)
        {
            float distanceSquared = Vector2.DistanceSquared(projectile.Center, target.Center);
            if (!target.CanBeChasedBy())
                continue;
            if (!CheckDistanceAndCollision(projectile, target, distanceSquared, closestDistanceSquared))
                continue;
            closestDistanceSquared = distanceSquared;
            TargetType = target.whoAmI;
        }
    }
}

public class TalentsApplyGlobalItem : GlobalItem
{
    public override void GrabRange(Item item, Player player, ref int grabRange)
    {
        var modif = player.GetModPlayer<TalentsApplyModPlayer>().Modifiers;
        if (modif is null)
            return;
        if(modif.ItemsGrapRange != 1f)
            grabRange += (int)(Player.defaultItemGrabRange * modif.ItemsGrapRange);
        if (modif.ItemsGrapRangeTable.ContainsKey(item.type) && modif.ItemsGrapRangeTable[item.type] != 1f)
            grabRange += (int)(Player.defaultItemGrabRange * modif.ItemsGrapRangeTable[item.type]);
    }
    public override void PickAmmo(Item ammo, Item weapon, Player player, ref int type, ref float speed, ref StatModifier damage, ref float knockback)
    {
        if (player == Main.LocalPlayer)
        {
            var modifiers = player.GetModPlayer<TalentsApplyModPlayer>().Modifiers;
            if (modifiers is not null && modifiers.SwitchAmmoModifier.IsReady)
            {
                if (type == ProjectileID.WoodenArrowFriendly)
                {
                    var newType = modifiers.SwitchAmmoModifier.GetArrowProjectileType();
                    if (newType != -1)
                        type = newType;
                }
                if (type == ProjectileID.Bullet)
                {
                    var newType = modifiers.SwitchAmmoModifier.GetBulletProjectileType();
                    if (newType != -1)
                        type = newType;
                }
            }
        }
    }
    public override bool ConsumeItem(Item item, Player player)
    {
        if (item.ammo != AmmoID.None)
        {
            var modifier = player.GetModPlayer<TalentsApplyModPlayer>().Modifiers;
            if (modifier.ChanceNotConsumeAmmo <= 0f)
                return true;
            if (modifier.ChanceNotConsumeAmmo >= 100f)
                return false;
            return player.RollLuck(100) <= modifier.ChanceNotConsumeAmmo;
        }
        return true;
    }
}

public class TalentsApplyGlobalNPC : GlobalNPC
{
    public override void EditSpawnRate(Player player, ref int spawnRate, ref int maxSpawns)
    {
        var modPlayer = player.GetModPlayer<TalentsApplyModPlayer>();
        if (modPlayer is null || 
            !modPlayer.CurrentBindEditSpawnRate ||
            modPlayer.Modifiers.SpawnRate == 0f)
            return;
        spawnRate = (int)(spawnRate / modPlayer.Modifiers.SpawnRate);
        maxSpawns = (int)(maxSpawns * modPlayer.Modifiers.SpawnRate);
        if (spawnRate == 0)
            spawnRate = 1;
    }
}