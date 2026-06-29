using System.Collections.Generic;

namespace DrivingMadeEasy.Coaching
{
    // ---------------------------------------------------------------------------------
    // The data model for the Coach. Deliberately free of any UnityEngine dependency so
    // the whole teach -> fade -> enforce engine can be unit-tested headlessly (SPEC §8.4).
    // ---------------------------------------------------------------------------------

    public enum RuleSeverity { Minor, Major, Severe }

    /// What the Coach decides to do when an encounter is evaluated.
    public enum CoachAction { ShowGuidance, StaySilent, IssuePenalty }

    public sealed class RulePenalty
    {
        public int Points;
        public int Fine;
        public bool InstantFail;

        public RulePenalty(int points, int fine, bool instantFail = false)
        {
            Points = points;
            Fine = fine;
            InstantFail = instantFail;
        }
    }

    /// All the things the Coach might say about a rule, at the right moment.
    public sealed class RuleGuidance
    {
        public string Preempt;    // heads-up BEFORE the decision (coaching phase only)
        public string Correction; // gentle fix after a coached mistake
        public string Encourage;  // positive note after a coached success
        public string PostHoc;    // explanation shown with a ticket (after graduation)
        public IReadOnlyList<string> Highlight; // world objects to highlight while coaching

        public RuleGuidance(string preempt, string correction, string encourage,
                            string postHoc, params string[] highlight)
        {
            Preempt = preempt;
            Correction = correction;
            Encourage = encourage;
            PostHoc = postHoc;
            Highlight = highlight;
        }
    }

    /// A single teachable/enforceable element of driving, authored once (SPEC §6.2, §8.3).
    public sealed class RuleDefinition
    {
        public string Id;
        public string Category;
        public string Title;
        public RuleSeverity Severity;
        public RulePenalty Penalty;
        public RuleGuidance Guidance;
        public int FreeEncounters;
        public int ReCoachAfterFailures;

        public RuleDefinition(string id, string category, string title, RuleSeverity severity,
                              RulePenalty penalty, RuleGuidance guidance,
                              int freeEncounters = 3, int reCoachAfterFailures = 3)
        {
            Id = id;
            Category = category;
            Title = title;
            Severity = severity;
            Penalty = penalty;
            Guidance = guidance;
            FreeEncounters = freeEncounters;
            ReCoachAfterFailures = reCoachAfterFailures;
        }
    }

    /// Returned when a rule becomes relevant: whether to show the pre-emptive heads-up.
    public struct PreemptiveCue
    {
        public bool ShouldShow;
        public string Message;
        public IReadOnlyList<string> Highlight;

        public static PreemptiveCue None => new PreemptiveCue { ShouldShow = false };
    }

    /// The result of evaluating a completed rule encounter.
    public sealed class CoachOutcome
    {
        public string RuleId;
        public CoachAction Action;
        public bool Passed;
        public bool WasCoachingPhase;
        public bool Graduated;          // this evaluation moved the rule to "enforced"
        public bool ReturnedToCoaching; // spaced-repetition re-coaching kicked in
        public string Message;          // correction / encouragement / ticket explanation
        public Ticket Ticket;           // non-null only when Action == IssuePenalty
    }

    public sealed class Ticket
    {
        public string RuleId;
        public string Title;
        public RuleSeverity Severity;
        public int Points;
        public int Fine;
        public bool InstantFail;
        public string Explanation;
    }
}
