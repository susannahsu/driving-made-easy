using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "stop on red" rule. If the light is red while you're in the approach zone,
    /// it fires the heads-up, watches whether you stopped, and on exit passes only if you
    /// came to a stop. If the light stayed green the whole way through, it's a no-event.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TrafficLightZone : MonoBehaviour
    {
        public string ruleId = "traffic_light";
        public CoachRuntime coach;
        public TrafficLight light;
        public float stopSpeedThreshold = 0.6f;

        private bool _armed;
        private bool _redWasActive;
        private float _minSpeedWhileRed;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _redWasActive = false;
            _minSpeedWhileRed = float.MaxValue;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null || light == null) return;
            if (!light.MustStop) return;

            if (!_redWasActive)
            {
                _redWasActive = true;
                coach.NotifyRelevant(ruleId);
            }
            _minSpeedWhileRed = Mathf.Min(_minSpeedWhileRed, Mathf.Abs(car.SpeedMetersPerSecond));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;

            if (!_redWasActive) return; // light was green — no encounter
            coach.NotifyEvaluated(ruleId, passed: _minSpeedWhileRed <= stopSpeedThreshold);
        }
    }
}
