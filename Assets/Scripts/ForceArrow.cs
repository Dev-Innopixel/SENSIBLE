using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ForceArrow : MonoBehaviour
{

    public Vector3 Force;
    private const float mag_max = 40.0F; //The force at which the arrow turns red
    private const float mag_min = 0.0F;
    private const float mag_medium = 20.0F; //The force at which the arrow turns yellow
    private Renderer arrowRenderer;
    private float threshold = 0.5F;
    private float arrowScale = 20.0F; //Could also be seperate scales for x, y and z


    void Start(){
        Force = new Vector3((float)0.0, (float)0.0, (float)0.0);
        arrowRenderer = this.GetComponentInChildren<Renderer>();
    }

    
    void Update(){
        float mag = (float)Force.magnitude;
        if (mag < threshold){
            arrowRenderer.enabled = false; 
        }
        if (mag > threshold){
            arrowRenderer.enabled = true;
            this.gameObject.transform.localScale = new Vector3(arrowScale * (mag / mag_max), arrowScale * (mag / mag_max), arrowScale * (mag / mag_max) + 0.5F);
            Vector3 Tempvec = new Vector3(-(Force[0]*Mathf.Cos(-Mathf.PI/6) - Force[2] * Mathf.Sin(-Mathf.PI / 6)), -Force[1], -(Force[2] * Mathf.Cos(-Mathf.PI / 6) + Force[0] * Mathf.Sin(-Mathf.PI / 6)));
            this.gameObject.transform.rotation = Quaternion.LookRotation(Tempvec);
            arrowRenderer.material.SetColor("_Color", set_lerp_color(mag));
        }

    }

    Color set_lerp_color(float mag){
        return Color.Lerp(Color.green, Color.red, mag/mag_max);
    }

}


