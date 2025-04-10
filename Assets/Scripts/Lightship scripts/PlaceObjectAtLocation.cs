using System;
using System.Collections;
using System.Collections.Generic;
using Niantic.Lightship.AR.WorldPositioning;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;

public class PlaceObjectAtLocation : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper objectHelper;
    [SerializeField] ARWorldPositioningManager positionManager;
    [SerializeField] ARCameraManager cameraManager;
    [SerializeField] Button button;

    [SerializeField] GameObject objectToPlace;

    void Start()
    {
        button.onClick.AddListener(SpawnOnClick);
    }

    void SpawnOnClick()
    {
        Transform cameraTransform = cameraManager.GetComponent<Transform>();
        (double latOffsetCam, double longOffsetCam) = GetGeographicOffsetsFromCameraPosition(cameraTransform.position);

        double latitude = positionManager.WorldTransform.OriginLatitude + latOffsetCam;
        double longitude = positionManager.WorldTransform.OriginLongitude + longOffsetCam;
        double altitude = 0.0f;

        GameObject newObject = Instantiate(objectToPlace);
        objectHelper.AddOrUpdateObject(newObject, latitude, longitude, altitude, Quaternion.identity);

        Debug.Log($"Placnig object {newObject.name} at lat {latitude} and {longitude}");
    }

    private (double, double) GetGeographicOffsetsFromCameraPosition(Vector3 position)
    {
        double latOffset = position.z / 111000;
        double longOffset = position.x / (111000 * Mathf.Cos(
                                (float) positionManager.WorldTransform.OriginLatitude * Mathf.Deg2Rad));

        return (latOffset, longOffset);
    }
}
