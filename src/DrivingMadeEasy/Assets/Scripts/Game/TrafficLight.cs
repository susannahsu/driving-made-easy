using UnityEngine;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// A signal head that cycles green → yellow → red on a timer and lights the matching
    /// bulb. <see cref="TrafficLightZone"/> reads <see cref="MustStop"/> to judge the rule.
    /// </summary>
    public class TrafficLight : MonoBehaviour
    {
        public enum Phase { Green, Yellow, Red }

        public float greenTime = 8f;
        public float yellowTime = 2.5f;
        public float redTime = 8f;

        public Renderer redBulb;
        public Renderer yellowBulb;
        public Renderer greenBulb;

        public Phase Current { get; private set; } = Phase.Green;
        public bool MustStop => Current == Phase.Red;

        private float _t;
        private static readonly Color Off = new Color(0.06f, 0.06f, 0.06f);

        private void Start() => Apply();

        private void Update()
        {
            _t += Time.deltaTime;
            float dur = Current == Phase.Green ? greenTime
                      : Current == Phase.Yellow ? yellowTime : redTime;
            if (_t >= dur)
            {
                _t = 0f;
                Current = Current == Phase.Green ? Phase.Yellow
                        : Current == Phase.Yellow ? Phase.Red : Phase.Green;
                Apply();
            }
        }

        private void Apply()
        {
            SetBulb(redBulb, Current == Phase.Red, Color.red);
            SetBulb(yellowBulb, Current == Phase.Yellow, new Color(1f, 0.8f, 0.1f));
            SetBulb(greenBulb, Current == Phase.Green, new Color(0.1f, 1f, 0.25f));
        }

        private static void SetBulb(Renderer r, bool on, Color c)
        {
            if (r == null) return;
            var m = r.material;
            m.color = on ? c : Off;
            m.SetColor("_EmissionColor", on ? c : Color.black);
        }
    }
}
