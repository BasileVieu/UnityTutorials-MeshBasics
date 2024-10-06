using UnityEngine;

public class CircleGizmo : MonoBehaviour
{
    public int resolution = 10;

    private void OnDrawGizmosSelected()
    {
        float step = 2.0f / resolution;

        for (var i = 0; i <= resolution; i++)
        {
            ShowPoint(i * step - 1.0f, -1.0f);
            ShowPoint(i * step - 1.0f, 1.0f);
        }

        for (var i = 1; i < resolution; i++)
        {
            ShowPoint(-1.0f, i * step - 1.0f);
            ShowPoint(1.0f, i * step - 1.0f);
        }
    }

    private void ShowPoint(float _x, float _y)
    {
        var square = new Vector2(_x, _y);
        Vector2 circle;
        circle.x = square.x * Mathf.Sqrt(1.0f - square.y * square.y * 0.5f);
        circle.y = square.y * Mathf.Sqrt(1.0f - square.x * square.x * 0.5f);

        Gizmos.color = Color.black;
        Gizmos.DrawSphere(square, 0.025f);

        Gizmos.color = Color.white;
        Gizmos.DrawSphere(circle, 0.025f);

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(square, circle);

        Gizmos.color = Color.gray;
        Gizmos.DrawLine(circle, Vector2.zero);
    }
}