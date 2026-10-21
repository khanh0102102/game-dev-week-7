# Game Dev Week 7 — Basic Animation Setup

This project upgrades the Unity 6.6 airplane scene from https://github.com/khanh0102102/BaiTapTuan1_MayBay. The original Assets/Scenes/SampleScene.unity is preserved with AirPlane, AirplaneBody, AirplaneWing, TailPlane, and AirplaneFin.

## Unity version

Use Unity 6.6 (6000.6.6f1), matching ProjectSettings/ProjectVersion.txt.

## Practice 2 — Airplane wing animation

1. Open Assets/Scenes/SampleScene.unity.
2. Choose Tools > Week 7 > Setup Airplane Wing Animation.
3. Select AirplaneWing and confirm an Animator uses Assets/Animation/AirplaneWingAnimatorController.controller.
4. Press Play. The one-second ThrottleUpAnimation changes the wing's local X rotation:

| Frame | Time | Rotation X |
|---:|---:|---:|
| 0 | 0 s | 0° |
| 15 | 0.25 s | -3.75° |
| 30 | 0.50 s | -7.5° |
| 45 | 0.75 s | -11.25° |
| 60 | 1.00 s | -15° |

The clip is 60 FPS and does not loop, so the wing holds its final -15° rotation. Re-run the menu to regenerate the clip and controller.

## Practice 1 — Mixamo X Bot / Swing Dancing

The Editor tool automates Unity's rig setup and Animator Controller, but the Mixamo FBX files must be downloaded by you:

1. Sign in to https://www.mixamo.com/.
2. Select X Bot and download the character as FBX with skin.
3. Select Swing Dancing and download the animation as FBX without skin.
4. Put both FBX files in Assets/Mixamo/; their names should contain X Bot and Swing Dancing.
5. Wait for Unity to import them and choose Tools > Week 7 > Setup Mixamo X Bot Dance.
6. The tool configures the character as Humanoid, maps the animation to the X Bot Avatar, creates XBotAnimatorController.controller, and adds Idle ↔ Swing Dancing transitions using the IsDancing boolean.
7. The tool creates Assets/Scenes/MixamoDanceScene.unity. Open it and press Play; the dance starts automatically. Press Space to toggle Idle/dancing.

The model and animation FBX files are not included because they must be downloaded from Mixamo. See Assets/Mixamo/README.md.

## Git

After running the setup menus, commit generated animation clips, Animator Controllers, changed scene files, and their .meta files. Do not commit Library/, Temp/, or Logs/.
