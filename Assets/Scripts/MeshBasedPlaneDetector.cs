using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Linq;

[RequireComponent(typeof(ARMeshManager))]
public class MeshBasedPlaneDetector : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject planePrefab;
    private ARMeshManager meshManager;

    [Header("Plane Detection Settings")]
    [SerializeField] private float minPlaneSize = 0.5f; // Increased minimum size
    [SerializeField] private float normalThreshold = 0.97f; // More strict normal threshold
    [SerializeField] private float mergingDistance = 0.2f; // Increased merging distance
    [SerializeField] private float minPointsForPlane = 20; // Minimum points to consider a plane
    [SerializeField] private float updateInterval = 0.5f; // How often to update planes

    [Header("Filtering")]
    [SerializeField] private bool detectHorizontal = true;
    [SerializeField] private bool detectVertical = true;
    [SerializeField] private float maxPlaneHeight = 2f; // Max height for horizontal planes
    [SerializeField] private float minPlaneHeight = -0.5f; // Min height for horizontal planes

    private List<DetectedPlane> detectedPlanes = new List<DetectedPlane>();
    private Transform planesParent;
    private float nextUpdateTime;

    private class DetectedPlane
    {
        public Vector3 position;
        public Vector3 normal;
        public float size;
        public GameObject visualObject;
        public PlaneType type;
        public Bounds bounds;

        public enum PlaneType
        {
            Horizontal,
            Vertical
        }
    }

    private void Start()
    {
        planesParent = new GameObject("Detected Planes").transform;
        planesParent.parent = transform;

        meshManager = GetComponent<ARMeshManager>();
        if (meshManager == null)
        {
            Debug.LogError("ARMeshManager not found!");
            return;
        }

        meshManager.meshesChanged += OnMeshesChanged;
    }

    private void OnMeshesChanged(ARMeshesChangedEventArgs args)
    {
        if (Time.time < nextUpdateTime) return;
        nextUpdateTime = Time.time + updateInterval;

        foreach (var mesh in args.added)
        {
            ProcessMesh(mesh);
        }

        // Cleanup overlapping planes
        CleanupOverlappingPlanes();
    }

    private void ProcessMesh(MeshFilter meshFilter)
    {
        if (meshFilter == null || meshFilter.mesh == null) return;

        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;
        Vector3[] normals = mesh.normals;

        Transform meshTransform = meshFilter.transform;
        Vector3[] worldVertices = vertices.Select(v => meshTransform.TransformPoint(v)).ToArray();
        Vector3[] worldNormals = normals.Select(n => meshTransform.TransformDirection(n)).ToArray();

        Dictionary<Vector3, List<Vector3>> normalGroups = new Dictionary<Vector3, List<Vector3>>();

        // Group vertices by their normals
        for (int i = 0; i < worldNormals.Length; i++)
        {
            Vector3 normal = worldNormals[i];
            
            // Only process clearly horizontal or vertical normals
            if (!IsValidNormal(normal)) continue;

            Vector3 roundedNormal = RoundNormal(normal);
            if (!normalGroups.ContainsKey(roundedNormal))
            {
                normalGroups[roundedNormal] = new List<Vector3>();
            }
            normalGroups[roundedNormal].Add(worldVertices[i]);
        }

        // Process each group
        foreach (var group in normalGroups)
        {
            if (group.Value.Count < minPointsForPlane) continue;

            ProcessPointGroup(group.Key, group.Value);
        }
    }

    private bool IsValidNormal(Vector3 normal)
    {
        float upDot = Vector3.Dot(normal, Vector3.up);
        float rightDot = Vector3.Dot(normal, Vector3.right);
        float forwardDot = Vector3.Dot(normal, Vector3.forward);

        // Check if it's clearly horizontal
        if (detectHorizontal && Mathf.Abs(upDot) > normalThreshold)
            return true;

        // Check if it's clearly vertical
        if (detectVertical && (Mathf.Abs(rightDot) > normalThreshold || Mathf.Abs(forwardDot) > normalThreshold))
            return true;

        return false;
    }

    private void ProcessPointGroup(Vector3 normal, List<Vector3> points)
    {
        // Calculate average position and bounds
        Vector3 avgPosition = Vector3.zero;
        foreach (var point in points)
        {
            avgPosition += point;
        }
        avgPosition /= points.Count;

        // Check height constraints for horizontal planes
        if (Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > normalThreshold)
        {
            if (avgPosition.y > maxPlaneHeight || avgPosition.y < minPlaneHeight)
                return;
        }

        // Calculate bounds
        Bounds bounds = new Bounds(avgPosition, Vector3.zero);
        foreach (var point in points)
        {
            bounds.Encapsulate(point);
        }

        // Check if plane is large enough
        float size = Mathf.Min(bounds.size.x, bounds.size.z);
        if (size < minPlaneSize) return;

        DetectedPlane newPlane = new DetectedPlane
        {
            position = avgPosition,
            normal = normal,
            size = size,
            bounds = bounds,
            type = Mathf.Abs(Vector3.Dot(normal, Vector3.up)) > normalThreshold ? 
                   DetectedPlane.PlaneType.Horizontal : 
                   DetectedPlane.PlaneType.Vertical
        };

        CreateOrUpdatePlane(newPlane);
    }

    private void CreateOrUpdatePlane(DetectedPlane newPlane)
    {
        // Check for nearby existing planes
        foreach (var existingPlane in detectedPlanes.ToList())
        {
            if (ArePlanesOverlapping(existingPlane, newPlane))
            {
                // Merge planes
                MergePlanes(existingPlane, newPlane);
                return;
            }
        }

        // Create new plane if no merge occurred
        CreatePlaneVisualization(newPlane);
        detectedPlanes.Add(newPlane);
    }

    private bool ArePlanesOverlapping(DetectedPlane plane1, DetectedPlane plane2)
    {
        if (plane1.type != plane2.type) return false;
        
        // Check if bounds overlap and normals are similar
        return plane1.bounds.Intersects(plane2.bounds) &&
               Vector3.Dot(plane1.normal, plane2.normal) > normalThreshold;
    }

    private void MergePlanes(DetectedPlane existing, DetectedPlane newPlane)
    {
        // Update bounds
        existing.bounds.Encapsulate(newPlane.bounds);
        existing.position = existing.bounds.center;
        existing.size = Mathf.Max(existing.size, newPlane.size);

        // Update visualization
        UpdatePlaneVisualization(existing);
    }

    private void CleanupOverlappingPlanes()
    {
        for (int i = detectedPlanes.Count - 1; i >= 0; i--)
        {
            for (int j = i - 1; j >= 0; j--)
            {
                if (ArePlanesOverlapping(detectedPlanes[i], detectedPlanes[j]))
                {
                    // Merge into the larger plane
                    if (detectedPlanes[i].size > detectedPlanes[j].size)
                    {
                        MergePlanes(detectedPlanes[i], detectedPlanes[j]);
                        RemovePlane(detectedPlanes[j]);
                    }
                    else
                    {
                        MergePlanes(detectedPlanes[j], detectedPlanes[i]);
                        RemovePlane(detectedPlanes[i]);
                    }
                    break;
                }
            }
        }
    }

    private void RemovePlane(DetectedPlane plane)
    {
        if (plane.visualObject != null)
            Destroy(plane.visualObject);
        detectedPlanes.Remove(plane);
    }

    private Vector3 RoundNormal(Vector3 normal)
    {
        return new Vector3(
            Mathf.Round(normal.x * 2) / 2,
            Mathf.Round(normal.y * 2) / 2,
            Mathf.Round(normal.z * 2) / 2
        ).normalized;
    }

    private void CreatePlaneVisualization(DetectedPlane plane)
    {
        if (planePrefab == null) return;

        GameObject planeObject = Instantiate(planePrefab, plane.position, Quaternion.identity, planesParent);

        // Set rotation based on normal
        if (plane.type == DetectedPlane.PlaneType.Horizontal)
        {
            planeObject.transform.up = plane.normal;
        }
        else
        {
            planeObject.transform.forward = plane.normal;
        }

        // Set scale based on bounds
        Vector2 size = new Vector2(plane.bounds.size.x, plane.bounds.size.z);
        planeObject.transform.localScale = new Vector3(size.x, size.y, 1);

        // Set color based on type
        Renderer renderer = planeObject.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            Color planeColor = plane.type == DetectedPlane.PlaneType.Horizontal ?
                              new Color(0, 1, 0, 0.3f) : // Green for horizontal
                              new Color(0, 0, 1, 0.3f);  // Blue for vertical
            renderer.material.color = planeColor;
        }

        plane.visualObject = planeObject;
    }

    private void UpdatePlaneVisualization(DetectedPlane plane)
    {
        if (plane.visualObject == null) return;

        plane.visualObject.transform.position = plane.position;
        
        if (plane.type == DetectedPlane.PlaneType.Horizontal)
        {
            plane.visualObject.transform.up = plane.normal;
        }
        else
        {
            plane.visualObject.transform.forward = plane.normal;
        }

        Vector2 size = new Vector2(plane.bounds.size.x, plane.bounds.size.z);
        plane.visualObject.transform.localScale = new Vector3(size.x, size.y, 1);
    }

    private void OnDestroy()
    {
        if (meshManager != null)
            meshManager.meshesChanged -= OnMeshesChanged;

        foreach (var plane in detectedPlanes)
        {
            if (plane.visualObject != null)
                Destroy(plane.visualObject);
        }
    }
}