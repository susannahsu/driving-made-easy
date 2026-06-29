using UnityEngine;

namespace DrivingMadeEasy.CameraRig
{
    /// <summary>
    /// Two driving views, toggled with a key (default C):
    ///  • <b>Cockpit</b> — rides in the driver's seat looking out over the hood, so it
    ///    feels like you're inside the car. This is the default and the most realistic.
    ///  • <b>Chase</b> — a smoothed third-person view from behind, handy for getting your
    ///    bearings.
    /// Later milestones add mirrors and a proper dashboard mesh (SPEC §5.2 / §10).
    /// </summary>
    public class DriverCamera : MonoBehaviour
    {
        public enum View { Cockpit, Chase }

        public Transform target;
        public View view = View.Cockpit;
        public KeyCode toggleKey = KeyCode.C;

        [Header("Cockpit (driver's seat)")]
        [Tooltip("Local offset from the car: -x = left seat (US), +y = eye height, +z = forward.")]
        public Vector3 cockpitOffset = new Vector3(-0.32f, 0.78f, -0.5f);
        [Tooltip("Tilt the gaze down so you see over the wheel to the road.")]
        public float cockpitPitch = 9f;

        [Header("Chase (third person)")]
        public Vector3 chaseOffset = new Vector3(0f, 3f, -7f);
        public float chaseLerp = 6f;
        public float lookAheadHeight = 1.2f;

        private void Update()
        {
            if (UnityEngine.Input.GetKeyDown(toggleKey))
            {
                view = view == View.Cockpit ? View.Chase : View.Cockpit;
            }
        }

        private void LateUpdate()
        {
            if (target == null) return;
            if (view == View.Cockpit) UpdateCockpit();
            else UpdateChase();
        }

        private void UpdateCockpit()
        {
            // Ride rigidly in the seat: position and rotation follow the car exactly, so
            // your "head" moves with the car like a real driver's.
            transform.position = target.TransformPoint(cockpitOffset);
            transform.rotation = target.rotation * Quaternion.Euler(cockpitPitch, 0f, 0f);
        }

        private void UpdateChase()
        {
            Vector3 desired = target.TransformPoint(chaseOffset);
            transform.position = Vector3.Lerp(
                transform.position, desired, chaseLerp * Time.deltaTime);

            Vector3 lookAt = target.position + Vector3.up * lookAheadHeight;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookAt - transform.position, Vector3.up),
                chaseLerp * Time.deltaTime);
        }
    }
}
