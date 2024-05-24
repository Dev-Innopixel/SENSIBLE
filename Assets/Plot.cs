using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Plot : MonoBehaviour
{
    public TCP_ROS TCP_ROS;
    public float destroyDistance = 20.0f;
    public float yAxisScaleFactor = 1.0f;
    public float desiredForce = 5f;

    public Transform backplane;
    public Transform DesiredForceLine;

    float scaleFactor = 1.0f;
    
    private void Start()
    {
        // set position of desired force line
        DesiredForceLine.localPosition = new Vector3(0, - backplane.localScale.y / 2 * 0.8f + yAxisScaleFactor * desiredForce, 0);

        scaleFactor = backplane.localScale.y / 30;
}

    void Update()
    {
        Vector3 currentForce = TCP_ROS.Force;
        GenerateSphere(currentForce.magnitude);
        MoveSpheres();
    }

    void GenerateSphere(float offset)
    {
        Vector3 spawnPosition = new Vector3(-(backplane.localScale.x - scaleFactor) / 2,- backplane.localScale.y * 0.8f / 2 + (offset * yAxisScaleFactor), 0);
        GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.parent = transform;
        sphere.transform.localPosition = spawnPosition;
        sphere.transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        //sphere.tag = "GeneratedSphere";
        //sphere.gameObject.tag = "GeneratedSphere";  
    }
    void MoveSpheres()
    {
        // Iterate through each child of the object
        for (int i = 0; i < transform.childCount; i++)
        {
            // Get the i-th child's Transform
            Transform childTransform = transform.GetChild(i);

            // Move the child in the X-axis
            childTransform.Translate(new Vector3(scaleFactor/2, 0f, 0f),transform);
            if (childTransform.transform.localPosition.x > (backplane.localScale.x - scaleFactor) /2) { Destroy(childTransform.gameObject); }
        }
    }
}
