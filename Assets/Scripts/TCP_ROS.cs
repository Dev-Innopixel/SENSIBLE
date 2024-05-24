using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;
using Unity.Robotics.ROSTCPConnector;
using RosMessageTypes.Std;

public class TCP_ROS : MonoBehaviour{

    public RobotController PandaControllerMaster;
    public RobotController PandaControllerRemtoe;
    
    public AxisCalculator AxisCalc;
    public RobotBaseRelativeRotationCalculator BaseRelativeRotation;
    public ForceArrow ForceArrow;
    public DisturbanceGenerator DisturbanceGenerator;
    public Slider Slider;

    public bool log;

    ROSConnection ros;
    
    public float publishMessageFrequency = 0.05f; // 20 times per second

    public bool UseSliderStiffness = false;

    private float timeElapsed;

    private bool remote_config_recieved = false;
    private bool master_config_recieved = false;

    public bool RobotConfigurationsReceived = false;

    public float d_prod;

    public Vector3 Force;

    void Start(){
        ros = ROSConnection.GetOrCreateInstance();

        ros.Subscribe<Float64MultiArrayMsg>("/joint_position_master", JointStateCallback1);
        ros.Subscribe<Float64MultiArrayMsg>("/joint_position_slave", JointStateCallback2);
        ros.Subscribe<Float64MultiArrayMsg>("/unity_command_", UnityCommandCallback);
        //ros.Subscribe<Float64MultiArrayMsg>("/ffb_unity", ForceCallback);

        ros.RegisterPublisher<Float64MultiArrayMsg>("/stiffness");
        ros.RegisterPublisher<Float64MultiArrayMsg>("/relative_rotation");
        ros.RegisterPublisher<Float64MultiArrayMsg>("/disturbance_robot");
        ros.RegisterPublisher<Float64MultiArrayMsg>("/disturbance_tool");
    }


    private void Update()
    {

        StiffnessPublisher();
        //RelativeRotationPublisher();
        DisturbancePublisher();
        RobotConfigurationsReceived = remote_config_recieved & master_config_recieved;
    }


    void StiffnessPublisher()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            MultiArrayDimensionMsg[] dim = new MultiArrayDimensionMsg[1];
            dim[0] = new MultiArrayDimensionMsg("", 3, 3);
            MultiArrayLayoutMsg layout = new MultiArrayLayoutMsg(dim, 0);
            double[] data = new double[3];

            if (UseSliderStiffness)
            {
                data[0] = Slider.stiffness;
                data[1] = Slider.stiffness;
                data[2] = Slider.stiffness;
            }
            else
            {
                float stiffnessGain = 800; // min = 8 max = 200

                data[0] = stiffnessGain * AxisCalc.principleAxes[0];
                data[1] = stiffnessGain * AxisCalc.principleAxes[2];
                data[2] = stiffnessGain * AxisCalc.principleAxes[1];
            }

            Float64MultiArrayMsg stiffness = new Float64MultiArrayMsg(layout, data);

            ros.Publish("/stiffness", stiffness);

            timeElapsed = 0;
        }
    }


    void RelativeRotationPublisher()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            MultiArrayDimensionMsg[] dim = new MultiArrayDimensionMsg[1];
            dim[0] = new MultiArrayDimensionMsg("", 9, 9);
            MultiArrayLayoutMsg layout = new MultiArrayLayoutMsg(dim, 0);
            double[] data = new double[9];

            double beta = BaseRelativeRotation.beta * Math.PI / 180.0;

            for (int i = 0; i < 9; i++)
            {
                data[i] = 0;
            }

            data[0] =  Math.Cos((double)beta);
            data[1] = -Math.Sin((double)beta);
            data[3] =  Math.Sin((double)beta);
            data[4] =  Math.Cos((double)beta);
            data[8] = 1;


            Float64MultiArrayMsg rotation_matrix = new Float64MultiArrayMsg(layout, data);

            ros.Publish("/relative_rotation", rotation_matrix);

            timeElapsed = 0;
        }
    }

    void DisturbancePublisher()
    {
        timeElapsed += Time.deltaTime;

        if (timeElapsed > publishMessageFrequency)
        {
            MultiArrayDimensionMsg[] dim = new MultiArrayDimensionMsg[1];
            dim[0] = new MultiArrayDimensionMsg("", 3, 3);
            MultiArrayLayoutMsg layout = new MultiArrayLayoutMsg(dim, 0);
            double[] data = new double[3];

            // Transform to Gazebo/real space
            data[0] =  DisturbanceGenerator.Disturbance.z;
            data[1] = -DisturbanceGenerator.Disturbance.x;
            data[2] =  DisturbanceGenerator.Disturbance.y;

            Float64MultiArrayMsg disturbanceVector = new Float64MultiArrayMsg(layout, data);

            // These are the same for now, but might be different in reality
            ros.Publish("/disturbance_robot", disturbanceVector);
            ros.Publish("/disturbance_tool", disturbanceVector);

            timeElapsed = 0;
        }
    }

    void JointStateCallback1(Float64MultiArrayMsg Msg)
    {
        //Callback for jointstate message, mirrors movement of master twin

        int i = 0;
        for (int j = 0; j < 7; j++)
        {
            while (PandaControllerMaster.Fixed(i))
            {
                PandaControllerMaster.positions[i++] = 0;
            }
            PandaControllerMaster.positions[i++] = Msg.data[j];

        }
        master_config_recieved = true;
    }

    void JointStateCallback2(Float64MultiArrayMsg Msg)
    {
        //Callback for jointstate message, mirrors movement of remote twin

        int i = 0;
        for (int j = 0; j < 7; j++)
        {
            while (PandaControllerRemtoe.Fixed(i))
            {
                PandaControllerRemtoe.positions[i++] = 0;
            }
            PandaControllerRemtoe.positions[i++] = Msg.data[j];

        }
        remote_config_recieved = true;
    }

    void UnityCommandCallback(Float64MultiArrayMsg Msg)
    {
        // This is only done because the ROS TCP plugin is too slow to do more messages
        d_prod = (float)Msg.data[0];
        Force = new Vector3(-(float)Msg.data[2], (float)Msg.data[3], (float)Msg.data[1]);
    }

    void ForceCallback(Float64MultiArrayMsg Msg) 
    {
        Force = new Vector3(-(float)Msg.data[1], (float)Msg.data[2], (float)Msg.data[0]);
        ForceArrow.Force = Force;
    }

}
