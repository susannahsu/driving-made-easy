# Lessons & Progression

How the player advances from an empty parking lot to merging onto a highway. Each lesson
introduces a focused set of rules (see [`rules-catalog.md`](./rules-catalog.md)); the
**Coach** teaches each new rule for its first 3 encounters, then enforces it
(see [`../SPEC.md`](../SPEC.md) §6). Lessons unlock in order, gated by a passing score.

```
Lesson 0 ──▶ Lesson 1 ──▶ Lesson 2 ──▶ Lesson 3 ──▶ Road Test
Controls     Residential   Town &        Highway       (capstone,
& parking    streets       intersections               no coaching)
```

After passing a lesson, its environment unlocks for **Free Drive** (open practice with
full enforcement).

---

## Lesson 0 — Controls & Calibration
**Environment:** empty parking lot · **Coaching:** maximal · **Enforcement:** none

Goal: get comfortable holding the phone as a wheel.
- Calibrate the wheel (neutral pose → center).
- Steering feel: slalom between cones, figure-eight.
- Gas / brake control: smooth starts and stops, hit a target stop line.
- Reverse and `parking` into a marked space.
- `seatbelt` ritual before moving.

**Pass:** complete each maneuver without hitting cones; demonstrate a controlled stop.

## Lesson 1 — Residential Streets
**Environment:** quiet residential street · **Coaching:** on · **Enforcement:** begins after 3 reps/rule

Introduces: `stop_sign`, `speed_limit`, `turn_signal`, `pedestrian_crosswalk`,
`lane_keeping`.
- Full stops at stop signs; obey the posted limit.
- Signal before every turn; auto-cancel after.
- Yield to pedestrians; watch driveways.

**Pass:** safety score ≥ threshold; each introduced rule reached at least graduation
(3 reps) with no severe violations.

## Lesson 2 — Town & Intersections
**Environment:** town grid · **Coaching:** on · **Enforcement:** active for graduated rules

Introduces: `traffic_light_red`/`_yellow`, `four_way_stop`, `right_on_red`,
`turn_from_correct_lane`, `school_zone`, `lane_change`.
- Obey signals; handle protected vs. unprotected turns.
- Right-of-way at 4-way stops (first-come, ties yield right).
- Slow for the school zone; pick the correct turn lane.

**Pass:** no red-light runs or pedestrian incidents; right-of-way rules graduated;
score ≥ threshold.

## Lesson 3 — Highway
**Environment:** on-ramp + freeway · **Coaching:** on · **Enforcement:** active

Introduces: `highway_merge`, `following_distance`, `no_passing`, exit handling,
higher-speed `lane_change`.
- Match ramp speed, signal, merge into a gap (gap highlighted during coached reps).
- Maintain a ≥ ~2s following distance.
- Change lanes with mirror/blind-spot checks; take the correct exit.

**Pass:** clean merge and exit; following distance maintained; score ≥ threshold.

## Road Test (capstone)
**Environment:** mixed route across all environments · **Coaching:** OFF · **Enforcement:** full

A graded evaluation modeled loosely on a DMV road-test rubric — zero free encounters,
every rule live. Produces a pass/fail plus a scored report. This is the "graduation"
that signals real-world readiness to *practice* on a real road (not a certification).

---

## Difficulty modifiers (apply across lessons)

- **Gentle:** 5 free encounters/rule, lighter traffic, wider timing windows.
- **Normal:** default (3 free encounters).
- **Exam:** 0 free encounters, denser traffic, stricter windows — for players prepping
  for a real test.
- **Conditions:** time-of-day (day/dusk/night) and weather (clear/rain) can be layered
  onto unlocked lessons, activating `headlights_at_night` / `wipers_in_rain`.
