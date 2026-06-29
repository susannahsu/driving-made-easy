using System.Collections.Generic;

namespace DrivingMadeEasy.Coaching
{
    /// Per-player, per-rule progress — the thing that makes the Coach adaptive and that
    /// would be persisted between drives (SPEC §8.3).
    public sealed class PlayerRuleState
    {
        public int EncounterCount;
        public bool Graduated;
        public int PostGradFailures;
    }

    public sealed class PlayerProfile
    {
        private readonly Dictionary<string, PlayerRuleState> _states =
            new Dictionary<string, PlayerRuleState>();

        public int LifetimeTickets;

        public int RulesMastered
        {
            get
            {
                int n = 0;
                foreach (var kv in _states) if (kv.Value.Graduated) n++;
                return n;
            }
        }

        public PlayerRuleState GetOrCreate(string ruleId)
        {
            if (!_states.TryGetValue(ruleId, out var st))
            {
                st = new PlayerRuleState();
                _states[ruleId] = st;
            }
            return st;
        }

        /// Read-only peek; returns null if the rule hasn't been encountered yet.
        public PlayerRuleState Peek(string ruleId)
            => _states.TryGetValue(ruleId, out var st) ? st : null;
    }

    /// Difficulty knobs (SPEC §6.1, §9). Overriding free-encounters is how Gentle / Normal
    /// / Exam modes change "how many coached reps before the training wheels come off."
    public sealed class DifficultySettings
    {
        public int? FreeEncountersOverride;

        public static DifficultySettings Normal => new DifficultySettings { FreeEncountersOverride = null };
        public static DifficultySettings Gentle => new DifficultySettings { FreeEncountersOverride = 5 };
        public static DifficultySettings Exam => new DifficultySettings { FreeEncountersOverride = 0 };
    }
}
