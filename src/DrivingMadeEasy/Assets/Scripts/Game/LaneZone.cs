using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "lane keeping" rule over a stretch of road: it watches how far the car
    /// strays from the lane and, on exit, passes only if you kept it on the road within the
    /// lane the whole way. A simple stand-in until proper multi-lane roads exist (SPEC §5).
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class LaneZone : MonoBehaviour
    {
        public string ruleId = "lane_keeping";
        public CoachRuntime coach;

        [Tooltip("Max distance from lane centre (world X) before it counts as drifting out.")]
        public float laneHalfWidth = 3.2f;

        private bool _armed;
        private bool _drifted;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _drifted = false;
            coach.NotifyRelevant(ruleId);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            if (Mathf.Abs(car.transform.position.x) > laneHalfWidth) _drifted = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;
            coach.NotifyEvaluated(ruleId, passed: !_drifted);
        }
    }
}
