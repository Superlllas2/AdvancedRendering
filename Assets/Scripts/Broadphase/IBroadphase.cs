using System.Collections.Generic;
using CollisionDetection.Core;

namespace CollisionDetection.Broadphase
{
    public interface IBroadphase
    {
        void GetCandidatePairs(IReadOnlyList<ConvexPolygon> shapes, List<(int, int)> pairs);
    }
}
