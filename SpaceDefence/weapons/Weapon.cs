using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence.weapons
{
    public abstract class Weapon
    {
        protected ContentManager content;

        public virtual void Load(ContentManager content)
        {
            this.content = content;
        }

        public virtual void Update(GameTime gameTime) { }

        /// <summary>
        /// Fire the weapon in a direction from a starting point.
        /// </summary>
        public abstract void Fire(Vector2 origin, Vector2 direction);
    }
}
