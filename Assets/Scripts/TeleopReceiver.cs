using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using WeArt.Components;


public class TeleopReceiver : MonoBehaviour
{
    public RobotController PandaController;
    public ForceArrow ArrowController;
    //public Reader weartReader;

    void Start()
    {
        //Joint State Socket
        Unity.Robotics.ROSTCPConnector.ROSConnection.instance.Subscribe<RosMessageTypes.Std.Float64MultiArrayMsg>("joint_position", JointStateCallback);
    }


    void JointStateCallback(RosMessageTypes.Std.Float64MultiArrayMsg Msg)
    {
        //Callback for jointstate message, mirrors movement of twin in gazebo

        int i = 0;
        for (int j = 0; j < 7; j++)
        {
            while (PandaController.Fixed(i))
            {
                PandaController.positions[i++] = 0;
            }
            PandaController.positions[i++] = Msg.data[j];


        }
        ArrowController.Force.x = (float)Msg.data[7];
        ArrowController.Force.y = (float)Msg.data[9];
        ArrowController.Force.z = (float)Msg.data[8];


        //weartReader.ForceValueInput = (float)Math.Sqrt(Math.Pow(Msg.data[7], 2) + Math.Pow(Msg.data[8], 2) + Math.Pow(Msg.data[9], 2));


    }
}
