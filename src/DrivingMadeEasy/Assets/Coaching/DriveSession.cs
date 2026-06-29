using System.Collections.Generic;

namespace DrivingMadeEasy.Coaching
{
    /// <summary>
    /// Per-drive scoring and the end-of-drive report (SPEC §7). One per drive: starts from
    /// a clean "license" score and is reduced by tickets. Pure data, no Unity dependency.
    /// </summary>
    public sealed class DriveSession
    {
        public int SafetyScore { get; private set; } = 100;
        public bool Failed { get; private set; }
        public readonly List<Ticket> Tickets = new List<Ticket>();
        public readonly List<string> RulesGraduated = new List<string>();

        public void Apply(CoachOutcome outcome)
        {
            if (outcome.Graduated && !RulesGraduated.Contains(outcome.RuleId))
                RulesGraduated.Add(outcome.RuleId);

            if (outcome.Action == CoachAction.IssuePenalty && outcome.Ticket != null)
            {
                Tickets.Add(outcome.Ticket);
                SafetyScore -= outcome.Ticket.Points;
                if (SafetyScore < 0) SafetyScore = 0;
                if (outcome.Ticket.InstantFail) Failed = true;
            }
        }

        public DriveReport BuildReport()
        {
            int fines = 0;
            foreach (var t in Tickets) fines += t.Fine;
            return new DriveReport
            {
                FinalScore = SafetyScore,
                Failed = Failed,
                Tickets = new List<Ticket>(Tickets),
                RulesGraduated = new List<string>(RulesGraduated),
                TotalFines = fines
            };
        }
    }

    public sealed class DriveReport
    {
        public int FinalScore;
        public bool Failed;
        public List<Ticket> Tickets;
        public List<string> RulesGraduated;
        public int TotalFines;
    }
}
