using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtDeviceTrackingObject"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtDeviceTrackingObject), true), CanEditMultipleObjects]
    public class WeArtDeviceTrackingObjectEditor : WeArtComponentEditor
    {

        public Dictionary<string, (Vector3, Vector3)> _offsetPresets = new Dictionary<string, (Vector3, Vector3)>()
        {
           { "Custom", (
                Vector3.zero,
                Vector3.zero
            )},
            { "HTC Vive Left", (
                new Vector3(-0.01f, 0.04f, -0.085f),
                new Vector3(0f, -90f, -90f)
            )},
            { "HTC Vive Right", (
                new Vector3(0.01f, 0.04f, -0.085f),
                new Vector3(0f, -90f, -90f)
            )},
            { "Oculus Quest Right - Wrist", (
                new Vector3(0.08f, -0.08f, -0.06f),
                new Vector3(-59.6f, -15.6f, 88.6f)
            )},
            { "Oculus Quest Left - Wrist", (
                new Vector3(-0.06f, -0.05f, -0.06f),
                new Vector3(-121.1f, 177.4f, 88.6f)
            )},
            { "Oculus Quest Right - Palm", (
                new Vector3(0.07f, 0.01f, 0.01f),
                new Vector3(-64.3f, 5.2f,  91.9f)
            )},
            { "Oculus Quest Left - Palm", (
                new Vector3(-0.06f, -0.01f, -0.01f),
                new Vector3(-121.1f, 177.4f, 88.6f)
            )},
            { "Oculus Quest Right - Palm Twisted", (
                new Vector3(0.07f, -0.08f, 0.03f),
                new Vector3(160.5f, 191.9f, -91f)
            )},
            { "Oculus Quest Left - Palm Twisted", (
                new Vector3(-0.07f, -0.04f, -0.01f),
                new Vector3(15.3f, -1.24f, -73.3f)
            )},
        };


        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();
            editor.Bind(serializedObject);

            // Label
            {
                var header = new Label("Tracking");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Tracking source
            {
                var property = serializedObject.FindProperty(nameof(WeArtDeviceTrackingObject._trackingSource));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "The transform attached to the tracked device object"
                };
                editor.Add(propertyField);
            }

            // Update method
            {
                var property = serializedObject.FindProperty(nameof(WeArtDeviceTrackingObject._updateMethod));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "The method to use in order to update the position and the rotation of this device"
                };
                editor.Add(propertyField);
            }

            // Label
            {
                var header = new Label("Offset");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Position offset
            var posOffsetProp = serializedObject.FindProperty(nameof(WeArtDeviceTrackingObject._positionOffset));
            var posOffsetPropField = new PropertyField(posOffsetProp);
            editor.Add(posOffsetPropField);

            // Rotation offset
            var rotOffsetProp = serializedObject.FindProperty(nameof(WeArtDeviceTrackingObject._rotationOffset));
            var rotOffsetPropField = new PropertyField(rotOffsetProp);
            editor.Add(rotOffsetPropField);

            // Offset preset
            var presetsKeys = _offsetPresets.Keys.ToList();
            int selectedIndex = 0;
            for (int i = 0; i < presetsKeys.Count; i++)
            {
                var preset = _offsetPresets[presetsKeys[i]];
                if (Approximately(preset.Item1, posOffsetProp.vector3Value) && Approximately(preset.Item2, rotOffsetProp.vector3Value))
                    selectedIndex = i;
            }
            var presetSelector = new PopupField<string>("Offset presets", presetsKeys, selectedIndex);
            editor.Add(presetSelector);

            onPresetChange();
            presetSelector.RegisterCallback<ChangeEvent<string>>(evt => onPresetChange());

            void onPresetChange()
            {
                bool isCustom = presetSelector.index == 0;

                posOffsetPropField.SetEnabled(isCustom);
                rotOffsetPropField.SetEnabled(isCustom);

                if (!isCustom)
                {
                    var preset = _offsetPresets[presetSelector.value];
                    posOffsetProp.vector3Value = preset.Item1;
                    rotOffsetProp.vector3Value = preset.Item2;
                    serializedObject.ApplyModifiedProperties();
                }
            }


            return editor;
        }

        private static bool Approximately(Vector3 v1, Vector3 v2)
        {
            return Mathf.Approximately(v1.x, v2.x) &&
                Mathf.Approximately(v1.y, v2.y) &&
                Mathf.Approximately(v1.z, v2.z);
        }
    }
}