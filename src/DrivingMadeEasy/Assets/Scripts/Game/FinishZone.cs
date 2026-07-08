using UnityEngine;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// The end of the lesson. When the player crosses it, the drive report pops up with a
    /// pass/fail verdict for the whole run.
    /// </summary>
    [RequireComponent(typeof(BoxCollider))]
    public class FinishZone : MonoBehaviour
    {
        public DriveReportHud report;

        private bool _done;

        private void OnTriggerEnter(Collider other)
        {
            if (_done) return;
            var car = other.GetComponentInParent<CarController>();
            if (car == null || report == null) return;
            _done = true;
            report.ShowLessonResult();
        }
    }
}
