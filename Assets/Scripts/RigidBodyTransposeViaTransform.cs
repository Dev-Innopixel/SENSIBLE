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
public class RigidBodyTransposeViaTransform : MonoBehaviour
{
    
    public bool UseRotation = true;
    public Transform FollowTransform;
    public bool HasBase = false;

    private Vector3 pos;
    private Quaternion rot;

    void Start()
    {

    }


    void Update()
    {

        if (!HasBase)
        {
            GeneratePose();
        }
        else
        {
            UpdatePoseIfMoved(); // Can change the state of HasBase
        }
        if (HasBase)
        {
            SetRigidBodyPose();
        }
    }



    void GeneratePose()
    {
        if (FollowTransform != null && FollowTransform.position != new Vector3(0, 0, 0))
        {
            pos = FollowTransform.position;
            rot = FollowTransform.rotation;

            HasBase = true;
        }

    }

    void SetRigidBodyPose()
    {
        if (FollowTransform != null)
        {
            if (UseRotation)
                GetComponent<ArticulationBody>().TeleportRoot(pos, rot);
            else
                GetComponent<ArticulationBody>().TeleportRoot(pos, transform.rotation);
        }
    }

    bool UpdatePoseIfMoved()
    {

        if (FollowTransform != null && pos != new Vector3(0, 0, 0))
        {
            if ((pos - FollowTransform.position).magnitude > 0.005)
            {
                pos = FollowTransform.position;
                rot = FollowTransform.rotation;
            } 
            
            HasBase = true;
        }
        else
        {
            HasBase = false;
        }

        return false;
    }

    /* //Tried to implement som averaging, but getting the correct average of a rotation is more diffuclt than anticipated and not useful enough in the end
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

