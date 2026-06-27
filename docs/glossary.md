# Glossary

Shared vocabulary used across the spec and docs.

| Term | Meaning |
| --- | --- |
| **Coach** | The adaptive teaching engine. Decides, per rule encounter, whether to **guide**, **stay silent**, or **penalize**. See [`../SPEC.md`](../SPEC.md) §6. |
| **Rule** | A single teachable/enforceable element of driving (a stop sign, the speed limit, signaling). Data-driven; defined once. See [`rules-catalog.md`](./rules-catalog.md). |
| **Relevance trigger** | The world condition that makes a rule apply right now (entering a stop sign's approach zone, passing a speed-limit sign). |
| **Correct-behavior predicate** | The testable condition the player must satisfy for the rule (full stop within the box; speed ≤ limit). |
| **Encounter** | One relevant occurrence of a rule where the player had a fair chance to act. Tracked per player via `encounterCount`. |
| **Free encounters** | The first N encounters of a rule (default 3) that are **coached, not penalized** — teaching mode. |
| **Graduated** | A rule that has passed its free encounters and is now **enforced** (mistakes → tickets/points). |
| **Re-coach** | A graduated rule temporarily returning to teaching mode after repeated failures (spaced repetition). |
| **Ticket** | A penalty event for breaking a graduated rule: points deducted, optional play-money fine, logged to the driving record. |
| **Severity** | A rule's penalty tier: `minor` / `major` / `severe` (severe can instant-fail with a replay). |
| **Safety score** | The per-drive score, starting from a clean baseline (e.g. 100) and reduced by violations. |
| **Driving record** | The persistent player profile: lifetime tickets, rules mastered, confidence trend. |
| **Lesson** | A guided level introducing a focused set of rules. See [`lessons.md`](./lessons.md). |
| **Free Drive** | Open practice in an unlocked environment with full enforcement and no lesson objectives. |
| **Road Test** | The capstone evaluation drive with coaching off and every rule live. |
| **Calibration** | Setting the phone's neutral "hands at 9-and-3" pose as 0° steering, via CoreMotion. |
| **Comfort / Tilt-throttle / Assisted** | The three control schemes (thumb pedals / tilt pedals / auto-throttle). See SPEC §3.3. |
