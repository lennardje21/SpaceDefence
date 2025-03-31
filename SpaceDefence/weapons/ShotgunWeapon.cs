using Microsoft.Xna.Framework;
using SpaceDefence.weapons;
using System;

namespace SpaceDefence
{
    public class ShotgunWeapon : Weapon
    {
        private float bulletSpeed = 130f;
        private int pelletCount = 5;
        private float spreadAngle = MathHelper.ToRadians(30); // total spread in radians

        public override void Fire(Vector2 origin, Vector2 direction)
        {
            float angleBetween = spreadAngle / (pelletCount - 1);
            float baseAngle = (float)Math.Atan2(direction.Y, direction.X) - (spreadAngle / 2f);

            for (int i = 0; i < pelletCount; i++)
            {
                float angle = baseAngle + (i * angleBetween);
                Vector2 spreadDir = new Vector2((float)Math.Cos(angle), (float)Math.Sin(angle));

                var bullet = new Bullet(origin, spreadDir, bulletSpeed);
                bullet.Load(content);
                GameManager.GetGameManager().AddGameObject(bullet);
            }
        }
    }
}
