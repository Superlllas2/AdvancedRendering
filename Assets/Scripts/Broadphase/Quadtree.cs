using System.Collections.Generic;
using CollisionDetection.Core;
using UnityEngine;

namespace CollisionDetection.Broadphase
{
    public class Quadtree : IBroadphase
    {
        readonly float worldMinX;
        readonly float worldMinY;
        readonly float worldMaxX;
        readonly float worldMaxY;
        readonly int maxDepth;
        readonly int maxPerNode;
        readonly Node root;
        readonly HashSet<long> seenPairs = new();

        public Quadtree(
            float worldMinX,
            float worldMinY,
            float worldMaxX,
            float worldMaxY,
            int maxDepth = 8,
            int maxPerNode = 8)
        {
            this.worldMinX = worldMinX;
            this.worldMinY = worldMinY;
            this.worldMaxX = worldMaxX;
            this.worldMaxY = worldMaxY;
            this.maxDepth = maxDepth;
            this.maxPerNode = maxPerNode;
            root = new Node(worldMinX, worldMinY, worldMaxX, worldMaxY, 0);
        }

        public void GetCandidatePairs(IReadOnlyList<ConvexPolygon> shapes, List<(int, int)> pairs)
        {
            pairs.Clear();
            seenPairs.Clear();
            root.Clear();
            root.SetBounds(worldMinX, worldMinY, worldMaxX, worldMaxY);

            for (int i = 0; i < shapes.Count; i++)
                root.Insert(i, shapes[i], shapes, maxDepth, maxPerNode);

            for (int i = 0; i < shapes.Count; i++)
                root.QueryPairs(i, shapes[i], shapes, seenPairs, pairs);
        }

        sealed class Node
        {
            float minX, minY, maxX, maxY;
            readonly int depth;
            readonly List<int> indices = new();
            Node nw, ne, sw, se;

            public Node(float minX, float minY, float maxX, float maxY, int depth)
            {
                SetBounds(minX, minY, maxX, maxY);
                this.depth = depth;
            }

            public void SetBounds(float minX, float minY, float maxX, float maxY)
            {
                this.minX = minX;
                this.minY = minY;
                this.maxX = maxX;
                this.maxY = maxY;
            }

            public void Clear()
            {
                indices.Clear();
                nw = ne = sw = se = null;
            }

            bool IsLeaf => nw == null;

            public void Insert(
                int index,
                ConvexPolygon shape,
                IReadOnlyList<ConvexPolygon> shapes,
                int maxDepth,
                int maxPerNode)
            {
                if (!Overlaps(shape))
                    return;

                if (IsLeaf)
                {
                    indices.Add(index);
                    if (indices.Count > maxPerNode && depth < maxDepth)
                        Subdivide(shapes, maxDepth, maxPerNode);
                    return;
                }

                nw.Insert(index, shape, shapes, maxDepth, maxPerNode);
                ne.Insert(index, shape, shapes, maxDepth, maxPerNode);
                sw.Insert(index, shape, shapes, maxDepth, maxPerNode);
                se.Insert(index, shape, shapes, maxDepth, maxPerNode);
            }

            void Subdivide(IReadOnlyList<ConvexPolygon> shapes, int maxDepth, int maxPerNode)
            {
                float midX = (minX + maxX) * 0.5f;
                float midY = (minY + maxY) * 0.5f;
                nw = new Node(minX, midY, midX, maxY, depth + 1);
                ne = new Node(midX, midY, maxX, maxY, depth + 1);
                sw = new Node(minX, minY, midX, midY, depth + 1);
                se = new Node(midX, minY, maxX, midY, depth + 1);

                var existing = new List<int>(indices);
                indices.Clear();

                for (int i = 0; i < existing.Count; i++)
                {
                    int index = existing[i];
                    Insert(index, shapes[index], shapes, maxDepth, maxPerNode);
                }
            }

            public void QueryPairs(
                int index,
                ConvexPolygon shape,
                IReadOnlyList<ConvexPolygon> shapes,
                HashSet<long> seen,
                List<(int, int)> pairs)
            {
                if (!Overlaps(shape))
                    return;

                for (int i = 0; i < indices.Count; i++)
                {
                    int other = indices[i];
                    if (other == index)
                        continue;

                    if (BroadphaseUtils.CirclesOverlap(shape, shapes[other]))
                        BroadphaseUtils.AddPair(index, other, seen, pairs);
                }

                if (!IsLeaf)
                {
                    nw.QueryPairs(index, shape, shapes, seen, pairs);
                    ne.QueryPairs(index, shape, shapes, seen, pairs);
                    sw.QueryPairs(index, shape, shapes, seen, pairs);
                    se.QueryPairs(index, shape, shapes, seen, pairs);
                }
            }

            bool Overlaps(ConvexPolygon shape)
            {
                float r = shape.BoundingRadius;
                float cx = shape.Position.x;
                float cy = shape.Position.y;
                return cx + r >= minX && cx - r <= maxX && cy + r >= minY && cy - r <= maxY;
            }
        }
    }
}
