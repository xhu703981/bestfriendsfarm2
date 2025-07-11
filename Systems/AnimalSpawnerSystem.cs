using Terraria;
using Terraria.ModLoader;
using Terraria.ModLoader.IO;
using Microsoft.Xna.Framework;
using BestFriendsFarm.Animals;
using Terraria.ID;
using System.Collections.Generic;
using System;

namespace BestFriendsFarm.Systems
{
    public class AnimalSpawnerSystem : ModSystem
    {
        // Store animal types and their spawn points
        private readonly List<int> animalTypes = new List<int> {
            ModContent.NPCType<Dog>(),
            ModContent.NPCType<Cat>(),
            ModContent.NPCType<Cow>()
        };
        private readonly Dictionary<int, Vector2> animalSpawnPoints = new();
        private bool spawnPointsInitialized = false;
        private int worldAnimalSpawnSeed = 0;

        public override void OnWorldLoad()
        {
            spawnPointsInitialized = false;
            animalSpawnPoints.Clear();
            worldAnimalSpawnSeed = 0;
        }

        public override void LoadWorldData(TagCompound tag)
        {
            animalSpawnPoints.Clear();
            spawnPointsInitialized = false; // <-- Add this line
            worldAnimalSpawnSeed = tag.ContainsKey("animal_spawn_seed") ? tag.GetInt("animal_spawn_seed") : 0;
            foreach (int type in animalTypes)
            {
                string xKey = $"animal_spawn_x_{type}";
                string yKey = $"animal_spawn_y_{type}";
                if (tag.ContainsKey(xKey) && tag.ContainsKey(yKey))
                {
                    float x = tag.GetFloat(xKey);
                    float y = tag.GetFloat(yKey);
                    animalSpawnPoints[type] = new Vector2(x, y);
                }
            }
        }

        public override void SaveWorldData(TagCompound tag)
        {
            tag["animal_spawn_seed"] = worldAnimalSpawnSeed;
            foreach (var kvp in animalSpawnPoints)
            {
                tag[$"animal_spawn_x_{kvp.Key}"] = kvp.Value.X;
                tag[$"animal_spawn_y_{kvp.Key}"] = kvp.Value.Y;
            }
        }

        private bool AnimalAlreadyExists(int npcType)
        {
            foreach (NPC npc in Main.npc)
            {
                if (npc.active && npc.type == npcType)
                    return true;
            }
            return false;
        }

        private void EnsureWorldSeed()
        {
            if (worldAnimalSpawnSeed == 0)
            {
                worldAnimalSpawnSeed = Guid.NewGuid().GetHashCode();
            }
        }

        public override void PostUpdateWorld()
        {
            if (Main.gameMenu)
                return;

            EnsureWorldSeed();

            // Lazy init: if spawn points are not set, choose and remember one for each animal
            if (!spawnPointsInitialized)
            {
                var rand = new System.Random(worldAnimalSpawnSeed);
                foreach (int type in animalTypes)
                {
                    if (!animalSpawnPoints.ContainsKey(type))
                        animalSpawnPoints[type] = FindSurfaceSpawnPoint(rand);
                }
                spawnPointsInitialized = true;
            }

            foreach (int type in animalTypes)
            {
                if (!AnimalAlreadyExists(type))
                {
                    Vector2 spawn = animalSpawnPoints[type];
                    NPC.NewNPC(null, (int)spawn.X, (int)spawn.Y, type);
                    string name = type == ModContent.NPCType<Dog>() ? "dog" : type == ModContent.NPCType<Cat>() ? "cat" : type == ModContent.NPCType<Cow>() ? "cow" : "animal";
                    string emoji = type == ModContent.NPCType<Dog>() ? "🐾" : type == ModContent.NPCType<Cat>() ? "🐾" : type == ModContent.NPCType<Cow>() ? "🐄" : "";
                    // Main.NewText($"A curious {name} has appeared {emoji}", 255, 255, 150);
                }
            }
        }

        private Vector2 FindSurfaceSpawnPoint(System.Random rand)
        {
            for (int attempt = 0; attempt < 500; attempt++)
            {
                int x = rand.Next(100, Main.maxTilesX - 100);
                int y = 100;

                // Walk down until solid tile is found
                while (y < Main.worldSurface && !WorldGen.SolidTile(x, y))
                    y++;

                if (y >= Main.maxTilesY - 200) continue;

                // Check for at least 5 air tiles above the ground, no water, no walls
                bool valid = true;
                for (int i = 1; i <= 5; i++)
                {
                    Tile above = Framing.GetTileSafely(x, y - i);
                    if (above.HasTile || above.LiquidAmount > 0 || above.WallType != 0)
                    {
                        valid = false;
                        break;
                    }
                }
                if (!valid) continue;

                // Also check the tile itself is not in water
                Tile ground = Framing.GetTileSafely(x, y);
                if (ground.LiquidAmount > 0) continue;

                int finalX = x;
                int finalY = y - 1; // Stand on top of the ground
                return new Vector2(finalX * 16, finalY * 16);
            }

            // Fallback: world spawn
            return new Vector2(Main.spawnTileX * 16f, (Main.spawnTileY - 1) * 16f);
        }

        // Example: Check for 8 flat solid tiles before spawning the cow
        private bool IsFlatGround(int startX, int y, int width = 8)
        {
            for (int i = 0; i < width; i++)
            {
                Tile tile = Framing.GetTileSafely(startX + i, y);
                if (!tile.HasTile || !Main.tileSolid[tile.TileType])
                    return false;
                // Optionally, check that the tile above is empty (so cow doesn't spawn inside a block)
                Tile above = Framing.GetTileSafely(startX + i, y - 1);
                if (above.HasTile)
                    return false;
            }
            return true;
        }
    }
}
