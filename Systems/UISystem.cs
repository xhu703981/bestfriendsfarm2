using Terraria.ModLoader;
using Terraria.UI;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using Terraria;
using BestFriendsFarm.UI;
using BestFriendsFarm.Systems;

public class UISystem : ModSystem
{
    internal AnimalFeedUI animalFeedUI;
    private UserInterface animalFeedInterface;

    public override void Load()
    {
        animalFeedUI = new AnimalFeedUI();
        animalFeedUI.Activate();
        animalFeedInterface = new UserInterface();
    }

    public override void UpdateUI(GameTime gameTime)
    {
        // Capture chat input for our custom text box
        var focusedBox = SimpleUITextBox.GetFocusedBox();
        if (focusedBox != null)
        {
            if (Main.drawingPlayerChat)
            {
                // Sync text every frame while chat is open
                focusedBox.Text = Main.chatText;
            }
            else if (Main.hasFocus)
            {
                // When chat closes, finalize text and unfocus
                focusedBox.Text = Main.chatText;
                Main.chatText = "";
                focusedBox.Unfocus();
                Main.blockInput = false;
                Main.chatRelease = false;
            }
        }
        if (animalFeedInterface?.CurrentState != null)
        {
            animalFeedInterface.Update(gameTime);
        }
    }

    public override void ModifyInterfaceLayers(List<GameInterfaceLayer> layers)
    {
        int inventoryLayerIndex = layers.FindIndex(layer => layer.Name.Equals("Vanilla: Inventory"));
        if (inventoryLayerIndex != -1)
        {
            layers.Insert(inventoryLayerIndex, new LegacyGameInterfaceLayer(
                "BestFriendsFarm: Animal Feed UI",
                delegate
                {
                    if (animalFeedInterface?.CurrentState != null)
                    {
                        animalFeedInterface.Draw(Main.spriteBatch, new GameTime());
                    }
                    return true;
                },
                InterfaceScaleType.UI));
        }
    }

    public static void ShowFeedUI(int npcWhoAmI, int npcType)
    {
        var uiSystem = ModContent.GetInstance<UISystem>();

        if (uiSystem.animalFeedUI != null)
        {
            // Main.NewText($"📦 Showing Animal Feed UI for NPC #{npcWhoAmI}");
            uiSystem.animalFeedUI.SetTarget(npcWhoAmI, npcType);
            uiSystem.animalFeedInterface?.SetState(uiSystem.animalFeedUI);
        }
        else
        {
            // Main.NewText("⚠️ animalFeedUI was null!");
        }
    }

    public static void HideUI()
    {
        ModContent.GetInstance<UISystem>().animalFeedInterface?.SetState(null);
    }
}
