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
        [Tooltip("Limb swing amplitude — kept subtle so the walk reads naturally, not flailing.")]
        public float swingDegrees = 16f;
        public float armSwingScale = 0.6f;

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
            float phase = Time.time * walkSpeed + phaseOffset;
            float span = Mathf.Max(0.01f, maxA - minA);
            float val = minA + Mathf.PingPong(phase, span);
            float delta = val - _prev;
            _prev = val;

            // A subtle vertical bob in time with the stride.
            float bob = Mathf.Abs(Mathf.Sin(phase * 2.6f)) * 0.025f;
            transform.position = alongZ
                ? new Vector3(_fixedX, _y + bob, val)
                : new Vector3(val, _y + bob, _fixedZ);

            if (Mathf.Abs(delta) > 1e-5f)
            {
                Vector3 fwd = alongZ
                    ? new Vector3(0f, 0f, Mathf.Sign(delta))
                    : new Vector3(Mathf.Sign(delta), 0f, 0f);
                transform.rotation = Quaternion.LookRotation(fwd, Vector3.up);
            }

            float s = Mathf.Sin(phase * 2.6f) * swingDegrees;
            float a = s * armSwingScale;
            if (leftLeg) leftLeg.localRotation = Quaternion.Euler(s, 0f, 0f);
            if (rightLeg) rightLeg.localRotation = Quaternion.Euler(-s, 0f, 0f);
            if (leftArm) leftArm.localRotation = Quaternion.Euler(-a, 0f, 0f);
            if (rightArm) rightArm.localRotation = Quaternion.Euler(a, 0f, 0f);
        }

        public bool IsOnCrosswalk => Mathf.Abs(transform.position.x) < onRoadHalfWidth;
    }
}
