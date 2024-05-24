using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PointPlacer : MonoBehaviour
{

    public AxisCalculator AxisClac;
    public FingerPointContainer FingerPointContainer;


    public GameObject[] ProjectedPoints;
    public LineRenderer[] LineRenderes;

    // Start is called before the first frame update
    void Start()
    {
    
    }

    // Update is called once per frame
    void Update()
    {
        

        if (FingerPointContainer.PointsValid())
        {
            SetVisible(true);
            PlacePoints();
        }
        else
        {
            SetVisible(false);
        }
    }

    void SetVisible(bool visible) {
        foreach (var Point in ProjectedPoints)
        {
            Point.GetComponent<MeshRenderer>().enabled = visible;
        }
    }

    void PlacePoints()
    {

        for (int i =0; i < 3; i++)
        {
            ProjectedPoints[i].transform.localPosition = AxisClac.projectedPoints[i];
        }

    }
}
