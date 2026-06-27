using System.Collections.Generic;
using CollisionDetection.Core;
using CollisionDetection.Simulation;
using UnityEngine;

namespace CollisionDetection.Rendering
{
    public class ShapeDebugDrawer : MonoBehaviour
    {
        [SerializeField] CollisionSimulation simulation;
        [SerializeField] Material lineMaterialAsset;
        [SerializeField] Color defaultColor = new(0.2f, 1f, 1f, 1f);
        [SerializeField] Color collisionColor = Color.red;
        [SerializeField] Color circleColor = new(1f, 1f, 1f, 0.5f);
        [SerializeField] float lineWidth = 0.04f;
        [SerializeField] int circleSegments = 32;

        readonly List<LineRenderer> polygonLines = new();
        readonly List<LineRenderer> circleLines = new();
        readonly Vector2[] vertexBuffer = new Vector2[8];
        Material lineMaterial;
        bool ownsLineMaterial;
        Transform lineRoot;

        void Awake()
        {
            if (simulation == null)
                simulation = GetComponent<CollisionSimulation>();

            lineMaterial = lineMaterialAsset != null
                ? lineMaterialAsset
                : CreateRuntimeLineMaterial();

            lineRoot = new GameObject("DebugLines").transform;
            lineRoot.SetParent(transform, false);
        }

        void OnDestroy()
        {
            if (ownsLineMaterial && lineMaterial != null)
                Destroy(lineMaterial);
            if (lineRoot != null)
                Destroy(lineRoot.gameObject);
        }

        void LateUpdate()
        {
            if (lineMaterial == null)
                return;

            if (simulation == null || simulation.Config == null || !simulation.Config.enableDebugDraw)
            {
                SetPoolActive(0);
                return;
            }

            var shapes = simulation.Shapes;
            int drawCount = Mathf.Min(shapes.Count, simulation.Config.maxDrawCount);
            EnsurePoolSize(drawCount);

            for (int i = 0; i < drawCount; i++)
                UpdateShapeLines(i, shapes[i]);

            SetPoolActive(drawCount);
        }

        void EnsurePoolSize(int count)
        {
            while (polygonLines.Count < count)
            {
                polygonLines.Add(CreateLineRenderer($"Polygon_{polygonLines.Count}", loop: true));
                circleLines.Add(CreateLineRenderer($"Circle_{circleLines.Count}", loop: true));
            }
        }

        void SetPoolActive(int activeCount)
        {
            for (int i = 0; i < polygonLines.Count; i++)
            {
                bool active = i < activeCount;
                polygonLines[i].gameObject.SetActive(active);
                circleLines[i].gameObject.SetActive(active);
            }
        }

        LineRenderer CreateLineRenderer(string name, bool loop)
        {
            var go = new GameObject(name);
            go.transform.SetParent(lineRoot, false);

            var lr = go.AddComponent<LineRenderer>();
            lr.useWorldSpace = true;
            lr.loop = loop;
            lr.widthMultiplier = lineWidth;
            lr.numCornerVertices = 2;
            lr.numCapVertices = 2;
            lr.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            lr.receiveShadows = false;
            lr.material = lineMaterial;
            lr.sortingOrder = 10;
            return lr;
        }

        void UpdateShapeLines(int index, ConvexPolygon shape)
        {
            Color polyColor = shape.IsColliding ? collisionColor : defaultColor;
            shape.GetWorldVertices(vertexBuffer);
            int count = shape.VertexCount;

            var polygon = polygonLines[index];
            polygon.positionCount = count;
            polygon.startColor = polyColor;
            polygon.endColor = polyColor;
            for (int v = 0; v < count; v++)
                polygon.SetPosition(v, new Vector3(vertexBuffer[v].x, vertexBuffer[v].y, 0f));

            var circle = circleLines[index];
            int circleCount = circleSegments + 1;
            circle.positionCount = circleCount;
            circle.startColor = circleColor;
            circle.endColor = circleColor;
            for (int s = 0; s < circleCount; s++)
            {
                float t = s / (float)circleSegments * Mathf.PI * 2f;
                circle.SetPosition(s, new Vector3(
                    shape.Position.x + Mathf.Cos(t) * shape.BoundingRadius,
                    shape.Position.y + Mathf.Sin(t) * shape.BoundingRadius,
                    0f));
            }
        }

        Material CreateRuntimeLineMaterial()
        {
            Shader shader = Shader.Find("Collision/LineUnlit")
                ?? Shader.Find("Sprites/Default")
                ?? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default")
                ?? Shader.Find("Universal Render Pipeline/Unlit");

            if (shader == null)
            {
                Debug.LogError("ShapeDebugDrawer: assign Assets/Materials/DebugLine.mat or add a compatible shader.");
                return null;
            }

            ownsLineMaterial = true;
            return new Material(shader) { hideFlags = HideFlags.HideAndDontSave };
        }
    }
}
