using Terraria.UI;
using Terraria.ModLoader;
using Terraria.GameContent.UI.Elements;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BestFriendsFarm.Systems; // This using statement is crucial for accessing LLMSystem
using Terraria;
using System.Threading.Tasks; // Required for Task.Run and async/await
using System.Diagnostics;     // For Stopwatch (though not directly used in DogFeedUI, it's good practice to keep related usings)
using System.IO;              // For file operations (not directly used here, but common in UI/mod logic)
using System.Net.Http;
using System.Text;
using System.Text.Json;
using BestFriendsFarm.UI; // If not already present
using Terraria.ID;
using System.Collections.Generic;
using BestFriendsFarm.Animals;
using System;


namespace BestFriendsFarm.UI
{
    public class AnimalFeedUI : UIState
    {
        private UIPanel panel;
        private UIText titleText;
        private UITextPanel<string> feedButton;
        private UITextPanel<string> talkButton;
        private SimpleUITextBox inputTextBox;
        private UIImage animalImage;

        public int npcID = -1; // Stores the ID of the NPC this UI is currently associated with
        public int animalType = -1; // Stores the NPC type (dog, cat, etc.)

        // Animal-specific UI text and LLM prompt
        private static readonly Dictionary<int, (string Name, string FeedPrompt, string NoFoodText, string LLMSystemPrompt)> AnimalInfo = new()
        {
            [ModContent.NPCType<Dog>()] = (
                "dog",
                "Feed your dog?",
                "You don't have any bones!",
                "You are a loving dog but not an AI. Respond like a dog talking in a cute and dummy way to a human (under one complete sentence)."
            ),
            [ModContent.NPCType<Cat>()] = (
                "cat",
                "Feed your cat?",
                "You don't have any fish!",
                "You are a loving cat but not an AI. Respond like a cat talking in a cute, aloof, and sassy way to a human (under one complete sentence)."
            ),
            [ModContent.NPCType<Cow>()] = (
                "cow",
                "Feed your cow?",
                "You don't have any hay!",
                "You are a gentle cow but not an AI. Respond like a cow talking to a human, always mention something about grass or mooing (under two complete sentence)."
            )
        };

        public override void OnInitialize()
        {
            // Initialize the main UI panel
            panel = new UIPanel();
            panel.SetPadding(10);
            panel.Left.Set(400f, 0f); // Position from left of the screen
            panel.Top.Set(200f, 0f);  // Position from top of the screen
            panel.Width.Set(280f, 0f); // Increased width for comfort
            panel.Height.Set(160f, 0f); // Increased height for comfort
            Append(panel); // Add the panel to this UIState

            // Initialize the title text
            titleText = new UIText("Spend time with your animal!");
            titleText.Top.Set(10f, 0f);
            titleText.Left.Set(20f, 0f);
            panel.Append(titleText); // Add text to the panel

            // Initialize the Feed Button (text-based)
            feedButton = new UITextPanel<string>("Feed");
            feedButton.Left.Set(40f, 0f);
            feedButton.Top.Set(40f, 0f);
            feedButton.Width.Set(60f, 0f);
            feedButton.Height.Set(30f, 0f);
            feedButton.OnLeftClick += OnFeedClick; // Assign click event handler
            panel.Append(feedButton); // Add button to the panel

            // Initialize the Talk Button (text-based)
            talkButton = new UITextPanel<string>("Talk");
            talkButton.Left.Set(120f, 0f);
            talkButton.Top.Set(40f, 0f);
            talkButton.Width.Set(60f, 0f);
            talkButton.Height.Set(30f, 0f);
            talkButton.OnLeftClick += OnTalkClick; // Assign click event handler
            panel.Append(talkButton); // Add button to the panel

            // Add a text input field
            inputTextBox = new SimpleUITextBox("Type your message...");
            inputTextBox.Top.Set(80f, 0f);
            inputTextBox.Left.Set(10f, 0f);
            inputTextBox.Width.Set(200f, 0f);
            panel.Append(inputTextBox);

            // (Animal image code remains commented out for now)
        }

        // Event handler for the Feed button click
        private void OnFeedClick(UIMouseEvent evt, UIElement listeningElement)
        {
            Player player = Main.LocalPlayer;
            var modPlayer = player.GetModPlayer<BondingPlayer>();
            bool fed = false;
            string noFoodText = "You don't have the right food!";
            if (AnimalInfo.TryGetValue(animalType, out var info))
                noFoodText = info.NoFoodText;

            if (animalType == ModContent.NPCType<Dog>())
            {
                int boneType = ItemID.Bone;
                if (player.HasItem(boneType))
                {
                    for (int i = 0; i < player.inventory.Length; i++)
                    {
                        if (player.inventory[i].type == boneType && player.inventory[i].stack > 0)
                        {
                            player.inventory[i].stack--;
                            fed = true;
                            break;
                        }
                    }
                }
            }
            else if (animalType == ModContent.NPCType<Cat>())
            {
                // Use the same fish list as in Cat.cs
                HashSet<int> fishIDs = new()
                {
                    ItemID.Bass, ItemID.Trout, ItemID.Salmon, ItemID.Tuna, ItemID.RedSnapper, ItemID.NeonTetra,
                    ItemID.ArmoredCavefish, ItemID.SpecularFish, ItemID.Prismite, ItemID.VariegatedLardfish,
                    ItemID.FlarefinKoi, ItemID.Obsidifish, ItemID.DoubleCod, ItemID.FrostMinnow, ItemID.Hemopiranha,
                    ItemID.Ebonkoi, ItemID.CrimsonTigerfish
                };
                for (int i = 0; i < player.inventory.Length; i++)
                {
                    if (fishIDs.Contains(player.inventory[i].type) && player.inventory[i].stack > 0)
                    {
                        player.inventory[i].stack--;
                        fed = true;
                        break;
                    }
                }
            }
            else if (animalType == ModContent.NPCType<Cow>())
            {
                int hayType = ItemID.Hay;
                if (player.HasItem(hayType))
                {
                    for (int i = 0; i < player.inventory.Length; i++)
                    {
                        if (player.inventory[i].type == hayType && player.inventory[i].stack > 0)
                        {
                            player.inventory[i].stack--;
                            fed = true;
                            break;
                        }
                    }
                }
            }
            // Add more animal types here as needed

            if (fed)
            {
                // Determine if favorite food
                bool isFavoriteFood = false;
                if (animalType == ModContent.NPCType<Dog>())
                    isFavoriteFood = true; // Only one food for dog
                else if (animalType == ModContent.NPCType<Cat>())
                    isFavoriteFood = true; // All fish are favorite for cat
                else if (animalType == ModContent.NPCType<Cow>())
                    isFavoriteFood = true; // Only hay for cow
                // Calculate bonding increase
                float bondingIncrease = modPlayer.CalculateBondingIncrease(animalType, npcID, isFavoriteFood);
                if (!modPlayer.bondLevels.ContainsKey(animalType))
                    modPlayer.bondLevels[animalType] = new Dictionary<int, float>();
                if (!modPlayer.bondLevels[animalType].ContainsKey(npcID))
                    modPlayer.bondLevels[animalType][npcID] = 0;
                modPlayer.bondLevels[animalType][npcID] = Math.Min(BondingPlayer.MaxBonding, modPlayer.bondLevels[animalType][npcID] + bondingIncrease);
                modPlayer.UpdateFeedStreak(animalType, npcID);
                modPlayer.SetOwned(animalType, npcID); // Ensure animal is set as owned when fed
                CombatText.NewText(Main.npc[npcID].Hitbox, Color.LightPink, $"+{bondingIncrease:F1} Bond!");
                Main.NewText($"✅ You fed your {AnimalInfo.GetValueOrDefault(animalType).Name ?? "animal"} and grew closer 💖");
            }
            else
            {
                Main.NewText(noFoodText, Color.Red);
            }
            UISystem.HideUI();
        }

        // Event handler for the Talk button click
        private void OnTalkClick(UIMouseEvent evt, UIElement listeningElement)
        {
            inputTextBox.Unfocus();
            Main.chatText = ""; // Clear the built-in chat box after sending
            string playerMessage = inputTextBox.Text;
            if (string.IsNullOrWhiteSpace(playerMessage))
            {
                Main.NewText("Please enter a message to talk to your animal!", Color.Red);
                return;
            }
            Main.NewText($"You: {playerMessage}", Color.LightCyan);
            Main.NewText("[Talking to animal... please wait a moment]", Color.LightBlue);
            string systemPrompt = AnimalInfo.TryGetValue(animalType, out var info) ? info.LLMSystemPrompt : "You are a loving animal.";
            Task.Run(async () =>
            {
                try
                {
                    string reply = await LLMHttpClient.GetLlamaReplyAsync(playerMessage, systemPrompt);
                    if (string.IsNullOrEmpty(reply))
                        reply = "** ... (no response or processing took too long)";
                    Main.QueueMainThreadAction(() => {
                        Main.NewText($"** {AnimalInfo.GetValueOrDefault(animalType).Name ?? "Animal"} says: ** {reply}", Color.LightGoldenrodYellow);
                    });
                }
                catch (System.Exception ex)
                {
                    Main.QueueMainThreadAction(() => {
                        Main.NewText($"[Error] Error during LLM conversation: {ex.Message}", Color.Red);
                    });
                }
            });
            UISystem.HideUI();
        }

        // Method to set the target NPC for this UI
        public void SetTarget(int npcWhoAmI, int npcType)
        {
            npcID = npcWhoAmI;
            animalType = npcType;
            if (AnimalInfo.TryGetValue(animalType, out var info))
                titleText.SetText($"Spend time with your {info.Name}!");
            else
                titleText.SetText("Spend time with your animal!");
            // (Animal image code remains commented out for now)
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // Allow closing the UI with Esc key
            if (Main.keyState.IsKeyDown(Microsoft.Xna.Framework.Input.Keys.Escape))
            {
                UISystem.HideUI();
            }
        }
    }

    // Place this outside the DogFeedUI class, but inside the namespace
    public static class LLMHttpClient
    {
        public static async Task<string> GetLlamaReplyAsync(string userInput, string systemPrompt)
        {
            var prompt = $"### System:\n{systemPrompt}\n### User:\n" + userInput.Replace("\n", "\\n") + "\n### Assistant:";
            var body = new { prompt = prompt };
            using var client = new HttpClient();
            var content = new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json");
            var response = await client.PostAsync(LLMSystem.LLMServerUrl + "/completion", content);
            var responseString = await response.Content.ReadAsStringAsync();
            //Main.NewText($"[LLM Raw] {responseString}", Color.Gray);

            // using var doc = JsonDocument.Parse(responseString);
            // if (doc.RootElement.TryGetProperty("content", out var contentProp))
            // {
            //     string llamaOutput = contentProp.GetString();
            //     string output = llamaOutput;
            //     int idx = output.LastIndexOf("### Assistant:");
            //     if (idx >= 0)
            //         output = output.Substring(idx + "### Assistant:".Length).Trim();
            //     return output;
            // }
            using var doc = JsonDocument.Parse(responseString);
            if (doc.RootElement.TryGetProperty("content", out var contentProp))
            {
                string rawContent = contentProp.GetString();
                //Main.NewText($"[LLM Raw Content] {rawContent}", Color.Gray);

                string reply = rawContent;

               
                // Combine all headers that should trigger a cut
                string[] cutHeaders = {
                    "#INPUT", "##INPUT", "##INSTRUCTION", "#OUTPUT", "##OUTPUT",
                    "##OUTPUT", "#OUTPUT", "### Assistant:", "Assistant:",
                    "##OUTPUT", "#OUTPUT", "### User:", "User:", "#","AI:","="
                };

                int cutIdx = -1;
                foreach (var header in cutHeaders)
                {
                    int idx = reply.IndexOf(header);
                    if (idx >= 0 && (cutIdx == -1 || idx < cutIdx))
                    {
                        cutIdx = idx;
                    }
                }
                if (cutIdx >= 0)
                {
                    reply = reply.Substring(0, cutIdx).Trim();
                }

                return reply;
            }
            return "(No response)";
        }
    }
}
