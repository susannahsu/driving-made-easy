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
                id: "pedestrian_crosswalk", category: "right_of_way", title: "Crosswalk",
                severity: RuleSeverity.Severe,
                penalty: new RulePenalty(points: 25, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Pedestrian at the crosswalk — slow down and let them cross.",
                    correction: "You needed to yield there. Slow right down whenever someone's in the crosswalk.",
                    encourage: "Good — you slowed and let them cross safely.",
                    postHoc: "Failure to yield to a pedestrian in a crosswalk.",
                    "crosswalk")));

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

            r.Add(new RuleDefinition(
                id: "traffic_light", category: "right_of_way", title: "Traffic light",
                severity: RuleSeverity.Severe,
                penalty: new RulePenalty(points: 25, fine: 490),
                guidance: new RuleGuidance(
                    preempt: "Red light ahead — come to a complete stop before the line and wait for green.",
                    correction: "You went on red — always stop and wait for the green.",
                    encourage: "Good — you stopped and waited for green.",
                    postHoc: "Ran a red light.",
                    "traffic_light", "stop_line")));

            r.Add(new RuleDefinition(
                id: "school_zone", category: "speed", title: "School zone",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 15, fine: 350),
                guidance: new RuleGuidance(
                    preempt: "School zone — slow to 15 and watch for children.",
                    correction: "Too fast for a school zone — ease down to 15 through here.",
                    encourage: "Good — nice and slow through the school zone.",
                    postHoc: "Exceeded the school-zone speed limit.",
                    "school_zone_sign")));

            r.Add(new RuleDefinition(
                id: "lane_keeping", category: "lane", title: "Lane keeping",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 10, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Keep to your lane — don't let the car drift across the lines.",
                    correction: "You drifted out of your lane — keep the car centered.",
                    encourage: "Good lane discipline.",
                    postHoc: "Drifted out of the lane / off the road.")));

            r.Add(new RuleDefinition(
                id: "four_way_stop", category: "right_of_way", title: "Four-way stop",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 15, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Four-way stop — stop completely, and let cross traffic clear before you go.",
                    correction: "At a four-way stop, come to a full stop and wait your turn — don't go while another car is crossing.",
                    encourage: "Good — full stop and you waited your turn.",
                    postHoc: "Four-way stop: rolled it or failed to yield to cross traffic.",
                    "stop_sign", "stop_line")));

            r.Add(new RuleDefinition(
                id: "following_distance", category: "lane", title: "Following distance",
                severity: RuleSeverity.Major,
                penalty: new RulePenalty(points: 10, fine: 238),
                guidance: new RuleGuidance(
                    preempt: "Keep back — leave a safe gap to the car ahead.",
                    correction: "You were tailgating — drop back to about a two-second gap.",
                    encourage: "Good — safe following distance.",
                    postHoc: "Followed too closely (tailgating).")));

            return r;
        }
    }
}
