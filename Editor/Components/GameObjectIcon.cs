using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class GameObjectIcon : MonoBehaviour
{
    [SerializeField] private Texture2D icon;
    [SerializeField] private Vector3 offset = Vector3.zero;
    [SerializeField] private float sphereRadius = 0.5f;
    [SerializeField] private float arrowLength = 1f;
    [SerializeField] private float arrowThickness = 0.1f;  // New parameter for arrow thickness
    [SerializeField] private Color sphereColor = new Color(0f, 1f, 0f, 0.2f); // Semi-transparent green
    [SerializeField] private Color arrowColor = new Color(0f, 0.7f, 1f, 1f);  // Light blue

#if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (icon == null) return;
        
        // Only show in edit mode
        if (Application.isPlaying) return;

        // Set the icon for this game object in the editor
        var content = new GUIContent(icon);
        EditorGUIUtility.SetIconForObject(gameObject, icon);

        // Draw Z-axis arrow (always visible)
        DrawDirectionalArrow();
    }

    private void OnDrawGizmosSelected()
    {
        if (Application.isPlaying) return;

        // Draw rotation sphere only when selected
        Gizmos.color = sphereColor;
        Gizmos.DrawWireSphere(transform.position, sphereRadius);
        
        // Draw circles to represent rotation
        DrawRotationCircles();
    }

    private void DrawRotationCircles()
    {
        // Draw three circles to represent rotation in different axes
        int segments = 32;
        Vector3 center = transform.position;

        // Draw circles in all three axes
        DrawCircle(center, transform.right, transform.up, segments, sphereRadius);
        DrawCircle(center, transform.up, transform.forward, segments, sphereRadius);
        DrawCircle(center, transform.forward, transform.right, segments, sphereRadius);
    }

    private void DrawCircle(Vector3 center, Vector3 normal, Vector3 forward, int segments, float radius)
    {
        Gizmos.color = sphereColor;
        float angleStep = 360f / segments;
        
        for (int i = 0; i < segments; i++)
        {
            float angle1 = angleStep * i * Mathf.Deg2Rad;
            float angle2 = angleStep * (i + 1) * Mathf.Deg2Rad;

            Vector3 point1 = center + (forward * Mathf.Cos(angle1) + Vector3.Cross(normal, forward) * Mathf.Sin(angle1)) * radius;
            Vector3 point2 = center + (forward * Mathf.Cos(angle2) + Vector3.Cross(normal, forward) * Mathf.Sin(angle2)) * radius;

            Gizmos.DrawLine(point1, point2);
        }
    }

    private void DrawDirectionalArrow()
    {
        Gizmos.color = arrowColor;
        Vector3 start = transform.position;
        Vector3 end = start + transform.forward * arrowLength;
        
        // Draw main line with thickness
        DrawThickLine(start, end, arrowThickness);
        
        // Draw arrow head
        float headLength = arrowLength * 0.2f;
        float headWidth = headLength * 0.7f;  // Made head wider
        Vector3 right = transform.right * headWidth;
        Vector3 up = transform.up * headWidth;
        
        // Draw thicker arrow head lines
        DrawThickLine(end, end - transform.forward * headLength + right, arrowThickness);
        DrawThickLine(end, end - transform.forward * headLength - right, arrowThickness);
        DrawThickLine(end, end - transform.forward * headLength + up, arrowThickness);
        DrawThickLine(end, end - transform.forward * headLength - up, arrowThickness);
    }

    private void DrawThickLine(Vector3 start, Vector3 end, float thickness)
    {
        // Draw multiple lines to create thickness effect
        Vector3 right = Vector3.Cross(end - start, Camera.current.transform.forward).normalized * thickness;
        Vector3 up = Vector3.Cross(right, end - start).normalized * thickness;

        // Draw main line
        Gizmos.DrawLine(start, end);

        // Draw additional lines for thickness
        Gizmos.DrawLine(start + right, end + right);
        Gizmos.DrawLine(start - right, end - right);
        Gizmos.DrawLine(start + up, end + up);
        Gizmos.DrawLine(start - up, end - up);

        // Draw connecting lines
        Gizmos.DrawLine(start + right, start - right);
        Gizmos.DrawLine(start + up, start - up);
        Gizmos.DrawLine(end + right, end - right);
        Gizmos.DrawLine(end + up, end - up);
    }

    private void OnDestroy()
    {
        // Clean up the icon when the component is removed
        EditorGUIUtility.SetIconForObject(gameObject, null);
    }
#endif
} 