using Terraria;
using Terraria.ID;
using Terraria.ModLoader;


namespace BestFriendsFarm.Systems
{
    public class DevTestSystem : ModSystem
    {
        public override void PostUpdatePlayers()
        {
            Player player = Main.LocalPlayer;
            // Only run in-game, not on menu
            if (Main.gameMenu) return;

            // Helper to count items in slots 11+
            int CountItemInRest(Player p, int itemType)
            {
                int count = 0;
                for (int i = 11; i < p.inventory.Length; i++)
                {
                    if (p.inventory[i].type == itemType)
                        count += p.inventory[i].stack;
                }
                return count;
            }

            // Helper to add items to slots 11+
            void AddItemToRest(Player p, int itemType, int amount)
            {
                for (int i = 11; i < p.inventory.Length && amount > 0; i++)
                {
                    if (p.inventory[i].type == 0 || p.inventory[i].type == itemType && p.inventory[i].stack < p.inventory[i].maxStack)
                    {
                        int add = amount;
                        if (p.inventory[i].type == itemType)
                            add = System.Math.Min(amount, p.inventory[i].maxStack - p.inventory[i].stack);
                        else
                            add = System.Math.Min(amount, 9999); // default maxStack
                        if (add > 0)
                        {
                            if (p.inventory[i].type == 0)
                            {
                                p.inventory[i].SetDefaults(itemType);
                                p.inventory[i].stack = add;
                            }
                            else
                            {
                                p.inventory[i].stack += add;
                            }
                            amount -= add;
                        }
                    }
                }
            }

            // Give 100 Bones, Hay, and Bass (fish) in slots 11+
            int[] itemTypes = { ItemID.Bone, ItemID.Hay, ItemID.Bass };
            string[] itemNames = { "Bones", "Hay", "Bass" };
            for (int idx = 0; idx < itemTypes.Length; idx++)
            {
                int type = itemTypes[idx];
                int have = CountItemInRest(player, type);
                if (have < 100)
                {
                    AddItemToRest(player, type, 100 - have);
                    // Main.NewText($"[DevTestSystem] Gave you {100 - have} {itemNames[idx]} in slots 11+ for testing.", 200, 255, 100);
                }
            }
        }
    }
}
