using UnityEngine;

public class PlayerInstantiationEvent : MonoBehaviour
{
    private void Start()
    {
        // Trigger this event when the player is instantiated
        ReactRaiseEvent.PlayerInstantiated();
    }
}
