namespace CollisionDetection.Metrics
{
    public struct FrameMetrics
    {
        public int NarrowphaseChecks;
        public int CollisionCount;
        public double CollisionStepMs;

        public void Reset()
        {
            NarrowphaseChecks = 0;
            CollisionCount = 0;
            CollisionStepMs = 0d;
        }
    }
}
