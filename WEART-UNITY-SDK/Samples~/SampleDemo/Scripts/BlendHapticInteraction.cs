using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WeArt.Components;
using WeArt.Core;
using Texture = WeArt.Core.Texture;

public class BlendHapticInteraction : MonoBehaviour
{

    [SerializeField]
    private Temperature _temperature;

    [SerializeField]
    private Force _force;

    [SerializeField]
    private Texture _texture;

    private Temperature _originalTemperature;
    private Texture _originalTexture;

    private WeArtTouchEffect _touchEffect;
    
    // Start is called before the first frame update
    void Start()
    {
        _touchEffect = new WeArtTouchEffect();
        _texture.Velocity = 0.5f;
        _texture.Volume = 80.0f;

        _touchEffect.Set(_temperature, _force, _texture, null);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private bool TryGetTouchableObject(Collider other, out WeArtTouchableObject touchableObject)
    {
        touchableObject = other.GetComponent<WeArtTouchableObject>();
        if (touchableObject != null)
        {
            return true;
        }
        else
            return false;
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
        WeArtTouchableObject touchableObject;
        if(TryGetTouchableObject(other, out touchableObject))
        {
            _originalTemperature = touchableObject.Temperature;
            _originalTexture = touchableObject.Texture;

            touchableObject.Temperature = _temperature;
            touchableObject.Texture = _texture;

        }

        WeArtHapticObject hapticObject;
        if (TryGetHapticObject(other, out hapticObject))
        {
            if (hapticObject.TouchedObjects.Count <= 0)
            {
                hapticObject.AddEffect(_touchEffect);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        WeArtTouchableObject touchableObject;
        if (TryGetTouchableObject(other, out touchableObject))
        {
            touchableObject.Temperature = _temperature;
            touchableObject.Texture = _texture;
        }

        WeArtHapticObject hapticObject;
        if(TryGetHapticObject(other, out hapticObject))
        {
            /*
            if (hapticObject.TouchedObjects.Count <= 0)
                hapticObject.Set(_touchEffect);
            *//*
            _texture.Active = true;
            _texture.VelocityZ = 0.5f;
            _texture.Volume = 100;
            _touchEffect.Set(_temperature, _force, _texture, null);*/
        }
    }

    private void OnTriggerExit(Collider other)
    {
        WeArtTouchableObject touchableObject;
        if (TryGetTouchableObject(other, out touchableObject))
        {
            touchableObject.Temperature = _originalTemperature;
            touchableObject.Texture = _originalTexture;
        }

        WeArtHapticObject hapticObject;
        if (TryGetHapticObject(other, out hapticObject))
        {
            hapticObject.RemoveEffect(_touchEffect);
        }
    }
}
