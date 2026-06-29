namespace DrivingMadeEasy.Coaching
{
    /// <summary>
    /// The starter California rule set. v1 ships CA (SPEC §12 decision); the registry is
    /// built per-state so future versions swap in another state's data. Fines are
    /// illustrative play-money values, loosely modeled on CA base + penalty assessments.
    /// See docs/rules-catalog.md for the full catalog this draws from.
    /// </summary>
    public static class CaliforniaRules
    {
        public static RuleRegistry BuildRegistry()
        {
            var r = new RuleRegistry();

            r.Add(new RuleDefinition(
                id: "stop_sign", category: "right_of_way", title: "Stop sign",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 15, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Stop sign ahead — come to a complete stop behind the white line, then go when it's clear.",
                    correction: "That was a rolling stop. Next time stop fully — count \"one-one-thousand\" before you go.",
                    encourage: "Nice — a full, complete stop. That's exactly right.",
                    postHoc: "Rolling stop: you didn't come to a complete stop behind the line. In California that's a citation.",
                    "stop_sign", "stop_line")));

            r.Add(new RuleDefinition(
                id: "speed_limit", category: "speed", title: "Speed limit",
                severity: RuleSeverity.Minor,
                penalty: new RulePenalty(points: 10, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Speed limit posted ahead — ease off and keep at or below it.",
                    correction: "You drifted over the limit there. Lift off the gas a little sooner.",
                    encourage: "Good — nicely within the limit.",
                    postHoc: "Exceeded the posted speed limit.",
                    "speed_limit_sign")));

            r.Add(new RuleDefinition(
                id: "turn_signal", category: "signaling", title: "Turn signal",
                severity: RuleSeverity.Minor,
                penalty: new RulePenalty(points: 5, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Turn coming up — signal at least 100 feet before you turn.",
                    correction: "Don't forget the signal next time — flick it on well before the turn.",
                    encourage: "Good, signaled in plenty of time.",
                    postHoc: "Turned or changed lanes without signaling in time.")));

            r.Add(new RuleDefinition(
                id: "yield", category: "right_of_way", title: "Yield",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 15, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Yield ahead — slow down and give way to crossing traffic; stop only if you need to.",
                    correction: "You needed to give way there. Slow earlier and let the other car go first.",
                    encourage: "Good — you yielded and let them through.",
                    postHoc: "Failure to yield the right-of-way.",
                    "yield_sign")));

            return r;
        }
    }
}
