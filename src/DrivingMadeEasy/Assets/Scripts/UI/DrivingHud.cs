using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.Game;

namespace DrivingMadeEasy.UI
{
    /// <summary>
    /// On-screen driving controls sized for a phone. Touches are hit-tested against button
    /// rects directly in Update (far more reliable on iOS than IMGUI RepeatButton), and
    /// OnGUI only draws. GAS / BRAKE / REV are hold buttons; RESET un-sticks the car;
    /// RECENTER re-zeros tilt. A diagnostic line shows steering + accelerometer + orientation.
    /// </summary>
    public class DrivingHud : MonoBehaviour
    {
        public CarController car;
        public MonoBehaviour driverInputSource; // MotionSteeringInput
        public CarRescue rescue;
        public bool showDiagnostics = true;

        private MotionSteeringInput _motion;
        private MotionSteeringInput Motion => _motion ??= driverInputSource as MotionSteeringInput;
        private IDriverInput Driver => driverInputSource as IDriverInput;

        private Rect _gas, _brake, _rev, _reset, _recenter;
        private bool _gasHeld, _brakeHeld, _revHeld;

        private float Scale => Mathf.Max(1f, Screen.height / 460f);

        private void ComputeRects()
        {
            float k = Scale, pad = 14f * k;
            float bw = 160f * k, bh = 120f * k;
            float bottom = Screen.height - bh - pad;
            _brake = new Rect(pad, bottom, bw, bh);
            _gas = new Rect(Screen.width - bw - pad, bottom, bw, bh);
            _rev = new Rect(pad, bottom - bh * 0.5f - 6f * k, bw, bh * 0.42f);

            float mbw = 160f * k, mbh = 54f * k, mcx = Screen.width * 0.5f;
            _reset = new Rect(mcx - mbw - 8f * k, Screen.height - mbh - pad, mbw, mbh);
            _recenter = new Rect(mcx + 8f * k, Screen.height - mbh - pad, mbw, mbh);
        }

        // Screen touches are bottom-left origin; GUI rects are top-left origin.
        private static Vector2 GuiPoint(Vector2 screenPos)
            => new Vector2(screenPos.x, Screen.height - screenPos.y);

        private void Update()
        {
            ComputeRects();
            _gasHeld = _brakeHeld = _revHeld = false;

            int n = UnityEngine.Input.touchCount;
            if (n > 0)
            {
                for (int i = 0; i < n; i++)
                {
                    var t = UnityEngine.Input.GetTouch(i);
                    Vector2 p = GuiPoint(t.position);
                    if (_gas.Contains(p)) _gasHeld = true;
                    if (_brake.Contains(p)) _brakeHeld = true;
                    if (_rev.Contains(p)) _revHeld = true;
                    if (t.phase == TouchPhase.Began)
                    {
                        if (_reset.Contains(p)) rescue?.Respawn();
                        if (_recenter.Contains(p)) Driver?.Calibrate();
                    }
                }
            }
            else // editor / mouse
            {
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    Vector2 p = GuiPoint(UnityEngine.Input.mousePosition);
                    if (_gas.Contains(p)) _gasHeld = true;
                    if (_brake.Contains(p)) _brakeHeld = true;
                    if (_rev.Contains(p)) _revHeld = true;
                }
                if (UnityEngine.Input.GetMouseButtonDown(0))
                {
                    Vector2 p = GuiPoint(UnityEngine.Input.mousePosition);
                    if (_reset.Contains(p)) rescue?.Respawn();
                    if (_recenter.Contains(p)) Driver?.Calibrate();
                }
            }

            if (Motion != null)
            {
                Motion.SetThrottle(_gasHeld ? 1f : 0f);
                Motion.SetBrake(_brakeHeld ? 1f : 0f);
                Motion.SetReverse(_revHeld);
            }
        }

        private GUIStyle Label(int baseSize, TextAnchor a = TextAnchor.MiddleCenter)
            => new GUIStyle(GUI.skin.box)
            { fontSize = Mathf.RoundToInt(baseSize * Scale), fontStyle = FontStyle.Bold, alignment = a };

        private void DrawButton(Rect r, string label, bool held, Color c)
        {
            Color prev = GUI.color;
            GUI.color = held ? Color.Lerp(c, Color.white, 0.5f) : new Color(c.r, c.g, c.b, 0.85f);
            GUI.Box(r, label, Label(20));
            GUI.color = prev;
        }

        private void OnGUI()
        {
            float k = Scale, pad = 14f * k;

            if (car != null)
            {
                int mph = Mathf.RoundToInt(Mathf.Abs(car.SpeedMph));
                GUI.Box(new Rect(pad, pad, 120f * k, 40f * k), $"{mph} mph", Label(15));

                bool flash = Mathf.Sin(Time.time * 8f) > 0f;
                float cx = Screen.width * 0.5f;
                Color prev = GUI.color;
                GUI.color = (car.Signal == TurnSignal.Left && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx - 74f * k, pad, 44f * k, 40f * k), "<", Label(18));
                GUI.color = (car.Signal == TurnSignal.Right && flash) ? Color.green : new Color(1f, 1f, 1f, 0.22f);
                GUI.Box(new Rect(cx + 30f * k, pad, 44f * k, 40f * k), ">", Label(18));
                GUI.color = prev;
            }

            if (showDiagnostics && car != null)
            {
                string diag = $"steer {car.SteeringValue:+0.00;-0.00; 0.00}   {Screen.orientation}";
                if (Motion != null && Motion.MotionActive)
                {
                    Vector3 a = Motion.RawAccel;
                    diag += $"   accel x {a.x:0.00} y {a.y:0.00} z {a.z:0.00}";
                }
                GUI.Box(new Rect(pad, pad + 46f * k, 460f * k, 28f * k), "  " + diag, Label(11, TextAnchor.MiddleLeft));
            }

            DrawButton(_gas, "GAS", _gasHeld, new Color(0.2f, 0.65f, 0.25f));
            DrawButton(_brake, "BRAKE", _brakeHeld, new Color(0.8f, 0.3f, 0.2f));
            DrawButton(_rev, "REV", _revHeld, new Color(0.4f, 0.4f, 0.5f));
            DrawButton(_reset, "RESET CAR", false, new Color(0.45f, 0.45f, 0.55f));
            DrawButton(_recenter, "RECENTER", false, new Color(0.45f, 0.45f, 0.55f));
        }
    }
}
