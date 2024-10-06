using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class CubeSphere : MonoBehaviour
{
    public int gridSize;

    public float radius = 1;

    private Vector3[] vertices;
    private Vector3[] normals;

    private Mesh mesh;

    private Color32[] cubeUV;

    private void Awake()
    {
        Generate();
    }

    private void Generate()
    {
        GetComponent<MeshFilter>().mesh = mesh = new Mesh();
        mesh.name = "Procedural Sphere";

        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateVertices()
    {
        var cornerVertices = 8;
        int edgeVertices = (gridSize + gridSize + gridSize - 3) * 4;
        int faceVertices = ((gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1) + (gridSize - 1) * (gridSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        var v = 0;

        for (var y = 0; y <= gridSize; y++)
        {
            for (var x = 0; x <= gridSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }

            for (var z = 1; z <= gridSize; z++)
            {
                SetVertex(v++, gridSize, y, z);
            }

            for (int x = gridSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, gridSize);
            }

            for (int z = gridSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

        for (var z = 1; z < gridSize; z++)
        {
            for (var x = 1; x < gridSize; x++)
            {
                SetVertex(v++, x, gridSize, z);
            }
        }

        for (var z = 1; z < gridSize; z++)
        {
            for (var x = 1; x < gridSize; x++)
            {
                SetVertex(v++, x, 0, z);
            }
        }

        mesh.vertices = vertices;
        mesh.normals = normals;
        mesh.colors32 = cubeUV;
    }

    private void SetVertex(int _i, int _x, int _y, int _z)
    {
        Vector3 v = new Vector3(_x, _y, _z) * 2.0f / gridSize - Vector3.one;

        float x2 = v.x * v.x;
        float y2 = v.y * v.y;
        float z2 = v.z * v.z;

        Vector3 s;
        s.x = v.x * Mathf.Sqrt(1.0f - y2 / 2.0f - z2 / 2.0f + y2 * z2 / 3.0f);
        s.y = v.y * Mathf.Sqrt(1.0f - x2 / 2.0f - z2 / 2.0f + x2 * z2 / 3.0f);
        s.z = v.z * Mathf.Sqrt(1.0f - x2 / 2.0f - y2 / 2.0f + x2 * y2 / 3.0f);

        normals[_i] = s;
        vertices[_i] = normals[_i] * radius;

        cubeUV[_i] = new Color32((byte)_x, (byte)_y, (byte)_z, 0);
    }

    private void CreateTriangles()
    {
        var trianglesZ = new int[(gridSize * gridSize) * 12];
        var trianglesX = new int[(gridSize * gridSize) * 12];
        var trianglesY = new int[(gridSize * gridSize) * 12];
        int ring = (gridSize + gridSize) * 2;
        var tX = 0;
        var tY = 0;
        var tZ = 0;
        var v = 0;

        for (var y = 0; y < gridSize; y++, v++)
        {
            for (var q = 0; q < gridSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < gridSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < gridSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < gridSize - 1; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }

            tX = SetQuad(trianglesX, tX, v, v - ring + 1, v + ring, v + 1);
        }

        tY = CreateTopFace(trianglesY, tY, ring);
        tY = CreateBottomFace(trianglesY, tY, ring);

        mesh.subMeshCount = 3;
        mesh.SetTriangles(trianglesZ, 0);
        mesh.SetTriangles(trianglesX, 1);
        mesh.SetTriangles(trianglesY, 2);
    }

    private static int SetQuad(int[] _triangles, int _i, int _v00, int _v10, int _v01, int _v11)
    {
        _triangles[_i] = _v00;
        _triangles[_i + 1] = _triangles[_i + 4] = _v01;
        _triangles[_i + 2] = _triangles[_i + 3] = _v10;
        _triangles[_i + 5] = _v11;

        return _i + 6;
    }

    private int CreateTopFace(int[] _triangles, int _t, int _ring)
    {
        int v = _ring * gridSize;

        for (var x = 0; x < gridSize - 1; x++, v++)
        {
            _t = SetQuad(_triangles, _t, v, v + 1, v + _ring - 1, v + _ring);
        }

        _t = SetQuad(_triangles, _t, v, v + 1, v + _ring - 1, v + 2);

        int vMin = _ring * (gridSize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (var z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            _t = SetQuad(_triangles, _t, vMin, vMid, vMin - 1, vMid + gridSize - 1);

            for (var x = 1; x < gridSize - 1; x++, vMid++)
            {
                _t = SetQuad(_triangles, _t, vMid, vMid + 1, vMid + gridSize - 1, vMid + gridSize);
            }

            _t = SetQuad(_triangles, _t, vMid, vMax, vMid + gridSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;

        _t = SetQuad(_triangles, _t, vMin, vMid, vMin - 1, vMin - 2);

        for (var x = 1; x < gridSize - 1; x++, vTop--, vMid++)
        {
            _t = SetQuad(_triangles, _t, vMid, vMid + 1, vTop, vTop - 1);
        }

        _t = SetQuad(_triangles, _t, vMid, vTop - 2, vTop, vTop - 1);

        return _t;
    }

    private int CreateBottomFace(int[] _triangles, int _t, int _ring)
    {
        var v = 1;

        int vMid = vertices.Length - (gridSize - 1) * (gridSize - 1);

        _t = SetQuad(_triangles, _t, _ring - 1, vMid, 0, 1);

        for (var x = 1; x < gridSize - 1; x++, v++, vMid++)
        {
            _t = SetQuad(_triangles, _t, vMid, vMid + 1, v, v + 1);
        }

        _t = SetQuad(_triangles, _t, vMid, v + 2, v, v + 1);

        int vMin = _ring - 2;
        vMid -= gridSize - 2;
        int vMax = v + 2;

        for (var z = 1; z < gridSize - 1; z++, vMin--, vMid++, vMax++)
        {
            _t = SetQuad(_triangles, _t, vMin, vMid + gridSize - 1, vMin + 1, vMid);

            for (var x = 1; x < gridSize - 1; x++, vMid++)
            {
                _t = SetQuad(_triangles, _t, vMid + gridSize - 1, vMid + gridSize, vMid, vMid + 1);
            }

            _t = SetQuad(_triangles, _t, vMid + gridSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;

        _t = SetQuad(_triangles, _t, vTop + 1, vTop, vTop + 2, vMid);

        for (var x = 1; x < gridSize - 1; x++, vTop--, vMid++)
        {
            _t = SetQuad(_triangles, _t, vTop, vTop - 1, vMid, vMid + 1);
        }

        _t = SetQuad(_triangles, _t, vTop, vTop - 1, vMid, vTop - 2);

        return _t;
    }

    private void CreateColliders()
    {
        gameObject.AddComponent<SphereCollider>();
    }

    /*private void OnDrawGizmos()
    {
        if (vertices == null)
        {
            return;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            Gizmos.color = Color.black;
            Gizmos.DrawSphere(vertices[i], 0.1f);
            Gizmos.color = Color.yellow;
            Gizmos.DrawRay(vertices[i], normals[i]);
        }
    }*/
}