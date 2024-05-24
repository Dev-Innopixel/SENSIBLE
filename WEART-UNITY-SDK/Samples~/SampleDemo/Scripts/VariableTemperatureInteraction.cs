using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;

public class VariableTemperatureInteraction : MonoBehaviour
{ 
    private enum TemperatureType { Hot = 0, Cold };

    private WeArtTouchableObject _touchableObject;

    private bool _stillTriggered;

    [SerializeField]
    private TemperatureType _temperatureType;

    // Start is called before the first frame update
    void Start()
    {
        _touchableObject = GetComponent<WeArtTouchableObject>(); 
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void OnTriggerEnter(Collider other)
    { 
        if (other.name == "WEARTLeftHand" || other.name == "WEARTRightHand") {

            _stillTriggered = true;

            Temperature temperature = _touchableObject.Temperature;
            temperature.Value = 0.5f;
            _touchableObject.Temperature = temperature;

            switch (_temperatureType)
            {
                case TemperatureType.Hot:
                    StartCoroutine(IncreaseTemperature(1.0f));
                    break;
                case TemperatureType.Cold:
                    StartCoroutine(DecreaseTemperature(1.0f));
                    break;
                default:
                    break;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    { 

        if (other.name == "WEARTLeftHand" || other.name == "WEARTRightHand")
        {
            Temperature temperature = _touchableObject.Temperature;
            temperature.Value = 0.5f;
            _touchableObject.Temperature = temperature;

            _stillTriggered = false;
        }
    }

    private IEnumerator IncreaseTemperature(float delay)
    {
        yield return new WaitForSeconds(delay);

        Temperature temperature = _touchableObject.Temperature;
        if(temperature.Value < 1.0f && _stillTriggered) 
        {
            temperature.Value += 0.2f;
            _touchableObject.Temperature = temperature;
            StartCoroutine(IncreaseTemperature(1.0f));
        }
    }

    private IEnumerator DecreaseTemperature(float delay)
    {
        yield return new WaitForSeconds(delay);

        Temperature temperature = _touchableObject.Temperature;
        if (temperature.Value > 0.0f && _stillTriggered)
        {
            temperature.Value -= 0.1f;
            _touchableObject.Temperature = temperature;
            if (_stillTriggered)
                StartCoroutine(DecreaseTemperature(1.0f));
        }
    }


}
