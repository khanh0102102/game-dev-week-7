# Game Dev Week 7 — Animation Practice

This repository upgrades the Week 1 airplane scene from [BaiTapTuan1_MayBay](https://github.com/khanh0102102/BaiTapTuan1_MayBay) and targets **Unity 6.6 (6000.6.6f1)**.

## Practice 1 — Mixamo X Bot / Swing Dancing

Unity cannot include the X Bot and Swing Dancing FBX files on your behalf because the assets need to be downloaded from your Adobe Mixamo account. After downloading the files:

1. In Mixamo, select **X Bot** and download the character as **FBX** with skin.
2. Select **Swing Dancing** and download the animation as **FBX** without skin.
3. Put the files in `Assets/Mixamo/`:
   - `X Bot.fbx`
   - `Swing Dancing.fbx`
4. Let Unity import them, then choose **Tools > Week 7 > Setup Mixamo X Bot Dance**.
5. The setup tool configures the character as Humanoid, maps the animation to the X Bot Avatar, creates `Assets/Animation/XBotAnimatorController.controller`, adds Idle ↔ Swing Dancing transitions using the `IsDancing` boolean, and places an animated X Bot in the airplane scene.
6. Press Play. Swing Dancing starts automatically. Press **Space** to toggle between dancing and idle.

The FBX files are not included in this repository; they must be downloaded from Mixamo.

## Practice 2 — Airplane wing takeoff animation

The Editor setup tool creates `Assets/Animation/ThrottleUpAnimation.anim` and an Animator Controller for `AirplaneWing`. The clip is 1 second at 60 FPS and animates the local X rotation through these exact keys:

| Frame | Time | Rotation X |
|---:|---:|---:|
| 0 | 0 s | 0° |
| 15 | 0.25 s | -3.75° |
| 30 | 0.50 s | -7.5° |
| 45 | 0.75 s | -11.25° |
| 60 | 1.00 s | -15° |

The animation does not loop; the wing holds its final takeoff rotation.

## Open and test

1. Open this repository in Unity Hub using Editor **6000.6.6f1**.
2. Wait for the project to import and scripts to compile.
3. Open `Assets/Scenes/SampleScene.unity`.
4. The first Editor import creates the wing clip, controller, and Animator binding.
5. Press Play and inspect `AirplaneWing`. Its X rotation should animate from 0° to -15° over 1 second.
6. Add the Mixamo FBX files and use the setup menu for Practice 1 as described above.

You can rerun **Tools > Week 7 > Setup Airplane Wing Animation** to regenerate the wing animation assets and Animator Controller.

## Source scene

The original Week 1 airplane scene is retained, including `AirPlane`, `AirplaneBody`, `AirplaneWing`, `TailPlane`, and `AirplaneFin`.

Generated Unity folders such as `Library/`, `Temp/`, and `Logs/` should not be committed.
