using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WeArt.Components;
using WeArt.Core;
using Texture = WeArt.Core.Texture;

public class TouchableObjectTemperatureChange : MonoBehaviour
{

    [SerializeField]
    private WeArtTouchableObject _touchableObject;
    [SerializeField]
    private float _timeInterval = 1f;
    [SerializeField]
    private float _temperatureDifference = -0.1f;

    private bool _isTouching = false;
    private float _cooldown;
    
    void Start()
    {
        _cooldown = _timeInterval;
    }

    void Update()
    {
        if (!_isTouching)
        {
            if (_touchableObject.AffectedHapticObjects.Count > 0)
            {
                _isTouching = true;
            }
        }
        else
        {
            HanddleTemperatureChange();

            if (_touchableObject.AffectedHapticObjects.Count <= 0)
            {
                _isTouching = false;
                _cooldown = _timeInterval;
                SetTemperatureWithValue(0.5f);
            }
        }
    }

    private void HanddleTemperatureChange()
    {
        _cooldown -= Time.deltaTime;
        float finalTemperature;
        if (_cooldown < 0)
        {
            _cooldown = _timeInterval;
            finalTemperature = _touchableObject.Temperature.Value + _temperatureDifference;

            if(finalTemperature < 0)
                finalTemperature = 0;

            if(finalTemperature > 1)
                finalTemperature = 1;

            SetTemperatureWithValue(finalTemperature);
        }
    }

    /// <summary>
    /// Returns a temperature with the specified value (min 0, max 1).
    /// </summary>
    /// <param name="pValue"></param>
    private void SetTemperatureWithValue(float pValue)
    {
        Temperature temp = Temperature.Default;
        temp.Value = pValue;
        temp.Active = true; // Make sure to activate the property.
        _touchableObject.Temperature = temp;
    }

}
