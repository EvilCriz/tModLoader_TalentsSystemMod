using Terraria.Localization;
using Terraria.ModLoader;

namespace TalentsSystem;

/// <summary>
///     A direct equivalent of <see cref="DamageClass.Magic"/>. Used as a "Warlock" class marker.
/// </summary>
public class WarlockDamageClass : DamageClass
{
    public override LocalizedText DisplayName => Language.GetText("LegacyTooltip.4");
    public override StatInheritanceData GetModifierInheritance(DamageClass damageClass)
        => damageClass == Generic || damageClass == Magic 
        ? StatInheritanceData.Full
        : StatInheritanceData.None;
    public override bool GetEffectInheritance(DamageClass damageClass) 
        => damageClass == Magic;
}