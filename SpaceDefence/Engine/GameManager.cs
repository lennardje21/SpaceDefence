using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceDefence.Engine;
using SpaceDefence.Screens;

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

        // size of the playingfield
        public static int _gameFieldWidth = 6000;
        public static int _gameFieldHeight = 6000;

        // background settings
        private Texture2D _starsTexture;
        private Texture2D _galaxyTexture;
        private Vector2 _galaxyPosition;
        private const int _backgroundTileSize = 768;

        // alien spawn settings
        private float _enemySpawnTimer = 0f;
        private float _spawnInterval = 10f; // Seconds
        private float _spawnSpeed = 50f;
        private int _maxAliens = 20;

        // asteroid spawn settings
        private float _asteroidSpawnTimer = 0f;
        private float _nextAsteroidSpawnTime = 10f;
        private Random rng = new();

        // score tracker
        private int score = 0;
        public int Score => score;
        private Texture2D hudArrow;

        public Random RNG { get; private set; }
        public Ship Player { get; private set; }
        public InputManager InputManager { get; private set; }
        public Game Game { get; private set; }

        private GameState _currentState = GameState.StartScreen;

        public static Rectangle _levelBounds = new Rectangle(0, 0, _gameFieldWidth, _gameFieldHeight);

        public Camera GetCamera()
        {
            return _camera;
        }

        public void SetPlayer(Ship player)
        {
            Player = player;
        }

        public void SetGameState(GameState newState)
        {
            _currentState = newState;
        }

        public void increaseScore(int points)
        {
            score += points;
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
            SetPlayer(player);

            _gameOverScreen = new GameOverScreen(Game.GraphicsDevice);
            _gameOverScreen.Load(content);
            _startScreen = new StartScreen(Game.GraphicsDevice);
            _startScreen.Load(content);
            _pauseScreen = new PauseScreen(Game.GraphicsDevice);
            _pauseScreen.Load(content);
            _camera = new Camera(Game.GraphicsDevice.Viewport);

            for (int i = 0; i < 5; i++)
            {
                Vector2 location;
                do
                {
                    location = RandomScreenLocation();
                }
                while (Vector2.Distance(location, Player.GetPosition().Center.ToVector2()) < 200); // min distance

                AddGameObject(new Asteroid(location));
            }

            AddGameObject(new Planet(new Vector2(500, RNG.Next(0, _gameFieldHeight)), "Earth", true));
            AddGameObject(new Planet(new Vector2(_gameFieldWidth - 500, RNG.Next(0, _gameFieldHeight)), "Alien_planet", false));

        }

        public void Load(ContentManager content)
        {
            // Load background textures
            _starsTexture = content.Load<Texture2D>("stars_texture");
            _galaxyTexture = content.Load<Texture2D>("galaxy");

            _galaxyPosition = new Vector2(
                RNG.Next(_levelBounds.Left, _levelBounds.Right - _galaxyTexture.Width),
                RNG.Next(_levelBounds.Top, _levelBounds.Bottom - _galaxyTexture.Height)
            );

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Load(content);
            }
        }


        public void HandleInput(InputManager inputManager)
        {
            if (inputManager.IsKeyPress(Keys.P))
            {
                if (_currentState == GameState.Playing)
                    SetGameState(GameState.Paused);
                else if (_currentState == GameState.Paused)
                    SetGameState(GameState.Playing);
            }
            if (_currentState != GameState.Playing)
            {
                return;
            }
            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.HandleInput(this.InputManager);
            }
        }

        public void CheckCollision()
        {
            for (int i = 0; i < _gameObjects.Count; i++)
            {
                for (int j = i + 1; j < _gameObjects.Count; j++)
                {
                    GameObject a = _gameObjects[i];
                    GameObject b = _gameObjects[j];

                    if (a.CheckCollision(b))
                    {
                        if ((a is Ship && b is Planet) || (a is Planet && b is Ship))
                        {
                            Ship ship = a is Ship ? (Ship)a : (Ship)b;
                            Planet planet = a is Planet ? (Planet)a : (Planet)b;

                            if (planet.IsPickupPlanet)
                            {
                                ship.PickUpCargo();
                            }
                            else
                            {
                                if (ship.DropOffCargo())
                                {
                                    score += 100;
                                }
                            }
                        }

                        a.OnCollision(b);
                        b.OnCollision(a);
                    }
                }
            }
        }

        public void Update(GameTime gameTime)
        {
            InputManager.Update();

            switch (_currentState)
            {
                case GameState.StartScreen:
                    _startScreen.Update();
                    break;
                case GameState.Playing:
                    HandleInput(InputManager);

                    foreach (GameObject gameObject in _gameObjects)
                    {
                        if (gameObject is Ship playerShip && playerShip.IsDead())
                            continue;

                        gameObject.Update(gameTime);
                    }

                    _enemySpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_enemySpawnTimer >= _spawnInterval)
                    {
                        _enemySpawnTimer = 0f;

                        int currentAliens = _gameObjects.FindAll(g => g is Alien).Count;
                        if (currentAliens < _maxAliens)
                        {
                            
                            Vector2 spawnPos = RandomScreenLocation();
                            AddGameObject(new Alien(Player, _spawnSpeed));
                            _spawnSpeed += 5f;
                        }
                    }

                    _asteroidSpawnTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_asteroidSpawnTimer >= _nextAsteroidSpawnTime)
                    {
                        _asteroidSpawnTimer = 0f;

                        AddGameObject(new Asteroid(RandomScreenLocation()));

                        _nextAsteroidSpawnTime = rng.Next(8, 21);
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
                    _pauseScreen.Update();
                    break;
                case GameState.GameOver:
                    _gameOverScreen.Update();
                    break;
            }
        }

        public void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _camera.Follow(Player.GetPosition().Center.ToVector2());

            spriteBatch.Begin(transformMatrix: _camera.GetTransform());

            for (int x = 0; x < _levelBounds.Width; x += _backgroundTileSize)
            {
                for (int y = 0; y < _levelBounds.Height; y += _backgroundTileSize)
                {
                    spriteBatch.Draw(_starsTexture, new Vector2(x, y), Color.White);
                }
            }

            spriteBatch.Draw(_galaxyTexture, _galaxyPosition, Color.White);

            foreach (GameObject gameObject in _gameObjects)
            {
                gameObject.Draw(gameTime, spriteBatch);
            }

            spriteBatch.End(); // End world draw

            spriteBatch.Begin(); // HUD Layer

            // HUD setup
            int cockpitHeight = 100;
            Rectangle cockpitArea = new Rectangle(0, SpaceDefence.screenHeight - cockpitHeight, SpaceDefence.screenWidth, cockpitHeight);

            // HUD background
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });
            spriteBatch.Draw(pixel, cockpitArea, Color.Black * 0.8f);

            // HUD data
            string cargoText = $"Cargo: {(Player.HasCargo ? "Yes" : "No")}";
            string scoreText = $"Score: {score}";
            string targetText = Player.HasCargo ? "Next: Drop-off" : "Next: Pickup";
            string weaponText = Player.CurrentWeapon();

            string[] hudTexts = { cargoText, scoreText, targetText, weaponText };
            SpriteFont font = _content.Load<SpriteFont>("HUDFont");

            // Layout
            int segments = hudTexts.Length;
            float segmentWidth = SpaceDefence.screenWidth / segments;
            float yPos = SpaceDefence.screenHeight - cockpitHeight / 2f - font.LineSpacing / 2f;

            for (int i = 0; i < segments; i++)
            {
                string text = hudTexts[i];
                Vector2 textSize = font.MeasureString(text);
                float xPos = (segmentWidth * i) + (segmentWidth / 2f) - (textSize.X / 2f);

                spriteBatch.DrawString(font, text, new Vector2(xPos, yPos), Color.White);
            }

            spriteBatch.End();

            spriteBatch.Begin();

            if (_currentState == GameState.Paused)
            {
                _pauseScreen.Draw(spriteBatch);
            }

            if (_currentState == GameState.GameOver)
            {
                _gameOverScreen.Draw(spriteBatch);
            }

            if (_currentState == GameState.StartScreen)
            {
                _startScreen.Draw(spriteBatch);
            }

            spriteBatch.End();
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

                Player.Kill();

                Vector2 playerPosition = Player.GetPosition().Center.ToVector2();
                AddGameObject(
                    new SpriteAnimation(
                        playerPosition,
                        "Explosion",
                        frameWidth: 64,
                        frameHeight: 64,
                        frameCount: 35,
                        frameTime: 0.05f,
                        loop: false,
                        scale: 4f,
                        autoDestroy: true
                    )
                );

                Task.Delay(1000).ContinueWith(t =>
                {
                    SetGameState(GameState.GameOver);
                    playerExplosionTriggered = false;
                });
            }
        }

        public void Restart()
        {
            // Reset game state
            _gameObjects.Clear();
            _toBeAdded.Clear();
            _toBeRemoved.Clear();

            // Reset score
            score = 0;

            // Spawn planets
            AddGameObject(new Planet(new Vector2(300, RNG.Next(0, _gameFieldHeight)), "Earth", true));
            AddGameObject(new Planet(new Vector2(3700, RNG.Next(0, _gameFieldHeight)), "Alien_planet", false));


            // Reinitialize player
            Player = new Ship(new Point(_gameFieldWidth / 2, _gameFieldHeight / 2));
            AddGameObject(Player);

            // Spawn objects
            AddGameObject(new Alien(Player, 50f));
            AddGameObject(new Supply());

            _currentState = GameState.Playing;
        }

        /// <summary>
        /// Get a random location on the screen.
        /// </summary>
        public Vector2 RandomScreenLocation()
        {
            return new Vector2(
                RNG.Next(0, _gameFieldWidth),
                RNG.Next(0, _gameFieldHeight));
        }

    }
}
