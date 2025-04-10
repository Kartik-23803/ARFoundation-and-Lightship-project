using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.NavigationMesh;
using TMPro;
using UnityEngine;


public class PlayerHealth : MonoBehaviour
{
    [SerializeField] float playerHitPoints = 200f;
    GameObject mainCanvas;
    NavigationMeshController navMesh;
    SpawnManager spawnManager;
    AudioManager audioManager;
    bool isDead;

    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        isDead = false;
        mainCanvas = GameObject.Find("Canvas");
        mainCanvas.SetActive(true);
        navMesh = FindObjectOfType<NavigationMeshController>();
        spawnManager = FindObjectOfType<SpawnManager>();
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void TakeDamage(float damage)
    {
        playerHitPoints -= damage;
        if (playerHitPoints <= 0)
        {
            Die();
        }
        Debug.Log($"player health: {playerHitPoints}");
    }

    public float GetHP()
    {
        return playerHitPoints;
    }

    public float GetFraction()
    {
        return playerHitPoints/200;
    }

    private void Die()
    {
        if (isDead == true) return;

        isDead = true;
        playerHitPoints = 0;
        HandleDeath();
    }

    private void HandleDeath()
    {
        GetComponentInChildren<Animator>().SetTrigger("dead");
        mainCanvas.SetActive(false);
        gameObject.GetComponent<LightshipNavMeshAgent>().StopMoving();
        navMesh.endCanvas.GetComponentInChildren<TextMeshProUGUI>().text =
                    $"Game Over\nYou Scored: {MarioScoreManager.Instance.score}";
        navMesh.endCanvas.SetActive(true);
        navMesh.endCanvas.GetComponent<Animator>().SetTrigger("game over");
        audioManager.gameAudio.Stop();
        audioManager.PlayDeathSound();
        StopCoroutine(spawnManager.ContinuousSpawning());
    }
}

