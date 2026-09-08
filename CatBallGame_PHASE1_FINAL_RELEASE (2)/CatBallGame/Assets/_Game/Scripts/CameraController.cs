using UnityEngine;
namespace Game.CameraControl
{
    public class CameraController : MonoBehaviour
    {
        [SerializeField] private Transform followTarget;
        [SerializeField] private Vector3 followOffset = new Vector3(0f,2.5f,-6f);
        [SerializeField] private float followSpeed = 4f;
        [SerializeField] private bool followWhileFlying = false;
        private Vector3 restPosition; private Quaternion restRotation;
        private void Awake(){restPosition=transform.position;restRotation=transform.rotation;}
        private void LateUpdate(){Vector3 desired=followWhileFlying&&followTarget!=null?followTarget.position+followOffset:restPosition;transform.position=Vector3.Lerp(transform.position,desired,followSpeed*Time.deltaTime);if(!followWhileFlying)transform.rotation=Quaternion.Slerp(transform.rotation,restRotation,followSpeed*Time.deltaTime);}
        public void SetFollowTarget(Transform target)=>followTarget=target;
    }
}
