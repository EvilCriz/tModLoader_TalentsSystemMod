using Terraria.Localization;
using LT = Terraria.Localization.LocalizedText;

namespace TalentsSystem.Utils;

public static class UtilsLocalization
{
    public static LT Common_KeybindWarning => GoR_Common(nameof(Common_KeybindWarning));
    public static LT Common_Unknown => GoR_Common(nameof(Common_Unknown));
    public static LT Common_Or => GoR_Common(nameof(Common_Or));
    public static LT Common_And => GoR_Common(nameof(Common_And));

    public static LT Melee => GoR_DamageDipslayName(nameof(Melee));
    public static LT Ranger => GoR_DamageDipslayName(nameof(Ranger));
    public static LT Mage => GoR_DamageDipslayName(nameof(Mage));
    public static LT Summoner => GoR_DamageDipslayName(nameof(Summoner));

    public static LT GoR_Common(string path) => GoR($"Common.{path}");
    public static LT GoR_DamageDipslayName(string path) => GoR_Common($"DamageDipslayName.{path}");


    public static LT UI_ModDescription = GoR_UIMainMenu(nameof(UI_ModDescription));

    public static LT UI_Header_Changelog => GoR_UIHeader(nameof(UI_Header_Changelog));
    public static LT UI_Header_Theme => GoR_UIHeader(nameof(UI_Header_Theme));
    public static LT UI_Header_Info => GoR_UIHeader(nameof(UI_Header_Info));
    public static LT UI_Header_Quests => GoR_UIHeader(nameof(UI_Header_Quests));
    public static LT UI_Header_Melee => GoR_UIHeader(nameof(UI_Header_Melee));
    public static LT UI_Header_Range => GoR_UIHeader(nameof(UI_Header_Range));
    public static LT UI_Header_Magic => GoR_UIHeader(nameof(UI_Header_Magic));
    public static LT UI_Header_Summon => GoR_UIHeader(nameof(UI_Header_Summon));
    public static LT UI_Header_Warlock => GoR_UIHeader(nameof(UI_Header_Warlock));
    public static LT UI_Header_Travel => GoR_UIHeader(nameof(UI_Header_Travel));
    public static LT UI_Header_Current => GoR_UIHeader(nameof(UI_Header_Current));


    public static LT UI_Theme_Terraria => GoR_UITheme(nameof(UI_Theme_Terraria));
    public static LT UI_Theme_Dark => GoR_UITheme(nameof(UI_Theme_Dark));
    public static LT UI_Theme_Blue => GoR_UITheme(nameof(UI_Theme_Blue));
    public static LT UI_Theme_Brown => GoR_UITheme(nameof(UI_Theme_Brown));
    public static LT UI_Theme_Phantom => GoR_UITheme(nameof(UI_Theme_Phantom));

    public static LT UI_Available_Points => GoR_UIPoints(nameof(UI_Available_Points));
    public static LT UI_Spent_Points => GoR_UIPoints(nameof(UI_Spent_Points));

    public static LT UI_Button_ActivateBranch => GoR_UIButtons(nameof(UI_Button_ActivateBranch));
    public static LT UI_Button_ResetPointsBranch => GoR_UIButtons(nameof(UI_Button_ResetPointsBranch));
    public static LT UI_Button_Ok => GoR_UIButtons(nameof(UI_Button_Ok));
    public static LT UI_Button_Cancel => GoR_UIButtons(nameof(UI_Button_Cancel));
    public static LT UI_Button_CompletedQuest => GoR_UIButtons(nameof(UI_Button_CompletedQuest));
    public static LT UI_Button_IncompletedQuest => GoR_UIButtons(nameof(UI_Button_IncompletedQuest));
    public static LT UI_Button_AllQuest => GoR_UIButtons(nameof(UI_Button_AllQuest));

    public static LT UI_Confirm_Header => GoR_UIConfirmation(nameof(UI_Confirm_Header));
    public static LT UI_Confirm_SwitchBranch => GoR_UIConfirmation(nameof(UI_Confirm_SwitchBranch));
    public static LT UI_Confirm_ResetPoints => GoR_UIConfirmation(nameof(UI_Confirm_ResetPoints));
    public static LT UI_Confirm_SpendPoint => GoR_UIConfirmation(nameof(UI_Confirm_SpendPoint));

    public static LT UI_LevelInfo_Level => GoR_UILevelInfo(nameof(UI_LevelInfo_Level));
    public static LT UI_LevelInfo_Exp => GoR_UILevelInfo(nameof(UI_LevelInfo_Exp));
    public static LT UI_LevelInfo_Quests => GoR_UILevelInfo(nameof(UI_LevelInfo_Quests));

    
    public static LT UI_Quests_Exp = GoR_UIQuests(nameof(UI_Quests_Exp));

    public static LT UI_Talents_ActivationType_Passive = GoR_UITalents(nameof(UI_Talents_ActivationType_Passive));
    public static LT UI_Talents_ActivationType_Active = GoR_UITalents(nameof(UI_Talents_ActivationType_Active));
    public static LT UI_Talents_ActivationType_Switchable = GoR_UITalents(nameof(UI_Talents_ActivationType_Switchable));
    public static LT UI_Talents_CurrentLevel = GoR_UITalents(nameof(UI_Talents_CurrentLevel));
    public static LT UI_Talents_NextLevel = GoR_UITalents(nameof(UI_Talents_NextLevel));


    public static LT GoR_UIMainMenu(string path) => GoR($"MainMenu.{path}");
    public static LT GoR_UIHeader(string path) => GoR_UIMainMenu($"Header.{path}");
    public static LT GoR_UITheme(string path) => GoR_UIMainMenu($"Themes.{path}");
    public static LT GoR_UIPoints(string path) => GoR_UIMainMenu($"PointsInfo.{path}");
    public static LT GoR_UIButtons(string path) => GoR_UIMainMenu($"Buttons.{path}");
    public static LT GoR_UIConfirmation(string path) => GoR_UIMainMenu($"Confirmation.{path}");
    public static LT GoR_UILevelInfo(string path) => GoR_UIMainMenu($"LevelInfo.{path}");
    public static LT GoR_UIQuests(string path) => GoR_UIMainMenu($"Quests.{path}");
    public static LT GoR_UITalents(string path) => GoR_UIMainMenu($"Talents.{path}");



    public static LT Quest_Items_Use => GoR_Quest_Items(nameof(Quest_Items_Use));
    public static LT Quest_Items_Use_Max => GoR_Quest_Items(nameof(Quest_Items_Use_Max));
    public static LT Quest_Items_Any_Receive => GoR_Quest_Items(nameof(Quest_Items_Any_Receive));
    public static LT Quest_Items_Any_Receive_Count => GoR_Quest_Items(nameof(Quest_Items_Any_Receive_Count));
    public static LT Quest_Items_All_Receive => GoR_Quest_Items(nameof(Quest_Items_All_Receive));
    public static LT Quest_Items_All_Receive_Count => GoR_Quest_Items(nameof(Quest_Items_All_Receive_Count));


    public static LT Quest_Kill_Any_Count => GoR_Quest_Kill(nameof(Quest_Kill_Any_Count));
    public static LT Quest_Kill_Any => GoR_Quest_Kill(nameof(Quest_Kill_Any));
    public static LT Quest_Kill_Any_Type => GoR_Quest_Kill(nameof(Quest_Kill_Any_Type));
    public static LT Quest_Kill_Any_Boss => GoR_Quest_Kill(nameof(Quest_Kill_Any_Boss));
    public static LT Quest_Kill_Boss => GoR_Quest_Kill(nameof(Quest_Kill_Boss));
    public static LT Quest_Kill_TownNPC => GoR_Quest_Kill(nameof(Quest_Kill_TownNPC));
    public static LT Quest_Kill_Town => GoR_Quest_Kill(nameof(Quest_Kill_Town));

    public static LT Quest_TalkNPC => GoR_Quest_TalkNPC(nameof(Quest_TalkNPC));

    public static LT Quest_Events_EndEvent = GoR_Quest_Events(nameof(Quest_Events_EndEvent));
    public static LT Quest_Events_EndEventOldArmy = GoR_Quest_Events(nameof(Quest_Events_EndEventOldArmy));

    public static LT Quest_Destroy_AnyObject = GoR_Quest_Destroy(nameof(Quest_Destroy_AnyObject));
    public static LT Quest_Destroy_AnyObject_Count = GoR_Quest_Destroy(nameof(Quest_Destroy_AnyObject_Count));
    public static LT Quest_Destroy_Object = GoR_Quest_Destroy(nameof(Quest_Destroy_Object));

    public static LT Quest_Visited_Zone = GoR_Quest_Visited(nameof(Quest_Visited_Zone));

    public static LT Quest_Other_PlayerDeath = GoR_Quest_Other(nameof(Quest_Other_PlayerDeath));


    public static LT GoR_Quest_Items(string path) => GoR_Quest($"Items.{path}");
    public static LT GoR_Quest_Kill(string path) => GoR_Quest($"Kill.{path}");
    public static LT GoR_Quest_TalkNPC(string path) => GoR_Quest($"TalkNPC.{path}");
    public static LT GoR_Quest_Events(string path) => GoR_Quest($"Events.{path}");
    public static LT GoR_Quest_Destroy(string path) => GoR_Quest($"Destroy.{path}");
    public static LT GoR_Quest_Visited(string path) => GoR_Quest($"Visited.{path}");
    public static LT GoR_Quest_Other(string path) => GoR_Quest($"Other.{path}");
    public static LT GoR_Quest(string path) => GoR($"Quests.{path}");



    public static LT Condtions_InShimmeringLake => GoR_Condition(nameof(Condtions_InShimmeringLake));
    public static LT GoR_Condition(string path) => GoR($"Condtions.{path}");



    public static LT Notification_Quest_Header => GoR_Notifications(nameof(Notification_Quest_Header));
    public static LT Notification_LevelUp_Header => GoR_Notifications(nameof(Notification_LevelUp_Header));
    public static LT Notification_LevelUp_Body => GoR_Notifications(nameof(Notification_LevelUp_Body));
    public static LT GoR_Notifications(string path) => GoR($"Notifications.{path}");

    public static LT GoR_Talents(string path) => GoR($"Talents.{path}");

    public static LT GoR(string path) => Language.GetOrRegister($"{GetLocaliztionBasePath()}.{path}");
    public static string GetLocaliztionBasePath() => "Mods.TalentsSystem";
}
