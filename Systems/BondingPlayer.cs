using Terraria;
using Terraria.ModLoader;
using Terraria.DataStructures;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Terraria.ModLoader.IO;
using System;

namespace BestFriendsFarm.Systems
{
    public class BondingPlayer : ModPlayer
    {
        // animalType -> set of owned NPC IDs
        public Dictionary<int, HashSet<int>> ownedAnimals = new();
        // animalType -> (npcID -> bondLevel)
        public Dictionary<int, Dictionary<int, float>> bondLevels = new();
        // animalType -> (npcID -> last fed time)
        public Dictionary<int, Dictionary<int, double>> lastFedTime = new();
        // animalType -> (npcID -> streak count)
        public Dictionary<int, Dictionary<int, int>> feedStreaks = new();
        // Buff thresholds
        public static readonly float[] BuffThresholds = { 20f, 50f, 80f };
        public static readonly int GracePeriodDays = 2;
        public static readonly float MaxBonding = 100f;
        public static readonly float DecayRate = 5f;
        public static readonly float BaseBonding = 10f;
        public static readonly float StreakBonusPerDay = 2f;
        public static readonly float MaxStreakBonus = 10f;
        public static readonly float FavoriteFoodBonus = 5f;
        private const float Threshold = 10f;

        public bool IsOwned(int animalType, int npcID)
        {
            return ownedAnimals.ContainsKey(animalType) && ownedAnimals[animalType].Contains(npcID);
        }

        public void SetOwned(int animalType, int npcID)
        {
            if (!ownedAnimals.ContainsKey(animalType))
                ownedAnimals[animalType] = new HashSet<int>();
            ownedAnimals[animalType].Add(npcID);
        }

        public float GetBondLevel(int animalType, int npcID)
        {
            if (bondLevels.ContainsKey(animalType) && bondLevels[animalType].ContainsKey(npcID))
                return bondLevels[animalType][npcID];
            return 0f;
        }

        public void AddBond(int animalType, int npcID, float amount)
        {
            if (!bondLevels.ContainsKey(animalType))
                bondLevels[animalType] = new Dictionary<int, float>();
            if (!bondLevels[animalType].ContainsKey(npcID))
                bondLevels[animalType][npcID] = 0f;
            bondLevels[animalType][npcID] += amount;
        }

        public bool IsBondedWith(int animalType)
        {
            if (!ownedAnimals.ContainsKey(animalType)) return false;
            foreach (int npcID in ownedAnimals[animalType])
            {
                // Check if the animal is active in the world
                NPC npc = Main.npc[npcID];
                if (npc != null && npc.active && npc.type == animalType)
                {
                    float bond = GetBondLevel(animalType, npcID);
                    if (bond >= Threshold)
                        return true;
                }
            }
            return false;
        }

        // Returns the current buff tier (0 = none, 1 = friendly, 2 = trusted, 3 = best friend)
        public int GetBuffTier(int animalType, int npcID)
        {
            float bond = GetBondLevel(animalType, npcID);
            if (bond >= BuffThresholds[2]) return 3;
            if (bond >= BuffThresholds[1]) return 2;
            if (bond >= BuffThresholds[0]) return 1;
            return 0;
        }

        // Call this when feeding an animal
        public float CalculateBondingIncrease(int animalType, int npcID, bool isFavoriteFood)
        {
            float currentBond = GetBondLevel(animalType, npcID);
            float streak = 0f;
            if (feedStreaks.ContainsKey(animalType) && feedStreaks[animalType].ContainsKey(npcID))
                streak = feedStreaks[animalType][npcID];
            float streakBonus = Math.Min(streak * StreakBonusPerDay, MaxStreakBonus);
            float favoriteBonus = isFavoriteFood ? FavoriteFoodBonus : 0f;
            float increase = BaseBonding * (1f - (currentBond / MaxBonding)) + streakBonus + favoriteBonus;
            return increase;
        }

        // Call this after feeding to update streak and last fed time
        public void UpdateFeedStreak(int animalType, int npcID)
        {
            double now = Main.time;
            if (!lastFedTime.ContainsKey(animalType)) lastFedTime[animalType] = new();
            if (!feedStreaks.ContainsKey(animalType)) feedStreaks[animalType] = new();
            if (lastFedTime[animalType].ContainsKey(npcID))
            {
                double last = lastFedTime[animalType][npcID];
                // If fed within 1 day, increment streak
                if (now - last < 86400) // 1 day in seconds
                    feedStreaks[animalType][npcID]++;
                else
                    feedStreaks[animalType][npcID] = 1;
            }
            else
            {
                feedStreaks[animalType][npcID] = 1;
            }
            lastFedTime[animalType][npcID] = now;
        }

        // Decay bonding over time, called in PostUpdate
        private void DecayBonding()
        {
            double now = Main.time;
            foreach (var animalPair in bondLevels)
            {
                int animalType = animalPair.Key;
                foreach (var npcPair in animalPair.Value)
                {
                    int npcID = npcPair.Key;
                    float bond = npcPair.Value;
                    double lastFed = 0;
                    if (lastFedTime.ContainsKey(animalType) && lastFedTime[animalType].ContainsKey(npcID))
                        lastFed = lastFedTime[animalType][npcID];
                    // If within grace period, skip decay
                    if (now - lastFed < GracePeriodDays * 86400) continue;
                    float decay = DecayRate * (bond / MaxBonding);
                    bondLevels[animalType][npcID] = Math.Max(0f, bond - decay / 60f); // decay per minute
                }
            }
        }

        public override void PostUpdate()
        {
            DecayBonding();
            // Removed automatic bond increase from proximity
            // Bonding should only increase when feeding via UI
        }

        public override void SaveData(TagCompound tag)
        {
            // Save bond levels
            List<int> animalTypes = new();
            List<List<int>> npcIdLists = new();
            List<List<float>> bondValueLists = new();
            foreach (var animalPair in bondLevels)
            {
                animalTypes.Add(animalPair.Key);
                var npcIds = new List<int>();
                var bondVals = new List<float>();
                foreach (var pair in animalPair.Value)
                {
                    npcIds.Add(pair.Key);
                    bondVals.Add(pair.Value);
                }
                npcIdLists.Add(npcIds);
                bondValueLists.Add(bondVals);
            }
            tag["bond_animalTypes"] = animalTypes;
            tag["bond_npcIdLists"] = npcIdLists;
            tag["bond_valueLists"] = bondValueLists;

            // Save owned animals
            List<int> ownedAnimalTypes = new();
            List<List<int>> ownedNpcIdLists = new();
            foreach (var animalPair in ownedAnimals)
            {
                ownedAnimalTypes.Add(animalPair.Key);
                ownedNpcIdLists.Add(new List<int>(animalPair.Value));
            }
            tag["owned_animalTypes"] = ownedAnimalTypes;
            tag["owned_npcIdLists"] = ownedNpcIdLists;
            //Main.NewText($"[DEBUG] SaveData: ownedAnimals={ownedAnimals.Count}, bondLevels={bondLevels.Count}");
        }

        public override void LoadData(TagCompound tag)
        {
            bondLevels.Clear();
            ownedAnimals.Clear();
            // Load bond levels
            if (tag.ContainsKey("bond_animalTypes") && tag.ContainsKey("bond_npcIdLists") && tag.ContainsKey("bond_valueLists"))
            {
                var animalTypes = tag.Get<List<int>>("bond_animalTypes");
                var npcIdLists = tag.Get<List<List<int>>>("bond_npcIdLists");
                var bondValueLists = tag.Get<List<List<float>>>("bond_valueLists");
                for (int i = 0; i < animalTypes.Count; i++)
                {
                    int animalType = animalTypes[i];
                    var npcIds = npcIdLists[i];
                    var bondVals = bondValueLists[i];
                    bondLevels[animalType] = new Dictionary<int, float>();
                    for (int j = 0; j < npcIds.Count && j < bondVals.Count; j++)
                    {
                        bondLevels[animalType][npcIds[j]] = bondVals[j];
                    }
                }
            }
            // Load owned animals
            if (tag.ContainsKey("owned_animalTypes") && tag.ContainsKey("owned_npcIdLists"))
            {
                var ownedAnimalTypes = tag.Get<List<int>>("owned_animalTypes");
                var ownedNpcIdLists = tag.Get<List<List<int>>>("owned_npcIdLists");
                for (int i = 0; i < ownedAnimalTypes.Count; i++)
                {
                    int animalType = ownedAnimalTypes[i];
                    ownedAnimals[animalType] = new HashSet<int>(ownedNpcIdLists[i]);
                }
            }
            //Main.NewText($"[DEBUG] LoadData: ownedAnimals={ownedAnimals.Count}, bondLevels={bondLevels.Count}");
        }

    }
    


}
