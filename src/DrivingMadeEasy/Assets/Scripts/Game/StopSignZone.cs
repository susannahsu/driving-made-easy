using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A trigger volume covering a stop sign's approach and stop line. It fires the rule's
    /// relevance when the car enters (so the Coach can give a heads-up while coaching),
    /// watches whether the car comes to a full stop while inside, and evaluates pass/fail
    /// when the car leaves. This is the first concrete "Rule relevance + evaluation" wiring
    /// from SPEC §8.2; later rules follow the same shape.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class StopSignZone : MonoBehaviour
    {
        public string ruleId = "stop_sign";
        public CoachRuntime coach;

        [Tooltip("Speed (m/s) at or below which the car counts as fully stopped.")]
        public float stopSpeedThreshold = 0.6f;

        private bool _armed;
        private bool _didStop;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _didStop = false;
            coach.NotifyRelevant(ruleId);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (Mathf.Abs(car.SpeedMetersPerSecond) <= stopSpeedThreshold) _didStop = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;
            coach.NotifyEvaluated(ruleId, passed: _didStop);
        }
    }
}
