using Microsoft.Xna.Framework;

namespace SpaceDefence.weapons
{
    public class LaserWeapon : Weapon
    {
        public override void Fire(Vector2 origin, Vector2 direction)
        {
            var laser = new Laser(new LinePieceCollider(origin, direction, SpaceDefence.screenWidth));
            laser.Load(content);
            GameManager.GetGameManager().AddGameObject(laser);
        }
    }
}
