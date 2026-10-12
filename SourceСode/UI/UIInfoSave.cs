using Terraria.ModLoader;
using Terraria.ModLoader.IO;

namespace TalentsSystem;

[Autoload(Side = ModSide.Client)]
public class UIInfoSave : ModPlayer
{
    public enum QuestDisplayFlagType
    { 
        All,
        Completed,
        Incompleted
    }
    private ThemeStyles.UIThemeType _menuThemeType = ThemeStyles.UIThemeType.Dark;
    public ThemeStyles.UIThemeType MenuThemeType
    {
        get => _menuThemeType;
        set 
        {
            _menuThemeType = value;
            ThemeStyles.Instance.SwitchTheme(value);
        }
    }
    public QuestDisplayFlagType QuestDisplayFlag = 0;
    public override void SaveData(TagCompound tag)
    {
        tag[nameof(MenuThemeType)] = (byte)MenuThemeType;
        tag[nameof(QuestDisplayFlag)] = (byte)QuestDisplayFlag;
    }
    public override void LoadData(TagCompound tag)
    {
        MenuThemeType = (ThemeStyles.UIThemeType)tag.GetByte(nameof(MenuThemeType));
        QuestDisplayFlag = (QuestDisplayFlagType)tag.GetByte(nameof(QuestDisplayFlag));
        ThemeStyles.Instance.SwitchTheme(MenuThemeType);
    }
}
