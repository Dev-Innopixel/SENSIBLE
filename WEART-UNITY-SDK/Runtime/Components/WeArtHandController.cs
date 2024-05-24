using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Playables;
using WeArt.Core;
using Texture = WeArt.Core.Texture;

using static WeArt.Components.WeArtTouchableObject;
using System.Linq;
using System.Collections;

namespace WeArt.Components
{
    /// <summary>
    /// This component is able to animate a virtual hand using closure data coming from
    /// a set of <see cref="WeArtThimbleTrackingObject"/> components.
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class WeArtHandController : MonoBehaviour
    {
        #region Fields

        /// <summary>
        /// Defines the _openedHandState.
        /// </summary>
        [SerializeField]
        internal AnimationClip _openedHandState;

        /// <summary>
        /// Defines the _closedHandState.
        /// </summary>
        [SerializeField]
        internal AnimationClip _closedHandState;

        /// <summary>
        /// Defines the _abductionHandState.
        /// </summary>
        [SerializeField]
        internal AnimationClip _abductionHandState;

        /// <summary>
        /// Defines the _thumbMask, _indexMask, _middleMask, _ringMask, _pinkyMask.
        /// </summary>
        [SerializeField]
        internal AvatarMask _thumbMask, _indexMask, _middleMask, _ringMask, _pinkyMask;

        /// <summary>
        /// Defines the _thumbThimble, _indexThimble, _middleThimble.
        /// </summary>
        [SerializeField]
        internal WeArtThimbleTrackingObject _thumbThimbleTracking, _indexThimbleTracking, _middleThimbleTracking;

        [SerializeField]
        internal WeArtHapticObject _thumbThimbleHaptic, _indexThimbleHaptic, _middleThimbleHaptic;


        [SerializeField]
        internal SkinnedMeshRenderer _handSkinnedMeshRenderer;

        [SerializeField]
        internal GameObject _logoHandPanel;

        [SerializeField]
        internal GameObject _grasper;

        [SerializeField]
        internal bool _graspSnapToPosition;

        /// <summary>
        /// Show the hand during the grasp.
        /// </summary>
        [SerializeField]
        internal bool _showHandOnGrasp;

        /// <summary>
        /// If you use custom poses, you have to add WeArtGraspPose.cs
        /// </summary>
        [SerializeField]
        internal bool _useCustomPoses;


        /// <summary>
        /// Defines the _animator.
        /// </summary>
        private Animator _animator;

        /// <summary>
        /// Defines the _fingers.
        /// </summary>
        private AvatarMask[] _fingers;

        /// <summary>
        /// Defines the _thimbles.
        /// </summary>
        private WeArtThimbleTrackingObject[] _thimbles;

        private PlayableGraph _graph;

        private AnimationLayerMixerPlayable[] _fingersMixers;

        private WeArtTouchableObject _touchableObject;

        private HandClosingState _handClosingState = HandClosingState.Open;

        private GraspingState _graspingState = GraspingState.Released;

        private readonly WeArtTouchEffect _thumbGraspingEffect = new WeArtTouchEffect();

        private readonly WeArtTouchEffect _indexGraspingEffect = new WeArtTouchEffect();

        private readonly WeArtTouchEffect _middleGraspingEffect = new WeArtTouchEffect();

        /// <summary>
        /// Define the hand side
        /// </summary>
        [SerializeField]
        internal HandSide _handSide;

        public delegate void GraspingDelegate(HandSide handSide, GameObject gameObject);
        public GraspingDelegate OnGraspingEvent;
        public GraspingDelegate OnReleaseEvent;

        #endregion

        #region Methods

        /// <summary>
        /// The Awake.
        /// </summary>
        private void Awake()
        {
            _animator = GetComponent<Animator>();
            _fingers = new AvatarMask[] { _thumbMask, _indexMask, _middleMask, _ringMask, _pinkyMask };
            _thimbles = new WeArtThimbleTrackingObject[] {
                _thumbThimbleTracking,
                _indexThimbleTracking,
                _middleThimbleTracking, _middleThimbleTracking, _middleThimbleTracking
            };
        }

        /// <summary>
        /// The OnEnable.
        /// </summary>
        private void OnEnable()
        {
            _graph = PlayableGraph.Create(nameof(WeArtHandController));

            var fingersLayerMixer = AnimationLayerMixerPlayable.Create(_graph, _fingers.Length);
            _fingersMixers = new AnimationLayerMixerPlayable[_fingers.Length];

            for (uint i = 0; i < _fingers.Length; i++)
            {
                var fingerMixer = AnimationLayerMixerPlayable.Create(_graph, 3);
                _graph.Connect(AnimationClipPlayable.Create(_graph, _openedHandState), 0, fingerMixer, 0);
                _graph.Connect(AnimationClipPlayable.Create(_graph, _closedHandState), 0, fingerMixer, 1);
                _graph.Connect(AnimationClipPlayable.Create(_graph, _abductionHandState), 0, fingerMixer, 2);

                fingerMixer.SetLayerAdditive(0, false);
                fingerMixer.SetLayerMaskFromAvatarMask(0, _fingers[i]);
                fingerMixer.SetInputWeight(0, 1);
                fingerMixer.SetInputWeight(1, 0);
                _fingersMixers[i] = fingerMixer;

                fingersLayerMixer.SetLayerAdditive(i, false);
                fingersLayerMixer.SetLayerMaskFromAvatarMask(i, _fingers[i]);
                _graph.Connect(fingerMixer, 0, fingersLayerMixer, (int)i);
                fingersLayerMixer.SetInputWeight((int)i, 1);
            }

            var handMixer = AnimationMixerPlayable.Create(_graph, 2);
            _graph.Connect(fingersLayerMixer, 0, handMixer, 0);
            handMixer.SetInputWeight(0, 1);
            var playableOutput = AnimationPlayableOutput.Create(_graph, nameof(WeArtHandController), _animator);
            playableOutput.SetSourcePlayable(handMixer);
            _graph.SetTimeUpdateMode(DirectorUpdateMode.GameTime);
            _graph.Play();

            // Subscribe custom finger closure behaviour during the grasp
            OnGraspingEvent += UpdateFingerClosure;
        }

        /// <summary>
        /// Get the grasping state of the hand controller.
        /// </summary>
        public GraspingState GraspingState
        {
            get => _graspingState;
            set => _graspingState = value;
        }

        /// <summary>
        /// Get the hand visibility during grasping
        /// </summary>
        public bool ShowHandOnGrasp
        {
            get => _showHandOnGrasp;
            set => _showHandOnGrasp = value;
        }

        /// <summary>
        /// The Update.
        /// </summary>
        private void Update()
        {
            _graph.Evaluate();


            if (_useCustomPoses && _graspingState == GraspingState.Grabbed)
            {
                // In this case the fingers not follow the tracking but are driven by WeArtGraspPose
                // The behaviour is called in this script in -> UpdateFingerClousure
            }
            else // Otherwise fingers behaviour works as always
            {
                for (int i = 0; i < _fingers.Length; i++)
                {
                    var weight = _thimbles[i].Closure.Value;
                    var abduction = _thimbles[i].Abduction.Value;

                    if (_thimbles[i].ActuationPoint == ActuationPoint.Thumb)
                        //Debug.Log("abduction value: " +  i + ": " +abduction);

                    _fingersMixers[i].SetInputWeight(0, 1 - weight);
                    _fingersMixers[i].SetInputWeight(1, weight);
                    _fingersMixers[i].SetInputWeight(2, abduction);
                }
            }

            CheckHandClosingState();

            HandlingGraspingState();

            if (_touchableObject != null && _graspingState == GraspingState.Grabbed)
            {
                ComputeDynamicGraspForce(
                    _thumbThimbleTracking.Closure.Value,
                    _middleThimbleTracking.Closure.Value,
                    _indexThimbleTracking.Closure.Value);
            }
        }
        

        /// <summary>
        /// The OnDisable.
        /// </summary>
        private void OnDisable()
        {
            _graph.Destroy();

            OnGraspingEvent -= UpdateFingerClosure;
        }

        /// <summary>
        /// Handle the behaviour of all fingers during the grasp
        /// </summary>
        private void UpdateFingerClosure(HandSide hand, GameObject gameObject)
        {          
            if (_useCustomPoses)
            {
                // In this case you have to use WeArtGraspPose on your touchable object to handle the fingers poses
                if (TryGetCustomPosesFromTouchable(gameObject, out var customPoses))
                {
                    StopAllCoroutines();

                    for (int i = 0; i < customPoses.fingersClosure.Length; i++)
                    {
                        var weight = customPoses.fingersClosure[i];                       

                        StartCoroutine(LerpPoses(_fingersMixers[i], 0, 1 - weight, customPoses.lerpTime));
                        StartCoroutine(LerpPoses(_fingersMixers[i], 1, weight, customPoses.lerpTime));
                    }
                }
            }
        }

        /// <summary>
        /// State Machine for Hand closure from paramenters 
        /// </summary>
        private void CheckHandClosingState()
        {
            if (_thumbThimbleTracking.Closure.Value >= WeArtConstants.thresholdThumbClosure &&
              _middleThimbleTracking.Closure.Value >= WeArtConstants.thresholdMiddleClosure ||
              _indexThimbleTracking.Closure.Value >= WeArtConstants.thresholdIndexClosure)
            {
                switch (_handClosingState)
                {
                    case HandClosingState.Open:
                        _handClosingState = HandClosingState.Closing;
                        break;
                    case HandClosingState.Closing:
                        _handClosingState = HandClosingState.Closed;
                        break;
                }
            }
            else
            {
                _handClosingState = HandClosingState.Open;
            }
        }

        /// <summary>
        /// Handle the changing state for grasping TouchableObjects
        /// </summary>
        private void HandlingGraspingState()
        {
            if (_handClosingState == HandClosingState.Closing && _touchableObject != null && _graspingState == GraspingState.Released)
            {
                _graspingState = GraspingState.Grabbed;

                OnGraspingEvent?.Invoke(_handSide, _touchableObject.gameObject);

                if(_touchableObject.GetGraspingState() == GraspingState.Grabbed)
                {
                    _touchableObject.Release();
                }

                _touchableObject.Grab(_grasper, _graspSnapToPosition);
                _touchableObject.GraspingHandController = this;

                //TODO here we can force the velocity of the texture
                if(_showHandOnGrasp == false)
                    SnapHand(false);

                UpdateGraspingEffect(WeArtConstants.graspForce, WeArtConstants.graspForce, WeArtConstants.graspForce);
                _thumbThimbleHaptic.AddEffect(_thumbGraspingEffect);
                _indexThimbleHaptic.AddEffect(_indexGraspingEffect);
                _middleThimbleHaptic.AddEffect(_middleGraspingEffect);
            }

            else if (_handClosingState == HandClosingState.Open && _touchableObject != null && _graspingState == GraspingState.Grabbed)
            {
                _graspingState = GraspingState.Released;

                if (_touchableObject.GraspingHandController == this)
                {
                    _touchableObject.Release();
                    _touchableObject.GraspingHandController = null;
                }
                SnapHand(true);

                OnReleaseEvent?.Invoke(_handSide, _touchableObject.gameObject);

                //TODO here we can force the velocity of the texture
                UpdateGraspingEffect(0, 0, 0);

                _thumbThimbleHaptic.RemoveEffect(_thumbGraspingEffect);
                _indexThimbleHaptic.RemoveEffect(_indexGraspingEffect);
                _middleThimbleHaptic.RemoveEffect(_middleGraspingEffect);
            }
        }

        private void ComputeDynamicGraspForce(float thumbClosureValue, float middelClosureValue, float indexClosureValue)
        {
            float deltaThumb = thumbClosureValue - WeArtConstants.thresholdThumbClosure;
            float deltaMiddle = middelClosureValue - WeArtConstants.thresholdMiddleClosure;
            float deltaIndex = indexClosureValue - WeArtConstants.thresholdIndexClosure;

            float stiffnessObject = _touchableObject.Stiffness.Value;

            float dinamicForceThumb = (deltaThumb * stiffnessObject) * WeArtConstants.dinamicForceSensibility;
            float dinamicForceMiddle = (deltaMiddle * stiffnessObject) * WeArtConstants.dinamicForceSensibility;
            float dinamicForceIndex = (deltaIndex * stiffnessObject) * WeArtConstants.dinamicForceSensibility;

            dinamicForceThumb = WeArtUtility.NormalizedGraspForceValue(dinamicForceThumb);
            dinamicForceMiddle = WeArtUtility.NormalizedGraspForceValue(dinamicForceMiddle);
            dinamicForceIndex = WeArtUtility.NormalizedGraspForceValue(dinamicForceIndex);

            //TODO here we can force the velocity of the texture
            UpdateGraspingEffect(dinamicForceThumb, dinamicForceIndex, dinamicForceMiddle);
        }

        private void UpdateGraspingEffect(float thumbForce, float indexForce, float middleForce)
        {
            Temperature temperature = Temperature.Default;
            if (_touchableObject != null)
            {
                temperature = _touchableObject.Temperature;
            }

            Force force = Force.Default;
            force.Active = true;

            force.Value = thumbForce;
            _thumbGraspingEffect.Set(temperature, force, Texture.Default, null);

            force.Value = indexForce;
            _indexGraspingEffect.Set(temperature, force, Texture.Default, null);

            force.Value = middleForce;
            _middleGraspingEffect.Set(temperature, force, Texture.Default, null);
        }


        /// <summary>
        /// Hide and show hand's render object during Grasping 
        /// </summary>
        /// <param name="showHands">The Show Hands<see cref="bool"/>.</param>
        private void SnapHand(bool showHands)
        {
            _handSkinnedMeshRenderer.enabled = showHands;
            _logoHandPanel.SetActive(showHands);
        }

        /// <summary>
        /// The TryGetTouchableObjectFromCollider.
        /// </summary>
        /// <param name="collider">The collider<see cref="Collider"/>.</param>
        /// <param name="touchableObject">The touchableObject<see cref="WeArtTouchableObject"/>.</param>
        /// <returns>The <see cref="bool"/>.</returns>
        private static bool TryGetTouchableObjectFromCollider(Collider collider, out WeArtTouchableObject touchableObject)
        {
            touchableObject = collider.gameObject.GetComponent<WeArtTouchableObject>();
            return touchableObject != null;
        }

        /// <summary>
        /// Try to get WeArtGraspPose script from current touchable object
        /// </summary>
        /// <param name="gameObject">Current grasped object</param>
        /// <param name="customPoses">Output of this result</param>
        /// <returns></returns>
        private static bool TryGetCustomPosesFromTouchable(GameObject gameObject, out WeArtGraspPose customPoses)
        {
            customPoses = gameObject.GetComponent<WeArtGraspPose>();
            return customPoses != null;
        }

        /// <summary>
        /// The OnTriggerEnter.
        /// </summary>
        /// <param name="other">The other<see cref="Collider"/>.</param>
        private void OnTriggerEnter(Collider other)
        {
            if (TryGetTouchableObjectFromCollider(other, out var touchableObject))
            {
                if (touchableObject.IsGraspable && _graspingState == GraspingState.Released)
                {
                    _touchableObject = touchableObject;
                }
            }
        }

        /*
        private void OnTriggerStay(Collider other)
        {
            if (TryGetTouchableObjectFromCollider(other, out var touchableObject))
            {
                if (touchableObject.IsGraspable && _graspingState == GraspingState.Grabbed)
                {
                    _touchableObject = touchableObject;
                }
            }
        }
        */

        /// <summary>
        /// The OnTriggerExit.
        /// </summary>
        /// <param name="other">The other<see cref="Collider"/>.</param>
        private void OnTriggerExit(Collider other)
        {
            if (_touchableObject != null)
            {
                if (_graspingState == GraspingState.Released && _touchableObject.CompareInstanceID(other.gameObject.GetInstanceID()))
                {
                    _touchableObject = null;
                }
            }
        }

        #endregion

        #region Coroutines

        /// <summary>
        /// Interpolate the finger pose during grasp when using WeArtGraspPose.cs
        /// </summary>
        /// <param name="finger"></param>
        /// <param name="inputIndex"></param>
        /// <param name="closure"></param>
        /// <param name="lerpTime"></param>
        /// <returns></returns>
        private IEnumerator LerpPoses(AnimationLayerMixerPlayable finger, int inputIndex, float closure, float lerpTime)
        {
            float t = 0f;
            float to = finger.GetInputWeight(inputIndex);

            while (t < lerpTime)
            {
                float lerp;
                lerp = Mathf.Lerp(to, closure, t / lerpTime);
                t += Time.deltaTime;

                finger.SetInputWeight(inputIndex, lerp);
                yield return null;
            }
        }
        #endregion
    }
}