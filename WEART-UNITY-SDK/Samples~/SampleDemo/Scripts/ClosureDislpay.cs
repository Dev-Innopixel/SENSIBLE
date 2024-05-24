using System.Collections;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using WeArt.Components;
using WeArt.Core;
using Texture = WeArt.Core.Texture;
using UnityEngine.UI;
public class ClosureDislpay : MonoBehaviour
{
    [SerializeField] private WeArtThimbleTrackingObject _thimbleTrackingObject;
    [SerializeField] private Transform _closureDisplay;
    [SerializeField] private Text _closureText;

    void Start()
    {
        
    }

    void Update()
    {
        _closureDisplay.localPosition = new Vector3(0, _thimbleTrackingObject.Closure.Value * 0.3f, 0);
        string textDisplay = _thimbleTrackingObject.Closure.Value.ToString();
        if (textDisplay.Length > 4)
        {
            textDisplay = textDisplay.Substring(0, 4);
        }
        _closureText.text = textDisplay;
    }
}
