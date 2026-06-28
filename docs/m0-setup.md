# M0 — Steering-feel prototype: setup & run

**Milestone goal (SPEC §11):** prove that holding the iPhone as a wheel and tilting to
steer *feels good* and is controllable. Nothing else in the product matters if this
doesn't — so M0 deliberately contains only: an empty parking lot, a forgiving car, tilt
steering with calibration, a cone slalom, and a minimal HUD.

The scene is **generated in code** by [`M0Bootstrap`](../src/DrivingMadeEasy/Assets/Scripts/Bootstrap/M0Bootstrap.cs),
so there's no hand-authored scene file to manage and everything is reviewable C#.

## What's here

```
src/DrivingMadeEasy/
├── Packages/manifest.json          Unity package deps
├── ProjectSettings/ProjectVersion  Unity version hint (2022.3 LTS+)
└── Assets/Scripts/
    ├── Input/IDriverInput.cs        control-scheme abstraction
    ├── Input/MotionSteeringInput.cs iPhone tilt → steering (calibration, dead zone,
    │                                sensitivity curve, smoothing, return-to-center)
    ├── Vehicle/CarController.cs      forgiving WheelCollider car
    ├── Camera/ChaseCamera.cs         smoothed follow camera
    ├── UI/DrivingHud.cs              speed + steering readout + Recenter button
    └── Bootstrap/M0Bootstrap.cs      builds the whole lot/car/cones at runtime
```

## Run it on a Mac/PC first (no phone needed)

1. Install **Unity 2022.3 LTS or newer** (via Unity Hub).
2. Open the project at `src/DrivingMadeEasy/`. Unity generates `Library/` and the
   `.meta` files on first import — that's expected.
3. Create a new empty scene (`File ▸ New Scene` ▸ *Basic / Empty*). Save it as
   `Assets/Scenes/M0.unity`.
4. Create an empty GameObject (`GameObject ▸ Create Empty`), name it `Bootstrap`, and
   add the **M0Bootstrap** component to it (Add Component ▸ search "M0Bootstrap").
5. Press **Play**. Drive with the keyboard fallback:
   - **A / D** (or ←/→) = steer
   - **W / S** (or ↑/↓) = gas / brake
   - hold **R** = reverse

This lets you tune steering feel (sensitivity curve, dead zone, responsiveness — all
exposed on the `MotionSteeringInput` component) before ever touching a phone.

## Run it on your iPhone

1. `File ▸ Build Settings ▸ iOS ▸ Switch Platform`, then add the M0 scene.
2. In `Player Settings`, set a Bundle Identifier and your (free) Apple ID team. Add a
   **camera/motion usage** description if prompted.
3. **Build** → Unity exports an Xcode project. Open it in Xcode, connect your iPhone,
   and Run. (Free Apple ID works; the app refreshes every 7 days. TestFlight optional
   later — see SPEC §12 distribution decision.)
4. Hold the phone in **landscape**. Tap **Recenter wheel** while holding your neutral
   "hands at 9-and-3" pose, then tilt to steer. Bottom-right of the screen = gas,
   bottom-left = brake.

## Tuning the steering feel (the whole point of M0)

On the `MotionSteeringInput` component:

| Field | Try this if… |
| --- | --- |
| `maxRollDegrees` | feels twitchy → raise; feels like you must tilt too far → lower |
| `deadZoneDegrees` | car wanders when holding straight → raise slightly |
| `sensitivityExponent` | hard to make fine corrections → raise (more gentle near center) |
| `responsiveness` | feels laggy → raise; feels nervous/jittery → lower |
| `returnToCenterAssist` / `assistStrength` | beginners → on + higher strength (faster self-centering); want raw control → off |
| `invertSteering` | **tilting left steers right** on your device → toggle this (gyro axis frames vary by device/orientation) |

> **On-device gyro caveat:** `Input.gyro.attitude` comes in the sensor's own coordinate
> frame. The code applies the standard gyro→Unity conversion and reads roll about the
> screen normal, but the correct axis/sign can still vary by device and orientation. If
> steering feels mapped to the wrong axis (e.g. pitch instead of roll) or is reversed,
> that's expected first-run tuning — flip `invertSteering` first, and if it's truly the
> wrong axis, this is the one spot to validate on real hardware.

## Exit criteria for M0

- Steering feels controllable and pleasant (you can weave the cone slalom smoothly).
- Calibration/recenter works and survives picking the phone up at a new angle.
- The car has believable momentum and braking but never spins out or feels punishing.

Once that bar is met, **M1** adds the Rules Engine + Coach (teach → fade → enforce).
