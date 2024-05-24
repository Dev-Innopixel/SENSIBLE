using System.ComponentModel;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;
using WeArt.Core;
using WeArt.Utils;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtTouchableObjectEditor"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtTouchableObject), true), CanEditMultipleObjects]
    public class WeArtTouchableObjectEditor : WeArtComponentEditor
    {
        private WeArtTouchableObject Touchable => serializedObject.targetObject as WeArtTouchableObject;

        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();
            editor.Bind(serializedObject);

            // Label
            {
                var header = new Label("Haptic feeling");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Temperature
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._temperature));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a thermic effect on touching haptic objects"
                };
                propertyField.RegisterCallback<ChangeEvent<Temperature>>(evt =>
                {
                    Touchable._temperature = evt.previousValue;
                    Touchable.Temperature = evt.newValue;
                });
                editor.Add(propertyField);
            }

            // Stiffness
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._stiffness));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a pressure force effect on touching haptic objects"
                };
                propertyField.RegisterCallback<ChangeEvent<Force>>(evt =>
                {
                    Touchable._stiffness = evt.previousValue;
                    Touchable.Stiffness = evt.newValue;
                });
                editor.Add(propertyField);
            }

            // Texture
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._texture));

                var propertyField = new PropertyField(property)
                {
                    tooltip = "It will trigger a haptic texture effect on touching haptic objects"
                };

                propertyField.RegisterCallback<ChangeEvent<Texture>>(evt =>
                {
                    Touchable._texture = evt.previousValue;
                    Touchable.Texture = evt.newValue;
                    Touchable._texture.Volume = WeArtConstants.maxVolumeTexture;
                    Touchable._volumeTexture = WeArtConstants.maxVolumeTexture;

                });
                editor.Add(propertyField);
            }

            // Label Volume
            {
                var header = new Label("Texture behaviour");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Volume
            {
                var valueProp = serializedObject.FindProperty(nameof(WeArtTouchableObject._volumeTexture));

                var container = new VisualElement()
                {
                    tooltip = "Controls intensity of textures"
                };
                container.AddToClassList("propertyRow");

                // Label
                var label = new Label(ObjectNames.NicifyVariableName(nameof(WeArtTouchableObject.VolumeTexture)));
                label.AddToClassList("unity-base-field__label");
                label.AddToClassList("unity-property-field__label");
                container.Add(label);

                // Value slider
                var valueSlider = new SliderWithInputField(WeArtConstants.minVolumeTexture, WeArtConstants.maxVolumeTexture);
                valueSlider.SetValueWithoutNotify(valueProp.floatValue);
                valueSlider.BindProperty(valueProp);

                container.RegisterCallback<ChangeEvent<float>>(evt =>
                {
                    Touchable._volumeTexture = evt.previousValue;
                    Touchable.VolumeTexture = evt.newValue;
                });

                container.Add(valueSlider);
                editor.Add(container);
            }

            // Forced Velocity
            {
                var container= new VisualElement()
                {
                    tooltip = "Controls intensity of textures"
                };
                container.AddToClassList("propertyRow");

                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._texture._forcedVelocity));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Enable natural or forced texture velocity"
                };

                container.RegisterCallback<ChangeEvent<bool>>(evt =>
                {
                    Touchable._texture._forcedVelocity = evt.previousValue;
                    Touchable._texture.ForcedVelocity = evt.newValue;
                });

                container.Add(propertyField);

                editor.Add(container); 
            }

            /*
            // Label
            {
                var header = new Label("Physic behaviour");
                header.AddToClassList("header");
                editor.Add(header);
            }
            
            // Collision multiplier
            {
                var valueProp = serializedObject.FindProperty(nameof(WeArtTouchableObject._collisionMultiplier));

                var container = new VisualElement()
                {
                    tooltip = "Controls how much the collision speed affects the perceived stiffness and the texture velocity"
                };
                container.AddToClassList("propertyRow");

                // Label
                var label = new Label(ObjectNames.NicifyVariableName(nameof(WeArtTouchableObject.CollisionMultiplier)));
                label.AddToClassList("unity-base-field__label");
                label.AddToClassList("unity-property-field__label");
                container.Add(label);

                // Value slider
                var valueSlider = new SliderWithInputField(WeArtConstants.minCollisionMultiplier, WeArtConstants.maxCollisionMultiplier);
                valueSlider.SetValueWithoutNotify(valueProp.floatValue);
                valueSlider.BindProperty(valueProp);
                container.Add(valueSlider);
                editor.Add(container);
            }
            */

            // Label
            {
                var header = new Label("Interaction");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Graspable
            {
                var property = serializedObject.FindProperty(nameof(WeArtTouchableObject._graspable));
                var propertyField = new PropertyField(property)
                {
                    tooltip = "Enable interaction with WEART Hands to manage grasping"
                };
              
                editor.Add(propertyField);
            }


            if (EditorApplication.isPlaying)
            {
                // Label
                {
                    var header = new Label("Runtime");
                    header.AddToClassList("header");
                    editor.Add(header);
                }

                // Affected haptic objectss
                {
                    var affectedHapticsContainer = new VisualElement();
                    affectedHapticsContainer.AddToClassList("propertyRows");
                    editor.Add(affectedHapticsContainer);

                    updateHaptics();
                    Touchable.OnAffectedHapticObjectsUpdate += updateHaptics;

                    void updateHaptics()
                    {
                        const string hapticsLabel = "Affected haptic objects";
                        affectedHapticsContainer.Clear();
                        if (Touchable.AffectedHapticObjects.Count > 0)
                        {
                            foreach (var haptic in Touchable.AffectedHapticObjects)
                            {
                                string label = affectedHapticsContainer.childCount == 0 ? hapticsLabel : " ";
                                var hapticField = createHapticField(label, haptic);
                                affectedHapticsContainer.Add(hapticField);
                            }
                        }
                        else
                        {
                            var objectField = createHapticField(hapticsLabel, null);
                            affectedHapticsContainer.Add(objectField);
                        }
                    }

                    VisualElement createHapticField(string label, WeArtHapticObject haptic)
                    {
                        var objectField = new ObjectField(label);
                        objectField.AddToClassList("propertyRow");
                        objectField.objectType = typeof(WeArtHapticObject);
                        objectField.Q(className: ObjectField.inputUssClassName).SetEnabled(false);
                        objectField.SetValueWithoutNotify(haptic);
                        return objectField;
                    }
                }
            }

            return editor;
        }
    }
}