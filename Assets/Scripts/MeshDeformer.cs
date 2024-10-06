using UnityEngine;

[RequireComponent(typeof(MeshFilter))]
public class MeshDeformer : MonoBehaviour
{
    private Mesh deformingMesh;

    private Vector3[] originalVertices;
    private Vector3[] displacedVertices;
    private Vector3[] vertexVelocities;

    public float springForce = 20.0f;
    public float damping = 5.0f;

    private float uniformScale = 1.0f;

    private void Start()
    {
        deformingMesh = GetComponent<MeshFilter>().mesh;

        originalVertices = deformingMesh.vertices;

        displacedVertices = new Vector3[originalVertices.Length];

        for (var i = 0; i < originalVertices.Length; i++)
        {
            displacedVertices[i] = originalVertices[i];
        }

        vertexVelocities = new Vector3[originalVertices.Length];
    }

    private void Update()
    {
        uniformScale = transform.localScale.x;

        for (var i = 0; i < displacedVertices.Length; i++)
        {
            UpdateVertex(i);
        }

        deformingMesh.vertices = displacedVertices;
        deformingMesh.RecalculateNormals();
    }

    private void UpdateVertex(int _i)
    {
        Vector3 velocity = vertexVelocities[_i];
        Vector3 displacement = displacedVertices[_i] - originalVertices[_i];
        displacement *= uniformScale;

        velocity -= displacement * springForce * Time.deltaTime;
        velocity *= 1.0f - damping * Time.deltaTime;
        vertexVelocities[_i] = velocity;

        displacedVertices[_i] += velocity * (Time.deltaTime / uniformScale);
    }

    public void AddDeformingForce(Vector3 _point, float _force)
    {
        _point = transform.InverseTransformPoint(_point);

        for (var i = 0; i < displacedVertices.Length; i++)
        {
            AddForceToVertex(i, _point, _force);
        }
    }

    private void AddForceToVertex(int _i, Vector3 _point, float _force)
    {
        Vector3 pointToVertex = displacedVertices[_i] - _point;
        pointToVertex *= uniformScale;

        float attenuateForce = _force / (1.0f + pointToVertex.sqrMagnitude);

        float velocity = attenuateForce * Time.deltaTime;

        vertexVelocities[_i] += pointToVertex.normalized * velocity;
    }
}