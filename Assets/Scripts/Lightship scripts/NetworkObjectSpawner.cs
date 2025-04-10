using Unity.Netcode;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;
using System.Collections.Generic;

public class NetworkObjectSpawner : NetworkBehaviour
{
    [SerializeField] private ObjectSpawner objectSpawner;
    public GameObject[] spawnablePrefabs; // Assign the registered prefabs in the Inspector

    // Spawn a specific object at a given position
    public void SpawnObject(int index, Vector3 position)
    {
        if (!IsServer) return; // Only the server/host should spawn objects

        // Validate index
        if (index < 0 || index >= spawnablePrefabs.Length)
        {
            Debug.LogWarning($"Invalid prefab index: {index}");
            return;
        }

        GameObject newObject = Instantiate(spawnablePrefabs[index], position, Quaternion.identity);
        
        // Ensure NetworkObject component
        NetworkObject networkObject = newObject.GetComponent<NetworkObject>();
        if (networkObject == null)
        {
            networkObject = newObject.AddComponent<NetworkObject>();
        }

        // Spawn the network object
        networkObject.Spawn();
    }

    // Create network objects from ObjectSpawner's prefabs
    void CreateNetworkObjects()
    {
        if (!IsServer) return; // Only server should create network objects

        // Ensure ObjectSpawner is referenced
        if (objectSpawner == null)
        {
            objectSpawner = FindObjectOfType<ObjectSpawner>();
            if (objectSpawner == null)
            {
                Debug.LogError("ObjectSpawner not found!");
                return;
            }
        }

        // Spawn each prefab from ObjectSpawner
        for (int i = 0; i < objectSpawner.objectPrefabs.Count; i++)
        {
            SpawnObject(i, Vector3.zero);
        }
    }

    // Optional method to spawn a random object
    public void SpawnRandomObject(Vector3 position)
    {
        if (!IsServer) return;

        int randomIndex = Random.Range(0, spawnablePrefabs.Length);
        SpawnObject(randomIndex, position);
    }

    // Optional method to spawn all registered prefabs
    public void SpawnAllPrefabs(Vector3 basePosition)
    {
        if (!IsServer) return;

        for (int i = 0; i < spawnablePrefabs.Length; i++)
        {
            // Offset each spawned object slightly
            Vector3 spawnPosition = basePosition + new Vector3(i * 2f, 0, 0);
            SpawnObject(i, spawnPosition);
        }
    }

    // Override to ensure setup
    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        // Optionally create network objects when spawned
        if (IsServer)
        {
            CreateNetworkObjects();
        }
    }
}