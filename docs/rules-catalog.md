# Rules & Signs Catalog

The set of traffic rules and signs the **Coach** teaches (first 3 encounters) and then
enforces (4th onward) — see [`../SPEC.md`](../SPEC.md) §6. Each rule is data-driven; the
fields below mirror the Rule schema in SPEC §8.3.

> **Region note:** v1 ships **California** (US, right-hand traffic). Signs/colors and a
> few right-of-way edge cases change by state, so the catalog is structured as a
> **swappable per-state data set** — future versions let the player **select their
> state** and load the matching rules. The rules below reflect California conventions
> (e.g. right-on-red permitted after a full stop unless posted otherwise; school-zone
> 25 mph when children are present).

## Legend

- **Severity:** `minor` (small points) · `major` (points + ticket/fine) · `severe`
  (big points + ticket, possible instant-fail + replay).
- **Free encounters:** coached reps before enforcement kicks in (default 3).

---

## v1 rules

### Right of way & stopping

| id | What the player must do | Severity | Notes |
| --- | --- | --- | --- |
| `stop_sign` | Full stop behind the line, hold ~1s, proceed when clear. | major | Rolling stops are the classic fail. |
| `yield_sign` | Slow, give way to crossing/oncoming traffic, stop only if needed. | major | Judged on whether right-of-way was actually ceded. |
| `four_way_stop` | Stop; yield to whoever arrived first; ties → yield to the right. | major | Right-of-way ordering is the teachable bit. |
| `traffic_light_red` | Stop fully before the line; remain until green. | severe | Running a red at speed = instant-fail + replay. |
| `traffic_light_yellow` | Stop if safe; don't accelerate to "beat" it. | minor | Coached toward caution, not punished for safe clearing. |
| `right_on_red` | Full stop first, yield, then turn (where permitted; obey "No Turn on Red"). | major | Region-dependent. |
| `pedestrian_crosswalk` | Yield/stop for pedestrians in or entering the crosswalk. | severe | Hitting a pedestrian = instant-fail + replay. |

### Speed & lane discipline

| id | What the player must do | Severity | Notes |
| --- | --- | --- | --- |
| `speed_limit` | Keep speed ≤ posted limit (and reasonable for conditions). | minor→major | Severity scales with how far over. |
| `school_zone` | Reduce to school-zone limit during posted hours; watch for kids. | major | Combines a speed rule with a hazard. |
| `lane_keeping` | Stay within the lane; no straddling/drifting. | major | Haptic rumble-strip feedback when drifting. |
| `following_distance` | Maintain a safe gap (≥ ~2s) to the car ahead. | major | Especially enforced on the highway. |
| `no_passing` | Don't pass over a solid line / in no-passing zones. | major | |

### Turns, signaling & maneuvers

| id | What the player must do | Severity | Notes |
| --- | --- | --- | --- |
| `turn_signal` | Signal ≥ ~30 m / 3s before a turn or lane change. | minor | Auto-cancel after the maneuver. |
| `lane_change` | Signal, check mirror/blind spot, change only when clear. | major | Mirror-check is part of the predicate. |
| `highway_merge` | Match traffic speed on the ramp, signal, merge into a gap. | major | Highlight the target gap during coached reps. |
| `turn_from_correct_lane` | Use the proper lane to turn (left from left, etc.). | minor | |
| `no_u_turn` | Don't U-turn where prohibited. | major | Sign-gated. |

### Vehicle operation (context lessons)

| id | What the player must do | Severity | Notes |
| --- | --- | --- | --- |
| `headlights_at_night` | Headlights on at dusk/night/rain. | minor | Triggers in night/rain lessons. |
| `wipers_in_rain` | Wipers on in rain for visibility. | minor | Rain lessons. |
| `seatbelt` | "Buckle up" before the drive starts. | minor | One-time pre-drive check / ritual. |
| `parking` | Park within the space; don't hit cones/cars; correct reverse use. | major | Parking-lot lesson; parallel parking post-v1. |

---

## Authoring checklist (per rule)

When adding a rule, define all of: `id`, `category`, `relevance` trigger,
`correctBehavior` predicate, `evaluationWindow`, `guidance` (preempt + correction +
highlight), `severity`, `penalty`, `freeEncounters`, `reCoachAfterFailures`. Add a
headless scenario test (SPEC §8.4) covering: relevance fires correctly, a correct rep
counts toward graduation, and a post-graduation failure produces the right ticket.

## Post-v1 candidates

Roundabouts, parallel parking, emergency-vehicle yielding, railroad crossings, one-way
streets, HOV/carpool lanes, roadwork zones, weather-adjusted speed, hand signals,
multi-lane roundabouts, and manual-transmission operation.
