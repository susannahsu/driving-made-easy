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
            const int pad = 12;
            if (coach != null && coach.Session != null)
            {
                var scoreStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 13, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(Screen.width - 134, pad, 122, 26),
                    $"Safety: {coach.Session.SafetyScore}", scoreStyle);

                var statusStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 11, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(Screen.width - 134, pad + 30, 122, 22),
                    coach.StatusLine(trackedRuleId), statusStyle);
            }

            // Active posted speed limit (top-left, below the speedometer).
            if (coach != null && coach.CurrentSpeedLimitMph > 0)
            {
                var limitStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 13, alignment = TextAnchor.MiddleCenter };
                GUI.Box(new Rect(pad, 48, 92, 26),
                    $"Limit {coach.CurrentSpeedLimitMph}", limitStyle);
            }

            if (Time.time < _bannerUntil && !string.IsNullOrEmpty(_banner))
            {
                var bannerStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 14, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                float w = Mathf.Min(520f, Screen.width - 280f);
                GUI.Box(new Rect((Screen.width - w) / 2f, pad, w, 48f), _banner, bannerStyle);
            }

            if (Time.time < _toastUntil && !string.IsNullOrEmpty(_toast))
            {
                Color prev = GUI.color;
                GUI.color = _toastIsPenalty
                    ? new Color(1f, 0.6f, 0.6f)
                    : new Color(0.6f, 1f, 0.6f);
                var toastStyle = new GUIStyle(GUI.skin.box)
                    { fontSize = 15, alignment = TextAnchor.MiddleCenter, wordWrap = true };
                float w = Mathf.Min(460f, Screen.width - 280f);
                GUI.Box(new Rect((Screen.width - w) / 2f, Screen.height * 0.62f, w, 50f),
                    _toast, toastStyle);
                GUI.color = prev;
            }
        }
    }
}
