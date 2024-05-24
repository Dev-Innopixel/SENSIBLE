using System.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using WeArt.Core;
using Texture = WeArt.Core.Texture;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom property drawer for properties of type <see cref="Texture"/>.
    /// </summary>
    [CustomPropertyDrawer(typeof(Texture))]
    public class TexturePropertyDrawer : WeArtPropertyDrawer
    {

        bool _isInsidefBounds = true;

        // The gradient indicating the possible slider's dragger colors
        private static readonly string[] _velocitySlidersLabels = new string[]
        {
            "Vx", "Vy", "Vz"
        };

        public override VisualElement CreatePropertyGUI(SerializedProperty property)
        {

            // Sub-properties
            var activeProp = property.FindPropertyRelative(nameof(Texture._active));
            var textureTypeProp = property.FindPropertyRelative(nameof(Texture._textureType));
            /*
            var velocitiesProps = new SerializedProperty[]
            {
                property.FindPropertyRelative(nameof(Texture._vx)),
                property.FindPropertyRelative(nameof(Texture._vy)),
                property.FindPropertyRelative(nameof(Texture._vz)),
            };
            */

            // Values getters and setters
            TextureType getTextureType() => (TextureType)textureTypeProp.intValue + WeArtConstants.minTextureIndex;
            void setTextureType(TextureType type) => textureTypeProp.intValue = (int)type - WeArtConstants.minTextureIndex;

            // Property container
            var container = new VisualElement();
            container.AddToClassList("propertyRow");
            container.AddToClassList("toggled");

            // Toggle
            var toggle = new Toggle();
            toggle.SetValueWithoutNotify(activeProp.boolValue);
            toggle.BindProperty(activeProp);
            toggle.RegisterCallback<ChangeEvent<bool>>(evt => sendChange(evt.newValue, null, null));
            container.Add(toggle);

            // Label
            var label = new Label();
            if (_isInsidefBounds)
            {
                 label = new Label(property.displayName);
            }else
            {
                 label = new Label(((int)getTextureType()).ToString());
            }

            label.AddToClassList("unity-base-field__label");
            label.AddToClassList("unity-property-field__label");
            container.Add(label);

            // Values
            var valuesContainer = new VisualElement();
            valuesContainer.SetEnabled(toggle.value);
            toggle.RegisterCallback<ChangeEvent<bool>>(evt => valuesContainer.SetEnabled(evt.newValue));
            valuesContainer.AddToClassList("horizontal");
            valuesContainer.AddToClassList("unity-property-field");
            {
                // Texture image
                var image = new Image();
                image.AddToClassList("texture-preview");
                image.image = GetTexturePreview(getTextureType());
                valuesContainer.Add(image);

                var innerValuesContainer = new VisualElement();
                innerValuesContainer.AddToClassList("unity-property-field");

                // Texture type
                var textureTypeField = new PropertyField(textureTypeProp)
                {
                    label = string.Empty
                };
                textureTypeField.userData = getTextureType();
                textureTypeField.RegisterCallback<ChangeEvent<string>>(evt =>
                {
                    var previousValue = (TextureType)textureTypeField.userData;
                    var newValue = getTextureType();
                    textureTypeField.userData = newValue;

                    setTextureType(previousValue);
                    sendChange(null, newValue, null);

                    image.image = GetTexturePreview(newValue);
                });
                innerValuesContainer.Add(textureTypeField);

                /*
                // Velocity foldout
                var velocityFoldout = new Foldout
                {
                    text = "Velocity",
                    value = false
                };
                innerValuesContainer.Add(velocityFoldout);
                */

                /*
                // Velocity sliders
                var velocitySliders = new Slider[velocitiesProps.Length];
                for (int i = 0; i < velocitySliders.Length; i++)
                {
                    var velocitySlider = new SliderWithInputField(WeArtConstants.minTextureVelocity, WeArtConstants.maxTextureVelocity);
                    velocitySlider.SetValueWithoutNotify(velocitiesProps[i].floatValue);
                    velocitySlider.BindProperty(velocitiesProps[i]);

                    // Slider label
                    Label sliderLabel = new Label(_velocitySlidersLabels[i]);
                    velocitySlider.Insert(0, sliderLabel);

                    // Changes
                    velocitySlider.RegisterCallback<ChangeEvent<float>>(evt =>
                        sendChange(null, null, velocitySliders.Select(s => s.value).ToArray())
                    );

                    velocitySliders[i] = velocitySlider;
                    velocityFoldout.Add(velocitySlider);
                }
                */
                valuesContainer.Add(innerValuesContainer);
            }

            container.Add(valuesContainer);

            return container;


            // Local function that creates and send a ChangeEvent<Texture> event
            void sendChange(bool? active, TextureType? type, float[] velocity)
            { 

                var previousValue = new Texture()
                {
                    Active = activeProp.boolValue,
                    TextureType = getTextureType()
                    /*
                    VelocityX = velocitiesProps[0].floatValue,
                    VelocityY = velocitiesProps[1].floatValue,
                    VelocityZ = velocitiesProps[2].floatValue
                    */
                };

                var newValue = new Texture()
                {
                    Active = active ?? previousValue.Active,
                    TextureType = type ?? previousValue.TextureType,
                    Velocity = velocity != null ? velocity[2] : previousValue.Velocity
                };

                if (!previousValue.Equals(newValue))
                {
                    using (var tempChangedEvt = ChangeEvent<Texture>.GetPooled(previousValue, newValue))
                    {
                        tempChangedEvt.target = container;
                        container.SendEvent(tempChangedEvt);
                    }
                }
            }
        }

        private static UnityEngine.Texture GetTexturePreview(TextureType textureType)
        {
            return Resources.Load<UnityEngine.Texture>($"Textures/{(int)textureType}");
        }
    }
}