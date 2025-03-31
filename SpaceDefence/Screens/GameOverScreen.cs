using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence.Screens
{
    public class GameOverScreen
    {
        private SpriteFont font;
        private GameManager gameManager;
        private GraphicsDevice graphicsDevice;

        public GameOverScreen(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            gameManager = GameManager.GetGameManager();
        }

        public void Load(ContentManager content)
        {
            font = content.Load<SpriteFont>("GameOverFont");
        }

        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.R)) // Restart Game
            {
                gameManager.Restart();
            }
            else if (keyboardState.IsKeyDown(Keys.Escape)) // Quit Game
            {
                System.Environment.Exit(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            string gameOverText = "Game Over!\nPress R to Restart\nPress ESC to Quit";
            string[] lines = gameOverText.Split('\n');

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

                spriteBatch.DrawString(font, line, position, Color.Red);
            }
        }
    }
}
