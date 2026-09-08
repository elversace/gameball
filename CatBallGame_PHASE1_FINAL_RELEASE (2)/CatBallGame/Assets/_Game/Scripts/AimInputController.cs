using UnityEngine;
using Game.Core;
using Game.Ball;

namespace Game.InputControl
{
    public class AimInputController : MonoBehaviour
    {
        [SerializeField] private BallController activeBall;
        [SerializeField] private TrajectoryPredictor trajectoryPredictor;
        [SerializeField] private Camera gameCamera;
        [SerializeField] private float minPower = 4f;
        [SerializeField] private float maxPower = 14f;
        [SerializeField] private float maxDragDistancePixels = 300f;
        [SerializeField] private float minDragToRegisterPixels = 5f;
        [SerializeField, Range(0f,1f)] private float minArcBias = 0.70f;
        [SerializeField] private float ballGrabRadiusPixels = 120f;
        private bool isDragging; private Vector2 dragStartScreenPos, currentDragScreenPos;
        private void Awake() { if (gameCamera == null) gameCamera = Camera.main; }
        private void Update()
        {
            if (GameManager.Instance == null || !GameManager.Instance.CanAim || activeBall == null || gameCamera == null) return;
            if (Input.touchCount > 0)
            {
                Touch t = Input.GetTouch(0);
                HandlePointer(t.position, t.phase == TouchPhase.Began, t.phase == TouchPhase.Moved || t.phase == TouchPhase.Stationary, t.phase == TouchPhase.Ended || t.phase == TouchPhase.Canceled);
            }
            else
            {
                if (Input.GetMouseButtonDown(0)) HandlePointer(Input.mousePosition, true, false, false);
                else if (Input.GetMouseButton(0)) HandlePointer(Input.mousePosition, false, true, false);
                else if (Input.GetMouseButtonUp(0)) HandlePointer(Input.mousePosition, false, false, true);
            }
        }
        private void HandlePointer(Vector2 screenPos, bool began, bool moved, bool ended)
        {
            if (began)
            {
                Vector3 ballScreen = gameCamera.WorldToScreenPoint(activeBall.transform.position);
                if (ballScreen.z <= 0f || Vector2.Distance(screenPos, new Vector2(ballScreen.x, ballScreen.y)) > ballGrabRadiusPixels) return;
                isDragging = true; dragStartScreenPos = screenPos; currentDragScreenPos = screenPos;
            }
            else if (moved && isDragging) { currentDragScreenPos = screenPos; UpdateAimPreview(); }
            else if (ended && isDragging) { currentDragScreenPos = screenPos; ReleaseThrow(); isDragging = false; trajectoryPredictor?.Hide(); }
        }
        private Vector2 GetDragVector() { Vector2 raw = dragStartScreenPos - currentDragScreenPos; float m = Mathf.Min(raw.magnitude, maxDragDistancePixels); return raw.sqrMagnitude > 0.000001f ? raw.normalized * m : Vector2.zero; }
        private Vector3 ComputeLaunchVelocity()
        {
            Vector2 drag = GetDragVector(); float ratio = Mathf.Clamp01(drag.magnitude / maxDragDistancePixels); float power = Mathf.Lerp(minPower, maxPower, ratio); Vector2 norm = drag.normalized;
            float effectiveY = Mathf.Max(norm.y, minArcBias);
            Vector3 direction = gameCamera.transform.right * norm.x + gameCamera.transform.up * effectiveY;
            return direction.sqrMagnitude > 0.0001f ? direction.normalized * power : gameCamera.transform.up * power;
        }
        private void UpdateAimPreview() { if (GetDragVector().magnitude < minDragToRegisterPixels) trajectoryPredictor?.Hide(); else trajectoryPredictor?.ShowPreview(activeBall.transform.position, ComputeLaunchVelocity()); }
        private void ReleaseThrow() { if (GetDragVector().magnitude < minDragToRegisterPixels) return; activeBall.Throw(ComputeLaunchVelocity()); GameEvents.RaiseBallThrown(); }
        public void SetActiveBall(BallController ball) => activeBall = ball;
        public void SetTrajectory(TrajectoryPredictor predictor) => trajectoryPredictor = predictor;
        public void SetCamera(Camera cam) => gameCamera = cam;
    }
}
