using System.Collections.Generic;

namespace DrivingMadeEasy.Coaching
{
    /// <summary>
    /// Headless scenario checks for the teach -> fade -> enforce behaviour (SPEC §8.4).
    /// Pure C#, no Unity / no play-mode, so they can run from the editor menu
    /// (Tools ▸ Driving Made Easy ▸ Run Coach Tests) or a CI harness later.
    /// </summary>
    public static class CoachScenarios
    {
        public sealed class Result
        {
            public string Name;
            public bool Passed;
            public string Detail;
        }

        public static List<Result> RunAll()
        {
            return new List<Result>
            {
                TeachThenEnforce(),
                CorrectRepsStillGraduate(),
                ExamModeEnforcesImmediately(),
                GentleModeGivesMoreReps(),
                RepeatedFailuresReCoach(),
                PreemptOnlyWhileCoaching(),
                ScoreAndTickets(),
            };
        }

        private static Coach NewCoach(DifficultySettings diff)
            => new Coach(CaliforniaRules.BuildRegistry(), new PlayerProfile(), diff);

        // First 3 mistakes are coached (no penalty); the 4th is ticketed.
        private static Result TeachThenEnforce()
        {
            var coach = NewCoach(DifficultySettings.Normal);
            for (int i = 0; i < 3; i++)
            {
                var o = coach.OnEvaluated("stop_sign", passed: false);
                if (o.Action == CoachAction.IssuePenalty)
                    return Fail("TeachThenEnforce", $"penalized on coached rep {i + 1}");
            }
            var last = coach.OnEvaluated("stop_sign", passed: false);
            return last.Action == CoachAction.IssuePenalty
                ? Pass("TeachThenEnforce")
                : Fail("TeachThenEnforce", "did not penalize after graduation");
        }

        // Doing it RIGHT three times still graduates the rule.
        private static Result CorrectRepsStillGraduate()
        {
            var coach = NewCoach(DifficultySettings.Normal);
            for (int i = 0; i < 3; i++) coach.OnEvaluated("stop_sign", passed: true);
            var o = coach.OnEvaluated("stop_sign", passed: false);
            return o.Action == CoachAction.IssuePenalty
                ? Pass("CorrectRepsStillGraduate")
                : Fail("CorrectRepsStillGraduate", "correct reps did not count toward graduation");
        }

        // Exam mode (0 free encounters): the very first mistake is ticketed.
        private static Result ExamModeEnforcesImmediately()
        {
            var coach = NewCoach(DifficultySettings.Exam);
            var o = coach.OnEvaluated("stop_sign", passed: false);
            return o.Action == CoachAction.IssuePenalty
                ? Pass("ExamModeEnforcesImmediately")
                : Fail("ExamModeEnforcesImmediately", "first mistake not penalized in Exam mode");
        }

        // Gentle mode (5 free encounters): five coached reps before enforcement.
        private static Result GentleModeGivesMoreReps()
        {
            var coach = NewCoach(DifficultySettings.Gentle);
            for (int i = 0; i < 5; i++)
            {
                var o = coach.OnEvaluated("stop_sign", passed: false);
                if (o.Action == CoachAction.IssuePenalty)
                    return Fail("GentleModeGivesMoreReps", $"penalized on coached rep {i + 1} of 5");
            }
            var last = coach.OnEvaluated("stop_sign", passed: false);
            return last.Action == CoachAction.IssuePenalty
                ? Pass("GentleModeGivesMoreReps")
                : Fail("GentleModeGivesMoreReps", "did not enforce after 5 gentle reps");
        }

        // Repeated post-graduation failures drop the rule back into coaching.
        private static Result RepeatedFailuresReCoach()
        {
            var coach = NewCoach(DifficultySettings.Normal);
            for (int i = 0; i < 3; i++) coach.OnEvaluated("stop_sign", passed: true); // graduate

            CoachOutcome o = null;
            for (int i = 0; i < 3; i++) o = coach.OnEvaluated("stop_sign", passed: false);
            if (o == null || !o.ReturnedToCoaching)
                return Fail("RepeatedFailuresReCoach", "did not return to coaching after repeated failures");

            var next = coach.OnEvaluated("stop_sign", passed: false);
            return next.Action != CoachAction.IssuePenalty
                ? Pass("RepeatedFailuresReCoach")
                : Fail("RepeatedFailuresReCoach", "still penalizing after re-coaching kicked in");
        }

        // The pre-emptive heads-up shows while coaching, and stops once graduated.
        private static Result PreemptOnlyWhileCoaching()
        {
            var coach = NewCoach(DifficultySettings.Normal);
            if (!coach.OnRelevant("stop_sign").ShouldShow)
                return Fail("PreemptOnlyWhileCoaching", "no pre-emptive cue during coaching");
            for (int i = 0; i < 3; i++) coach.OnEvaluated("stop_sign", passed: true); // graduate
            if (coach.OnRelevant("stop_sign").ShouldShow)
                return Fail("PreemptOnlyWhileCoaching", "still cueing after graduation");
            return Pass("PreemptOnlyWhileCoaching");
        }

        // Scoring: an enforced stop-sign failure deducts its points and logs a ticket.
        private static Result ScoreAndTickets()
        {
            var coach = NewCoach(DifficultySettings.Exam);
            var session = new DriveSession();
            session.Apply(coach.OnEvaluated("stop_sign", passed: false));
            if (session.Tickets.Count != 1)
                return Fail("ScoreAndTickets", "ticket not recorded");
            if (session.SafetyScore != 85)
                return Fail("ScoreAndTickets", $"score was {session.SafetyScore}, expected 85");
            return Pass("ScoreAndTickets");
        }

        private static Result Pass(string name) => new Result { Name = name, Passed = true };
        private static Result Fail(string name, string detail)
            => new Result { Name = name, Passed = false, Detail = detail };
    }
}
