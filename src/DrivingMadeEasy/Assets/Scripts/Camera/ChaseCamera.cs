using UnityEngine;

namespace DrivingMadeEasy.CameraRig
{
    /// <summary>
    /// Simple smoothed chase camera for M0. Later milestones add a selectable
    /// cockpit/dashboard view (the most realistic) and mirrors for lane-change checks.
    /// </summary>
    public class ChaseCamera : MonoBehaviour
    {
        public Transform target;
        public Vector3 localOffset = new Vector3(0f, 3f, -7f);
        public float followLerp = 6f;
        public float lookAheadHeight = 1.2f;

        private void LateUpdate()
        {
            if (target == null) return;

            Vector3 desired = target.TransformPoint(localOffset);
            transform.position = Vector3.Lerp(
                transform.position, desired, followLerp * Time.deltaTime);

            Vector3 lookAt = target.position + Vector3.up * lookAheadHeight;
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                Quaternion.LookRotation(lookAt - transform.position, Vector3.up),
                followLerp * Time.deltaTime);
        }
    }
}
