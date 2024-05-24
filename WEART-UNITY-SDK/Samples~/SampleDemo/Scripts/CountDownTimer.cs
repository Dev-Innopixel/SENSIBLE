using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace WEART
{
    [AddComponentMenu("Scripts/WEART/CountDownTimer")]
    public class CountDownTimer : MonoBehaviour
    {
        [SerializeField]
        private bool _enableDemoMode = true;

        [SerializeField]
        private float _timeLeftSeconds = 90.0f;

        [SerializeField]
        private float[] _stepTime;

        [SerializeField]
        private Text _textTimer;

        private bool _startTimer = false;
        private int _currentState = 0;

        public delegate void StateChanged(int currentState);
        public StateChanged OnStateChanged;

        // Start is called before the first frame update
        void Start()
        {
            OnStateChanged?.Invoke(_currentState);

            if(_enableDemoMode)
            {
                _startTimer = true;
                _textTimer.gameObject.SetActive(true);
            }
            else
            {
                _textTimer.gameObject.SetActive(false);
            }
        }

        // Update is called once per frame
        void Update()
        {
            if (_startTimer)
            {
                if (_timeLeftSeconds > 0)
                {
                    _timeLeftSeconds -= Time.deltaTime;
                    string minituesLeft = Mathf.FloorToInt(_timeLeftSeconds / 60).ToString();
                    string seconds = (_timeLeftSeconds % 60).ToString("F0");
                    seconds = seconds.Length == 1 ? seconds = "0" + seconds : seconds;

                    WDebug.Log(minituesLeft + ":" + seconds);

                    _textTimer.text = "Time experience: " + minituesLeft + "' " + seconds + "''";

                    if (_currentState < _stepTime.Length)
                    {
                        float stepTime = _stepTime[_currentState];
                        if (_timeLeftSeconds <= stepTime)
                        {
                            _currentState++;
                            OnStateChanged?.Invoke(_currentState);
                            //WDebug.Log("CHANGE STATE: " + _currentState);
                        }
                    }
                    else
                    {
                        // End experience
                        WDebug.Log(">>>>>>> END TIMER EXPERIENCE <<<<<<<<<<<");
                    }
                }
            }
        }
    }
}
