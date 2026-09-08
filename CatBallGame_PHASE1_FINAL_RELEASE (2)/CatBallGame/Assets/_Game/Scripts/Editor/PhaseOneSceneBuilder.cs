#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Core;
using Game.Ball;
using Game.Targets;
using Game.InputControl;
using Game.CameraControl;
using Game.UI;

namespace Game.EditorTools
{
    /// <summary>
    /// Builds the Phase 1 prototype scene from code: Camera, Ground, Cat
    /// placeholder, Ball, Target, input/trajectory, GameManager and a
    /// minimal Win/Fail UI, then saves it. This exists because scene
    /// assembly (GameObject hierarchy + Inspector wiring) can only be done
    /// inside the Unity Editor itself — run the menu item below once you've
    /// imported this Assets/_Game folder into a Unity project.
    /// </summary>
    public static class PhaseOneSceneBuilder
    {
        private const string SceneFolder = "Assets/_Game/Scenes";
        private const string ScenePath = SceneFolder + "/Phase1_Prototype.unity";

        private static readonly Color CgiGreen = new Color32(0x55, 0xB8, 0x5A, 0xFF);
        private static readonly Color DarkPink = new Color32(0xD9, 0x4F, 0x78, 0xFF);
        private static readonly Color PaleYellow = new Color32(0xF4, 0xE7, 0xA1, 0xFF);

        [MenuItem("Tools/Cat Ball Game/Build Phase 1 Scene")]
        public static void BuildScene()
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            CreateGround();
            BallController ball = CreateBall(out float ballRadius);
            CreateCatPlaceholder();
            CreateTarget();
            TrajectoryPredictor trajectory = CreateTrajectoryLine();
            Camera cam = CreateCamera(ball.transform);
            CreateDirectionalLight();

            var gameManagerGO = new GameObject("GameManager");
            gameManagerGO.AddComponent<GameManager>();

            var inputGO = new GameObject("AimInputController");
            AimInputController aimInput = inputGO.AddComponent<AimInputController>();
            WireAimInput(aimInput, ball, trajectory, cam);
            WireTrajectoryBallRadius(trajectory, ballRadius);

            CreateUICanvas();
            CreateEventSystem();
            ConfigureLandscapeOrientation();

            EnsureFolder(SceneFolder);
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene, ScenePath);

            Debug.Log("[CatBallGame] Phase 1 scene built and saved at " + ScenePath +
                       ". Press Play, then click-drag from the ball (mouse in Editor, touch on device) and release to throw.");
        }

        private static GameObject CreateGround()
        {
            GameObject ground = GameObject.CreatePrimitive(PrimitiveType.Cube);
            ground.name = "Ground";
            ground.transform.position = new Vector3(0f, -0.5f, 0f);
            ground.transform.localScale = new Vector3(30f, 1f, 30f);
            ApplyColor(ground, CgiGreen);
            return ground;
        }

        private static BallController CreateBall(out float worldRadius)
        {
            GameObject ballGO = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            ballGO.name = "Ball";
            ballGO.transform.position = new Vector3(-6f, 1f, 0f);
            ballGO.transform.localScale = Vector3.one * 0.6f;
            ApplyColor(ballGO, PaleYellow);

            Rigidbody rb = ballGO.AddComponent<Rigidbody>();
            rb.mass = 1f;
            rb.drag = 0.05f;
            rb.angularDrag = 0.2f;
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // Default PhysicMaterial has ~0 bounciness, which reads as "the ball just stops" —
            // not the "ترتد" (bounces) behavior asked for. Give it real bounce.
            var bouncyMat = new PhysicMaterial("BallPhysics")
            {
                bounciness = 0.45f,
                dynamicFriction = 0.4f,
                staticFriction = 0.4f,
                frictionCombine = PhysicMaterialCombine.Average,
                bounceCombine = PhysicMaterialCombine.Maximum
            };
            var sphereCollider = ballGO.GetComponent<SphereCollider>();
            sphereCollider.sharedMaterial = bouncyMat;

            // Computed, not hardcoded, so the trajectory preview's SphereCast radius can
            // never silently drift out of sync with the ball actually being thrown.
            worldRadius = sphereCollider.radius * ballGO.transform.lossyScale.x;

            return ballGO.AddComponent<BallController>();
        }

        private static void CreateCatPlaceholder()
        {
            GameObject cat = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            cat.name = "Cat_Placeholder";
            cat.transform.position = new Vector3(-7.5f, 1f, 0f);
            cat.transform.localScale = new Vector3(0.8f, 1f, 0.8f);
            ApplyColor(cat, DarkPink);

            // Purely visual for now — no collider needed until Phase 5 animation/VFX work.
            Object.DestroyImmediate(cat.GetComponent<Collider>());
        }

        private static TargetController CreateTarget()
        {
            GameObject targetGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetGO.name = "Target_01";
            targetGO.transform.position = new Vector3(6f, 1f, 0f);
            targetGO.transform.localScale = new Vector3(1.2f, 1.2f, 1.2f);
            ApplyColor(targetGO, DarkPink);

            Rigidbody rb = targetGO.AddComponent<Rigidbody>();
            rb.isKinematic = true; // stays put, but still reports collisions with the dynamic ball

            return targetGO.AddComponent<TargetController>();
        }

        private static TrajectoryPredictor CreateTrajectoryLine()
        {
            var go = new GameObject("TrajectoryLine");
            var lr = go.AddComponent<LineRenderer>();
            lr.widthMultiplier = 0.08f;
            lr.material = new Material(Shader.Find("Sprites/Default"));
            lr.startColor = Color.white;
            lr.endColor = new Color(1f, 1f, 1f, 0.15f);
            lr.positionCount = 0;
            lr.useWorldSpace = true;

            return go.AddComponent<TrajectoryPredictor>();
        }

        private static Camera CreateCamera(Transform ballTransform)
        {
            var camGO = new GameObject("Main Camera");
            camGO.tag = "MainCamera";
            var cam = camGO.AddComponent<Camera>();
            cam.orthographic = false; // explicit — was previously relying on the engine default
            cam.fieldOfView = 45f;
            camGO.transform.position = new Vector3(-1f, 4f, -12f);
            camGO.transform.rotation = Quaternion.Euler(12f, 0f, 0f);
            camGO.AddComponent<AudioListener>();

            CameraController camController = camGO.AddComponent<CameraController>();
            var so = new SerializedObject(camController);
            so.FindProperty("followTarget").objectReferenceValue = ballTransform;
            so.ApplyModifiedProperties();

            return cam;
        }

        private static void CreateDirectionalLight()
        {
            var lightGO = new GameObject("Directional Light");
            var light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1.1f;
            lightGO.transform.rotation = Quaternion.Euler(50f, -30f, 0f);
        }

        private static void WireAimInput(AimInputController aimInput, BallController ball,
            TrajectoryPredictor trajectory, Camera cam)
        {
            var so = new SerializedObject(aimInput);
            so.FindProperty("activeBall").objectReferenceValue = ball;
            so.FindProperty("trajectoryPredictor").objectReferenceValue = trajectory;
            so.FindProperty("gameCamera").objectReferenceValue = cam;
            so.ApplyModifiedProperties();
        }

        private static void WireTrajectoryBallRadius(TrajectoryPredictor trajectory, float ballRadius)
        {
            var so = new SerializedObject(trajectory);
            so.FindProperty("ballRadius").floatValue = ballRadius;
            so.ApplyModifiedProperties();
        }

        private static void CreateUICanvas()
        {
            var canvasGO = new GameObject("UICanvas");
            var canvas = canvasGO.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = canvasGO.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); // landscape reference
            // Landscape-only game: height is far more consistent across devices than width,
            // so match by height (1) rather than the default match-by-width (0).
            scaler.matchWidthOrHeight = 1f;

            canvasGO.AddComponent<GraphicRaycaster>();

            GameObject winPanel = CreatePanel(canvasGO.transform, "WinPanel", new Color(CgiGreen.r, CgiGreen.g, CgiGreen.b, 0.9f));
            Button winRetry = CreateButton(winPanel.transform, "Retry");

            GameObject failPanel = CreatePanel(canvasGO.transform, "FailPanel", new Color(DarkPink.r, DarkPink.g, DarkPink.b, 0.9f));
            Button failRetry = CreateButton(failPanel.transform, "Retry");

            winPanel.SetActive(false);
            failPanel.SetActive(false);

            UIManager uiManager = canvasGO.AddComponent<UIManager>();
            var so = new SerializedObject(uiManager);
            so.FindProperty("winPanel").objectReferenceValue = winPanel;
            so.FindProperty("failPanel").objectReferenceValue = failPanel;
            so.FindProperty("retryButton").objectReferenceValue = winRetry;
            so.FindProperty("retryButtonFail").objectReferenceValue = failRetry;
            so.ApplyModifiedProperties();
        }

        private static GameObject CreatePanel(Transform parent, string name, Color color)
        {
            var panel = new GameObject(name, typeof(RectTransform));
            panel.transform.SetParent(parent, false);
            var rect = panel.GetComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            panel.AddComponent<Image>().color = color;
            return panel;
        }

        private static Button CreateButton(Transform parent, string label)
        {
            var btnGO = new GameObject("RetryButton", typeof(RectTransform));
            btnGO.transform.SetParent(parent, false);
            var rect = btnGO.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(280f, 90f);
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = new Vector2(0f, -150f);

            btnGO.AddComponent<Image>().color = PaleYellow;
            var button = btnGO.AddComponent<Button>();

            var textGO = new GameObject("Label", typeof(RectTransform));
            textGO.transform.SetParent(btnGO.transform, false);
            var textRect = textGO.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            var text = textGO.AddComponent<Text>();
            text.text = label;
            text.alignment = TextAnchor.MiddleCenter;
            text.color = Color.black;
            text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

            return button;
        }

        private static void CreateEventSystem()
        {
            // Without this, GraphicRaycaster + Buttons exist but nothing ever
            // dispatches pointer events to them — Retry would be unclickable.
            if (Object.FindObjectOfType<EventSystem>() != null) return;

            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>(); // matches the legacy Input Manager used by AimInputController
        }

        private static void ConfigureLandscapeOrientation()
        {
            // Fixed Landscape Left only — not AutoRotation. A fixed orientation makes the
            // allowedAutorotateTo* flags moot (Unity ignores them once defaultInterfaceOrientation
            // is a specific orientation rather than AutoRotation), so they're intentionally not set here.
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.LandscapeLeft;
        }

        private static void ApplyColor(GameObject go, Color color)
        {
            var renderer = go.GetComponent<Renderer>();
            if (renderer == null) return;

            Shader shader = Shader.Find("Standard") ?? Shader.Find("Universal Render Pipeline/Lit");
            var mat = new Material(shader) { color = color };
            renderer.sharedMaterial = mat;
        }

        private static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            string parent = Path.GetDirectoryName(path)?.Replace("\\", "/");
            string folderName = Path.GetFileName(path);

            if (!string.IsNullOrEmpty(parent) && !AssetDatabase.IsValidFolder(parent))
                EnsureFolder(parent);

            AssetDatabase.CreateFolder(parent, folderName);
        }
    }
}
#endif
