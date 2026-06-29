using System.Collections.Generic;

namespace DrivingMadeEasy.Coaching
{
    public sealed class RuleRegistry
    {
        private readonly Dictionary<string, RuleDefinition> _rules =
            new Dictionary<string, RuleDefinition>();

        public void Add(RuleDefinition rule) => _rules[rule.Id] = rule;
        public RuleDefinition Get(string id) => _rules.TryGetValue(id, out var r) ? r : null;
        public bool Has(string id) => _rules.ContainsKey(id);
        public IEnumerable<RuleDefinition> All => _rules.Values;
    }

    /// <summary>
    /// The teach -> fade -> enforce engine (SPEC §6). Pure logic, no Unity dependency, so
    /// it is fully unit-testable. Every decision is a deterministic function of
    /// (rule, player rule state, pass/fail, difficulty):
    ///
    ///   • encounters 1..N (N = free encounters): COACHED — guidance, never a penalty.
    ///   • encounter N+1 onward: ENFORCED — silent on success, ticket on failure.
    ///   • a correct rep still counts toward graduating a rule.
    ///   • too many post-graduation failures drop the rule back into coaching.
    /// </summary>
    public sealed class Coach
    {
        private readonly RuleRegistry _rules;
        private readonly PlayerProfile _profile;
        private readonly DifficultySettings _difficulty;

        public Coach(RuleRegistry rules, PlayerProfile profile, DifficultySettings difficulty)
        {
            _rules = rules;
            _profile = profile;
            _difficulty = difficulty;
        }

        public int EffectiveFreeEncounters(RuleDefinition rule)
            => _difficulty.FreeEncountersOverride ?? rule.FreeEncounters;

        public bool IsCoaching(string ruleId)
        {
            var rule = _rules.Get(ruleId);
            if (rule == null) return false;
            var st = _profile.Peek(ruleId);
            int count = st?.EncounterCount ?? 0;
            bool graduated = st?.Graduated ?? false;
            return !graduated && count < EffectiveFreeEncounters(rule);
        }

        /// Snapshot of a rule's coaching progress, for HUD/debug. Returns false if unknown.
        public bool TryInspect(string ruleId, out int count, out int free, out bool graduated)
        {
            var rule = _rules.Get(ruleId);
            if (rule == null) { count = 0; free = 0; graduated = false; return false; }
            var st = _profile.Peek(ruleId);
            count = st?.EncounterCount ?? 0;
            free = EffectiveFreeEncounters(rule);
            graduated = st?.Graduated ?? false;
            return true;
        }

        /// Call when a rule becomes relevant (the player is approaching the decision point).
        public PreemptiveCue OnRelevant(string ruleId)
        {
            var rule = _rules.Get(ruleId);
            if (rule == null || !IsCoaching(ruleId)) return PreemptiveCue.None;
            return new PreemptiveCue
            {
                ShouldShow = true,
                Message = rule.Guidance.Preempt,
                Highlight = rule.Guidance.Highlight
            };
        }

        /// Call when the encounter is decided (the player passed or failed the rule).
        public CoachOutcome OnEvaluated(string ruleId, bool passed)
        {
            var rule = _rules.Get(ruleId);
            var outcome = new CoachOutcome { RuleId = ruleId, Passed = passed };
            if (rule == null) { outcome.Action = CoachAction.StaySilent; return outcome; }

            var st = _profile.GetOrCreate(ruleId);
            bool coaching = IsCoaching(ruleId); // evaluate BEFORE we bump the count
            outcome.WasCoachingPhase = coaching;

            st.EncounterCount++;

            if (coaching)
            {
                // Teaching mode: never penalize. Encourage on success, correct on mistake.
                outcome.Action = CoachAction.ShowGuidance;
                outcome.Message = passed ? rule.Guidance.Encourage : rule.Guidance.Correction;

                if (st.EncounterCount >= EffectiveFreeEncounters(rule))
                {
                    st.Graduated = true;
                    outcome.Graduated = true;
                }
            }
            else if (passed)
            {
                // Enforced and correct: stay out of the way.
                outcome.Action = CoachAction.StaySilent;
            }
            else
            {
                // Enforced and wrong: ticket, with the explanation deferred to the report.
                outcome.Action = CoachAction.IssuePenalty;
                outcome.Message = rule.Guidance.PostHoc;
                outcome.Ticket = new Ticket
                {
                    RuleId = rule.Id,
                    Title = rule.Title,
                    Severity = rule.Severity,
                    Points = rule.Penalty.Points,
                    Fine = rule.Penalty.Fine,
                    InstantFail = rule.Penalty.InstantFail,
                    Explanation = rule.Guidance.PostHoc
                };

                st.PostGradFailures++;
                _profile.LifetimeTickets++;

                if (st.PostGradFailures >= rule.ReCoachAfterFailures)
                {
                    // Spaced repetition: a rule the player keeps failing gets re-taught.
                    st.Graduated = false;
                    st.EncounterCount = 0;
                    st.PostGradFailures = 0;
                    outcome.ReturnedToCoaching = true;
                }
            }

            return outcome;
        }
    }
}
