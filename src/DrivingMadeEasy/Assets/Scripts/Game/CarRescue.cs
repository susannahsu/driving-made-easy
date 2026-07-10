using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Keeps the player from ever getting stuck: remembers the last good on-road pose, and
    /// puts the car back there (upright, stopped) on demand (the RESET button) or
    /// automatically if it flips over or wanders off the road for a while.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    public class CarRescue : MonoBehaviour
    {
        public float laneHalfWidth = 3.2f;
        public float autoRescueOffRoadSeconds = 5f;
        public float autoRescueFlippedSeconds = 2f;

        private Rigidbody _rb;
        private Vector3 _lastGoodPos;
        private Quaternion _lastGoodRot;
        private float _offRoadTimer;
        private float _flippedTimer;

        private void Awake()
        {
            _rb = GetComponent<Rigidbody>();
            _lastGoodPos = transform.position;
            _lastGoodRot = transform.rotation;
        }

        private void FixedUpdate()
        {
            bool upright = transform.up.y > 0.5f;
            bool onRoad = Mathf.Abs(transform.position.x) < laneHalfWidth + 1.5f;

            // Record a safe restore point whenever we're driving normally on the road.
            if (upright && onRoad && _rb.velocity.magnitude > 0.5f)
            {
                _lastGoodPos = transform.position;
                _lastGoodRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
            }

            _flippedTimer = upright ? 0f : _flippedTimer + Time.fixedDeltaTime;
            _offRoadTimer = onRoad ? 0f : _offRoadTimer + Time.fixedDeltaTime;

            if (_flippedTimer > autoRescueFlippedSeconds || _offRoadTimer > autoRescueOffRoadSeconds)
                Respawn();
        }

        /// Put the car back on the road, upright and stopped.
        public void Respawn()
        {
            transform.position = _lastGoodPos + Vector3.up * 0.5f;
            transform.rotation = _lastGoodRot;
            _rb.velocity = Vector3.zero;
            _rb.angularVelocity = Vector3.zero;
            _offRoadTimer = 0f;
            _flippedTimer = 0f;
        }
    }
}
