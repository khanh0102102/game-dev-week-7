#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

[InitializeOnLoad]
public static class AnimationPracticeSetup
{
    private const string SampleScenePath = "Assets/Scenes/SampleScene.unity";
    private const string DanceScenePath = "Assets/Scenes/MixamoDanceScene.unity";
    private const string AnimationFolder = "Assets/Animation";
    private const string MixamoFolder = "Assets/Mixamo";
    private const string WingClipPath = AnimationFolder + "/ThrottleUpAnimation.anim";
    private const string WingControllerPath = AnimationFolder + "/AirplaneWingAnimatorController.controller";
    private const string MixamoControllerPath = AnimationFolder + "/XBotAnimatorController.controller";

    static AnimationPracticeSetup()
    {
        EditorApplication.delayCall += () =>
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                return;

            try
            {
                EnsureFolder(AnimationFolder);
                EnsureFolder(MixamoFolder);
                EnsureWingAssets(false);
            }
            catch (Exception exception)
            {
                Debug.LogWarning("Week 7 animation assets can be generated using Tools > Week 7 > Setup Airplane Wing Animation. " + exception.Message);
            }
        };
    }

    [MenuItem("Tools/Week 7/Setup Airplane Wing Animation")]
    public static void SetupAirplaneWingAnimation()
    {
        EnsureFolder(AnimationFolder);
        EnsureWingAssets(true);

        if (SceneManager.GetActiveScene().path != SampleScenePath)
        {
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                return;

            if (!File.Exists(AbsoluteAssetPath(SampleScenePath)))
            {
                EditorUtility.DisplayDialog(
                    "Week 1 airplane scene not found",
                    "The original SampleScene.unity was not found. Make sure the Week 1 airplane project files are present.",
                    "OK");
                return;
            }

            EditorSceneManager.OpenScene(SampleScenePath, OpenSceneMode.Single);
        }

        GameObject wing = GameObject.Find("AirplaneWing");
        if (wing == null)
        {
            EditorUtility.DisplayDialog(
                "AirplaneWing not found",
                "The current SampleScene must contain the AirplaneWing GameObject from Week 1.",
                "OK");
            return;
        }

        Animator animator = wing.GetComponent<Animator>();
        if (animator == null)
            animator = Undo.AddComponent<Animator>(wing);

        animator.runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(WingControllerPath);
        animator.applyRootMotion = false;

        EditorUtility.SetDirty(animator);
        EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EditorGUIUtility.PingObject(wing);
        Debug.Log("ThrottleUpAnimation assigned to AirplaneWing. Press Play to test the 0° to -15° X rotation over one second.");
    }

    [MenuItem("Tools/Week 7/Setup Mixamo X Bot Dance")]
    public static void SetupMixamoXBotDance()
    {
        EnsureFolder(AnimationFolder);
        EnsureFolder(MixamoFolder);

        string[] fbxPaths = AssetDatabase.FindAssets(string.Empty, new[] { MixamoFolder })
            .Select(AssetDatabase.GUIDToAssetPath)
            .Where(path => path.EndsWith(".fbx", StringComparison.OrdinalIgnoreCase))
            .Distinct()
            .ToArray();

        string characterPath = fbxPaths.FirstOrDefault(path =>
            Path.GetFileNameWithoutExtension(path).Replace(" ", "").ToLowerInvariant().Contains("xbot"));
        string dancePath = fbxPaths.FirstOrDefault(path =>
            Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains("swing") ||
            Path.GetFileNameWithoutExtension(path).ToLowerInvariant().Contains("dancing"));

        if (string.IsNullOrEmpty(characterPath) || string.IsNullOrEmpty(dancePath))
        {
            EditorUtility.DisplayDialog(
                "Mixamo FBX files are missing",
                "Download these two FBX files from Mixamo and place both in Assets/Mixamo:\n\n" +
                "1. X Bot.fbx — character download with skin.\n" +
                "2. Swing Dancing.fbx — animation download without skin.\n\n" +
                "Wait for Unity to import them, then run this menu again.",
                "OK");
            return;
        }

        ModelImporter characterImporter = AssetImporter.GetAtPath(characterPath) as ModelImporter;
        if (characterImporter == null)
            throw new InvalidOperationException("Could not access the X Bot FBX importer.");

        characterImporter.importAnimation = true;
        characterImporter.animationType = ModelImporterAnimationType.Human;
        characterImporter.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
        characterImporter.SaveAndReimport();

        Avatar avatar = AssetDatabase.LoadAllAssetsAtPath(characterPath)
            .OfType<Avatar>()
            .FirstOrDefault(item => item != null && item.isHuman);

        if (avatar == null)
        {
            EditorUtility.DisplayDialog(
                "X Bot Avatar could not be created",
                "Select the X Bot FBX, open Rig, set Animation Type to Humanoid, then click Apply. Verify that its Avatar is valid.",
                "OK");
            return;
        }

        ModelImporter animationImporter = AssetImporter.GetAtPath(dancePath) as ModelImporter;
        if (animationImporter == null)
            throw new InvalidOperationException("Could not access the Swing Dancing FBX importer.");

        animationImporter.importAnimation = true;
        animationImporter.animationType = ModelImporterAnimationType.Human;
        animationImporter.avatarSetup = ModelImporterAvatarSetup.CopyFromOther;
        animationImporter.sourceAvatar = avatar;

        ModelImporterClipAnimation[] clipSettings = animationImporter.clipAnimations;
        if (clipSettings == null || clipSettings.Length == 0)
            clipSettings = animationImporter.defaultClipAnimations;

        if (clipSettings != null)
        {
            foreach (ModelImporterClipAnimation clipSetting in clipSettings)
            {
                clipSetting.loopTime = true;
                clipSetting.loopPose = true;
            }
            animationImporter.clipAnimations = clipSettings;
        }
        animationImporter.SaveAndReimport();
        AssetDatabase.Refresh();

        AnimationClip danceClip = AssetDatabase.LoadAllAssetsAtPath(dancePath)
            .OfType<AnimationClip>()
            .FirstOrDefault(clip => clip != null && clip.length > 0f &&
                                     !clip.name.StartsWith("__", StringComparison.Ordinal));

        if (danceClip == null)
        {
            EditorUtility.DisplayDialog(
                "Swing Dancing clip not found",
                "Unity did not expose an AnimationClip from the downloaded FBX. Select the animation FBX, enable Import Animation, and click Apply.",
                "OK");
            return;
        }

        AnimatorController controller = BuildMixamoController(danceClip);
        GameObject characterAsset = AssetDatabase.LoadAssetAtPath<GameObject>(characterPath);
        if (characterAsset == null)
            throw new InvalidOperationException("Could not load the X Bot model as a GameObject.");

        if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            return;

        Scene danceScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
        GameObject character = PrefabUtility.InstantiatePrefab(characterAsset) as GameObject;
        if (character == null)
            character = UnityEngine.Object.Instantiate(characterAsset);

        character.name = "XBot_Swing_Dance";
        character.transform.position = Vector3.zero;
        character.transform.rotation = Quaternion.Euler(0f, 180f, 0f);

        Animator animator = character.GetComponent<Animator>();
        if (animator == null)
            animator = character.AddComponent<Animator>();

        animator.avatar = avatar;
        animator.runtimeAnimatorController = controller;
        animator.applyRootMotion = false;

        if (animator.GetComponent<MixamoDanceDemoController>() == null)
            animator.gameObject.AddComponent<MixamoDanceDemoController>();

        CreateDanceStage();
        EditorSceneManager.MarkSceneDirty(danceScene);
        EditorSceneManager.SaveScene(danceScene, DanceScenePath);
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene(SampleScenePath, true),
            new EditorBuildSettingsScene(DanceScenePath, true)
        };
        AssetDatabase.SaveAssets();
        Selection.activeGameObject = character;
        Debug.Log("Mixamo practice scene created. Press Play; Swing Dancing starts automatically. Press Space to toggle Idle/Dance.");
    }

    private static void EnsureWingAssets(bool rebuild)
    {
        EnsureFolder(AnimationFolder);

        AnimationClip clip = AssetDatabase.LoadAssetAtPath<AnimationClip>(WingClipPath);
        AnimatorController controller = AssetDatabase.LoadAssetAtPath<AnimatorController>(WingControllerPath);

        if (rebuild)
        {
            if (clip != null)
                AssetDatabase.DeleteAsset(WingClipPath);
            if (controller != null)
                AssetDatabase.DeleteAsset(WingControllerPath);
            clip = null;
            controller = null;
        }

        if (clip == null)
        {
            clip = new AnimationClip
            {
                name = "ThrottleUpAnimation",
                frameRate = 60f,
                legacy = false
            };

            float[] times = { 0f, 0.25f, 0.5f, 0.75f, 1f };
            float[] rotations = { 0f, -3.75f, -7.5f, -11.25f, -15f };
            Keyframe[] keys = new Keyframe[times.Length];
            for (int i = 0; i < times.Length; i++)
                keys[i] = new Keyframe(times[i], rotations[i]);

            AnimationCurve curve = new AnimationCurve(keys);
            for (int i = 0; i < keys.Length; i++)
            {
                AnimationUtility.SetKeyLeftTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
                AnimationUtility.SetKeyRightTangentMode(curve, i, AnimationUtility.TangentMode.Linear);
            }

            EditorCurveBinding binding = EditorCurveBinding.FloatCurve(
                string.Empty, typeof(Transform), "localEulerAnglesRaw.x");
            AnimationUtility.SetEditorCurve(clip, binding, curve);

            AnimationClipSettings settings = AnimationUtility.GetAnimationClipSettings(clip);
            settings.loopTime = false;
            settings.loopBlend = false;
            AnimationUtility.SetAnimationClipSettings(clip, settings);
            AssetDatabase.CreateAsset(clip, WingClipPath);
        }

        if (controller == null)
        {
            controller = AnimatorController.CreateAnimatorControllerAtPath(WingControllerPath);
            AnimatorState state = controller.layers[0].stateMachine.AddState("ThrottleUpAnimation");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
        }
        else
        {
            AnimatorState state = controller.layers[0].stateMachine.states
                .Select(item => item.state)
                .FirstOrDefault(item => item.name == "ThrottleUpAnimation");
            if (state == null)
                state = controller.layers[0].stateMachine.AddState("ThrottleUpAnimation");
            state.motion = clip;
            controller.layers[0].stateMachine.defaultState = state;
            EditorUtility.SetDirty(controller);
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }

    private static AnimatorController BuildMixamoController(AnimationClip danceClip)
    {
        if (AssetDatabase.LoadAssetAtPath<AnimatorController>(MixamoControllerPath) != null)
            AssetDatabase.DeleteAsset(MixamoControllerPath);

        AnimatorController controller = AnimatorController.CreateAnimatorControllerAtPath(MixamoControllerPath);
        controller.AddParameter("IsDancing", AnimatorControllerParameterType.Bool);

        AnimatorStateMachine stateMachine = controller.layers[0].stateMachine;
        AnimatorState idle = stateMachine.AddState("Idle");
        AnimatorState dance = stateMachine.AddState("Swing Dancing");
        dance.motion = danceClip;
        stateMachine.defaultState = idle;

        AnimatorStateTransition idleToDance = idle.AddTransition(dance);
        idleToDance.hasExitTime = false;
        idleToDance.duration = 0.12f;
        idleToDance.AddCondition(AnimatorConditionMode.If, 0f, "IsDancing");

        AnimatorStateTransition danceToIdle = dance.AddTransition(idle);
        danceToIdle.hasExitTime = false;
        danceToIdle.duration = 0.12f;
        danceToIdle.AddCondition(AnimatorConditionMode.IfNot, 0f, "IsDancing");

        EditorUtility.SetDirty(controller);
        AssetDatabase.SaveAssets();
        return controller;
    }

    private static void CreateDanceStage()
    {
        GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
        ground.name = "DanceStage";
        ground.transform.position = new Vector3(0f, -0.03f, 0f);
        ground.transform.localScale = new Vector3(1.5f, 1f, 1.5f);

        GameObject lightObject = new GameObject("Directional Light");
        Light light = lightObject.AddComponent<Light>();
        light.type = LightType.Directional;
        light.intensity = 1.4f;
        lightObject.transform.rotation = Quaternion.Euler(48f, -28f, 0f);

        GameObject cameraObject = new GameObject("Main Camera");
        cameraObject.tag = "MainCamera";
        Camera camera = cameraObject.AddComponent<Camera>();
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.10f, 0.13f, 0.19f);
        camera.fieldOfView = 40f;
        cameraObject.transform.position = new Vector3(0f, 1.45f, -6f);
        cameraObject.transform.rotation = Quaternion.Euler(6f, 0f, 0f);
        cameraObject.AddComponent<AudioListener>();
    }

    private static string AbsoluteAssetPath(string assetPath)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        return Path.Combine(projectRoot, assetPath.Replace('/', Path.DirectorySeparatorChar));
    }

    private static void EnsureFolder(string assetPath)
    {
        if (AssetDatabase.IsValidFolder(assetPath))
            return;

        int separator = assetPath.LastIndexOf('/');
        string parent = assetPath.Substring(0, separator);
        string folder = assetPath.Substring(separator + 1);
        if (!AssetDatabase.IsValidFolder(parent))
            EnsureFolder(parent);

        AssetDatabase.CreateFolder(parent, folder);
    }
}
#endif
