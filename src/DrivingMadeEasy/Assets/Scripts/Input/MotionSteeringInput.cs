using UnityEngine;

namespace DrivingMadeEasy.Input
{
    /// <summary>
    /// Turns the iPhone into a steering wheel using CoreMotion (exposed to Unity via the
    /// device gyro/attitude). The phone is held in landscape; its *roll* relative to a
    /// calibrated neutral pose becomes the steering angle.
    ///
    /// This is the most important script in M0 — if the steering doesn't feel good, the
    /// whole product premise fails (see SPEC §12 risks). Everything here exists to make
    /// the tilt feel controllable: calibration, a dead zone, a non-linear sensitivity
    /// curve (fine near center, quicker at the edges), smoothing, and optional
    /// return-to-center assist that mimics a real wheel's caster.
    ///
    /// In the Editor (no gyro) it falls back to A/D or arrow keys + W/S so steering feel
    /// and the rest of the game can be iterated on a Mac/PC before deploying to a phone.
    /// </summary>
    public class MotionSteeringInput : MonoBehaviour, IDriverInput
    {
        [Header("Steering feel")]
        [Tooltip("Roll angle (degrees) from neutral that corresponds to full lock.")]
        [Range(20f, 90f)] public float maxRollDegrees = 75f;

        [Tooltip("Roll within this many degrees of neutral reads as straight (anti-jitter).")]
        [Range(0f, 10f)] public float deadZoneDegrees = 2.5f;

        [Tooltip("1 = linear. >1 = gentler near center, sharper near full lock.")]
        [Range(1f, 3f)] public float sensitivityExponent = 1.7f;

        [Tooltip("Flip if tilting left steers right on your device.")]
        public bool invertSteering = false;

        [Tooltip("How quickly the steering value chases the raw input. Higher = snappier.")]
        [Range(1f, 30f)] public float responsiveness = 10f;

        [Header("Tilt steering (accelerometer — robust on device)")]
        [Tooltip("Use the accelerometer for tilt rather than gyro attitude (more reliable).")]
        public bool useAccelerometer = true;
        [Tooltip("Accelerometer axis that reads left/right tilt in landscape (0=x, 1=y, 2=z). " +
                 "On iPhone in landscape this is X (confirmed on-device); the diagnostic HUD " +
                 "shows which axis moves when you tilt.")]
        [Range(0, 2)] public int accelAxis = 0;
        [Tooltip("Tilt (in g) beyond neutral before steering begins.")]
        public float accelDeadZone = 0.06f;
        [Tooltip("Tilt (in g) from neutral that gives full lock. Larger = LESS sensitive.")]
        public float accelRange = 0.5f;

        [Header("Return-to-center assist")]
        [Tooltip("Beginner aid: pulls steering toward 0 when the phone is near level.")]
        public bool returnToCenterAssist = true;
        [Range(0f, 5f)] public float assistStrength = 1.5f;

        [Header("Throttle / brake (M0 uses keyboard in editor, touch on device)")]
        [Tooltip("On-device: bottom-right of the screen is gas, bottom-left is brake.")]
        public bool useTouchPedals = true;

        [Header("Desktop testing only (no gyro)")]
        [Tooltip("On a laptop with no motion sensor, steer by moving the mouse left/right " +
                 "across the Game view — continuous, closer to tilting a phone than A/D keys. " +
                 "On the actual iPhone this is ignored; real tilt steering takes over.")]
        public bool desktopMouseSteering = true;

        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public float Brake { get; private set; }
        public bool Reverse { get; private set; }
        public TurnSignal Signal { get; private set; }

        // Auto-cancel bookkeeping: once you've actually turned (steering swung past a
        // threshold) and then straightened out, the blinker switches itself off.
        private bool _steeredHard;

        // Calibration.
        private Quaternion _neutralAttitude = Quaternion.identity;
        private float _neutralAccel;
        private bool _gyroAvailable;

        // On-screen pedal state, driven by the HUD buttons.
        private float _uiThrottle, _uiBrake;
        private bool _uiReverse;

        /// Live accelerometer reading, for the diagnostic HUD.
        public Vector3 RawAccel => UnityEngine.Input.acceleration;
        public bool MotionActive => _gyroAvailable;

        private void Awake()
        {
            _gyroAvailable = SystemInfo.supportsGyroscope;
            if (_gyroAvailable)
            {
                UnityEngine.Input.gyro.enabled = true;
            }
            UnityEngine.Input.multiTouchEnabled = true;
            Calibrate();
            // Re-calibrate once the app has settled into landscape and the phone is being
            // held, so tilt steering is zeroed correctly even before RECENTER is tapped.
            Invoke(nameof(Calibrate), 1.0f);
        }

        /// <summary>Capture the current device pose as neutral (straight ahead).</summary>
        public void Calibrate()
        {
            if (_gyroAvailable)
            {
                _neutralAttitude = GyroToUnity(UnityEngine.Input.gyro.attitude);
                _neutralAccel = UnityEngine.Input.acceleration[Mathf.Clamp(accelAxis, 0, 2)];
            }
        }

        // On-screen buttons feed these.
        public void SetThrottle(float v) => _uiThrottle = Mathf.Clamp01(v);
        public void SetBrake(float v) => _uiBrake = Mathf.Clamp01(v);
        public void SetReverse(bool r) => _uiReverse = r;

        private void Update()
        {
            float targetSteer = _gyroAvailable ? ReadTiltSteering() : ReadDesktopSteering();

            // Smoothly chase the target so raw sensor noise / key taps don't feel twitchy.
            // Caster-like assist: chase faster when returning toward straight than when
            // turning away from it, so the "wheel" self-centers like a real one.
            float rate = responsiveness;
            bool returningToCenter = Mathf.Abs(targetSteer) < Mathf.Abs(Steering);
            if (returnToCenterAssist && returningToCenter)
            {
                rate *= 1f + assistStrength;
            }

            Steering = Mathf.MoveTowards(Steering, targetSteer, rate * Time.deltaTime);
            Steering = Mathf.Clamp(Steering, -1f, 1f);

            ReadPedals();
            ReadTurnSignal();
        }

        private void ReadTurnSignal()
        {
            // Desktop: Q toggles left, E toggles right (press again, or the opposite, to
            // cancel). On device this will become an edge swipe later.
            if (UnityEngine.Input.GetKeyDown(KeyCode.Q))
                Signal = Signal == TurnSignal.Left ? TurnSignal.None : TurnSignal.Left;
            if (UnityEngine.Input.GetKeyDown(KeyCode.E))
                Signal = Signal == TurnSignal.Right ? TurnSignal.None : TurnSignal.Right;

            // Auto-cancel like a real wheel: after a real turn (hard steer) you straighten
            // out, and the blinker clicks off.
            if (Mathf.Abs(Steering) > 0.45f) _steeredHard = true;
            if (_steeredHard && Mathf.Abs(Steering) < 0.1f)
            {
                Signal = TurnSignal.None;
                _steeredHard = false;
            }
        }

        private float ReadTiltSteering()
        {
            if (useAccelerometer)
            {
                // The accelerometer measures gravity; tilting the phone like a wheel changes
                // the chosen axis. Robust and easy to reason about, unlike gyro attitude.
                int axis = Mathf.Clamp(accelAxis, 0, 2);
                float accelDelta = UnityEngine.Input.acceleration[axis] - _neutralAccel;
                if (invertSteering) accelDelta = -accelDelta;

                if (Mathf.Abs(accelDelta) <= accelDeadZone) return 0f;
                float usable = Mathf.Max(0.01f, accelRange - accelDeadZone);
                float norm = Mathf.Clamp((Mathf.Abs(accelDelta) - accelDeadZone) / usable, 0f, 1f);
                return Mathf.Sign(accelDelta) * Mathf.Pow(norm, sensitivityExponent);
            }

            // Legacy gyro-attitude path (kept as a fallback).
            Quaternion attitude = GyroToUnity(UnityEngine.Input.gyro.attitude);
            Quaternion delta = Quaternion.Inverse(_neutralAttitude) * attitude;

            // Roll about the screen normal is what "tilting the wheel" produces in
            // landscape. NOTE: the exact axis/sign is device-orientation dependent and
            // MUST be validated on-device — flip `invertSteering` if left/right reverse.
            float rollDeg = NormalizeAngle(delta.eulerAngles.z);
            if (invertSteering) rollDeg = -rollDeg;

            // Dead zone around center → treat as perfectly straight.
            if (Mathf.Abs(rollDeg) <= deadZoneDegrees)
            {
                return 0f;
            }

            float signedBeyondDeadzone = Mathf.Sign(rollDeg) *
                                         (Mathf.Abs(rollDeg) - deadZoneDegrees);
            float usableRange = Mathf.Max(1f, maxRollDegrees - deadZoneDegrees);
            float normalized = Mathf.Clamp(signedBeyondDeadzone / usableRange, -1f, 1f);

            // Non-linear curve: fine control near center, quicker toward full lock.
            float shaped = Mathf.Sign(normalized) *
                           Mathf.Pow(Mathf.Abs(normalized), sensitivityExponent);
            return shaped;
        }

        private float ReadDesktopSteering()
        {
            // Keyboard always works as a fallback.
            float keyboard = UnityEngine.Input.GetAxisRaw("Horizontal");
            if (!desktopMouseSteering || Mathf.Abs(keyboard) > 0.01f)
            {
                return keyboard;
            }

            // Otherwise steer by mouse X across the Game view: an analog, continuous
            // stand-in for tilting the phone. Center of the view = straight ahead.
            float fromCenter = (UnityEngine.Input.mousePosition.x - Screen.width * 0.5f)
                               / (Screen.width * 0.5f);
            return Mathf.Clamp(fromCenter, -1f, 1f);
        }

        private void ReadPedals()
        {
            // Keyboard (editor) and the on-screen GAS/BRAKE buttons (device) both feed in,
            // so whichever is pressed wins.
            float kThrottle = 0f, kBrake = 0f;
            bool kReverse = false;
            if (!_gyroAvailable)
            {
                float v = UnityEngine.Input.GetAxisRaw("Vertical");
                kThrottle = Mathf.Max(0f, v);
                kBrake = Mathf.Max(0f, -v);
                kReverse = UnityEngine.Input.GetKey(KeyCode.R);
            }

            Throttle = Mathf.Clamp01(Mathf.Max(_uiThrottle, kThrottle));
            Brake = Mathf.Clamp01(Mathf.Max(_uiBrake, kBrake));
            Reverse = _uiReverse || kReverse;
        }

        /// <summary>
        /// Convert a gyro attitude quaternion (right-handed device frame) into Unity's
        /// left-handed world space. This is the well-known transform required to make
        /// Input.gyro usable; without it the axes are mirrored/rotated.
        /// </summary>
        private static Quaternion GyroToUnity(Quaternion q)
        {
            return new Quaternion(q.x, q.y, -q.z, -q.w);
        }

        /// <summary>Map [0,360) euler angle to [-180,180] so left/right are signed.</summary>
        private static float NormalizeAngle(float deg)
        {
            deg %= 360f;
            if (deg > 180f) deg -= 360f;
            return deg;
        }
    }
}
