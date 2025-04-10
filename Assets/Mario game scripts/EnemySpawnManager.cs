using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;

public class EnemySpawnManager : MonoBehaviour
{
    [Header("Enemy Prefabs")]
    public GameObject[] enemyPrefabs;

    [Header("Spawn Settings")]
    public int maxEnemies = 5;
    public float minSpawnRadius = 8f; // Minimum distance from player
    public float maxSpawnRadius = 15f; // Maximum distance from player
    public float spawnInterval = 10f;
    public float attackRange = 2f; // Distance at which enemies start attacking

    [Header("References")]
    [SerializeField] private SpawnManager spawnManager;
    [SerializeField] private LightshipNavMeshManager navMesh;

    private Transform playerTransform;
    private List<GameObject> spawnedEnemies = new List<GameObject>();
    private bool isSpawningEnabled = false;
    private int currentEnemyCount = 0;

    private void Start()
    {
        if (enemyPrefabs == null || enemyPrefabs.Length == 0)
        {
            // Debug.LogError("[Enemy Spawner] No enemy prefabs assigned!");
            return;
        }

        if (spawnManager == null)
        {
            spawnManager = FindObjectOfType<SpawnManager>();
            if (spawnManager == null)
            {
                // Debug.LogError("[Enemy Spawner] Could not find SpawnManager!");
                return;
            }
        }

        if (navMesh == null)
        {
            navMesh = FindObjectOfType<LightshipNavMeshManager>();
            if (navMesh == null)
            {
                // Debug.LogError("[Enemy Spawner] Could not find LightshipNavMeshManager!");
                return;
            }
        }
    }

    public void InitializeEnemySpawning(GameObject player)
    {
        playerTransform = player.transform;
        isSpawningEnabled = true;
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
                // Debug.Log("[Enemy Spawner] NavMesh initialized successfully!");
                StartCoroutine(ContinuousEnemySpawning());
                yield break;
            }

            // Debug.Log($"[Enemy Spawner] Waiting for NavMesh... Attempt {attempts + 1}");

            // Wait for a second before next check
            yield return new WaitForSeconds(1f);
            attempts++;
        }

        // Debug.LogError("[Enemy Spawner] Failed to initialize NavMesh after multiple attempts.");
    }

    private IEnumerator ContinuousEnemySpawning()
    {
        while (isSpawningEnabled && currentEnemyCount < maxEnemies)
        {
            // Wait for the spawn interval
            yield return new WaitForSeconds(spawnInterval);

            // Spawn a single enemy
            SpawnEnemy();
        }

        // Debug.Log("[Enemy Spawner] Reached maximum spawn limit.");
    }

    private void SpawnEnemy()
    {
        if (playerTransform == null) return;

        Vector3 spawnPosition = FindValidSpawnPosition();
        if (spawnPosition != Vector3.zero)
        {
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];
            GameObject enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);

            // Configure EnemyAI with attack range
            EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
            if (enemyAI != null)
            {
                enemyAI.SetAttackRange(attackRange);
            }

            spawnedEnemies.Add(enemy);
            currentEnemyCount++;

            // Debug.Log($"[Enemy Spawner] Spawned enemy at {spawnPosition}");
            // Debug.Log($"[Enemy Spawner] Distance from player: {Vector3.Distance(enemy.transform.position, playerTransform.position)}");
        }
    }

    private Vector3 FindValidSpawnPosition()
    {
        int maxAttempts = 10;
        for (int i = 0; i < maxAttempts; i++)
        {
            // Get random angle
            float randomAngle = Random.Range(0f, 360f);
            float randomDistance = Random.Range(minSpawnRadius, maxSpawnRadius);

            // Calculate position
            Vector3 direction = Quaternion.Euler(0, randomAngle, 0) * Vector3.forward;
            Vector3 targetPosition = playerTransform.position + direction * randomDistance;

            // Use SpawnManager's method to find a valid NavMesh position
            Vector3 validPosition = spawnManager.FindClosestNavMeshPosition(targetPosition);
            
            // If the position is valid and not too close to other enemies
            if (validPosition != Vector3.zero && !IsTooCloseToOtherEnemies(validPosition))
            {
                return validPosition;
            }
        }

        // Debug.LogWarning("[Enemy Spawner] Could not find valid spawn position");
        return Vector3.zero;
    }

    private bool IsTooCloseToOtherEnemies(Vector3 position)
    {
        foreach (GameObject enemy in spawnedEnemies)
        {
            if (enemy != null && Vector3.Distance(position, enemy.transform.position) < 2f)
            {
                return true;
            }
        }
        return false;
    }

    public void OnEnemyDeath(GameObject enemy)
    {
        if (spawnedEnemies.Contains(enemy))
        {
            spawnedEnemies.Remove(enemy);
            currentEnemyCount--;
            
            // Spawn a new enemy to replace the dead one
            if (isSpawningEnabled && currentEnemyCount < maxEnemies)
            {
                StartCoroutine(DelayedEnemySpawn());
            }
        }
    }

    private IEnumerator DelayedEnemySpawn()
    {
        yield return new WaitForSeconds(spawnInterval);
        SpawnEnemy();
    }

    private void OnDestroy()
    {
        // Clean up spawned enemies
        foreach (var enemy in spawnedEnemies)
        {
            if (enemy != null)
            {
                Destroy(enemy);
            }
        }
        spawnedEnemies.Clear();
    }

    // Debug visualization
    private void OnDrawGizmosSelected()
    {
        if (playerTransform != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(playerTransform.position, minSpawnRadius);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(playerTransform.position, maxSpawnRadius);
        }
    }
}