using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A simple pedestrian that paces back and forth across a crosswalk. Visual only (no
    /// collider) — the crosswalk yield rule is judged by <see cref="CrosswalkZone"/>, not by
    /// physical contact. A first taste of the "world has life in it" goal (SPEC §5.2);
    /// proper pedestrian AI (waiting for signals, occasional jaywalking) comes later.
    /// </summary>
    public class Pedestrian : MonoBehaviour
    {
        [Tooltip("Walking pace in m/s (~1.4 = a normal walk).")]
        public float walkSpeed = 1.4f;
        public float minX = -5f;
        public float maxX = 5f;

        [Tooltip("Counts as 'on the crosswalk' (i.e. in the car's path) when |x| is under this.")]
        public float onRoadHalfWidth = 3.3f;

        private float _z, _y;

        private void Start()
        {
            _z = transform.position.z;
            _y = transform.position.y;
        }

        private void Update()
        {
            float span = Mathf.Max(0.01f, maxX - minX);
            float x = minX + Mathf.PingPong(Time.time * walkSpeed, span);
            transform.position = new Vector3(x, _y, _z);
        }

        public bool IsOnCrosswalk => Mathf.Abs(transform.position.x) < onRoadHalfWidth;
    }
}
