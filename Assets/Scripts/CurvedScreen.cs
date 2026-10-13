using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CurvedScreen : MonoBehaviour {
    public float radius = 5f;
    public float horizontalFov = 180f;
    public float verticalFov = 70f;
    public int segments = 64;

    void Awake() {
        var mesh = new Mesh();
        int cols = segments + 1;
        var verts = new Vector3[cols * 2];
        var uvs = new Vector2[cols * 2];
        var tris = new int[segments * 6];

        float halfH = radius * Mathf.Tan(verticalFov * 0.5f * Mathf.Deg2Rad);

        for (int i = 0; i < cols; i++) {
            float t = i / (float)segments;
            float ang = Mathf.Lerp(-horizontalFov / 2f, horizontalFov / 2f, t) * Mathf.Deg2Rad;
            float x = Mathf.Sin(ang) * radius;
            float z = Mathf.Cos(ang) * radius;
            verts[i] = new Vector3(x, -halfH, z);
            verts[i + cols] = new Vector3(x, halfH, z);
            uvs[i] = new Vector2(t, 0);
            uvs[i + cols] = new Vector2(t, 1);
        }

        for (int i = 0; i < segments; i++) {
            int k = i * 6;
            tris[k]     = i;
            tris[k + 1] = i + cols;
            tris[k + 2] = i + 1;
            tris[k + 3] = i + 1;
            tris[k + 4] = i + cols;
            tris[k + 5] = i + cols + 1;
        }

        mesh.vertices = verts;
        mesh.uv = uvs;
        mesh.triangles = tris;
        mesh.RecalculateNormals();
        GetComponent<MeshFilter>().mesh = mesh;
    }
}
