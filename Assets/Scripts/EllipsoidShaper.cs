using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EllipsoidShaper : MonoBehaviour
{

    public AxisCalculator AxisCalc;
    public TabletController TabletController;

    public bool useTablet = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (true || AxisCalc.PointsValid())
        {
            FormEllipsoid();
        }
    }
    
    void FormEllipsoid()
    {
        if (useTablet)
        {
            transform.localScale = TabletController.principleAxes;
        }
        else
        {
            transform.localScale = AxisCalc.principleAxes;
        }
    }
}
