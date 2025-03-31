using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class Planet : GameObject
    {
        private SpriteAnimation _animation;
        private bool isPickup;
        private string textureName;
        private Vector2 _position;

        public Planet(Vector2 position, string textureName, bool isPickup)
        {
            this.isPickup = isPickup;
            this.textureName = textureName;
            _position = position;
            // Temporarily assign a dummy collider (will be replaced after loading)
            SetCollider(new CircleCollider(position, 64));
        }

        public override void Load(ContentManager content)
        {
            _animation = new SpriteAnimation(
                _position,
                textureName,
                frameWidth: 96,
                frameHeight: 96,
                frameCount: 77,
                frameTime: 0.07f,
                loop: true,
                scale: 3f,
                autoDestroy: false
            );
            _animation.Load(content);

            // Replace collider with actual size
            SetCollider(new CircleCollider(_animation.GetPosition(), 32f * 1.5f)); // Match visual scale
        }

        public override void Update(GameTime gameTime)
        {
            _animation.Update(gameTime);
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            _animation.Draw(gameTime, spriteBatch);
        }

        public bool IsPickupPlanet => isPickup;
    }
}
