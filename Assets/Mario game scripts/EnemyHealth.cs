using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyHealth : MonoBehaviour
{
    [SerializeField] float hitPoints = 100f;
    // AudioSource deathAudio;
    // AudioClip sound;
    Rigidbody rb;
    bool isDead = false;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        // deathAudio = GetComponent<AudioSource>();
        // deathAudio.enabled = false;
        if(rb == null) return;
        rb.isKinematic = false;
    }

    public bool IsDead()
    {
        return isDead;
    }

    public void TakeDamage(float damage)
    {
        hitPoints -= damage;
        if(hitPoints <= 0)
        {
            rb.isKinematic = true;
            Die();
        }
    }

    private void Die()
    {
        if(isDead) { return; }
        isDead = true;
        // deathAudio.PlayOneShot(sound);
        GetComponent<Animator>().SetTrigger("Dead");
        // deathAudio.enabled = true;
        GetComponent<SphereCollider>().enabled = false;
    }

    public float GetEnemyHP()
    {
        return hitPoints;
    }
}
