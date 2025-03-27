using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace SpaceDefence.Collision
{
    public class RectangleCollider : Collider, IEquatable<RectangleCollider>
    {
        public Rectangle shape;
        public float Rotation { get; set; } = 0f; // Rotation in radians
        public Vector2 Center
        {
            get => shape.Center.ToVector2();
            set => shape = new Rectangle(
                (int)(value.X - shape.Width / 2),
                (int)(value.Y - shape.Height / 2),
                shape.Width,
                shape.Height);
        }


        public RectangleCollider(Vector2 center, float width, float height)
        {
            int x = (int)(center.X - width / 2);
            int y = (int)(center.Y - height / 2);
            shape = new Rectangle(x, y, (int)width, (int)height);
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
            Vector2[] corners = GetRotatedCorners();

            float minX = MathF.Min(MathF.Min(corners[0].X, corners[1].X), MathF.Min(corners[2].X, corners[3].X));
            float minY = MathF.Min(MathF.Min(corners[0].Y, corners[1].Y), MathF.Min(corners[2].Y, corners[3].Y));
            float maxX = MathF.Max(MathF.Max(corners[0].X, corners[1].X), MathF.Max(corners[2].X, corners[3].X));
            float maxY = MathF.Max(MathF.Max(corners[0].Y, corners[1].Y), MathF.Max(corners[2].Y, corners[3].Y));

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        public override bool Intersects(CircleCollider other)
        {
            Vector2 closestPoint = GetClosestPointOnRotatedRect(other.Center);
            return (closestPoint - other.Center).LengthSquared() < other.Radius * other.Radius;
        }

        public override bool Intersects(RectangleCollider other)
        {
            return GetBoundingBox().Intersects(other.GetBoundingBox());
        }

        public override bool Intersects(LinePieceCollider other)
        {
            return other.Intersects(this);
        }

        // ✅ New Helper Functions for Rotation Handling

        /// <summary>
        /// Returns the four corners of the rotated rectangle.
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

        /// <summary>
        /// Rotates a point around a given origin.
        /// </summary>
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
        /// Gets the closest point on the rotated rectangle to a target point.
        /// </summary>
        private Vector2 GetClosestPointOnRotatedRect(Vector2 point)
        {
            Vector2 center = shape.Center.ToVector2();
            Vector2 localPoint = RotatePoint(point, -Rotation, center);

            float clampedX = Math.Clamp(localPoint.X, shape.Left, shape.Right);
            float clampedY = Math.Clamp(localPoint.Y, shape.Top, shape.Bottom);

            Vector2 clamped = new Vector2(clampedX, clampedY);
            return RotatePoint(clamped, Rotation, center);
        }

        /// <summary>
        /// Draws the collider for debugging purposes.
        /// </summary>
        public void Draw(SpriteBatch spriteBatch, Texture2D texture)
        {
            Vector2[] corners = GetRotatedCorners();
            for (int i = 0; i < 4; i++)
            {
                int next = (i + 1) % 4;
                DrawLine(spriteBatch, corners[i], corners[next], texture, Color.Green);
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
