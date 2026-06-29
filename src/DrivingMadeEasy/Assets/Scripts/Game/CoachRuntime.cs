using System;
using UnityEngine;
using DrivingMadeEasy.Coaching;

namespace DrivingMadeEasy.Game
{
    /// <summary>
    /// Scene-side owner of the Coach (SPEC §8.2): world triggers call NotifyRelevant /
    /// NotifyEvaluated, and this routes the engine's decisions out to the HUD (and, on
    /// device, the voice) via events. The engine itself stays pure and testable.
    /// </summary>
    public class CoachRuntime : MonoBehaviour
    {
        public enum Difficulty { Gentle, Normal, Exam }
        public Difficulty difficulty = Difficulty.Normal;

        private Coach _coach;
        private PlayerProfile _profile;
        private DriveSession _session;

        /// Fired when a rule becomes relevant and the player should get a heads-up.
        public event Action<PreemptiveCue> Preempt;
        /// Fired when an encounter is decided (guidance / silence / ticket).
        public event Action<CoachOutcome> Outcome;

        public DriveSession Session => _session;

        private void Awake()
        {
            _profile = new PlayerProfile();
            _session = new DriveSession();

            DifficultySettings diff;
            switch (difficulty)
            {
                case Difficulty.Gentle: diff = DifficultySettings.Gentle; break;
                case Difficulty.Exam: diff = DifficultySettings.Exam; break;
                default: diff = DifficultySettings.Normal; break;
            }
            _coach = new Coach(CaliforniaRules.BuildRegistry(), _profile, diff);
        }

        public void NotifyRelevant(string ruleId)
        {
            var cue = _coach.OnRelevant(ruleId);
            if (cue.ShouldShow) Preempt?.Invoke(cue);
        }

        public void NotifyEvaluated(string ruleId, bool passed)
        {
            var outcome = _coach.OnEvaluated(ruleId, passed);
            _session.Apply(outcome);
            Outcome?.Invoke(outcome);
        }

        /// Short status line for the HUD, e.g. "stop_sign: coaching 2/3" or "...: enforced".
        public string StatusLine(string ruleId)
        {
            if (_coach != null && _coach.TryInspect(ruleId, out int c, out int free, out bool grad))
                return grad ? $"{ruleId}: enforced" : $"{ruleId}: coaching {c}/{free}";
            return string.Empty;
        }
    }
}
