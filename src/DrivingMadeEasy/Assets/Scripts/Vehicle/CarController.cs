using System;
using UnityEngine;
using DrivingMadeEasy.Input;

namespace DrivingMadeEasy.Vehicle
{
    /// <summary>
    /// A WheelCollider-based car tuned to be <b>believable but forgiving</b>
    /// (SPEC §12 decision): it has real momentum, weight transfer and realistic
    /// braking distances, but high grip and an anti-spin stabilizer keep it from
    /// spinning out or behaving punishingly for a returning, anxious driver.
    ///
    /// Driver intent comes from any <see cref="IDriverInput"/> — phone tilt on device,
    /// keyboard in the editor — so the car is decoupled from the control scheme.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarController : MonoBehaviour
    {
        [Serializable]
        public class Axle
        {
            public WheelCollider leftWheel;
            public WheelCollider rightWheel;
            public Transform leftMesh;
            public Transform rightMesh;
            public bool motor;    // does this axle apply drive torque?
            public bool steering; // does this axle steer?
        }

        [Header("Wiring")]
        public Axle[] axles;
        [Tooltip("Anything implementing IDriverInput. If null, found on this GameObject.")]
        public MonoBehaviour driverInputSource;

        [Header("Performance (forgiving)")]
        public float maxMotorTorque = 900f;
        public float maxBrakeTorque = 2600f;
        public float maxSteerAngle = 32f;
        [Tooltip("Hard speed cap (m/s) so M0 stays controllable. 13.4 m/s ≈ 30 mph.")]
        public float maxSpeed = 13.4f;

        [Header("Stability assists")]
        [Tooltip("Counter-steer torque that resists spinning out. 0 = none.")]
        [Range(0f, 1f)] public float antiSpin = 0.6f;
        [Tooltip("Extra downforce-like grip so the car stays planted.")]
        public float gripAssist = 0.5f;

        private Rigidbody _rb;
        private IDriverInput _input;

        /// <summary>Current forward speed in m/s (negative when reversing).</summary>
        public float SpeedMetersPerSecond { get; private set; }
        public float SpeedMph => SpeedMetersPerSecond * 2.2369f;
        public float SteeringValue => _input?.Steering ?? 0f;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            // Lower center of mass = much less likely to tip / feel tippy.
            _rb.centerOfMass += Vector3.down * 0.5f;

            _input = driverInputSource as IDriverInput
                     ?? GetComponent<IDriverInput>();
            if (_input == null)
            {
                Debug.LogError("CarController has no IDriverInput source assigned.");
            }
        }

        private void FixedUpdate()
        {
            if (_input == null) return;

            SpeedMetersPerSecond = Vector3.Dot(_rb.velocity, transform.forward);
            float steer = _input.Steering * maxSteerAngle;

            float drive = ResolveDriveTorque();
            float brake = _input.Brake * maxBrakeTorque;

            foreach (var axle in axles)
            {
                if (axle.steering)
                {
                    axle.leftWheel.steerAngle = steer;
                    axle.rightWheel.steerAngle = steer;
                }
                if (axle.motor)
                {
                    axle.leftWheel.motorTorque = drive;
                    axle.rightWheel.motorTorque = drive;
                }
                axle.leftWheel.brakeTorque = brake;
                axle.rightWheel.brakeTorque = brake;

                SyncMesh(axle.leftWheel, axle.leftMesh);
                SyncMesh(axle.rightWheel, axle.rightMesh);
            }

            ApplyStabilityAssists();
        }

        private float ResolveDriveTorque()
        {
            bool wantsReverse = _input.Reverse;
            float throttle = _input.Throttle;

            // Respect the speed cap that keeps M0 approachable.
            if (!wantsReverse && SpeedMetersPerSecond >= maxSpeed) throttle = 0f;
            if (wantsReverse && SpeedMetersPerSecond <= -maxSpeed * 0.4f) throttle = 0f;

            float dir = wantsReverse ? -1f : 1f;
            return throttle * maxMotorTorque * dir;
        }

        private void ApplyStabilityAssists()
        {
            // Anti-spin: damp yaw that isn't justified by the steering input, so the back
            // end doesn't step out and frighten the driver.
            if (antiSpin > 0f)
            {
                float yaw = _rb.angularVelocity.y;
                float intendedYaw = _input.Steering * 1.2f; // rough target yaw rate
                float correction = (intendedYaw - yaw) * antiSpin;
                _rb.AddTorque(Vector3.up * correction, ForceMode.Acceleration);
            }

            // Grip assist: a mild downward push that scales with speed keeps the tires
            // planted (cheap stand-in for proper aero/grip tuning in M0).
            if (gripAssist > 0f)
            {
                float push = gripAssist * Mathf.Abs(SpeedMetersPerSecond);
                _rb.AddForce(-transform.up * push, ForceMode.Acceleration);
            }
        }

        private static void SyncMesh(WheelCollider collider, Transform mesh)
        {
            if (mesh == null) return;
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            mesh.SetPositionAndRotation(pos, rot);
        }
    }
}
