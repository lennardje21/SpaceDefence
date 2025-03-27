using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence.Engine
{
    public class StartScreen
    {
        private SpriteFont font;
        private Texture2D bg;
        private GraphicsDevice graphicsDevice;
        private GameManager gameManager;

        public StartScreen(GraphicsDevice graphicsDevice)
        {
            this.graphicsDevice = graphicsDevice;
            gameManager = GameManager.GetGameManager();
        }

        public void Load(ContentManager content)
        {
            font = content.Load<SpriteFont>("GameFont"); // Ensure a font is loaded
            bg = content.Load<Texture2D>("space-defence-bg");
        }

        public void Update()
        {
            KeyboardState keyboardState = Keyboard.GetState();

            if (keyboardState.IsKeyDown(Keys.Enter)) // Start the game
            {
                gameManager.SetGameState(GameState.Playing);
            }
            else if (keyboardState.IsKeyDown(Keys.Escape)) // Quit
            {
                System.Environment.Exit(0);
            }
        }

        public void Draw(SpriteBatch spriteBatch)
        {
            graphicsDevice.Clear(Color.Black);
            spriteBatch.Draw(bg, new Rectangle(0, 0, graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height), Color.White);
            string message = "Press ENTER to Start\nPress ESC to Quit";
            Vector2 textSize = font.MeasureString(message);
            Vector2 textPosition = new Vector2(
                (graphicsDevice.Viewport.Width - textSize.X) / 2,
                (graphicsDevice.Viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, message, textPosition, Color.White);
        }
    }
}
