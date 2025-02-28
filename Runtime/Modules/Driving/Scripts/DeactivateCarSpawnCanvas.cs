using UnityEngine;
using UnityEngine.UI; // Include UnityEngine.UI namespace
using System.Collections; // Required for IEnumerator and Coroutines

namespace Spaces.Core.Vehicle.Runtime
{
    

public class DeactivateCarSpawnCanvas : MonoBehaviour
{
    public Canvas canvasToDeactivate; // Assign this in the Inspector
    public Button myButton; // Change TMP_Button to Button

    void Start()
    {
        if (myButton != null)
        {
            myButton.onClick.AddListener(DeactivateWithDelay);
        }
    }

    public void DeactivateWithDelay()
    {
        StartCoroutine(DeactivateAfterDelay(2.0f));
    }

    private IEnumerator DeactivateAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (canvasToDeactivate != null)
        {
            canvasToDeactivate.gameObject.SetActive(false);
        }
    }
}
}