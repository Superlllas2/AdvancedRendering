using UnityEngine;

namespace CollisionDetection.Core
{
    public class ConvexPolygon
    {
        public Vector2 Position;
        public float Rotation;
        public Vector2 Velocity;
        public float AngularVelocity;
        public Vector2[] LocalVertices;
        public float BoundingRadius;
        public bool IsColliding;

        public int VertexCount => LocalVertices.Length;

        public void GetWorldVertices(Vector2[] buffer)
        {
            float cos = Mathf.Cos(Rotation);
            float sin = Mathf.Sin(Rotation);

            for (int i = 0; i < LocalVertices.Length; i++)
            {
                Vector2 v = LocalVertices[i];
                buffer[i] = new Vector2(
                    Position.x + v.x * cos - v.y * sin,
                    Position.y + v.x * sin + v.y * cos);
            }
        }

        public void Step(float dt)
        {
            Position += Velocity * dt;
            Rotation += AngularVelocity * dt;
        }
    }
}
