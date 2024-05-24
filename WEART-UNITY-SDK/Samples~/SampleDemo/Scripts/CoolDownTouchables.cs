using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Components;
using WeArt.Core;

namespace WEART
{
    public class CoolDownTouchables : MonoBehaviour
    {
        private WeArtTouchableObject _touchableObjects;

        private Temperature _originalTemperature;

        // Start is called before the first frame update
        void Start()
        {
         
        }

        // Update is called once per frame
        void Update()
        {

        }

        public void Initialize(WeArtTouchableObject touchable)
        {
            _touchableObjects = touchable;

            _originalTemperature = _touchableObjects.Temperature;
        }

        public void CoolDown()
        {
            Temperature newTemperature = Temperature.Default;
            newTemperature.Active = true;
            newTemperature.Value = 0.1f;

            _touchableObjects.Temperature = newTemperature;
        }

        public void ResetTemperature()
        {
            _touchableObjects.Temperature = _originalTemperature;
        }
    }
}
