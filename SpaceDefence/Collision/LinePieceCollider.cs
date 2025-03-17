using System;
using SpaceDefence.Collision;
using Microsoft.Xna.Framework;

namespace SpaceDefence
{

    public class LinePieceCollider : Collider, IEquatable<LinePieceCollider>
    {

        public Vector2 Start;
        public Vector2 End;

        /// <summary>
        /// The length of the LinePiece, changing the length moves the end vector to adjust the length.
        /// </summary>
        public float Length 
        { 
            get { 
                return (End - Start).Length(); 
            } 
            set {
                End = Start + GetDirection() * value; 
            }
        }

        /// <summary>
        /// The A component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardA
        {
            get
            {
                return End.Y - Start.Y;
            }
        }

        /// <summary>
        /// The B component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardB
        {
            get
            {
                return -(End.X - Start.X);
            }
        }

        /// <summary>
        /// The C component from the standard line formula Ax + By + C = 0
        /// </summary>
        public float StandardC
        {
            get
            {
                return (StandardA * Start.X) + (StandardB * Start.Y);
            }
        }

        public LinePieceCollider(Vector2 start, Vector2 end)
        {
            Start = start;
            End = end;
        }
        
        public LinePieceCollider(Vector2 start, Vector2 direction, float length)
        {
            Start = start;
            End = start + direction * length;
        }

        /// <summary>
        /// Should return the angle between a given direction and the up vector.
        /// </summary>
        /// <param name="direction">The Vector2 pointing out from (0,0) to calculate the angle to.</param>
        /// <returns> The angle in radians between the the up vector and the direction to the cursor.</returns>
        public static float GetAngle(Vector2 direction)
        {
            // Normalize the direction vector
            direction = Vector2.Normalize(direction);

            // Calculate the angle between the direction and the up vector (0, -1)
            return (float)Math.Atan2(direction.X, -direction.Y);
        }


        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public static Vector2 GetDirection(Vector2 point1, Vector2 point2)
        {
            // Calculate the vector difference
            Vector2 direction = point2 - point1;

            // Normalize it to get a unit direction vector
            return Vector2.Normalize(direction);
        }


        /// <summary>
        /// Gets whether or not the Line intersects another Line
        /// </summary>
        /// <param name="other">The Line to check for intersection</param>
        /// <returns>true there is any overlap between the Circle and the Line.</returns>
        public override bool Intersects(LinePieceCollider other)
        {
            // TODO Implement.
            return false;
        }


        /// <summary>
        /// Gets whether or not the line intersects a Circle.
        /// </summary>
        /// <param name="other">The Circle to check for intersection.</param>
        /// <returns>true there is any overlap between the two Circles.</returns>
        public override bool Intersects(CircleCollider other)
        {
            // Compute the closest point on the infinite line
            Vector2 closestPoint = NearestPointOnLine(other.Center);

            // Check if the closest point lies on the actual line segment
            if (!IsPointOnSegment(closestPoint, this.Start, this.End))
            {
                return false; // The intersection occurs outside the line segment
            }

            // Check if the closest point is within the circle's radius
            float distanceSquared = (other.Center - closestPoint).LengthSquared();
            return distanceSquared < (other.Radius * other.Radius);
        }


        /// <summary>
        /// Gets whether or not the Line intersects the Rectangle.
        /// </summary>
        /// <param name="other">The Rectangle to check for intersection.</param>
        /// <returns>true there is any overlap between the Circle and the Rectangle.</returns>
        public override bool Intersects(RectangleCollider other)
        {
            Vector2 topLeft = other.shape.Location.ToVector2();
            Vector2 topRight = topLeft + new Vector2(other.shape.Width, 0);
            Vector2 bottomLeft = topLeft + new Vector2(0, other.shape.Height);
            Vector2 bottomRight = topLeft + new Vector2(other.shape.Width, other.shape.Height);

            // Define the rectangle edges as line segments
            LinePieceCollider topEdge = new LinePieceCollider(topLeft, topRight);
            LinePieceCollider bottomEdge = new LinePieceCollider(bottomLeft, bottomRight);
            LinePieceCollider leftEdge = new LinePieceCollider(topLeft, bottomLeft);
            LinePieceCollider rightEdge = new LinePieceCollider(topRight, bottomRight);

            // Use `GetIntersection()` instead of `Intersects()` for better accuracy
            return GetIntersection(topEdge) != Vector2.Zero ||
                   GetIntersection(bottomEdge) != Vector2.Zero ||
                   GetIntersection(leftEdge) != Vector2.Zero ||
                   GetIntersection(rightEdge) != Vector2.Zero;
        }



        /// <summary>
        /// Calculates the intersection point between 2 lines.
        /// </summary>
        /// <param name="Other">The line to intersect with</param>
        /// <returns>A Vector2 with the point of intersection.</returns>
        public Vector2 GetIntersection(LinePieceCollider Other)
        {
            float A1 = this.StandardA;
            float B1 = this.StandardB;
            float C1 = this.StandardC;

            float A2 = Other.StandardA;
            float B2 = Other.StandardB;
            float C2 = Other.StandardC;

            float denominator = A1 * B2 - A2 * B1;

            // Check if lines are parallel (denominator is 0)
            if (Math.Abs(denominator) < 0.0001f)
            {
                return Vector2.Zero; // No intersection or lines are collinear
            }

            // Compute intersection point using determinant formulas
            float x = (B2 * C1 - B1 * C2) / denominator;
            float y = (A1 * C2 - A2 * C1) / denominator;

            Vector2 intersection = new Vector2(x, y);

            // Check if the intersection point lies within both line segments
            if (IsPointOnSegment(intersection, this.Start, this.End) &&
                IsPointOnSegment(intersection, Other.Start, Other.End))
            {
                return intersection;
            }

            return Vector2.Zero; // No intersection within segment bounds
        }

        // Helper function to check if a point is on a segment
        private bool IsPointOnSegment(Vector2 point, Vector2 start, Vector2 end)
        {
            return point.X >= Math.Min(start.X, end.X) && point.X <= Math.Max(start.X, end.X) &&
                   point.Y >= Math.Min(start.Y, end.Y) && point.Y <= Math.Max(start.Y, end.Y);
        }

        /// <summary>
        /// Finds the nearest point on a line to a given vector, taking into account if the line is .
        /// </summary>
        /// <param name="other">The Vector you want to find the nearest point to.</param>
        /// <returns>The nearest point on the line.</returns>
        public Vector2 NearestPointOnLine(Vector2 point)
        {
            Vector2 lineDirection = End - Start;
            Vector2 startToPoint = point - Start;

            float lengthSquared = lineDirection.LengthSquared();
            float projectionFactor = Vector2.Dot(startToPoint, lineDirection) / lengthSquared;

            // Clamp the projection factor to ensure the nearest point is within the line segment
            projectionFactor = Math.Clamp(projectionFactor, 0, 1);

            // Compute the nearest point
            return Start + projectionFactor * lineDirection;
        }


        /// <summary>
        /// Returns the enclosing Axis Aligned Bounding Box containing the control points for the line.
        /// As an unbound line has infinite length, the returned bounding box assumes the line to be bound.
        /// </summary>
        /// <returns></returns>
        public override Rectangle GetBoundingBox()
        {
            Point topLeft = new Point((int)Math.Min(Start.X, End.X), (int)Math.Min(Start.Y, End.Y));
            Point size = new Point((int)Math.Max(Start.X, End.X), (int)Math.Max(Start.Y, End.X)) - topLeft;
            return new Rectangle(topLeft,size);
        }


        /// <summary>
        /// Gets whether or not the provided coordinates lie on the line.
        /// </summary>
        /// <param name="coordinates">The coordinates to check.</param>
        /// <returns>true if the coordinates are within the circle.</returns>
        public override bool Contains(Vector2 coordinates)
        {
            // TODO: Implement

            return false;
        }

        public bool Equals(LinePieceCollider other)
        {
            return other.Start == this.Start && other.End == this.End;
        }

        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public static Vector2 GetDirection(Point point1, Point point2)
        {
            return GetDirection(point1.ToVector2(), point2.ToVector2());
        }


        /// <summary>
        /// Calculates the normalized vector pointing from point1 to point2
        /// </summary>
        /// <returns> A Vector2 containing the direction from point1 to point2. </returns>
        public Vector2 GetDirection()
        {
            return GetDirection(Start, End);
        }


        /// <summary>
        /// Should return the angle between a given direction and the up vector.
        /// </summary>
        /// <param name="direction">The Vector2 pointing out from (0,0) to calculate the angle to.</param>
        /// <returns> The angle in radians between the the up vector and the direction to the cursor.</returns>
        public float GetAngle()
        {
            return GetAngle(GetDirection());
        }
    }
}
