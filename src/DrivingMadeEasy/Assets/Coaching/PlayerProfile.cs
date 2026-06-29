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

        // ---- Persistence (plain DTOs; the actual file I/O lives in the Unity layer) ----

        [System.Serializable]
        public sealed class RuleStateDto
        {
            public string ruleId;
            public int encounterCount;
            public bool graduated;
            public int postGradFailures;
        }

        [System.Serializable]
        public sealed class ProfileDto
        {
            public int lifetimeTickets;
            public List<RuleStateDto> rules = new List<RuleStateDto>();
        }

        public ProfileDto ToDto()
        {
            var dto = new ProfileDto { lifetimeTickets = LifetimeTickets };
            foreach (var kv in _states)
            {
                dto.rules.Add(new RuleStateDto
                {
                    ruleId = kv.Key,
                    encounterCount = kv.Value.EncounterCount,
                    graduated = kv.Value.Graduated,
                    postGradFailures = kv.Value.PostGradFailures
                });
            }
            return dto;
        }

        public static PlayerProfile FromDto(ProfileDto dto)
        {
            var p = new PlayerProfile();
            if (dto == null) return p;
            p.LifetimeTickets = dto.lifetimeTickets;
            if (dto.rules != null)
            {
                foreach (var rs in dto.rules)
                {
                    if (string.IsNullOrEmpty(rs.ruleId)) continue;
                    var st = p.GetOrCreate(rs.ruleId);
                    st.EncounterCount = rs.encounterCount;
                    st.Graduated = rs.graduated;
                    st.PostGradFailures = rs.postGradFailures;
                }
            }
            return p;
        }
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
