    using Niantic.Lightship.AR.ObjectDetection;
    using UnityEngine;

    public class ObjectDetectionSample: MonoBehaviour
    {
        [SerializeField]
        private float _probabilityThreshold = 0.5f;

        [SerializeField]
        private ARObjectDetectionManager _objectDetectionManager;

        private Color[] _colors = new Color[]
        {
            Color.red,
            Color.blue,
            Color.green,
            Color.yellow,
            Color.magenta,
            Color.cyan,
            Color.white,
            Color.black
        };

        [SerializeField]
        private DrawRect _drawRect;

        private Canvas _canvas;

        private void Awake()
        {
            _canvas = FindObjectOfType<Canvas>();
        }

        public void Start()
        {
            _objectDetectionManager.enabled = true;
            _objectDetectionManager.MetadataInitialized += OnMetadataInitialized;
        }

        private void OnDestroy()
        {
            _objectDetectionManager.MetadataInitialized -= OnMetadataInitialized;
            _objectDetectionManager.ObjectDetectionsUpdated -= ObjectDetectionsUpdated;
        }

        private void OnMetadataInitialized(ARObjectDetectionModelEventArgs args)
        {
            _objectDetectionManager.ObjectDetectionsUpdated += ObjectDetectionsUpdated;
        }

        private void ObjectDetectionsUpdated(ARObjectDetectionsUpdatedEventArgs args)
        {
            string resultString = "";
            float _confidence = 0;
            string _name = "";
            var result = args.Results;
            if (result == null)
            {
                return;
            }

            _drawRect.ClearRects();

            for (int i = 0; i < result.Count; i++)
            {
                var detection = result[i];
                var categorizations = detection.GetConfidentCategorizations(_probabilityThreshold);
                if (categorizations.Count <= 0)
                {
                    break;
                }

                categorizations.Sort((a, b) => b.Confidence.CompareTo(a.Confidence));
                var categoryToDisplay = categorizations[0];
                _confidence = categoryToDisplay.Confidence;
                _name = categoryToDisplay.CategoryName;

                int h = Mathf.FloorToInt(_canvas.GetComponent<RectTransform>().rect.height);
                int w = Mathf.FloorToInt(_canvas.GetComponent<RectTransform>().rect.width);

                // Get the rect around the detected object
                var _rect = result[i].CalculateRect(w,h,Screen.orientation);

                // resultString = $"{_name}: {_confidence}\n";
                resultString = $"{_name}\n\n";
                // Draw the rect
                _drawRect.CreateRect(_rect, _colors[i % _colors.Length], resultString);
            }
        }
    }