using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using GameCreator.Runtime.Common;
using Fusion;

public class SpawnPrefabScript : NetworkBehaviour
{
    public NetworkObject prefabToSpawn; // The NetworkObject prefab to spawn
    public Button spawnButton;
    public AudioClip spawnSound; // AudioClip to be played on spawn
    public float distanceFromPlayer = 6.0f; // Distance in front of the player to spawn the object
    public float heightOffset = 0.0f; // Height offset for the spawn position

    private NetworkObject spawnedCar;
    private AudioSource audioSource;

    private void Start()
    {
        if (spawnButton == null)
        {
            Debug.LogError("Spawn button reference not set!");
            return;
        }

        // Ensure the script has an AudioSource component
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        spawnButton.onClick.AddListener(ToggleSpawn);
    }

    private void ToggleSpawn()
    {
        Debug.Log("Button Clicked - Toggle Spawn");

        // If a car is already spawned, despawn it
        if (spawnedCar != null)
        {
            Debug.Log("Despawning existing spawned object");
            Runner.Despawn(spawnedCar);
            spawnedCar = null;
        }
        else
        {
            // Spawn a new car
            SpawnPrefab();
        }
    }

    private void SpawnPrefab()
    {
        if (prefabToSpawn == null)
        {
            Debug.LogError("Prefab to spawn is not assigned or is null.");
            return;
        }

        Transform playerTransform = ShortcutPlayer.Transform;
        if (playerTransform == null)
        {
            Debug.LogError("Player transform not found!");
            return;
        }

        Vector3 spawnPosition = playerTransform.position + playerTransform.forward * distanceFromPlayer + Vector3.up * heightOffset;
        Quaternion spawnRotation = playerTransform.rotation;

        Debug.Log($"Spawning prefab: {prefabToSpawn.name} at position: {spawnPosition}");

        // Use Photon Fusion's Spawn method to create a networked object and assign ownership
        spawnedCar = Runner.Spawn(prefabToSpawn, spawnPosition, spawnRotation, Runner.LocalPlayer);

        if (spawnedCar != null)
        {
            Debug.Log("Object Spawned Successfully");

            // Play the spawn sound
            PlaySpawnSound();
        }
        else
        {
            Debug.LogError("Failed to Spawn Object");
        }
    }

    private void PlaySpawnSound()
    {
        if (spawnSound != null)
        {
            audioSource.PlayOneShot(spawnSound);
        }
    }
}
