using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class Explosion : GameObject
    {
        private Texture2D spriteSheet;
        private const int frameWidth = 64;
        private const int frameHeight = 64;
        private int currentFrame = 0;
        private float frameTime = 0.05f; // 50ms per frame
        private float timer = 0f;
        private const int totalFrames = 35; // The total frames from the explosion spritesheet
        private Vector2 position;
        private float explosionScale; // Customizable size for explosions
        private bool explosionFinished = false; // **Tracks if explosion is done**

        public Explosion(Vector2 position, float scale = 4f)
        {
            this.position = position;
            this.explosionScale = scale;
        }

        public override void Load(ContentManager content)
        {
            spriteSheet = content.Load<Texture2D>("Explosion"); // Ensure this matches the content pipeline name
        }

        public override void Update(GameTime gameTime)
        {
            if (explosionFinished) return; // **Stop updating if explosion is done**

            timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (timer >= frameTime)
            {
                timer = 0f;
                currentFrame++;

                // **Mark explosion as finished & remove**
                if (currentFrame >= totalFrames)
                {
                    explosionFinished = true;
                    GameManager.GetGameManager().RemoveGameObject(this);
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (explosionFinished) return; // **Don't draw if explosion is done**

            // Extract the correct frame from the spritesheet
            Rectangle sourceRect = new Rectangle(frameWidth * currentFrame, 0, frameWidth, frameHeight);
            Vector2 explosionOrigin = new Vector2(frameWidth / 2, frameHeight / 2);

            spriteBatch.Draw(spriteSheet, position, sourceRect, Color.White, 0f, explosionOrigin, explosionScale, SpriteEffects.None, 0f);
        }
    }
}
