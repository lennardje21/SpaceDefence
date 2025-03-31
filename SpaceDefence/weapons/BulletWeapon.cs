using Microsoft.Xna.Framework;

namespace SpaceDefence.weapons
{
    public class BulletWeapon : Weapon
    {
        private float speed = 150f;

        public override void Fire(Vector2 origin, Vector2 direction)
        {
            var bullet = new Bullet(origin, direction, speed);
            bullet.Load(content);
            GameManager.GetGameManager().AddGameObject(bullet);
        }
    }
}
