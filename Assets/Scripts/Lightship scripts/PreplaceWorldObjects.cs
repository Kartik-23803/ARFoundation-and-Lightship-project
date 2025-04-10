using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;
using Niantic.Lightship.AR.XRSubsystems;

public class PreplaceWorldObjects : MonoBehaviour
{
    [SerializeField] List<GameObject> objectsToPlace = new();
    [SerializeField] List<LatLong> latLongs = new();
    [SerializeField] ARWorldPositioningManager positioningManager;
    [SerializeField] ARWorldPositioningObjectHelper objectHelper;
    List<Material> materials = new();
    List<GameObject> instaniatedObjects = new();

    void Start()
    {
        foreach(var gpsCoord in latLongs)
        {
            GameObject newObject = Instantiate(objectsToPlace[latLongs.IndexOf(gpsCoord) % objectsToPlace.Count]);

            objectHelper.AddOrUpdateObject(newObject, gpsCoord.latitude, gpsCoord.longitude, 0, Quaternion.identity);
        }

        positioningManager.OnStatusChanged += OnStatusChanged;
    }

    void OnStatusChanged(WorldPositioningStatus status)
    {
        Debug.Log("Status changed to "+ status);
    }
}

[System.Serializable]
public struct LatLong
{
    public double latitude;
    public double longitude;
}