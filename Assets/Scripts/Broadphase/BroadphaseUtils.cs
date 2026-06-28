using System.Collections.Generic;
using CollisionDetection.Core;
using UnityEngine;

namespace CollisionDetection.Broadphase
{
    public static class BroadphaseUtils
    {
        public static bool CirclesOverlap(ConvexPolygon a, ConvexPolygon b)
        {
            float radiusSum = a.BoundingRadius + b.BoundingRadius;
            return (a.Position - b.Position).sqrMagnitude <= radiusSum * radiusSum;
        }

        public static void AddPair(int i, int j, HashSet<long> seen, List<(int, int)> pairs)
        {
            if (i > j)
                (i, j) = (j, i);

            long key = ((long)i << 32) | (uint)j;
            if (seen.Add(key))
                pairs.Add((i, j));
        }
    }
}
