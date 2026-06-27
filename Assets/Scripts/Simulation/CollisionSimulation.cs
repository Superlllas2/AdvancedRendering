using System.Collections.Generic;
using System.Diagnostics;
using CollisionDetection.Broadphase;
using CollisionDetection.Core;
using CollisionDetection.Metrics;
using UnityEngine;
using Debug = UnityEngine.Debug;

namespace CollisionDetection.Simulation
{
    public class CollisionSimulation : MonoBehaviour
    {
        [SerializeField] SimulationConfig config;

        readonly List<ConvexPolygon> shapes = new();
        readonly List<(int, int)> candidatePairs = new();
        readonly List<(int, int)> brutePairs = new();
        readonly BruteForceBroadphase bruteForce = new();
        SpatialHashGrid spatialHash;
        Quadtree quadtree;
        IBroadphase activeBroadphase;

        float accumulator;
        FrameMetrics lastFrameMetrics;

        public IReadOnlyList<ConvexPolygon> Shapes => shapes;
        public FrameMetrics LastFrameMetrics => lastFrameMetrics;
        public SimulationConfig Config => config;

        void Awake()
        {
#if UNITY_EDITOR
            if (config == null)
                config = UnityEditor.AssetDatabase.LoadAssetAtPath<SimulationConfig>("Assets/Settings/SimulationConfig.asset");
#endif
            if (config == null)
            {
                Debug.LogError("CollisionSimulation requires a SimulationConfig.");
                enabled = false;
                return;
            }

            InitializeBroadphases();
            ResetScene();
            RunSatValidation();
        }

        void Start()
        {
            if (config.runBenchmarkOnStart)
            {
                var runner = GetComponent<Benchmark.BenchmarkRunner>();
                if (runner == null)
                    runner = gameObject.AddComponent<Benchmark.BenchmarkRunner>();
                runner.RunAllExperiments(config);
            }
        }

        void Update()
        {
            accumulator += Time.deltaTime;
            float dt = config.fixedTimestep;

            while (accumulator >= dt)
            {
                StepSimulation(dt);
                accumulator -= dt;
            }
        }

        public void ResetScene()
        {
            shapes.Clear();
            shapes.AddRange(SceneGenerator.Generate(config));
            accumulator = 0f;
        }

        void InitializeBroadphases()
        {
            float extent = config.worldHalfExtent;
            spatialHash = new SpatialHashGrid(config.gridCellSize);
            quadtree = new Quadtree(-extent, -extent, extent, extent, config.quadtreeMaxDepth, config.quadtreeMaxPerNode);
            SetBroadphaseMode(config.broadphaseMode);
        }

        public void SetBroadphaseMode(BroadphaseMode mode)
        {
            activeBroadphase = mode switch
            {
                BroadphaseMode.SpatialHashGrid => spatialHash,
                BroadphaseMode.Quadtree => quadtree,
                _ => bruteForce
            };
        }

        void StepSimulation(float dt)
        {
            for (int i = 0; i < shapes.Count; i++)
                shapes[i].Step(dt);

            WrapPositions();

            for (int i = 0; i < shapes.Count; i++)
                shapes[i].IsColliding = false;

            var stopwatch = Stopwatch.StartNew();
            activeBroadphase.GetCandidatePairs(shapes, candidatePairs);

            int collisionCount = 0;
            int narrowphaseChecks = 0;

            for (int p = 0; p < candidatePairs.Count; p++)
            {
                var (i, j) = candidatePairs[p];
                narrowphaseChecks++;
                if (SatNarrowphase.Overlaps(shapes[i], shapes[j]))
                {
                    shapes[i].IsColliding = true;
                    shapes[j].IsColliding = true;
                    collisionCount++;
                }
            }

            stopwatch.Stop();

            if (config.validateBroadphaseEquality && activeBroadphase != bruteForce)
                ValidateAgainstBruteForce(collisionCount);

            lastFrameMetrics = new FrameMetrics
            {
                NarrowphaseChecks = narrowphaseChecks,
                CollisionCount = collisionCount,
                CollisionStepMs = stopwatch.Elapsed.TotalMilliseconds
            };
        }

        void ValidateAgainstBruteForce(int collisionCount)
        {
            bruteForce.GetCandidatePairs(shapes, brutePairs);
            int bruteCollisions = 0;

            for (int p = 0; p < brutePairs.Count; p++)
            {
                var (i, j) = brutePairs[p];
                if (SatNarrowphase.Overlaps(shapes[i], shapes[j]))
                    bruteCollisions++;
            }

            if (bruteCollisions != collisionCount)
            {
                Debug.LogError(
                    $"Broadphase mismatch: active={collisionCount}, brute={bruteCollisions}, mode={config.broadphaseMode}");
            }
        }

        void WrapPositions()
        {
            float extent = config.worldHalfExtent;
            for (int i = 0; i < shapes.Count; i++)
            {
                var shape = shapes[i];
                if (shape.Position.x > extent) shape.Position.x = -extent;
                else if (shape.Position.x < -extent) shape.Position.x = extent;

                if (shape.Position.y > extent) shape.Position.y = -extent;
                else if (shape.Position.y < -extent) shape.Position.y = extent;
            }
        }

        void RunSatValidation()
        {
            var squareA = ShapeFactory.CreateRegularPolygon(4, 1f, Vector2.zero, 0f, Vector2.zero, 0f);
            var squareB = ShapeFactory.CreateRegularPolygon(4, 1f, new Vector2(1.5f, 0f), 0f, Vector2.zero, 0f);
            var squareSeparated = ShapeFactory.CreateRegularPolygon(4, 1f, new Vector2(3f, 0f), 0f, Vector2.zero, 0f);

            Debug.Assert(SatNarrowphase.Overlaps(squareA, squareB), "SAT: overlapping squares should collide");
            Debug.Assert(!SatNarrowphase.Overlaps(squareA, squareSeparated), "SAT: separated squares should not collide");

            squareB.Rotation = Mathf.PI * 0.25f;
            Debug.Assert(SatNarrowphase.Overlaps(squareA, squareB), "SAT: rotated overlap should collide");

            squareB.Position = new Vector2(3f, 0f);
            squareB.Rotation = Mathf.PI * 0.25f;
            Debug.Assert(!SatNarrowphase.Overlaps(squareA, squareB), "SAT: rotated separated should not collide");
        }
    }
}
