namespace DrivingMadeEasy.Input
{
    /// <summary>
    /// Abstraction over "where driver intent comes from" so the vehicle never cares
    /// whether it is being driven by phone tilt, on-screen pedals, keyboard (editor),
    /// or — later — a scripted scenario in a headless test.
    /// </summary>
    public interface IDriverInput
    {
        /// <summary>Steering intent in [-1, 1]. -1 = full left, +1 = full right.</summary>
        float Steering { get; }

        /// <summary>Throttle in [0, 1].</summary>
        float Throttle { get; }

        /// <summary>Brake in [0, 1].</summary>
        float Brake { get; }

        /// <summary>True while the player is requesting reverse gear.</summary>
        bool Reverse { get; }

        /// <summary>
        /// Re-zero the steering to the device's current pose ("hands at 9-and-3").
        /// No-op for input sources that don't need calibration (keyboard, scripted).
        /// </summary>
        void Calibrate();
    }
}
