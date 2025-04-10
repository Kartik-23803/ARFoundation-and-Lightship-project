using UnityEngine;
using System;

public class PickupEffect : MonoBehaviour
{
    public PlayerManager playerManager; // Reference to player management script
    
    // Cache components
    private PickupType pickupType;
    private Collider pickupCollider;
    MarioScoreManager marioScoreManager;
    AudioManager audioManager;
    
    // Optional visual effects
    [SerializeField] private GameObject pickupParticleEffect;
    [SerializeField] private AudioClip pickupSound;

    
    private bool isCollected = false;
    
    private void Awake()
    {
        // Cache components for better performance
        pickupType = GetComponent<PickupType>();
        pickupCollider = GetComponent<Collider>();
        
        // Ensure collider is set as trigger
        if (pickupCollider != null && !pickupCollider.isTrigger)
        {
            pickupCollider.isTrigger = true;
        }
    }
    
    private void OnEnable()
    {
        // Register with SpawnManager if it exists
        SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
        if (spawnManager != null)
        {
            // This is just a placeholder - the actual registration happens in SpawnManager
        }
    }

    void Start()
    {
        marioScoreManager = FindObjectOfType<MarioScoreManager>();
        audioManager = FindObjectOfType<AudioManager>();
    }

    private void OnTriggerEnter(Collider other)
    {
        // Prevent double collection
        if (isCollected) return;
        
        // DebugLog($"Trigger entered by: {other.name} with tag: {other.tag}");
        
        if (other.gameObject.tag == "Player")
        {
            // If playerManager is null, try to get it from the colliding object
            if (playerManager == null)
            {
                playerManager = other.GetComponent<PlayerManager>();
                if (playerManager == null)
                {
                    playerManager = other.GetComponentInParent<PlayerManager>();
                }
            }
            
            if (pickupType != null && playerManager != null)
            {
                isCollected = true;
                
                // Apply the pickup effect
                ApplyPickupEffect(pickupType);
                
                // Play visual and sound effects
                PlayPickupEffects();
                
                // Notify SpawnManager about object removal
                SpawnManager spawnManager = FindObjectOfType<SpawnManager>();
                if (spawnManager != null)
                {
                    spawnManager.RemoveSpawnedObject(gameObject);
                }
                
                // Destroy the pickup object
                Destroy(gameObject);
            }
            else
            {
                if (pickupType == null)
                    Debug.LogError("PickupType component missing on " + gameObject.name);
                if (playerManager == null)
                    Debug.LogError("PlayerManager reference missing and not found on colliding object " + other.name);
            }
        }
    }

    private void ApplyPickupEffect(PickupType pickupType)
    {
        switch (pickupType.pickupType)
        {
            case PickupType.ObjectType.Coin:
                HandleCoinPickup(pickupType);
                break;
            case PickupType.ObjectType.SpeedBoost:
                HandleSpeedBoost(pickupType);
                break;
            case PickupType.ObjectType.Shield:
                HandleShield(pickupType);
                break;
            case PickupType.ObjectType.DoublePoints:
                HandleDoublePoints(pickupType);
                break;
            case PickupType.ObjectType.Magnet:
                HandleMagnet(pickupType);
                break;
            case PickupType.ObjectType.ExtraLife:
                HandleExtraLife(pickupType);
                break;
            // case PickupType.ObjectType.Obstacle:
            //     HandleObstacle(pickupType);
            //     break;
            case PickupType.ObjectType.JumpBoost:
                HandleJumpBoost(pickupType);
                break;
            case PickupType.ObjectType.TimeFreeze:
                HandleTimeFreeze(pickupType);
                break;
            case PickupType.ObjectType.Multiplier:
                HandleMultiplier(pickupType);
                break;
        }
    }

    // Specific Pickup Effect Methods
    private void HandleCoinPickup(PickupType pickupType)
    {
        if(marioScoreManager != null)
        {
            marioScoreManager.AddCoinScore();
        }

        if (audioManager != null)
        {
            audioManager.PlayCoinSound();
        }
        DebugLog($"Collected {pickupType.value} coins");
    }

    private void HandleSpeedBoost(PickupType pickupType)
    {
        playerManager.ActivateSpeedBoost(pickupType.duration);
        DebugLog($"Speed Boost activated for {pickupType.duration} seconds");
    }

    private void HandleShield(PickupType pickupType)
    {
        playerManager.ActivateShield(pickupType.duration);
        DebugLog($"Shield activated for {pickupType.duration} seconds");
    }

    private void HandleDoublePoints(PickupType pickupType)
    {
        // If you have a ScoreManager, uncomment this:
        // if (playerManager.scoreManager != null)
        //     playerManager.scoreManager.ActivateDoublePoints(pickupType.duration);
        DebugLog($"Double Points activated for {pickupType.duration} seconds");
    }

    private void HandleMagnet(PickupType pickupType)
    {
        playerManager.ActivateMagnet(pickupType.duration);
        DebugLog($"Magnet activated for {pickupType.duration} seconds");
    }

    private void HandleExtraLife(PickupType pickupType)
    {
        playerManager.AddExtraLife(pickupType.value);
        DebugLog($"Added {pickupType.value} extra life/lives");
    }

    // private void HandleObstacle(PickupType pickupType)
    // {
    //     playerManager.TakeDamage(pickupType.value);
    //     DebugLog($"Took {pickupType.value} damage from obstacle");
    // }

    private void HandleJumpBoost(PickupType pickupType)
    {
        playerManager.ActivateJumpBoost(pickupType.duration);
        DebugLog($"Jump Boost activated for {pickupType.duration} seconds");
    }

    private void HandleTimeFreeze(PickupType pickupType)
    {
        // Implement time freeze logic
        // You might want to add this to PlayerManager
        DebugLog($"Time Freeze activated for {pickupType.duration} seconds");
    }

    private void HandleMultiplier(PickupType pickupType)
    {
        // If you have a ScoreManager, uncomment this:
        // if (playerManager.scoreManager != null)
        //     playerManager.scoreManager.ActivateScoreMultiplier(pickupType.value, pickupType.duration);
        DebugLog($"Score Multiplier x{pickupType.value} activated");
    }

    private void PlayPickupEffects()
    {
        // Play sound effect if assigned
        // if (audioSource != null)
        // {
        //     audioSource.clip = pickupSound;
        //     audioSource.Play();
        // }
        
        // Spawn particle effect if assigned
        if (pickupParticleEffect != null)
        {
            GameObject effect = Instantiate(pickupParticleEffect, transform.position, Quaternion.identity);
            
            // Auto-destroy the particle effect after 2 seconds
            ParticleSystem particleSystem = effect.GetComponent<ParticleSystem>();
            if (particleSystem != null)
            {
                float duration = particleSystem.main.duration + particleSystem.main.startLifetime.constant;
                Destroy(effect, duration);
            }
            else
            {
                Destroy(effect, 2f);
            }
        }
    }
    
    // Conditional debug logging to reduce performance impact
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    private void DebugLog(string message)
    {
        Debug.Log(message);
    }
}

// using UnityEngine;
// using System;

// public class PickupEffect : MonoBehaviour
// {
//     public PlayerManager playerManager; // Reference to player management script
//     // private ScoreManager scoreManager;   // Reference to score management script

//     // private void Awake()
//     // {
//         // Find necessary managers
//         // playerManager = FindObjectOfType<PlayerManager>();
//         // scoreManager = FindObjectOfType<ScoreManager>();
//     // }

//     private void OnTriggerEnter(Collider other)
//     {
//         Debug.Log($"Trigger entered by: {other.name} with tag: {other.tag}");
//         if (other.gameObject.tag == "Player")
//         {
//             PickupType pickupType = GetComponent<PickupType>();
            
//             if (pickupType != null)
//             {
//                 ApplyPickupEffect(pickupType);
                
//                 // Optional: Add visual and sound effects
//                 PlayPickupEffects();
                
//                 // Destroy the pickup object
//                 Destroy(gameObject);
//             }
//         }
//     }

//     private void ApplyPickupEffect(PickupType pickupType)
//     {
//         switch (pickupType.pickupType)
//         {
//             case PickupType.ObjectType.Coin:
//                 HandleCoinPickup(pickupType);
//                 break;
//             case PickupType.ObjectType.SpeedBoost:
//                 HandleSpeedBoost(pickupType);
//                 break;
//             case PickupType.ObjectType.Shield:
//                 HandleShield(pickupType);
//                 break;
//             case PickupType.ObjectType.DoublePoints:
//                 HandleDoublePoints(pickupType);
//                 break;
//             case PickupType.ObjectType.Magnet:
//                 HandleMagnet(pickupType);
//                 break;
//             case PickupType.ObjectType.ExtraLife:
//                 HandleExtraLife(pickupType);
//                 break;
//             case PickupType.ObjectType.Obstacle:
//                 HandleObstacle(pickupType);
//                 break;
//             case PickupType.ObjectType.JumpBoost:
//                 HandleJumpBoost(pickupType);
//                 break;
//             case PickupType.ObjectType.TimeFreeze:
//                 HandleTimeFreeze(pickupType);
//                 break;
//             case PickupType.ObjectType.Multiplier:
//                 HandleMultiplier(pickupType);
//                 break;
//         }
//     }

//     // Specific Pickup Effect Methods
//     private void HandleCoinPickup(PickupType pickupType)
//     {
//         // Add coins to player's score
//         // scoreManager.AddCoins(pickupType.value);
//         Debug.Log($"Collected {pickupType.value} coins");
//     }

//     private void HandleSpeedBoost(PickupType pickupType)
//     {
//         playerManager.ActivateSpeedBoost(pickupType.duration);
        
//     }

//     private void HandleShield(PickupType pickupType)
//     {
//         playerManager.ActivateShield(pickupType.duration);
//         Debug.Log($"Shield activated for {pickupType.duration} seconds");
//     }

//     private void HandleDoublePoints(PickupType pickupType)
//     {
//         // scoreManager.ActivateDoublePoints(pickupType.duration);
//         Debug.Log($"Double Points activated for {pickupType.duration} seconds");
//     }

//     private void HandleMagnet(PickupType pickupType)
//     {
//         playerManager.ActivateMagnet(pickupType.duration);
//         Debug.Log($"Magnet activated for {pickupType.duration} seconds");
//     }

//     private void HandleExtraLife(PickupType pickupType)
//     {
//         playerManager.AddExtraLife(pickupType.value);
//         Debug.Log($"Added {pickupType.value} extra life/lives");
//     }

//     private void HandleObstacle(PickupType pickupType)
//     {
//         playerManager.TakeDamage(pickupType.value);
//         Debug.Log($"Took {pickupType.value} damage from obstacle");
//     }

//     private void HandleJumpBoost(PickupType pickupType)
//     {
//         playerManager.ActivateJumpBoost(pickupType.duration);
//         Debug.Log($"Jump Boost activated for {pickupType.duration} seconds");
//     }

//     private void HandleTimeFreeze(PickupType pickupType)
//     {
//         // Implement time freeze logic
//         Debug.Log($"Time Freeze activated for {pickupType.duration} seconds");
//     }

//     private void HandleMultiplier(PickupType pickupType)
//     {
//         // scoreManager.ActivateScoreMultiplier(pickupType.value, pickupType.duration);
//         Debug.Log($"Score Multiplier x{pickupType.value} activated");
//     }

//     private void PlayPickupEffects()
//     {
//         // Optional: Add sound and visual effects
//         // AudioSource.PlayClipAtPoint(pickupSound, transform.position);
//         // Instantiate(pickupParticleEffect, transform.position, Quaternion.identity);
//     }
// }