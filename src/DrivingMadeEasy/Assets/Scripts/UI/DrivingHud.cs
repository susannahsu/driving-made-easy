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

        private void Awake()
        {
            _input = driverInputSource as IDriverInput;
        }

        private void OnGUI()
        {
            const int pad = 16;
            var boxStyle = new GUIStyle(GUI.skin.box) { fontSize = 20, alignment = TextAnchor.MiddleLeft };

            if (car != null)
            {
                int mph = Mathf.RoundToInt(Mathf.Abs(car.SpeedMph));
                GUI.Box(new Rect(pad, pad, 220, 44), $"  {mph} mph", boxStyle);

                float steer = car.SteeringValue;
                GUI.Box(new Rect(pad, pad + 52, 220, 44),
                    $"  steer: {steer:+0.00;-0.00; 0.00}", boxStyle);
            }

            // Big, thumb-friendly recalibrate button (bottom-center).
            float bw = 220, bh = 56;
            var rect = new Rect((Screen.width - bw) / 2f, Screen.height - bh - pad, bw, bh);
            var btnStyle = new GUIStyle(GUI.skin.button) { fontSize = 20 };
            if (GUI.Button(rect, "Recenter wheel", btnStyle))
            {
                _input?.Calibrate();
            }
        }
    }
}
