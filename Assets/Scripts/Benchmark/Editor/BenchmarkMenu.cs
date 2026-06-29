#if UNITY_EDITOR
using CollisionDetection.Simulation;
using UnityEditor;
using UnityEngine;

namespace CollisionDetection.Benchmark.Editor
{
    public static class BenchmarkMenu
    {
        [MenuItem("Collision/Run All Benchmark Experiments")]
        static void RunAllBenchmarks() => Run(full: true);

        [MenuItem("Collision/Run Quick Benchmarks (~1 min)")]
        static void RunQuickBenchmarks() => Run(full: false);

        static void Run(bool full)
        {
            var config = AssetDatabase.LoadAssetAtPath<SimulationConfig>("Assets/Settings/SimulationConfig.asset");
            if (config == null)
            {
                Debug.LogError("Missing Assets/Settings/SimulationConfig.asset");
                return;
            }

            if (!EditorUtility.DisplayDialog(
                    full ? "Run Full Benchmarks" : "Run Quick Benchmarks",
                    full
                        ? "Full suite may take 10–30+ minutes. Unity will look frozen but is working.\n\nUse Quick Benchmarks for a fast sanity check."
                        : "Runs a reduced set of configs (~1–3 minutes).",
                    "Run",
                    "Cancel"))
                return;

            try
            {
                var runner = new GameObject("BenchmarkRunner").AddComponent<BenchmarkRunner>();
                EditorUtility.DisplayProgressBar("Benchmarks", "Starting...", 0f);
                runner.RunAllExperiments(config, quick: !full);
                Object.DestroyImmediate(runner.gameObject);
                Debug.Log($"Benchmark CSVs exported to: {CsvExporter.ResultsDirectory}");
                EditorUtility.DisplayDialog("Done", $"CSVs saved to:\n{CsvExporter.ResultsDirectory}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif
