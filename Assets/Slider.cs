using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Slider : MonoBehaviour
{
    public Transform Sphere;
    public Transform Rod;
    public Transform HandTracker;
    public Transform Arrow;

    public TCP_ROS TCP_ROS;
    public TaskTrack TaskTrack;

    public int MinStiffness = 50;
    public int MaxStiffness = 400;

    public bool lock_on = false;
    private bool was_lock_on = false;
    
    public float stiffness = 0.5f;

    private float position = 0.5f;
    private float initial_hand_position = 0f;

    private float slider_length = 0.4f;
    private float arrow_scale = 5f;

    private Renderer arrowRenderer;

    //public float dot_product;
    double _Sk = 0;

    public float desiredForce = 1f;

    float MapValue(float x, float minx, float maxx, float minout, float maxout)
    {
        float normed = (x - minx) / (maxx - minx);
        return minout + normed * (maxout - minout);
    }

    // Start is called before the first frame update
    void Start()
    {
        slider_length = Rod.transform.localScale.y * 2;
        arrowRenderer = Arrow.GetComponentInChildren<Renderer>();
        arrow_scale = arrowRenderer.transform.localScale.z;
        Debug.Log(arrow_scale);
    }

    // Update is called once per frame
    void Update()
    {
        if (lock_on)
        {
            if (!was_lock_on)
            {
                position = 0.5f;
                initial_hand_position = HandTracker.position.y;
                was_lock_on = true;
            }

            //dot_product = Mathf.Abs(Vector3.Dot(TaskTrack.x_a_d, TCP_ROS.Force));

            bool test = CUSUM_Bounded(0f, 0.2f, 0.1f, 50f, Mathf.Abs(TCP_ROS.d_prod));
            int val = chengStiffnessPolicy(test, TaskTrack.TrackingError, TCP_ROS.Force.magnitude - desiredForce);
            setIndicatorArrow((val - 1)* arrow_scale);

            position = HandTracker.position.y - initial_hand_position;
            position = Mathf.Min(Mathf.Max(position, -slider_length / 2f), slider_length / 2f); // clap values to possible slider position
            stiffness = MapValue(position, -slider_length / 2f, slider_length / 2f, MinStiffness, MaxStiffness); // map from slider position to desired stiffness range

            Sphere.transform.localPosition = new Vector3(0, position, 0);

        }
        else 
        {
            was_lock_on = false;
        }
    }

    void setIndicatorArrow(float value)
    {
        float mag = Mathf.Abs(value);
        arrowRenderer.transform.localScale = new Vector3(mag, mag, mag);
        Vector3 Tempvec = new Vector3(0, value, 0);
        arrowRenderer.transform.rotation = Quaternion.LookRotation(Tempvec);
    }

    int chengStiffnessPolicy(bool isTouching, float trackingError, float forceError)
    {
        if (isTouching == false)
        {
            if (trackingError > 0.05)
            {
                return 2;
            }
            else return 1;
        }
        if (isTouching == true)
        {
            if (forceError > 0.05)
            {
                return 0;
            }
            else return 1;
        }
        return 1;
    }
    bool CUSUM_Bounded(double mu0, double mu1, double std, double threshold, double zk)
    {
        double sk = ((zk - mu0) * (zk - mu0) - (zk - mu1) * (zk - mu1)) / (2 * (std * std));
        _Sk = Math.Min(Math.Max(0 + sk, 0), 1.1 * threshold);
        return _Sk >= threshold;
    }

}
