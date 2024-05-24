using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class RobotController : MonoBehaviour{
    private ArticulationBody[] articulationChain;
    public double[] positions;

    public bool remote;

    //Not sure if these are needed
    private float stiffness = 0;
    private float damping = 0;
    private float forceLimit = 0;
    private float linearDamping = 0;
    private int jointFriction = 0;
    private int angularDamping = 0;
    private int mass = 1; //Has to be non-zero and positive

    void Start() {

        articulationChain = this.GetComponentsInChildren<ArticulationBody>();
        positions = new double[articulationChain.Length];
        
        foreach (ArticulationBody joint in articulationChain)
        {
            joint.gameObject.AddComponent<RobotJointControl>();

            joint.mass = mass;
            joint.jointFriction = jointFriction;
            joint.angularDamping = angularDamping;
            joint.linearDamping = linearDamping;

            ArticulationDrive currentDrive = joint.xDrive;
            currentDrive.stiffness = stiffness;
            currentDrive.damping = damping;
            currentDrive.forceLimit = forceLimit;
            joint.xDrive = currentDrive;
                
            joint.useGravity = false;

        }
        // Hard coded to start in different configurations
        
        positions[4] = -1.6;
        positions[6] = 1.6;
        if (remote)
        {
            positions[2] = -0.5;
            positions[4] = -2.1;
            positions[7] = 0.52;
        }
    }

    public void Update(){
        int jointIndex = 0;
        foreach(double position in positions){
            RobotJointControl current = articulationChain[jointIndex++].GetComponent<RobotJointControl>();
            current.position = position;
        }
    }
        
    public bool Fixed(int jointIndex){
        return (articulationChain[jointIndex].dofCount == 0);      
    }
        

}

