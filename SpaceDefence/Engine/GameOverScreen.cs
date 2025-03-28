﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence
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
            Vector2 textSize = font.MeasureString(gameOverText);
            Vector2 textPosition = new Vector2(
                (graphicsDevice.Viewport.Width - textSize.X) / 2,
                (graphicsDevice.Viewport.Height - textSize.Y) / 2
            );

            spriteBatch.DrawString(font, gameOverText, textPosition, Color.Red);
        }


        private void RestartGame()
        {
            gameManager.Restart();
        }
    }
}
