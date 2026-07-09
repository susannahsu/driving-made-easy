# Putting it on your iPhone

A one-time setup to build the app onto your own iPhone with a **free Apple ID** (no paid
developer account). Rough time: ~20–40 min, mostly the iOS module download.

## 1. Add the iOS Build Support module
Unity Hub → **Installs** → the ⚙️/three-dots on your 2022.3 editor → **Add modules** →
check **iOS Build Support** → Install. (Leave the rest unchecked.)

## 2. Prepare the project (one click)
Open the project in Unity, then run:
**Tools ▸ Driving Made Easy ▸ Prepare iOS Build**

This force-includes the Standard shader (so materials don't turn pink on device), sets
**landscape** orientation, a bundle id, and — at build time — the motion-usage permission
that tilt steering needs. Check the Console for the "Prepared iOS build" message.

## 3. Make sure the scene is in the build
**File ▸ Build Settings**. If the list is empty, open your `M0` scene and click
**Add Open Scenes**. Then select **iOS** and click **Switch Platform** (this reimports —
a few minutes).

## 4. Signing (free Apple ID)
Still in Build Settings → **Player Settings ▸ Other Settings**:
- **Bundle Identifier**: leave the default `com.drivingmadeeasy.app` (or make it unique).
- You'll set the signing **Team** in Xcode in step 6 — no paid account needed.

## 5. Build to an Xcode project
Back in Build Settings → **Build**. Pick an empty folder (e.g. `ios-build/`). Unity
exports an **Xcode project** there.

## 6. Run from Xcode
1. Open the generated `Unity-iPhone.xcodeproj` in **Xcode**.
2. Connect your iPhone by cable; unlock it and "Trust This Computer".
3. In Xcode, select the **Unity-iPhone** target → **Signing & Capabilities** →
   check **Automatically manage signing** → pick your **personal team** (your Apple ID;
   add it under Xcode ▸ Settings ▸ Accounts if it's not listed).
4. Choose your iPhone as the run destination (top bar) and press **▶ Run**.
5. First run: on the iPhone, go to **Settings ▸ General ▸ VPN & Device Management** →
   trust your developer profile. Then launch the app again.

## 7. Drive it
- Hold the phone in **landscape**.
- Tap **Recenter wheel** while holding your neutral "hands at 9-and-3" pose.
- **Tilt** the phone to steer.
- **Gas / brake**: touch the **bottom-right** (gas) and **bottom-left** (brake) of the
  screen with your thumbs.

## Known gaps on device (desktop-only for now)
These are keyboard controls with no touch equivalent yet — they simply won't fire on the
phone until we add on-screen buttons:
- **Turn signals** (Q/E), **camera toggle** (C), **end-drive report** (Tab).

The core loop — tilt steering, gas/brake, and the Coach teaching/enforcing the rules — all
works on device. If the steering feels **mirrored** (tilt left → turns right), toggle
**Invert Steering** on the car's *Motion Steering Input* component; the gyro axis varies by
device and that's the one field expected to need a flip.

## If materials look pink on device
That means the Standard shader was still stripped — re-run **Prepare iOS Build**, rebuild,
and confirm `Standard` appears under **Project Settings ▸ Graphics ▸ Always Included
Shaders**.
