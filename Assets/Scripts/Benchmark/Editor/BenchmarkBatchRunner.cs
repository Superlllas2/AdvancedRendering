#if UNITY_EDITOR
using CollisionDetection.Benchmark;
using CollisionDetection.Simulation;
using UnityEditor;
using UnityEngine;

namespace CollisionDetection.Benchmark.Editor
{
    public static class BenchmarkBatchRunner
    {
        public static void RunAllFromCommandLine()
        {
            var config = AssetDatabase.LoadAssetAtPath<SimulationConfig>("Assets/Settings/SimulationConfig.asset");
            if (config == null)
            {
                Debug.LogError("SimulationConfig asset missing.");
                EditorApplication.Exit(1);
                return;
            }

            var runner = new GameObject("BenchmarkRunner").AddComponent<BenchmarkRunner>();
            runner.RunAllExperiments(config);
            Object.DestroyImmediate(runner.gameObject);
            Debug.Log($"Benchmark CSVs exported to: {CsvExporter.ResultsDirectory}");
            EditorApplication.Exit(0);
        }
    }
}
#endif
