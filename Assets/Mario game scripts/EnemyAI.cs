// using System.Collections;
// using System.Collections.Generic;
// using Niantic.Lightship.AR.NavigationMesh;
// using UnityEngine;

// public class EnemyAI : MonoBehaviour
// {
//     [Header("Combat Settings")]
//     [SerializeField] float chaseRange = 10f;
//     [SerializeField] float attackRange = 2f;
//     [SerializeField] int attackDamage = 20;

//     private float distanceToTarget = Mathf.Infinity;
//     // private bool isProvoked = false;
//     private EnemyHealth enemyHealth;
//     private Transform target;
//     private Animator animator;
//     private bool isAttacking = false;
//     private LightshipNavMeshAgent navMeshAgent;
//     private float lastPathUpdateTime;
//     private float pathUpdateInterval = 0.5f;

//     void Start()
//     {
//         enemyHealth = GetComponent<EnemyHealth>();
//         animator = GetComponentInChildren<Animator>();
//         navMeshAgent = GetComponent<LightshipNavMeshAgent>();

//         // Find player
//         PlayerManager playerManager = FindObjectOfType<PlayerManager>();
//         if (playerManager != null)
//         {
//             target = playerManager.transform;
//             Debug.Log("Enemy found player target");
//         }
//         else
//         {
//             Debug.LogError("Enemy couldn't find PlayerManager!");
//         }

//         // Start chasing immediately
//         // isProvoked = true;
//     }

//     void Update()
//     {
//         if (target == null) return;

//         if (enemyHealth != null && enemyHealth.IsDead())
//         {
//             HandleDeath();
//             return;
//         }

//         // Calculate distance to player
//         distanceToTarget = Vector3.Distance(target.position, transform.position);

//         // Check if we should be chasing or attacking
//         // if (isProvoked)
//         // {
//         // }
//         if(distanceToTarget <= chaseRange)
//         {
//             EngageTarget();
//         }
//     }

//     private void HandleDeath()
//     {
//         // Disable components
//         enabled = false;

//         // Notify spawn manager
//         EnemySpawnManager spawnManager = FindObjectOfType<EnemySpawnManager>();
//         if (spawnManager != null)
//         {
//             spawnManager.OnEnemyDeath(gameObject);
//         }
//     }

//     public void SetAttackRange(float range)
//     {
//         attackRange = range;
//     }

//     public void OnDamageTaken()
//     {
//         // isProvoked = true;
//     }

//     private void EngageTarget()
//     {
//         // Always face the target
//         FaceTarget();

//         // If we're outside attack range, chase the player
//         if (distanceToTarget > attackRange)
//         {
//             ChaseTarget();
//         }
//         // If we're within attack range, attack
//         else if (distanceToTarget <= attackRange)
//         {
//             AttackTarget();
//         }
//     }

//     private void ChaseTarget()
//     {
//         isAttacking = false;

//         // Update animation
//         if (animator != null)
//         {
//             animator.SetBool("Attack", false);
//             animator.SetTrigger("Move");
//         }
//         navMeshAgent.SetDestination(target.position);

//         // Update the destination only periodically to avoid overloading the pathfinding
//         // if (Time.time - lastPathUpdateTime > pathUpdateInterval)
//         // {
//         //     // Only update if we have a NavMeshAgent
//         //     if (navMeshAgent != null)
//         //     {
//         //         try
//         //         {
//         //             lastPathUpdateTime = Time.time;
//         //             Debug.Log($"Updated NavMesh path to {target.position}");
//         //         }
//         //         catch (System.Exception e)
//         //         {
//         //             Debug.LogWarning($"Failed to set destination: {e.Message}");
//         //         }
//         //     }
//         // }
//     }

//     private void AttackTarget()
//     {
//         isAttacking = true;

//         // Update animation
//         if (animator != null)
//         {
//             animator.SetBool("Attack", true);
//         }
//     }

//     private void FaceTarget()
//     {
//         Vector3 direction = (target.position - transform.position).normalized;
//         Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
//         transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//     }

//     // Called by animation events to deal damage
//     public void DealDamage()
//     {
//         if (target == null) return;

//         // Check if player is still in range
//         if (distanceToTarget <= attackRange * 1.2f)
//         {
//             PlayerManager playerManager = target.GetComponent<PlayerManager>();
//             if (playerManager != null)
//             {
//                 playerManager.TakeDamage(attackDamage);
//                 Debug.Log("Enemy dealt damage to player");
//             }
//         }
//     }

//     void OnDrawGizmosSelected()
//     {
//         // Draw chase range
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, chaseRange);

//         // Draw attack range
//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, attackRange);
//     }
// }

// using System.Collections;
// using System.Collections.Generic;
// using Niantic.Lightship.AR.NavigationMesh;
// using UnityEngine;

// public class EnemyAI : MonoBehaviour
// {
//     [Header("Combat Settings")]
//     [SerializeField] float chaseRange = 10f;
//     [SerializeField] float attackRange = 2f;
//     [SerializeField] int attackDamage = 20;

//     private float distanceToTarget = Mathf.Infinity;
//     private EnemyHealth enemyHealth;
//     private Transform target;
//     private Animator animator;
//     private bool isAttacking = false;
//     private LightshipNavMeshAgent navMeshAgent;
//     private Vector3 lastKnownPlayerPosition;
//     private Vector3 lastEnemyPosition;

//     void Start()
//     {
//         enemyHealth = GetComponent<EnemyHealth>();
//         animator = GetComponentInChildren<Animator>();
//         navMeshAgent = GetComponent<LightshipNavMeshAgent>();
//         lastEnemyPosition = transform.position;

//         Debug.Log($"[{gameObject.name}] Start - Position: {transform.position}");

//         if (navMeshAgent == null)
//         {
//             Debug.LogError($"[{gameObject.name}] No NavMeshAgent found!");
//             return;
//         }
//         else
//         {
//             Debug.Log($"[{gameObject.name}] NavMeshAgent found");
//         }

//         // Find player
//         PlayerManager playerManager = FindObjectOfType<PlayerManager>();
//         if (playerManager != null)
//         {
//             target = playerManager.transform;
//             lastKnownPlayerPosition = target.position;
//             Debug.Log($"[{gameObject.name}] Found player target at {target.position}");

//             // Initial destination set
//             try
//             {
//                 navMeshAgent.SetDestination(target.position);
//                 Debug.Log($"[{gameObject.name}] Initial destination set to {target.position}");
//             }
//             catch (System.Exception e)
//             {
//                 Debug.LogError($"[{gameObject.name}] Failed to set initial destination: {e.Message}");
//             }
//         }
//         else
//         {
//             Debug.LogError($"[{gameObject.name}] Couldn't find PlayerManager!");
//         }

//         StartCoroutine(UpdateDestinationRoutine());
//         StartCoroutine(DebugMovementRoutine());
//     }

//     private IEnumerator DebugMovementRoutine()
//     {
//         while (enabled)
//         {
//             if (target != null)
//             {
//                 float distanceMoved = Vector3.Distance(lastEnemyPosition, transform.position);
//                 Debug.Log($"[{gameObject.name}] Movement Check:" +
//                     $"\nCurrent Position: {transform.position}" +
//                     $"\nDistance moved: {distanceMoved}" +
//                     $"\nDistance to target: {distanceToTarget}" +
//                     $"\nPlayer position: {target.position}" +
//                     $"\nIs attacking: {isAttacking}");

//                 lastEnemyPosition = transform.position;
//             }
//             yield return new WaitForSeconds(1f);
//         }
//     }

//     private IEnumerator UpdateDestinationRoutine()
//     {
//         Debug.Log($"[{gameObject.name}] Started UpdateDestinationRoutine");
//         while (enabled)
//         {
//             if (target != null && !isAttacking && distanceToTarget <= chaseRange)
//             {
//                 float playerMovement = Vector3.Distance(lastKnownPlayerPosition, target.position);
//                 Debug.Log($"[{gameObject.name}] Player movement check: {playerMovement}");

//                 if (playerMovement > 0.1f)
//                 {
//                     try
//                     {
//                         navMeshAgent.SetDestination(target.position);
//                         lastKnownPlayerPosition = target.position;
//                         Debug.Log($"[{gameObject.name}] Updated destination to: {target.position}");
//                     }
//                     catch (System.Exception e)
//                     {
//                         Debug.LogError($"[{gameObject.name}] Failed to update destination: {e.Message}");
//                     }
//                 }
//             }
//             yield return new WaitForSeconds(0.2f);
//         }
//     }

//     void Update()
//     {
//         if (target == null)
//         {
//             Debug.LogWarning($"[{gameObject.name}] No target!");
//             return;
//         }

//         if (enemyHealth != null && enemyHealth.IsDead())
//         {
//             HandleDeath();
//             return;
//         }

//         distanceToTarget = Vector3.Distance(target.position, transform.position);


//         if(distanceToTarget <= chaseRange)
//         {
//             EngageTarget();
//         }
//         else
//         {
//             if (animator != null)
//             {
//                 animator.SetBool("Attack", false);
//                 Debug.Log($"[{gameObject.name}] Player out of range, stopping attack");
//             }
//             isAttacking = false;
//         }
//     }

//     private void HandleDeath()
//     {
//         Debug.Log($"[{gameObject.name}] Handling death");
//         StopAllCoroutines();
//         enabled = false;

//         if (animator != null)
//         {
//             animator.SetBool("Attack", false);
//         }

//         EnemySpawnManager spawnManager = FindObjectOfType<EnemySpawnManager>();
//         if (spawnManager != null)
//         {
//             spawnManager.OnEnemyDeath(gameObject);
//         }
//     }

//     public void SetAttackRange(float range)
//     {
//         attackRange = range;
//         Debug.Log($"[{gameObject.name}] Attack range set to {range}");
//     }

//     private void EngageTarget()
//     {
//         FaceTarget();

//         if (distanceToTarget > attackRange)
//         {
//             ChaseTarget();
//         }
//         else
//         {
//             AttackTarget();
//         }
//     }

//     private void ChaseTarget()
//     {
//         if (isAttacking)
//         {
//             Debug.Log($"[{gameObject.name}] Transitioning from attack to chase");
//         }

//         isAttacking = false;

//         if (animator != null)
//         {
//             bool currentAttackState = animator.GetBool("Attack");
//             animator.SetBool("Attack", false);
//             animator.SetTrigger("Move");
//             Debug.Log($"[{gameObject.name}] Chase animations updated - Previous attack state: {currentAttackState}");
//         }
//         else
//         {
//             Debug.LogWarning($"[{gameObject.name}] No animator found!");
//         }
//     }

//     private void AttackTarget()
//     {
//         if (!isAttacking)
//         {
//             Debug.Log($"[{gameObject.name}] Starting attack at distance {distanceToTarget}");
//         }

//         isAttacking = true;

//         if (animator != null)
//         {
//             bool currentAttackState = animator.GetBool("Attack");
//             animator.SetBool("Attack", true);
//             Debug.Log($"[{gameObject.name}] Attack animation updated - Previous state: {currentAttackState}");
//         }
//     }

//     private void FaceTarget()
//     {
//         Vector3 direction = (target.position - transform.position).normalized;
//         Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
//         transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
//     }

//     public void DealDamage()
//     {
//         if (target == null) return;

//         if (distanceToTarget <= attackRange * 1.2f)
//         {
//             PlayerManager playerManager = target.GetComponent<PlayerManager>();
//             if (playerManager != null)
//             {
//                 playerManager.TakeDamage(attackDamage);
//                 Debug.Log($"[{gameObject.name}] Dealt damage to player at distance {distanceToTarget}");
//             }
//         }
//     }

//     void OnDrawGizmosSelected()
//     {
//         Gizmos.color = Color.red;
//         Gizmos.DrawWireSphere(transform.position, chaseRange);

//         Gizmos.color = Color.yellow;
//         Gizmos.DrawWireSphere(transform.position, attackRange);
//     }
// }

using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using UnityEngine;

public class EnemyAI : MonoBehaviour
{
    [Header("Combat Settings")]
    [SerializeField] float chaseRange = 10f;
    [SerializeField] float attackRange = 2f;
    [SerializeField] int attackDamage = 20;
    [SerializeField] float destinationUpdateThreshold = 0.3f;

    private float distanceToTarget = Mathf.Infinity;
    private EnemyHealth enemyHealth;
    private Transform target;
    private Animator animator;
    private bool isAttacking = false;
    private LightshipNavMeshAgent navMeshAgent;
    private Vector3 lastPlayerPosition;
    PlayerHealth playerHealth;

    void Start()
    {
        enemyHealth = GetComponent<EnemyHealth>();
        animator = GetComponentInChildren<Animator>();
        navMeshAgent = GetComponent<LightshipNavMeshAgent>();
        playerHealth = FindObjectOfType<PlayerHealth>();

        // Find player
        PlayerManager playerManager = FindObjectOfType<PlayerManager>();
        if (playerManager != null)
        {
            target = playerManager.transform;
            lastPlayerPosition = target.position;
            // Debug.Log($"[{gameObject.name}] Found player target at {target.position}");

            // Set initial destination
            if (navMeshAgent != null)
            {
                navMeshAgent.SetDestination(target.position);
                // Debug.Log($"[{gameObject.name}] Set initial destination to {target.position}");
            }
        }
        else
        {
            // Debug.LogError($"[{gameObject.name}] Couldn't find PlayerManager!");
        }
    }

    void FixedUpdate()
    {
        if (target == null || navMeshAgent == null) return;

        if (enemyHealth != null && enemyHealth.IsDead())
        {
            HandleDeath();
            return;
        }

        // Calculate distance to player
        distanceToTarget = Vector3.Distance(target.position, transform.position);

        // Check if player has moved
        float playerMovement = Vector3.Distance(lastPlayerPosition, target.position);

        if (distanceToTarget <= chaseRange)
        {
            // Debug current state
            // Debug.Log($"[{gameObject.name}] Distance to target: {distanceToTarget}, " +
            //          $"Player movement: {playerMovement}, " +
            //          $"Is attacking: {isAttacking}");

            // If player has moved more than threshold and we're not attacking
            if (playerMovement > destinationUpdateThreshold && distanceToTarget > attackRange)
            {
                navMeshAgent.SetDestination(target.position);
                // Debug.Log($"[{gameObject.name}] Updating destination to {target.position}");
                lastPlayerPosition = target.position;
            }

            EngageTarget();
        }
        else
        {
            // Reset attack animation when player is out of range
            if (animator != null)
            {
                animator.SetBool("Attack", false);
                animator.SetTrigger("Idle");
            }
            isAttacking = false;
        }
    }

    private void HandleDeath()
    {
        enabled = false;
        if (navMeshAgent != null) navMeshAgent.enabled = false;

        if (animator != null)
        {
            animator.SetBool("Attack", false);
        }

        EnemySpawnManager spawnManager = FindObjectOfType<EnemySpawnManager>();
        if (spawnManager != null)
        {
            spawnManager.OnEnemyDeath(gameObject);
        }
    }

    public void SetAttackRange(float range)
    {
        attackRange = range;
    }

    private void EngageTarget()
    {
        FaceTarget();

        if (distanceToTarget > attackRange)
        {
            ChaseTarget();
        }
        else
        {
            AttackTarget();
        }
    }

    private void ChaseTarget()
    {
        if (isAttacking)
        {
            // Transitioning from attack to chase
            navMeshAgent.SetDestination(target.position);
            // Debug.Log($"[{gameObject.name}] Transitioning from attack to chase");
        }

        isAttacking = false;

        if (animator != null)
        {
            animator.SetBool("Attack", false);
            animator.SetTrigger("Move");
            // Debug.Log($"[{gameObject.name}] Playing move animation");
        }
    }

    private void AttackTarget()
    {
        isAttacking = true;

        // Stop movement when attacking
        if (navMeshAgent != null && navMeshAgent.enabled)
        {
            navMeshAgent.SetDestination(transform.position);
        }

        if (animator != null)
        {
            animator.SetBool("Attack", true);
            // Debug.Log($"[{gameObject.name}] Playing attack animation");
        }
    }

    private void FaceTarget()
    {
        Vector3 direction = (target.position - transform.position).normalized;
        Quaternion lookRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
        transform.rotation = Quaternion.Slerp(transform.rotation, lookRotation, Time.deltaTime * 5f);
    }

    public void DealDamage()
    {
        if (target == null) return;

        PlayerManager playerManager = target.GetComponent<PlayerManager>();
        if (playerManager != null)
        {
            playerManager.TakeDamage(attackDamage);
            if(playerHealth.IsDead() == true)
            {
                // animator.SetBool("Attack", false);
                animator.SetTrigger("Idle");
            }
            // Debug.Log($"[{gameObject.name}] Dealt damage to player");
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, chaseRange);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        // Draw line to target if available
        if (Application.isPlaying && target != null)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(transform.position, target.position);
        }
    }
}