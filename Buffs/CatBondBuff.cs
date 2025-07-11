using Terraria;
using Terraria.ModLoader;

namespace BestFriendsFarm.Buffs
{
    public class CatBondBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
