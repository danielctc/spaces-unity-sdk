using UnityEngine;
using TMPro;

namespace Spaces.Fusion.Runtime
{
    [RequireComponent(typeof(MeshFilter))]
    [RequireComponent(typeof(MeshRenderer))]
    public class PortalMeshGenerator : MonoBehaviour
    {
        [Range(3, 64)]
        public int segments = 32;
        public float radius = 1f;

        [Header("Label Settings")]
        public string labelText = "Portal";
        public float labelOffset = 0.01f; // Distance in front of the portal
        public float labelScale = 0.1f; // Scale of the text
        public Color labelColor = Color.white;
        [Header("Shadow Settings")]
        public Color shadowColor = new Color(0, 0, 0, 0.5f);
        public Vector2 shadowOffset = new Vector2(0.02f, -0.02f);
        public float shadowSoftness = 0.5f;

        [SerializeField]
        private MeshFilter meshFilter;
        [SerializeField]
        private MeshRenderer meshRenderer;
        private TextMeshPro tmpLabel;
        private Camera mainCamera;

        void Reset()
        {
            // This is called when the component is first added or reset
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            GenerateMesh();
        }

        void Awake()
        {
            // Get components if not set
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
            if (meshRenderer == null)
                meshRenderer = GetComponent<MeshRenderer>();
            mainCamera = Camera.main;

            if (meshFilter == null)
            {
                Debug.LogError("MeshFilter is missing!");
                return;
            }

            if (meshRenderer == null)
            {
                Debug.LogError("MeshRenderer is missing!");
                return;
            }

            // Generate the mesh if it doesn't exist
            if (meshFilter.sharedMesh == null)
            {
                GenerateMesh();
            }

            // Create and setup the TMP label
            SetupLabel();
        }

        void SetupLabel()
        {
            // Create a new GameObject for the label
            GameObject labelObj = new GameObject("PortalLabel");
            labelObj.transform.SetParent(transform);
            labelObj.transform.localPosition = Vector3.zero;

            // Add TextMeshPro component
            tmpLabel = labelObj.AddComponent<TextMeshPro>();
            tmpLabel.text = labelText;
            tmpLabel.fontSize = 36;
            tmpLabel.color = labelColor;
            tmpLabel.alignment = TextAlignmentOptions.Center;
            tmpLabel.enableWordWrapping = false;
            tmpLabel.overflowMode = TextOverflowModes.Overflow;
            
            // Setup shadow
            tmpLabel.fontSharedMaterial.EnableKeyword("UNDERLAY_ON");
            tmpLabel.fontSharedMaterial.SetColor("_UnderlayColor", shadowColor);
            tmpLabel.fontSharedMaterial.SetFloat("_UnderlayDilate", shadowSoftness);
            tmpLabel.fontSharedMaterial.SetFloat("_UnderlaySoftness", shadowSoftness);
            tmpLabel.fontSharedMaterial.SetVector("_UnderlayOffset", shadowOffset);
            
            // Force text update to get proper bounds
            tmpLabel.ForceMeshUpdate();
            
            // Scale the text
            tmpLabel.transform.localScale = Vector3.one * labelScale;
            
            // Position in front of the portal
            tmpLabel.transform.localPosition = new Vector3(0, 0, labelOffset);

            // Ensure the text renders in front
            tmpLabel.sortingOrder = 32767; // Maximum sorting order
            tmpLabel.renderer.sortingOrder = 32767;
            
            // Set render queue to be higher than the portal
            tmpLabel.material.renderQueue = 3000;
        }

        void LateUpdate()
        {
            if (tmpLabel != null && mainCamera != null)
            {
                // Make the label face the camera
                tmpLabel.transform.rotation = mainCamera.transform.rotation;
                
                // Ensure the label is always in front
                Vector3 cameraDir = (mainCamera.transform.position - transform.position).normalized;
                tmpLabel.transform.position = transform.position + cameraDir * labelOffset;
            }
        }

        void OnValidate()
        {
            // Regenerate mesh when values change in editor
            if (Application.isPlaying) return;
            
            if (meshFilter == null)
                meshFilter = GetComponent<MeshFilter>();
                
            if (meshFilter != null)
                GenerateMesh();
        }

        public void GenerateMesh()
        {
            if (meshFilter == null)
            {
                Debug.LogError("MeshFilter is missing!");
                return;
            }

            // Create new mesh
            Mesh mesh = new Mesh();
            mesh.name = "PortalMesh";

            // Generate vertices
            Vector3[] vertices = new Vector3[segments + 1];
            Vector2[] uv = new Vector2[segments + 1];
            
            // Center vertex
            vertices[0] = Vector3.zero;
            uv[0] = new Vector2(0.5f, 0.5f);

            // Outer vertices
            for (int i = 0; i < segments; i++)
            {
                float angle = (float)i / segments * Mathf.PI * 2;
                float x = Mathf.Cos(angle) * radius;
                float y = Mathf.Sin(angle) * radius;
                vertices[i + 1] = new Vector3(x, y, 0);
                
                // UV coordinates
                float u = (Mathf.Cos(angle) + 1) * 0.5f;
                float v = (Mathf.Sin(angle) + 1) * 0.5f;
                uv[i + 1] = new Vector2(u, v);
            }

            // Generate triangles
            int[] triangles = new int[segments * 3];
            for (int i = 0; i < segments; i++)
            {
                int triangleIndex = i * 3;
                triangles[triangleIndex] = 0; // Center vertex
                triangles[triangleIndex + 1] = i + 1;
                triangles[triangleIndex + 2] = (i + 1) % segments + 1;
            }

            // Assign to mesh
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.uv = uv;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();

            // Assign mesh to filter
            meshFilter.sharedMesh = mesh;

            // Debug check
            if (meshFilter.sharedMesh == null)
            {
                Debug.LogError("Failed to assign mesh to MeshFilter!");
            }
        }

        // Public method to update the label text
        public void SetLabelText(string newText)
        {
            if (tmpLabel != null)
            {
                tmpLabel.text = newText;
            }
        }
    }
} 