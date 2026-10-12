//using Terraria.UI;
//using UIeXtension;

//namespace TalentsSystem.Other;
//internal class ModIconSpriteCreatorSystem : UIExModSystem<ModIconSpriteCreatorState> { }
//internal class ModIconSpriteCreatorState : UIState
//{
//    public override void OnActivate()
//    {
//        if (TalentsSystem.IsPlayerNotReady())
//            return;
//        //////////////////////
//        RemoveAllChildren();
//        UIExLayout.BeginLayoutPreparation(this);
//        UIExUniformGrid grid = new();
//        Append(grid);
//        ///////////////////
//        float sizePx = 80f, sizeSpacePx = 4f, sizePadding = 4f;
//        grid.Width.Pixels = sizePx;
//        grid.Height.Pixels = sizePx;
//        grid.HAlign = 0.5f;
//        grid.VAlign = 0.3f;
//        grid.StyleLayout.Set(
//            justifyContent: UIeXtension.Enums.UIExAlignment.Stretch,
//            alignItems: UIeXtension.Enums.UIExAlignment.Stretch);
//        var style = grid.StyleLayout.UniformGrid();
//        style.Set(
//            rowsSpace: StyleDimension.FromPixels(sizeSpacePx),
//            columnSpace: StyleDimension.FromPixels(sizeSpacePx));
//        grid.StyleDisplay.tModLoaderStyle = true;
//        grid.StyleDisplay.PaddingAutoControl = false;
//        grid.SetPadding(sizePadding);
//        //////////////////
//        AppendButton("⚔");
//        AppendButton("➶");
//        AppendButton("✦");
//        AppendButton("☄");
//        void AppendButton(string text)
//        {
//            var wrap = Utils.UtilsUI.CreateWrapTextInFrame(text);
//            var style = wrap.StyleDisplay;
//            style.tModLoaderStyle = true;
//            style.PaddingAutoControl = false;
//            grid.Append(wrap);
//        }
//        //////////////////
//        ThemeStyles.Instance.ApplyStylesToBranch(this);
//        UIExLayout.EndLayoutPreparation(this);
//        UIExLayout.BeginLayoutPreparation(this);
//    }
//}