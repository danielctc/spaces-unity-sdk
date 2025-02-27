using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using Spaces.React.Runtime;

namespace Spaces.React.Runtime
{
    [RequireComponent(typeof(BoxCollider))]
    public class SpacesVideoHolder : MonoBehaviour
    {
        [Tooltip("The size of the video holder.")]
        public Vector2 size = new Vector2(1f, 1f);

        private string objectName;

        private void Awake()
        {
            // Automatically set the GameObject's name as the identifier
            objectName = gameObject.name;

            // Ensure there is a collider for interaction
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = new Vector3(size.x, size.y, 0.1f); // Keep it thin for clicking
                collider.isTrigger = true;
            }
        }

        private void OnDrawGizmos()
        {
            // Draw a wireframe to represent the video holder
            Gizmos.color = Color.green;
            Gizmos.matrix = Matrix4x4.TRS(transform.position, transform.rotation, transform.localScale);
            Gizmos.DrawWireCube(Vector3.zero, new Vector3(size.x, size.y, 0f));
        }

        public void SetSize(Vector2 newSize)
        {
            size = newSize;

            // Update the collider size to match the new dimensions
            BoxCollider collider = GetComponent<BoxCollider>();
            if (collider != null)
            {
                collider.size = new Vector3(size.x, size.y, 0.1f);
            }
        }
    }
}
