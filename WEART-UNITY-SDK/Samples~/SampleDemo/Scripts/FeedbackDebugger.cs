using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;

public class FeedbackDebugger : MonoBehaviour
{
    [SerializeField]
    private WeArtHapticObject _index, _thumb, _middle;

    private float _lastForceIndex, _lastForceMiddle, _lastForceThumb;


    // Start is called before the first frame update
    void Start()
    {
        _lastForceIndex = _index.Force.Value;
        _lastForceMiddle = _middle.Force.Value;
        _lastForceThumb = _thumb.Force.Value;   
    }

    // Update is called once per frame
    void Update()
    {
        if(_lastForceIndex != _index.Force.Value)
        {
            Debug.Log("Force index update:    [" + _index.Force.Value + "]");
        }

        _lastForceIndex = _index.Force.Value;

    }
}
