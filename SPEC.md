# Driving Made Easy — Product & Technical Specification

**Version:** 0.1 (draft)
**Status:** Planning
**Last updated:** 2026-06-27

---

## 1. Vision

A mobile driving **simulator** for licensed-but-rusty drivers who want to rebuild
their confidence and habits before getting back behind a real wheel. The player holds
their iPhone like a steering wheel and tilts to steer through realistic environments —
city streets, intersections, and highways — full of live traffic, pedestrians, traffic
signals, and posted signs.

What makes it different from an arcade racer is the **Coach**: an adaptive teaching
system that gives friendly, just-in-time guidance the first few times you meet a new
sign or situation, then steps back and lets real consequences (tickets, points
deductions) do the teaching. The north-star feeling is *"this is as close to real
driving as a game can get, and it made me a safer driver."*

### Who it's for

- **Primary:** licensed drivers who haven't driven in months/years and feel anxious.
- **Secondary:** new drivers studying for a road test; anyone wanting low-stakes
  practice of specific maneuvers (4-way stops, merging, parallel parking).

### Why now / why this approach

Driving anxiety is real and there's no safe, repeatable, zero-cost way to rehearse the
*decision-making* parts of driving. A phone-as-wheel simulator removes every real-world
risk while keeping the parts that matter: reading the road, obeying rules, and building
muscle memory for the right reactions.

---

## 2. Goals & non-goals

### Goals

1. **Faithful rules-of-the-road practice.** Signs, signals, right-of-way, speed limits,
   lane discipline, and signaling are modeled accurately and enforced consistently.
2. **The Coach.** Adaptive guidance: teach → fade → enforce. (See §6 — this is the
   product's heart.)
3. **Realism within reach.** Believable vehicle handling, traffic behavior, and
   environments — "convincing," not "research-grade physics."
4. **Transfer.** Habits built in-game (full stops, mirror/signal checks, following
   distance) should map directly to real driving.
5. **Confidence over score.** The emotional goal is reduced anxiety, so tone is
   supportive and progress is visible.

### Non-goals (at least for v1)

- Not a multiplayer or online-competitive racing game.
- Not a photoreal AAA simulator; no licensed real cars or real cities.
- Not a replacement for a licensed instructor or a real road test.
- Not region-exhaustive: v1 ships **one rule set** (US right-hand-drive, a single
  state's conventions) with the architecture built so other regions can be added later.
- No VR headset support in v1 (designed so it's not precluded).

---

## 3. Platform & control scheme

### 3.1 Device & rendering

- **Platform:** iOS (iPhone), portrait-agnostic but **landscape** is the primary
  play orientation. Target iPhone 12 and newer; iOS 17+.
- **Rendering happens on the phone.** The phone is *both* the steering wheel and the
  screen — you hold it in landscape, tilt to steer, and watch the road on the same
  display (the established mobile pattern, à la tilt-steer kart games).
- **Companion big-screen mode (stretch, post-v1):** AirPlay / cast the game view to a
  TV or Mac while the phone stays in hand as a pure controller, for a more immersive,
  "sit back" feel. Designed for but not built in v1.

### 3.2 Steering — the iPhone as a wheel

Steering uses **CoreMotion** (device attitude / gyroscope + accelerometer fusion):

- Hold the phone in landscape. Its **roll** angle maps to the steering angle.
- **Calibration:** at the start of every drive (and on demand) the player holds a
  neutral "hands at 9-and-3" pose and taps *Center*. That pose becomes 0° steering.
- **Sensitivity curve:** non-linear mapping so small tilts give fine control near
  center and larger tilts give quicker turns — tunable in settings (Light / Normal /
  Sport). A configurable max roll (~75°) corresponds to full lock.
- **Dead zone** around center to avoid jitter; smoothing filter on the raw signal.
- **Return-to-center assist** (optional, on by default for beginners): mild
  auto-centering that mimics a real wheel's caster, so letting the phone level out
  straightens the car.

### 3.3 Pedals & gear

Default scheme (**"Comfort"**):

- **Gas** and **Brake** are on-screen touch zones reachable by the thumbs while the
  phone is held wheel-style (gas right-thumb, brake left-thumb). Pressure → analog via
  press duration / a vertical slide, with simpler tap-and-hold fallback.
- **Automatic transmission** only in v1 (no clutch). Reverse engaged via an on-screen
  **R** toggle (used for parking lessons).
- **Turn signals:** swipe-left / swipe-right gestures on the edge of the screen, or
  dedicated on-screen blinker buttons. Signals **auto-cancel** after a completed turn,
  like a real car.
- **Horn, headlights, wipers:** secondary on-screen controls (used by specific
  lessons — e.g. headlights at night, wipers in rain).

Alternate schemes offered in settings:

- **"Tilt-throttle"** — tilt the phone forward/back (pitch) for gas/brake, freeing the
  thumbs. More immersive, harder to master.
- **"Assisted"** — auto-throttle to the speed limit, player only steers and brakes.
  Good for the very first lessons and for anxious players.

### 3.4 Accessibility

- Full alternative control mapping (on-screen virtual wheel for players who can't tilt).
- Haptic feedback for lane departure, rumble strips, collisions, and ticket events.
- Colorblind-safe sign/signal palettes; captions for all spoken Coach audio.
- Adjustable difficulty of traffic density and reaction-time windows.

---

## 4. Core gameplay loop

```
        ┌─────────────────────────────────────────────┐
        │                                             │
        ▼                                             │
   Pick a Lesson  →  Calibrate wheel  →  DRIVE  ──┐    │
   (or Free Drive)                                │    │
                                                  ▼    │
                          Coach watches every rule trigger
                                                  │
                    ┌─────────────┬───────────────┴──────────┐
                    ▼             ▼                          ▼
            new rule (<3x)   known rule, correct      known rule, wrong
            → live guidance   → silent, points kept   → TICKET / points lost
                    │             │                          │
                    └─────────────┴──────────────┬───────────┘
                                                 ▼
                                          End-of-drive report
                                  (score, tickets, what you learned,
                                   what to practice next)  ───────────┘
```

Moment-to-moment, the player is **driving and continuously being evaluated** against a
live set of rules relevant to their current context (road type, nearby signs, signals,
traffic). The Coach decides whether each evaluation produces *guidance*, *silence*, or
a *penalty* (§6). Each drive ends with a report that drives progression (§9).

---

## 5. Environments & realism

### 5.1 Environments (v1)

Built as modular, hand-authored "courses" rather than an open world:

1. **Empty parking lot** — controls tutorial, calibration, basic maneuvers, parking.
2. **Quiet residential street** — stop signs, speed limits, pedestrians, driveways.
3. **Town with intersections** — traffic lights, 4-way stops, turn lanes, crosswalks,
   school zone, right-of-way scenarios.
4. **Highway on-ramp & freeway** — merging, lane changes, following distance, exits,
   speed management.

Environments are stitched into **routes** for lessons; a larger free-roam map is a
post-v1 goal.

### 5.2 What "realistic" means here

| System | v1 target |
| --- | --- |
| **Vehicle physics** | Per-wheel model (suspension, grip, weight transfer, understeer/oversteer in extremis). Believable braking distances and momentum. Not a tire-temperature sim. |
| **Traffic AI** | Cars follow lanes, obey lights/signs, keep following distance, yield, change lanes, and *react to the player* (brake/honk if the player does something dangerous). |
| **Pedestrians** | Use crosswalks, wait for signals, occasionally jaywalk (scripted hazard events). |
| **Signs & signals** | 3D world objects with real placement and meaning; signals run real phase timing (green/yellow/red, protected turns). |
| **Speed limits** | Per road-segment; posted on signs and enforced. |
| **Time & weather** | Day/dusk/night and clear/rain as lesson modifiers (affect headlights, visibility, stopping distance). Snow/fog post-v1. |
| **Camera** | Selectable: cockpit/dashboard view (most realistic) and chase view. Mirrors / rear view available for lane-change checks. |
| **Audio** | Engine, tire, ambient traffic, turn-signal tick, horn, plus Coach voice. |

### 5.3 Failure & safety states

- **Collision** (with car, pedestrian, or property) → drive may end or continue
  depending on severity; always logged and explained.
- **Dangerous events** (running a red, wrong-way, far over the limit) → immediate
  ticket and, for severe cases, an instant lesson-fail with a replay.
- **Replays:** the moment leading up to a major mistake can be replayed from a
  bird's-eye view in the end report so the player sees what went wrong.

---

## 6. The Coach (the core differentiator)

The Coach is an adaptive teaching engine layered over a catalog of **Rules**. Every
teachable element of driving — a stop sign, a yield, the speed limit, signaling before
a turn, following distance, a school zone — is a Rule.

### 6.1 The teach → fade → enforce model

Each Rule is tracked **per player** with an `encounterCount`. Behavior depends on it:

| Encounter | Coach behavior | Penalty on mistake? |
| --- | --- | --- |
| **1st, 2nd, 3rd** time the rule is *relevant* | **Pre-emptive guidance**: a friendly heads-up *before* the decision point ("Stop sign ahead — come to a complete stop behind the white line, then go when it's clear"). If the player still does it wrong, gentle in-the-moment correction. | **No.** Teaching mode — mistakes are coached, not punished. |
| **4th time onward** | **Silent.** No hint. The Coach just watches. | **Yes.** A mistake → ticket and/or points deduction, with a short *post-hoc* explanation in the end report. |

> The "3 free encounters" count is the default and is configurable per difficulty
> (e.g. an "Exam" mode with 0 free encounters; a "Gentle" mode with 5).

Key design rules:

- The counter advances **per distinct rule**, not globally — so meeting your *first*
  highway merge still gives you 3 coached reps even if you're a veteran at stop signs.
- An encounter only "counts" when the rule was genuinely **relevant and the player had a
  fair chance to act** (it doesn't burn a free rep on a sign the player couldn't see).
- **Correct execution during coached reps still counts** toward the 3 — you can
  "graduate" a rule by doing it right, not just by being exposed to it.
- A rule the player keeps failing after graduation can be **re-coached**: after N
  post-graduation failures it temporarily re-enters teaching mode (spaced repetition).

### 6.2 Anatomy of a Rule

Each Rule is a data-driven object (see §8 for the schema). Conceptually it has:

- **Identity & category** (e.g. `stop_sign`, category `right_of_way`).
- **Relevance trigger** — the world condition that makes it apply (entering a stop
  sign's approach zone; speed-limit sign passed; nearing a crosswalk with a pedestrian).
- **Correct-behavior predicate** — what the player must do (full stop within the box;
  speed ≤ limit; signal ≥ X meters before the turn; yield to the car on the right).
- **Evaluation window** — when/where success or failure is judged.
- **Guidance content** — the pre-emptive heads-up + the gentle correction (text + voice
  + optional on-screen highlight of the sign/line).
- **Severity & penalty** — points deducted and/or fine, plus whether a severe version
  is an instant fail (e.g. running a red at speed).
- **Re-coach policy** — failures-before-re-teaching.

### 6.3 Guidance delivery

- **Spoken + captioned**, in a calm coach voice, timed to arrive *before* the decision
  (e.g. ~3 seconds out), never mid-maneuver where it would distract.
- **Optional visual cues** during coached reps only: a soft highlight on the relevant
  sign, the stop line, or the gap to merge into. These disappear once a rule graduates.
- **Never nag.** At most one active guidance prompt at a time; the Coach prioritizes the
  most imminent / highest-severity rule and queues or drops the rest.

### 6.4 Tickets & the post-hoc explanation

When a graduated rule is broken:

- A brief, non-blocking **ticket toast** appears ("🚩 Rolling stop — did not come to a
  full stop. −15 pts").
- The full *why* is saved for the **end-of-drive report**, with the replay clip and a
  reminder of the correct action — so the player isn't lectured mid-drive but always
  learns afterward.

### 6.5 Why this works

It mirrors how a good human instructor operates: explain the new thing a few times,
then let you try it for real with stakes, then review the mistakes. The fading guidance
prevents dependence, and the deferred explanations keep the driving immersive.

---

## 7. Scoring, tickets & progression economy

### 7.1 Per-drive scoring

- Each drive starts at a baseline **safety score** (e.g. 100, framed like a "clean
  license").
- Graduated-rule violations deduct **points** scaled by severity; severe violations also
  issue a **ticket** with a (play-money) **fine**.
- Smooth, rule-abiding driving earns small positive feedback (no over-gamified combo
  meters — the tone stays "competent adult driver," not "arcade").
- A drive's report shows: final score, tickets, rules graduated this drive, rules still
  in coaching, and **"practice next"** suggestions.

### 7.2 Meta-progression

- **Lessons** unlock in sequence (parking lot → residential → town → highway), each
  gated by demonstrating the prerequisite rules at a passing score (§ `docs/lessons.md`).
- A persistent **"driving record"** profile shows lifetime tickets, rules mastered, and
  a confidence trend over time.
- **Free Drive** unlocks per environment once its lesson is passed, for open practice
  with full enforcement.
- Optional **"Road Test"** mode: a graded, no-coaching evaluation drive that mimics a
  real DMV road test rubric — the capstone.

### 7.3 Severity tiers (examples)

| Tier | Examples | Effect |
| --- | --- | --- |
| Minor | Late signal, mild over-limit, wide stop | small points |
| Major | Rolling a stop sign, failure to yield, lane straddling | points + ticket/fine |
| Severe | Running a red, wrong-way, hitting a pedestrian, dangerous speed | big points + ticket + possible instant fail + replay |

---

## 8. Technical architecture

### 8.1 Recommended stack

- **Engine: Unity (C#).** Rationale: mature mobile/iOS pipeline, built-in vehicle
  physics (`WheelCollider`), large asset ecosystem for cars/roads/signs (cutting art
  cost dramatically for a solo/small build), straightforward CoreMotion access, and a
  clean way to express the data-driven Rule/Coach system in C# + ScriptableObjects.
  - *Alternatives considered:* native **SwiftUI + RealityKit/SceneKit** (best OS
    integration, but far more 3D and physics work to build from scratch); **Unreal**
    (gorgeous, heavier for mobile and a steeper solo learning curve). Unity is the
    pragmatic middle for a realistic-enough mobile sim.
- **Input:** CoreMotion via Unity's input system (attitude/roll for steering).
- **Persistence:** local first (player profile, per-rule encounter counts, progress) via
  a local store; optional cloud sync (iCloud / a lightweight backend) post-v1.
- **Audio:** Unity audio + pre-recorded (or TTS-generated) Coach voice lines.

> The Rule/Coach design in §6/§8.2 is engine-agnostic — if the team chooses a native
> Swift build instead, the same data model and state machine port directly.

### 8.2 System decomposition

```
┌──────────────────────────────────────────────────────────────────┐
│                           Game App                                │
│                                                                    │
│  ┌────────────┐   ┌─────────────────┐   ┌──────────────────────┐  │
│  │  Input /   │   │  Vehicle &      │   │  World / Environment  │  │
│  │  Steering  │──▶│  Physics        │──▶│  (roads, signs,       │  │
│  │ (CoreMotion)│   │ (WheelCollider) │   │   signals, traffic)   │  │
│  └────────────┘   └─────────────────┘   └──────────┬───────────┘  │
│                                                    │ world events  │
│                                                    ▼               │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                    RULES ENGINE                              │  │
│  │  • Rule registry (data-driven, ScriptableObjects)           │  │
│  │  • Relevance detection (trigger zones / context)            │  │
│  │  • Behavior evaluation (predicate per rule)                 │  │
│  └───────────────┬─────────────────────────────────────────────┘  │
│                  │ evaluations (relevant / pass / fail)            │
│                  ▼                                                 │
│  ┌─────────────────────────────────────────────────────────────┐  │
│  │                    COACH ENGINE                              │  │
│  │  reads per-rule encounterCount → decides:                   │  │
│  │  GUIDE  |  STAY SILENT  |  PENALIZE                          │  │
│  └───────┬─────────────────────────────────┬───────────────────┘  │
│          │ guidance cues                   │ tickets / points     │
│          ▼                                 ▼                      │
│  ┌───────────────┐               ┌────────────────────────────┐  │
│  │  Coach UI/    │               │  Scoring + Player Profile  │  │
│  │  Voice/HUD    │               │  (persistent record)       │  │
│  └───────────────┘               └────────────────────────────┘  │
└──────────────────────────────────────────────────────────────────┘
```

**Traffic & signs feed the Rules Engine; the Rules Engine feeds the Coach; the Coach is
the only thing that decides guide-vs-silence-vs-penalty.** This separation keeps "what
is the rule" independent from "how do we teach it," which is what lets the
teach→fade→enforce behavior live in one place and stay tunable.

### 8.3 Data model (illustrative)

```jsonc
// A Rule definition (authored once, data-driven)
{
  "id": "stop_sign",
  "category": "right_of_way",
  "title": "Stop sign",
  "relevance": { "trigger": "enter_zone", "zone": "stop_sign_approach" },
  "correctBehavior": {
    "type": "full_stop",
    "params": { "maxSpeed": 0.5, "withinBox": "stop_line", "minHoldSeconds": 1.0 }
  },
  "evaluationWindow": { "from": "stop_line_-10m", "to": "stop_line_+3m" },
  "guidance": {
    "preempt": "Stop sign ahead — come to a complete stop behind the white line.",
    "correction": "That was a rolling stop. Next time, stop fully and count one-two.",
    "highlight": ["stop_sign", "stop_line"]
  },
  "severity": "major",
  "penalty": { "points": 15, "fine": 200, "instantFail": false },
  "freeEncounters": 3,
  "reCoachAfterFailures": 3
}
```

```jsonc
// Per-player tracking (persistent)
{
  "playerId": "local",
  "rules": {
    "stop_sign":   { "encounterCount": 4, "graduated": true,  "postGradFailures": 1 },
    "highway_merge": { "encounterCount": 1, "graduated": false, "postGradFailures": 0 }
  },
  "drivingRecord": { "lifetimeTickets": 3, "rulesMastered": 12, "confidenceTrend": [ ... ] }
}
```

The Coach's decision is a pure function of `(rule, playerRuleState, evaluationResult,
difficultySettings)` — easy to unit-test in isolation from the 3D game.

### 8.4 Testability

- The Rules and Coach engines are deterministic and **headless-testable** with scripted
  scenarios ("car approaches stop sign at 8 m/s and doesn't stop → expect: encounter
  counted; if graduated → ticket of severity major").
- Physics/traffic get a **scenario harness** (spawn a situation, script inputs, assert
  outcomes) so rule logic can be validated without manual play.

---

## 9. Lessons & content (summary)

The full progression lives in [`docs/lessons.md`](./docs/lessons.md); the rule/sign
catalog lives in [`docs/rules-catalog.md`](./docs/rules-catalog.md). In brief, v1 ships:

1. **Lesson 0 — Controls & Calibration** (parking lot): steering, gas/brake, parking,
   reversing.
2. **Lesson 1 — Residential**: stop signs, speed limits, pedestrians, signaling.
3. **Lesson 2 — Town & Intersections**: traffic lights, 4-way stops, turn lanes,
   crosswalks, school zone, right-of-way.
4. **Lesson 3 — Highway**: on-ramp merge, lane changes, following distance, exits.
5. **Road Test** (capstone, no coaching).

---

## 10. UX & screens

- **Home / Driving Record** — confidence trend, continue lesson, free drive, road test.
- **Lesson select** — locked/unlocked path, what each teaches.
- **Pre-drive** — choose camera, control scheme, time-of-day/weather (where allowed);
  **calibrate the wheel**.
- **In-drive HUD** — speedometer, current speed limit, gear/signal indicators, minimal
  Coach prompt area, mirrors for lane checks. (Kept clean to preserve immersion.)
- **End-of-drive report** — score, tickets with replays, rules graduated, "practice
  next."
- **Settings** — sensitivity, control scheme, difficulty (free-encounter count, traffic
  density), accessibility, audio/captions.

Tone throughout: calm, encouraging, adult — a supportive coach, not a scolding cop.

---

## 11. Roadmap / milestones

| Milestone | Scope | Exit criteria |
| --- | --- | --- |
| **M0 — Prototype "feel"** | iPhone tilt-steering + simple car physics in an empty lot. | Steering feels controllable and fun; calibration works. |
| **M1 — Rules + Coach core** | Rules engine + Coach state machine + 3–4 rules (stop sign, speed limit, signal, yield), headless tests, basic HUD/voice. | teach→fade→enforce demonstrably works for those rules. |
| **M2 — Residential lesson** | Quiet-street environment, pedestrians, end-of-drive report, persistence. | Lesson 1 fully playable end to end. |
| **M3 — Intersections** | Traffic lights, 4-way stops, crosswalks, school zone, richer traffic AI. | Lesson 2 playable; right-of-way rules enforced. |
| **M4 — Highway** | On-ramp/merge, lane changes, following distance, exits. | Lesson 3 playable. |
| **M5 — Polish & Road Test** | Replays, weather/time, audio pass, accessibility, Road Test capstone, settings. | Beta-ready vertical slice across all lessons. |
| **Post-v1** | Big-screen cast mode, open free-roam map, more regions/rule sets, manual transmission, cloud sync, snow/fog. | — |

---

## 12. Risks & open questions

**Risks**
- **Steering feel** is make-or-break; if tilt-steering isn't satisfying, the whole
  premise fails. *Mitigation:* M0 exists solely to de-risk this before anything else.
- **Content cost** (3D art for cars/roads/signs/cities). *Mitigation:* asset packs +
  modular, hand-authored courses instead of an open world in v1.
- **Coach feeling naggy or robotic.** *Mitigation:* one-prompt-at-a-time priority,
  deferred explanations, calm VO, tunable free-encounter count.
- **Real-world transfer is unproven.** *Mitigation:* model rules accurately, frame as
  practice not certification, gather user feedback in beta.

**Open questions**
1. **Whose rules?** Which state's/region's rules and signs ship first? (Affects sign
   art, right-of-way edge cases, speed-limit conventions.)
2. **Does the game render on the phone, or cast to a big screen for v1?** (Spec assumes
   phone-renders for v1; cast is post-v1.)
3. **How "hard" is realistic physics worth being** vs. approachability for anxious
   beginners? (Suggest: believable but forgiving, with an "Assisted" mode.)
4. **Monetization / distribution** — personal project, free, or App Store? (Out of scope
   for this spec but affects backend and asset-licensing choices.)
5. **Voice** — recorded VO vs. on-device TTS for Coach lines?

---

## 13. Repository layout (proposed)

```
driving-made-easy/
├── README.md            ← project intro
├── SPEC.md              ← this document
├── docs/
│   ├── rules-catalog.md ← the traffic rules & signs the Coach teaches/enforces
│   ├── lessons.md       ← lesson / level progression
│   └── glossary.md      ← shared vocabulary
└── (src/ added at M0 — Unity project, once the stack is confirmed)
```

---

*This is a living document. Section 12's open questions should be resolved before M1,
and the rule/lesson catalogs in `docs/` evolve alongside implementation.*
