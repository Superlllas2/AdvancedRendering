using System.Collections.Generic;
using CollisionDetection.Core;

namespace CollisionDetection.Broadphase
{
    public class BruteForceBroadphase : IBroadphase
    {
        public void GetCandidatePairs(IReadOnlyList<ConvexPolygon> shapes, List<(int, int)> pairs)
        {
            pairs.Clear();
            int count = shapes.Count;

            for (int i = 0; i < count; i++)
            {
                for (int j = i + 1; j < count; j++)
                {
                    if (BroadphaseUtils.CirclesOverlap(shapes[i], shapes[j]))
                        pairs.Add((i, j));
                }
            }
        }
    }
}
