using System.Collections;
using UnityEngine;
using Game.Core;
using Game.Targets;

namespace Game.Ball
{
    [RequireComponent(typeof(Rigidbody), typeof(SphereCollider))]
    public class BallController : MonoBehaviour
    {
        [SerializeField] private float settleLinearVelocityThreshold = 0.05f;
        [SerializeField] private float settleAngularVelocityThreshold = 0.5f;
        [SerializeField] private float settleCheckDelay = 0.6f;
        [SerializeField] private float maxFlightTime = 6f;
        [SerializeField] private float autoResetDelay = 0.4f;
        private Rigidbody rb;
        private Vector3 startPosition;
        private Quaternion startRotation;
        private bool isFlying;
        private Coroutine settleRoutine;
        public float Radius => GetComponent<SphereCollider>().radius * Mathf.Max(transform.lossyScale.x, 0.001f);
        private void Awake()
        {
            rb = GetComponent<Rigidbody>(); startPosition = transform.position; startRotation = transform.rotation; SetPhysicsActive(false);
        }
        private void OnEnable() { GameEvents.OnRetry += ResetBall; }
        private void OnDisable() { GameEvents.OnRetry -= ResetBall; }
        public void Throw(Vector3 velocity)
        {
            if (isFlying) return;
            SetPhysicsActive(true); rb.velocity = velocity; rb.angularVelocity = Vector3.zero; isFlying = true;
            if (settleRoutine != null) StopCoroutine(settleRoutine);
            settleRoutine = StartCoroutine(WatchForSettle());
        }
        private IEnumerator WatchForSettle()
        {
            float elapsed = 0f, stillTime = 0f; bool settledNaturally = false;
            while (elapsed < maxFlightTime)
            {
                elapsed += Time.deltaTime;
                bool below = rb.velocity.magnitude < settleLinearVelocityThreshold && rb.angularVelocity.magnitude < settleAngularVelocityThreshold;
                if (below) { stillTime += Time.deltaTime; if (stillTime >= settleCheckDelay) { settledNaturally = true; break; } }
                else stillTime = 0f;
                yield return null;
            }
            if (!settledNaturally) Debug.LogWarning($"[BallController] {name} forced settle after {maxFlightTime:F1}s; tune physics if frequent.");
            isFlying = false; settleRoutine = null; GameEvents.RaiseBallSettled(this);
            yield return new WaitForSeconds(autoResetDelay);
            ResetBall();
        }
        public void ResetBall()
        {
            if (settleRoutine != null) { StopCoroutine(settleRoutine); settleRoutine = null; }
            isFlying = false; SetPhysicsActive(false); transform.SetPositionAndRotation(startPosition, startRotation); rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero;
        }
        private void SetPhysicsActive(bool active) { rb.isKinematic = !active; rb.useGravity = active; }
        private void OnCollisionEnter(Collision collision)
        {
            var target = collision.collider.GetComponentInParent<TargetController>();
            if (target != null) target.Hit();
        }
    }
}
