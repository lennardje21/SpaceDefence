using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence
{
    public class PauseScreen
    {
        private SpriteFont font;
        private Texture2D overlay;
        private GraphicsDevice graphicsDevice;
        private GameManager gameManager;

        public PauseScreen(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            gameManager = GameManager.GetGameManager();

            // Create a semi-transparent black texture
            overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 150) }); // 150 Alpha for transparency
        }

        public void Load(ContentManager content)
        {
            font = content.Load<SpriteFont>("GameFont");
        }

        public void Update()
        {
            if (gameManager.InputManager.IsKeyPress(Keys.P)) // Unpause
            {
                gameManager.SetGameState(GameState.Playing);
            }
            else if (gameManager.InputManager.IsKeyPress(Keys.Escape)) // Quit
            {
                System.Environment.Exit(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            // Draw the semi-transparent overlay
            spriteBatch.Draw(overlay, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);

            // Draw pause text
            string message = "Paused\nPress P to Continue\nPress ESC to Quit";
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = new Vector2(
                (graphicsDevice.Viewport.Width - textSize.X) / 2,
                (graphicsDevice.Viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, message, textPosition, Color.Yellow);
        }
    }

}
