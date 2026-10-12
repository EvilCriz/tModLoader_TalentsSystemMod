using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Terraria;
using Terraria.GameInput;
using Terraria.ModLoader;

namespace TalentsSystem;

public class KeybindSystem : ModSystem
{
    /// <summary>
    /// Main menu <see cref="Terraria.UI.UIState"/>. Stored for subscribing to and unsubscribing from menu events.
    /// </summary>
    public static UIMenuState MainMenuState;
    public static ModKeybind MainMenuKeybind;
    /////////////
    //////// Genaral:
    public static ModKeybind ChargeKeybind;
    public static ModKeybind RodDiscordKeybind;
    public static ModKeybind SwithWithoutCollisionProjectileKeybind;
    public static ModKeybind SwithProjectileAIMKeybind;

    // Warrior:
    public static ModKeybind ReincarnationKeybind;
    public static ModKeybind SpawnRateKeybind;

    // Ranger:
    public static ModKeybind SwithAmmoKeybind;

    // Traveler:
    public static ModKeybind SwithSandStormKeybind;
    public static ModKeybind ExploreMapKeybind;
    
    // Mage:
    public static ModKeybind FullManaRestorationKeybind;

    // Warlock
    public static ModKeybind ManaHealingKeybind;
    public static ModKeybind HealthRegenManaKeybind;

    public override void Load()
    {
        MainMenuKeybind = KeybindLoader.RegisterKeybind(Mod, "MainMenu", "X");
        ////////////////////////////
        ChargeKeybind = KeybindLoader.RegisterKeybind(Mod, "Charge", "Q");
        RodDiscordKeybind = KeybindLoader.RegisterKeybind(Mod, "RodDiscord", "F");
        SwithWithoutCollisionProjectileKeybind = KeybindLoader.RegisterKeybind(Mod, "SwithWithoutCollisionProjectile", "Y");
        SwithProjectileAIMKeybind = KeybindLoader.RegisterKeybind(Mod, "SwithProjectileAIM", "T");
        ///////////////////////////
        SwithAmmoKeybind = KeybindLoader.RegisterKeybind(Mod, "SwithAmmo", "Z");
        ///////////////////////////
        SwithSandStormKeybind = KeybindLoader.RegisterKeybind(Mod, "SwithSandStorm", "C");
        ExploreMapKeybind = KeybindLoader.RegisterKeybind(Mod, "ExploreMap", "Z");
        ///////////////////////////
        ReincarnationKeybind = KeybindLoader.RegisterKeybind(Mod, "Reincarnation", "C");
        SpawnRateKeybind = KeybindLoader.RegisterKeybind(Mod, "SpawnRate", "Z");
        ///////////////////////////
        FullManaRestorationKeybind = KeybindLoader.RegisterKeybind(Mod, "FullManaRestoration", "C");
        ///////////////////////////
        ManaHealingKeybind = KeybindLoader.RegisterKeybind(Mod, "ManaHealing", "C");
        HealthRegenManaKeybind = KeybindLoader.RegisterKeybind(Mod, "HealthRegenMana", "Z");
    }
    public override void Unload()
    {
        if (MainMenuState is not null)
        {
            MainMenuState.BranchActivated -= EventsManager.OnBranchActivated;
            MainMenuState.BranchReseted -= EventsManager.OnBranchReset;
            MainMenuState.TalentLevelIncreased -= EventsManager.OnTalentLevelIncreased;
            MainMenuState = null;
        }
        MainMenuKeybind = null;
        /////////////////
        ChargeKeybind = null;
        RodDiscordKeybind = null;
        SwithAmmoKeybind = null;
        SwithSandStormKeybind = null;
        ExploreMapKeybind = null;
        SwithWithoutCollisionProjectileKeybind = null;
        SwithProjectileAIMKeybind = null;
        ReincarnationKeybind = null;
        SpawnRateKeybind = null;
        FullManaRestorationKeybind = null;
        ManaHealingKeybind = null;
        HealthRegenManaKeybind = null;
    }

    /// <summary>
    ///     This method is used to handle key presses associated with opening or closing the main menu, 
    ///     allowing the player to interact with the menu during an automatic pause.
    /// </summary>
    /// <param name="gameTime"></param>
    public override void UpdateUI(GameTime gameTime)
    {
        if (MainMenuKeybind.JustReleased)
        {
            var system = ModContent.GetInstance<UIMenuSystem>();
            if (MainMenuState is null)
            {
                system.Show();
                MainMenuState = system.GetUIState();
                MainMenuState.BranchReseted += EventsManager.OnBranchReset;
                MainMenuState.BranchActivated += EventsManager.OnBranchActivated;
                MainMenuState.TalentLevelIncreased += EventsManager.OnTalentLevelIncreased;
            }
            else
                system.SwitchState();
        }

        /// Pressing Esc should close the main menu and pop-up notifications, provided the mod's configuration settings allow it.
        if (Main.keyState.IsKeyDown(Keys.Escape) &&
            Main.oldKeyState.IsKeyUp(Keys.Escape))
        {
            var config = ModContent.GetInstance<TalentSystemConfig>();
            if (config.CloseMenuByEsc)
            {
                var systemMainMenu = ModContent.GetInstance<UIMenuSystem>();
                var systemNotif = ModContent.GetInstance<UINotificationSystem>();
                systemMainMenu.Hide();
                systemNotif.ClearNotifications();
            }
        }
    }
}

public class KeybindSystemModPlayer : ModPlayer
{
    /// <summary>
    ///     Display a message upon entering the world stating that the key to open the main menu is not assigned.
    /// </summary>
    public override void OnEnterWorld()
    {
        bool hasKey = KeybindSystem.MainMenuKeybind.GetAssignedKeys().Count > 0;
        if (hasKey)
            return;
        ////////////////////
        string message = $"[{Mod.DisplayName}]: ";
        message += Utils.UtilsLocalization.Common_KeybindWarning.Value;
        Main.NewText(message, Color.DarkOrange);
    }

    // DEBUG:
    public override void ProcessTriggers(TriggersSet triggersSet)
    {
        //if (Main.keyState.IsKeyDown(Keys.Y) &&
        //    Main.oldKeyState.IsKeyUp(Keys.Y))
        //{
        //    TalentsManager.Unload();
        //    TalentsManager.Load();
        //    Player.GetModPlayer<TalentsApplyModPlayer>()?.RefreshModifiers();
        //    ModContent.GetInstance<UIMenuSystem>()?.GetUIState()?.OnActivate();
        //}
        //if (Main.keyState.IsKeyDown(Keys.Y) &&
        //    Main.oldKeyState.IsKeyUp(Keys.Y))
        //{
        //    var system = ModContent.GetInstance<Other.ModIconSpriteCreaterSystem>();
        //    if (system is not null)
        //        system.SwitchState();
        //}
        //if (Main.keyState.IsKeyDown(Keys.G) &&
        //    Main.oldKeyState.IsKeyUp(Keys.G))
        //{
        //    return;
        //    var save = TS.GetLocal<QuestSaveInfo>();
        //    var quest = QuestsManager.Instance;
        //    if (save is null || quest is null)
        //        return;
        //    ////////////////////
        //    save.Level = 0;
        //    save.Exp = 0;
        //    save.CompletedQuests.Clear();
        //    save.IncompletedQuests.Clear();
        //    foreach (var id in quest.AllQuestsArray)
        //        save.IncompletedQuests.Add(id);
        //    save.UpdateSortedActualQuestTable(quest);
        //    //Complete([QuestID.Workbench, QuestID.HellForge, QuestID.ObjDestroy_25_000]);
        //    //Complete([QuestID.Wood, QuestID.Workbench, QuestID.ObjDestroy_1000]);
        //    ///Complete([QuestID.Wood]);
        //    void Complete(int[] quests)
        //    {
        //        foreach (var q in quests)
        //            save.CompleteQuest(q);
        //    }

        //    foreach (var fiel in typeof(QuestID).GetFields())
        //    {
        //        var id = (int)fiel.GetRawConstantValue()!;
        //        save.CompleteQuest(id);
        //    }
        //}
    }
}