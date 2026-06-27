using UnityEngine;

namespace CollisionDetection.Core
{
    public static class SatNarrowphase
    {
        static readonly Vector2[] BufferA = new Vector2[8];
        static readonly Vector2[] BufferB = new Vector2[8];

        public static bool Overlaps(ConvexPolygon a, ConvexPolygon b)
        {
            a.GetWorldVertices(BufferA);
            b.GetWorldVertices(BufferB);
            return Overlaps(BufferA, a.VertexCount, BufferB, b.VertexCount);
        }

        public static bool Overlaps(Vector2[] vertsA, int countA, Vector2[] vertsB, int countB)
        {
            if (!OverlapsOnAxes(vertsA, countA, vertsB, countB))
                return false;

            return OverlapsOnAxes(vertsB, countB, vertsA, countA);
        }

        static bool OverlapsOnAxes(Vector2[] vertsA, int countA, Vector2[] vertsB, int countB)
        {
            for (int i = 0; i < countA; i++)
            {
                Vector2 edge = vertsA[(i + 1) % countA] - vertsA[i];
                Vector2 axis = new Vector2(-edge.y, edge.x);

                Project(vertsA, countA, axis, out float minA, out float maxA);
                Project(vertsB, countB, axis, out float minB, out float maxB);

                if (maxA < minB || maxB < minA)
                    return false;
            }

            return true;
        }

        static void Project(Vector2[] verts, int count, Vector2 axis, out float min, out float max)
        {
            float dot = Vector2.Dot(verts[0], axis);
            min = dot;
            max = dot;

            for (int i = 1; i < count; i++)
            {
                dot = Vector2.Dot(verts[i], axis);
                if (dot < min) min = dot;
                if (dot > max) max = dot;
            }
        }
    }
}
