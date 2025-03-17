using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using SpaceDefence;
using System;

internal class Alien : GameObject
{
    private CircleCollider _circleCollider;
    private Texture2D _texture;
    private float playerClearance = 50; // Distance at which the game is over
    private float speed; // Speed of the alien
    private Ship player;

    public Alien(Ship player, float speed)
    {
        this.player = player;
        this.speed = speed;
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
        if (other is Bullet || other is Laser)
        {
            // Increase speed for the next alien and spawn a new one
            float newSpeed = speed + 10f; // Increase speed by 10
            GameManager.GetGameManager().RemoveGameObject(this);
            GameManager.GetGameManager().AddGameObject(new Alien(player, newSpeed));
        }
        else if (other is Ship)
        {
            Console.WriteLine("Alien hit the player! Game Over."); // Debugging output
            GameManager.GetGameManager().GameOver();
        }
        base.OnCollision(other);
    }


    public void RandomMove()
    {
        GameManager gm = GameManager.GetGameManager();
        _circleCollider.Center = gm.RandomScreenLocation();

        Vector2 centerOfPlayer = player.GetPosition().Center.ToVector2();
        while ((_circleCollider.Center - centerOfPlayer).Length() < playerClearance)
            _circleCollider.Center = gm.RandomScreenLocation();
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);

        // Chase the player
        Vector2 direction = Vector2.Normalize(player.GetPosition().Center.ToVector2() - _circleCollider.Center);
        _circleCollider.Center += direction * speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

        if ((_circleCollider.Center - player.GetPosition().Center.ToVector2()).Length() < playerClearance)
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