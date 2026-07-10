using UnityEngine;
using DrivingMadeEasy.Vehicle;
using DrivingMadeEasy.Input;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Procedurally generated car audio (no asset files): a looping engine whose pitch and
    /// volume rise with speed, a blinker tick while a turn signal is on, and a horn.
    /// </summary>
    public class CarAudio : MonoBehaviour
    {
        public CarController car;
        public float maxSpeed = 13.4f;

        private AudioSource _engine;
        private AudioSource _sfx;
        private AudioClip _tick;
        private AudioClip _horn;
        private float _blinkTimer;

        private void Awake()
        {
            _engine = gameObject.AddComponent<AudioSource>();
            _engine.clip = MakeEngine();
            _engine.loop = true;
            _engine.spatialBlend = 0f;
            _engine.volume = 0.28f;
            _engine.pitch = 0.6f;
            _engine.Play();

            _sfx = gameObject.AddComponent<AudioSource>();
            _sfx.spatialBlend = 0f;
            _sfx.playOnAwake = false;
            _tick = MakeClick();
            _horn = MakeHorn();
        }

        private void Update()
        {
            if (car != null)
            {
                float t = Mathf.Clamp01(Mathf.Abs(car.SpeedMetersPerSecond) / Mathf.Max(1f, maxSpeed));
                _engine.pitch = Mathf.Lerp(0.6f, 1.9f, t);
                _engine.volume = Mathf.Lerp(0.24f, 0.5f, t);
            }

            TurnSignal sig = car != null ? car.Signal : TurnSignal.None;
            if (sig != TurnSignal.None)
            {
                _blinkTimer -= Time.deltaTime;
                if (_blinkTimer <= 0f)
                {
                    _blinkTimer = 0.5f;
                    _sfx.PlayOneShot(_tick, 0.6f);
                }
            }
            else
            {
                _blinkTimer = 0f;
            }
        }

        public void Horn()
        {
            if (_sfx != null && _horn != null) _sfx.PlayOneShot(_horn, 0.8f);
        }

        // ---- Procedural clips ----

        private static AudioClip MakeEngine()
        {
            const int rate = 44100;
            const int n = rate; // 1-second loop
            const float baseF = 68f; // integer cycles/sec => seamless harmonics
            var data = new float[n];
            var rng = new System.Random(7);
            for (int i = 0; i < n; i++)
            {
                float ph = i / (float)rate;
                float v = Mathf.Sin(2f * Mathf.PI * baseF * ph) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * baseF * 2f * ph) * 0.28f
                        + Mathf.Sin(2f * Mathf.PI * baseF * 3f * ph) * 0.16f
                        + Mathf.Sin(2f * Mathf.PI * baseF * 4f * ph) * 0.08f;
                v += ((float)rng.NextDouble() * 2f - 1f) * 0.04f;
                data[i] = v * 0.4f;
            }
            var clip = AudioClip.Create("engine", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeClick()
        {
            const int rate = 44100;
            int n = (int)(rate * 0.04f);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float env = 1f - i / (float)n;
                data[i] = Mathf.Sin(2f * Mathf.PI * 1800f * i / rate) * env * 0.5f;
            }
            var clip = AudioClip.Create("tick", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }

        private static AudioClip MakeHorn()
        {
            const int rate = 44100;
            int n = (int)(rate * 0.4f);
            var data = new float[n];
            for (int i = 0; i < n; i++)
            {
                float attack = Mathf.Clamp01(i / (rate * 0.02f));
                float release = Mathf.Clamp01((n - i) / (rate * 0.05f));
                float ph = i / (float)rate;
                float v = Mathf.Sin(2f * Mathf.PI * 440f * ph) * 0.5f
                        + Mathf.Sin(2f * Mathf.PI * 554f * ph) * 0.4f;
                data[i] = v * attack * release * 0.4f;
            }
            var clip = AudioClip.Create("horn", n, 1, rate, false);
            clip.SetData(data, 0);
            return clip;
        }
    }
}
