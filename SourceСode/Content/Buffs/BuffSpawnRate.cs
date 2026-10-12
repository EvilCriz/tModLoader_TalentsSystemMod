using Terraria;
using Terraria.ModLoader;

namespace TalentsSystem.Content;

public class BuffSpawnRate : ModBuff
{
    public override string Texture => "TalentsSystem/Assets/Buffs/BuffSpawnRate";
    public override void SetStaticDefaults()
    {
        Main.buffNoSave[Type] = true;
        Main.debuff[Type] = false;
        Main.buffNoTimeDisplay[Type] = true;
    }
    public override bool RightClick(int buffIndex) => false;

    public override void Update(Player player, ref int buffIndex) => player.buffTime[buffIndex] = 60;
}
