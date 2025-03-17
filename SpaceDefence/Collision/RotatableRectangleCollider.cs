using System;
using Microsoft.Xna.Framework;

namespace SpaceDefence.Collision
{
    public class RotatableRectangleCollider : Collider
    {
        public Vector2 Center;
        public float Width;
        public float Height;
        public float Rotation; // Rotation in radians

        public RotatableRectangleCollider(Vector2 center, float width, float height, float rotation = 0f)
        {
            Center = center;
            Width = width;
            Height = height;
            Rotation = rotation;
        }

        public override bool Contains(Vector2 loc)
        {
            Vector2 localPoint = RotatePoint(loc, -Rotation, Center);
            return Math.Abs(localPoint.X - Center.X) <= Width / 2 &&
                   Math.Abs(localPoint.Y - Center.Y) <= Height / 2;
        }

        public override bool Intersects(CircleCollider other)
        {
            Vector2 closestPoint = GetClosestPoint(other.Center);
            return (closestPoint - other.Center).LengthSquared() < (other.Radius * other.Radius);
        }

        public override bool Intersects(RectangleCollider other)
        {
            return other.shape.Intersects(GetBoundingBox());
        }

        public override bool Intersects(LinePieceCollider other)
        {
            Vector2[] rectCorners = GetRotatedCorners();
            LinePieceCollider topEdge = new LinePieceCollider(rectCorners[0], rectCorners[1]);
            LinePieceCollider bottomEdge = new LinePieceCollider(rectCorners[2], rectCorners[3]);
            LinePieceCollider leftEdge = new LinePieceCollider(rectCorners[0], rectCorners[2]);
            LinePieceCollider rightEdge = new LinePieceCollider(rectCorners[1], rectCorners[3]);

            return other.Intersects(topEdge) || other.Intersects(bottomEdge) ||
                   other.Intersects(leftEdge) || other.Intersects(rightEdge);
        }

        public override Rectangle GetBoundingBox()
        {
            Vector2[] corners = GetRotatedCorners();
            float minX = Math.Min(corners[0].X, Math.Min(corners[1].X, Math.Min(corners[2].X, corners[3].X)));
            float minY = Math.Min(corners[0].Y, Math.Min(corners[1].Y, Math.Min(corners[2].Y, corners[3].Y)));
            float maxX = Math.Max(corners[0].X, Math.Max(corners[1].X, Math.Max(corners[2].X, corners[3].X)));
            float maxY = Math.Max(corners[0].Y, Math.Max(corners[1].Y, Math.Max(corners[2].Y, corners[3].Y)));

            return new Rectangle((int)minX, (int)minY, (int)(maxX - minX), (int)(maxY - minY));
        }

        public Vector2[] GetRotatedCorners()
        {
            Vector2 halfSize = new Vector2(Width / 2, Height / 2);
            Vector2[] corners = new Vector2[]
            {
                Center + RotatePoint(new Vector2(-halfSize.X, -halfSize.Y), Rotation, Vector2.Zero),
                Center + RotatePoint(new Vector2(halfSize.X, -halfSize.Y), Rotation, Vector2.Zero),
                Center + RotatePoint(new Vector2(-halfSize.X, halfSize.Y), Rotation, Vector2.Zero),
                Center + RotatePoint(new Vector2(halfSize.X, halfSize.Y), Rotation, Vector2.Zero)
            };
            return corners;
        }

        private Vector2 GetClosestPoint(Vector2 point)
        {
            Vector2 localPoint = RotatePoint(point, -Rotation, Center);
            float clampedX = Math.Clamp(localPoint.X, Center.X - Width / 2, Center.X + Width / 2);
            float clampedY = Math.Clamp(localPoint.Y, Center.Y - Height / 2, Center.Y + Height / 2);
            return RotatePoint(new Vector2(clampedX, clampedY), Rotation, Center);
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
    }
}
