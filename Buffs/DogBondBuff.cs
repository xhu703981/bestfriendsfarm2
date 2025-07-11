using Terraria;
using Terraria.ModLoader;

namespace BestFriendsFarm.Buffs
{
    public class DogBondBuff : ModBuff
    {
        public override void SetStaticDefaults()
        {
            Main.buffNoSave[Type] = true;
            Main.buffNoTimeDisplay[Type] = false;
        }
    }
}
