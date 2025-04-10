// using UnityEngine;
// using UnityEngine.XR.ARFoundation;
// using System.Collections.Generic;

// [RequireComponent(typeof(ARPlaneManager))]
// public class ARPlanColorizer : MonoBehaviour
// {
//     [Header("Plane Detection Settings")]
//     [SerializeField] private float minPlaneSize = 0.2f;
//     [SerializeField] private float maxPlaneSize = 10f;
//     [SerializeField] private bool detectHorizontal = true;
//     [SerializeField] private bool detectVertical = true;
//     [SerializeField] private Material horizontalPlaneMaterial;
//     [SerializeField] private Material verticalPlaneMaterial;

//     private ARPlaneManager planeManager;
//     private List<ARPlane> activePlanes = new List<ARPlane>();

//     private void Awake()
//     {
//         planeManager = GetComponent<ARPlaneManager>();
//     }

//     private void OnEnable()
//     {
//         planeManager.planesChanged += OnPlanesChanged;
//     }

//     private void OnDisable()
//     {
//         planeManager.planesChanged -= OnPlanesChanged;
//     }

//     private void Start()
//     {
//         ConfigurePlaneDetection();
//     }

//     private void ConfigurePlaneDetection()
//     {
//         // Configure plane detection modes
//         planeManager.requestedDetectionMode = GetDetectionMode();

//         // Set other AR Plane Manager properties
//         // planeManager.planePrefab = CreateCustomPlanePrefab();
//     }

//     private UnityEngine.XR.ARSubsystems.PlaneDetectionMode GetDetectionMode()
//     {
//         UnityEngine.XR.ARSubsystems.PlaneDetectionMode mode = 
//             UnityEngine.XR.ARSubsystems.PlaneDetectionMode.None;

//         if (detectHorizontal)
//             mode |= UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Horizontal;
//         if (detectVertical)
//             mode |= UnityEngine.XR.ARSubsystems.PlaneDetectionMode.Vertical;

//         return mode;
//     }

//     private void OnPlanesChanged(ARPlanesChangedEventArgs args)
//     {
//         // Handle added planes
//         foreach (ARPlane plane in args.added)
//         {
//             ProcessPlane(plane);
//         }

//         // Handle updated planes
//         foreach (ARPlane plane in args.updated)
//         {
//             ProcessPlane(plane);
//         }

//         // Handle removed planes
//         foreach (ARPlane plane in args.removed)
//         {
//             if (activePlanes.Contains(plane))
//             {
//                 activePlanes.Remove(plane);
//             }
//         }
//     }

//     private void ProcessPlane(ARPlane plane)
//     {
//         // Check plane size
//         float planeSize = Mathf.Max(plane.size.x, plane.size.y);
        
//         if (planeSize < minPlaneSize || planeSize > maxPlaneSize)
//         {
//             plane.gameObject.SetActive(false);
//             return;
//         }

//         // Check plane alignment
//         bool isHorizontal = Vector3.Angle(plane.normal, Vector3.up) < 10f ||
//                            Vector3.Angle(plane.normal, Vector3.down) < 10f;
//         bool isVertical = Vector3.Angle(plane.normal, Vector3.up) > 80f;

//         // Enable/disable based on orientation
//         bool shouldBeActive = (isHorizontal && detectHorizontal) || 
//                             (isVertical && detectVertical);
        
//         plane.gameObject.SetActive(shouldBeActive);

//         // Apply appropriate material
//         if (shouldBeActive)
//         {
//             var meshRenderer = plane.GetComponent<MeshRenderer>();
//             if (meshRenderer != null)
//             {
//                 meshRenderer.material = isHorizontal ? 
//                     horizontalPlaneMaterial : verticalPlaneMaterial;
//             }

//             if (!activePlanes.Contains(plane))
//             {
//                 activePlanes.Add(plane);
//             }
//         }
//     }

//     public void ToggleHorizontalDetection(bool enable)
//     {
//         detectHorizontal = enable;
//         UpdateDetectionMode();
//     }

//     public void ToggleVerticalDetection(bool enable)
//     {
//         detectVertical = enable;
//         UpdateDetectionMode();
//     }

//     private void UpdateDetectionMode()
//     {
//         planeManager.requestedDetectionMode = GetDetectionMode();
        
//         // Update existing planes
//         foreach (var plane in activePlanes)
//         {
//             ProcessPlane(plane);
//         }
//     }
// }

using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARPlane))]
[RequireComponent(typeof(MeshRenderer))]
public class ARPlanColorizer : MonoBehaviour
{
    ARPlane arPlane;
    MeshRenderer planeMeshRenderer;

    void Awake()
    {
        arPlane = GetComponent<ARPlane>();
        planeMeshRenderer = GetComponent<MeshRenderer>();
    }

    void Start()
    {
        UpdatePlaneColor();    
    }

    void UpdatePlaneColor()
    {
        Color planeMatColor = Color.gray;

        switch(arPlane.classification)
        {
            case PlaneClassification.Floor:
                planeMatColor = Color.green;
                break;
            case PlaneClassification.Wall:
                planeMatColor = Color.white;
                break;
            case PlaneClassification.Ceiling:
                planeMatColor = Color.red;
                break;
            // case PlaneClassification.Table:
            //     planeMatColor = Color.yellow;
            //     break;
            // case PlaneClassification.Seat:
            //     planeMatColor = Color.blue;
            //     break;
            case PlaneClassification.Door:
                planeMatColor = Color.magenta;
                break;
            case PlaneClassification.Window:
                planeMatColor = Color.cyan;
                break;
        }

        planeMatColor.a = 0.15f;
        planeMeshRenderer.material.color = planeMatColor;
    }
}
