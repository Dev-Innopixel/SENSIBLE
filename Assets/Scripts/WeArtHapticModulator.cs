using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Core;

public class WeArtHapticModulator : MonoBehaviour
{
    public WeArt.Components.WeArtHapticObject Thumb;
    public WeArt.Components.WeArtHapticObject Index;
    public WeArt.Components.WeArtHapticObject Middle;

    private WeArt.Components.WeArtHapticObject[] HapticFingers = new WeArt.Components.WeArtHapticObject[3];

    public FingerPointFinder Fingers;
    public AxisCalculator AxisCalc;

    public Force[] Forces = new Force[3];

    public 

    // Start is called before the first frame update
    void Start()
    {
        HapticFingers[0] = Thumb;
        HapticFingers[1] = Index;
        HapticFingers[2] = Middle;

        for (int i = 0; i < 3; i++)
        {
            Forces[i].Active = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 3; i++)
        {

            float val = AxisCalc.projectedPoints[i].magnitude;

            val /= (float)AxisCalc.axis_max;

            if (AxisCalc.projectedPoints[i].magnitude > AxisCalc.axis_max*0.95f) {
                val = 0;
            }

            Forces[i].Value = val;

            HapticFingers[i].Force = Forces[i];
        }

 
    }
    private void OnApplicationQuit()
    {
        for (int i = 0; i < 3; i++)
        {
            Forces[i].Value = 0f;
            HapticFingers[i].Force = Forces[i];
            Forces[i].Active = false;
        }
    }
}
