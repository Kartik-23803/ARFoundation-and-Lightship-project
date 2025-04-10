using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class QuestionManager : MonoBehaviour
{
    [SerializeField] QuestionSetSO questionSet;
    [SerializeField] GameObject imagePrefab;    
    // [SerializeField] GameObject buttonPrefab;    
    [SerializeField] Sprite yesSprite;
    [SerializeField] Sprite noSprite;

    [Header("Position Settings")]
    [SerializeField] float questionYOffset = 0.3f;  
    [SerializeField] float optionsYOffset = 0.2f;   
    [SerializeField] float optionsXOffset = 0.2f;   

    [Header("Size Settings")]
    [SerializeField] float questionSize = 0.2f;     
    [SerializeField] float optionSize = 0.1f;       

    [Header("Feedback Images")]
    [SerializeField] Sprite correctSprite;
    [SerializeField] Sprite wrongSprite;

    [SerializeField] Canvas screenshotCanvas;
    // [SerializeField] Button screenshotButton;

    [Header("End Game UI")]
    [SerializeField] List<Sprite> scoreSprites;
    [SerializeField] ParticleSystem confettiEffect;

    [Header("Supporting Image Settings")]
    [SerializeField] float supportImageYOffset = -0.3f;
    [SerializeField] float supportImageSize = 0.3f;
    [SerializeField] float confettiZOffset = -1f;

    GameObject uiContainer;
    GameObject supportImageObject;
    GameObject scoreObject;
    // GameObject ssButton;
    GameObject questionObject;
    GameObject yesObject;
    GameObject noObject;
    GameObject feedbackObject;
    GameObject ssButton;

    int currentQuestionIndex = 0;
    bool isAnswering = false;
    ARFace trackedFace;
    ARFaceManager faceManager;
    public int scoreCount = 0;
    bool isGameComplete = false;

    ScreenshotManager screenshotManager;

    void Start()
    {
        faceManager = FindObjectOfType<ARFaceManager>();
        faceManager.facesChanged += OnFacesChanged;
        screenshotManager = FindObjectOfType<ScreenshotManager>();

        if(screenshotCanvas!=null)
        {
            screenshotCanvas.enabled = false;
        }
    }

    void InstantiateUIElements()
    {
        if (questionSet == null || questionSet.questions.Count == 0) return;

        uiContainer = new GameObject("UI Container");
        uiContainer.transform.parent = trackedFace.transform;
        uiContainer.transform.localPosition = Vector3.zero;

        if(isGameComplete)
        {
            ShowEndGameUI();
        }
        else
        {
            if(imagePrefab == null) Debug.LogError("No image prefab");
            else
            {
                questionObject = Instantiate(imagePrefab, uiContainer.transform);
                SetupUIElement(questionObject, new Vector3(0, questionYOffset, 0), questionSize, 
                    questionSet.questions[currentQuestionIndex].questionImage);

                yesObject = Instantiate(imagePrefab, uiContainer.transform);
                SetupUIElement(yesObject, new Vector3(optionsXOffset, optionsYOffset, 0), 
                    optionSize, yesSprite);

                noObject = Instantiate(imagePrefab, uiContainer.transform);
                SetupUIElement(noObject, new Vector3(-optionsXOffset, optionsYOffset, 0), 
                    optionSize, noSprite);

                SetupFeedbackImage();

                if (questionSet.questions[currentQuestionIndex].hasSupportingImage &&
                questionSet.questions[currentQuestionIndex].supportingImage != null)
                {
                    SetupSupportingImage();
                }
            }
        }
    }

    void SetupSupportingImage()
    {
        if (supportImageObject != null)
        {
            Destroy(supportImageObject);
        }

        supportImageObject = Instantiate(imagePrefab);
        Canvas supportCanvas = supportImageObject.GetComponentInChildren<Canvas>();
        supportCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        supportCanvas.sortingOrder = 50;

        Image supportImage = supportImageObject.GetComponentInChildren<Image>();
        supportImage.sprite = questionSet.questions[currentQuestionIndex].supportingImage;
        supportImage.preserveAspect = true;

        RectTransform supportRect = supportImage.GetComponent<RectTransform>();
        supportRect.anchorMin = new Vector2(0.5f, 0);
        supportRect.anchorMax = new Vector2(0.5f, 0);
        supportRect.pivot = new Vector2(0.5f, 0);
        supportRect.anchoredPosition = new Vector2(0, 50);
        supportRect.sizeDelta = new Vector2(400, 200);
        supportRect.anchoredPosition = new Vector2(0,supportImageYOffset);
        supportRect.localScale = new Vector2(supportImageSize, supportImageSize);
    }

    void SetupFeedbackImage()
    {
        feedbackObject = Instantiate(imagePrefab, uiContainer.transform);
        SetupUIElement(feedbackObject, new Vector3(0, optionsYOffset, 0), optionSize, null);
        feedbackObject.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        feedbackObject.GetComponentInChildren<Image>().enabled = false;
    }

    void SetupUIElement(GameObject uiElement, Vector3 localPosition, float size, Sprite sprite)
    {
        Canvas canvas = uiElement.GetComponentInChildren<Canvas>();
        canvas.worldCamera = Camera.main;
        RectTransform canvasRect = canvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(size, size);

        Image image = uiElement.GetComponentInChildren<Image>();
        RectTransform imageRect = image.GetComponent<RectTransform>();
        imageRect.sizeDelta = new Vector2(size, size);
        image.sprite = sprite;
        image.preserveAspect = true;

        uiElement.transform.localPosition = localPosition;
    }

    void Update()
    {
        if (trackedFace != null && uiContainer != null)
        {
            Quaternion cameraRotation = Camera.main.transform.rotation;
            Vector3 euler = cameraRotation.eulerAngles;
            euler.z = 0;
            uiContainer.transform.rotation = Quaternion.Euler(euler);

            float zRotation = trackedFace.transform.eulerAngles.z;
            if (zRotation > 180) zRotation -= 360;

            if (Mathf.Abs(zRotation) > 15f && !isAnswering)
            {
                CheckAnswer(zRotation > 0);
            }
        }
    }

    public void CheckAnswer(bool playerAnswer)
    {
        if (!isAnswering && currentQuestionIndex < questionSet.questions.Count)
        {
            isAnswering = true;
            bool correctAnswer = questionSet.questions[currentQuestionIndex].correctAnswer;

            if (feedbackObject != null)
            {
                Image feedbackImage = feedbackObject.GetComponentInChildren<Image>();
                feedbackImage.enabled = true;

                if (playerAnswer == correctAnswer)
                {
                    Debug.Log("Correct!");
                    feedbackImage.sprite = correctSprite;
                    scoreCount++;
                }
                else
                {
                    Debug.Log("Wrong!");
                    feedbackImage.sprite = wrongSprite;
                }
            }

            Invoke("NextQuestion", 1.5f);
        }
    }

    void NextQuestion()
    {
        currentQuestionIndex++;
        isAnswering = false;

        if (feedbackObject != null)
        {
            feedbackObject.GetComponentInChildren<Image>().enabled = false;
        }

        if (currentQuestionIndex < questionSet.questions.Count)
        {
            if (questionObject != null)
            {
                Image questionImage = questionObject.GetComponentInChildren<Image>();
                if (questionImage != null)
                {
                    questionImage.sprite = questionSet.questions[currentQuestionIndex].questionImage;
                }
            }

            if (supportImageObject != null)
            {
                Destroy(supportImageObject);
            }
            
            if (questionSet.questions[currentQuestionIndex].hasSupportingImage &&
                questionSet.questions[currentQuestionIndex].supportingImage != null)
            {
                SetupSupportingImage();
            }
        }
        else
        {
            Debug.Log("Game Complete!");
            isGameComplete = true;
            DestroyUIElements();
            ShowEndGameUI();
        }
    }

    

    void ShowEndGameUI()
    {
        uiContainer = new GameObject("End Game UI Container");
        uiContainer.transform.parent = trackedFace.transform;
        uiContainer.transform.localPosition = Vector3.zero;

        if(imagePrefab != null)
        {
            scoreObject = Instantiate(imagePrefab, uiContainer.transform);
            SetupUIElement(scoreObject, new Vector3(0, optionsYOffset + 0.1f, 0), questionSize, 
                scoreSprites[Mathf.Clamp(scoreCount, 0, scoreSprites.Count - 1)]);

            if (confettiEffect != null)
            {
                confettiEffect.gameObject.SetActive(true);
                confettiEffect.transform.parent = trackedFace.transform;
                confettiEffect.transform.localPosition = new Vector3(0, 0, confettiZOffset);
                confettiEffect.transform.localRotation = Quaternion.Euler(-90, 0, 0);
                confettiEffect.Play();
            }
            if (screenshotCanvas != null)
            {
                imagePrefab.SetActive(false);
                screenshotCanvas.enabled = true;
                screenshotCanvas.GetComponentInChildren<Button>().onClick.AddListener(screenshotManager.ClickShare);
            }
        }
        else
        {
            Debug.LogError("No image prefab");
        }
    }

    void DestroyUIElements()
    {
        if (uiContainer != null) Destroy(uiContainer);
        if (!isGameComplete && confettiEffect != null) confettiEffect.Stop();
        if (supportImageObject != null) Destroy(supportImageObject);
    }

    void OnFacesChanged(ARFacesChangedEventArgs args)
    {
        foreach (ARFace face in args.added)
        {
            trackedFace = face;
            InstantiateUIElements();
        }

        foreach (ARFace face in args.removed)
        {
            if (trackedFace == face)
            {
                trackedFace = null;
                DestroyUIElements();
            }
        }
    }

    void OnDestroy()
    {
        if (faceManager != null)
        {
            faceManager.facesChanged -= OnFacesChanged;
        }
    }

    public void ButtonPressed()
    {
        Debug.LogError("Button pressed");
    }
}

// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.UI;
// using UnityEngine.XR.ARFoundation;

// public class QuestionManager : MonoBehaviour
// {
//     [SerializeField] QuestionSetSO questionSet;
//     [SerializeField] GameObject imagePrefab;    
//     [SerializeField] Sprite yesSprite;
//     [SerializeField] Sprite noSprite;
    
//     [Header("Position Settings")]
//     [SerializeField] float questionYOffset = 0.3f;  
//     [SerializeField] float optionsYOffset = 0.2f;   
//     [SerializeField] float optionsXOffset = 0.2f;   
    
//     [Header("Size Settings")]
//     [SerializeField] float questionSize = 0.2f;     
//     [SerializeField] float optionSize = 0.1f;

//     [Header("Feedback Images")]
//     [SerializeField] Sprite correctSprite;
//     [SerializeField] Sprite wrongSprite;    

//     [Header("Score Display")]
//     [SerializeField] List<Sprite> scoreSprites;

//     [Header("Effects")]
//     [SerializeField] ParticleSystem scoreParticleSystem;
//     [SerializeField] Canvas screenshotCanvas;

// GameObject scoreDisplayObject;

//     GameObject questionObject;
//     GameObject yesObject;
//     GameObject noObject;
//     int currentQuestionIndex = 0;
//     bool isAnswering = false;
//     ARFace trackedFace;
//     ARFaceManager faceManager;
//     GameObject feedbackObject;
//     int scoreCount = 0;

//     void Start()
//     {
//         scoreCount = 0;
//         faceManager = FindObjectOfType<ARFaceManager>();
//         faceManager.facesChanged += OnFacesChanged;
//     }

//     void InstantiateUIElements()
//     {
//         if (questionSet == null || questionSet.questions.Count == 0) return;

//         // Create question image
//         questionObject = Instantiate(imagePrefab);
//         SetupUIElement(questionObject, new Vector3(0, questionYOffset, 0), questionSize, 
//             questionSet.questions[currentQuestionIndex].questionImage);

//         // Create YES option
//         yesObject = Instantiate(imagePrefab);
//         SetupUIElement(yesObject, new Vector3(optionsXOffset, optionsYOffset, 0), 
//             optionSize, yesSprite);

//         // Create NO option
//         noObject = Instantiate(imagePrefab);
//         SetupUIElement(noObject, new Vector3(-optionsXOffset, optionsYOffset, 0), 
//             optionSize, noSprite);
//     }

//     void SetupUIElement(GameObject uiElement, Vector3 localPosition, float size, Sprite sprite)
//     {
//         // Setup Canvas
//         Canvas canvas = uiElement.GetComponentInChildren<Canvas>();
//         canvas.worldCamera = Camera.main;
//         RectTransform canvasRect = canvas.GetComponent<RectTransform>();
//         canvasRect.sizeDelta = new Vector2(size, size);

//         // Setup Image
//         Image image = uiElement.GetComponentInChildren<Image>();
//         RectTransform imageRect = image.GetComponent<RectTransform>();
//         imageRect.sizeDelta = new Vector2(size, size);
//         image.sprite = sprite;
//         image.preserveAspect = true;

//         // Position and parent
//         uiElement.transform.parent = trackedFace.transform;
//         uiElement.transform.localPosition = localPosition;
//         // Set initial rotation to face camera
//         // uiElement.transform.rotation = Camera.main.transform.rotation;
//     }

//     void Update()
//     {
//         if (trackedFace != null && questionObject != null)
//         {
//             // Make UI elements face the camera with corrected rotation
//             Quaternion cameraRotation = Camera.main.transform.rotation;
//             Vector3 euler = cameraRotation.eulerAngles;
//             euler.z = 0; // Keep z rotation at 0 to prevent flipping
//             Quaternion correctedRotation = Quaternion.Euler(euler);

//             questionObject.transform.rotation = correctedRotation;
//             if (yesObject != null) yesObject.transform.rotation = correctedRotation;
//             if (noObject != null) noObject.transform.rotation = correctedRotation;

//             // Check for head tilt
//             float zRotation = trackedFace.transform.eulerAngles.z;
//             if (zRotation > 180) zRotation -= 360;

//             if (Mathf.Abs(zRotation) > 15f && !isAnswering)
//             {
//                 CheckAnswer(zRotation > 0);
//             }
//         }
//     }

//     public void CheckAnswer(bool playerAnswer)
//     {
//         if (!isAnswering && currentQuestionIndex < questionSet.questions.Count)
//         {
//             isAnswering = true;
//             bool correctAnswer = questionSet.questions[currentQuestionIndex].correctAnswer;
            
//             if (feedbackObject != null)
//             {
//                 Image feedbackImage = feedbackObject.GetComponentInChildren<Image>();
//                 feedbackImage.enabled = true;
                
//                 if (playerAnswer == correctAnswer)
//                 {
//                     Debug.Log("Correct!");
//                     feedbackImage.sprite = correctSprite;
//                     scoreCount++;
//                 }
//                 else
//                 {
//                     Debug.Log("Wrong!");
//                     feedbackImage.sprite = wrongSprite;
//                 }
//             }

//             Invoke("NextQuestion", 1.5f);
//         }
//     }

//     // public void CheckAnswer(bool playerAnswer)
//     // {
//     //     if (!isAnswering && currentQuestionIndex < questionSet.questions.Count)
//     //     {
//     //         isAnswering = true;
//     //         bool correctAnswer = questionSet.questions[currentQuestionIndex].correctAnswer;
            
//     //         if (playerAnswer == correctAnswer)
//     //         {
//     //             Debug.Log("Correct!");
//     //             if (trackedFace != null)
//     //             {
//     //                 trackedFace.GetComponent<MeshRenderer>().material.color = Color.green;
//     //             }
//     //         }
//     //         else
//     //         {
//     //             Debug.Log("Wrong!");
//     //             if (trackedFace != null)
//     //             {
//     //                 trackedFace.GetComponent<MeshRenderer>().material.color = Color.red;
//     //             }
//     //         }

//     //         Invoke("NextQuestion", 1.5f);
//     //     }
//     // }

//     void NextQuestion()
//     {
//         currentQuestionIndex++;
//         isAnswering = false;

//         if (currentQuestionIndex < questionSet.questions.Count)
//         {
//             // Update question image
//             if (questionObject != null)
//             {
//                 Image questionImage = questionObject.GetComponentInChildren<Image>();
//                 if (questionImage != null)
//                 {
//                     questionImage.sprite = questionSet.questions[currentQuestionIndex].questionImage;
//                 }
//             }
//         }
//         else
//         {
//             Debug.Log("Game Complete!");
//             DestroyUIElements();
//         }
//     }

//     void DestroyUIElements()
//     {
//         if (questionObject != null) Destroy(questionObject);
//         if (yesObject != null) Destroy(yesObject);
//         if (noObject != null) Destroy(noObject);
//     }

//     void OnFacesChanged(ARFacesChangedEventArgs args)
//     {
//         foreach (ARFace face in args.added)
//         {
//             trackedFace = face;
//             InstantiateUIElements();
//         }

//         foreach (ARFace face in args.removed)
//         {
//             if (trackedFace == face)
//             {
//                 trackedFace = null;
//                 DestroyUIElements();
//             }
//         }
//     }

//     void ShowFinalScore()
//     {
//         // Remove yes/no options
//         if (yesObject != null) Destroy(yesObject);
//         if (noObject != null) Destroy(noObject);

//         // Display final score
//         if (questionObject != null && scoreSprites != null && scoreCount < scoreSprites.Count)
//         {
//             Image questionImage = questionObject.GetComponentInChildren<Image>();
//             if (questionImage != null)
//             {
//                 questionImage.sprite = scoreSprites[scoreCount];
//             }

//             // Play particle effect
//             if (scoreParticleSystem != null)
//             {
//                 scoreParticleSystem.Play();
//             }

//             // Enable screenshot canvas
//             if (screenshotCanvas != null)
//             {
//                 screenshotCanvas.enabled = true;
//             }
//         }
//     }

//     void OnDestroy()
//     {
//         if (faceManager != null)
//         {
//             faceManager.facesChanged -= OnFacesChanged;
//         }
//     }
// }