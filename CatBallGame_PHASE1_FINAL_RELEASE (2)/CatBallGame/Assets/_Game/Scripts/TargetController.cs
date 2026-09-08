using System.Collections;
using UnityEngine;
using Game.Core;
namespace Game.Targets
{
    public class TargetController : MonoBehaviour
    {
        [SerializeField] private GameObject visualRoot;
        [SerializeField] private float hitScaleDuration=0.2f;
        private bool isHit; private Vector3 originalScale; private Coroutine feedbackRoutine;
        private Transform VisualTransform=>visualRoot!=null?visualRoot.transform:transform;
        private void Awake(){originalScale=VisualTransform.localScale;}
        public void Hit(){if(isHit)return;isHit=true;GameEvents.RaiseTargetHit(this);StopFeedbackRoutine();feedbackRoutine=StartCoroutine(HitFeedback());}
        private IEnumerator HitFeedback(){Transform t=VisualTransform;Vector3 squashed=originalScale*1.3f;float e=0f;while(e<hitScaleDuration){e+=Time.deltaTime;t.localScale=Vector3.Lerp(originalScale,squashed,e/Mathf.Max(hitScaleDuration,0.0001f));yield return null;}gameObject.SetActive(false);feedbackRoutine=null;}
        public void ResetTarget(){StopFeedbackRoutine();isHit=false;VisualTransform.localScale=originalScale;gameObject.SetActive(true);}
        private void StopFeedbackRoutine(){if(feedbackRoutine!=null){StopCoroutine(feedbackRoutine);feedbackRoutine=null;}}
    }
}
