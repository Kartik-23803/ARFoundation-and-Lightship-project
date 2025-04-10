using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MarioScoreManager : MonoBehaviour
{
    public static MarioScoreManager Instance { get; private set; }
    public int score = 0;
    [SerializeField] ScoreDisplay scoreDisplay;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    public void AddCoinScore()
    {
        score += 100;
        scoreDisplay.UpdateScore(score);
    }

    public int GetScore()
    {
        return score;
    }
}
