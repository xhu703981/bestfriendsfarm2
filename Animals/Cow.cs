using Terraria;
using Terraria.ID;
using Terraria.ModLoader;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BestFriendsFarm.Systems;
using System;

namespace BestFriendsFarm.Animals
{
    public class Cow : ModNPC
    {
        private Vector2 spawnOrigin;
        private bool initialized = false;

        public override void SetStaticDefaults()
        {
            Main.npcFrameCount[NPC.type] = 5;
        }

        public override void SetDefaults()
        {
            NPC.width = 130;
            NPC.height = 105;
            NPC.friendly = true;
            NPC.lifeMax = 120;
            NPC.dontTakeDamage = true;
            NPC.noGravity = false;
            NPC.noTileCollide = false;
            NPC.aiStyle = 7;
            AIType = NPCID.Guide;
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
            if (NPC.velocity.X != 0)
                NPC.spriteDirection = NPC.direction;
        }

        public override void FindFrame(int frameHeight)
        {
            if (Math.Abs(NPC.velocity.X) > 0.1f)
            {
                NPC.frameCounter += 1.0;
                if (NPC.frameCounter >= 9.0)
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
                NPC.frame.Y = frameHeight * 3;
                NPC.frameCounter = 0;
            }
        }

        public override void PostDraw(SpriteBatch spriteBatch, Vector2 screenPos, Color drawColor)
        {
            // No extra drawing for now
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