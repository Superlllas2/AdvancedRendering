using UnityEngine;

namespace CollisionDetection.Core
{
    public static class ShapeFactory
    {
        public static ConvexPolygon CreateRegularPolygon(
            int vertexCount,
            float radius,
            Vector2 position,
            float rotation,
            Vector2 velocity,
            float angularVelocity)
        {
            vertexCount = Mathf.Clamp(vertexCount, 3, 6);
            var localVertices = new Vector2[vertexCount];
            float angleStep = Mathf.PI * 2f / vertexCount;
            float startAngle = Mathf.PI * 0.5f;

            float maxDist = 0f;
            for (int i = 0; i < vertexCount; i++)
            {
                float angle = startAngle + angleStep * i;
                var vertex = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
                localVertices[i] = vertex;
                maxDist = Mathf.Max(maxDist, vertex.magnitude);
            }

            return new ConvexPolygon
            {
                Position = position,
                Rotation = rotation,
                Velocity = velocity,
                AngularVelocity = angularVelocity,
                LocalVertices = localVertices,
                BoundingRadius = maxDist,
                IsColliding = false
            };
        }
    }
}
