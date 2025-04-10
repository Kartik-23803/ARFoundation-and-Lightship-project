// using UnityEngine;
// using Niantic.Lightship.AR.NavigationMesh;
// using System.Collections.Generic;
// using System.Linq;
// using System;

// public class SpawnManager : MonoBehaviour
// {
//     [Header("Spawnable Objects")]
//     public GameObject[] coinPrefabs;
//     public GameObject[] obstaclePrefabs;

//     [Header("Spawn Settings")]
//     public int maxObjectsToSpawn = 10;
//     public float minDistanceBetweenObjects = 1f;

//     [SerializeField] LightshipNavMeshManager navMesh; // Assign this in inspector

//     private void Start()
//     {
//         // Validate prefabs
//         if (coinPrefabs == null || coinPrefabs.Length == 0 || 
//             obstaclePrefabs == null || obstaclePrefabs.Length == 0)
//         {
//             Debug.LogError("[NavMesh Spawner] Prefabs not assigned!");
//             return;
//         }

//         if (navMesh == null)
//         {
//             navMesh = FindObjectOfType<LightshipNavMeshManager>();
//         }

//         if (navMesh == null)
//         {
//             Debug.LogError("[NavMesh Spawner] Could not find NavMesh Manager!");
//             return;
//         }

//         // Start checking for NavMesh periodically
//         StartCoroutine(WaitForNavMeshInitialization());
//     }

//     private System.Collections.IEnumerator WaitForNavMeshInitialization()
//     {
//         int maxAttempts = 30; // Wait up to 30 seconds
//         int attempts = 0;

//         while (attempts < maxAttempts)
//         {
//             // Check if NavMesh is ready
//             if (navMesh.LightshipNavMesh != null && 
//                 navMesh.LightshipNavMesh.Area > 0 && 
//                 navMesh.LightshipNavMesh.Surfaces.Count > 0)
//             {
//                 Debug.Log("[NavMesh Spawner] NavMesh initialized successfully!");
//                 SpawnObjects();
//                 yield break;
//             }

//             Debug.Log($"[NavMesh Spawner] Waiting for NavMesh... Attempt {attempts + 1}");

//             // Wait for a second before next check
//             yield return new WaitForSeconds(1f);
//             attempts++;
//         }

//         Debug.LogError("[NavMesh Spawner] Failed to initialize NavMesh after multiple attempts.");
//     }

//     private void SpawnObjects()
//     {
//         try 
//         {
//             // Get tile locations directly from surfaces
//             List<Vector3> tileLocations = new List<Vector3>();
//             float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;

//             foreach (Surface surface in navMesh.LightshipNavMesh.Surfaces)
//             {
//                 foreach (GridNode tile in surface.Elements)
//                 {
//                     // Manually calculate world position
//                     Vector3 tileWorldPosition = new Vector3(
//                         tile.Coordinates.x * tileSize + (tileSize / 2f),
//                         surface.Elevation,
//                         tile.Coordinates.y * tileSize + (tileSize / 2f)
//                     );

//                     tileLocations.Add(tileWorldPosition);
//                 }
//             }

//             Debug.Log($"[NavMesh Spawner] Total Tile Locations: {tileLocations.Count}");

//             // Rest of your existing spawn logic...
//             Shuffle(tileLocations);

//             List<Vector3> spawnedPositions = new List<Vector3>();
//             int successfulSpawns = 0;

//             for (int i = 0; i < Mathf.Min(maxObjectsToSpawn, tileLocations.Count); i++)
//             {
//                 Vector3 potentialSpawnPosition = tileLocations[i];

//                 if (IsValidSpawnPosition(potentialSpawnPosition, spawnedPositions))
//                 {
//                     GameObject[] objectsToChooseFrom = successfulSpawns % 2 == 0 ? coinPrefabs : obstaclePrefabs;
//                     GameObject objectToSpawn = objectsToChooseFrom[UnityEngine.Random.Range(0, objectsToChooseFrom.Length)];

//                     GameObject spawnedObject = Instantiate(objectToSpawn, potentialSpawnPosition, Quaternion.identity);
//                     spawnedObject.tag = "SpawnedObject";

//                     spawnedPositions.Add(potentialSpawnPosition);
//                     successfulSpawns++;

//                     Debug.Log($"[NavMesh Spawner] Spawned {objectToSpawn.name} at {potentialSpawnPosition}");
//                 }

//                 if (successfulSpawns >= maxObjectsToSpawn)
//                     break;
//             }

//             Debug.Log($"[NavMesh Spawner] Spawn complete. Successfully spawned {successfulSpawns} objects.");
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"[NavMesh Spawner] Spawn error: {e.Message}");
//         }
//     }

//     // Utility method to get tile coordinates
//     public List<Vector2Int> GetTileCoordinates()
//     {
//         if (navMesh?.LightshipNavMesh == null)
//             return new List<Vector2Int>();

//         float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;

//         return navMesh.LightshipNavMesh.Surfaces
//             .SelectMany(s => s.Elements.Select(tile => new 
//             {
//                 Tile = tile,
//                 Surface = s
//             }))
//             .Select(x => new Vector3(
//                 x.Tile.Coordinates.x * tileSize + (tileSize / 2f), 
//                 x.Surface.Elevation,
//                 x.Tile.Coordinates.y * tileSize + (tileSize / 2f)
//             ))
//             .Select(pos => Utils.PositionToTile(pos, tileSize))
//             .ToList();
//     }

//     bool IsValidSpawnPosition(Vector3 position, List<Vector3> existingPositions)
//     {
//         foreach (Vector3 existingPosition in existingPositions)
//         {
//             if (Vector3.Distance(position, existingPosition) < minDistanceBetweenObjects)
//             {
//                 return false;
//             }
//         }
//         return true;
//     }

//     // Fisher-Yates shuffle algorithm
//     void Shuffle<T>(List<T> list)
//     {
//         for (int i = 0; i < list.Count; i++)
//         {
//             T temp = list[i];
//             int randomIndex = UnityEngine.Random.Range(i, list.Count);
//             list[i] = list[randomIndex];
//             list[randomIndex] = temp;
//         }
//     }
// }

using UnityEngine;
using Niantic.Lightship.AR.NavigationMesh;
using System.Collections.Generic;
using System.Linq;
using System;
using System.Collections;

public class SpawnManager : MonoBehaviour
{
    [Header("Spawnable Objects")]
    public GameObject[] coinPrefabs;
    public GameObject[] powerupPrefabs;
    public GameObject[] obstaclePrefabs;

    [Header("Spawn Probabilities")]
    [Range(0f, 1f)] public float coinProbability = 0.45f;
    [Range(0f, 1f)] public float obstacleProbability = 0.45f;
    [Range(0f, 1f)] public float powerupProbability = 0.1f;

    [Header("Spawn Settings")]
    public int maxObjectsToSpawn = 10;
    public float minDistanceBetweenObjects = 1f;
    public float spawnInterval = 5f;
    public float spawnDistance = 3f; // Distance in front of the player to spawn objects

    [Header("Performance Settings")]
    [SerializeField] private float navMeshQueryCacheTime = 30f; // How long to keep cached positions
    [SerializeField] private float gridSize = 0.5f; // Grid size for position caching

    private int instantiatedObjects = 0;
    private GameObject navMeshAgent; // Reference to the player/agent
    private PlayerManager playerManager;

    [SerializeField] LightshipNavMeshManager navMesh;

    // Cache for NavMesh position queries
    private Dictionary<Vector3Int, NavMeshPositionInfo> navMeshPositionCache = new Dictionary<Vector3Int, NavMeshPositionInfo>();
    private List<GameObject> spawnedObjects = new List<GameObject>();

    // Structure to store cached position data with timestamp
    private struct NavMeshPositionInfo
    {
        public Vector3 position;
        public float timestamp;

        public NavMeshPositionInfo(Vector3 pos)
        {
            position = pos;
            timestamp = Time.time;
        }
    }

    private void Start()
    {
        // Validate prefabs
        if (coinPrefabs == null || coinPrefabs.Length == 0 ||
            powerupPrefabs == null || powerupPrefabs.Length == 0 ||
            obstaclePrefabs == null || obstaclePrefabs.Length == 0)
        {
            // Debug.LogError("[NavMesh Spawner] Prefabs not assigned!");
            return;
        }

        if (navMesh == null)
        {
            navMesh = FindObjectOfType<LightshipNavMeshManager>();
        }

        if (navMesh == null)
        {
            // Debug.LogError("[NavMesh Spawner] Could not find NavMesh Manager!");
            return;
        }

        // Start cache cleanup coroutine
        StartCoroutine(CleanupNavMeshCache());
    }

    // Call this method when the player is first spawned
    public void InitializeSpawningWithAgent(GameObject agent)
    {
        navMeshAgent = agent;
        playerManager = agent.GetComponent<PlayerManager>();

        // Start checking for NavMesh periodically
        StartCoroutine(WaitForNavMeshInitializationAndSpawn());
    }

    private IEnumerator WaitForNavMeshInitializationAndSpawn()
    {
        int maxAttempts = 30;
        int attempts = 0;

        while (attempts < maxAttempts)
        {
            // Check if NavMesh is ready
            if (navMesh.LightshipNavMesh != null &&
                navMesh.LightshipNavMesh.Area > 0 &&
                navMesh.LightshipNavMesh.Surfaces.Count > 0)
            {
                // DebugLog("[NavMesh Spawner] NavMesh initialized successfully!");
                StartCoroutine(ContinuousSpawning());
                yield break;
            }

            // DebugLog($"[NavMesh Spawner] Waiting for NavMesh... Attempt {attempts + 1}");

            // Wait for a second before next check
            yield return new WaitForSeconds(1f);
            attempts++;
        }

        // Debug.LogError("[NavMesh Spawner] Failed to initialize NavMesh after multiple attempts.");
    }

    public IEnumerator ContinuousSpawning()
    {
        while (instantiatedObjects < maxObjectsToSpawn)
        {
            // Wait for the spawn interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn a single object
            yield return StartCoroutine(SpawnSingleObjectInFrontOfPlayer());
        }

        // DebugLog("[NavMesh Spawner] Reached maximum spawn limit.");
    }

    private IEnumerator SpawnSingleObjectInFrontOfPlayer()
    {
        try
        {
            if (navMeshAgent == null)
            {
                // Debug.LogError("[NavMesh Spawner] No NavMesh agent set!");
                yield break;
            }

            // Calculate spawn position in front of the player
            Vector3 playerForward = navMeshAgent.transform.forward;
            Vector3 spawnPosition = navMeshAgent.transform.position + (playerForward * spawnDistance);

            // Find the closest valid NavMesh position
            Vector3 finalSpawnPosition = FindClosestNavMeshPosition(spawnPosition);

            // Determine object type based on probabilities
            GameObject objectToSpawn = DetermineObjectToSpawn();

            if (objectToSpawn != null)
            {
                GameObject spawnedObject = Instantiate(objectToSpawn, finalSpawnPosition, Quaternion.identity);
                spawnedObject.tag = "SpawnedObject";
                spawnedObjects.Add(spawnedObject);
                instantiatedObjects++;

                // Set player reference if it's a pickup
                PickupEffect pickup = spawnedObject.GetComponent<PickupEffect>();
                if (pickup != null && playerManager != null)
                {
                    pickup.playerManager = playerManager;
                }

                // DebugLog($"[NavMesh Spawner] Spawned {objectToSpawn.name} at {finalSpawnPosition}");
                // DebugLog($"[NavMesh Spawner] Current total spawned objects: {instantiatedObjects}");
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[NavMesh Spawner] Spawn error: {e.Message}");
        }

        yield return null;
    }

    private GameObject DetermineObjectToSpawn()
    {
        float randomValue = UnityEngine.Random.value;

        if (randomValue < powerupProbability)
        {
            // Spawn Powerup (10% chance)
            return powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)];
        }
        else if (randomValue < powerupProbability + coinProbability)
        {
            // Spawn Coin (45% chance)
            return coinPrefabs[UnityEngine.Random.Range(0, coinPrefabs.Length)];
        }
        else
        {
            // Spawn Obstacle (45% chance)
            return obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
        }
    }

    // Helper to convert world position to grid key for caching
    public Vector3Int WorldToGridKey(Vector3 worldPos)
    {
        return new Vector3Int(
            Mathf.RoundToInt(worldPos.x / gridSize),
            Mathf.RoundToInt(worldPos.y / gridSize),
            Mathf.RoundToInt(worldPos.z / gridSize)
        );
    }

    // Find the closest valid NavMesh position with caching
    public Vector3 FindClosestNavMeshPosition(Vector3 originalPosition)
    {
        // Check cache first
        Vector3Int gridKey = WorldToGridKey(originalPosition);

        if (navMeshPositionCache.TryGetValue(gridKey, out NavMeshPositionInfo cachedInfo))
        {
            // Only use cache if it's not too old
            if (Time.time - cachedInfo.timestamp < navMeshQueryCacheTime)
            {
                return cachedInfo.position;
            }
        }

        // If not in cache or cache is too old, compute new position
        Vector3 result = originalPosition;

        // If the original position is on the NavMesh, return it
        if (IsPositionOnNavMesh(originalPosition))
        {
            result = originalPosition;
        }
        else
        {
            // Try finding a close position on the NavMesh
            float[] searchRadii = new float[] { 0.5f, 1f, 2f, 3f };

            foreach (float radius in searchRadii)
            {
                Vector3 closestPoint = FindNearestNavMeshPoint(originalPosition, radius);
                if (closestPoint != Vector3.zero)
                {
                    result = closestPoint;
                    break;
                }
            }
        }

        // Cache the result
        navMeshPositionCache[gridKey] = new NavMeshPositionInfo(result);
        return result;
    }

    // Check if a position is on the NavMesh
    private bool IsPositionOnNavMesh(Vector3 position)
    {
        if (navMesh?.LightshipNavMesh == null) return false;

        float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;
        Vector3Int gridKey = WorldToGridKey(position);

        // Quick check for surfaces near this grid position
        foreach (Surface surface in navMesh.LightshipNavMesh.Surfaces)
        {
            foreach (GridNode tile in surface.Elements)
            {
                Vector3 tileWorldPosition = new Vector3(
                    tile.Coordinates.x * tileSize + (tileSize / 2f),
                    surface.Elevation,
                    tile.Coordinates.y * tileSize + (tileSize / 2f)
                );

                if (Vector3.Distance(position, tileWorldPosition) < tileSize / 2f)
                {
                    return true;
                }
            }
        }

        return false;
    }

    // Find the nearest point on the NavMesh within a given radius
    private Vector3 FindNearestNavMeshPoint(Vector3 originalPosition, float searchRadius)
    {
        if (navMesh?.LightshipNavMesh == null) return Vector3.zero;

        float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;
        Vector3 nearestPoint = Vector3.zero;
        float shortestDistance = float.MaxValue;

        // Calculate grid bounds for search to avoid checking all tiles
        Vector3Int gridCenter = WorldToGridKey(originalPosition);
        int gridRadius = Mathf.CeilToInt(searchRadius / gridSize);

        foreach (Surface surface in navMesh.LightshipNavMesh.Surfaces)
        {
            foreach (GridNode tile in surface.Elements)
            {
                Vector3 tileWorldPosition = new Vector3(
                    tile.Coordinates.x * tileSize + (tileSize / 2f),
                    surface.Elevation,
                    tile.Coordinates.y * tileSize + (tileSize / 2f)
                );

                // Skip tiles outside our search radius
                Vector3Int tileGrid = WorldToGridKey(tileWorldPosition);
                if (Mathf.Abs(tileGrid.x - gridCenter.x) > gridRadius ||
                    Mathf.Abs(tileGrid.z - gridCenter.z) > gridRadius)
                {
                    continue;
                }

                float distance = Vector3.Distance(originalPosition, tileWorldPosition);

                if (distance <= searchRadius && distance < shortestDistance)
                {
                    nearestPoint = tileWorldPosition;
                    shortestDistance = distance;
                }
            }
        }

        return nearestPoint;
    }

    // Periodically clean up old cache entries
    private IEnumerator CleanupNavMeshCache()
    {
        while (true)
        {
            yield return new WaitForSeconds(60f); // Check every minute

            float currentTime = Time.time;
            List<Vector3Int> keysToRemove = new List<Vector3Int>();

            foreach (var entry in navMeshPositionCache)
            {
                if (currentTime - entry.Value.timestamp > navMeshQueryCacheTime)
                {
                    keysToRemove.Add(entry.Key);
                }
            }

            foreach (var key in keysToRemove)
            {
                navMeshPositionCache.Remove(key);
            }

            // DebugLog($"[NavMesh Spawner] Cleaned up {keysToRemove.Count} cache entries");
        }
    }

    // Clean up spawned objects when they're destroyed
    public void RemoveSpawnedObject(GameObject obj)
    {
        if (spawnedObjects.Contains(obj))
        {
            spawnedObjects.Remove(obj);
        }
    }

    // Optional: Method to adjust probabilities at runtime
    public void SetSpawnProbabilities(float coinProb, float obstacleProb, float powerupProb)
    {
        // Ensure probabilities add up to 1
        float total = coinProb + obstacleProb + powerupProb;

        coinProbability = coinProb / total;
        obstacleProbability = obstacleProb / total;
        powerupProbability = powerupProb / total;
    }

    // Utility method to shuffle list
    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    // Conditional debug logging to reduce performance impact
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DebugLog(string message)
    {
        Debug.Log(message);
    }

    private void OnDestroy()
    {
        // Clean up any remaining spawned objects
        foreach (var obj in spawnedObjects)
        {
            if (obj != null)
            {
                Destroy(obj);
            }
        }
        spawnedObjects.Clear();
    }
}

// using UnityEngine;
// using Niantic.Lightship.AR.NavigationMesh;
// using System.Collections.Generic;
// using System.Linq;
// using System;
// using System.Collections;

// public class SpawnManager : MonoBehaviour
// {
//     [Header("Spawnable Objects")]
//     public GameObject[] coinPrefabs;
//     public GameObject[] powerupPrefabs;
//     public GameObject[] obstaclePrefabs;

//     [Header("Spawn Probabilities")]
//     [Range(0f, 1f)] public float coinProbability = 0.45f;
//     [Range(0f, 1f)] public float obstacleProbability = 0.45f;
//     [Range(0f, 1f)] public float powerupProbability = 0.1f;

//     [Header("Spawn Settings")]
//     public int maxObjectsToSpawn = 10;
//     public float minDistanceBetweenObjects = 1f;
//     public float spawnInterval = 5f;
//     public float spawnDistance = 3f; // Distance in front of the player to spawn objects

//     private int instantiatedObjects = 0;
//     private GameObject navMeshAgent; // Reference to the player/agent

//     [SerializeField] LightshipNavMeshManager navMesh;

//     private void Start()
//     {
//         // Validate prefabs
//         if (coinPrefabs == null || coinPrefabs.Length == 0 ||
//             powerupPrefabs == null || powerupPrefabs.Length == 0 || 
//             obstaclePrefabs == null || obstaclePrefabs.Length == 0)
//         {
//             Debug.LogError("[NavMesh Spawner] Prefabs not assigned!");
//             return;
//         }

//         if (navMesh == null)
//         {
//             navMesh = FindObjectOfType<LightshipNavMeshManager>();
//         }

//         if (navMesh == null)
//         {
//             Debug.LogError("[NavMesh Spawner] Could not find NavMesh Manager!");
//             return;
//         }
//     }

//     // Call this method when the player is first spawned
//     public void InitializeSpawningWithAgent(GameObject agent)
//     {
//         navMeshAgent = agent;

//         // Start checking for NavMesh periodically
//         StartCoroutine(WaitForNavMeshInitializationAndSpawn());
//     }

//     private IEnumerator WaitForNavMeshInitializationAndSpawn()
//     {
//         int maxAttempts = 30;
//         int attempts = 0;

//         while (attempts < maxAttempts)
//         {
//             // Check if NavMesh is ready
//             if (navMesh.LightshipNavMesh != null && 
//                 navMesh.LightshipNavMesh.Area > 0 && 
//                 navMesh.LightshipNavMesh.Surfaces.Count > 0)
//             {
//                 Debug.Log("[NavMesh Spawner] NavMesh initialized successfully!");
//                 StartCoroutine(ContinuousSpawning());
//                 yield break;
//             }

//             Debug.Log($"[NavMesh Spawner] Waiting for NavMesh... Attempt {attempts + 1}");

//             // Wait for a second before next check
//             yield return new WaitForSeconds(1f);
//             attempts++;
//         }

//         Debug.LogError("[NavMesh Spawner] Failed to initialize NavMesh after multiple attempts.");
//     }

//     private IEnumerator ContinuousSpawning()
//     {
//         while (instantiatedObjects < maxObjectsToSpawn)
//         {
//             // Wait for the spawn interval
//             yield return new WaitForSeconds(spawnInterval);

//             // Spawn a single object
//             yield return StartCoroutine(SpawnSingleObjectInFrontOfPlayer());
//         }

//         Debug.Log("[NavMesh Spawner] Reached maximum spawn limit.");
//     }

//     private IEnumerator SpawnSingleObjectInFrontOfPlayer()
//     {
//         try 
//         {
//             if (navMeshAgent == null)
//             {
//                 Debug.LogError("[NavMesh Spawner] No NavMesh agent set!");
//                 yield break;
//             }

//             // Calculate spawn position in front of the player
//             Vector3 playerForward = navMeshAgent.transform.forward;
//             Vector3 spawnPosition = navMeshAgent.transform.position + (playerForward * spawnDistance);

//             // Find the closest valid NavMesh position
//             Vector3 finalSpawnPosition = FindClosestNavMeshPosition(spawnPosition);

//             // Determine object type based on probabilities
//             GameObject objectToSpawn = DetermineObjectToSpawn();

//             if (objectToSpawn != null)
//             {
//                 GameObject spawnedObject = Instantiate(objectToSpawn, finalSpawnPosition, Quaternion.identity);
//                 spawnedObject.tag = "SpawnedObject";
//                 instantiatedObjects++;

//                 Debug.Log($"[NavMesh Spawner] Spawned {objectToSpawn.name} at {finalSpawnPosition}");
//                 Debug.Log($"[NavMesh Spawner] Current total spawned objects: {instantiatedObjects}");
//             }
//         }
//         catch (System.Exception e)
//         {
//             Debug.LogError($"[NavMesh Spawner] Spawn error: {e.Message}");
//         }

//         yield return null;
//     }

//     private GameObject DetermineObjectToSpawn()
//     {
//         float randomValue = UnityEngine.Random.value;

//         if (randomValue < powerupProbability)
//         {
//             // Spawn Powerup (10% chance)
//             return powerupPrefabs[UnityEngine.Random.Range(0, powerupPrefabs.Length)];
//         }
//         else if (randomValue < powerupProbability + coinProbability)
//         {
//             // Spawn Coin (45% chance)
//             return coinPrefabs[UnityEngine.Random.Range(0, coinPrefabs.Length)];
//         }
//         else
//         {
//             // Spawn Obstacle (45% chance)
//             return obstaclePrefabs[UnityEngine.Random.Range(0, obstaclePrefabs.Length)];
//         }
//     }

//     // Find the closest valid NavMesh position
//     private Vector3 FindClosestNavMeshPosition(Vector3 originalPosition)
//     {
//         // If the original position is on the NavMesh, return it
//         if (IsPositionOnNavMesh(originalPosition))
//         {
//             return originalPosition;
//         }

//         // Try finding a close position on the NavMesh
//         float[] searchRadii = new float[] { 0.5f, 1f, 2f, 3f };

//         foreach (float radius in searchRadii)
//         {
//             Vector3 closestPoint = FindNearestNavMeshPoint(originalPosition, radius);
//             if (closestPoint != Vector3.zero)
//             {
//                 return closestPoint;
//             }
//         }

//         // Fallback to original position if no NavMesh position found
//         return originalPosition;
//     }

//     // Check if a position is on the NavMesh
//     private bool IsPositionOnNavMesh(Vector3 position)
//     {
//         if (navMesh?.LightshipNavMesh == null) return false;

//         float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;

//         foreach (Surface surface in navMesh.LightshipNavMesh.Surfaces)
//         {
//             foreach (GridNode tile in surface.Elements)
//             {
//                 Vector3 tileWorldPosition = new Vector3(
//                     tile.Coordinates.x * tileSize + (tileSize / 2f),
//                     surface.Elevation,
//                     tile.Coordinates.y * tileSize + (tileSize / 2f)
//                 );

//                 if (Vector3.Distance(position, tileWorldPosition) < tileSize / 2f)
//                 {
//                     return true;
//                 }
//             }
//         }

//         return false;
//     }

//     // Find the nearest point on the NavMesh within a given radius
//     private Vector3 FindNearestNavMeshPoint(Vector3 originalPosition, float searchRadius)
//     {
//         if (navMesh?.LightshipNavMesh == null) return Vector3.zero;

//         float tileSize = navMesh.LightshipNavMesh.Settings.TileSize;
//         Vector3 nearestPoint = Vector3.zero;
//         float shortestDistance = float.MaxValue;

//         foreach (Surface surface in navMesh.LightshipNavMesh.Surfaces)
//         {
//             foreach (GridNode tile in surface.Elements)
//             {
//                 Vector3 tileWorldPosition = new Vector3(
//                     tile.Coordinates.x * tileSize + (tileSize / 2f),
//                     surface.Elevation,
//                     tile.Coordinates.y * tileSize + (tileSize / 2f)
//                 );

//                 float distance = Vector3.Distance(originalPosition, tileWorldPosition);

//                 if (distance <= searchRadius && distance < shortestDistance)
//                 {
//                     nearestPoint = tileWorldPosition;
//                     shortestDistance = distance;
//                 }
//             }
//         }

//         return nearestPoint;
//     }

//     // Optional: Method to adjust probabilities at runtime
//     public void SetSpawnProbabilities(float coinProb, float obstacleProb, float powerupProb)
//     {
//         // Ensure probabilities add up to 1
//         float total = coinProb + obstacleProb + powerupProb;

//         coinProbability = coinProb / total;
//         obstacleProbability = obstacleProb / total;
//         powerupProbability = powerupProb / total;
//     }

//     // Utility method to shuffle list
//     void Shuffle<T>(List<T> list)
//     {
//         for (int i = 0; i < list.Count; i++)
//         {
//             T temp = list[i];
//             int randomIndex = UnityEngine.Random.Range(i, list.Count);
//             list[i] = list[randomIndex];
//             list[randomIndex] = temp;
//         }
//     }
// }