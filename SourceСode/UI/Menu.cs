using Humanizer;
using log4net.Layout;
using Microsoft.VisualBasic;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;
using System;
using System.Buffers.Text;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Security.Policy;
using System.Xml.Linq;
using TalentsSystem.Utils;
using Terraria;
using Terraria.Audio;
using Terraria.GameContent;
using Terraria.GameContent.UI.Elements;
using Terraria.ID;
using Terraria.Localization;
using Terraria.ModLoader;
using Terraria.UI;
using UIeXtension;
using UIeXtension.Enums;
using UIeXtension.MethodsExtensions;
using GL = UIeXtension.Styles.UIExGridLength;
using QDF = TalentsSystem.UIInfoSave.QuestDisplayFlagType;
using TBT = TalentsSystem.TalentsBranchType;
using TS = TalentsSystem.TalentsSystem;
using UL = TalentsSystem.Utils.UtilsLocalization;

namespace TalentsSystem;

public class UIMenuSystem : UIExModSystem<UIMenuState> 
{
    private float _lastUIScale;

    public override void Load()
    {
        base.Load();
        _lastUIScale = Main.UIScale;
    }
    public override void UpdateUI(GameTime gameTime)
    {
        base.UpdateUI(gameTime);
        if (!GetState() || _lastUIScale == Main.UIScale)
            return;
        /////////////////
        _lastUIScale = Main.UIScale;
        State.OnActivate();
    }
    public static void SafeUpdate()
    {
        if (Main.netMode == NetmodeID.Server)
            return;
        UIMenuSystem system = ModContent.GetInstance<UIMenuSystem>();
        if (system is null)
            return;
        var state = system.GetUIState();
        if (state is null)
            return;
        state.OnActivate();
    }
}

/// <summary>
///     Mod main menu
/// </summary>
public class UIMenuState : UIState
{
    /// <summary>
    ///     Used to switch between submenus
    /// </summary>
    public enum MenuType : byte
    {
        Changelog,
        Theme,
        Info,
        Quests,
        Classes
    }

    public event Action<TBT> BranchActivated;
    public event Action<TBT> BranchReseted;
    public event Action<Talent> TalentLevelIncreased;

    /// <summary>
    ///     A status flag indicating whether the confirmation window is open
    /// </summary>
    protected bool StateConfirmation = false;

    /// <summary>
    ///     Top-level layout manager. 
    ///     It controls the positioning of the main interface elements: 
    ///     the header; 
    ///     the scrolling menu for selecting submenus and main content sections; 
    ///     and the bottom information panel.
    /// </summary>
    protected UIExGrid MainGrid;

    /// <summary>
    ///     Layout manager for the bottom control panel
    /// </summary>
    protected UIExGrid SideBottomGrid;

    /// <summary>
    ///     A layout component that manages the positioning of content preceding the scrollable list content.
    /// </summary>
    protected UIExStackPanel ScrollContentStackPanel;

    /// <summary>
    ///     Arranges the elements of the confirmation window.
    /// </summary>
    protected UIConfirmationGrid ConfirmationGrid;

    /// <summary>
    ///     List of key information for each submenu
    /// </summary>
    protected UIList ScrollListContent;

    /// <summary>
    ///     Scroll wheel <see cref="ScrollListContent"/>
    /// </summary>
    protected UIScrollbar ScrollbarContent;

    protected UITextAutoScale HeaderText;

    protected UITextAutoScale FreePointsText;
    protected UITextAutoScale SpentPointsText;
    protected UITextAutoScale ActivateText;
    protected UITextAutoScale ResetText;

    /// <summary>
    ///     Places <see cref="TooltipGrid"/> on the screen
    /// </summary>
    protected UIExCanvas TooltipCanvas;

    /// <summary>
    ///     Arranges the elements of the talent description.
    /// </summary>
    protected UIConfirmationGrid TooltipGrid;

    protected static MenuType LastSelectedMenuType = MenuType.Info;

    /// <summary>
    ///     The last selected talent submenu. 
    ///     Null if the last selected submenu does not belong to a talent tree.
    /// </summary>
    protected static TBT? LastSectionTBTMenuType = null; 

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (MainGrid is not null &&
            Main.LocalPlayer is not null
            && MainGrid.ContainsPoint(Main.MouseScreen))
                Main.LocalPlayer.mouseInterface = true;
    }
    public override void OnInitialize()
    {
        if (TS.IsPlayerNotReady())
            return;

        // Prevents all layout components in the current UIState from performing layout until EndLayoutPreparation is called.
        // During initialization, the layout is practically useless, as the elements are still in the process of being created.
        UIExLayout.BeginLayoutPreparation(this);

        /////////////////////
        MainGrid = new();
        SideBottomGrid = new();
        ScrollContentStackPanel = new();
        ConfirmationGrid = new();
        //////////////////////
        ScrollbarContent = new();
        ScrollListContent = new();
        ScrollListContent.ManualSortMethod = items => { };
        ScrollListContent.PaddingLeft = 10f;
        ScrollListContent.ListPadding = 15f;
        HeaderText = UtilsUI.CreateUIText("");
        //////////////////////
        FreePointsText = UtilsUI.CreateUIText("");
        SpentPointsText = UtilsUI.CreateUIText("");
        ActivateText = UtilsUI.CreateUIText(UL.UI_Button_ActivateBranch);
        ResetText = UtilsUI.CreateUIText(UL.UI_Button_ResetPointsBranch);
        FreePointsText = UtilsUI.CreateUIText("");
        SpentPointsText = UtilsUI.CreateUIText("");
        //////////////////////
        TooltipCanvas = new();
        TooltipGrid = new();
        //////////////////////
        InitializeMainGrid();
        InitializeSideButton();
        InitializeLeftSideStackPanel();
        InitializeScrollContentStackPanel();
        InitializeTooltipGrid();
        InitializeConfirmationGrid();
    }
    public override void OnActivate()
    {
        if (TS.IsPlayerNotReady())
            return;
        if (MainGrid is null)
            OnInitialize();
        //////////////////////
        SwitchMenu();
        SetMainGridSize();

        // Applying the UI theme to the current UIState
        ThemeStyles.Instance.ApplyStylesToBranch(this);

        // Notifies the entire layout system within the current UIState that a forced recalculation of element layout is required.
        UIExLayout.EndLayoutPreparation(this);

        // Prevents all layout components in the current UIState from performing layout until the next call to EndLayoutPreparation.
        UIExLayout.BeginLayoutPreparation(this);

        UtilsUI.RefreshTextScaleForBranch(this);
    }
    public override void OnDeactivate()
    {
        HideConfirmation();
        HideTalentTooltip();
    }

    /// <summary>
    ///     Sets the size of the top layout component(which is the top display panel) based on the selected menu.
    /// </summary>
    protected void SetMainGridSize()
    {
        if (LastSelectedMenuType == MenuType.Info || LastSelectedMenuType == MenuType.Quests)
        {
            MainGrid.MinWidth.Precent = 0.15f;
            MainGrid.Width.Pixels = 1500f;
        }
        else
        {
            MainGrid.MinWidth.Precent = 0.1f;
            MainGrid.Width.Pixels = 1000f;
        }
        MainGrid.MaxWidth.Precent = 0.95f;
        MainGrid.Height.Precent = 0.8f;
    }
    protected void InitializeMainGrid() 
    {
        Append(MainGrid);

        MainGrid.HAlign = 0.5f;
        MainGrid.VAlign = 0.5f;

        // Sets the UIPanel style for the current UIExElement (which is subsequently modified in the code by the ThemeStyle class).
        MainGrid.StyleDisplay.tModLoaderStyle = true;

        // Specifies that child elements must stretch to fit the size of the cell in which it is placed.
        MainGrid.StyleLayout.JustifyContent = UIExAlignment.Stretch;
        MainGrid.StyleLayout.AlignItems = UIExAlignment.Stretch;

        // Grid settings
        var styleLayoutGrid = MainGrid.StyleLayout.Grid();
        styleLayoutGrid.AddRowDefinitions(
            GL.FromFr(0.075f),
            GL.FromPx(7f),
            GL.FromFr(1f));
        styleLayoutGrid.AddColumnDefinitions(
            GL.FromFr(0.125f),
            GL.FromPx(7f),
            GL.FromFr(1f),
            GL.FromPx(54f),
            GL.FromPx(20f));
        ///////////////////////////
        UtilsUI.AddSeparator(MainGrid, UIExOrientation.Horizontal, row: 1, column: 0);
        UtilsUI.AddSeparator(MainGrid, UIExOrientation.Vertical, row: 2, column: 1, rowSpan: 1);
    }

    /// <summary>
    ///     Adds a bottom panel if one has not already been added. 
    ///     Checks regarding whether the panel should be added are performed outside this method.
    /// </summary>
    protected void AddSideBottomMainGrid()
    {
        var styleLayoutGrid = MainGrid.StyleLayout.Grid();
        if (styleLayoutGrid.RowsCount == 5)
            return;
        //////////////////////////
        styleLayoutGrid.AddRowDefinitions(
            GL.FromPx(7f),
            GL.FromPx(50f));
        //////////////////////////
        MainGrid.Data = UtilsUI.AddSeparator(MainGrid, UIExOrientation.Horizontal, row: 3, column: 0);
        MainGrid.Append(SideBottomGrid);
    }

    /// <summary>
    ///     Removes the bottom information panel if it has already been added. 
    ///     Checks regarding whether the panel should be removed are performed outside this method.
    /// </summary>
    protected void RemoveSideBottomMainGrid()
    {
        var styleLayoutGrid = MainGrid.StyleLayout.Grid();
        if (styleLayoutGrid.RowsCount == 3)
            return;
        ////////////////////////
        styleLayoutGrid.RemoveRowDefinitionsRange(3, 2);
        MainGrid.RemoveChild((UISep)MainGrid.Data);
        MainGrid.RemoveChild(SideBottomGrid);
    }

    protected void InitializeLeftSideStackPanel() 
    {
        // An inner container holding a Scrollbar and a UIList (see below)
        UIExGrid leftSideGrid = new();
        MainGrid.Append(leftSideGrid);

        // Specifies the position leftSideGrid (in the MainGrid)
        leftSideGrid.StyleLayoutChild().Grid().Set(row: 2, column: 0, rowSpan: 1, columnSpan: 1);

        // Child elements must be centered along each axis.
        leftSideGrid.StyleLayout.Set(
            orientation: UIExOrientation.Vertical,
            justifyContent: UIExAlignment.Center,
            alignItems: UIExAlignment.Center);

        // leftSideGrid must create a grid based on the following principle:
        // allocate space for UIScrollbar (GL.FromAuto()) and assign the remaining space to UIList (GL.FromFr(1f)).
        var styleGrid = leftSideGrid.StyleLayout.Grid();
        styleGrid.AddColumnDefinitions(GL.FromFr(1f), GL.FromAuto());
        styleGrid.AddRowDefinition(GL.FromFr(1f));
        
        /////////////////////////////
        UIScrollbar scroll = new();
        leftSideGrid.Append(scroll);

        // The size is specified using a layout style class, as this element is nested within a UI Extension library layout component.
        var styleChild = scroll.StyleLayoutChild();
        styleChild.Margin = new(pixels: true, left: 0, top: 20f, right: 0f, bottom: 20f);
        styleChild.Width.Pixels = 20f;
        styleChild.Height.Precent = 0.9f;
        scroll.StyleLayoutChild().Grid().Column = 1;
        UIList list = new();
        leftSideGrid.Append(list);

        // It is specified that the UIList must stretch along each axis, overriding similar values ​​set in leftSideGrid.
        list.StyleLayoutChild().Set(justifySelf: UIExAlignment.Stretch, alignSelf: UIExAlignment.Stretch);

        list.StyleLayoutChild().Margin = new(pixels: true, left: 0, top: 20f, right: 0f, bottom: 20f);
        list.SetScrollbar(scroll);

        /////////////////////////////
        AppendButton(ItemID.Book, padding: 0f, MenuType.Changelog, null);
        UtilsUI.AddSeparator(list, 5f, 5f);
        AppendButton(ItemID.Paintbrush, padding: 0f, MenuType.Theme, null);
        UtilsUI.AddSeparator(list, 5f, 5f);
        AppendButton(ItemID.GuideVoodooDoll, padding: 0f, MenuType.Info, null);
        AppendButton(ItemID.FallenStar, padding: 0f, MenuType.Quests, null);
        UtilsUI.AddSeparator(list, 5f, 5f);
        AppendButton(ItemID.Zenith, padding: 15f, MenuType.Classes, TBT.Melee);
        AppendButton(ItemID.Phantasm, padding: 15f, MenuType.Classes, TBT.Ranged);
        AppendButton(ItemID.LastPrism, padding: 0f, MenuType.Classes, TBT.Magic);
        AppendButton(ItemID.StardustDragonStaff, padding: 15f, MenuType.Classes, TBT.Summon);
        AppendButton(ItemID.ShadowFlameHexDoll, padding: 0f, MenuType.Classes, TBT.Warlock);
        AppendButton(ItemID.Compass, padding: 0f, MenuType.Classes, TBT.Travel);

        void AppendButton(int itemID, float padding, MenuType type, TBT ? tbtType)
        {
            // IconAnimationInfo and UIIconAnimation are classes for playing sprite animations, created by this mod.
            IconAnimationInfo iconAnimInfo = IconAnimationInfo.FromItemID(itemID);
            iconAnimInfo.UseParentDimensions = padding > 0f;
            UIIconAnimation image = new(iconAnimInfo);
            //////////////////////////
            UIButtonFrame frame = UIExLayout.Wrap<UIButtonFrame>(image, stretch: false, center: true);
            list.Add(frame);
            frame.SetPadding(padding);
            frame.StyleDisplay.tModLoaderStyle = true;
            frame.StyleDisplay.PaddingAutoControl = padding == 0f;
            frame.Width = StyleDimension.FromPixels(64f);
            frame.Height = StyleDimension.FromPixels(64f);
            frame.HAlign = 0.5f;
            AddLeftClickEvent(frame, (evt, el) =>
            {
                if (type != MenuType.Changelog && LastSelectedMenuType == type && LastSectionTBTMenuType == tbtType)
                    return;
                LastSelectedMenuType = type;
                LastSectionTBTMenuType = tbtType;

                // If the values ​​LastSelectedMenuType/LastSectionTBTMenuType have changed,
                // the menu must be reactivated to display the required elements and arrange them.
                OnActivate();
            });
        }
    }
    protected void InitializeScrollContentStackPanel()
    {
        // Operations of this kind were analyzed in the comments for the InitializeLeftSideStackPanel method.
        ScrollContentStackPanel.StyleLayoutChild().Grid().Set(row: 2, column: 2, rowSpan: 1, columnSpan: 1);
        var style1 = ScrollContentStackPanel.StyleLayout;
        style1.JustifyContent = UIExAlignment.Start;
        style1.AlignItems = UIExAlignment.Stretch;
        style1.StackPanel().Spacing.Pixels = 10f;
        /////////////////////////

        // The parameters of the UIList containing the main menu's core information are specified.
        // ScrollListContent is not being added inside ScrollContentStackPanel in this method.
        // This is because the order in which elements are added is important for UIExStackPanel,
        // and certain elements (depending on the selected submenu) must be positioned earlier (higher up) than ScrollListContent.
        // This will be covered in more detail during the description of the SwitchMenu method
        // and the FillQuestsToMenuStackPanel method, which populates the quest menu.
        ScrollListContent.StyleLayoutChild().JustifySelf = UIExAlignment.Stretch;
        ScrollListContent.SetScrollbar(ScrollbarContent);
        ////////////////////////
        var style2 = ScrollbarContent.StyleLayoutChild();
        style2.Grid().Set(row: 2, column: 5, rowSpan: 1, columnSpan: 1);
        style2.Margin.Top.Pixels = 40f;
        style2.Margin.Bottom.Pixels = 40f;
        ////////////////////////
        MainGrid.Append(ScrollContentStackPanel);
        MainGrid.Append(ScrollbarContent);
    }

    /// <summary>
    ///     Switching the current submenu
    /// </summary>
    protected void SwitchMenu()
    {
        string headerText = "";

        // The bottom information menu is required only for the talent tree submenus;
        // it should be hidden for other menus.
        // Therefore, the bottom menu is removed beforehand
        RemoveSideBottomMainGrid();

        // Clearing the main menu content area before populating it with content from the newly selected submenu.
        ScrollContentStackPanel.RemoveAllChildren();
        ScrollListContent.Clear();
        ScrollListContent.PaddingTop = 50f;
        /////////////////////
        switch (LastSelectedMenuType)
        {
            case MenuType.Changelog:
                headerText = UL.UI_Header_Changelog.Value;
                FillChangelogButtonsToMenuList();
                break;
            case MenuType.Theme:
                headerText = UL.UI_Header_Theme.Value;
                FillThemeToMenuList();
                break;
            case MenuType.Info:
                headerText = UL.UI_Header_Info.Value;
                FillInfoToMenuList();
                break;
            case MenuType.Quests:
                headerText = UL.UI_Header_Quests.Value;
                FillQuestsToMenuStackPanel();
                break;
            case MenuType.Classes:
                switch (LastSectionTBTMenuType)
                {
                    case TBT.Melee:
                        headerText = UL.UI_Header_Melee.Value;
                        break;
                    case TBT.Ranged:
                        headerText = UL.UI_Header_Range.Value;
                        break;
                    case TBT.Magic:
                        headerText = UL.UI_Header_Magic.Value;
                        break;
                    case TBT.Summon:
                        headerText = UL.UI_Header_Summon.Value;
                        break;
                    case TBT.Travel:
                        headerText = UL.UI_Header_Travel.Value;
                        break;
                    case TBT.Warlock:
                        headerText = UL.UI_Header_Warlock.Value;
                        break;
                }
                AddSideBottomMainGrid();
                var save = TS.GetLocal<TalentsSaveInfo>();
                if (save.CurrentBranch == LastSectionTBTMenuType)
                    headerText = $"{headerText} ({UL.UI_Header_Current.Value})";
                FillTalentsToMenuList();
                break;
        }
        
        FillOrClearSideButtomGrid();
        CreateHeaderText(headerText);

        // Adding a UIList to the ScrollContentStackPanel after preliminary information has been added to the ScrollContentStackPanel during submenu population(if applicable).
        ScrollContentStackPanel.Append(ScrollListContent);
    }

    /// <summary>
    ///     Populates buttons, each displaying information about a specific update.
    /// </summary>
    protected void FillChangelogButtonsToMenuList()
    {
        AppendButton(new DateTime(year: 2026, month: 10, day: 12), version: "0.0.0.1");
        /////////////////////
        void AppendButton(DateTime date, string version)
        {
            string path = "Mods.TalentsSystem.Changelogs";
            string versionPath = $"{path}.v_{version.Replace('.', '_')}";
            string text = $"[{date.ToString("d", CultureInfo.CurrentCulture)}] " +
                $"{Language.GetOrRegister($"{path}.Version")}: {version} | " +
                $"{Language.GetOrRegister($"{versionPath}.Header")}";
            UIExElement wrap = UtilsUI.CreateWrapText<UIExElement>(text);
            wrap.StyleDisplay.tModLoaderStyle = true;
            wrap.StyleDisplay.PaddingAutoControl = false;
            wrap.PaddingTop = 20f;
            wrap.PaddingBottom = 20f;
            wrap.Height.Pixels = 75f;
            wrap.Width.Set(-40f, 1f);
            wrap.MarginLeft = 20f;
            wrap.MarginRight = 20f;
            ScrollListContent.Add(wrap);
            wrap.OnLeftClick += (evt, el) => FillChangelogInfoToMenuList($"{versionPath}.Description");
        }
    }

    /// <summary>
    ///     Removes buttons added via method FillChangelogButtonsToMenuList and populates ScrollListContent with a description of the specific update.
    /// </summary>
    protected void FillChangelogInfoToMenuList(string path)
    {
        ScrollListContent.Clear();
        string text = Language.GetOrRegister(path).Value;
        string[] split1 = text.Split("\n\n");
        foreach (var s1 in split1)
        {
            string[] split2 = s1.Split('\n');
            foreach (var s2 in split2)
            {
                var uiText = new UIText(s2, 1.2f);
                float height = FontAssets.MouseText.Value.MeasureString(s2).Y;
                uiText.Height.Pixels = height * 1.2f;
                uiText.MarginLeft = 20f;
                ScrollListContent.Add(uiText);
            }
            UtilsUI.AddSeparator(ScrollListContent);
        }
        ThemeStyles.Instance.ApplyStylesToBranch(ScrollListContent);
    }

    /// <summary>
    ///     Main content population: button for selecting a specific topic
    /// </summary>
    protected void FillThemeToMenuList()
    {
        AppendButton(UL.UI_Theme_Terraria.Value, ThemeStyles.UIThemeType.Terraria);
        AppendButton(UL.UI_Theme_Blue.Value, ThemeStyles.UIThemeType.Blue);
        AppendButton(UL.UI_Theme_Dark.Value, ThemeStyles.UIThemeType.Dark);
        AppendButton(UL.UI_Theme_Brown.Value, ThemeStyles.UIThemeType.Brown);
        AppendButton(UL.UI_Theme_Phantom.Value, ThemeStyles.UIThemeType.Phantom);
        void AppendButton(string text, ThemeStyles.UIThemeType type)
        {
            UIExElement wrap = UtilsUI.CreateWrapText<UIExElement>(text);
            wrap.StyleDisplay.tModLoaderStyle = true;
            wrap.StyleDisplay.PaddingAutoControl = false;
            wrap.PaddingTop = 20f;
            wrap.PaddingBottom = 20f;
            wrap.Height.Pixels = 75f;
            wrap.Width.Set(-40f, 1f);
            wrap.MarginLeft = 20f;
            wrap.MarginRight = 20f;
            AddLeftClickEvent(
                wrap, 
                (evt, el) =>
                {
                    var save = TS.GetLocal<UIInfoSave>();
                    save.MenuThemeType = type;
                    ThemeStyles.Instance.ApplyStylesToBranch(this);
                },
                soundOpen: SoundID.DD2_WitherBeastCrystalImpact);
            ///////////////////
            ScrollListContent.Add(wrap);
        }
    }

    /// <summary>
    ///     Populating the main content area with the mod description
    /// </summary>
    protected void FillInfoToMenuList()
    {
        string text = UL.UI_ModDescription.Value.Replace("\r", "");
        string[] texts = text.Split("\n\n");
        for (int i = 0; i < texts.Length; i++)
        {
            string[] textsInner = texts[i].Split("\n");
            for (int j = 0; j < textsInner.Length; j++)
            {
                if (textsInner[j] == string.Empty)
                    UtilsUI.AddSeparator(ScrollListContent);
                else
                {
                    var uiText = new UIText(textsInner[j], 1.2f);
                    float height = FontAssets.MouseText.Value.MeasureString(textsInner[j]).Y;
                    uiText.Height.Pixels = height * 1.2f;
                    uiText.MarginLeft = 20f;
                    ScrollListContent.Add(uiText);
                }
            }
            if (i != texts.Length - 1)
                UtilsUI.AddSeparator(ScrollListContent);
        }
    }

    /// <summary>
    ///     Populating the main content with quest information
    /// </summary>
    protected void FillQuestsToMenuStackPanel()
    {
        ScrollListContent.PaddingTop = 0f;
        UtilsUI.AddSeparator(ScrollListContent, marginTop: 0f, marginBottom: 0f);

        // Information on current level, experience, and number of tasks
        UIExStackPanel stackPanelLevelInfo = new();
        // Buttons for switching task sorting (all, completed, incomplete)
        UIExStackPanel stackPanelButtons = new();
        SetStackPanelStyle(stackPanelLevelInfo, tModLoaderStyle: true, paddingAutoControl: false);
        SetStackPanelStyle(stackPanelButtons, tModLoaderStyle: false, paddingAutoControl: false);
        ScrollContentStackPanel.Append(stackPanelLevelInfo);
        ScrollContentStackPanel.Append(stackPanelButtons);
        void SetStackPanelStyle(UIExStackPanel stackPanel, bool tModLoaderStyle, bool paddingAutoControl)
        {
            stackPanel.StyleDisplay.tModLoaderStyle = tModLoaderStyle;
            stackPanel.StyleDisplay.PaddingAutoControl = paddingAutoControl;

            var style = stackPanel.StyleLayoutChild();
            style.Height.Pixels = 48f;
            style.Margin.Top.Pixels = 10f;
            stackPanel.StyleLayout.Set(UIExOrientation.Horizontal, UIExAlignment.Stretch, UIExAlignment.Stretch);
            stackPanel.StyleLayout.StackPanel().Spacing.Pixels = 5f;
        }
        /////////////////
        var save = TS.GetLocal<QuestSaveInfo>();
        QuestsManager questManager = QuestsManager.Instance;
        int level = save.Level;
        int maxLevel = save.GetMaxLevel();
        bool max = level >= maxLevel;
        int exp = max ? save.GetMaxLevelExp() : save.Exp;
        int maxExp = max ? exp : save.GetNeedExp(level);
        int questsCompleted = save.CompletedQuests.Count;
        int maxQuests = questManager.AllQuestsArray.Count;
        ////////////////
        //float stackPanelTextScale = 1.0f;
        var wrapsLevelInfo = CreateAndAppendWrapTexts(
            stackPanelLevelInfo,
            (UL.UI_LevelInfo_Level.WithFormatArgs(level, maxLevel).Value, false),
            (UL.UI_LevelInfo_Exp.WithFormatArgs(exp, maxExp).Value, false),
            (UL.UI_LevelInfo_Quests.WithFormatArgs(questsCompleted, maxQuests).Value, false));
        var wrapsButtons = CreateAndAppendWrapTexts(
            stackPanelButtons,
            (UL.UI_Button_AllQuest.Value, true),
            (UL.UI_Button_CompletedQuest.Value, true),
            (UL.UI_Button_IncompletedQuest.Value, true));
        UIExElement[] CreateAndAppendWrapTexts(UIExStackPanel stackPanel, params (string text, bool tStyle)[] informations)
        {
            var result = new UIExElement[informations.Length];
            for(int i = 0; i < informations.Length; i++)
            {
                var info = informations[i];
                var wrap = UtilsUI.CreateWrapText<UIExElement>(info.text);
                wrap.StyleDisplay.tModLoaderStyle = info.tStyle;
                var style = wrap.StyleLayoutChild();
                style.Margin.Left.Percent = 0.025f;
                style.Margin.Right.Percent = 0.025f;
                stackPanel.Append(wrap);
                result[i] = wrap;
            }
            return result;
        }
        //////////////////
        var saveCommon = TS.GetLocal<UIInfoSave>();
        AddLeftClickEvent(wrapsButtons[0], QDF.All);
        AddLeftClickEvent(wrapsButtons[1], QDF.Completed);
        AddLeftClickEvent(wrapsButtons[2], QDF.Incompleted);
        void AddLeftClickEvent(UIElement element, QDF flagDisplay)
        {
            element.OnLeftClick += (evt, el) =>
            {
                if (flagDisplay == saveCommon.QuestDisplayFlag)
                    SoundEngine.PlaySound(SoundID.MenuClose);
                else
                {
                    SoundEngine.PlaySound(SoundID.DD2_WitherBeastCrystalImpact);
                    saveCommon.QuestDisplayFlag = flagDisplay;
                    OnActivate();
                }
            };
        }
        //////////////////
        int count = saveCommon.QuestDisplayFlag switch
        {
            QDF.Completed => save.CompletedQuests.Count,
            QDF.Incompleted => save.IncompletedQuests.Count,
            _ => questManager.AllQuestsArray.Count
        };
        int[] questsIds = new int[count];
        for (int i = 0; i < count; i++)
            questsIds[i] = saveCommon.QuestDisplayFlag switch
            {
                QDF.Completed => save.CompletedQuests[i],
                QDF.Incompleted => save.IncompletedQuests[i],
                _ => questManager.AllQuestsArray[i]
            };
        questsIds = questsIds.OrderBy(questId => questManager.QuestExpTable[questId]).ToArray();
        for (int i = 0; i < questsIds.Length; i++)
        {
            Quest quest = questManager.QuestsTable[questsIds[i]];
            UIExStackPanel questStackPanel = UtilsUI.GetQuestInfoStackPanel(quest, showSync: true, tModLoaderStyle: false);
            ScrollListContent.Add(questStackPanel);
            UtilsUI.AddSeparator(ScrollListContent, marginTop: 0f, marginBottom: 0f);
        }
    }

    /// <summary>
    ///     Populating the main content area with information about talents
    /// </summary>
    protected void FillTalentsToMenuList()
    {
        TBT branch = (TBT)LastSectionTBTMenuType;
        var levels = TalentsManager.Instance.LevelsTalentsTable[branch];
        List<int> sortedLevels = new();
        foreach(var level in levels.Keys)
            sortedLevels.Add(level);
        sortedLevels = sortedLevels.Distinct().OrderBy(level => level).ToList();
        bool isMaxPointsInSection = true;
        foreach (var level in sortedLevels)
        {
            FillTalentsSection(branch, level, ref isMaxPointsInSection, levels[level]);
            UtilsUI.AddSeparator(ScrollListContent, 0f, 0f);
        }
    }

    /// <summary>
    ///     Filling out the dedicated talents section
    /// </summary>
    protected void FillTalentsSection(TBT branch, int sectionLevel, ref bool lastIsMaxPointsInSection, List<Talent> talents)
    {
        var save = TS.GetLocal<TalentsSaveInfo>();
        int totalPointsUsed = 0;
        foreach (var talent in talents)
            totalPointsUsed += save.GetActualPoints(branch, talent.ID);
        //////////////////////
        int maxPoints = TalentsManager.Instance.TalentMaxPointsTable[branch][talents[0].ID];
        bool isMaxPointsInSection = maxPoints == totalPointsUsed;
        bool? sectionAccessFlag = null;
        if (isMaxPointsInSection)
            sectionAccessFlag = true;
        else if(!lastIsMaxPointsInSection)
            sectionAccessFlag = false;
        lastIsMaxPointsInSection = isMaxPointsInSection;
        //////////////////////
        AssetsManager.UITextureID sectionBorderIconID = sectionAccessFlag switch
        {
            true => AssetsManager.UITextureID.SectionTalentFrameMax,
            false => AssetsManager.UITextureID.SectionTalentFrameClose,
            _ => AssetsManager.UITextureID.SectionTalentFrameCurrent
        };
        UIImage sectionFrame = new(AssetsManager.Instance.GetAssetUI(sectionBorderIconID).Value);
        sectionFrame.ScaleToFit = true;
        //UIElement sectionFrame = new();
        ScrollListContent.Add(sectionFrame);
        sectionFrame.Width.Percent = 1f;
        sectionFrame.Height.Pixels = 256f;
        /////////////////////

        // Container for horizontally arranging elements within a section.
        // You specify the number of columns for this layout component, and it handles the positioning of the elements itself.
        // Alternatively, you could use UIExStackPanel.
        UIExUniformGrid grid = new UIExUniformGrid();
        sectionFrame.Append(grid);
        grid.Width = StyleDimension.Fill;
        grid.Height = StyleDimension.Fill;
        grid.StyleLayout.Set(
            orientation: UIExOrientation.Horizontal,
            justifyContent: UIExAlignment.Stretch,
            alignItems: UIExAlignment.Stretch);
        grid.StyleLayout.UniformGrid().Set(rowsCount: 1, columnsCount: talents.Count);
        foreach (var talent in talents)
            AddTalentToSection(save, grid, branch, talent, maxPoints, sectionAccessFlag);
    }

    /// <summary>
    ///     Adding a separate talent to the section
    /// </summary>
    /// <param name="save">The ModPlayer class, containing information about the player's current talent points.</param>
    protected void AddTalentToSection(
        TalentsSaveInfo save, 
        UIExUniformGrid grid, 
        TBT branch, 
        Talent talent, 
        int maxPoints,
        bool? sectionAccessFlag)
    {
        int points = save.GetActualPoints(branch, talent.ID);
        AssetsManager.UITextureID frameIconType = AssetsManager.UITextureID.TalentFrameClose;
        if (points > 0 && points < maxPoints)
            frameIconType = AssetsManager.UITextureID.TalentFrameCurrent;
        else if(points >= maxPoints)
            frameIconType = AssetsManager.UITextureID.TalentFrameMax;
        //////////////////////////

        // Arranges the talent title and icon vertically, one after the other.
        UIExStackPanel talentStackPanel = new();
        grid.Append(talentStackPanel);
        talentStackPanel.StyleLayout.Set(
            orientation: UIExOrientation.Vertical,
            justifyContent: UIExAlignment.Center,
            alignItems: UIExAlignment.Center);
        talentStackPanel.StyleLayout.StackPanel().Spacing = StyleDimension.FromPercent(0.075f);
        ///////////////////////////
        UIText header = new(talent.Header, 1.2f);
        talentStackPanel.Append(header);
        ///////////////////////////
        UIImage frameIcon = new(AssetsManager.Instance.GetAssetUI(frameIconType).Value);
        talentStackPanel.Append(frameIcon);
        frameIcon.ScaleToFit = true;
        var style = frameIcon.StyleLayoutChild();
        style.Width.Pixels = 90f;
        style.Height.Pixels = 90f;
        frameIcon.SetPadding(25f);
        //////////////////////////
        UIIconAnimation icon = new(talent.Icon);
        frameIcon.Append(icon);
        icon.Width = StyleDimension.Fill;
        icon.Height = StyleDimension.Fill;
        icon.IgnoresMouseInteraction = true;
        //////////////////////////
        UIText pointsInfo = new($"{points} / {maxPoints}", 1.2f);
        talentStackPanel.Append(pointsInfo);
        //////////////////////////
        frameIcon.OnMouseOver += (evt, el) => ShowTalentTooltip(
            talent.Header.Value,
            talent.ActivateType,
            points > 0 ? talent.DescriptionGetter(points).Value : null,
            points < maxPoints ? talent.DescriptionGetter(points + 1).Value : null);
        frameIcon.OnMouseOut += (evt, el) => HideTalentTooltip();
        //////////////////////////
        if (sectionAccessFlag is not null || save.GetFreePointsBranch(branch) < 1)
        {
            AddLeftClickEvent(
                frameIcon, 
                (evt, el) => SoundEngine.PlaySound(SoundID.Unlock));
        }
        else
            AddConfirmationLeftClickEvent(
                frameIcon,
                (evt, el) =>
                {
                    TalentLevelIncreased.Invoke(talent);
                },
                SoundID.MenuOpen,
                null,
                UL.UI_Confirm_SpendPoint.WithFormatArgs(talent.Header, points + 1).Value);
    }

    /// <summary>
    ///     Initialize the bottom section with information about the current branch:
    ///     allocated talent points; 
    ///     available talent points; 
    ///     skill reset button for the current tree; 
    ///     button to select the tree as the active one
    /// </summary>
    protected void InitializeSideButton()
    {
        SideBottomGrid.StyleLayoutChild().Grid().Set(row: 4, column: 0, columnSpan: int.MaxValue);
        ///////////////////
        var styleLayout = SideBottomGrid.StyleLayout;
        styleLayout.JustifyContent = UIExAlignment.Stretch;
        styleLayout.AlignItems = UIExAlignment.Stretch;
        /////////////////////
        var styleLayoutGrid = styleLayout.Grid();
        styleLayoutGrid.AddRowDefinition(GL.FromFr(1f));
        styleLayoutGrid.AddColumnDefinitions(
            GL.FromFr(1f),
            GL.FromPx(7f),
            GL.FromFr(1f),
            GL.FromPx(7f),
            GL.FromFr(0.75f),
            GL.FromPx(7f),
            GL.FromFr(0.75f));
        //////////////////////
        UtilsUI.AddSeparator(SideBottomGrid, UIExOrientation.Vertical, row: 0, column: 1);
        UtilsUI.AddSeparator(SideBottomGrid, UIExOrientation.Vertical, row: 0, column: 3);
        UtilsUI.AddSeparator(SideBottomGrid, UIExOrientation.Vertical, row: 0, column: 5);
    }

    /// <summary>
    /// Removes old information from the bottom panel.
    /// If the current submenu requires the bottom panel to be displayed, it populates it with up-to-date information
    /// </summary>
    protected void FillOrClearSideButtomGrid()
    {
        Prepare(
            (FreePointsText, false),
            (SpentPointsText, false),
            (ActivateText, true),
            (ResetText, true));
        void Prepare(params (UIElement element, bool removeSelf)[] information)
        {
            foreach (var info in information)
            {
                if (info.element.Parent is null)
                    continue;

                // If the current text has a wrapper element (for the Reset and Activate buttons), remove the parent from its container.
                info.element.Parent.Remove();

                // Remove self from the parent element, if required.
                if (info.removeSelf)
                    info.element.Remove();
            }
        }
        if (LastSelectedMenuType != MenuType.Classes)
            return;

        ///////////////

        TBT convertedTBT = (TBT)LastSectionTBTMenuType;
        var save1 = TS.GetLocal<QuestSaveInfo>();
        var save2 = TS.GetLocal<TalentsSaveInfo>();
        int level = save1.Level;
        int spentPoints = save2.GetSpentPointsBranch(convertedTBT);
        int freePoints = save2.GetFreePointsBranch(convertedTBT);
        //////////////////////
        FreePointsText.SetText(UL.UI_Available_Points.WithFormatArgs(freePoints));
        SpentPointsText.SetText(UL.UI_Spent_Points.WithFormatArgs(spentPoints));
        //////////////////////
        if (FreePointsText.Parent is null)
        {
            UIExLayout.Wrap<UIExElement>(FreePointsText);
            UIExLayout.Wrap<UIExElement>(SpentPointsText);
            SpentPointsText.Parent.StyleLayoutChild().Grid().Column = 2;
        }
        SideBottomGrid.Append(FreePointsText.Parent);
        SideBottomGrid.Append(SpentPointsText.Parent);
        //////////////////////
        if(spentPoints > 0)
            AppendButton(
                ResetText,
                UL.UI_Confirm_ResetPoints.Value,
                column: 4,
                () => BranchReseted(convertedTBT));
        if(convertedTBT != save2.CurrentBranch)
            AppendButton(
                ActivateText,
                UL.UI_Confirm_SwitchBranch.Value,
                column: 6,
                () => BranchActivated(convertedTBT));
        void AppendButton(UITextAutoScale btText, string confirmText, int column, Action action)
        {
            var wrap3 = UtilsUI.WrapInButtonFrame(btText);
            wrap3.StyleDisplay.PaddingAutoControl = false;
            wrap3.StyleLayoutChild().Grid().Column = column;
            SideBottomGrid.Append(wrap3);
            //////////////////////
            wrap3.StyleDisplay.tModLoaderStyle = true;
            wrap3.StyleLayoutChild().Margin = new(
                pixels: false,
                left: 0.0125f,
                right: 0.0125f,
                top: 0.075f,
                bottom: 0.075f);
            //////////////////////
            AddConfirmationLeftClickEvent(
                wrap3,
                (evt, el) => action(),
                SoundID.MenuOpen,
                null,
                confirmText);
        }
    }
    public void InitializeConfirmationGrid()
    {
        // The main container for the confirmation window.
        // It is located within MainGrid and constrained by its dimensions.
        // ConfirmationGrid must always be positioned in the center of MainGrid and manage its own size.
        // To achieve this, the WithoutLayout flag is set to true,
        // indicating that the parent layout manager should not control the position and size of this element
        // (i.e., excluding the element from the layout lifecycle).
        ConfirmationGrid.StyleLayoutChild().WithoutLayout = true;
        ConfirmationGrid.HAlign = 0.5f;
        ConfirmationGrid.VAlign = 0.5f;

        // As an alternative, the following code could be used:
        //      ConfirmationGrid.StyleLayoutChild().Grid().Set(row: 0, column: 0, rowSpan: int.MaxValue, columnSpan: int.MaxValue);
        //      ConfirmationGrid.StyleLayoutChild().Set(justifySelf: UIExAlignment.Center, alignSelf: UIExAlignment.Center);
        // This code specifies that ConfirmationGrid should occupy the entire MainGrid.
        // Since MainGrid stretches elements along all axes, we need to override this behavior and specify that ConfirmationGrid should be centered.

        ////////////////////////////
        ConfirmationGrid.StyleLayout.Set(
            justifyContent: UIExAlignment.Stretch,
            alignItems: UIExAlignment.Stretch);
        var style = ConfirmationGrid.StyleLayout.Grid();
        style.AddColumnDefinition(GL.FromFr(2), repeat: 2);
        style.RowsSpace.Precent = 0.02f;
        style.ColumnsSpace.Precent = 0.02f;
        ////////////////////////////
        ConfirmationGrid.StyleDisplay.tModLoaderStyle = true;
    }

    /// <summary/>
    /// <param name="clickEvent">Confirmation event (pressing OK)</param>
    protected void ShowConfirmation(MouseEvent clickEvent, string descriptions)
        => ShowConfirmation(clickEvent, descriptions.Split('\n'));
    /// <summary/>
    /// <param name="clickEvent">Confirmation event (pressing OK)</param>
    protected void ShowConfirmation(MouseEvent clickEvent, string[] descriptions)
    {
        ConfirmationGrid.RemoveAllChildren();
        ConfirmationGrid.Remove();
        UIExGrid grid = null;
        if (StateConfirmation)
            return;
        ///////////////////

        // If the mod configuration specifies automatic confirmation:
        // 1.Trigger the confirmation event.
        // 2. Hide the confirmation window.
        // 3. Reactivate the main menu.
        var config = ModContent.GetInstance<TalentSystemConfig>();
        if (config is not null && config.AutoConfirmation)
        {
            clickEvent.Invoke(null, null);
            HideConfirmation();
            OnActivate();
            return;
        }
        /////////////////////
        /////////////////////
        StateConfirmation = true;
        grid = ConfirmationGrid;
        ////////////////////////
        string headerText = UL.UI_Confirm_Header.Value;
        string okButtonText = UL.UI_Button_Ok.Value;
        string cancelButtonText = UL.UI_Button_Cancel.Value;
        Vector2 okButtonSize = GetTextSize(okButtonText);
        Vector2 cancelButtonSize = GetTextSize(cancelButtonText);
        float width = Math.Max(okButtonSize.X, cancelButtonSize.X);
        float height = Math.Max(okButtonSize.Y, cancelButtonSize.Y);
        CalculatedSize(headerText);
        CalculatedSize(descriptions);
        void CalculatedSize(params string[] texts)
        {
            foreach (var text in texts)
            {
                Vector2 size = GetTextSize(text);
                width = Math.Max(width, size.X);
                height += size.Y;
            }
        }
        //////////////////////
        var style = grid.StyleLayout.Grid();

        // Removal of all previous AddRowDefinitions that might have been created during a prior call to the confirmation window.
        style.RemoveRowDefinitionsRange(0, style.RowsCount);

        style.AddRowDefinitions(
            GL.FromFr(1.1f),
            GL.FromPx(8f));
        style.AddRowDefinition(GL.FromFr(1f), repeat: descriptions.Length);
        style.AddRowDefinitions(GL.FromPx(8f), GL.FromFr(1f));
        ////////////////////
        var uiHeader = UtilsUI.CreateWrapText<UIExElement>(headerText);
        UITextAutoScale[] uiDescriptions = new UITextAutoScale[descriptions.Length];
        for (int i = 0; i < descriptions.Length; i++)
            uiDescriptions[i] = UtilsUI.CreateUIText(descriptions[i]);
        var uiOkButton = UtilsUI.CreateWrapText<UIButtonFrame>(okButtonText);
        var uiCancelButton = UtilsUI.CreateWrapText<UIButtonFrame>(cancelButtonText);
        uiOkButton.StyleDisplay.tModLoaderStyle = true;
        uiCancelButton.StyleDisplay.tModLoaderStyle = true;
        ////////////////////
        int row = 0;
        AppendUIText(uiHeader);
        AddSeparator();
        foreach (var desc in uiDescriptions)
            AppendUIText(desc);
        AddSeparator();
        AppendUIText(uiOkButton);
        AppendUIText(uiCancelButton);
        uiOkButton.StyleLayoutChild().Grid().ColumnSpan = 1;
        uiCancelButton.StyleLayoutChild().Grid().Column = 1;
        void AppendUIText(UIElement element)
        {
            element.StyleLayoutChild().Grid().Set(row++, column: 0, columnSpan: 2);
            grid.Append(element);
        }
        void AddSeparator()
            => UtilsUI.AddSeparator(grid, orientation: UIExOrientation.Horizontal, row++, column: 0, columnSpan: 2);
        ////////////////////
        grid.Width.Pixels = width * 1.1f;
        grid.Height.Pixels = height * 2f;
        ////////////////////
        uiOkButton.OnLeftClick += (evt, el) => 
        { 
            clickEvent.Invoke(evt, el); 
            HideConfirmation();
            OnActivate();
        };
        uiCancelButton.OnLeftClick += (evt, el) => HideConfirmation();
        ////////////////////
        MainGrid.Append(grid);

        // The confirmation window has been prepared and added to MainGrid.
        // It is necessary to apply the theme to the confirmation window and trigger
        // a relayout to ensure the elements are correctly positioned by the layout managers.
        ThemeStyles.Instance.ApplyStylesToBranch(grid);
        UIExLayout.EndLayoutPreparation(this);
        UIExLayout.BeginLayoutPreparation(this);
    }
    protected void HideConfirmation()
    {
        if (ConfirmationGrid is not null)
            ConfirmationGrid.Remove();
        StateConfirmation = false;
    }
    protected void CreateHeaderText(string text)
    {
        if (HeaderText.Parent is not null)
            MainGrid.RemoveChild(HeaderText.Parent);
        /////////////////////////
        HeaderText.SetText(text);
        //UtilsUI.PrepareTextsLayout(HeaderText);
        var wrap = UIExLayout.Wrap<UIExElement>(HeaderText);
        wrap.IgnoresMouseInteraction = true;
        MainGrid.Append(wrap);
        wrap.StyleLayoutChild().Grid().Set(row: 0, column: 0, rowSpan: 1, columnSpan: int.MaxValue);
    }

    /// <summary>
    ///     Pop-up window with a description of the selected talent
    /// </summary>
    protected void InitializeTooltipGrid()
    {
        TooltipCanvas.Width = StyleDimension.Fill;
        TooltipCanvas.Height = StyleDimension.Fill;
        TooltipCanvas.IgnoresMouseInteraction = true;
        Append(TooltipCanvas);
        ///////////////////////
        TooltipGrid.StyleDisplay.tModLoaderStyle = true;
        TooltipGrid.StyleLayout.Set(
            orientation: UIExOrientation.Vertical,
            justifyContent: UIExAlignment.Stretch,
            alignItems: UIExAlignment.Stretch);
        TooltipGrid.StyleLayout.Grid().AddColumnDefinition(GL.FromFr(1f));
        TooltipGrid.IgnoresMouseInteraction = true;
    }
    protected void ShowTalentTooltip(string header, Talent.ActivationType type, string currentTooltips, string nextTooltips)
    {
        // If the current description builder has a parent, it means the talent description is already being displayed.
        if (TooltipGrid.Parent is not null)
            return;
        //////////////////////////////////
        string activationTypeDescription = type switch
        {
            Talent.ActivationType.Active => UL.UI_Talents_ActivationType_Active.Value,
            Talent.ActivationType.Switchable => UL.UI_Talents_ActivationType_Switchable.Value,
            _ => UL.UI_Talents_ActivationType_Passive.Value
        };
        ///////////////////////////////////
        string[] currentTooltipSplit = currentTooltips is null ? new string[0] : currentTooltips.Split('\n');
        string[] nextTooltipSplit = nextTooltips is null ? new string[0] : nextTooltips.Split('\n');
        float width = 0;
        float height = 0;
        string currentLevelHeader = currentTooltipSplit.Length > 0 ? UL.UI_Talents_CurrentLevel.Value : string.Empty;
        string nextLevelHeader = nextTooltipSplit.Length > 0 ? UL.UI_Talents_NextLevel.Value : string.Empty;
        CalculateNextTextSize(header, currentLevelHeader, nextLevelHeader);
        CalculateNextTextSize(activationTypeDescription);
        CalculateNextTextSize(currentTooltipSplit);
        CalculateNextTextSize(nextTooltipSplit);
        void CalculateNextTextSize(params string[] texts)
        {
            foreach (var text in texts)
            {
                Vector2 size = GetTextSize(text, 1f);
                width = Math.Max(width, size.X);
                height += size.Y;
            }
        }
        /////////////////////////////////////
        TooltipGrid.Width.Pixels = width;
        TooltipGrid.Height.Pixels = height;
        var styleGridTooltip = TooltipGrid.StyleLayout.Grid();
        styleGridTooltip.AddRowDefinitions(
            GL.FromFr(1.1f),
            GL.FromPx(8f),
            GL.FromFr(1.1f));
        AddRowDefinitions(currentTooltipSplit);
        AddRowDefinitions(nextTooltipSplit);
        void AddRowDefinitions(string[] split)
        {
            if (split.Length > 0)
            {
                styleGridTooltip.AddRowDefinition(GL.FromPx(8f));
                styleGridTooltip.AddRowDefinition(GL.FromFr(1f));
                styleGridTooltip.AddRowDefinition(GL.FromFr(0.6f), repeat: split.Length);
            }
        }
        /////////////////////////////////////
        var uiTextHeader = UtilsUI.CreateUIText(header, 1.2f);
        var uiTextActivationType = UtilsUI.CreateUIText(activationTypeDescription);
        var level = CreateLevelTexts(currentTooltipSplit, currentLevelHeader);
        var uiCurrentLevelHeader = level.header;
        var uiTextsCurrentTooltip = level.texts;
        level = CreateLevelTexts(nextTooltipSplit, nextLevelHeader);
        var uiNextLevelHeader = level.header;
        var uiTextsNextTooltip = level.texts;
        (UITextAutoScale header, UITextAutoScale[] texts) CreateLevelTexts(string[] texts, string leveHeaderText)
        {
            if (texts.Length < 1)
                return (null, null);
            UITextAutoScale uiLevelHeaderText = UtilsUI.CreateUIText(leveHeaderText, 1.05f);
            UITextAutoScale[] uiTexts = new UITextAutoScale[texts.Length];
            for (int i = 0; i < texts.Length; i++)
                uiTexts[i] = UtilsUI.CreateUIText(texts[i], 1f);
            return (uiLevelHeaderText, uiTexts);
        }
        /////////////////////////////////////
        uiTextHeader.StyleLayoutChild().Margin.SetPrecent(0.025f);
        uiTextActivationType.StyleLayoutChild().Margin.SetPrecent(0.025f);
        /////////////////////////////////////
        int row = 0;
        AddToGrid(uiTextHeader);
        AddSeparator();
        AddToGrid(uiTextActivationType);
        AppendLevel(uiCurrentLevelHeader, uiTextsCurrentTooltip);
        AppendLevel(uiNextLevelHeader, uiTextsNextTooltip);
        void AppendLevel(UITextAutoScale levelHeader, UITextAutoScale[] texts)
        {
            if (texts is null || texts.Length == 0)
                return;
            AddSeparator();
            AddToGrid(levelHeader);
            foreach (var text in texts)
                AddToGrid(text);
        }
        void AddSeparator() => UtilsUI.AddSeparator(TooltipGrid, UIExOrientation.Horizontal, row++, 0);
        void AddToGrid(UIElement element)
        {
            element.StyleLayoutChild().Grid().Row = row++;
            TooltipGrid.Append(element);
        }
        //////////////////////////////////////
        var styleChild = TooltipGrid.StyleLayoutChild();
        styleChild.Width.Pixels = width * 1.2f;
        styleChild.Height.Pixels = height * 1.5f;
        //////////////////////////////////////
        Vector2 mouse = Main.MouseScreen;
        var styleChildCanvas = TooltipGrid.StyleLayoutChild().Canvas();
        var parentDimensions = GetInnerDimensions();
        styleChildCanvas.Left.Pixels = mouse.X - (styleChild.Width.GetValue(parentDimensions.Width) / 2f);
        styleChildCanvas.Top.Pixels = mouse.Y - (styleChild.Height.GetValue(parentDimensions.Height) / 2f);
        ////////////////////////////////////
        ThemeStyles.Instance.ApplyStylesToBranch(TooltipGrid);
        TooltipCanvas.Append(TooltipGrid);
        UIExLayout.EndLayoutPreparation(this);
        UIExLayout.BeginLayoutPreparation(this);
    }

    protected void HideTalentTooltip()
    {
        var style = TooltipGrid.StyleLayout.Grid();
        style.RemoveRowDefinitionsRange(0, style.RowsCount);
        TooltipGrid.RemoveAllChildren();
        TooltipGrid.Remove();
    }

    /// <summary>
    ///     Plays a sound when the button is pressed. U
    ///     ses default values ​​for any null arguments provided. 
    ///     The sound selected depends on <see cref="StateConfirmation"/>
    /// </summary>
    protected void ClickPlaySound(SoundStyle? soundOpen, SoundStyle? soundClose)
    {
        if (soundOpen is null)
            soundOpen = SoundID.MenuTick;
        if(soundClose is null)
            soundClose = SoundID.Unlock;
        SoundEngine.PlaySound(StateConfirmation ? soundClose : soundOpen);
    }

    /// <summary>
    ///     Adds an OnLeftClick event to the element. 
    ///     Automates the check for whether a confirmation window is already open and handles sound playback.
    /// </summary>
    protected void AddLeftClickEvent(
        UIElement element, MouseEvent mouseEvent, 
        SoundStyle? soundOpen = null, SoundStyle? soundClose = null)
    {
        element.OnLeftClick += (evt, el) =>
        {
            ClickPlaySound(soundOpen, soundClose);
            if (StateConfirmation)
                return;
            mouseEvent.Invoke(evt, el);
        };
    }

    /// <summary>
    ///     Adds an OnLeftClick event to the element. 
    ///     It automates the check for whether a confirmation window is already open and handles sound playback. 
    ///     It also automates the display of the click confirmation window.
    /// </summary>
    protected void AddConfirmationLeftClickEvent(
        UIElement element, 
        MouseEvent mouseEvent,
        SoundStyle? soundOpen = null,
        SoundStyle? soundClose = null,
        params string[] texts)
    {
        element.OnLeftClick += (evt, el) =>
        {
            ClickPlaySound(soundOpen, soundClose);
            if (StateConfirmation)
                return;
            ShowConfirmation(mouseEvent, texts);
        };
    }

    protected Vector2 GetTextSize(UITextAutoScale text, float textScale = 1f)
        => GetTextSize(text.Text, textScale);
    protected Vector2 GetTextSize(string text, float textScale = 1f)
        => FontAssets.MouseText.Value.MeasureString(text) * textScale;
}