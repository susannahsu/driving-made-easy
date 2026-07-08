using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Marks a pedestrian or car as something the player can hit. Its trigger volume
    /// registers a collision with the Coach when the player's (moving) car overlaps it —
    /// no physics response, just the consequence. A short cooldown avoids double-counting.
    /// </summary>
    public class Hazard : MonoBehaviour
    {
        public CoachRuntime coach;
        public string ruleId = "collision";
        public float cooldown = 3f;

        private float _nextAllowed;

        private void OnTriggerEnter(Collider other) => Register(other);
        private void OnTriggerStay(Collider other) => Register(other);

        private void Register(Collider other)
        {
            if (coach == null || Time.time < _nextAllowed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (Mathf.Abs(car.SpeedMetersPerSecond) < 0.5f) return; // must be moving to "hit"
            _nextAllowed = Time.time + cooldown;
            coach.NotifyEvaluated(ruleId, passed: false);
        }
    }
}
