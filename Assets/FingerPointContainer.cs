using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FingerPointContainer : MonoBehaviour
{

    public FingerPointFinder FingerPointFinder;
    public Transform[] manusFingerCapsules;
    public Transform[] fingerPoints = new Transform[3];


    public bool manus;

    // Update is called once per frame
    void Update()
    {
        if (manus)
        {
            fingerPoints = manusFingerCapsules;
        }
        else
        {
            if (PointsValid())
            {
                fingerPoints = FingerPointFinder.FingerPoints;
            }
        }
    }

    public bool PointsValid()
    {
        if (manus)
        { 
            return true;
        }
        else
        {
            for (int i = 0; i < 3; i++)
                if (FingerPointFinder.IdMap[i] == -1)
                    return false;

            foreach (Transform point in FingerPointFinder.FingerTips)
            {
                for (int i = 0; i < 3; i++)
                {
                    if (double.IsNaN(point.position[i]))
                        return false;
                }
            }
            return true;
        }
    }

}
