using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceDefence.Engine;

namespace SpaceDefence
{
    public enum GameState
    {
        StartScreen,
        Playing,
        Paused,
        ExplosionPlaying,
        GameOver
    }

    public class GameManager
    {
        private static GameManager gameManager;

        private List<GameObject> _gameObjects;
        private List<GameObject> _toBeRemoved;
        private List<GameObject> _toBeAdded;
        private ContentManager _content;

        private GameOverScreen _gameOverScreen;
        private StartScreen _startScreen;
        private PauseScreen _pauseScreen;
        private Camera _camera;

        public static int _gameFieldWidth = 4000;
        public static int _gameFieldHeight = 4000;


        public Random RNG { get; private set; }
        public Ship Player { get; private set; }
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }
        private GameState currentState = GameState.StartScreen;
        public static Rectangle LevelBounds = new Rectangle(0, 0, _gameFieldWidth, _gameFieldHeight);



        public void SetPlayer(Ship player)
        {
            Player = player;
        }

        public void SetGameState(GameState newState)
        {
            currentState = newState;
        }

        public static GameManager GetGameManager()
        {
            if(gameManager == null)
                gameManager = new GameManager();
            return gameManager;
        }
        public GameManager()
        {
            _gameObjects = new List<GameObject>();
            _toBeRemoved = new List<GameObject>();
            _toBeAdded = new List<GameObject>();
            InputManager = new InputManager();
            RNG = new Random();
        }

        public void Initialize(ContentManager content, Game game, Ship player)
        {
            Game = game;
            _content = content;
            SetPlayer(player); // Use the method to set the player

            _gameOverScreen = new GameOverScreen(Game.GraphicsDevice);
            _gameOverScreen.Load(content);
            _startScreen = new StartScreen(Game.GraphicsDevice);
            _startScreen.Load(content);
            _pauseScreen = new PauseScreen(Game.GraphicsDevice);
            _pauseScreen.Load(content);
            _camera = new Camera(Game.GraphicsDevice.Viewport);
        }

        public void Load(ContentManager content)
        {
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load(content);
            }
        }

        public void HandleInput(InputManager inputManager)
        {
            if (inputManager.IsKeyPress(Keys.P))
            {
                if (currentState == GameState.Playing)
                    SetGameState(GameState.Paused);
                else if (currentState == GameState.Paused)
                    SetGameState(GameState.Playing);
            }
            // **Disable player input when dead**
            if (currentState != GameState.Playing)
            {
                return; // Prevents movement, shooting, or any input
            }
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.HandleInput(this.InputManager);
            }
        }


        public void CheckCollision()
        {
            // Checks once for every pair of 2 GameObjects if they collide.
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                for (int j = i + 1; j < _gameObjects.Count; j++)
                {
                    if (_gameObjects[i].CheckCollision(_gameObjects[j]))
                    {
                        _gameObjects[i].OnCollision(_gameObjects[j]);
                        _gameObjects[j].OnCollision(_gameObjects[i]);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            InputManager.Update(); // Ensure input updates first

            switch (currentState)
            {
                case GameState.StartScreen:
                    _startScreen.Update();
                    break;
                case GameState.Playing:
                    HandleInput(InputManager);

                    foreach (GameObject gameObject in _gameObjects)
                    {
                        // Prevent the player from moving if they are dead
                        if (gameObject is Ship playerShip && playerShip.IsDead())
                            continue;

                        gameObject.Update(gameTime);
                    }

                    CheckCollision();

                    foreach (GameObject gameObject in _toBeAdded)
                    {
                        gameObject.Load(_content);
                        _gameObjects.Add(gameObject);
                    }
                    _toBeAdded.Clear();

                    foreach (GameObject gameObject in _toBeRemoved)
                    {
                        gameObject.Destroy();
                        _gameObjects.Remove(gameObject);
                    }
                    _toBeRemoved.Clear();
                    break;
                case GameState.Paused:
                    _pauseScreen.Update(); // Ensure pause menu responds to input
                    break;
                case GameState.GameOver:
                    _gameOverScreen.Update(); // Ensure game over screen responds to input
                    break;
            }
        }


        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _camera.Follow(Player.GetPosition().Center.ToVector2());

            spriteBatch.Begin(transformMatrix: _camera.GetTransform());

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End(); // End world draw

            spriteBatch.Begin();

            if (currentState == GameState.Paused)
            {
                _pauseScreen.Draw(spriteBatch);
            }

            if (currentState == GameState.GameOver)
            {
                _gameOverScreen.Draw(spriteBatch);
            }

            if (currentState == GameState.StartScreen)
            {
                _startScreen.Draw(spriteBatch);
            }

            spriteBatch.End(); // End UI draw
        }


        /// <summary>
        /// Add a new GameObject to the GameManager. 
        /// The GameObject will be added at the start of the next Update step. 
        /// Once it is added, the GameManager will ensure all steps of the game loop will be called on the object automatically. 
        /// </summary>
        /// <param name="gameObject"> The GameObject to add. </param>
        public void AddGameObject(GameObject gameObject)
        {
            _toBeAdded.Add(gameObject);
        }

        /// <summary>
        /// Remove GameObject from the GameManager. 
        /// The GameObject will be removed at the start of the next Update step and its Destroy() mehtod will be called.
        /// After that the object will no longer receive any updates.
        /// </summary>
        /// <param name="gameObject"> The GameObject to Remove. </param>
        public void RemoveGameObject(GameObject gameObject)
        {
            _toBeRemoved.Add(gameObject);
        }

        private bool playerExplosionTriggered = false;

        public void GameOver()
        {
            if (!playerExplosionTriggered)
            {
                playerExplosionTriggered = true;
                Console.WriteLine("Player destroyed! Triggering explosion...");

                // Stop player movement and input
                Player.Kill();

                // Create explosion at player's position
                Vector2 playerPosition = Player.GetPosition().Center.ToVector2();
                AddGameObject(new Explosion(playerPosition, 4f));

                // Delay switching to the Game Over screen so explosion plays first
                Task.Delay(1000).ContinueWith(t =>
                {
                    SetGameState(GameState.GameOver);
                    playerExplosionTriggered = false; // Reset flag for next game
                });
            }
        }

        public void Restart()
        {
            Console.WriteLine("Restarting Game...");

            // Reset game state
            _gameObjects.Clear();
            _toBeAdded.Clear();
            _toBeRemoved.Clear();

            // Reinitialize player
            Player = new Ship(new Point(Game.GraphicsDevice.Viewport.Width / 2, Game.GraphicsDevice.Viewport.Height / 2));
            AddGameObject(Player);

            // Spawn an initial alien
            AddGameObject(new Alien(Player, 50f));

            AddGameObject(new Supply());

            currentState = GameState.Playing;
        }


        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, Game.GraphicsDevice.Viewport.Width),
                RNG.Next(0, Game.GraphicsDevice.Viewport.Height));
        }

    }
}
