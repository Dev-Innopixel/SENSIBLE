using System;
using UnityEngine;

namespace WeArt.Components
{
    /// <summary>
    /// This component can be used to follow a specified spatially tracked transform.
    /// Add it to the root object of your avatar virtual hand. Make sure to specify
    /// the right spatial offset between the tracked object and the WeArt device.
    /// </summary>
    public class WeArtDeviceTrackingObject : MonoBehaviour
    {
        /// <summary>
        /// Possible tracking follow behaviours
        /// </summary>
        public enum TrackingUpdateMethod
        {
            TransformUpdate,
            TransformLateUpdate,
            RigidbodyUpdate,
        }

        [SerializeField]
        internal TrackingUpdateMethod _updateMethod;

        [SerializeField]
        internal Transform _trackingSource;

        [SerializeField]
        internal Vector3 _positionOffset;

        [SerializeField]
        internal Vector3 _rotationOffset;


        [NonSerialized]
        internal Rigidbody _rigidBody;


        /// <summary>
        /// The method to use in order to update the position and the rotation of this device
        /// </summary>
        public TrackingUpdateMethod UpdateMethod
        {
            get => _updateMethod;
            set => _updateMethod = value;
        }

        /// <summary>
        /// The transform attached to the tracked device object
        /// </summary>
        public Transform TrackingSource
        {
            get => _trackingSource;
            set => _trackingSource = value;
        }

        /// <summary>
        /// The position offset between this device and the tracked one
        /// </summary>
        public Vector3 PositionOffset
        {
            get => _positionOffset;
            set => _positionOffset = value;
        }

        /// <summary>
        /// The rotation offset between this device and the tracked one
        /// </summary>
        public Vector3 RotationOffset
        {
            get => _rotationOffset;
            set => _rotationOffset = value;
        }


        private void Update()
        {
            if (UpdateMethod == TrackingUpdateMethod.TransformUpdate)
                UpdateTransform();
        }

        private void LateUpdate()
        {
            if (UpdateMethod == TrackingUpdateMethod.TransformLateUpdate)
                UpdateTransform();
        }

        private void FixedUpdate()
        {
            if (UpdateMethod == TrackingUpdateMethod.RigidbodyUpdate)
                UpdateRigidbody();
        }

        private void UpdateTransform()
        {
            if (TrackingSource == null)
                return;

            transform.position = TrackingSource.TransformPoint(_positionOffset);
            transform.eulerAngles = (TrackingSource.rotation * Quaternion.Euler(_rotationOffset)).eulerAngles;
        }

        private void UpdateRigidbody()
        {
            if (TrackingSource == null)
                return;

            if (_rigidBody == null)
            {
                _rigidBody = GetComponent<Rigidbody>();
                if (_rigidBody == null)
                {
                    Debug.LogWarning($"Cannot use {nameof(TrackingUpdateMethod.RigidbodyUpdate)} method without a {nameof(Rigidbody)}");
                    return;
                }
            }

            _rigidBody.position = TrackingSource.TransformPoint(_positionOffset);
            _rigidBody.rotation = TrackingSource.rotation * Quaternion.Euler(_rotationOffset);
        }
    }
}