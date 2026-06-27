using System.Collections.Generic;
using CollisionDetection.Core;
using UnityEngine;

namespace CollisionDetection.Simulation
{
    public static class SceneGenerator
    {
        public static List<ConvexPolygon> Generate(SimulationConfig config)
        {
            var random = new System.Random(config.seed);
            var shapes = new List<ConvexPolygon>(config.shapeCount);
            float extent = config.worldHalfExtent;

            for (int i = 0; i < config.shapeCount; i++)
            {
                int vertexCount = random.Next(3, 7);
                float radius = Mathf.Lerp(config.minRadius, config.maxRadius, (float)random.NextDouble());
                var position = new Vector2(
                    Lerp(random, -extent, extent),
                    Lerp(random, -extent, extent));

                float rotation = Lerp(random, 0f, Mathf.PI * 2f);
                float speed = Lerp(random, config.minSpeed, config.maxSpeed);
                float moveAngle = Lerp(random, 0f, Mathf.PI * 2f);
                var velocity = new Vector2(Mathf.Cos(moveAngle), Mathf.Sin(moveAngle)) * speed;
                float angularVelocity = Lerp(random, config.minAngularSpeed, config.maxAngularSpeed);

                shapes.Add(ShapeFactory.CreateRegularPolygon(
                    vertexCount,
                    radius,
                    position,
                    rotation,
                    velocity,
                    angularVelocity));
            }

            return shapes;
        }

        public static List<ConvexPolygon> GenerateWithVertexCount(SimulationConfig config, int vertexCount)
        {
            var random = new System.Random(config.seed);
            var shapes = new List<ConvexPolygon>(config.shapeCount);
            float extent = config.worldHalfExtent;

            for (int i = 0; i < config.shapeCount; i++)
            {
                float radius = Mathf.Lerp(config.minRadius, config.maxRadius, (float)random.NextDouble());
                var position = new Vector2(
                    Lerp(random, -extent, extent),
                    Lerp(random, -extent, extent));

                float rotation = Lerp(random, 0f, Mathf.PI * 2f);
                float speed = Lerp(random, config.minSpeed, config.maxSpeed);
                float moveAngle = Lerp(random, 0f, Mathf.PI * 2f);
                var velocity = new Vector2(Mathf.Cos(moveAngle), Mathf.Sin(moveAngle)) * speed;
                float angularVelocity = Lerp(random, config.minAngularSpeed, config.maxAngularSpeed);

                shapes.Add(ShapeFactory.CreateRegularPolygon(
                    vertexCount,
                    radius,
                    position,
                    rotation,
                    velocity,
                    angularVelocity));
            }

            return shapes;
        }

        static float Lerp(System.Random random, float min, float max)
        {
            return Mathf.Lerp(min, max, (float)random.NextDouble());
        }
    }
}
