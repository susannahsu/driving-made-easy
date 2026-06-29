using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A posted-speed-limit stretch. Unlike the stop sign (a single moment), this rule is
    /// evaluated continuously: relevance fires on entry, the car's speed is watched the
    /// whole way through, and the encounter passes only if you never exceeded the limit
    /// (plus a small grace). Same teach -> fade -> enforce treatment via the Coach.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class SpeedZone : MonoBehaviour
    {
        public string ruleId = "speed_limit";
        public CoachRuntime coach;
        public int limitMph = 15;

        [Tooltip("Grace above the limit (mph) before it counts as speeding.")]
        public float toleranceMph = 2f;

        private bool _armed;
        private bool _exceeded;

        private float LimitMps => limitMph / 2.2369f;
        private float ToleranceMps => toleranceMph / 2.2369f;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _exceeded = false;
            coach.CurrentSpeedLimitMph = limitMph;
            coach.NotifyRelevant(ruleId);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (Mathf.Abs(car.SpeedMetersPerSecond) > LimitMps + ToleranceMps) _exceeded = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;
            coach.CurrentSpeedLimitMph = 0;
            coach.NotifyEvaluated(ruleId, passed: !_exceeded);
        }
    }
}
