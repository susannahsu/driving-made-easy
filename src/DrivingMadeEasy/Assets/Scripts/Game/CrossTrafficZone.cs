using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "yield to cross traffic" rule at an intersection. It becomes a real
    /// encounter only when a traffic car is actually in the intersection while you're on the
    /// approach (a fair chance to act): at that point it fires the heads-up, watches whether
    /// you slowed enough to let them through, and evaluates on exit — same teach -> fade ->
    /// enforce path as the other rules.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class CrossTrafficZone : MonoBehaviour
    {
        public string ruleId = "yield";
        public CoachRuntime coach;
        public TrafficCar[] traffic;

        [Tooltip("Centre of the intersection — traffic within conflictRadius of it counts as crossing.")]
        public Vector3 conflictCenter;
        public float conflictRadius = 9f;

        [Tooltip("Slow to at or below this (m/s) while a car is crossing to count as yielding.")]
        public float yieldSpeed = 2.5f;

        private bool _armed;
        private bool _conflictWasPresent;
        private float _minSpeedWhileConflict;

        private bool ConflictPresent()
        {
            if (traffic == null) return false;
            foreach (var t in traffic)
                if (t != null && t.IsNear(conflictCenter, conflictRadius)) return true;
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _conflictWasPresent = false;
            _minSpeedWhileConflict = float.MaxValue;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (!ConflictPresent()) return;

            if (!_conflictWasPresent)
            {
                _conflictWasPresent = true;
                coach.NotifyRelevant(ruleId); // heads-up the moment cross traffic is in play
            }
            _minSpeedWhileConflict = Mathf.Min(_minSpeedWhileConflict, Mathf.Abs(car.SpeedMetersPerSecond));
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;

            if (!_conflictWasPresent) return; // nothing was crossing — not an encounter
            coach.NotifyEvaluated(ruleId, passed: _minSpeedWhileConflict <= yieldSpeed);
        }
    }
}
