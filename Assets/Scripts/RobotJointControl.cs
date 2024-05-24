using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RobotJointControl : MonoBehaviour{
    public double position;
    public ArticulationBody joint;
    void Start(){
        joint = this.GetComponent<ArticulationBody>();
    }

    public void FixedUpdate(){
        if(joint.jointType != ArticulationJointType.FixedJoint){
                ArticulationReducedSpace jointState = joint.jointPosition;
                jointState[0] = (float)position;
                joint.jointPosition = jointState;

                ArticulationDrive currentDrive = joint.xDrive;
                currentDrive.target = (float)position;

                joint.xDrive = currentDrive;

            
        }
    }
}
