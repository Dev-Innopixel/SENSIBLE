using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;
using UnityEngine.Events;

[System.Serializable]
public class GestureConstrains
{
    public GestureManager.GestureType type;
    public float minThumbValue;
    public float maxThumbValue;
    public float minIndexValue;
    public float maxIndexValue;
    public float minMiddleValue;
    public float maxMiddleValue;
    public GestureManager.GestureDirection handDirection;
    public GestureManager.GestureDirection viewDirection;
    public float dotProductValue;
}

public class GestureManager : MonoBehaviour
{
    [SerializeField] private WeArtThimbleTrackingObject _hapticThumb;
    [SerializeField] private WeArtThimbleTrackingObject _hapticIndex;
    [SerializeField] private WeArtThimbleTrackingObject _hapticMiddle;
    [SerializeField] private WeArtHandController _handController;
    [SerializeField] private Transform _handTransform;
    [SerializeField] private Transform _viewTransform;

    [SerializeField] private float _maxTimeForGesture = 1f;

    public UnityEvent none = new UnityEvent();
    public UnityEvent stop = new UnityEvent();
    public UnityEvent pinch = new UnityEvent();
    public UnityEvent thumbsUp = new UnityEvent();
    public UnityEvent fist = new UnityEvent();

    List<GestureConstrains> _gestureConstrains = new List<GestureConstrains>();
    private GestureConstrains _currentGesture; 

    public enum GestureType { None, Stop, Pinch, ThumbsUp, Fist}
    public enum GestureDirection { Front, Back, Up, Down, Left, Right };
    private GestureType _lastGesture = GestureType.None;

    private float _progressTimeGesture = 0;
    private GestureConstrains _progressGesture;
    private bool _needToChangeGesture = false;

    void Start()
    {
        InitializeGestures();
    }

    void InitializeGestures()
    {
        AddGesture(GestureType.Stop, 0f, 0f, 0f, 0f, 0f, 0f, GestureDirection.Down, GestureDirection.Front, 0.6f);
        AddGesture(GestureType.Pinch, 0f, 1f, 0.3f, 1f, 0f, 0.3f,GestureDirection.Front, GestureDirection.Front , 0.1f);
        AddGesture(GestureType.ThumbsUp, 0f, 0f, 0.3f, 1f, 0.3f, 1f, GestureDirection.Left,GestureDirection.Up, 0.6f);
        AddGesture(GestureType.Fist, 0.5f, 1f, 0.5f, 1f, 0.5f, 1f, GestureDirection.Down,GestureDirection.Front, 0.6f);
    }

    void AddGesture(GestureType pType, float pMinThumbValue, float pMaxThumbValue, float pMinIndexValue,
        float pMaxIndexValue,float pMinMiddleValue, float pMaxMiddleValue, GestureDirection pHandDirection,
        GestureDirection pViewDirection, float pDotProductValue )
    {
        GestureConstrains gesture = new GestureConstrains();
        gesture.type = pType;
        gesture.minThumbValue = pMinThumbValue;
        gesture.maxThumbValue = pMaxThumbValue;
        gesture.minIndexValue = pMinIndexValue;
        gesture.maxIndexValue = pMaxIndexValue;
        gesture.minMiddleValue = pMinMiddleValue;
        gesture.maxMiddleValue = pMaxMiddleValue;
        gesture.handDirection = pHandDirection;
        gesture.viewDirection = pViewDirection;
        gesture.dotProductValue = pDotProductValue;

        _gestureConstrains.Add(gesture);
    }

    void Update()
    {
        //Debug.Log(_hapticThumb.Closure.Value + " " + _hapticIndex.Closure.Value + " " + _hapticMiddle.Closure.Value);
        CheckCurrentGesture();
    }

    void CheckCurrentGesture()
    {
        _currentGesture = null;

        if(_handController.GraspingState == GraspingState.Grabbed)
        {
            ResetGesture();
            return;
        }

        foreach (var gesture in _gestureConstrains)
        {
            if (_hapticThumb.Closure.Value >= gesture.minThumbValue && _hapticThumb.Closure.Value <= gesture.maxThumbValue
            && _hapticIndex.Closure.Value >= gesture.minIndexValue && _hapticIndex.Closure.Value <= gesture.maxIndexValue
            && _hapticMiddle.Closure.Value >= gesture.minMiddleValue && _hapticMiddle.Closure.Value <= gesture.maxMiddleValue)
            {

                if (DotProductCheck(gesture.handDirection, gesture.viewDirection, gesture.dotProductValue))
                {
                    _currentGesture = gesture;
                    break;
                }

            }
        }

        if (_currentGesture != null)
        {
            if(_currentGesture != _progressGesture )
            {
                ResetGesture();
            }

            if(_needToChangeGesture)
            {
                return;
            }

            _progressTimeGesture += Time.deltaTime;

            if (_progressTimeGesture > _maxTimeForGesture)
            {
                _needToChangeGesture = true;

                switch (_currentGesture.type)
                {
                    case GestureType.Stop:
                        Debug.Log("Stop");
                        stop.Invoke();
                        break;

                    case GestureType.Pinch:
                        Debug.Log("Pinch");
                        pinch.Invoke();
                        break;

                    case GestureType.ThumbsUp:
                        Debug.Log("ThumbsUp");
                        thumbsUp.Invoke();
                        break;
                    case GestureType.Fist:
                        Debug.Log("Fist");
                        fist.Invoke();
                        break;
                }
            }
        }
        else
        {
            ResetGesture();
        }
    }

    void ResetGesture()
    {
        _progressGesture = _currentGesture;
        _progressTimeGesture = 0;
        _needToChangeGesture = false;
        none.Invoke();
    }

    bool DotProductCheck(GestureDirection pHandDirection,GestureDirection pViewDirection, float pDotProductValue)
    {
        Vector3 handVector = Vector3.zero;
        Vector3 viewVector = Vector3.zero;

        switch(pHandDirection)
        {
            case GestureDirection.Left:
                handVector = _handTransform.right;
                break;
            case GestureDirection.Right:
                handVector = _handTransform.right * -1;
                break;
            case GestureDirection.Up:
                handVector = _handTransform.up;
                break;
            case GestureDirection.Down:
                handVector = _handTransform.up * -1;
                break;
            case GestureDirection.Front:
                handVector = _handTransform.forward * -1;
                break;
            case GestureDirection.Back:
                handVector = _handTransform.forward;
                break;
        }

        switch (pViewDirection)
        {
            case GestureDirection.Left:
                viewVector = _viewTransform.right * -1;
                break;
            case GestureDirection.Right:
                viewVector = _viewTransform.right ;
                break;
            case GestureDirection.Up:
                viewVector = _viewTransform.up;
                break;
            case GestureDirection.Down:
                viewVector = _viewTransform.up * -1;
                break;
            case GestureDirection.Front:
                viewVector = _viewTransform.forward ;
                break;
            case GestureDirection.Back:
                viewVector = _viewTransform.forward * -1;
                break;
        }

        return Vector3.Dot(handVector, viewVector) > pDotProductValue;
    }

}


