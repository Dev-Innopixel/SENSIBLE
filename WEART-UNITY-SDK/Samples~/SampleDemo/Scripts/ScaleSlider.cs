using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScaleSlider : MonoBehaviour
{
    [SerializeField] Transform _target;
    [SerializeField] WeArtSlider _slider;
    [SerializeField] float _minScale =0.5f;
    [SerializeField] float _maxScale = 1.5f;
    private float _lastSliderValue = 0;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        _target.localScale = _target.localScale + Vector3.one  * -((_lastSliderValue - _slider.GetValue())/2);
        _lastSliderValue = _slider.GetValue();

        if(_target.localScale.x > _maxScale)
        {
            _target.localScale = Vector3.one * _maxScale;
        }

        if (_target.localScale.x < _minScale)
        {
            _target.localScale = Vector3.one * _minScale;
        }
    }

    public void SetLastSliderValue(float pValue)
    {
        _lastSliderValue = pValue;
    }
}
