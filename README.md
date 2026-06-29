# Driving Made Easy 🚗

A driving **simulation game** that turns your iPhone into a steering wheel so you can
get your driving confidence back — practice city streets, highways, traffic, signs,
and the rules of the road without ever leaving your couch.

The app behaves like a patient driving coach. The **first three times** you meet a new
sign or situation, it gives you friendly, just-in-time guidance ("Stop sign ahead —
come to a *full* stop behind the line"). After that, the training wheels come off:
no more hints, and breaking a rule earns you a **ticket** or a **points deduction**,
just like real life.

> **Goal:** make the experience feel as close to real driving as possible, so the
> habits you build in the game transfer to the road.

---

## Status

🏗️ **M1 — Rules Engine + Coach.** Building on the M0 steering prototype, the Coach now
works: drive up to the stop sign in the parking lot and it coaches you for 3 encounters,
then starts ticketing you (teach → fade → enforce). The full spec is in
[`SPEC.md`](./SPEC.md); the Unity project is in
[`src/DrivingMadeEasy/`](./src/DrivingMadeEasy).

- Run it: [`docs/m0-setup.md`](./docs/m0-setup.md) (open in Unity, press Play — keyboard/
  mouse on a laptop; real tilt on the iPhone).
- See the Coach + how it works: [`docs/m1-coach.md`](./docs/m1-coach.md).

## Documents

| Doc | What's inside |
| --- | --- |
| [`SPEC.md`](./SPEC.md) | The complete spec: vision, gameplay, the Coach system, controls, realism, tech architecture, data model, roadmap |
| [`docs/rules-catalog.md`](./docs/rules-catalog.md) | The catalog of traffic rules & signs the Coach teaches and enforces |
| [`docs/lessons.md`](./docs/lessons.md) | The lesson / level progression (parking lot → streets → highway) |
| [`docs/glossary.md`](./docs/glossary.md) | Shared vocabulary used across the spec |
| [`docs/m0-setup.md`](./docs/m0-setup.md) | How to open & run the M0 steering prototype |

## The one-paragraph pitch

You hold your iPhone like a steering wheel and tilt to steer. You drive through
realistic environments with live traffic, pedestrians, signals, and posted signs.
A coaching engine watches everything you do. New things get explained up front three
times; after that you're on your own and mistakes cost you. Over a series of lessons
you graduate from an empty parking lot to merging onto a busy highway — rebuilding
real, transferable driving instincts.
