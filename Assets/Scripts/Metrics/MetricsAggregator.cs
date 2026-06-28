using System;
using System.Collections.Generic;

namespace CollisionDetection.Metrics
{
    public static class MetricsAggregator
    {
        public static void ComputeStats(IReadOnlyList<FrameMetrics> frames, out double meanChecks, out double stdChecks,
            out double meanMs, out double stdMs, out double meanCollisions, out double stdCollisions)
        {
            int n = frames.Count;
            if (n == 0)
            {
                meanChecks = stdChecks = meanMs = stdMs = meanCollisions = stdCollisions = 0d;
                return;
            }

            double sumChecks = 0d, sumMs = 0d, sumCollisions = 0d;
            for (int i = 0; i < n; i++)
            {
                sumChecks += frames[i].NarrowphaseChecks;
                sumMs += frames[i].CollisionStepMs;
                sumCollisions += frames[i].CollisionCount;
            }

            meanChecks = sumChecks / n;
            meanMs = sumMs / n;
            meanCollisions = sumCollisions / n;

            double varChecks = 0d, varMs = 0d, varCollisions = 0d;
            for (int i = 0; i < n; i++)
            {
                varChecks += Math.Pow(frames[i].NarrowphaseChecks - meanChecks, 2);
                varMs += Math.Pow(frames[i].CollisionStepMs - meanMs, 2);
                varCollisions += Math.Pow(frames[i].CollisionCount - meanCollisions, 2);
            }

            stdChecks = Math.Sqrt(varChecks / n);
            stdMs = Math.Sqrt(varMs / n);
            stdCollisions = Math.Sqrt(varCollisions / n);
        }
    }
}
