using UnityEngine;
using DrivingMadeEasy.Coaching;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Minimal coaching overlay for M1: a guidance banner (the heads-up / correction), a
    /// brief ticket or graduation toast, the live safety score, and a small rule-status
    /// readout so you can watch a rule move from "coaching 1/3" to "enforced". The polished
    /// HUD and on-device voice come later (SPEC §10, §6.3). ASCII only — the built-in GUI
    /// font has no emoji glyphs.
    /// </summary>
    public class CoachHud : MonoBehaviour
    {
        public CoachRuntime coach;
        public string trackedRuleId = "stop_sign";

        private string _banner;
        private float _bannerUntil;
        private string _toast;
        private float _toastUntil;
        private bool _toastIsPenalty;

        // Subscribe in Start (not OnEnable): the bootstrap assigns `coach` right after
        // AddComponent, which runs after OnEnable but before Start.
        private void Start()
        {
            if (coach == null) return;
            coach.Preempt += OnPreempt;
            coach.Outcome += OnOutcome;
        }

        private void OnDestroy()
        {
            if (coach == null) return;
            coach.Preempt -= OnPreempt;
            coach.Outcome -= OnOutcome;
        }

        private void OnPreempt(PreemptiveCue cue)
        {
            _banner = "COACH:  " + cue.Message;
            _bannerUntil = Time.time + 5f;
        }

        private void OnOutcome(CoachOutcome o)
        {
            if (!string.IsNullOrEmpty(o.Message))
            {
                _banner = (o.WasCoachingPhase ? "COACH:  " : "") + o.Message;
                _bannerUntil = Time.time + 5f;
            }

            if (o.Action == CoachAction.IssuePenalty && o.Ticket != null)
            {
                _toast = $"TICKET — {o.Ticket.Title}: -{o.Ticket.Points} pts  (${o.Ticket.Fine} fine)";
                _toastUntil = Time.time + 4.5f;
                _toastIsPenalty = true;
            }
            else if (o.Graduated)
            {
                _toast = $"You've got \"{o.RuleId}\" down — no more hints from here.";
                _toastUntil = Time.time + 4.5f;
                _toastIsPenalty = false;
            }
        }

        private float Scale => Mathf.Max(1f, Screen.height / 460f);

        private GUIStyle Box(int baseSize, bool wrap = false)
        {
            return new GUIStyle(GUI.skin.box)
            {
                fontSize = Mathf.RoundToInt(baseSize * Scale),
                alignment = TextAnchor.MiddleCenter,
                wordWrap = wrap
            };
        }

        private void OnGUI()
        {
            float k = Scale;
            float pad = 12f * k;

            if (coach != null && coach.Session != null)
            {
                GUI.Box(new Rect(Screen.width - 150f * k, pad, 138f * k, 30f * k),
                    $"Safety: {coach.Session.SafetyScore}", Box(14));
                GUI.Box(new Rect(Screen.width - 150f * k, pad + 34f * k, 138f * k, 26f * k),
                    coach.StatusLine(trackedRuleId), Box(11));
            }

            // Active posted speed limit (top-left, below the speedometer).
            if (coach != null && coach.CurrentSpeedLimitMph > 0)
            {
                GUI.Box(new Rect(pad, pad + 46f * k + 30f * k, 110f * k, 30f * k),
                    $"Limit {coach.CurrentSpeedLimitMph}", Box(14));
            }

            if (Time.time < _bannerUntil && !string.IsNullOrEmpty(_banner))
            {
                float w = Mathf.Min(760f * k, Screen.width - 320f * k);
                GUI.Box(new Rect((Screen.width - w) / 2f, pad + 44f * k, w, 62f * k), _banner, Box(15, true));
            }

            if (Time.time < _toastUntil && !string.IsNullOrEmpty(_toast))
            {
                Color prev = GUI.color;
                GUI.color = _toastIsPenalty
                    ? new Color(1f, 0.6f, 0.6f)
                    : new Color(0.6f, 1f, 0.6f);
                float w = Mathf.Min(640f * k, Screen.width - 320f * k);
                GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.5f, w, 60f * k), _toast, Box(16, true));
                GUI.color = prev;
            }
        }
    }
}
