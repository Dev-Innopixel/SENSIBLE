/* 
Copyright © 2016 NaturalPoint Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License. 
*/

using System;
using UnityEngine;


/// <summary>
/// Implements live tracking of streamed OptiTrack rigid body data onto an object.
/// </summary>
public class RigidBodyTransposeViaOptitrack : MonoBehaviour
{
    [Tooltip("The object containing the OptiTrackStreamingClient script.")]
    public OptitrackStreamingClient StreamingClient;

    [Tooltip("The Streaming ID of the rigid body in Motive")]
    public Int32 RigidBodyId;

    [Tooltip("Subscribes to this asset when using Unicast streaming.")]
    public bool NetworkCompensation = true;


    public bool HasBase = false;

    public GameObject OptitrackPose;

    void Start()
    {


        // If the user didn't explicitly associate a client, find a suitable default.
        if (this.StreamingClient == null)
        {
            this.StreamingClient = OptitrackStreamingClient.FindDefaultClient();

            // If we still couldn't find one, disable this component.
            if (this.StreamingClient == null)
            {
                Debug.LogError(GetType().FullName + ": Streaming client not set, and no " + typeof(OptitrackStreamingClient).FullName + " components found in scene; disabling this component.", this);
                this.enabled = false;
                return;
            }
        }

        this.StreamingClient.RegisterRigidBody(this, RigidBodyId);

        while (StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation) == null) { }
    }


#if UNITY_2017_1_OR_NEWER
    void OnEnable()
    {
        Application.onBeforeRender += OnBeforeRender;
    }


    void OnDisable()
    {
        Application.onBeforeRender -= OnBeforeRender;
    }


    void OnBeforeRender()
    {
        UpdatePose();
    }
#endif


    void Update()
    {

        if (!HasBase)
        {
            GeneratePose();
        }
        else
        {
            if (PoseMoved()) {
                GeneratePose();
            }
            UpdatePose();
        }
    }


    void GeneratePose()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
        if (rbState != null)
        {
            OptitrackPose.transform.localPosition = rbState.Pose.Position;
            OptitrackPose.transform.localRotation = rbState.Pose.Orientation;

            HasBase = true;
        }

    }

    void UpdatePose()
    {
        if (OptitrackPose != null)
        {
            GetComponent<ArticulationBody>().TeleportRoot(OptitrackPose.transform.position, OptitrackPose.transform.rotation);
        }
    }

    bool PoseMoved()
    {
        OptitrackRigidBodyState rbState = StreamingClient.GetLatestRigidBodyState(RigidBodyId, NetworkCompensation);
        if (rbState != null)
        {
            if ((rbState.Pose.Position - OptitrackPose.transform.localPosition).magnitude > 0.005)
            {
                OptitrackPose.transform.localPosition = rbState.Pose.Position;
                OptitrackPose.transform.localRotation = rbState.Pose.Orientation;
            } 
            
            HasBase = true;
        }

        return false;
    }

    /* //I tried to implement som averaging, but getting the correct average of a rotation is more diffuclt than i anticipated and not worth it in the end 
       //(this is possibly a solution https://stackoverflow.com/questions/51517466/what-is-the-correct-way-to-average-several-rotation-matrices)

    void GetAverageTransform()
    {
        Vector3 sumPositions;
        Quaternion sumRotations;

        for (int i = 0; i < Num; i++)
        {
            
        }
        
        for (int M = 0; M < 4; M++)
        {
            for (int N = 0; N < 4; N++)
            {
                float sum = 0;
                
                avg[M, N] = sum / N;
            }
        }
        res.SetPositionAndRotation(avg.GetPosition(), avg.GetRotation());
    }
    */
}

