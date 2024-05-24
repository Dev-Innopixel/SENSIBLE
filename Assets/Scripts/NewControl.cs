using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using WeArt.Components;
using WeArt.Core;
using System.Reflection;


public class NewControl : MonoBehaviour
{
    public GameObject ellipsoid;
    public bool forceFeedback = false;
    public bool temperatureFeedback = false;

    public float fromMinValue = 0.1f;
    public float fromMaxValue = 0.3f;


    private int numObjects = 6;

    // List to store generated objects
    private List<GameObject> generatedObjects = new List<GameObject>();

    //Haptics force
    Force forceThumb = Force.Default;
    Force forceIndex = Force.Default;
    Force forceMiddle = Force.Default;

    // Haptic Actuators
    public WeArtHapticObject ThimbleThumb;
    public WeArtHapticObject ThimbleIndex;
    public WeArtHapticObject ThimbleMiddle;

    // Haptics Temperature
    Temperature temperatureThumb = Temperature.Default;
    Temperature temperatureIndex = Temperature.Default;
    Temperature temperatureMiddle = Temperature.Default;


    // Start is called before the first frame update
    void Start()
    {
        // Setup of Haptics
        ThimbleThumb.HandSides = HandSideFlags.Right;
        ThimbleIndex.HandSides = HandSideFlags.Right;
        ThimbleMiddle.HandSides = HandSideFlags.Right;

        ThimbleThumb.ActuationPoints = ActuationPointFlags.Thumb;
        ThimbleIndex.ActuationPoints = ActuationPointFlags.Index;
        ThimbleMiddle.ActuationPoints = ActuationPointFlags.Middle;

        forceThumb.Active = true;
        forceIndex.Active = true;
        forceMiddle.Active = true;

        temperatureThumb.Active = true;
        temperatureIndex.Active = true;
        temperatureMiddle.Active = true;

        // Set ellipsoid triggger to true
        ellipsoid.GetComponent<Collider>().isTrigger = true;

        // Generate and set the interaction Objects
        for (int k = 0; k < numObjects; k++)
        {
            GenerateMeshObject(k, ellipsoid, generatedObjects);
        }
        ControlSpawner(ellipsoid, generatedObjects);
        generatedObjects[3].GetComponent<Renderer>().material.color = Color.green;
        generatedObjects[4].GetComponent<Renderer>().material.color = Color.blue;
        generatedObjects[5].GetComponent<Renderer>().material.color = Color.red;
    }
    // Update is called once per frame
    void Update()
    {
        ellipsoidShaper(ellipsoid, generatedObjects);
        sphereTracker(generatedObjects, ellipsoid);

        if(forceFeedback == true)
        {
            SetForce(ellipsoid, generatedObjects, fromMinValue, fromMaxValue);
        }
        if (temperatureFeedback == true)
        {
            SetTemperature(ellipsoid, generatedObjects, fromMinValue, fromMaxValue);
        }
    }
    void GenerateMeshObject(int objectName, GameObject ellipsoid, List<GameObject> listOfObjects)
    {
        // Create a new game object with a primitive mesh
        GameObject meshObject = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        // Rename the GameObject if needed
        meshObject.name = objectName.ToString();

        // Set the spawn location
        meshObject.transform.position = ellipsoid.transform.position;

        // Set the spawn scale
        meshObject.transform.localScale = new Vector3(0.01f, 0.01f, 0.01f);
        meshObject.GetComponent<Collider>().isTrigger = true;

        // Access and modify other components as needed
        // For example, to change the material, you can use:

        if (objectName > 2)
        {
            //WEART stuff
            meshObject.transform.localScale = new Vector3(0.012f, 0.012f, 0.012f);

            meshObject.AddComponent<Rigidbody>();
            meshObject.GetComponent<Rigidbody>().useGravity = false;
            meshObject.AddComponent<WeArt.Components.WeArtTouchableObject>();
            meshObject.GetComponent<WeArtTouchableObject>().enabled = true;
            meshObject.GetComponent<WeArtTouchableObject>().Graspable = true;
        }

        // Add the generated object to the list
        listOfObjects.Add(meshObject);
    }
    void ControlSpawner(GameObject ellipsoid, List<GameObject> listOfObjects)
    {
        if (listOfObjects.Count >= 6) // Ensure there are at least three objects in the list
        {
            // Update the position of the first object in the list
            listOfObjects[0].transform.position = new Vector3(
                ellipsoid.transform.position.x,
                ellipsoid.transform.position.y + ellipsoid.transform.localScale.y / 2,
                ellipsoid.transform.position.z
            );

            // Update the position of the second object in the list
            listOfObjects[1].transform.position = new Vector3(
                ellipsoid.transform.position.x,
                ellipsoid.transform.position.y,
                ellipsoid.transform.position.z - ellipsoid.transform.localScale.z / 2
            );

            // Update the position of the third object in the list
            listOfObjects[2].transform.position = new Vector3(
                ellipsoid.transform.position.x + ellipsoid.transform.localScale.x / 2,
                ellipsoid.transform.position.y,
                ellipsoid.transform.position.z
            );


            // Update the position of the first object in the list
            listOfObjects[3].transform.position = new Vector3(
                ellipsoid.transform.position.x,
                ellipsoid.transform.position.y + 0.01f + ellipsoid.transform.localScale.y / 2,
                ellipsoid.transform.position.z
            );

            // Update the position of the second object in the list
            listOfObjects[4].transform.position = new Vector3(
                ellipsoid.transform.position.x,
                ellipsoid.transform.position.y,
                ellipsoid.transform.position.z - 0.01f - ellipsoid.transform.localScale.z / 2
            );

            // Update the position of the third object in the list
            listOfObjects[5].transform.position = new Vector3(
                ellipsoid.transform.position.x + 0.01f + ellipsoid.transform.localScale.x / 2,
                ellipsoid.transform.position.y,
                ellipsoid.transform.position.z
            );
        }
    }
    void ellipsoidShaper(GameObject ellipsoid, List<GameObject> listOfObjects)
    {
        float sizeY = 2 * Vector3.Distance(ellipsoid.transform.position, listOfObjects[0].transform.position);
        float sizeZ = 2 * Vector3.Distance(ellipsoid.transform.position, listOfObjects[1].transform.position);
        float sizeX = 2 * Vector3.Distance(ellipsoid.transform.position, listOfObjects[2].transform.position);

        ellipsoid.transform.localScale = new Vector3(sizeX, sizeY, sizeZ);
    }
    void sphereTracker(List<GameObject> listOfObjects, GameObject ellipsoid)
    {
        listOfObjects[0].transform.position = new Vector3(ellipsoid.transform.position.x, listOfObjects[3].transform.position.y, ellipsoid.transform.position.z);
        listOfObjects[1].transform.position = new Vector3(ellipsoid.transform.position.x, ellipsoid.transform.position.y, listOfObjects[4].transform.position.z);
        listOfObjects[2].transform.position = new Vector3(listOfObjects[5].transform.position.x, ellipsoid.transform.position.y, ellipsoid.transform.position.z);

    }

    void SetForce(GameObject ellipsoid, List<GameObject> listOfObjects, float fromMinValue, float fromMaxValue)
    {
        forceThumb.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[1].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);
        forceIndex.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[0].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);
        forceMiddle.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[2].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);

        ThimbleThumb.Force = forceThumb;
        ThimbleIndex.Force = forceIndex;
        ThimbleMiddle.Force = forceMiddle;
    }
    void SetTemperature(GameObject ellipsoid, List<GameObject> listOfObjects, float fromMinValue, float fromMaxValue)
    {
        temperatureThumb.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[1].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);
        temperatureIndex.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[0].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);
        temperatureMiddle.Value = MapValue(Vector3.Distance(ellipsoid.transform.position, listOfObjects[2].transform.position), fromMinValue, fromMaxValue, 0.0f, 1.0f);

        //temperatureThumb.Value = Vector3.Distance(ellipsoid.transform.position, listOfObjects[1].transform.position);
        //temperatureIndex.Value = Vector3.Distance(ellipsoid.transform.position, listOfObjects[0].transform.position);
        //temperatureMiddle.Value = Vector3.Distance(ellipsoid.transform.position, listOfObjects[2].transform.position);

        ThimbleThumb.Temperature = temperatureThumb;
        ThimbleIndex.Temperature = temperatureIndex;
        ThimbleMiddle.Temperature = temperatureMiddle;
    }

    private float MapValue(float value, float fromMin, float fromMax, float toMin, float toMax)
    {
        // Ensure the value is within the source range
        value = Math.Max(Math.Min(value, fromMax), fromMin);

        // Calculate the ratio of the value within the source range
        float ratio = (value - fromMin) / (fromMax - fromMin);

        // Map the ratio to the target range
        float result = toMin + (ratio * (toMax - toMin));

        return result;
    }
}
