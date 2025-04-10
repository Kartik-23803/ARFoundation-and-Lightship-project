using UnityEngine;
using Niantic.Lightship.AR.ObjectDetection;

public class LogResults : MonoBehaviour
{
    [SerializeField]
    private ARObjectDetectionManager _objectDetectionManager;

    private void Start()
    {
        _objectDetectionManager.enabled = true;
        _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;
    }

    private void OnMetadataInitialized(ARObjectDetectionModelEventArgs args)
    {
        _objectDetectionManager.ObjectDetectionsUpdated += ObjectDetectionsUpdated;
    }

    private void ObjectDetectionsUpdated(ARObjectDetectionsUpdatedEventArgs args)
    {
        // Initialize our output string
        string resultString = "";
        var result = args.Results;

        if (result == null)
        {
            return;
        }

        // Reset our results string
        resultString = "";

        // Iterate through our results. Each result can contain more than one category.
        for (int i = 0; i < result.Count; i++)
        {
            var detection = result[i];
            var categorizations = detection.GetConfidentCategorizations();
            if (categorizations.Count <= 0)
            {
                break;
            }

            // Sort our categorizations by highest confidence
            categorizations.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));

            // List each category that this object could be
            for (int j = 0; j < categorizations.Count; j++)
            {
                var categoryToDisplay = categorizations[j];

                resultString += "Detected " + $"{categoryToDisplay.CategoryName}: " + "with " + $"{categoryToDisplay.Confidence} Confidence \n";
            }
        }

        // Log all results
        Debug.Log(resultString);
    }


    private void OnDestroy()
    {
        _objectDetectionManager.MetadataInitialized -= OnMetadataInitialized;
        _objectDetectionManager.ObjectDetectionsUpdated -= ObjectDetectionsUpdated;
    }
}