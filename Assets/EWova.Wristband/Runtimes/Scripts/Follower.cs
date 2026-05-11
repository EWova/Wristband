using UnityEngine;

namespace EWova.Wristband
{
    public class Follower : MonoBehaviour
    {
        new private Transform transform
        {
            get
            {
                if (Application.isPlaying)
                {
                    if (_transform == null)
                        _transform = base.transform;
                    return _transform;
                }
                else
                {
                    return base.transform;
                }
            }
        }
        public Transform Pivot;
        public Transform LookAt;
        public float smoothTime = 0.2f;
        public bool followRotation = false;
        public Vector3 rotationPivotOffset = Vector3.zero;

        private Vector3 velocity;
        private Transform _transform;

        private void LateUpdate()
        {
            if (Pivot)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    Pivot.position,
                    ref velocity,
                    smoothTime
                );
            }

            if (LookAt && followRotation)
            {
                Vector3 targetDirection = LookAt.position - (transform.position + transform.rotation * rotationPivotOffset);
                if (targetDirection.sqrMagnitude > 0.001f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(-targetDirection);
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime / smoothTime);
                }
            }
        }

        //private void OnDrawGizmos()
        //{
        //    if (LookAt)
        //    {
        //        Gizmos.color = Color.blue;
        //        Gizmos.DrawSphere(transform.position, 0.1f);

        //        // draw offset
        //        Gizmos.color = Color.cyan;
        //        Vector3 offsetPos = transform.position + transform.rotation * rotationPivotOffset;
        //        Gizmos.DrawSphere(offsetPos, 0.1f);
        //    }
        //}
    }
}
