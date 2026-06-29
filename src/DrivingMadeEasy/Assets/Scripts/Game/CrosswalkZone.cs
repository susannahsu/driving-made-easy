using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "yield to a pedestrian in a crosswalk" rule. It only becomes a real
    /// encounter when a pedestrian is actually on the crosswalk while the car is in the
    /// zone (a fair chance to act, SPEC §6.1): at that moment it fires the heads-up, then
    /// watches whether the car slowed enough to yield, and evaluates on exit.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CrosswalkZone : MonoBehaviour
    {
        public string ruleId = "pedestrian_crosswalk";
        public CoachRuntime coach;
        public Pedestrian pedestrian;

        [Tooltip("Slow to at or below this (m/s) while someone is crossing to count as yielding.")]
        public float yieldSpeed = 2.5f;

        private bool _armed;
        private bool _pedWasPresent;
        private float _minSpeedWhilePed;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _pedWasPresent = false;
            _minSpeedWhilePed = float.MaxValue;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;

            bool pedOn = pedestrian != null && pedestrian.IsOnCrosswalk;
            if (!pedOn) return;

            if (!_pedWasPresent)
            {
                _pedWasPresent = true;
                coach.NotifyRelevant(ruleId); // heads-up the moment a pedestrian steps in
            }
            _minSpeedWhilePed = Mathf.Min(_minSpeedWhilePed, Mathf.Abs(car.SpeedMetersPerSecond));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;

            if (!_pedWasPresent) return; // nobody was crossing — not an encounter
            coach.NotifyEvaluated(ruleId, passed: _minSpeedWhilePed <= yieldSpeed);
        }
    }
}
