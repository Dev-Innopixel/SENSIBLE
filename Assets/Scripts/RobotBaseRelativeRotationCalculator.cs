using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RobotBaseRelativeRotationCalculator : MonoBehaviour
{

    public GameObject MasterBase;
    public GameObject RemoteBase;

    public float beta;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        beta = RemoteBase.transform.rotation.eulerAngles[1] - MasterBase.transform.rotation.eulerAngles[1]; // Master with respect to Remote
    }
}
