using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class RoundedCube : MonoBehaviour
{
    public int xSize;
    public int ySize;
    public int zSize;
    public int roundness;

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
        mesh.name = "Procedural Cube";

        CreateVertices();
        CreateTriangles();
        CreateColliders();
    }

    private void CreateVertices()
    {
        var cornerVertices = 8;
        int edgeVertices = (xSize + ySize + zSize - 3) * 4;
        int faceVertices = ((xSize - 1) * (ySize - 1) + (xSize - 1) * (zSize - 1) + (ySize - 1) * (zSize - 1)) * 2;

        vertices = new Vector3[cornerVertices + edgeVertices + faceVertices];
        normals = new Vector3[vertices.Length];
        cubeUV = new Color32[vertices.Length];

        var v = 0;

        for (var y = 0; y <= ySize; y++)
        {
            for (var x = 0; x <= xSize; x++)
            {
                SetVertex(v++, x, y, 0);
            }

            for (var z = 1; z <= zSize; z++)
            {
                SetVertex(v++, xSize, y, z);
            }

            for (int x = xSize - 1; x >= 0; x--)
            {
                SetVertex(v++, x, y, zSize);
            }

            for (int z = zSize - 1; z > 0; z--)
            {
                SetVertex(v++, 0, y, z);
            }
        }

        for (var z = 1; z < zSize; z++)
        {
            for (var x = 1; x < xSize; x++)
            {
                SetVertex(v++, x, ySize, z);
            }
        }

        for (var z = 1; z < zSize; z++)
        {
            for (var x = 1; x < xSize; x++)
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
        Vector3 inner = vertices[_i] = new Vector3(_x, _y, _z);

        if (_x < roundness)
        {
            inner.x = roundness;
        }
        else if (_x > xSize - roundness)
        {
            inner.x = xSize - roundness;
        }

        if (_y < roundness)
        {
            inner.y = roundness;
        }
        else if (_y > ySize - roundness)
        {
            inner.y = ySize - roundness;
        }

        if (_z < roundness)
        {
            inner.z = roundness;
        }
        else if (_z > zSize - roundness)
        {
            inner.z = zSize - roundness;
        }

        normals[_i] = (vertices[_i] - inner).normalized;
        vertices[_i] = inner + normals[_i] * roundness;
        cubeUV[_i] = new Color32((byte)_x, (byte)_y, (byte)_z, 0);
    }

    private void CreateTriangles()
    {
        var trianglesZ = new int[(xSize * ySize) * 12];
        var trianglesX = new int[(ySize * zSize) * 12];
        var trianglesY = new int[(xSize * zSize) * 12];
        int ring = (xSize + zSize) * 2;
        var tX = 0;
        var tY = 0;
        var tZ = 0;
        var v = 0;

        for (var y = 0; y < ySize; y++, v++)
        {
            for (var q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < zSize; q++, v++)
            {
                tX = SetQuad(trianglesX, tX, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < xSize; q++, v++)
            {
                tZ = SetQuad(trianglesZ, tZ, v, v + 1, v + ring, v + ring + 1);
            }

            for (var q = 0; q < zSize - 1; q++, v++)
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
        int v = _ring * ySize;

        for (var x = 0; x < xSize - 1; x++, v++)
        {
            _t = SetQuad(_triangles, _t, v, v + 1, v + _ring - 1, v + _ring);
        }

        _t = SetQuad(_triangles, _t, v, v + 1, v + _ring - 1, v + 2);

        int vMin = _ring * (ySize + 1) - 1;
        int vMid = vMin + 1;
        int vMax = v + 2;

        for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            _t = SetQuad(_triangles, _t, vMin, vMid, vMin - 1, vMid + xSize - 1);

            for (var x = 1; x < xSize - 1; x++, vMid++)
            {
                _t = SetQuad(_triangles, _t, vMid, vMid + 1, vMid + xSize - 1, vMid + xSize);
            }

            _t = SetQuad(_triangles, _t, vMid, vMax, vMid + xSize - 1, vMax + 1);
        }

        int vTop = vMin - 2;

        _t = SetQuad(_triangles, _t, vMin, vMid, vMin - 1, vMin - 2);

        for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            _t = SetQuad(_triangles, _t, vMid, vMid + 1, vTop, vTop - 1);
        }

        _t = SetQuad(_triangles, _t, vMid, vTop - 2, vTop, vTop - 1);

        return _t;
    }

    private int CreateBottomFace(int[] _triangles, int _t, int _ring)
    {
        var v = 1;

        int vMid = vertices.Length - (xSize - 1) * (zSize - 1);

        _t = SetQuad(_triangles, _t, _ring - 1, vMid, 0, 1);

        for (var x = 1; x < xSize - 1; x++, v++, vMid++)
        {
            _t = SetQuad(_triangles, _t, vMid, vMid + 1, v, v + 1);
        }

        _t = SetQuad(_triangles, _t, vMid, v + 2, v, v + 1);

        int vMin = _ring - 2;
        vMid -= xSize - 2;
        int vMax = v + 2;

        for (var z = 1; z < zSize - 1; z++, vMin--, vMid++, vMax++)
        {
            _t = SetQuad(_triangles, _t, vMin, vMid + xSize - 1, vMin + 1, vMid);

            for (var x = 1; x < xSize - 1; x++, vMid++)
            {
                _t = SetQuad(_triangles, _t, vMid + xSize - 1, vMid + xSize, vMid, vMid + 1);
            }

            _t = SetQuad(_triangles, _t, vMid + xSize - 1, vMax + 1, vMid, vMax);
        }

        int vTop = vMin - 1;

        _t = SetQuad(_triangles, _t, vTop + 1, vTop, vTop + 2, vMid);

        for (var x = 1; x < xSize - 1; x++, vTop--, vMid++)
        {
            _t = SetQuad(_triangles, _t, vTop, vTop - 1, vMid, vMid + 1);
        }

        _t = SetQuad(_triangles, _t, vTop, vTop - 1, vMid, vTop - 2);

        return _t;
    }

    private void AddBoxCollider(float _x, float _y, float _z)
    {
        var c = gameObject.AddComponent<BoxCollider>();
        c.size = new Vector3(_x, _y, _z);
    }

    private void AddCapsuleCollider(int _direction, float _x, float _y, float _z)
    {
        var c = gameObject.AddComponent<CapsuleCollider>();
        c.center = new Vector3(_x, _y, _z);
        c.direction = _direction;
        c.radius = roundness;
        c.height = c.center[_direction] * 2.0f;
    }

    private void CreateColliders()
    {
        AddBoxCollider(xSize, ySize - roundness * 2, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize, zSize - roundness * 2);
        AddBoxCollider(xSize - roundness * 2, ySize - roundness * 2, zSize);

        Vector3 min = Vector3.one * roundness;
        Vector3 half = new Vector3(xSize, ySize, zSize) * 0.5f;
        Vector3 max = new Vector3(xSize, ySize, zSize) - min;

        AddCapsuleCollider(0, half.x, min.y, min.z);
        AddCapsuleCollider(0, half.x, min.y, max.z);
        AddCapsuleCollider(0, half.x, max.y, min.z);
        AddCapsuleCollider(0, half.x, max.y, max.z);

        AddCapsuleCollider(1, min.x, half.y, min.z);
        AddCapsuleCollider(1, min.x, half.y, max.z);
        AddCapsuleCollider(1, max.x, half.y, min.z);
        AddCapsuleCollider(1, max.x, half.y, max.z);

        AddCapsuleCollider(2, min.x, min.y, half.z);
        AddCapsuleCollider(2, min.x, max.y, half.z);
        AddCapsuleCollider(2, max.x, min.y, half.z);
        AddCapsuleCollider(2, max.x, max.y, half.z);
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