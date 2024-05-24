using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WeArt.Components;
using WeArt.Core;
using Texture = WeArt.Core.Texture;

public class HapticObjectTemperatureChange : MonoBehaviour
{

    [SerializeField]
    private float _timeInterval = 1f;
    [SerializeField]
    private float _temperatureDifference = 0.1f;

    private bool _isTouching = false;
    private float _cooldown;

    private int _collidingHapticObjects = 0;

    private WeArtTouchEffect _touchEffect;
    
    void Start()
    {
        _touchEffect = new WeArtTouchEffect();

        Temperature temp = Temperature.Default;
        temp.Active = true; // Make sure to activate the properties.

        Force force = Force.Default;
        force.Active = true;
        force.Value = 0.7f;

        Texture texture = Texture.Default;
        texture.Active = true;
        texture.TextureType = TextureType.Aluminium;

        _touchEffect.Set(temp, force, texture, null);
    }

    void Update()
    {
        if (!_isTouching)
        {
            if (_collidingHapticObjects > 0)
            {
                _isTouching = true;
            }
        }
        else
        {
            HanddleTemperatureChange();

            if (_collidingHapticObjects <= 0)
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
            finalTemperature = _touchEffect.Temperature.Value + _temperatureDifference;

            if (finalTemperature < 0)
                finalTemperature = 0;

            if (finalTemperature > 1)
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
        _touchEffect.Set(temp, _touchEffect.Force, _touchEffect.Texture, null);
    }

    private bool TryGetHapticObject(Collider other, out WeArtHapticObject hapticObject)
    {
        hapticObject = other.GetComponent<WeArtHapticObject>();
        if(hapticObject != null)
        {
            return true;
        }
        else 
            return false;

    }

    private void OnTriggerEnter(Collider other)
    {
        WeArtHapticObject hapticObject;
        if (TryGetHapticObject(other, out hapticObject))
        {
            if (hapticObject.TouchedObjects.Count <= 0)
            {
                hapticObject.AddEffect(_touchEffect);
                _collidingHapticObjects++;
            }
        }
    }


    private void OnTriggerExit(Collider other)
    {
        WeArtHapticObject hapticObject;
        if (TryGetHapticObject(other, out hapticObject))
        {
            hapticObject.RemoveEffect(_touchEffect);
            _collidingHapticObjects--;
        }
    }
}
