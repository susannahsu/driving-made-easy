using UnityEngine;
using DrivingMadeEasy.Input;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.Game;

namespace DrivingMadeEasy.UI
{
    /// <summary>
    /// On-screen driving controls for the phone. Touches are hit-tested against button rects
    /// directly (reliable on iOS). Hold GAS / BRAKE / REV; tap SIG-L / SIG-R (blinkers),
    /// LIGHTS, HORN, RESET (un-stick), RECENTER (re-zero tilt).
    /// </summary>
    public class DrivingHud : MonoBehaviour
    {
        public CarController car;
        public MonoBehaviour driverInputSource; // MotionSteeringInput
        public CarRescue rescue;
        public CarLights lights;
        public CarAudio audioRig;
        public bool showDiagnostics = true;

        private MotionSteeringInput _motion;
        private MotionSteeringInput Motion => _motion ??= driverInputSource as MotionSteeringInput;
        private IDriverInput Driver => driverInputSource as IDriverInput;

        private Rect _gas, _brake, _rev, _reset, _recenter, _left, _right, _lights, _horn;
        private bool _gasHeld, _brakeHeld, _revHeld;

        private float Scale => Mathf.Max(1f, Screen.height / 460f);

        private void ComputeRects()
        {
            float k = Scale, pad = 14f * k;
            float bw = 150f * k, bh = 115f * k;
            float bottom = Screen.height - bh - pad;
            _brake = new Rect(pad, bottom, bw, bh);
            _gas = new Rect(Screen.width - bw - pad, bottom, bw, bh);

            float sh = bh * 0.4f;
            _rev = new Rect(pad, bottom - sh - 6f * k, bw, sh);
            _left = new Rect(pad, bottom - 2f * (sh + 6f * k), bw, sh);
            _right = new Rect(Screen.width - bw - pad, bottom - sh - 6f * k, bw, sh);

            float mbw = 150f * k, mbh = 52f * k, gap = 8f * k, cx = Screen.width * 0.5f;
            float cy = Screen.height - mbh - pad;
            float total = 4f * mbw + 3f * gap;
            float x0 = cx - total * 0.5f;
            _reset = new Rect(x0, cy, mbw, mbh);
            _recenter = new Rect(x0 + (mbw + gap), cy, mbw, mbh);
            _lights = new Rect(x0 + 2f * (mbw + gap), cy, mbw, mbh);
            _horn = new Rect(x0 + 3f * (mbw + gap), cy, mbw, mbh);
        }

        private static Vector2 GuiPoint(Vector2 screenPos)
            => new Vector2(screenPos.x, Screen.height - screenPos.y);

        private void Tap(Vector2 p)
        {
            if (_reset.Contains(p)) rescue?.Respawn();
            if (_recenter.Contains(p)) Driver?.Calibrate();
            if (_left.Contains(p)) Motion?.ToggleSignal(TurnSignal.Left);
            if (_right.Contains(p)) Motion?.ToggleSignal(TurnSignal.Right);
            if (_lights.Contains(p)) lights?.Toggle();
            if (_horn.Contains(p)) audioRig?.Horn();
        }

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
                    if (t.phase == TouchPhase.Began) Tap(p);
                }
            }
            else
            {
                if (UnityEngine.Input.GetMouseButton(0))
                {
                    Vector2 p = GuiPoint(UnityEngine.Input.mousePosition);
                    if (_gas.Contains(p)) _gasHeld = true;
                    if (_brake.Contains(p)) _brakeHeld = true;
                    if (_rev.Contains(p)) _revHeld = true;
                }
                if (UnityEngine.Input.GetMouseButtonDown(0))
                    Tap(GuiPoint(UnityEngine.Input.mousePosition));
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

        private void DrawButton(Rect r, string label, bool active, Color c)
        {
            Color prev = GUI.color;
            GUI.color = active ? Color.Lerp(c, Color.white, 0.55f) : new Color(c.r, c.g, c.b, 0.85f);
            GUI.Box(r, label, Label(16));
            GUI.color = prev;
        }

        private void OnGUI()
        {
            float k = Scale, pad = 14f * k;

            if (car != null)
            {
                int mph = Mathf.RoundToInt(Mathf.Abs(car.SpeedMph));
                GUI.Box(new Rect(pad, pad, 120f * k, 40f * k), $"{mph} mph", Label(15));
            }

            if (showDiagnostics && car != null)
            {
                string diag = $"steer {car.SteeringValue:+0.00;-0.00; 0.00}   {Screen.orientation}";
                if (Motion != null && Motion.MotionActive)
                {
                    Vector3 a = Motion.RawAccel;
                    diag += $"   accel x {a.x:0.00} y {a.y:0.00} z {a.z:0.00}";
                }
                GUI.Box(new Rect(pad, pad + 46f * k, 460f * k, 26f * k), "  " + diag, Label(10, TextAnchor.MiddleLeft));
            }

            var green = new Color(0.2f, 0.65f, 0.25f);
            var red = new Color(0.8f, 0.3f, 0.2f);
            var grey = new Color(0.45f, 0.45f, 0.55f);
            var amber = new Color(0.9f, 0.65f, 0.15f);

            DrawButton(_gas, "GAS", _gasHeld, green);
            DrawButton(_brake, "BRAKE", _brakeHeld, red);
            DrawButton(_rev, "REV", _revHeld, grey);

            bool sigL = car != null && car.Signal == TurnSignal.Left;
            bool sigR = car != null && car.Signal == TurnSignal.Right;
            DrawButton(_left, "< SIGNAL", sigL, sigL ? amber : grey);
            DrawButton(_right, "SIGNAL >", sigR, sigR ? amber : grey);

            DrawButton(_reset, "RESET", false, grey);
            DrawButton(_recenter, "RECENTER", false, grey);
            DrawButton(_lights, "LIGHTS", lights != null && lights.On, lights != null && lights.On ? amber : grey);
            DrawButton(_horn, "HORN", false, grey);
        }
    }
}
