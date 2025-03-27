using System;
using SpaceDefence.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace SpaceDefence
{
    public class Ship : GameObject
    {
        private Texture2D ship_body;
        private Texture2D base_turret;
        private Texture2D laser_turret;
        private float buffTimer = 10;
        private float buffDuration = 10f;
        private RectangleCollider _collider; // Rotatable collider for ship
        private Point target;

        private Vector2 velocity = Vector2.Zero; // Current movement velocity
        private Vector2 acceleration = Vector2.Zero; // Movement acceleration
        private float speed = 10f; // Base acceleration speed
        private float friction = 0.98f; // Simulated space friction to gradually slow down movement
        private float rotationAngle = 0f; // Stores the ship's rotation angle
        private int screenWidth = GameManager._gameFieldWidth; // Adjust as needed
        private int screenHeight = GameManager._gameFieldHeight; // Adjust as needed

        /// <summary>
        /// The player character
        /// </summary>
        /// <param name="Position">The ship's starting position</param>
        public Ship(Point Position)
        {
            _collider = new RectangleCollider(Position.ToVector2(), 50f, 120f); // Adjust size as needed
            SetCollider(_collider);
            GameManager.GetGameManager().SetPlayer(this); // Set the player reference
        }

        public override void Load(ContentManager content)
        {
            ship_body = content.Load<Texture2D>("ship_body");
            base_turret = content.Load<Texture2D>("base_turret");
            laser_turret = content.Load<Texture2D>("laser_turret");
            base.Load(content);
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            // Convert screen mouse position to world position
            Vector2 screenMouse = inputManager.CurrentMouseState.Position.ToVector2();
            target = GameManager.GetGameManager().GetCamera().ScreenToWorld(screenMouse).ToPoint();

            // Reset acceleration each frame
            acceleration = Vector2.Zero;
            if (inputManager.IsKeyDown(Keys.W)) acceleration += new Vector2(0, -1);
            if (inputManager.IsKeyDown(Keys.S)) acceleration += new Vector2(0, 1);
            if (inputManager.IsKeyDown(Keys.A)) acceleration += new Vector2(-1, 0);
            if (inputManager.IsKeyDown(Keys.D)) acceleration += new Vector2(1, 0);

            // Normalize acceleration to ensure equal speed in all directions
            if (acceleration.LengthSquared() > 0)
            {
                acceleration = Vector2.Normalize(acceleration) * speed;
            }

            // --- Shooting Mechanic ---
            if (inputManager.LeftMousePress())
            {
                Vector2 aimDirection = LinePieceCollider.GetDirection(GetPosition().Center, target);
                Vector2 turretExit = _collider.Center + aimDirection * base_turret.Height / 2f;

                if (buffTimer <= 0)
                {
                    GameManager.GetGameManager().AddGameObject(new Bullet(turretExit, aimDirection, 150));
                }
                else
                {
                    GameManager.GetGameManager().AddGameObject(new Laser(new LinePieceCollider(turretExit, target.ToVector2()), SpaceDefence.screenWidth));
                }
            }
        }


        private bool isDead = false; // **Track if the player is dead**

        public void Kill()
        {
            if (!isDead) // Prevent multiple deaths
            {
                isDead = true;
                velocity = Vector2.Zero; // **Stop movement**
                acceleration = Vector2.Zero;
                Console.WriteLine("Player has been destroyed. Disabling input and movement.");
            }
        }
        public bool IsDead() { return isDead; }

        public override void Update(GameTime gameTime)
        {
            if (isDead) return; // **Prevent movement & input if dead**

            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            // Update Buff Timer
            if (buffTimer > 0)
                buffTimer -= deltaTime;

            // Apply acceleration to velocity
            velocity += acceleration * deltaTime;

            // Apply friction to simulate inertia in space
            velocity *= friction;

            // Rotate the ship in the direction of velocity (only if moving)
            if (velocity.LengthSquared() > 0.01f)
            {
                rotationAngle = (float)Math.Atan2(velocity.X, -velocity.Y);
            }

            // Update collider to match ship position and rotation
            _collider.Center += velocity;
            _collider.Rotation = rotationAngle;

            // Handle screen wrapping
            WrapScreen();

            base.Update(gameTime);
        }

        private void WrapScreen()
        {
            Vector2 center = _collider.Center;

            if (center.X < 0) center.X = screenWidth;
            if (center.X > screenWidth) center.X = 0;
            if (center.Y < 0) center.Y = screenHeight;
            if (center.Y > screenHeight) center.Y = 0;

            _collider.Center = center;
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            // Draw ship with rotation applied
            spriteBatch.Draw(
                ship_body,
                _collider.Center,
                null,
                Color.White,
                rotationAngle,
                new Vector2(ship_body.Width / 2f, ship_body.Height / 2f),
                1f,
                SpriteEffects.None,
                0
            );

            // Aim turret based on mouse position
            float aimAngle = LinePieceCollider.GetAngle(LinePieceCollider.GetDirection(GetPosition().Center, target));

            if (buffTimer <= 0)
            {
                spriteBatch.Draw(base_turret, _collider.Center, null, Color.White, aimAngle,
                    new Vector2(base_turret.Width / 2f, base_turret.Height / 2f), 1f, SpriteEffects.None, 0);
            }
            else
            {
                spriteBatch.Draw(laser_turret, _collider.Center, null, Color.White, aimAngle,
                    new Vector2(laser_turret.Width / 2f, laser_turret.Height / 2f), 1f, SpriteEffects.None, 0);
            }

            // Draw debug collider border
            DrawRotatableCollider(spriteBatch, _collider);

            base.Draw(gameTime, spriteBatch);
        }

        private void DrawRotatableCollider(SpriteBatch spriteBatch, RectangleCollider collider)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.Red });

            Vector2[] corners = collider.GetRotatedCorners();

            DrawLine(spriteBatch, pixel, corners[0], corners[1]);
            DrawLine(spriteBatch, pixel, corners[1], corners[2]);
            DrawLine(spriteBatch, pixel, corners[2], corners[3]);
            DrawLine(spriteBatch, pixel, corners[3], corners[0]);
        }

        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(texture, start, null, Color.Red, angle, Vector2.Zero, new Vector2(edge.Length(), 1), SpriteEffects.None, 0);
        }

        public void Buff()
        {
            buffTimer = buffDuration;
        }

        public Rectangle GetPosition()
        {
            return _collider.GetBoundingBox();
        }
    }
}
