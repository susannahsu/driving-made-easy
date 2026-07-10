using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.Game;

namespace DrivingMadeEasy.UI
{
    /// <summary>
    /// The on-screen driving controls + readouts, sized for a phone. Big GAS / BRAKE / REV
    /// buttons you hold with your thumbs, a RESET button that un-sticks the car, a
    /// RECENTER button to re-zero the tilt, the speed, turn-signal tell-tales, and a small
    /// diagnostic line (steering value + accelerometer) to help tune tilt steering.
    /// </summary>
    public class DrivingHud : MonoBehaviour
    {
        public CarController car;
        public MonoBehaviour driverInputSource; // MotionSteeringInput
        public CarRescue rescue;
        public bool showDiagnostics = true;

        private MotionSteeringInput _motion;
        // Resolved lazily — the bootstrap assigns driverInputSource after AddComponent.
        private MotionSteeringInput Motion => _motion ??= driverInputSource as MotionSteeringInput;
        private IDriverInput _input;
        private IDriverInput Input => _input ??= driverInputSource as IDriverInput;

        private float Scale => Mathf.Max(1f, Screen.height / 460f);

        private GUIStyle Style(int baseSize, FontStyle fs = FontStyle.Bold)
        {
            return new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.RoundToInt(baseSize * Scale),
                fontStyle = fs,
                alignment = TextAnchor.MiddleCenter
            };
        }

        private void OnGUI()
        {
            float k = Scale;
            float pad = 14f * k;

            // ---- Readouts (top) ----
            if (car != null)
            {
                int mph = Mathf.RoundToInt(Mathf.Abs(car.SpeedMph));
                GUI.Box(new Rect(pad, pad, 120f * k, 40f * k), $"{mph} mph", Style(15));

                bool flash = Mathf.Sin(Time.time * 8f) > 0f;
                var arrow = Style(18);
                float cx = Screen.width * 0.5f;
                Color prev = GUI.color;
                GUI.color = (car.Signal == TurnSignal.Left && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx - 74f * k, pad, 44f * k, 40f * k), "<", arrow);
                GUI.color = (car.Signal == TurnSignal.Right && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx + 30f * k, pad, 44f * k, 40f * k), ">", arrow);
                GUI.color = prev;
            }

            // ---- Diagnostic (steering + accelerometer) ----
            if (showDiagnostics && car != null)
            {
                string diag = $"steer {car.SteeringValue:+0.00;-0.00; 0.00}";
                if (Motion != null && Motion.MotionActive)
                {
                    Vector3 a = Motion.RawAccel;
                    diag += $"   accel  x {a.x:0.00}  y {a.y:0.00}  z {a.z:0.00}";
                }
                var dstyle = Style(11, FontStyle.Normal);
                dstyle.alignment = TextAnchor.MiddleLeft;
                GUI.Box(new Rect(pad, pad + 46f * k, 360f * k, 28f * k), "  " + diag, dstyle);
            }

            // ---- Buttons ----
            float bw = 150f * k, bh = 96f * k;
            float bottom = Screen.height - bh - pad;

            // BRAKE (bottom-left), GAS (bottom-right) — hold to press.
            bool brakeHeld = GUI.RepeatButton(new Rect(pad, bottom, bw, bh), "BRAKE", Style(20));
            bool gasHeld = GUI.RepeatButton(new Rect(Screen.width - bw - pad, bottom, bw, bh), "GAS", Style(20));

            // REVERSE toggle (small, above brake).
            bool revHeld = GUI.RepeatButton(new Rect(pad, bottom - bh * 0.55f - 6f * k, bw, bh * 0.5f), "REV", Style(14));

            if (Motion != null)
            {
                Motion.SetThrottle(gasHeld ? 1f : 0f);
                Motion.SetBrake(brakeHeld ? 1f : 0f);
                Motion.SetReverse(revHeld);
            }

            // RESET (un-stick) + RECENTER, bottom-center.
            float mbw = 150f * k, mbh = 44f * k;
            float mcx = Screen.width * 0.5f;
            if (GUI.Button(new Rect(mcx - mbw - 6f * k, Screen.height - mbh - pad, mbw, mbh), "RESET CAR", Style(15)))
                rescue?.Respawn();
            if (GUI.Button(new Rect(mcx + 6f * k, Screen.height - mbh - pad, mbw, mbh), "RECENTER", Style(15)))
                Input?.Calibrate();
        }
    }
}
