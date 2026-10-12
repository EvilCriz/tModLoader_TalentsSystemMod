using System.ComponentModel;
using Terraria.ModLoader.Config;

namespace TalentsSystem;

public class TalentSystemConfig : ModConfig
{
    public override ConfigScope Mode => ConfigScope.ClientSide;

    [Header("MainMenuHeader")]

    [DefaultValue(false)]
    public bool AutoConfirmation;

    [DefaultValue(true)]
    public bool CloseMenuByEsc;
}