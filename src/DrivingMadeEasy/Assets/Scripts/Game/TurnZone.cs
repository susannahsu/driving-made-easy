using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "signal before you turn" rule at an intersection. It fires the heads-up
    /// on entry, watches your heading and blinker while you're in the junction, and on exit
    /// decides whether you actually turned. Only a real turn counts as an encounter; if you
    /// go straight through, it's a no-event. You pass if the matching blinker was on.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class TurnZone : MonoBehaviour
    {
        public string ruleId = "turn_signal";
        public CoachRuntime coach;

        [Tooltip("Heading change (degrees) that counts as a turn rather than going straight.")]
        public float turnThresholdDegrees = 35f;

        private bool _armed;
        private float _entryYaw;
        private bool _usedLeft;
        private bool _usedRight;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _entryYaw = car.transform.eulerAngles.y;
            _usedLeft = _usedRight = false;
            coach.NotifyRelevant(ruleId);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (car.Signal == TurnSignal.Left) _usedLeft = true;
            if (car.Signal == TurnSignal.Right) _usedRight = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;

            // Positive yaw delta = turned right, negative = turned left (Unity is +y CW).
            float delta = Mathf.DeltaAngle(_entryYaw, car.transform.eulerAngles.y);
            if (Mathf.Abs(delta) < turnThresholdDegrees) return; // went straight — no encounter

            bool turnedRight = delta > 0f;
            bool signaledCorrectly = turnedRight ? _usedRight : _usedLeft;
            coach.NotifyEvaluated(ruleId, passed: signaledCorrectly);
        }
    }
}
