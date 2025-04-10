using UnityEngine;
using Niantic.Lightship.AR.WorldPositioning;
using System;
using System.Collections;

public class AddWPSObjects : MonoBehaviour
{
    [SerializeField] ARWorldPositioningObjectHelper positioningHelper;
    [SerializeField] Camera trackingCamera;
    private GameObject gpsCube = null;

    double latitude;
    double longitude;
    double altitude;

    IEnumerator Start()
    {
        Input.location.Start();

        int maxWait = 5;
        while (Input.location.status == LocationServiceStatus.Initializing && maxWait > 0)
        {
            yield return new WaitForSeconds(1);
            maxWait--;
        }
        
        if (maxWait < 1 || Input.location.status == LocationServiceStatus.Failed)
        {
            Debug.LogError("GPS failed to start");
            yield break;
        }

        double latitude = Input.location.lastData.latitude;
        double longitude = Input.location.lastData.longitude;
        double altitude = 0.0;

        GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        cube.transform.localScale *= 2.0f;
        positioningHelper.AddOrUpdateObject(cube, latitude, longitude, altitude, Quaternion.identity);
    }

    void Update()
    { 
        if(gpsCube == null)
        {
            gpsCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            gpsCube.GetComponent<Renderer>().material.color = Color.red;
        }

        if (Input.location.isEnabledByUser)
        {
            double deviceLatitude = Input.location.lastData.latitude;
            double deviceLongitude = Input.location.lastData.longitude;
            
            Vector2 eastNorthOffsetMetres = EastNorthOffset(latitude,longitude, deviceLatitude, deviceLongitude);
            Vector3 trackingOffsetMetres = Quaternion.Euler(0, 0, Input.compass.trueHeading)*new Vector3(eastNorthOffsetMetres[0], (float)altitude, eastNorthOffsetMetres[1]);
            Vector3 trackingMetres = trackingCamera.transform.localPosition + trackingOffsetMetres;
            gpsCube.transform.localPosition = trackingMetres;
        }
    }

    public double GetLatitude()
    {
        return latitude;
    }

    public double GetLongitude()
    {
        return longitude;
    }

    public Vector2 EastNorthOffset(double latitudeDegreesA, double longitudeDegreesA, double latitudeDegreesB, double longitudeDegreesB)
    {
        double DEGREES_TO_METRES = 111139.0;
        float lonDifferenceMetres = (float)(Math.Cos((latitudeDegreesA+latitudeDegreesB)*0.5* Math.PI / 180.0) * (longitudeDegreesA - longitudeDegreesB) * DEGREES_TO_METRES);
        float latDifferenceMetres = (float)((latitudeDegreesA - latitudeDegreesB) * DEGREES_TO_METRES);
        return new Vector2(lonDifferenceMetres,latDifferenceMetres);
    }
}