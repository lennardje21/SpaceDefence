using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpaceDefence.Collision;
using System;

namespace SpaceDefence
{
    public class Asteroid : GameObject
    {
        private Texture2D _texture;
        private CircleCollider _collider;
        private Vector2 _position;
        private float scale = 2f;
        private const float visibleScale = 0.35f; // Portion of the texture that actually represents the visible rock
        private bool drawDebug = true; // Toggle hitbox visibility

        public Asteroid(Vector2 position, float scale = 2f)
        {
            this._position = position;
            this.scale = scale;
        }

        public override void Load(ContentManager content)
        {
            _texture = content.Load<Texture2D>("Asteroid"); // Ensure this texture exists in Content

            float radius = (_texture.Width / 2f) * visibleScale * scale;
            _collider = new CircleCollider(_position, radius);
            SetCollider(_collider);
        }

        public override void Update(GameTime gameTime)
        {
            // Asteroids do not move
        }

        public override void OnCollision(GameObject other)
        {
            if (other is Ship || other is Alien)
            {
                GameManager.GetGameManager().RemoveGameObject(other);

                if (other is Ship)
                    GameManager.GetGameManager().GameOver();

                GameManager.GetGameManager().AddGameObject(new Explosion(_collider.Center, 2.0f));
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            Vector2 origin = new Vector2(_texture.Width / 2f, _texture.Height / 2f);

            spriteBatch.Draw(
                _texture,
                _collider.Center,
                null,
                Color.White,
                0f,
                origin,
                scale,
                SpriteEffects.None,
                0f
            );

            if (drawDebug)
                DrawCircle(spriteBatch, _collider.Center, _collider.Radius, 32, Color.Red);
        }

        // Draw a circle outline for collider debugging
        private void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, int segments, Color color)
        {
            Texture2D pixel = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
            pixel.SetData(new[] { Color.White });

            float increment = MathHelper.TwoPi / segments;
            Vector2 lastPoint = center + new Vector2((float)Math.Cos(0), (float)Math.Sin(0)) * radius;

            for (int i = 1; i <= segments; i++)
            {
                float angle = i * increment;
                Vector2 newPoint = center + new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle)) * radius;
                DrawLine(spriteBatch, pixel, lastPoint, newPoint, color);
                lastPoint = newPoint;
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Texture2D texture, Vector2 start, Vector2 end, Color color)
        {
            Vector2 edge = end - start;
            float angle = (float)Math.Atan2(edge.Y, edge.X);
            spriteBatch.Draw(texture, start, null, color, angle, Vector2.Zero, new Vector2(edge.Length(), 1), SpriteEffects.None, 0);
        }
    }
}
