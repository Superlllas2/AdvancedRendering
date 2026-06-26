using CollisionDetection.Broadphase;
using UnityEngine;

namespace CollisionDetection.Simulation
{
    [CreateAssetMenu(fileName = "SimulationConfig", menuName = "Collision/Simulation Config")]
    public class SimulationConfig : ScriptableObject
    {
        public int seed = 42;
        public int shapeCount = 20;
        public float worldHalfExtent = 8f;
        public float minRadius = 0.2f;
        public float maxRadius = 0.5f;
        public float minSpeed = 0.5f;
        public float maxSpeed = 2f;
        public float minAngularSpeed = -2f;
        public float maxAngularSpeed = 2f;
        public float fixedTimestep = 1f / 60f;
        public BroadphaseMode broadphaseMode = BroadphaseMode.BruteForce;
        public float gridCellSize = 1.5f;
        public int quadtreeMaxDepth = 8;
        public int quadtreeMaxPerNode = 8;
        public bool runBenchmarkOnStart;
        public bool enableDebugDraw = true;
        public int maxDrawCount = 500;
        public bool validateBroadphaseEquality = true;
    }
}
