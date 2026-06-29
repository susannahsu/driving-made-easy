using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A pedestrian that paces along a path (across a crosswalk, or down a sidewalk),
    /// faces its direction of travel, and swings its legs and arms so it reads as walking.
    /// Visual only — yielding is judged by <see cref="CrosswalkZone"/>. Proper pedestrian
    /// AI (waiting for signals, jaywalking) comes later (SPEC §5.2).
    /// </summary>
    public class Pedestrian : MonoBehaviour
    {
        [Tooltip("Walking pace in m/s (~1.4 = a normal walk).")]
        public float walkSpeed = 1.4f;

        [Tooltip("Range of travel along the chosen axis (min/max), and which axis to walk.")]
        public float minA = -5f;
        public float maxA = 5f;
        public bool alongZ = false;
        public float phaseOffset = 0f;

        [Tooltip("Counts as 'on the crosswalk' (in the car's path) when |x| is under this.")]
        public float onRoadHalfWidth = 3.3f;

        public Transform leftLeg, rightLeg, leftArm, rightArm;
        public float swingDegrees = 32f;

        private float _fixedX, _fixedZ, _y, _prev;

        /// Fluent setup used by the bootstrap right after BuildPerson.
        public Pedestrian Configure(float min, float max, bool walkAlongZ, float phase)
        {
            minA = min;
            maxA = max;
            alongZ = walkAlongZ;
            phaseOffset = phase;
            return this;
        }

        private void Start()
        {
            var p = transform.position;
            _y = p.y; _fixedX = p.x; _fixedZ = p.z;
            _prev = minA;
        }

        private void Update()
        {
            float span = Mathf.Max(0.01f, maxA - minA);
            float val = minA + Mathf.PingPong(Time.time * walkSpeed + phaseOffset, span);
            float delta = val - _prev;
            _prev = val;

            transform.position = alongZ
                ? new Vector3(_fixedX, _y, val)
                : new Vector3(val, _y, _fixedZ);

            if (Mathf.Abs(delta) > 1e-5f)
            {
                Vector3 fwd = alongZ
                    ? new Vector3(0f, 0f, Mathf.Sign(delta))
                    : new Vector3(Mathf.Sign(delta), 0f, 0f);
                transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            }

            float s = Mathf.Sin((Time.time * walkSpeed + phaseOffset) * 3.2f) * swingDegrees;
            if (leftLeg) leftLeg.localRotation = Quaternion.Euler(s, 0f, 0f);
            if (rightLeg) rightLeg.localRotation = Quaternion.Euler(-s, 0f, 0f);
            if (leftArm) leftArm.localRotation = Quaternion.Euler(-s, 0f, 0f);
            if (rightArm) rightArm.localRotation = Quaternion.Euler(s, 0f, 0f);
        }

        public bool IsOnCrosswalk => Mathf.Abs(transform.position.x) < onRoadHalfWidth;
    }
}
