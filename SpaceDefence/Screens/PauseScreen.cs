using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence.Screens
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

            overlay = new Texture2D(graphicsDevice, 1, 1);
            overlay.SetData(new[] { new Color(0, 0, 0, 150) }); // 150 for transparency
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
            spriteBatch.Draw(overlay, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);

            string pauseText = "Paused\nPress P to Continue\nPress ESC to Quit";
            string[] lines = pauseText.Split('\n');

            float lineHeight = font.LineSpacing;
            float totalHeight = lineHeight * lines.Length;

            float startY = (graphicsDevice.Viewport.Height - totalHeight) / 2;

            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                Vector2 lineSize = font.MeasureString(line);

                Vector2 position = new Vector2(
                    (graphicsDevice.Viewport.Width - lineSize.X) / 2, // center horizontally
                    startY + i * lineHeight // stack vertically
                );

                spriteBatch.DrawString(font, line, position, Color.Yellow);
            }
        }
    }

}
