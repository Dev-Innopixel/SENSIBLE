using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using WeArt.Core;
using WeArt.Messages;
using Texture = WeArt.Core.Texture;

namespace WeArt.Components
{
    /// <summary>
    /// This component controls the haptic actuators of one or more hardware thimbles.
    /// The haptic control can be issued:
    /// 1) Manually from the Unity inspector
    /// 2) When a <see cref="WeArtTouchableObject"/> collides with this object
    /// 3) On custom haptic effects added or removed
    /// 4) On direct value set, through the public properties
    /// </summary>
    public class WeArtHapticObject : MonoBehaviour
    {
        [SerializeField]
        internal HandSideFlags _handSides = HandSideFlags.None;

        [SerializeField]
        internal ActuationPointFlags _actuationPoints = ActuationPointFlags.None;


        [SerializeField]
        internal Temperature _temperature = Temperature.Default;

        [SerializeField]
        internal Force _force = Force.Default;

        [SerializeField]
        internal Texture _texture = Texture.Default;


        [NonSerialized]
        internal IWeArtEffect _activeEffect;

        [NonSerialized]
        internal List<WeArtTouchableObject> _touchedObjects = new List<WeArtTouchableObject>();


        /// <summary>
        /// Called when the resultant haptic effect changes because of the influence
        /// caused by the currently active effects
        /// </summary>
        public event Action OnActiveEffectsUpdate;


        /// <summary>
        /// The hand sides to control with this component
        /// </summary>
        public HandSideFlags HandSides
        {
            get => _handSides;
            set
            {
                if (value != _handSides)
                {
                    var sidesToStop = _handSides ^ value & _handSides;
                    _handSides = sidesToStop;
                    StopControl();

                    _handSides = value;
                    StartControl();
                }
            }
        }

        /// <summary>
        /// The thimbles to control with this component
        /// </summary>
        public ActuationPointFlags ActuationPoints
        {
            get => _actuationPoints;
            set
            {
                if (value != _actuationPoints)
                {
                    var pointsToStop = _actuationPoints ^ value & _actuationPoints;
                    _actuationPoints = pointsToStop;
                    StopControl();

                    _actuationPoints = value;
                    StartControl();
                }
            }
        }

        /// <summary>
        /// The current temperature of the specified thimbles
        /// </summary>
        public Temperature Temperature
        {
            get => _temperature;
            set
            {
                if (!_temperature.Equals(value))
                {
                    _temperature = value;

                    if (value.Active)
                        SendSetTemperature();
                    else
                        SendStopTemperature();
                }
            }
        }

        /// <summary>
        /// The current pressing force of the specified thimbles
        /// </summary>
        public Force Force
        {
            get => _force;
            set
            {
                if (!_force.Equals(value))
                {
                    _force = value;

                    if (value.Active)
                        SendSetForce();
                    else
                        SendStopForce();
                }
            }
        }

        /// <summary>
        /// The current texture feeling applied on the specified thimbles
        /// </summary>
        public Texture Texture
        {
            get => _texture;
            set
            {
                if (!_texture.Equals(value))
                {
                    _texture = value;

                    if (value.Active)
                        SendSetTexture();
                    else
                        SendStopTexture();
                }
            }
        }

        /// <summary>
        /// The currently active effects on this object
        /// </summary>
        public IWeArtEffect ActiveEffect => _activeEffect;

        /// <summary>
        /// The currently active effects on this object
        /// </summary>
        public IReadOnlyList<WeArtTouchableObject> TouchedObjects => _touchedObjects;


        /// <summary>
        /// Adds a haptic effect to this object. This effect will have an influence
        /// as long as it is not removed or the haptic properties are programmatically
        /// forced to have a specified value.
        /// </summary>
        /// <param name="effect">The haptic effect to add to this object</param>
        public void AddEffect(IWeArtEffect effect)
        {
            _activeEffect = effect;
            UpdateEffects();
            effect.OnUpdate += UpdateEffects;
        }

        /// <summary>
        /// Removes a haptic effect from the set of influencing effects
        /// </summary>
        /// <param name="effect">The haptic effect to remove</param>
        public void RemoveEffect(IWeArtEffect effect)
        {
            _activeEffect = null;
            UpdateEffects();
            effect.OnUpdate -= UpdateEffects;
        }

        /// <summary>
        /// Internally updates the resultant haptic effect caused by the set of active effects.
        /// </summary>
        private void UpdateEffects()
        {
            var lastTemperature = Temperature.Default;
            if (_activeEffect != null)
            {
                lastTemperature = _activeEffect.Temperature;
            }

            Temperature = lastTemperature;

            var lastForce = Force.Default;
            if (_activeEffect != null)
            {
                lastForce = _activeEffect.Force;
            }
            Force = lastForce;

            var lastTexture = Texture.Default;
            if (_activeEffect != null)
            {
                lastTexture = _activeEffect.Texture;
            }
            Texture = lastTexture;

            OnActiveEffectsUpdate?.Invoke();
        }

        private void Init()
        {
            var client = WeArtController.Instance.Client;
            client.OnConnectionStatusChanged -= OnConnectionChanged;
            client.OnConnectionStatusChanged += OnConnectionChanged;
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
            if (connected)
                StartControl();
        }

        internal void StartControl()
        {
            if (Temperature.Active)
                SendSetTemperature();

            if (Force.Active)
                SendSetForce();

            if (Texture.Active)
                SendSetTexture();
        }

        internal void StopControl()
        {
            if (Temperature.Active)
                SendStopTemperature();

            if (Force.Active)
                SendStopForce();

            if (Texture.Active)
                SendStopTexture();
        }


        private void SendSetTemperature() => SendMessage((handSide, actuationPoint) => new SetTemperatureMessage()
        {
            Temperature = _temperature.Value,
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopTemperature() => SendMessage((handSide, actuationPoint) => new StopTemperatureMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendSetForce() => SendMessage((handSide, actuationPoint) => new SetForceMessage()
        {
            Force = new float[] { _force.Value, _force.Value, _force.Value },
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopForce() => SendMessage((handSide, actuationPoint) => new StopForceMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendSetTexture() => SendMessage((handSide, actuationPoint) => new SetTextureMessage()
        {
            //TODO here we can force the velocity of the texture
            TextureIndex = (int)_texture.TextureType,
            TextureVelocity = new float[] { WeArtConstants.defaultTextureVelocity_X, WeArtConstants.defaultTextureVelocity_Y, _texture.Velocity },
            TextureVolume = _texture.Volume,
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendStopTexture() => SendMessage((handSide, actuationPoint) => new StopTextureMessage()
        {
            HandSide = handSide,
            ActuationPoint = actuationPoint
        });

        private void SendMessage(Func<HandSide, ActuationPoint, IWeArtMessage> createMessage)
        {
            var controller = WeArtController.Instance;
            if (controller == null)
                return;

            foreach (var handSide in WeArtConstants.HandSides)
                if (HandSides.HasFlag((HandSideFlags)(1 << (int)handSide)))
                    foreach (var actuationPoint in WeArtConstants.ActuationPoints)
                        if (ActuationPoints.HasFlag((ActuationPointFlags)(1 << (int)actuationPoint)))
                            controller.Client.SendMessage(createMessage(handSide, actuationPoint));
        }
    }
}