# M1 — Rules Engine + Coach

This milestone implements the heart of the product: the **Coach** that teaches a rule for
its first few encounters, then fades the help and enforces it with tickets
(SPEC [§6](../SPEC.md)). It ships as a Unity-independent core (so it's unit-testable) plus
a thin Unity layer, with a working **stop sign** wired into the M0 parking lot.

## Try it (in the M0 scene)

Open and Play the project as in [`m0-setup.md`](./m0-setup.md), then drive **forward** from
the start toward the **red stop sign and white line**:

1. **First 3 times** you reach it, the Coach gives a friendly heads-up ("Stop sign ahead —
   come to a complete stop behind the white line…"). If you roll through, it gently
   corrects you — **no penalty**. (In an empty lot, just loop around and re-approach to
   rack up encounters.)
2. **4th time onward**, the hints stop. Now if you roll the stop, you get a **ticket**: a
   toast pops up and your **Safety score** (top-right) drops.

Watch the small status readout under the score: it moves from `stop_sign: coaching 1/3`
→ `2/3` → `3/3` → `enforced`. A full, complete stop counts as a successful rep and also
graduates the rule.

> Tip: to see enforcement sooner, you can set the **Coach** object's *Difficulty* to
> **Exam** (0 free encounters) in the Inspector while playing — every mistake is ticketed
> immediately. **Gentle** gives 5 coached reps instead of 3.

## Run the headless tests

The teach → fade → enforce logic is covered by pure-C# scenario tests. In Unity:

**Tools ▸ Driving Made Easy ▸ Run Coach Tests** → results print in the Console
(`PASS`/`FAIL` per scenario, then a summary). They cover: coaching-then-enforcing,
correct reps graduating a rule, Exam/Gentle modes, spaced-repetition re-coaching,
pre-emptive cues only while coaching, and scoring/tickets.

## How it's built

```
Assets/Coaching/            ← pure C# core (no UnityEngine) — fully testable
  CoachingModel.cs            rules, penalties, guidance, outcomes, tickets
  PlayerProfile.cs            per-rule progress + difficulty settings
  Coach.cs                    the teach→fade→enforce engine + rule registry
  CaliforniaRules.cs          the starter CA rule data (stop sign, speed, signal, yield)
  DriveSession.cs             per-drive score + end-of-drive report
  CoachScenarios.cs           headless tests
Assets/Scripts/Game/         ← Unity layer
  CoachRuntime.cs             owns the Coach; turns world events into HUD/voice events
  StopSignZone.cs             trigger that detects the approach + judges the stop
  CoachHud.cs                 banner / ticket toast / score / status overlay
Assets/Editor/
  CoachTestsMenu.cs           the "Run Coach Tests" menu item
```

The key design point (SPEC §8.2): the **engine decides** guide-vs-silent-vs-penalize, and
it's a deterministic function of `(rule, player state, pass/fail, difficulty)` — which is
exactly what makes it testable without the game running.

## Also in (early M2 scaffolding)

- **A road**: a straight asphalt strip with edge lines and a dashed center line, so the
  lot reads as a street. Signs and zones are laid out along it in sequence.
- **Speed-limit rule, in-world**: a posted limit sign + a `SpeedZone` that watches your
  speed the whole way through and coaches/enforces just like the stop sign. The active
  limit shows on the HUD ("Limit: 20 mph").
- **End-of-drive report**: press **Tab** to finish a drive and see your safety score, the
  tickets you collected (with the explanations the Coach held back during the drive), and
  which rules you graduated. "Drive again" resets the score but keeps what you've learned.
- **Persistence**: your coaching progress is saved to disk, so the Coach remembers which
  rules you've already mastered between drives and app launches. Reset it any time via
  **Tools ▸ Driving Made Easy ▸ Reset Coaching Progress**.
- **Pedestrian crosswalk, in-world**: a pedestrian paces across a striped crosswalk; when
  someone's in your path the Coach prompts you to yield, and a `CrosswalkZone` checks
  whether you actually slowed down for them. It only counts as an encounter when a
  pedestrian is genuinely crossing (a fair chance to act). This is the iconic "don't hit
  people" rule — severity Severe.

- **Intersection with cross traffic, in-world**: a 4-way junction where a small fleet of
  cars streams across your path. When a car is crossing as you approach, the Coach prompts
  you to **yield** (slow and let them through), and a `CrossTrafficZone` checks whether you
  actually slowed — the car-to-car `yield` rule, now live. It only counts when traffic is
  genuinely crossing (wait for a gap and you pass clean).

The drive now runs as a little course: **stop sign → 20 mph stretch → pedestrian
crosswalk → cross-traffic junction → cone slalom**.

- **Turn signals, in-world**: blinkers (Q/E on desktop) with a dashboard tell-tale and
  real-style auto-cancel after a turn. A `TurnZone` over the junction detects whether you
  actually turned and checks you signaled that direction — the `turn_signal` rule. Driving
  straight through is a no-event.

The course now exercises **5 in-world rules**: stop sign, speed limit, crosswalk yield,
cross-traffic yield, and turn signal.

## What's next (toward a full Lesson 1)

Still to wire in: traffic lights, 4-way stops, lane-keeping, following distance, and a
school zone. Traffic is currently visual-only and loops on a fixed path; proper
lane-following / light-obeying AI, collisions, and a real road network are the rest of M2.
Voice is still on-screen text — on-device text-to-speech (`AVSpeechSynthesizer`) arrives
with the iPhone build.
