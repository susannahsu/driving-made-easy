using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;

namespace DrivingMadeEasy.UI
{
    /// <summary>
    /// Minimal immediate-mode HUD for M0: speed, a steering-angle indicator, and a
    /// Recalibrate button. Kept as OnGUI so M0 needs zero Canvas wiring; the real,
    /// clean HUD (speed, posted limit, gear/signal, Coach prompt area, mirrors) arrives
    /// with the later milestones per SPEC §10.
    /// </summary>
    public class DrivingHud : MonoBehaviour
    {
        public CarController car;
        public MonoBehaviour driverInputSource; // anything implementing IDriverInput

        private IDriverInput _input;

        // Resolved lazily, NOT in Awake: when the scene is built procedurally the
        // bootstrap assigns driverInputSource right after AddComponent, which runs
        // *after* Awake — caching in Awake would leave this permanently null.
        private IDriverInput Input => _input ??= driverInputSource as IDriverInput;

        private void OnGUI()
        {
            const int pad = 12;
            var boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 14, alignment = TextAnchor.MiddleCenter };

            if (car != null)
            {
                int mph = Mathf.RoundToInt(Mathf.Abs(car.SpeedMph));
                GUI.Box(new Rect(pad, pad, 92, 30), $"{mph} mph", boxStyle);

                // Turn-signal tell-tales, top-centre, blinking when active.
                bool flash = Mathf.Sin(Time.time * 8f) > 0f;
                var arrow = new GUIStyle(GUI.skin.box) { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                float cx = Screen.width * 0.5f;
                Color prev = GUI.color;
                GUI.color = (car.Signal == TurnSignal.Left && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx - 74, pad, 48, 30), "<", arrow);
                GUI.color = (car.Signal == TurnSignal.Right && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx + 26, pad, 48, 30), ">", arrow);
                GUI.color = prev;
            }

            // Compact recalibrate button (bottom-left, out of the road view).
            var rect = new Rect(pad, Screen.height - 34 - pad, 130, 30);
            var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 13 };
            if (GUI.Button(rect, "Recenter wheel", btnStyle))
            {
                Input?.Calibrate();
            }
        }
    }
}
