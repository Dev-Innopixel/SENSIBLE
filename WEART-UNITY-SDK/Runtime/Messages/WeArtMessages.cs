using System.Collections.Generic;
using System;
using WeArt.Core;
using static WeArt.Messages.WeArtMessageCustomSerializer;

namespace WeArt.Messages
{
    /// <summary>
    /// Interface for all messages sent or received on communicating with the middleware
    /// </summary>
    public interface IWeArtMessage { }

    /// <summary>
    /// Interface for all messages that need a custom serialization procedure
    /// (e.g. non-serializable fields, custom serialization based on type or fields)
    /// </summary>
    public interface IWeArtMessageCustomSerialization
    {
        string[] Serialize();

        bool Deserialize(string[] fields);
    }


    /// <summary>
    /// Message that requests the middleware to start and to turn on the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StartFromClient")]
    public class StartFromClientMessage : IWeArtMessage, IWeArtMessageCustomSerialization
    {
        public string SdkType = WeArtConstants.WEART_SDK_TYPE;
        public string SdkVersion = WeArtConstants.WEART_SDK_VERSION;
        public TrackingType TrackingType = TrackingType.WEART_HAND;

        public bool Deserialize(string[] fields)
        {
            // Default (deprecated)
            if (fields.Length == 1)
            {
                TrackingType = TrackingType.DEFAULT;
            }
            // Other (newer) types
            else
            {
                SdkType = fields[1];
                SdkVersion = fields[2];
                TrackingType = TrackingTypeExtension.Deserialize(fields[3]);
            }
            return true;
        }

        public string[] Serialize()
        {
            if (TrackingType == TrackingType.DEFAULT)
                return new string[] { "StartFromClient" };
            else
                return new string[] { "StartFromClient", SdkType, SdkVersion, TrackingType.Serialize() };
        }
    }


    /// <summary>
    /// Message that requests the middleware to stop and to turn off the hardware
    /// </summary>
    [WeArtMiddlewareMessageID("StopFromClient")]
    public class StopFromClientMessage : IWeArtMessage { }

    /// <summary>
    /// Message that requests the middleware to start the calibration
    /// </summary>
    [WeArtMiddlewareMessageID("StartCalibration")]
    public class StartCalibrationFromClientMessage : IWeArtMessage { };

    /// <summary>
    /// Message that requests the middleare to stop the calibration
    /// </summary>
    [WeArtMiddlewareMessageID("StopCalibration")]
    public class StopCalibrationFromClientMessage : IWeArtMessage { };


    /// <summary>
    /// Message received from the middleware upon closing it
    /// </summary>
    [WeArtMiddlewareMessageID("exit")]
    public class ExitMessage : IWeArtMessage { }


    /// <summary>
    /// Message received from the middleware upon disconnection
    /// </summary>
    [WeArtMiddlewareMessageID("disconnect")]
    public class DisconnectMessage : IWeArtMessage { }


    /// <summary>
    /// Message sent to the middleware to set the temperature of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("temperature")]
    public class SetTemperatureMessage : IWeArtMessage
    {
        public float Temperature;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the temperature actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopTemperature")]
    public class StopTemperatureMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to set the pressure force of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("force")]
    public class SetForceMessage : IWeArtMessage
    {
        public float[] Force;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the force actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopForce")]
    public class StopForceMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to set the haptic texture of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("texture")]
    public class SetTextureMessage : IWeArtMessage
    {
        public int TextureIndex;
        public float[] TextureVelocity;
        public float TextureVolume;
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message sent to the middleware to stop the haptic texture actuator of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("stopTexture")]
    public class StopTextureMessage : IWeArtMessage
    {
        public HandSide HandSide;
        public ActuationPoint ActuationPoint;
    }


    /// <summary>
    /// Message received from the middleware containing the closure amount of thimbles
    /// </summary>
    [WeArtMiddlewareMessageID("Tracking")]
    public class TrackingMessage : IWeArtMessage, IWeArtMessageCustomSerialization
    {
        public TrackingType TrackingType { get; set; }
        private readonly Dictionary<(HandSide, ActuationPoint), byte> Closures = new Dictionary<(HandSide, ActuationPoint), byte>();
        private readonly Dictionary<(HandSide, ActuationPoint), byte> Abductions = new Dictionary<(HandSide, ActuationPoint), byte>();

        /// <summary>
        /// Allows to get the closure value for agiven actuation point on a given hand.
        /// </summary>
        /// <param name="handSide">Hand from which to select the actuation point</param>
        /// <param name="actuationPoint">Actuation point from which to take the closure</param>
        /// <returns>the closure value of the given point and hand</returns>
        public Closure GetClosure(HandSide handSide, ActuationPoint actuationPoint)
        {
            float closureMaxValue = 255;
            if (Closures.ContainsKey((handSide, actuationPoint)))
                return new Closure { Value = Closures[(handSide, actuationPoint)] / closureMaxValue };
            return new Closure { Value = 0 };
        }

        /// <summary>
        /// Allows to get the abduction value for agiven actuation point on a given hand.
        /// The presence of this data depends on the type of tracking message received,
        /// which depends on the parameters sent to the middleware with the start command.
        /// In particular:
        /// * DEFAULT: No abduction values
        /// * CLAP_HAND: Abduction values for index and middle finger
        /// </summary>
        /// <param name="handSide">Hand from which to select the actuation point</param>
        /// <param name="actuationPoint">Actuation point from which to take the closure</param>
        /// <returns>the closure value of the given point and hand</returns>
        public Abduction GetAbduction(HandSide handSide, ActuationPoint actuationPoint)
        {
            float abductionMaxValue = 255;
            if (Abductions.ContainsKey((handSide, actuationPoint)))
                return new Abduction { Value = Abductions[(handSide, actuationPoint)] / abductionMaxValue };
            return new Abduction { Value = 0 };
        }

        public bool Deserialize(string[] fields)
        {
            try
            {
                TrackingType = TrackingTypeExtension.Deserialize(fields[1]);
                switch (TrackingType)
                {
                    case TrackingType.DEFAULT: DeserializeDefault(fields); break;
                    case TrackingType.WEART_HAND: DeserializeWeart(fields); break;
                }
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public string[] Serialize()
        {
            switch (TrackingType)
            {
                case TrackingType.DEFAULT: return SerializeDefault();
                case TrackingType.WEART_HAND: return SerializeWeart();
            }
            return new string[] { };
        }

        // Serialization/Deserialization utils

        private void DeserializeDefault(string[] fields)
        {
            Closures[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[1]);
            Closures[(HandSide.Right, ActuationPoint.Index)] = byte.Parse(fields[2]);
            Closures[(HandSide.Right, ActuationPoint.Middle)] = byte.Parse(fields[3]);
            Closures[(HandSide.Right, ActuationPoint.Palm)] = byte.Parse(fields[4]);
            Closures[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[5]);
            Closures[(HandSide.Left, ActuationPoint.Index)] = byte.Parse(fields[6]);
            Closures[(HandSide.Left, ActuationPoint.Middle)] = byte.Parse(fields[7]);
            Closures[(HandSide.Left, ActuationPoint.Palm)] = byte.Parse(fields[8]);
        }

        private string[] SerializeDefault()
        {
            string[] fields = new string[9];
            int i = 0;
            fields[i++] = "Tracking";
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Thumb)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Index)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Middle)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Palm)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Thumb)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Index)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Middle)).ToString();
            fields[i] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Palm)).ToString();
            return fields;
        }

        private void DeserializeWeart(string[] fields)
        {
            // Right Hand
            Closures[(HandSide.Right, ActuationPoint.Index)] = byte.Parse(fields[2]);
            Closures[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[3]);
            Abductions[(HandSide.Right, ActuationPoint.Thumb)] = byte.Parse(fields[4]);
            Closures[(HandSide.Right, ActuationPoint.Middle)] = byte.Parse(fields[5]);

            // Left Hand
            Closures[(HandSide.Left, ActuationPoint.Index)] = byte.Parse(fields[6]);
            Closures[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[7]);
            Abductions[(HandSide.Left, ActuationPoint.Thumb)] = byte.Parse(fields[8]);
            Closures[(HandSide.Left, ActuationPoint.Middle)] = byte.Parse(fields[9]);
        }

        private string[] SerializeWeart()
        {
            string[] fields = new string[10];
            int i = 0;
            fields[i++] = "Tracking";
            fields[i++] = TrackingType.Serialize();

            // Right Hand
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Index)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Thumb)).ToString();
            fields[i++] = Abductions.GetValueOrDefault((HandSide.Right, ActuationPoint.Thumb)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Right, ActuationPoint.Middle)).ToString();

            // Left Hand
            fields[i++] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Index)).ToString();
            fields[i++] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Thumb)).ToString();
            fields[i++] = Abductions.GetValueOrDefault((HandSide.Left, ActuationPoint.Thumb)).ToString();
            fields[i] = Closures.GetValueOrDefault((HandSide.Left, ActuationPoint.Middle)).ToString();

            return fields;
        }
    }

    // <summary>
    /// Message received from the middleware calibration state
    /// </summary>
    [WeArtMiddlewareMessageID("CalibrationStatus")]
    public class TrackingCalibrationStatus : IWeArtMessage
    {
        public int handSide;
        public int status;

        public int GetStatus() { return status; }
        public HandSide GetHandSide() { return (HandSide)handSide; }
    }

    /// <summary>
    /// Message received from the middleware calibration result
    /// </summary>
    [WeArtMiddlewareMessageID("CalibrationResult")]
    public class TrackingCalibrationResult : IWeArtMessage
    {
        public int handSide;
        public int result;

        public int GetResult() { return result; }
        public HandSide GetHandSide() { return (HandSide)handSide; }
    }
}