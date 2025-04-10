using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using UnityEngine.XR.ARFoundation;

public class ScoreManager : MonoBehaviour
{
    public int scoreCount = 0;
    [SerializeField] List<Sprite> scoreSprites = new List<Sprite>();
    [SerializeField] private GameObject imagePrefab;    
    [SerializeField] private float scoreYOffset = 0.2f; 
    [SerializeField] private float scoreSize = 0.3f;    
    [SerializeField] GameObject confetti = null;
    [SerializeField] GameObject screenshot = null;

    private GameObject scoreDisplay;
    private ARFace trackedFace;

    void Start()
    {
        scoreCount = 0;
        confetti.SetActive(false);
        screenshot.SetActive(false);
    }

    public void SetTrackedFace(ARFace face)
    {
        trackedFace = face;
    }

    public void DisplayFinalScore()
    {
        if (trackedFace == null || scoreSprites.Count == 0) return;

        // Create score display
        scoreDisplay = Instantiate(imagePrefab);
        
        // Setup Canvas
        Canvas canvas = scoreDisplay.GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(scoreSize, scoreSize);

        // Setup Image
        Image image = scoreDisplay.GetComponentInChildren<Image>();
        RectTransform imageRect = image.GetComponent<RectTransform>();
        imageRect.sizeDelta = new Vector2(scoreSize, scoreSize);
        
        // Set the appropriate score sprite based on score count
        int spriteIndex = Mathf.Clamp(scoreCount, 0, scoreSprites.Count - 1);
        image.sprite = scoreSprites[spriteIndex];
        image.preserveAspect = true;

        // Position and parent
        scoreDisplay.transform.parent = trackedFace.transform;
        scoreDisplay.transform.localPosition = new Vector3(0, scoreYOffset, 0);
        confetti.SetActive(true);
        screenshot.SetActive(true);
    }

    public void UpdateScoreRotation()
    {
        if (scoreDisplay != null)
        {
            Quaternion cameraRotation = Camera.main.transform.rotation;
            Vector3 euler = cameraRotation.eulerAngles;
            euler.z = 0;
            scoreDisplay.transform.rotation = Quaternion.Euler(euler);
        }
    }

    public void CleanUp()
    {
        if (scoreDisplay != null)
        {
            Destroy(scoreDisplay);
        }
    }
}