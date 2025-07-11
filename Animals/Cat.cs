using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BestFriendsFarm.Systems;
using System;
using System.Collections.Generic;

namespace BestFriendsFarm.Animals
{
    public class Cat : ModNPC
    {
        private Vector2 spawnOrigin;
        private bool initialized = false;

        private static readonly HashSet<int> FishItemIDs = new()
        {
            ItemID.Bass,
            ItemID.Trout,
            ItemID.Salmon,
            ItemID.Tuna,
            ItemID.RedSnapper,
            ItemID.NeonTetra,
            ItemID.ArmoredCavefish,
            ItemID.SpecularFish,
            ItemID.Prismite,
            ItemID.VariegatedLardfish,
            ItemID.FlarefinKoi,
            ItemID.Obsidifish,
            ItemID.DoubleCod,
            ItemID.FrostMinnow,
            ItemID.Hemopiranha,
            ItemID.Ebonkoi,
            ItemID.CrimsonTigerfish,
             // Remove if you don't want crates
            // ...add more fish as desired
        };

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 7;
        }

        public override void SetDefaults()
        {
            NPC.width = 72;
            NPC.height = 54;
            NPC.friendly = true;
            NPC.lifeMax = 100;
            NPC.dontTakeDamage = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;

            // Use Guide-like movement (handles slopes, cliffs)
            NPC.aiStyle = 7;
            AIType = NPCID.Guide;
            // DO NOT set AnimationType so we use our own FindFrame
            DrawOffsetY = 10; // Increase this value until the feet touch the ground
        }

        public override void AI()
        {
            Player player = Main.player[NPC.target];

            if (!initialized)
            {
                spawnOrigin = NPC.Center;
                initialized = true;
            }

            var modPlayer = player.GetModPlayer<BondingPlayer>();

            HandleInteraction(player, modPlayer);

            // ✅ Flip sprite to match movement
            if (NPC.velocity.X != 0)
                NPC.spriteDirection = NPC.direction;
        }

        public override void FindFrame(int frameHeight)
        {
            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                NPC.frameCounter += 1.0;

                if (NPC.frameCounter >= 9.0) // Adjusted threshold for slower walk
                {
                    NPC.frame.Y += frameHeight;
                    NPC.frameCounter = 0.0;
                }

                if (NPC.frame.Y >= frameHeight * Main.npcFrameCount[NPC.type])
                {
                    NPC.frame.Y = 0;
                }
            }
            else
            {
                NPC.frame.Y = frameHeight * 3; // idle frame
                NPC.frameCounter = 0;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // Remove Untamed text
        }

        private void HandleInteraction(Player player, BondingPlayer modPlayer)
        {
            if (Main.player.IndexInRange(Main.myPlayer))
            {
                Player local = Main.player[Main.myPlayer];
                if (local.Hitbox.Intersects(NPC.Hitbox) && Main.mouseRight && Main.mouseRightRelease)
                {
                    UISystem.ShowFeedUI(NPC.whoAmI, NPC.type);
                }
            }
        }
    }
}
