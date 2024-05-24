using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;

public class VariableTemperature : MonoBehaviour
{
    private WeArtTouchableObject _touchableObject;
    private Material _material;

    private Temperature _temperature;
    private float _currentTemperatureValue = 0.5f;
    private bool _dirTemp = false;
    private Color _blueColor = Color.blue;
    private Color _redColor = Color.red;

    // Start is called before the first frame update
    void Start()
    {
        _touchableObject = GetComponent<WeArtTouchableObject>();
        _material = GetComponent<Renderer>().material;

        _temperature = _touchableObject.Temperature;
        _temperature.Value = _currentTemperatureValue;
        _touchableObject.Temperature = _temperature;

        StartCoroutine(UpdateHaptics(1.5f));
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private IEnumerator UpdateHaptics(float delay)
    {
        yield return new WaitForSeconds(delay); 
        

        //Cold 
        if(!_dirTemp)
        {
            if(_currentTemperatureValue > 0.0)
            {
                _currentTemperatureValue -= .1f;
            }
            else
            {
                _dirTemp = true;
                _currentTemperatureValue = 0.0f;
            }
        }
        //Hot
        else if (_dirTemp)
        {
            if (_currentTemperatureValue < 1.0)
            {
                _currentTemperatureValue += .1f;
            }
            else
            {
                _dirTemp = false;
                _currentTemperatureValue = 1.0f;
            }
        }

        if(_currentTemperatureValue < 0.5f)
        {
            float offset = _currentTemperatureValue;
            Color newColor = new Color(_blueColor.r - offset, _blueColor.g - offset, _blueColor.b);
            _material.color = newColor;
        }
        else
        {
            float offset = _currentTemperatureValue;
            Color newColor = new Color(_redColor.r, _redColor.g - offset, _redColor.b - offset);
            _material.color = newColor;
        }

        _temperature.Value = _currentTemperatureValue;
        _touchableObject.Temperature = _temperature;

        StartCoroutine(UpdateHaptics(1.5f));
    }
}
