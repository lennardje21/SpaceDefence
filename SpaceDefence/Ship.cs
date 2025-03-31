using System;
using SpaceDefence.Collision;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using SpaceDefence.weapons;

namespace SpaceDefence
{
    public class Ship : GameObject
    {
        private Texture2D ship_body;
        private Texture2D base_turret;
        private Texture2D laser_turret;
        private Texture2D shotgun_turret;

        private float buffTimer = 10;
        private float buffDuration = 10f;
        private RectangleCollider _collider; // collider for ship
        private Point target;

        // weapons
        private Weapon currentWeapon;
        private Weapon bulletWeapon;
        private Weapon laserWeapon;
        private Weapon shotgunWeapon;

        private Vector2 velocity = Vector2.Zero;
        private Vector2 acceleration = Vector2.Zero;
        private float speed = 20f;
        private float friction = 0.97f;
        private float rotationAngle = 0f; // keep track of ship angle
        private int screenWidth = GameManager._gameFieldWidth;
        private int screenHeight = GameManager._gameFieldHeight;

        private GameManager gm = GameManager.GetGameManager();

        private bool hasCargo = false;
        public bool HasCargo => hasCargo;

        public string CurrentWeapon()
        {
            if (currentWeapon == bulletWeapon)
            {
                return "Bullet";
            }
            else if (currentWeapon == laserWeapon)
            {
                return "Laser";
            }
            return "Shotgun";
        }

        /// <summary>
        /// The player character
        /// </summary>
        /// <param name="Position">The ship's starting position</param>
        public Ship(Point Position)
        {
            _collider = new RectangleCollider(Position.ToVector2(), 50f, 120f);
            SetCollider(_collider);
            gm.SetPlayer(this);
        }

        public override void Load(ContentManager content)
        {
            ship_body = content.Load<Texture2D>("ship_body");
            bulletWeapon = new BulletWeapon();
            laserWeapon = new LaserWeapon();
            shotgunWeapon = new ShotgunWeapon();

            bulletWeapon.Load(content);
            laserWeapon.Load(content);
            shotgunWeapon.Load(content);

            currentWeapon = bulletWeapon; // start with bullet

            base_turret = content.Load<Texture2D>("base_turret");
            laser_turret = content.Load<Texture2D>("laser_turret");
            shotgun_turret = content.Load<Texture2D>("shotgun_turret");

            base.Load(content);
        }

        public override void HandleInput(InputManager inputManager)
        {
            base.HandleInput(inputManager);

            Vector2 screenMouse = inputManager.CurrentMouseState.Position.ToVector2();
            target = gm.GetCamera().ScreenToWorld(screenMouse).ToPoint();

            acceleration = Vector2.Zero;
            if (inputManager.IsKeyDown(Keys.W)) acceleration += new Vector2(0, -1);
            if (inputManager.IsKeyDown(Keys.S)) acceleration += new Vector2(0, 1);
            if (inputManager.IsKeyDown(Keys.A)) acceleration += new Vector2(-1, 0);
            if (inputManager.IsKeyDown(Keys.D)) acceleration += new Vector2(1, 0);

            if (acceleration.LengthSquared() > 0)
            {
                acceleration = Vector2.Normalize(acceleration) * speed;
            }

            if (inputManager.IsKeyPress(Keys.D1))
                currentWeapon = bulletWeapon;
            else if (inputManager.IsKeyPress(Keys.D2))
                currentWeapon = laserWeapon;

            if (inputManager.LeftMousePress())
            {
                Vector2 aimDirection = LinePieceCollider.GetDirection(GetPosition().Center, target);
                Vector2 turretExit = _collider.Center + aimDirection * base_turret.Height / 2f;

                currentWeapon.Fire(turretExit, aimDirection);
            }

        }


        private bool isDead = false;

        public void Kill()
        {
            if (!isDead)
            {
                isDead = true;
                velocity = Vector2.Zero;
                acceleration = Vector2.Zero;
            }
        }
        public bool IsDead() { return isDead; }

        public override void Update(GameTime gameTime)
        {
            if (isDead) return;

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

            if (buffTimer > 0)
{
                buffTimer -= deltaTime;
                if (buffTimer <= 0)
                {
                    currentWeapon = bulletWeapon; // revert to default
                }
            }


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

            // Aim turret at mouse cursor
            float aimAngle = LinePieceCollider.GetAngle(LinePieceCollider.GetDirection(GetPosition().Center, target));

            Texture2D currentTurretTexture = base_turret;

            if (currentWeapon == laserWeapon)
                currentTurretTexture = laser_turret;
            else if (currentWeapon == shotgunWeapon && shotgun_turret != null)
                currentTurretTexture = shotgun_turret;

            spriteBatch.Draw(currentTurretTexture, _collider.Center, null, Color.White, aimAngle,
                new Vector2(currentTurretTexture.Width / 2f, currentTurretTexture.Height / 2f), 1f, SpriteEffects.None, 0);


            // Draw debug collider border
            //DrawRotatableCollider(spriteBatch, _collider);

            base.Draw(gameTime, spriteBatch);
        }

        public void PickUpCargo()
        {
            if (!hasCargo)
            {
                hasCargo = true;
            }
        }

        public bool DropOffCargo()
        {
            if (hasCargo)
            {
                hasCargo = false;
                return true; // Successful delivery
            }
            return false;
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
            currentWeapon = shotgunWeapon;
        }

        public Rectangle GetPosition()
        {
            return _collider.GetBoundingBox();
        }
    }
}
