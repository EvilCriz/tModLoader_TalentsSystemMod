using Terraria;
using Terraria.ModLoader;

namespace TalentsSystem.Content;

public class BuffManaHealingCooldown : ModBuff
{
    public override string Texture => "TalentsSystem/Assets/Buffs/BuffManaHealingCooldown";
    private int _lastMana = -1;
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = true;
        Main.buffNoTimeDisplay[Type] = false;
    }
    public override bool RightClick(int buffIndex) => false;

    public override void Update(Player player, ref int buffIndex)
    {
        var modif = player.GetModPlayer<TalentsApplyModPlayer>().Modifiers;
        if (modif is null || !modif.ManaHealignModifier.IsReady)
        {
            player.ClearBuff(Type);
            return;
        }
        if (_lastMana == -1)
        {
            _lastMana = player.statMana;
        }
        int dur1 = modif.ManaHealignModifier.DebuffDurationInSec * 60;
        int dur2 = modif.ManaHealignModifier.ManaRegenerationDisabledDurationInSec * 60;
        if (player.buffTime[buffIndex] >= dur1 - dur2)
        {
            int diff = player.statMana - _lastMana;
            if (diff > 0 && diff <= 5)
            {
                player.statMana = _lastMana;
            }
            else
            {
                _lastMana = player.statMana;
            }
        }
    }
}
