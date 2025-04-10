using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class FaceMeshRotation : MonoBehaviour
{
    // [SerializeField] TextMeshProUGUI faceRot;
    [SerializeField] QuestionManager questionManager;
    [SerializeField] float rotationThreshold = 10f;
    Vector3 playerPos;
    ARFaceManager aRFaceManager;
    public ARFace trackedFace;

    void Start()
    {
        aRFaceManager = FindObjectOfType<ARFaceManager>();
        // Subscribe to the face detected event
        aRFaceManager.facesChanged += OnFacesChanged;
    }

    void OnFacesChanged(ARFacesChangedEventArgs args)
    {
        // When a new face is added
        foreach (ARFace face in args.added)
        {
            trackedFace = face;
        }
        // When a face is removed
        foreach (ARFace face in args.removed)
        {
            if (trackedFace == face)
                trackedFace = null;
        }
    }

    void Update()
    {
        if (trackedFace != null)
        {
            // Get the rotation in degrees and format it to 1 decimal place
            float zRotation = trackedFace.transform.eulerAngles.z;
            // faceRot.text = zRotation.ToString("F1");
            playerPos = trackedFace.transform.position;
            if (zRotation > 180) zRotation -= 360;

            // Check head tilt
            if (Mathf.Abs(zRotation) > rotationThreshold)
            {
                if (zRotation > 0)
                {
                    // Head tilted right - Yes
                    questionManager.CheckAnswer(true);
                }
                else
                {
                    // Head tilted left - No
                    questionManager.CheckAnswer(false);
                }
            }
        }
        // else
        // {
        //     faceRot.text = "No face detected";
        // }
    }

    void OnDestroy()
    {
        // Unsubscribe from the event when the script is destroyed
        if (aRFaceManager != null)
            aRFaceManager.facesChanged -= OnFacesChanged;
    }

    public Vector3 GetFacePosition()
    {
        return playerPos;
    }
}