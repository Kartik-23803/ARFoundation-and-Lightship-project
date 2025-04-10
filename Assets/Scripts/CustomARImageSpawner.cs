// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;

// public class CustomARImageSpawner : MonoBehaviour
// {
//     [SerializeField]
//     private ARTrackedImageManager trackedImageManager;

//     [SerializeField]
//     private GameObject prefabToSpawn;

//     [SerializeField]
//     private Vector3 spawnRotation = Vector3.zero;

//     [SerializeField]
//     private float spawnDistance = 0.2f;

//     private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();
//     private float previousYRotation;

//     private void Awake()
//     {
//         if (trackedImageManager == null)
//         {
//             trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
//         }
//         previousYRotation = Camera.main.transform.eulerAngles.y;
//     }

//     private void OnEnable()
//     {
//         trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
//     }

//     private void OnDisable()
//     {
//         trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
//     }

//     private void Update()
//     {
//         // Get current camera Y rotation
//         float currentYRotation = Camera.main.transform.eulerAngles.y;

//         // Calculate the rotation difference
//         float rotationDifference = currentYRotation - previousYRotation;

//         // Counter-rotate all spawned prefabs
//         foreach (var prefab in spawnedPrefabs.Values)
//         {
//             if (prefab != null && prefab.activeSelf)
//             {
//                 // Apply counter-rotation around Y axis
//                 prefab.transform.Rotate(0, -rotationDifference, 0, Space.World);
//             }
//         }

//         // Update previous rotation for next frame
//         previousYRotation = currentYRotation;
//     }

//     private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
//     {
//         foreach (ARTrackedImage trackedImage in eventArgs.added)
//         {
//             SpawnPrefab(trackedImage);
//         }

//         foreach (ARTrackedImage trackedImage in eventArgs.updated)
//         {
//             UpdatePrefab(trackedImage);
//         }

//         foreach (ARTrackedImage trackedImage in eventArgs.removed)
//         {
//             if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
//             {
//                 Destroy(spawnedPrefabs[trackedImage.referenceImage.name]);
//                 spawnedPrefabs.Remove(trackedImage.referenceImage.name);
//             }
//         }
//     }

//     private void SpawnPrefab(ARTrackedImage trackedImage)
//     {
//         if (prefabToSpawn == null) return;

//         Vector3 spawnPosition = trackedImage.transform.position + 
//                               (trackedImage.transform.up * spawnDistance);

//         GameObject newPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);
//         newPrefab.transform.rotation = trackedImage.transform.rotation * Quaternion.Euler(spawnRotation);
//         newPrefab.transform.parent = trackedImage.transform;

//         spawnedPrefabs[trackedImage.referenceImage.name] = newPrefab;
//     }

//     private void UpdatePrefab(ARTrackedImage trackedImage)
//     {
//         if (spawnedPrefabs.TryGetValue(trackedImage.referenceImage.name, out GameObject spawnedPrefab))
//         {
//             spawnedPrefab.transform.position = trackedImage.transform.position + 
//                                              (trackedImage.transform.up * spawnDistance);

//             spawnedPrefab.SetActive(trackedImage.trackingState == 
//                                   UnityEngine.XR.ARSubsystems.TrackingState.Tracking);
//         }
//     }
// }


using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

[RequireComponent(typeof(ARTrackedImageManager))]
public class CustomARImageSpawner : MonoBehaviour
{
    [SerializeField] GameObject[] placeablePrefab;
    Dictionary<string, GameObject> spawnedPrefab = new Dictionary<string, GameObject>();
    ARTrackedImageManager trackedImageManager;

    void Awake()
    {
        trackedImageManager = FindObjectOfType<ARTrackedImageManager>();

        foreach(GameObject prefab in placeablePrefab)
        {
            GameObject newPrefab = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            newPrefab.name = prefab.name;
            spawnedPrefab.Add(prefab.name, newPrefab);
        }
    }

    void OnEnable()
    {
        trackedImageManager.trackedImagesChanged += ImageChanged;
    }

    void OnDisable()
    {
        trackedImageManager.trackedImagesChanged -= ImageChanged;
    }

    void ImageChanged(ARTrackedImagesChangedEventArgs eventArgs)
    {
        foreach(ARTrackedImage trackedImage in eventArgs.added)
        {
            UpdateImage(trackedImage);
        }
        foreach(ARTrackedImage trackedImage in eventArgs.updated)
        {
            UpdateImage(trackedImage);
        }
        foreach(ARTrackedImage trackedImage in eventArgs.removed)
        {
            spawnedPrefab[trackedImage.name].SetActive(false);
        }
    }

    void UpdateImage(ARTrackedImage trackedImage)
    {
        string name = trackedImage.referenceImage.name;
        Vector3 position = trackedImage.transform.position;

        GameObject prefab = spawnedPrefab[name];
        prefab.transform.position = position;
        prefab.SetActive(true);

        foreach(GameObject go in spawnedPrefab.Values)
        {
            if(go.name != name)
            {
                go.SetActive(false);
            }
        }
    }
}




// using System.Collections.Generic;
// using UnityEngine;
// using UnityEngine.XR.ARFoundation;

// public class CustomARImageSpawner : MonoBehaviour
// {
//     [SerializeField]
//     private ARTrackedImageManager trackedImageManager;

//     [SerializeField]
//     private GameObject prefabToSpawn;

//     [SerializeField]
//     private Vector3 spawnRotation = Vector3.zero;

//     [SerializeField]
//     private float spawnDistance = 0.2f;

//     private Dictionary<string, GameObject> spawnedPrefabs = new Dictionary<string, GameObject>();

//     private void Awake()
//     {
//         if (trackedImageManager == null)
//         {
//             trackedImageManager = FindObjectOfType<ARTrackedImageManager>();
//         }
//     }

//     private void OnEnable()
//     {
//         trackedImageManager.trackedImagesChanged += OnTrackedImagesChanged;
//     }

//     private void OnDisable()
//     {
//         trackedImageManager.trackedImagesChanged -= OnTrackedImagesChanged;
//     }

//     private void OnTrackedImagesChanged(ARTrackedImagesChangedEventArgs eventArgs)
//     {
//         foreach (ARTrackedImage trackedImage in eventArgs.added)
//         {
//             SpawnPrefab(trackedImage);
//         }

//         foreach (ARTrackedImage trackedImage in eventArgs.updated)
//         {
//             UpdatePrefab(trackedImage);
//         }

//         foreach (ARTrackedImage trackedImage in eventArgs.removed)
//         {
//             if (spawnedPrefabs.ContainsKey(trackedImage.referenceImage.name))
//             {
//                 Destroy(spawnedPrefabs[trackedImage.referenceImage.name]);
//                 spawnedPrefabs.Remove(trackedImage.referenceImage.name);
//             }
//         }
//     }

//     private void SpawnPrefab(ARTrackedImage trackedImage)
//     {
//         if (prefabToSpawn == null) return;

//         // Calculate spawn position perpendicular to the image
//         Vector3 spawnPosition = trackedImage.transform.position + 
//                               (trackedImage.transform.up * spawnDistance);

//         // Create the prefab
//         GameObject newPrefab = Instantiate(prefabToSpawn, spawnPosition, Quaternion.identity);

//         // Get the camera's forward direction
//         Vector3 cameraForward = Camera.main.transform.forward;
//         cameraForward.y = 0; // Project onto horizontal plane
//         cameraForward.Normalize();

//         // Get the tracked image's forward direction
//         Vector3 imageForward = -trackedImage.transform.up;
//         imageForward.y = 0; // Project onto horizontal plane
//         imageForward.Normalize();

//         // Calculate the angle between camera and image
//         float angle = Vector3.SignedAngle(imageForward, cameraForward, Vector3.up);

//         // If the angle is greater than 90 degrees, we need to flip the rotation
//         if (Mathf.Abs(angle) > 90f)
//         {
//             // Rotate 180 degrees around the up axis
//             newPrefab.transform.rotation = trackedImage.transform.rotation * 
//                                          Quaternion.Euler(spawnRotation) * 
//                                          Quaternion.Euler(0, 180f, 0);
//         }
//         else
//         {
//             newPrefab.transform.rotation = trackedImage.transform.rotation * 
//                                          Quaternion.Euler(spawnRotation);
//         }

//         newPrefab.transform.parent = trackedImage.transform;
//         spawnedPrefabs[trackedImage.referenceImage.name] = newPrefab;
//     }

//     private void UpdatePrefab(ARTrackedImage trackedImage)
//     {
//         if (spawnedPrefabs.TryGetValue(trackedImage.referenceImage.name, out GameObject spawnedPrefab))
//         {
//             // Update position
//             spawnedPrefab.transform.position = trackedImage.transform.position + 
//                                              (trackedImage.transform.up * spawnDistance);

//             // Get the camera's forward direction
//             Vector3 cameraForward = Camera.main.transform.forward;
//             cameraForward.y = 0;
//             cameraForward.Normalize();

//             // Get the tracked image's forward direction
//             Vector3 imageForward = -trackedImage.transform.up;
//             imageForward.y = 0;
//             imageForward.Normalize();

//             // Calculate the angle between camera and image
//             float angle = Vector3.SignedAngle(imageForward, cameraForward, Vector3.up);

//             // Update rotation based on camera angle
//             if (Mathf.Abs(angle) > 90f)
//             {
//                 spawnedPrefab.transform.rotation = trackedImage.transform.rotation * 
//                                                  Quaternion.Euler(spawnRotation) * 
//                                                  Quaternion.Euler(0, 180f, 0);
//             }
//             else
//             {
//                 spawnedPrefab.transform.rotation = trackedImage.transform.rotation * 
//                                                  Quaternion.Euler(spawnRotation);
//             }

//             spawnedPrefab.SetActive(trackedImage.trackingState == 
//                                   UnityEngine.XR.ARSubsystems.TrackingState.Tracking);
//         }
//     }

//     public void UpdateSpawnRotation(Vector3 newRotation)
//     {
//         spawnRotation = newRotation;
        
//         foreach (var prefab in spawnedPrefabs.Values)
//         {
//             if (prefab != null)
//             {
//                 var trackedImage = prefab.transform.parent.GetComponent<ARTrackedImage>();
//                 if (trackedImage != null)
//                 {
//                     UpdatePrefab(trackedImage);
//                 }
//             }
//         }
//     }
// }