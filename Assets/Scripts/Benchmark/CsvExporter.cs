using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CollisionDetection.Metrics;

namespace CollisionDetection.Benchmark
{
    public static class CsvExporter
    {
        public static string ResultsDirectory =>
            Path.Combine(UnityEngine.Application.persistentDataPath, "BenchmarkResults");

        public static void ExportRow(BenchmarkResultRow row)
        {
            Directory.CreateDirectory(ResultsDirectory);
            string path = Path.Combine(ResultsDirectory, $"{row.Experiment}.csv");
            bool writeHeader = !File.Exists(path);

            using var writer = new StreamWriter(path, append: true, Encoding.UTF8);
            if (writeHeader)
                writer.WriteLine(BenchmarkResultRow.Header);

            writer.WriteLine(row.ToCsv());
        }
    }

    [Serializable]
    public struct BenchmarkResultRow
    {
        public string Experiment;
        public int Seed;
        public int ObjectCount;
        public string Broadphase;
        public float GridCellSize;
        public int QuadtreeMaxDepth;
        public int QuadtreeMaxPerNode;
        public int VertexCount;
        public int Repetition;
        public double MeanNarrowphaseChecks;
        public double StdNarrowphaseChecks;
        public double MeanCollisionMs;
        public double StdCollisionMs;
        public double MeanCollisionCount;
        public double StdCollisionCount;

        public const string Header =
            "experiment,seed,object_count,broadphase,grid_cell_size,quadtree_max_depth,quadtree_max_per_node,vertex_count,repetition,mean_narrowphase_checks,std_narrowphase_checks,mean_collision_ms,std_collision_ms,mean_collision_count,std_collision_count";

        public string ToCsv()
        {
            return string.Join(",",
                Experiment,
                Seed,
                ObjectCount,
                Broadphase,
                GridCellSize.ToString("F4"),
                QuadtreeMaxDepth,
                QuadtreeMaxPerNode,
                VertexCount,
                Repetition,
                MeanNarrowphaseChecks.ToString("F4"),
                StdNarrowphaseChecks.ToString("F4"),
                MeanCollisionMs.ToString("F6"),
                StdCollisionMs.ToString("F6"),
                MeanCollisionCount.ToString("F4"),
                StdCollisionCount.ToString("F4"));
        }
    }
}
