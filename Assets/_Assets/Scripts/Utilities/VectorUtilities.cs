using UnityEngine;

namespace Project.Utilities
{
    /// <summary>
    /// A static class providing extension and utility methods for working with Vector3 in the XZ plane.
    /// </summary>
    public static class VectorUtilities
    {
        /// <summary>
        /// Returns a new Vector3 with the same X and Z components as the original,
        /// but with the Y component set to 0. Useful for flattening a vector onto the XZ plane.
        /// </summary>
        /// <param name="vector">The original Vector3.</param>
        /// <returns>A Vector3 with Y = 0.</returns>
        public static Vector3 ToXZ(this Vector3 vector)
        {
            return new Vector3(vector.x, 0, vector.z);
        }

        public static Vector3 ToXY(this Vector3 vector)
        {
            return new Vector3(vector.x, vector.y, 0);
        }

        /// <summary>
        /// Calculates the distance between two points, ignoring their Y (vertical) component.
        /// Useful for measuring horizontal distance on the XZ plane.
        /// </summary>
        /// <param name="pointA">The first point.</param>
        /// <param name="pointB">The second point.</param>
        /// <returns>The distance between the two points on the XZ plane.</returns>
        public static float DistanceXZ(Vector3 pointA, Vector3 pointB)
        {
            return Vector3.Distance(pointA.ToXZ(), pointB.ToXZ());
        }

        public static float DistanceXY(Vector3 pointA , Vector3 pointB)
        {
            return Vector3.Distance(pointA.ToXY(), pointB.ToXY());
        }
    }
}