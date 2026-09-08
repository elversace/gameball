using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Game.Ball; using Game.Core; using Game.Targets; using Game.InputControl; using Game.CameraControl; using Game.UI;
namespace Game.Runtime
{
    public class Phase1Bootstrap : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)] static void Prepare(){Application.targetFrameRate=60;Screen.orientation=ScreenOrientation.LandscapeLeft;Screen.autorotateToLandscapeLeft=true;Screen.autorotateToLandscapeRight=false;Screen.autorotateToPortrait=false;Screen.autorotateToPortraitUpsideDown=false;}
        private void Awake(){Build();}
        private void Build()
        {
            if(FindObjectOfType<GameManager>()!=null)return;
            CreateGround(); BallController ball=CreateBall(); CreateCat(); CreateTarget(); TrajectoryPredictor trajectory=CreateTrajectory(ball); Camera cam=CreateCamera(); CreateLight();
            var gm=new GameObject("GameManager");gm.AddComponent<GameManager>();
            var input=new GameObject("AimInputController").AddComponent<AimInputController>();input.SetActiveBall(ball);input.SetTrajectory(trajectory);input.SetCamera(cam);
            CreateUI(); CreateEventSystem();
        }
        GameObject CreateGround(){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Ground";g.transform.position=new Vector3(0,-.5f,0);g.transform.localScale=new Vector3(30,1,30);Color(g,new Color32(85,184,90,255));return g;}
        BallController CreateBall(){var g=GameObject.CreatePrimitive(PrimitiveType.Sphere);g.name="Ball";g.transform.position=new Vector3(-6,1,0);g.transform.localScale=Vector3.one*.6f;Color(g,new Color32(244,231,161,255));var rb=g.AddComponent<Rigidbody>();rb.mass=1;rb.drag=.05f;rb.angularDrag=.2f;rb.interpolation=RigidbodyInterpolation.Interpolate;rb.collisionDetectionMode=CollisionDetectionMode.ContinuousDynamic;var mat=new PhysicMaterial("BallPhysics"){bounciness=.45f,dynamicFriction=.4f,staticFriction=.4f,frictionCombine=PhysicMaterialCombine.Average,bounceCombine=PhysicMaterialCombine.Maximum};g.GetComponent<SphereCollider>().sharedMaterial=mat;return g.AddComponent<BallController>();}
        void CreateCat(){var g=GameObject.CreatePrimitive(PrimitiveType.Capsule);g.name="Cat_Placeholder";g.transform.position=new Vector3(-7.5f,1,0);g.transform.localScale=new Vector3(.8f,1,.8f);Color(g,new Color32(217,79,120,255));Destroy(g.GetComponent<Collider>());}
        void CreateTarget(){var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name="Target_01";g.transform.position=new Vector3(6,1,0);g.transform.localScale=Vector3.one*1.2f;Color(g,new Color32(217,79,120,255));var rb=g.AddComponent<Rigidbody>();rb.isKinematic=true;g.AddComponent<TargetController>();}
        TrajectoryPredictor CreateTrajectory(BallController ball){var g=new GameObject("TrajectoryLine");var lr=g.AddComponent<LineRenderer>();lr.widthMultiplier=.08f;lr.material=new Material(Shader.Find("Sprites/Default"));lr.startColor=Color.white;lr.endColor=new Color(1,1,1,.15f);lr.positionCount=0;lr.useWorldSpace=true;var t=g.AddComponent<TrajectoryPredictor>();t.SetBallRadius(ball.Radius);return t;}
        Camera CreateCamera(){var g=new GameObject("Main Camera");g.tag="MainCamera";var c=g.AddComponent<Camera>();c.orthographic=false;c.fieldOfView=45;g.transform.position=new Vector3(-1,4,-12);g.transform.rotation=Quaternion.Euler(12,0,0);g.AddComponent<AudioListener>();var controller=g.AddComponent<CameraController>();controller.SetFollowTarget(FindObjectOfType<BallController>()?.transform);return c;}
        void CreateLight(){var g=new GameObject("Directional Light");var l=g.AddComponent<Light>();l.type=LightType.Directional;l.intensity=1.1f;g.transform.rotation=Quaternion.Euler(50,-30,0);}
        void CreateUI(){var cgo=new GameObject("UICanvas");var c=cgo.AddComponent<Canvas>();c.renderMode=RenderMode.ScreenSpaceOverlay;var s=cgo.AddComponent<CanvasScaler>();s.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;s.referenceResolution=new Vector2(1920,1080);s.matchWidthOrHeight=1;s.referencePixelsPerUnit=100;cgo.AddComponent<GraphicRaycaster>();var win=Panel(cgo.transform,"WinPanel",new Color(0.33f,.72f,.35f,.9f));var fail=Panel(cgo.transform,"FailPanel",new Color(.85f,.31f,.47f,.9f));var wb=Button(win.transform);var fb=Button(fail.transform);win.SetActive(false);fail.SetActive(false);var ui=cgo.AddComponent<UIManager>();ui.Configure(win,fail,wb,fb);}
        GameObject Panel(Transform p,string name,Color col){var g=new GameObject(name,typeof(RectTransform));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;g.AddComponent<Image>().color=col;return g;}
        Button Button(Transform p){var g=new GameObject("RetryButton",typeof(RectTransform));g.transform.SetParent(p,false);var r=g.GetComponent<RectTransform>();r.sizeDelta=new Vector2(280,90);r.anchorMin=r.anchorMax=new Vector2(.5f,.5f);r.anchoredPosition=new Vector2(0,-150);g.AddComponent<Image>().color=new Color32(244,231,161,255);var b=g.AddComponent<Button>();var tg=new GameObject("Label",typeof(RectTransform));tg.transform.SetParent(g.transform,false);var tr=tg.GetComponent<RectTransform>();tr.anchorMin=Vector2.zero;tr.anchorMax=Vector2.one;tr.offsetMin=tr.offsetMax=Vector2.zero;var text=tg.AddComponent<Text>();text.text="Retry";text.alignment=TextAnchor.MiddleCenter;text.color=Color.black;text.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");return b;}
        void CreateEventSystem(){if(FindObjectOfType<EventSystem>()!=null)return;var g=new GameObject("EventSystem");g.AddComponent<EventSystem>();g.AddComponent<StandaloneInputModule>();}
        void Color(GameObject g,Color c){var r=g.GetComponent<Renderer>();if(r==null)return;var m=new Material(Shader.Find("Standard")??Shader.Find("Universal Render Pipeline/Lit"));m.color=c;r.sharedMaterial=m;}
    }
}
