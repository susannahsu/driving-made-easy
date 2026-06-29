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

## What's next (toward M2)

Only the **stop sign** is placed in the world so far; `speed_limit`, `turn_signal`, and
`yield` are defined and tested but get their in-world detection as we build the real
residential environment (Lesson 1). Voice is still on-screen text — on-device
text-to-speech (`AVSpeechSynthesizer`) arrives with the iPhone build.
