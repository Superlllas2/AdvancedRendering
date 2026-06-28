using System;
using System.Collections.Generic;
using CollisionDetection.Broadphase;
using CollisionDetection.Core;
using CollisionDetection.Metrics;
using CollisionDetection.Simulation;
using UnityEngine;

namespace CollisionDetection.Benchmark
{
    public class BenchmarkRunner : MonoBehaviour
    {
        [SerializeField] int warmUpFrames = 30;
        [SerializeField] int measuredFrames = 120;
        [SerializeField] int repetitions = 3;

        public void RunAllExperiments(SimulationConfig baseConfig, bool quick = false)
        {
            if (quick)
            {
                RunCrossoverExperiment(baseConfig, quickCounts);
                RunBroadphaseTuningExperiment(baseConfig, quick: true);
                RunVertexCountExperiment(baseConfig);
            }
            else
            {
                RunCrossoverExperiment(baseConfig, fullCounts);
                RunBroadphaseTuningExperiment(baseConfig);
                RunVertexCountExperiment(baseConfig);
            }

            Debug.Log($"Benchmarks complete. CSV files written to: {CsvExporter.ResultsDirectory}");
        }

        static readonly int[] fullCounts = { 100, 250, 500, 1000, 2000, 5000, 10000 };
        static readonly int[] quickCounts = { 100, 500, 1000, 2000 };

        public void RunCrossoverExperiment(SimulationConfig baseConfig, int[] counts = null)
        {
            counts ??= fullCounts;
            var modes = new[] { BroadphaseMode.BruteForce, BroadphaseMode.SpatialHashGrid, BroadphaseMode.Quadtree };

            foreach (var mode in modes)
            {
                foreach (int count in counts)
                {
                    if (mode == BroadphaseMode.BruteForce && count > 2000)
                        continue;

                    var config = CloneConfig(baseConfig);
                    config.shapeCount = count;
                    config.broadphaseMode = mode;
                    config.enableDebugDraw = false;
                    config.validateBroadphaseEquality = false;

                    RunConfig(config, "crossover", 0, baseConfig.seed);
                }
            }
        }

        public void RunBroadphaseTuningExperiment(SimulationConfig baseConfig, bool quick = false)
        {
            float[] cellSizes = quick
                ? new[] { 1f, 1.5f, 2f }
                : new[] { 0.5f, 1f, 1.5f, 2f, 3f, 4f };
            int[] maxDepths = quick ? new[] { 6, 8 } : new[] { 4, 6, 8, 10 };
            int[] maxPerNode = quick ? new[] { 8 } : new[] { 4, 8, 16 };

            var config = CloneConfig(baseConfig);
            config.shapeCount = quick ? 1000 : 2000;
            config.enableDebugDraw = false;
            config.validateBroadphaseEquality = false;

            foreach (float cell in cellSizes)
            {
                config.broadphaseMode = BroadphaseMode.SpatialHashGrid;
                config.gridCellSize = cell;
                RunConfig(config, "broadphase_tuning_grid", cell, baseConfig.seed);
            }

            foreach (int depth in maxDepths)
            {
                foreach (int perNode in maxPerNode)
                {
                    config.broadphaseMode = BroadphaseMode.Quadtree;
                    config.quadtreeMaxDepth = depth;
                    config.quadtreeMaxPerNode = perNode;
                    RunConfig(config, "broadphase_tuning_quadtree", depth, baseConfig.seed, perNode);
                }
            }
        }

        public void RunVertexCountExperiment(SimulationConfig baseConfig)
        {
            int[] vertexCounts = { 3, 4, 5, 6 };
            var config = CloneConfig(baseConfig);
            config.shapeCount = 2000;
            config.broadphaseMode = BroadphaseMode.SpatialHashGrid;
            config.enableDebugDraw = false;
            config.validateBroadphaseEquality = false;

            foreach (int vertices in vertexCounts)
            {
                RunConfig(config, "vertex_count", vertices, baseConfig.seed, vertexCount: vertices);
            }
        }

        void RunConfig(
            SimulationConfig config,
            string experiment,
            float tuningParam,
            int seed,
            int quadtreeMaxPerNode = 0,
            int vertexCount = 0)
        {
            for (int rep = 0; rep < repetitions; rep++)
            {
                var frames = Simulate(config, vertexCount);
                MetricsAggregator.ComputeStats(
                    frames,
                    out double meanChecks, out double stdChecks,
                    out double meanMs, out double stdMs,
                    out double meanCollisions, out double stdCollisions);

                CsvExporter.ExportRow(new BenchmarkResultRow
                {
                    Experiment = experiment,
                    Seed = seed,
                    ObjectCount = config.shapeCount,
                    Broadphase = config.broadphaseMode.ToString(),
                    GridCellSize = config.broadphaseMode == BroadphaseMode.SpatialHashGrid ? config.gridCellSize : tuningParam,
                    QuadtreeMaxDepth = config.broadphaseMode == BroadphaseMode.Quadtree ? (int)tuningParam : config.quadtreeMaxDepth,
                    QuadtreeMaxPerNode = quadtreeMaxPerNode > 0 ? quadtreeMaxPerNode : config.quadtreeMaxPerNode,
                    VertexCount = vertexCount,
                    Repetition = rep,
                    MeanNarrowphaseChecks = meanChecks,
                    StdNarrowphaseChecks = stdChecks,
                    MeanCollisionMs = meanMs,
                    StdCollisionMs = stdMs,
                    MeanCollisionCount = meanCollisions,
                    StdCollisionCount = stdCollisions
                });
            }
        }

        List<FrameMetrics> Simulate(SimulationConfig config, int fixedVertexCount)
        {
            var shapes = fixedVertexCount > 0
                ? SceneGenerator.GenerateWithVertexCount(config, fixedVertexCount)
                : SceneGenerator.Generate(config);

            IBroadphase broadphase = CreateBroadphase(config);
            var brute = new BruteForceBroadphase();
            var pairs = new List<(int, int)>();
            var brutePairs = new List<(int, int)>();
            var frames = new List<FrameMetrics>(measuredFrames);
            float dt = config.fixedTimestep;
            float extent = config.worldHalfExtent;

            for (int frame = 0; frame < warmUpFrames + measuredFrames; frame++)
            {
                for (int i = 0; i < shapes.Count; i++)
                {
                    shapes[i].Step(dt);
                    Wrap(shapes[i], extent);
                }

                var sw = System.Diagnostics.Stopwatch.StartNew();
                broadphase.GetCandidatePairs(shapes, pairs);

                int collisionCount = 0;
                int checks = 0;
                for (int p = 0; p < pairs.Count; p++)
                {
                    var (i, j) = pairs[p];
                    checks++;
                    if (SatNarrowphase.Overlaps(shapes[i], shapes[j]))
                        collisionCount++;
                }

                sw.Stop();

                if (config.validateBroadphaseEquality && config.broadphaseMode != BroadphaseMode.BruteForce)
                {
                    brute.GetCandidatePairs(shapes, brutePairs);
                    int bruteCollisions = 0;
                    for (int p = 0; p < brutePairs.Count; p++)
                    {
                        var (i, j) = brutePairs[p];
                        if (SatNarrowphase.Overlaps(shapes[i], shapes[j]))
                            bruteCollisions++;
                    }

                    if (bruteCollisions != collisionCount)
                    {
                        throw new InvalidOperationException(
                            $"Benchmark broadphase mismatch at frame {frame}: {collisionCount} vs {bruteCollisions}");
                    }
                }

                if (frame >= warmUpFrames)
                {
                    frames.Add(new FrameMetrics
                    {
                        NarrowphaseChecks = checks,
                        CollisionCount = collisionCount,
                        CollisionStepMs = sw.Elapsed.TotalMilliseconds
                    });
                }
            }

            return frames;
        }

        static IBroadphase CreateBroadphase(SimulationConfig config)
        {
            float extent = config.worldHalfExtent;
            return config.broadphaseMode switch
            {
                BroadphaseMode.SpatialHashGrid => new SpatialHashGrid(config.gridCellSize),
                BroadphaseMode.Quadtree => new Quadtree(-extent, -extent, extent, extent, config.quadtreeMaxDepth, config.quadtreeMaxPerNode),
                _ => new BruteForceBroadphase()
            };
        }

        static void Wrap(ConvexPolygon shape, float extent)
        {
            if (shape.Position.x > extent) shape.Position.x = -extent;
            else if (shape.Position.x < -extent) shape.Position.x = extent;

            if (shape.Position.y > extent) shape.Position.y = -extent;
            else if (shape.Position.y < -extent) shape.Position.y = extent;
        }

        static SimulationConfig CloneConfig(SimulationConfig source)
        {
            var clone = ScriptableObject.CreateInstance<SimulationConfig>();
            clone.seed = source.seed;
            clone.shapeCount = source.shapeCount;
            clone.worldHalfExtent = source.worldHalfExtent;
            clone.minRadius = source.minRadius;
            clone.maxRadius = source.maxRadius;
            clone.minSpeed = source.minSpeed;
            clone.maxSpeed = source.maxSpeed;
            clone.minAngularSpeed = source.minAngularSpeed;
            clone.maxAngularSpeed = source.maxAngularSpeed;
            clone.fixedTimestep = source.fixedTimestep;
            clone.broadphaseMode = source.broadphaseMode;
            clone.gridCellSize = source.gridCellSize;
            clone.quadtreeMaxDepth = source.quadtreeMaxDepth;
            clone.quadtreeMaxPerNode = source.quadtreeMaxPerNode;
            clone.enableDebugDraw = source.enableDebugDraw;
            clone.maxDrawCount = source.maxDrawCount;
            clone.validateBroadphaseEquality = source.validateBroadphaseEquality;
            return clone;
        }
    }
}
