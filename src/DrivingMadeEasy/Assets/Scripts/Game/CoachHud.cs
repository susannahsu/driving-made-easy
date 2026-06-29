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

        private void OnGUI()
        {
            if (coach != null && coach.Session != null)
            {
                var scoreStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 20, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(Screen.width - 196, 16, 180, 44),
                    $"Safety: {coach.Session.SafetyScore}", scoreStyle);

                var statusStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 14, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(Screen.width - 196, 64, 180, 28),
                    coach.StatusLine(trackedRuleId), statusStyle);
            }

            // Active posted speed limit (top-left, below the speedometer/steer readouts).
            if (coach != null && coach.CurrentSpeedLimitMph > 0)
            {
                var limitStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 18, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(16, 120, 180, 40),
                    $"Limit: {coach.CurrentSpeedLimitMph} mph", limitStyle);
            }

            if (Time.time < _bannerUntil && !string.IsNullOrEmpty(_banner))
            {
                var bannerStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 17, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                float w = Mathf.Min(720f, Screen.width - 220f);
                GUI.Box(new Rect((Screen.width - w) / 2f, 16f, w, 64f), _banner, bannerStyle);
            }

            if (Time.time < _toastUntil && !string.IsNullOrEmpty(_toast))
            {
                Color prev = GUI.color;
                GUI.color = _toastIsPenalty
                    ? new Color(1f, 0.6f, 0.6f)
                    : new Color(0.6f, 1f, 0.6f);
                var toastStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 19, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                float w = Mathf.Min(640f, Screen.width - 220f);
                GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.5f - 30f, w, 60f),
                    _toast, toastStyle);
                GUI.color = prev;
            }
        }
    }
}
