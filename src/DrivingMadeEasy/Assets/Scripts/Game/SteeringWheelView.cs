using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Spins a cockpit steering-wheel mesh to match the car's steering, so the driver's
    /// view feels like actually holding a wheel. Attach to the wheel's pivot; it preserves
    /// the pivot's mounted tilt and rotates about the column axis (local Z).
    /// </summary>
    public class SteeringWheelView : MonoBehaviour
    {
        public CarController car;

        [Tooltip("Degrees the wheel turns at full steering lock.")]
        public float maxWheelDegrees = 120f;

        [Tooltip("How quickly the visual wheel chases the steering value.")]
        public float smooth = 12f;

        private Quaternion _base;
        private float _angle;

        private void Start() => _base = transform.localRotation;

        private void Update()
        {
            float steer = car != null ? car.SteeringValue : 0f;
            float target = -steer * maxWheelDegrees;
            _angle = Mathf.Lerp(_angle, target, Mathf.Clamp01(smooth * Time.deltaTime));
            transform.localRotation = _base * Quaternion.Euler(0f, 0f, _angle);
        }
    }
}
