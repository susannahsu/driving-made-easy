using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "four-way stop" rule: you must come to a full stop AND not proceed while a
    /// cross car is in the box. (A pragmatic stand-in for full first-come right-of-way
    /// ordering — it enforces the safe behaviour: stop, then yield to cross traffic.)
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FourWayStopZone : MonoBehaviour
    {
        public string ruleId = "four_way_stop";
        public CoachRuntime coach;
        public TrafficCar[] crossTraffic;

        public Vector3 conflictCenter;
        public float conflictRadius = 8f;
        public float stopSpeedThreshold = 0.6f;

        private bool _armed;
        private bool _didStop;
        private bool _wentWhileCross;

        private bool CrossPresent()
        {
            if (crossTraffic == null) return false;
            foreach (var t in crossTraffic)
                if (t != null && t.IsNear(conflictCenter, conflictRadius)) return true;
            return false;
        }

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _didStop = false;
            _wentWhileCross = false;
            coach.NotifyRelevant(ruleId);
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;

            float sp = Mathf.Abs(car.SpeedMetersPerSecond);
            if (sp <= stopSpeedThreshold) _didStop = true;
            if (sp > 2f && CrossPresent()) _wentWhileCross = true;
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;
            coach.NotifyEvaluated(ruleId, passed: _didStop && !_wentWhileCross);
        }
    }
}
