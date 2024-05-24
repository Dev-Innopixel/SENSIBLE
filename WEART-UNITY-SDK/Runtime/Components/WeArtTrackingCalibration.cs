using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WeArt.Core;
using WeArt.Messages;
using UnityEngine.Events;

namespace WeArt.Components
{
    public class WeArtTrackingCalibration : MonoBehaviour
    {
        [System.Serializable]
        public class TrackingEvent : UnityEvent<HandSide> { }

        // Events
        [Header("Tracking Events")]

        // Unity Events
        [SerializeField]
        internal TrackingEvent _OnCalibrationStart;
        [SerializeField]
        internal TrackingEvent _OnCalibrationFinish;
        [SerializeField]
        internal TrackingEvent _OnCalibrationResultSuccess;
        [SerializeField]
        internal TrackingEvent _OnCalibrationResultFail;
        
        // Private fields
        private int CalibrationStatus = -1;
        private int CalibrationResult = -1;

        /// <summary>
        /// Static Class Calibration constants
        /// </summary>
        public static class Calibration
        {
            public static int STATE_INITIALIZATION = 0;
            public static int STATE_CALIBRATING = 1;
            public static int STATE_RUN = 2;

            public static int RESULT_SUCCESS = 0;
            public static int RESULT_ERROR = 1;
        }

        private HandSide currentHand;

        private bool task_calibrationStart = false;
        private bool task_calibrationFinished = false;
        private bool task_success = false;
        private bool task_failure = false;
        private bool task_resetDone = false;
        private bool task_resetRequired = false;

        private void Update()
        {
            if (task_calibrationStart)
            {
                _OnCalibrationStart?.Invoke(currentHand);
                task_calibrationStart = false;
            }

            if(task_calibrationFinished)
            {
                _OnCalibrationFinish?.Invoke(currentHand);
                task_calibrationFinished = false;
            }

            if(task_success)
            {
                _OnCalibrationResultSuccess?.Invoke(currentHand);
                task_success = false;
            }

            if (task_failure)
            {
                _OnCalibrationResultFail?.Invoke(currentHand);
                task_failure = false;
            }
        }

        private void Init()
        {
            var client = WeArtController.Instance.Client;
            client.OnConnectionStatusChanged -= OnConnectionChanged;
            client.OnConnectionStatusChanged += OnConnectionChanged;
            client.OnMessage -= OnMessageReceived;
            client.OnMessage += OnMessageReceived;
        }

        private void OnEnable()
        {
            Init();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (gameObject.scene.IsValid())
                Init();
        }
#endif

        internal void OnConnectionChanged(bool connected)
        {

        }

        private void OnMessageReceived(WeArtClient.MessageType type, IWeArtMessage message)
        {
            if (type == WeArtClient.MessageType.MessageReceived)
            {
                if (message is TrackingCalibrationStatus trackingCalibrationStatus)
                {
                    CalibrationStatus = trackingCalibrationStatus.GetStatus();
                    HandSide handSide = trackingCalibrationStatus.GetHandSide();
                    currentHand = handSide;

                    if (CalibrationStatus == Calibration.STATE_CALIBRATING)
                    {
                        task_calibrationStart = true;
                    }
                    else if (CalibrationStatus == Calibration.STATE_RUN)
                    {
                        task_calibrationFinished = true;
                    }
                }
                else if (message is TrackingCalibrationResult trackingCalibrationResult)
                {
                    CalibrationResult = trackingCalibrationResult.GetResult();
                    HandSide handSide = trackingCalibrationResult.GetHandSide();
                    currentHand = handSide;

                    if (CalibrationResult == Calibration.RESULT_SUCCESS)
                    {
                        task_success = true;
                    }
                    else if (CalibrationResult == Calibration.RESULT_ERROR)
                    {
                        task_failure = true;
                    }

                }
            }
        }
    }
}
