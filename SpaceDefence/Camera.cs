using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence
{
    public class Camera
    {
        private Vector2 position;
        private readonly Viewport viewport;

        public Camera(Viewport viewport)
        {
            this.viewport = viewport;
        }

        public Matrix GetTransform()
        {
            return Matrix.CreateTranslation(new Vector3(-position + new Vector2(viewport.Width / 2, viewport.Height / 2), 0));
        }

        public void Follow(Vector2 target)
        {
            position = target;

            // Clamp camera position to level bounds (optional)
            position.X = MathHelper.Clamp(position.X, viewport.Width / 2, GameManager.LevelBounds.Width - viewport.Width / 2);
            position.Y = MathHelper.Clamp(position.Y, viewport.Height / 2, GameManager.LevelBounds.Height - viewport.Height / 2);
        }


        public Vector2 GetPosition()
        {
            return position;
        }

        public Vector2 ScreenToWorld(Vector2 screenPosition)
        {
            return Vector2.Transform(screenPosition, Matrix.Invert(GetTransform()));
        }

    }
}
