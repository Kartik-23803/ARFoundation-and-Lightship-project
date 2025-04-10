using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthBar : MonoBehaviour
{
    GameObject foreground = null;
    RectTransform foregroundTransform = null;
    // [SerializeField] Canvas rootCanvas = null;
    PlayerHealth healthComponent = null;

    void Start()
    {
        healthComponent = GetComponent<PlayerHealth>();
        foreground = GameObject.Find("Foreground");
        foregroundTransform = foreground.GetComponent<RectTransform>();
        // rootCanvas.enabled = false;
        // GetComponentInChildren<Canvas>().enabled = false;
    }

    // public void EnemyHealthBar(float value)
    void Update()
    {
        // if(Mathf.Approximately(healthComponent.GetFraction(), 0))
        if (Mathf.Approximately(healthComponent.GetFraction(), 0) || Mathf.Approximately(healthComponent.GetFraction(), 1))
        {
            // rootCanvas.enabled = false;
            return;
            // GetComponentInChildren<Canvas>().enabled = false;
        }

        // rootCanvas.enabled = true;
        // GetComponentInChildren<Canvas>().enabled = true;
        foregroundTransform.localScale = new Vector3(healthComponent.GetFraction(), 1, 1);
    }
}
