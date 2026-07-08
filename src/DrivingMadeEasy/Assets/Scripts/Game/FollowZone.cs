using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Judges the "following distance" rule. It becomes an encounter only when you're
    /// actually behind and closing on the lead car (a fair chance to act): it fires the
    /// heads-up then, tracks the closest gap you left, and on exit passes only if you kept
    /// a safe cushion instead of tailgating.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FollowZone : MonoBehaviour
    {
        public string ruleId = "following_distance";
        public CoachRuntime coach;
        public Transform leadCar;

        [Tooltip("Gap (m, bumper-to-bumper) at or above which you're following safely.")]
        public float safeGap = 6f;
        [Tooltip("Gap under which you count as 'following' the lead car at all.")]
        public float relevantGap = 16f;

        private bool _armed;
        private bool _wasFollowing;
        private float _minGap;

        private void OnTriggerEnter(Collider other)
        {
            var car = other.GetComponentInParent<CarController>();
            if (car == null || coach == null) return;
            _armed = true;
            _wasFollowing = false;
            _minGap = float.MaxValue;
        }

        private void OnTriggerStay(Collider other)
        {
            if (!_armed || leadCar == null) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;

            float gap = leadCar.position.z - car.transform.position.z - 4.2f; // bumper-to-bumper
            if (gap > 0f && gap < relevantGap && Mathf.Abs(car.SpeedMetersPerSecond) > 1.5f)
            {
                if (!_wasFollowing)
                {
                    _wasFollowing = true;
                    coach.NotifyRelevant(ruleId);
                }
                _minGap = Mathf.Min(_minGap, gap);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (!_armed) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null) return;
            _armed = false;
            if (!_wasFollowing) return; // never got behind the lead car — no encounter
            coach.NotifyEvaluated(ruleId, passed: _minGap >= safeGap);
        }
    }
}
