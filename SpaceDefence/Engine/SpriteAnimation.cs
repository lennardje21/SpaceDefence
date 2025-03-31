using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class SpriteAnimation : GameObject
    {
        private Texture2D _spriteSheet;
        private Vector2 _position;
        private int _frameWidth;
        private int _frameHeight;
        private int _frameCount;
        private float _frameTime;
        private float _timer;
        private int _currentFrame = 0;
        private bool _loop;
        private bool _finished = false;
        private float _scale;
        private bool _autoDestroy;

        public SpriteAnimation(
            Vector2 position,
            string spriteSheetName,
            int frameWidth,
            int frameHeight,
            int frameCount,
            float frameTime,
            bool loop = false,
            float scale = 1f,
            bool autoDestroy = true)
        {
            _position = position;
            SpriteSheetName = spriteSheetName;
            _frameWidth = frameWidth;
            _frameHeight = frameHeight;
            _frameCount = frameCount;
            _frameTime = frameTime;
            _loop = loop;
            _scale = scale;
            _autoDestroy = autoDestroy;
        }

        public string SpriteSheetName { get; }

        public override void Load(ContentManager content)
        {
            _spriteSheet = content.Load<Texture2D>(SpriteSheetName);
        }

        public override void Update(GameTime gameTime)
        {
            if (_finished) return;

            _timer += (float)gameTime.ElapsedGameTime.TotalSeconds;

            if (_timer >= _frameTime)
            {
                _timer -= _frameTime;
                _currentFrame++;

                if (_currentFrame >= _frameCount)
                {
                    if (_loop)
                    {
                        _currentFrame = 0;
                    }
                    else
                    {
                        _finished = true;
                        if (_autoDestroy)
                            GameManager.GetGameManager().RemoveGameObject(this);
                    }
                }
            }
        }

        public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
        {
            if (_finished) return;

            Rectangle sourceRect = new Rectangle(_currentFrame * _frameWidth, 0, _frameWidth, _frameHeight);
            Vector2 origin = new Vector2(_frameWidth / 2f, _frameHeight / 2f);

            spriteBatch.Draw(
                _spriteSheet,
                _position,
                sourceRect,
                Color.White,
                0f,
                origin,
                _scale,
                SpriteEffects.None,
                0f
            );
        }

        public Vector2 GetPosition()
        {
            return _position;
        }
    }
}
