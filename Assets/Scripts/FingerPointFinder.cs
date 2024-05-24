using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics;

public class FingerPointFinder : MonoBehaviour
{

    public Transform[] FingerTips = new Transform[3];
    public int[] ids = new int[3];
    public int[] tempIds = new int[3];

    public int[] IdMap = new int[3];

    private bool FoundFingers = false;

    public GameObject OptitrackClient;
    public Transform[] FingerPoints = new Transform[3];

    public bool log = false;

    // Start is called before the first frame update
    void Start()
    {
        IdMap[0] = -1;
        IdMap[1] = -1;
        IdMap[2] = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (FoundFingers)
        {
            int res = GetOptitrackPoints();
            if (res != 0 && log)
            {
                Debug.Log(res);
            }
        }
        else
        {
            if (ResetFingers() == 0) FoundFingers = true;
        }
    }

    int GetOptitrackPoints()
    {
        int res = SetPointObjects();
        if (res != 0)
            return res;

        if (FixIdMap() != 0)
        {
            FoundFingers = false;
            return 3;
        }

        for (int i = 0; i < 3; i++)
        {
            FingerTips[i] = FingerPoints[IdMap[i]];
        }

        return 0;
    }

    int SetPointObjects()
    {
        int count = 0;
        foreach (Transform child in OptitrackClient.transform)
        {
            if (count > 2) return 2;
            string name = child.name;
            if (name.StartsWith("Passive (PointCloud ID: "))
            {
                FingerPoints[count] = child;
                tempIds[count] = int.Parse(name.Substring(24, name.Length - 24 - 1)); // (total length - header length - the closing bracket)
                count++;
            }
        }


        if (count < 3) return 1;
        return 0;
    }

    int FixIdMap()
    {
        int correct = 0;
        for (int i = 0; i < 3; i++) 
        {
            if (tempIds[i] == ids[i]) correct++;
        }
        if (correct == 3) return 0;

        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < 3; k++)
                {
                    if (i != j && i != k && j != k) {
                        correct = 0;
                        if (ids[0] == tempIds[i]) correct++;
                        if (ids[1] == tempIds[j]) correct++;
                        if (ids[2] == tempIds[k]) correct++;
                        
                        if (correct == 3 || correct == 2) {
                            ids[0] = tempIds[i];
                            ids[1] = tempIds[j];
                            ids[2] = tempIds[k];

                            IdMap[i] = 0;
                            IdMap[j] = 1;
                            IdMap[k] = 2;
                            return 0;
                        }
                    }
                }
            }
        }

        return 1;
    }


    int ResetFingers()
    {
        int res = SetPointObjects();
        if (res != 0)
            return res;


        Debug.Log("Trying to reset finger point configuration");
        float avgy = 0;
        float maxydelta = 0.03f;

        for (int i = 0; i < 3; i++)
        {
            avgy += FingerPoints[i].position[1];
        }
        avgy /= 3;

        for (int i = 0; i < 3; i++)
        {
            if (Mathf.Abs(avgy - FingerPoints[i].position[1]) > maxydelta)
                return 4;
        }

        float maxdist = 0;
        float mindist = float.PositiveInfinity;

        int maxDistPair = 0;
        int minDistPair = 0;

        for (int i = 0; i < 3; i++)
        {
            float tempdist = Vector3.Distance(FingerPoints[i].position, FingerPoints[(i+1) % 3].position);
            if (tempdist < mindist)
            {
                mindist = tempdist;
                minDistPair = i;
            }
            else if (tempdist > maxdist)
            {
                maxdist = tempdist;
                maxDistPair = i;
            }
        
        }

        if (maxdist > 0.25f || maxdist < 0.10f || mindist > 0.06f) //Check if hand dimensions make sense given the reset pose
            return 5;

        int thumb = (minDistPair + 2) % 3;
        int index = (maxDistPair + 2) % 3;
        int middle = (3 - index + 2*thumb) % 3;


        for (int i = 0; i < 3; i++)
        {
            ids[i] = tempIds[i];
        }

        IdMap[thumb] = 0;
        IdMap[index] = 1;
        IdMap[middle] = 2;

        Debug.Log("Finger configuration found");

        return 0;
    }

}
