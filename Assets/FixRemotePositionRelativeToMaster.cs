using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixRemotePositionRelativeToMaster : MonoBehaviour
{
    public GameObject MasterBase;
    public GameObject RemoteBase;
    public Transform MasterTCP;
    public Transform RemoteTCP;

    public TCP_ROS TCP_ROS;

    public bool transpose = false;
    private int ripple = 0;
    private Vector3 diff;

    private bool ConfigTransposedOnce = false;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (transpose)
            doTranspose();
        if (!ConfigTransposedOnce & TCP_ROS & TCP_ROS.RobotConfigurationsReceived)
        {
            Debug.Log("Did automated transpose");
            doTranspose();
            ConfigTransposedOnce = true;
        }
    }

    void doTranspose()
    {
        if (ripple == 0)
        {
            // rotate remote to be 180 degrees off from master
            Quaternion rotation = Quaternion.Euler(MasterBase.transform.rotation.eulerAngles + Quaternion.Euler(0, 180, 0).eulerAngles);
            RemoteBase.GetComponent<ArticulationBody>().TeleportRoot(RemoteBase.transform.position, rotation);
            RemoteBase.transform.rotation = rotation;
            ripple = 1;
        }
        else if (ripple == 1)
        {
            // get position difference between remote tcp and remote base
            diff = MasterTCP.position - RemoteTCP.position;
            // translate remote base by the difference
            RemoteBase.GetComponent<ArticulationBody>().TeleportRoot(RemoteBase.transform.position + diff, RemoteBase.transform.rotation);
            RemoteBase.transform.position = RemoteBase.transform.position + diff;
            ripple = 0;
            transpose = false;
        }
    }
}
