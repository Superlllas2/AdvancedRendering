using System.Collections.Generic;
using CollisionDetection.Core;
using UnityEngine;

namespace CollisionDetection.Broadphase
{
    public class SpatialHashGrid : IBroadphase
    {
        readonly float cellSize;
        readonly float invCellSize;
        readonly Dictionary<long, List<int>> cells = new();
        readonly HashSet<long> seenPairs = new();

        public SpatialHashGrid(float cellSize)
        {
            this.cellSize = Mathf.Max(cellSize, 0.0001f);
            invCellSize = 1f / this.cellSize;
        }

        public void GetCandidatePairs(IReadOnlyList<ConvexPolygon> shapes, List<(int, int)> pairs)
        {
            pairs.Clear();
            seenPairs.Clear();
            cells.Clear();

            for (int i = 0; i < shapes.Count; i++)
                InsertIntoCells(i, shapes[i]);

            foreach (var kvp in cells)
            {
                List<int> bucket = kvp.Value;
                for (int a = 0; a < bucket.Count; a++)
                {
                    for (int b = a + 1; b < bucket.Count; b++)
                    {
                        int i = bucket[a];
                        int j = bucket[b];
                        if (BroadphaseUtils.CirclesOverlap(shapes[i], shapes[j]))
                            BroadphaseUtils.AddPair(i, j, seenPairs, pairs);
                    }
                }
            }
        }

        void InsertIntoCells(int index, ConvexPolygon shape)
        {
            GetCellRange(shape, out int minX, out int maxX, out int minY, out int maxY);

            for (int x = minX; x <= maxX; x++)
            {
                for (int y = minY; y <= maxY; y++)
                {
                    long key = CellKey(x, y);
                    if (!cells.TryGetValue(key, out List<int> bucket))
                    {
                        bucket = new List<int>();
                        cells[key] = bucket;
                    }

                    bucket.Add(index);
                }
            }
        }

        void GetCellRange(ConvexPolygon shape, out int minX, out int maxX, out int minY, out int maxY)
        {
            float r = shape.BoundingRadius;
            minX = Mathf.FloorToInt((shape.Position.x - r) * invCellSize);
            maxX = Mathf.FloorToInt((shape.Position.x + r) * invCellSize);
            minY = Mathf.FloorToInt((shape.Position.y - r) * invCellSize);
            maxY = Mathf.FloorToInt((shape.Position.y + r) * invCellSize);
        }

        static long CellKey(int x, int y) => ((long)x << 32) ^ (uint)y;
    }
}
