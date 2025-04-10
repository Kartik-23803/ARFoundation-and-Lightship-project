using UnityEngine;
using TMPro;

public class ScoreDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI scoreText;
    
    private void Start()
    {
        if (scoreText == null)
        {
            scoreText = GetComponent<TextMeshProUGUI>();
        }
        
        UpdateScore(MarioScoreManager.Instance.score);
        
        // InvokeRepeating("UpdateScore", 0.1f, 0.5f);
    }
    
    public void UpdateScore(int points)
    {
        if (scoreText != null && MarioScoreManager.Instance != null)
        {
            scoreText.text = points.ToString();
        }
    }
}