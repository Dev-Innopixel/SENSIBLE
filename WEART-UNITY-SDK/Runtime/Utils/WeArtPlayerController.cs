using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.XR;

namespace WEART
{
    public class WeArtPlayerController : MonoBehaviour
    {
        //Add the transform of the Vr camera here
        [SerializeField]
        private Transform _targetOrientation;

        [SerializeField]
        private float _speed = 0.5f;

        [SerializeField]
        private float _stepPosY = 0.005f;

        void Start()
        {

        }

        void Update()
        {

            if (Input.GetKey(KeyCode.A))
            {
                transform.position -= _targetOrientation.right * Time.deltaTime * _speed;
            }
            else if (Input.GetKey(KeyCode.D))
            {
                //position.x += 0.05f;
                transform.position += _targetOrientation.right * Time.deltaTime * _speed;
            }
            else if (Input.GetKey(KeyCode.W))
            {
                transform.position += _targetOrientation.forward * Time.deltaTime * _speed;
            }
            else if (Input.GetKey(KeyCode.S))
            {
                transform.position -= _targetOrientation.forward * Time.deltaTime * _speed;
            }
            else if (Input.GetKey(KeyCode.Q))
            {
                Vector3 position = transform.position;
                position.y -= _stepPosY;
                transform.position = position;
            }
            else if (Input.GetKey(KeyCode.E))
            {
                Vector3 position = transform.position;
                position.y += _stepPosY;
                transform.position = position;
            }
            else if (Input.GetKey(KeyCode.Z))
            {
                transform.RotateAround(new Vector3(_targetOrientation.position.x,transform.position.y, _targetOrientation.position.z) ,Vector3.up,-1);
            }
            else if (Input.GetKey(KeyCode.X))
            {
                transform.RotateAround(new Vector3(_targetOrientation.position.x, transform.position.y, _targetOrientation.position.z), Vector3.up, 1);
            }
            else if (Input.GetKey(KeyCode.R))
            {
                List<InputDevice> devices = new List<InputDevice>();
                InputDevices.GetDevices(devices);
                if (devices.Count > 0)
                {
                    devices[0].subsystem.TryRecenter();
                }
            }
            else if (Input.GetKey(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }
}