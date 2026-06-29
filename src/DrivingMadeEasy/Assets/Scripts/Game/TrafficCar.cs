using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A simple ambient traffic car that drives in a straight line and loops, set up by the
    /// bootstrap to stream across an intersection. Kinematic and visual-only for now (no
    /// collider) — the cross-traffic yield rule is judged by <see cref="CrossTrafficZone"/>.
    /// Proper lane-following / light-obeying AI (SPEC §5.2) comes later; this is enough to
    /// give the intersection real, moving traffic to yield to.
    /// </summary>
    public class TrafficCar : MonoBehaviour
    {
        public Vector3 startPoint;
        public Vector3 endPoint;
        public float speed = 9f;

        [Tooltip("0..1 starting position along the path, used to space a fleet into a stream.")]
        public float startOffset;

        private float _t;

        private void Start()
        {
            _t = Mathf.Repeat(startOffset, 1f);
            Vector3 dir = endPoint - startPoint;
            if (dir.sqrMagnitude > 0.0001f)
                transform.rotation = Quaternion.LookRotation(dir.normalized, Vector3.up);
        }

        private void Update()
        {
            float length = Vector3.Distance(startPoint, endPoint);
            if (length < 0.01f) return;
            _t += (speed / length) * Time.deltaTime;
            if (_t >= 1f) _t -= 1f; // loop back to the start
            transform.position = Vector3.Lerp(startPoint, endPoint, _t);
        }

        public bool IsNear(Vector3 point, float radius)
            => (transform.position - point).sqrMagnitude <= radius * radius;
    }
}
