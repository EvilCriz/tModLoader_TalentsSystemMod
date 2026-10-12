using Terraria;
using Terraria.ModLoader;

namespace TalentsSystem.Content;
public class BuffOutOfCombat : ModBuff
{
    public override string Texture => "TalentsSystem/Assets/Buffs/BuffOutOfCombat";
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = false;
        Main.buffNoTimeDisplay[Type] = false;
    }
    public override bool RightClick(int buffIndex) => false;

    public override void Update(Player player, ref int buffIndex) => player.buffTime[buffIndex] += 2;
}