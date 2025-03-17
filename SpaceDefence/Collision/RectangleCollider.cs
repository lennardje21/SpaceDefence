using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence.Collision
{
    public class RectangleCollider : Collider, IEquatable<RectangleCollider>
    {
        public Rectangle shape;
        public float Rotation { get; set; } = 0f; // Rotation in radians

        public RectangleCollider(Rectangle shape)
        {
            this.shape = shape;
        }

        public override bool Contains(Vector2 loc)
        {
            Vector2 localPoint = RotatePoint(loc, -Rotation, shape.Center.ToVector2());
            return shape.Contains(localPoint.ToPoint());
        }

        public bool Equals(RectangleCollider other)
        {
            return shape == other.shape;
        }

        public override Rectangle GetBoundingBox()
        {
            return shape;
        }

        public override bool Intersects(CircleCollider other)
        {
            return other.Intersects(this);
        }

        public override bool Intersects(RectangleCollider other)
        {
            return shape.Intersects(other.shape);
        }

        public override bool Intersects(LinePieceCollider other)
        {
            return other.Intersects(this);
        }

        // ✅ New Helper Functions for Rotation Handling

        /// <summary>
        /// Returns the four corners of the rotated rectangle
        /// </summary>
        public Vector2[] GetRotatedCorners()
        {
            Vector2 center = shape.Center.ToVector2();
            Vector2 halfSize = new Vector2(shape.Width / 2f, shape.Height / 2f);

            Vector2[] corners = new Vector2[]
            {
                center + RotatePoint(-halfSize, Rotation, Vector2.Zero),
                center + RotatePoint(new Vector2(halfSize.X, -halfSize.Y), Rotation, Vector2.Zero),
                center + RotatePoint(halfSize, Rotation, Vector2.Zero),
                center + RotatePoint(new Vector2(-halfSize.X, halfSize.Y), Rotation, Vector2.Zero)
            };

            return corners;
        }

        private Vector2 RotatePoint(Vector2 point, float angle, Vector2 origin)
        {
            float cos = (float)Math.Cos(angle);
            float sin = (float)Math.Sin(angle);
            Vector2 translated = point - origin;
            float newX = translated.X * cos - translated.Y * sin;
            float newY = translated.X * sin + translated.Y * cos;
            return new Vector2(newX, newY) + origin;
        }

        /// <summary>
        /// Draws the collider for debugging purposes
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            Vector2[] corners = GetRotatedCorners();
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                DrawLine(spriteBatch, corners[i], corners[next], texture, Color.Green); // Green debug border
            }
        }

        private void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Texture2D texture, Color color)
        {
            Vector2 direction = end - start;
            float length = direction.Length();
            float angle = (float)Math.Atan2(direction.Y, direction.X);
            spriteBatch.Draw(texture, start, null, color, angle, Vector2.Zero, new Vector2(length, 2), SpriteEffects.None, 0);
        }
    }
}
