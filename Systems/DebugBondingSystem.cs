// using Terraria;
// using Terraria.ID;
// using Terraria.ModLoader;
// using Microsoft.Xna.Framework.Input;
// using BestFriendsFarm.Systems;

// namespace BestFriendsFarm.Systems
// {
//     public class DebugBondingSystem : ModSystem
//     {
//         public override void PostUpdateInput()
//         {
//             Player player = Main.LocalPlayer;
//             var modPlayer = player.GetModPlayer<BondingPlayer>();

//             // Increase bonding with B
//             if (Main.keyState.IsKeyDown(Keys.B))
//             {
//                 foreach (NPC npc in Main.npc)
//                 {
//                     if (npc.active && npc.type == ModContent.NPCType<Animals.Dog>())
//                     {
//                         if (!modPlayer.bondLevels.ContainsKey(npc.whoAmI))
//                             modPlayer.bondLevels[npc.whoAmI] = 0f;

//                         modPlayer.bondLevels[npc.whoAmI] += 1f;
//                         // Main.NewText($"Bonding raised to: {modPlayer.bondLevels[npc.whoAmI]}");
//                         break;
//                     }
//                 }
//             }

//             // Decrease bonding with N
//             if (Main.keyState.IsKeyDown(Keys.N))
//             {
//                 foreach (NPC npc in Main.npc)
//                 {
//                     if (npc.active && npc.type == ModContent.NPCType<Animals.Dog>())
//                     {
//                         if (!modPlayer.bondLevels.ContainsKey(npc.whoAmI))
//                             modPlayer.bondLevels[npc.whoAmI] = 0f;

//                         modPlayer.bondLevels[npc.whoAmI] -= 1f;
//                         if (modPlayer.bondLevels[npc.whoAmI] < 0)
//                             modPlayer.bondLevels[npc.whoAmI] = 0f;

//                         // Main.NewText($"Bonding lowered to: {modPlayer.bondLevels[npc.whoAmI]}");
//                         break;
//                     }
//                 }
//             }
//         }
//     }
// }
