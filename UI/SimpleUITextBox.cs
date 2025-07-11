using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Terraria;
using Terraria.GameContent.UI.Elements;
using Terraria.UI;
using Terraria.ID;
using Terraria.ModLoader;
using Terraria.GameContent;
using ReLogic.Graphics;

namespace BestFriendsFarm.UI
{
    public class SimpleUITextBox : UIElement
    {
        public string Text { get; set; } = "";
        private string hint;
        private bool focused = false;
        private static SimpleUITextBox focusedBox = null;

        public SimpleUITextBox(string hint = "")
        {
            this.hint = hint;
            Width.Set(180f, 0f);
            Height.Set(30f, 0f);
            SetPadding(6f);
        }

        public override void LeftClick(UIMouseEvent evt)
        {
            base.LeftClick(evt);
            focusedBox = this;
            focused = true;
            Main.clrInput();
            Main.drawingPlayerChat = true;
            Main.chatText = this.Text;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            // No longer handle chatText here; handled in ModSystem
        }

        public override void Draw(SpriteBatch spriteBatch)
        {
            base.Draw(spriteBatch);
            var boxRect = GetDimensions().ToRectangle();
            spriteBatch.Draw(TextureAssets.MagicPixel.Value, boxRect, Color.Black * 0.7f);

            string drawText = string.IsNullOrEmpty(Text) && !focused ? hint : Text;
            Color textColor = string.IsNullOrEmpty(Text) && !focused ? Color.Gray : Color.White;
            Utils.DrawBorderString(spriteBatch, drawText, boxRect.TopLeft() + new Vector2(4, 6), textColor);

            if (focused && (Main.GameUpdateCount / 20) % 2 == 0)
            {
                var size = FontAssets.MouseText.Value.MeasureString(drawText);
                spriteBatch.DrawString(
                    FontAssets.MouseText.Value,
                    "|",
                    boxRect.TopLeft() + new Vector2(4 + size.X, 6),
                    textColor
                );
            }
        }

        public void Unfocus()
        {
            focused = false;
            if (focusedBox == this) focusedBox = null;
        }

        public static SimpleUITextBox GetFocusedBox() => focusedBox;
    }
}
