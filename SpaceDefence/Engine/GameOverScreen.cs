using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence
{
    public class GameOverScreen
    {
        private SpriteFont font;
        private bool isGameOver;
        private GameManager gameManager;
        private GraphicsDevice graphicsDevice;

        public GameOverScreen(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            gameManager = GameManager.GetGameManager();
            isGameOver = false;
        }

        public void Load(ContentManager content)
        {
            font = content.Load<SpriteFont>("GameOverFont"); // Make sure you have a SpriteFont named GameOverFont
        }

        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.R)) // Restart Game
            {
                RestartGame();
            }
            else if (keyboardState.IsKeyDown(Keys.Escape)) // Quit Game
            {
                System.Environment.Exit(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();

            string gameOverText = "Game Over!\nPress R to Restart\nPress ESC to Quit";
            Vector2 textSize = font.MeasureString(gameOverText);
            Vector2 textPosition = new Vector2(
                (graphicsDevice.Viewport.Width - textSize.X) / 2,
                (graphicsDevice.Viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, gameOverText, textPosition, Color.Red);

            spriteBatch.End();
        }

        private void RestartGame()
        {
            gameManager.Restart();
        }
    }
}
