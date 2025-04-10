using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    PlayerHealth playerHealth;

    void Start()
    {
        playerHealth = GetComponent<PlayerHealth>();
    }

    public void ActivateSpeedBoost(float duration)
    {
        Debug.Log("Activted speed boost");
    }

    public void ActivateShield(float duration)
    {
        Debug.Log("Activted shield");
    }

    public void ActivateMagnet(float duration)
    {
        Debug.Log("Activted magnet");
    }

    public void AddExtraLife(float duration)
    {
        Debug.Log("Added extra life");
    }

    public void TakeDamage(float damage)
    {
        playerHealth.TakeDamage(damage);
        Debug.Log("damage taken");
    }
    
    public void ActivateJumpBoost(float duration)
    {
        Debug.Log("Activted jump boost");
    }
}
