using System;
using System.Collections;
using UnityEngine;
using WeArt.Core;
using WeArt.Messages;
using WeArt.Utils;
using ClientError = WeArt.Core.WeArtClient.ErrorType;

namespace WeArt.Components
{
    /// <summary>
    /// This component wraps and exposes the network client that communicates with the hardware middleware.
    /// Use the <see cref="Instance"/> singleton property to retrieve the instance.
    /// </summary>
    public class WeArtController : MonoBehaviour
    {
        private static WeArtController _instance;

        /// <summary>
        /// The singleton instance of <see cref="WeArtController"/>
        /// </summary>
        public static WeArtController Instance
        {
            private set => _instance = Instance;
            get
            {
                if (_instance == null)
                    _instance = FindObjectOfType<WeArtController>();
                return _instance;
            }
        }


        [SerializeField]
        internal int _clientPort = 13031;

        [SerializeField]
        internal bool _startAutomatically = true;

        [SerializeField]
        internal bool _startCalibrationAutomatically = true;

        [SerializeField]
        internal bool _debugMessages = false;

        [NonSerialized]
        private WeArtClient _weArtClient;


        /// <summary>
        /// The network client that communicates with the hardware middleware.
        /// </summary>
        public WeArtClient Client
        {
            get
            {
                if (_weArtClient == null)
                {
                    _weArtClient = new WeArtClient
                    {
                        IpAddress = WeArtNetwork.LocalIPAddress,
                        Port = _clientPort,
                    };
                    _weArtClient.OnConnectionStatusChanged += OnConnectionChanged;
                    _weArtClient.OnTextMessage += OnMessage;
                    _weArtClient.OnError += OnError;
                }
                return _weArtClient;
            }
        }

        private void Awake()
        {
            if (_instance == null)
                _instance = this;

            else if (_instance != this)
                Destroy(this);
        }

        private void Start()
        {
            if (_startAutomatically)
            {
                Client.Start();
            }


        }

        private void OnDestroy()
        {
            Client.Stop();
        }

        private void OnApplicationQuit()
        {
            Client.Stop();
        }

        private void OnConnectionChanged(bool connected)
        {
            if (connected)
            {
                WeArtLog.Log($"Connected to {Client.IpAddress}.");

                if(_startAutomatically)
                {
                    Client.SendStartDevice();

                    if(_startCalibrationAutomatically)
                    {
                        Client.StartCalibration();
                    }
                }
            }
            else
                WeArtLog.Log($"Disconnected from {Client.IpAddress}.");
        }

        public void StartCalibration()
        {
            Client.StartCalibration();
        }

        public void StopCalibration()
        {
            Client.StopCalibration();
        }
        private void OnMessage(WeArtClient.MessageType type, string message)
        {
            if (!_debugMessages)
                return;

            if (type == WeArtClient.MessageType.MessageSent)
                WeArtLog.Log($"To Middleware: \"{message}\"");

            else if (type == WeArtClient.MessageType.MessageReceived)
                WeArtLog.Log($"From Middleware: \"{message}\"");
        }

        private void OnError(ClientError error, Exception exception)
        {
            string errorMessage;
            switch (error)
            {
                case ClientError.ConnectionError:
                    errorMessage = $"Cannot connect to {Client.IpAddress}";
                    break;
                case ClientError.SendMessageError:
                    errorMessage = $"Error on send message";
                    break;
                case ClientError.ReceiveMessageError:
                    errorMessage = $"Error on message received";
                    break;
                default:
                    throw new NotImplementedException();
            }
            WeArtLog.Log($"{errorMessage}\n{exception.StackTrace}", LogType.Error);
        }
    }
}