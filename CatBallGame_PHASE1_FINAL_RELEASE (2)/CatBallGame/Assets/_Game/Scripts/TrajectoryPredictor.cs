using UnityEngine;

namespace Game.Ball
{
    [RequireComponent(typeof(LineRenderer))]
    public class TrajectoryPredictor : MonoBehaviour
    {
        [SerializeField] private int pointCount = 36;
        [SerializeField] private float timeStep = 0.04f;
        [SerializeField] private float ballRadius = 0.3f;
        [SerializeField] private LayerMask collisionMask = ~0;
        private LineRenderer line;
        private Vector3[] pointsBuffer;
        private RaycastHit[] hitsBuffer;
        private void Awake()
        {
            line = GetComponent<LineRenderer>();
            line.positionCount = 0;
            pointsBuffer = new Vector3[Mathf.Max(2, pointCount)];
            hitsBuffer = new RaycastHit[16]; // reused every call — generous headroom for this scene's collider count
        }
        public void ShowPreview(Vector3 startPos, Vector3 velocity)
        {
            if (pointsBuffer == null || pointsBuffer.Length != Mathf.Max(2, pointCount))
                pointsBuffer = new Vector3[Mathf.Max(2, pointCount)];
            Vector3 pos = startPos, vel = velocity;
            int actualCount = 0;
            for (int i = 0; i < pointsBuffer.Length; i++)
            {
                pointsBuffer[i] = pos; actualCount = i + 1;
                Vector3 nextPos = pos + vel * timeStep + 0.5f * Physics.gravity * timeStep * timeStep;
                Vector3 segment = nextPos - pos;
                float distance = segment.magnitude;
                if (distance > 0.0001f)
                {
                    // NonAlloc (reused buffer) instead of SphereCastAll, which allocates a new
                    // array EVERY call — ShowPreview runs every Update while the player drags to
                    // aim, so an allocating API here means dozens of GC allocations per frame on
                    // exactly the mobile hot path this project has repeatedly had to fix elsewhere.
                    int hitCount = Physics.SphereCastNonAlloc(pos, Mathf.Max(0f, ballRadius), segment / distance, hitsBuffer, distance, collisionMask, QueryTriggerInteraction.Ignore);
                    int bestIndex = -1;
                    float bestDistance = float.MaxValue;
                    for (int h = 0; h < hitCount; h++)
                    {
                        if (hitsBuffer[h].collider == null) continue;
                        // Skip the ball's own collider: the first segment starts exactly at the
                        // ball's position, so Unity's default Physics.queriesStartInColliders=true
                        // would otherwise report an immediate self-hit at ~distance 0 on every aim.
                        if (hitsBuffer[h].collider.GetComponentInParent<BallController>() != null) continue;
                        if (hitsBuffer[h].distance < bestDistance) { bestDistance = hitsBuffer[h].distance; bestIndex = h; }
                    }
                    if (bestIndex >= 0)
                    {
                        pointsBuffer[i] = hitsBuffer[bestIndex].point;
                        break;
                    }
                }
                pos = nextPos; vel += Physics.gravity * timeStep;
            }
            line.positionCount = actualCount;
            line.SetPositions(pointsBuffer);
        }
        public void Hide() { if (line != null) line.positionCount = 0; }
        public void SetBallRadius(float radius) { ballRadius = Mathf.Max(0.001f, radius); }
    }
}
