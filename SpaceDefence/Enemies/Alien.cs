using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpaceDefence;
using System;
using SpaceDefence.weapons;

internal class Alien : GameObject
{
    private CircleCollider _circleCollider;
    private Texture2D _texture;
    private float _playerClearance = 50;
    private float _speed;
    private Ship player;

    public Alien(Ship player, float speed)
    {
        this.player = player;
        this._speed = speed;
    }

    public override void Load(ContentManager content)
    {
        base.Load(content);
        _texture = content.Load<Texture2D>("Alien");
        _circleCollider = new CircleCollider(Vector2.Zero, _texture.Width / 2);
        SetCollider(_circleCollider);
        RandomMove();
    }

    public override void OnCollision(GameObject other)
    {
        if (other is Bullet || other is Laser || other is Asteroid)
        {
            // Get alien's position for explosion
            Vector2 explosionPosition = _circleCollider.Center;

            // Add explosion effect
            GameManager.GetGameManager().AddGameObject(
                new SpriteAnimation(
                    explosionPosition,
                    "Explosion",
                    frameWidth: 64,
                    frameHeight: 64,
                    frameCount: 35,
                    frameTime: 0.05f,
                    loop: false,
                    scale: 2f,
                    autoDestroy: true
                )
            );

            // Remove alien and spawn a faster one
            float newSpeed = _speed + 10f;
            GameManager.GetGameManager().RemoveGameObject(this);
            GameManager.GetGameManager().AddGameObject(new Alien(GameManager.GetGameManager().Player, newSpeed));

        } else if (other is Ship)
        {
            GameManager.GetGameManager().GameOver();
        }
        base.OnCollision(other);
    }


    public void RandomMove()
    {
        GameManager gm = GameManager.GetGameManager();
        _circleCollider.Center = gm.RandomScreenLocation();

        Vector2 centerOfPlayer = player.GetPosition().Center.ToVector2();
        while ((_circleCollider.Center - centerOfPlayer).Length() < _playerClearance)
            _circleCollider.Center = gm.RandomScreenLocation();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Chase the player
        Vector2 direction = Vector2.Normalize(player.GetPosition().Center.ToVector2() - _circleCollider.Center);
        _circleCollider.Center += direction * _speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if ((_circleCollider.Center - player.GetPosition().Center.ToVector2()).Length() < _playerClearance)
        {
            GameManager.GetGameManager().GameOver();
        }
    }

    public override void Draw(GameTime gameTime, SpriteBatch spriteBatch)
    {
        spriteBatch.Draw(_texture, _circleCollider.GetBoundingBox(), Color.White);
        base.Draw(gameTime, spriteBatch);
    }
}