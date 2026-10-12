using Microsoft.CodeAnalysis.Recommendations;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using TalentsSystem.Utils;
using Terraria;
using Terraria.GameContent;
using Terraria.ModLoader;
using Terraria.UI;
using UIeXtension;
using UIeXtension.MethodsExtensions;
using UIeXtension.Styles;
using static System.Runtime.InteropServices.JavaScript.JSType;
using LT = Terraria.Localization.LocalizedText;
using TS = TalentsSystem.TalentsSystem;
using UL = TalentsSystem.Utils.UtilsLocalization;

namespace TalentsSystem;

// This document contains minimal commentary.
// It also avoids repeating explanations already provided in the comments regarding the main menu.
// It is recommended to begin exploring this mod's UI with the UI/Menu.cs file.

public class UINotificationSystem : UIExModSystem<UINotificationState>
{
    protected int Counter = 0;
    protected Dictionary<int, int> NotificationTimeTable = new();
    protected int Delay = 0;
    public override void OnWorldUnload()
    {
        base.OnWorldUnload();
        ClearNotifications();
    }
    public override void UpdateUI(GameTime gameTime)
    {
        base.UpdateUI(gameTime);
        if (NotificationTimeTable is null)
            return;
        ////////////////////
        Delay += gameTime.ElapsedGameTime.Milliseconds;
        if (Delay >= 250)
        {
            List<int> Removes = new();
            foreach (var key in NotificationTimeTable.Keys)
            {
                NotificationTimeTable[key] -= Delay;
                if (NotificationTimeTable[key] <= 0)
                    Removes.Add(key);
            }
            Delay = 0;
            ////////////////////
            foreach (var key in Removes)
            {
                State.RemoveNotification(key);
                NotificationTimeTable.Remove(key);
            }
            if (NotificationTimeTable.Count == 0)
                ClearNotifications();
        }
    }
    public void ClearNotifications()
    {
        Hide();
        NotificationTimeTable = null;
        Counter = 0;
    }

    protected static void AddNotification(Action<int, UINotificationState> action)
    {
        if (TS.GetLocal<QuestSaveInfo>() is null)
            return;
        var system = ModContent.GetInstance<UINotificationSystem>();
        if (system is null)
            return;
        ////////////////////
        if (system.NotificationTimeTable is null)
            system.NotificationTimeTable = new();
        system.NotificationTimeTable[system.Counter] = 5000;
        system.Show();
        action.Invoke(system.Counter, system.State);
        system.Counter++;
    }
    public static void AddQuestCompleteNotification(int questId)
        => AddNotification((notifId, state) => state.CreateQuestNotification(notifId, questId));
    public static void AddLevelUpNotification()
        => AddNotification((notifId, state) => state.CreateLevelUpNotification(notifId));
}

// This document contains minimal commentary.
// It also avoids repeating explanations already provided in the comments regarding the main menu.
// It is recommended to begin exploring this mod's UI with the UI/Menu.cs file.

public class UINotificationState : UIState
{
    protected class NotificationInfo
    {
        public UIExGrid NotificationGrid;
        public StyleLayoutChildCanvas StyleCanvas;
    }
    ////////////////////
    protected float AnimationStartPos;
    protected float AnimationEndPos;
    protected float AnimationStepPx = 1f;
    protected int AnimationSpeedMs = 100;
    protected int AnimationCurrentMs = 0;
    protected int AnimationNotifId = -1;
    ////////////////////
    protected UIExCanvas MainCanvas;
    protected Dictionary<int, NotificationInfo> NotificationsInfoTable;


    // This document contains minimal commentary.
    // It also avoids repeating explanations already provided in the comments regarding the main menu.
    // It is recommended to begin exploring this mod's UI with the UI/Menu.cs file.

    public override void OnInitialize()
    {
        UIExLayout.BeginLayoutPreparation(this);
        MainCanvas = new();
        NotificationsInfoTable = new();
        InitializeMainCanvas();
    }
    public override void OnActivate()
    {
        if (TS.IsPlayerNotReady())
            return;
        ////////////////////
        AnimationStepPx = 15;
        AnimationSpeedMs = 1;
        ThemeStyles.Instance.ApplyStylesToBranch(this);
    }
    public override void OnDeactivate()
    {
        if (TS.IsPlayerNotReady())
            return;
        NotificationsInfoTable.Clear();
        MainCanvas.RemoveAllChildren();
    }

    protected void InitializeMainCanvas()
    {
        Append(MainCanvas);
        MainCanvas.Width = StyleDimension.Fill;
        MainCanvas.Height = StyleDimension.Fill;
        MainCanvas.StyleLayout.Set(
            orientation: UIeXtension.Enums.UIExOrientation.Vertical,
            justifyContent: UIeXtension.Enums.UIExAlignment.End,
            alignItems: UIeXtension.Enums.UIExAlignment.Center);
        MainCanvas.StyleLayout.Canvas().AllowOverflow = true;
    }

    protected void CreateNotification(int notifId, LT headerText, UIElement bodyElement)
    {
        if (MainCanvas is null)
            return;

        // Notification layout component.
        // Used to arrange the notification's title, separator, and main information.
        UIExGrid grid = new UIExGrid();
        MainCanvas.Append(grid);
        ///////////////////
        var styleGrid = grid.StyleLayout.Grid();
        var styleLayoutChild = grid.StyleLayoutChild();
        grid.StyleDisplay.tModLoaderStyle = true;
        styleLayoutChild.Width.Pixels = 800f;
        styleLayoutChild.Height.Pixels = 130f;
        grid.StyleLayout.Set(
            justifyContent: UIeXtension.Enums.UIExAlignment.Stretch,
            alignItems: UIeXtension.Enums.UIExAlignment.Stretch);
        styleGrid.AddColumnDefinition(UIExGridLength.FromFr(1f));
        styleGrid.AddRowDefinitions(
            UIExGridLength.FromPx(30f),
            UIExGridLength.FromPx(7f),
            UIExGridLength.FromFr(1f));
        ///////////////////
        var wrapHeaderText = UtilsUI.CreateWrapText<UIExElement>(headerText);
        grid.Append(wrapHeaderText);
        ///////////////////
        UtilsUI.AddSeparator(grid, UIeXtension.Enums.UIExOrientation.Horizontal, row: 1, column: 1);
        ///////////////////
        var style = bodyElement.StyleLayoutChild();
        style.Grid().Row = 3;
        style.Margin.Top.Precent = 0.05f;
        grid.Append(bodyElement);
        ///////////////////

        // Indicates that the Grid must perform a forced layout of its elements so that the final sizes of the child elements are known at this stage.
        // The size of the Grid itself will depend on this.
        UIExLayout.EndLayoutPreparation(this);
        
        /////////////////////////////////

        var inner = MainCanvas.GetInnerDimensions();
        float gridHeight = grid.Height.GetValue(inner.Height);
        styleLayoutChild.Canvas().Top.Pixels = gridHeight;
        ///////////////////
        AnimationStartPos = 0f;
        AnimationEndPos = grid.GetOuterDimensions().Height * 1.25f;
        AnimationNotifId = notifId;
        ///////////////////
        int counter = 0;
        foreach (var key in NotificationsInfoTable.Keys)
        {
            var notif = NotificationsInfoTable[key];
            notif.StyleCanvas.Bottom.Pixels = (gridHeight + 20f) * ++counter;
        }

        ///////////////////

        ThemeStyles.Instance.ApplyStylesToBranch(grid);
        UIExLayout.EndLayoutPreparation(this);
        UIExLayout.BeginLayoutPreparation(this);
        ///////////////////
        NotificationsInfoTable[notifId] = new();
        NotificationsInfoTable[notifId].NotificationGrid = grid;
        NotificationsInfoTable[notifId].StyleCanvas = styleLayoutChild.Canvas();
    }
    public void CreateQuestNotification(int notifId, int questId)
    {
        if (QuestsManager.Instance is null)
            return;
        ///////////////////
        Quest quest = QuestsManager.Instance.QuestsTable[questId];
        UIExStackPanel questStackPanel = UtilsUI.GetQuestInfoStackPanel(
            quest, 
            showSync: false,
            tModLoaderStyle: false);
        CreateNotification(notifId, UL.Notification_Quest_Header, questStackPanel);
    }
    public void CreateLevelUpNotification(int notifId)
    {
        var save = TS.GetLocal<QuestSaveInfo>();
        if (save is null)
            return;
        if (Main.instance is null)
            return;
        ///////////////////
        float margin = 0.025f;
        UIExStackPanel levelUpStackPanel = new();
        levelUpStackPanel.StyleLayout.Set(
            orientation: UIeXtension.Enums.UIExOrientation.Horizontal,
            justifyContent: UIeXtension.Enums.UIExAlignment.Start,
            alignItems: UIeXtension.Enums.UIExAlignment.Center);

        ///////////////////
        Texture2D textureIcon = TextureAssets.Sun.Value;
        Texture2D backTextureIcon = AssetsManager.Instance.GetAssetUI(AssetsManager.UITextureID.LevelUpFrame).Value;
        void AddIcon(bool left)
        {
            var icon = UtilsUI.CreateFrameIcon(textureIcon, backTextureIcon, iconSecond: true);
            var style = icon.StyleLayoutChild();
            if (left) style.Margin.Left.Precent = margin;
            else style.Margin.Right.Precent = margin;
            levelUpStackPanel.Append(icon);
        }
        ///////////////////
        AddIcon(left: true);
        ////////////////////
        var wrapTextBody = UtilsUI.CreateWrapText<UIExElement>(UL.Notification_LevelUp_Body.WithFormatArgs(save.Level).Value);
        var style = wrapTextBody.StyleLayoutChild();
        style.JustifySelf = UIeXtension.Enums.UIExAlignment.Stretch;
        style.AlignSelf = UIeXtension.Enums.UIExAlignment.Stretch;
        style.Margin.SetPrecent(0.04f);
        wrapTextBody.StyleDisplay.tModLoaderStyle = true;
        levelUpStackPanel.Append(wrapTextBody);
        ////////////////////
        AddIcon(left: false);

        ////////////////////

        CreateNotification(notifId, UL.Notification_LevelUp_Header, levelUpStackPanel);
    }
    public void RemoveNotification(int notifId)
    {
        if (!NotificationsInfoTable.ContainsKey(notifId))
            return;
        ////////////////////
        var grid = NotificationsInfoTable[notifId].NotificationGrid;
        MainCanvas.RemoveChild(grid);
        NotificationsInfoTable.Remove(notifId);
        if (NotificationsInfoTable.Count == 0)
            AnimationNotifId = -1;
    }
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (NotificationsInfoTable is null || NotificationsInfoTable.Count == 0)
            return;
        if (AnimationNotifId == -1)
            return;
        ////////////////////
        AnimationCurrentMs += gameTime.ElapsedGameTime.Milliseconds;
        if (AnimationCurrentMs >= AnimationSpeedMs)
        {
            AnimationCurrentMs = 0;
            AnimationStartPos += AnimationStepPx;
            if (AnimationStartPos >= AnimationEndPos)
            {
                AnimationNotifId = -1;
                AnimationStartPos = AnimationEndPos;
            }
            ////////////////////
            foreach (var notif in NotificationsInfoTable.Values)
                notif.StyleCanvas.Bottom.Pixels += AnimationStepPx;

            ////////////////////
            
            UIExLayout.EndLayoutPreparation(this);
            UIExLayout.BeginLayoutPreparation(this);
        }
    }
}
