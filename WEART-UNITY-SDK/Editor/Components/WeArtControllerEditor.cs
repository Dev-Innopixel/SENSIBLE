using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;
using WeArt.Components;

namespace WeArt.UnityEditor
{
    /// <summary>
    /// A custom inspector for components of type <see cref="WeArtController"/>.
    /// </summary>
    [CustomEditor(typeof(WeArtController), true), CanEditMultipleObjects]
    public class WeArtControllerEditor : WeArtComponentEditor
    {
        private Button _startButton, _stopButton;
        private Button _startCalibration, _stopCalibration;

        private WeArtController Controller => serializedObject.targetObject as WeArtController;

        public override VisualElement CreateInspectorGUI()
        {
            var editor = base.CreateInspectorGUI();

            // Label
            {
                var header = new Label("Settings");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Client port
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._clientPort));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "The local network port used by the middleware"
                });
            }

            // Start automatically
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._startAutomatically));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the middleware will be started automatically on Start()"
                });
            }

            // Start calibration automatically after run Middleware
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._startCalibrationAutomatically));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, the middleware will start the finger tracking calibration process after be started"
                });
            }

            // Debug messages
            {
                var property = serializedObject.FindProperty(nameof(WeArtController._debugMessages));
                editor.Add(new PropertyField(property)
                {
                    tooltip = "If true, any messages send or received from/to the middleware will be logged in the console"
                });
            }


            // Label
            {
                var header = new Label("Middleware control");
                header.AddToClassList("header");
                editor.Add(header);
            }

            // Start button
            {
                _startButton = new Button
                {
                    text = "Start middleware",
                    tooltip = "Click here to start the middleware and turn the hardware on"
                };
                _startButton.clicked += () => SetMiddleware(true);
                _startButton.Display(!Controller.Client.IsConnected);
                editor.Add(_startButton);
            }

            // Stop button
            {
                _stopButton = new Button
                {
                    text = "Stop middleware",
                    tooltip = "Click here to stop the middleware and turn the hardware off"
                };
                _stopButton.clicked += () => SetMiddleware(false);
                _stopButton.Display(Controller.Client.IsConnected);
                editor.Add(_stopButton);
            }

            // Start calibration
            {
                _startCalibration = new Button
                {
                    text = "Start Calibration",
                    tooltip = "Click here to start the finger tracking calibration process"
                };
                _startCalibration.clicked += () => SetCalibration(true);
                _startCalibration.Display(!Controller.Client.IsConnected);
                editor.Add(_startCalibration);
            }

            // Stop Calibration
            {
                _stopCalibration = new Button
                {
                    text = "Stop Calibration",
                    tooltip = "Click here to stop the finger tracking calibration process"
                };
                _stopCalibration.clicked += () => SetCalibration(false);
                _stopCalibration.Display(Controller.Client.IsConnected);
                editor.Add(_stopCalibration);
            }

            return editor;
        }

        private void SetMiddleware(bool on)
        {
            _startButton.SetEnabled(false);
            _stopButton.SetEnabled(false);
            if (on)
            {
                Controller.Client.OnConnectionStatusChanged += onClientConnectionChanged;
                Controller.Client.Start();
                EditorApplication.playModeStateChanged += onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += onScriptsReloading;
            }
            else
            {
                Controller.Client.OnConnectionStatusChanged += onClientConnectionChanged;
                Controller.Client.Stop();
                EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= onScriptsReloading;
            }
            _startButton.Display(!on);
            _stopButton.Display(on);

            void onClientConnectionChanged(bool _)
            {
                _startButton.SetEnabled(true);
                _stopButton.SetEnabled(true);
                Controller.Client.OnConnectionStatusChanged -= onClientConnectionChanged;
            }

            void onPlayModeStateChanged(PlayModeStateChange change)
            {
                if (change == PlayModeStateChange.ExitingEditMode)
                    SetMiddleware(false);
            }

            void onScriptsReloading() => SetMiddleware(false);
        }

        private void SetCalibration(bool on)
        {
            _startCalibration.SetEnabled(true);
            _stopCalibration.SetEnabled(true);

            if (on)
            {
                Controller.Client.StartCalibration();
                EditorApplication.playModeStateChanged += onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload += onScriptsReloading;
            }
            else
            {
                Controller.Client.StopCalibration();
                EditorApplication.playModeStateChanged -= onPlayModeStateChanged;
                AssemblyReloadEvents.beforeAssemblyReload -= onScriptsReloading;
            }

            _startCalibration.Display(!on);
            _stopCalibration.Display(on);

           
            void onPlayModeStateChanged(PlayModeStateChange change)
            {
                if (change == PlayModeStateChange.ExitingEditMode)
                    SetCalibration(false);
            }

            void onScriptsReloading() => SetCalibration(false);
        }
    }

    public static class LocalExtension
    {
        public static void Display(this VisualElement element, bool show)
        {
            element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}