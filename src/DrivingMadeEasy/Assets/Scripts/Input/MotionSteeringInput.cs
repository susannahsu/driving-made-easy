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

        [Tooltip("How quickly the steering value chases the raw input. Higher = snappier.")]
        [Range(1f, 30f)] public float responsiveness = 12f;

        [Header("Return-to-center assist")]
        [Tooltip("Beginner aid: pulls steering toward 0 when the phone is near level.")]
        public bool returnToCenterAssist = true;
        [Range(0f, 5f)] public float assistStrength = 1.5f;

        [Header("Throttle / brake (M0 uses keyboard in editor, touch on device)")]
        [Tooltip("On-device: bottom-right of the screen is gas, bottom-left is brake.")]
        public bool useTouchPedals = true;

        public float Steering { get; private set; }
        public float Throttle { get; private set; }
        public float Brake { get; private set; }
        public bool Reverse { get; private set; }

        // Calibration: the device attitude captured as "straight ahead".
        private Quaternion _neutralAttitude = Quaternion.identity;
        private bool _gyroAvailable;

        private void Awake()
        {
            _gyroAvailable = SystemInfo.supportsGyroscope;
            if (_gyroAvailable)
            {
                UnityEngine.Input.gyro.enabled = true;
            }
            Calibrate();
        }

        /// <summary>Capture the current device pose as neutral (0° steering).</summary>
        public void Calibrate()
        {
            if (_gyroAvailable)
            {
                _neutralAttitude = UnityEngine.Input.gyro.attitude;
            }
        }

        private void Update()
        {
            float targetSteer = _gyroAvailable ? ReadTiltSteering() : ReadKeyboardSteering();

            // Smoothly chase the target so raw sensor noise / key taps don't feel twitchy.
            Steering = Mathf.MoveTowards(
                Steering, targetSteer, responsiveness * Time.deltaTime);
            Steering = Mathf.Clamp(Steering, -1f, 1f);

            ReadPedals();
        }

        private float ReadTiltSteering()
        {
            // Roll of the device relative to the calibrated neutral pose.
            Quaternion delta = Quaternion.Inverse(_neutralAttitude) * UnityEngine.Input.gyro.attitude;
            float rollDeg = NormalizeAngle(delta.eulerAngles.z);

            // Dead zone around center.
            if (Mathf.Abs(rollDeg) <= deadZoneDegrees)
            {
                return returnToCenterAssist ? 0f : Steering * (1f - assistStrength * Time.deltaTime);
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

        private float ReadKeyboardSteering()
        {
            // Editor / desktop fallback so steering can be iterated without a phone.
            return UnityEngine.Input.GetAxisRaw("Horizontal");
        }

        private void ReadPedals()
        {
            if (!_gyroAvailable || !useTouchPedals)
            {
                // Editor / desktop: W or Up = gas, S or Down = brake, R held = reverse.
                float v = UnityEngine.Input.GetAxisRaw("Vertical");
                Throttle = Mathf.Max(0f, v);
                Brake = Mathf.Max(0f, -v);
                Reverse = UnityEngine.Input.GetKey(KeyCode.R);
                return;
            }

            // On device: bottom-right half of the screen = gas, bottom-left half = brake,
            // so the thumbs reach them while the phone is held wheel-style. M0 keeps this
            // deliberately simple (digital); analog pressure is a later milestone.
            float gas = 0f, brake = 0f;
            foreach (Touch t in UnityEngine.Input.touches)
            {
                if (t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled) continue;
                if (t.position.y > Screen.height * 0.5f) continue; // only bottom band acts as pedals
                if (t.position.x > Screen.width * 0.5f) gas = 1f; else brake = 1f;
            }
            Throttle = gas;
            Brake = brake;
            Reverse = false; // reverse handled by an on-screen toggle in a later milestone
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
