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
        public Transform LookAt;
        public float smoothTime = 0.2f;
        public bool followRotation = false;
        public Vector3 rotationPivotOffset = Vector3.zero;

        private Transform _pivot;
        private Vector3 _velocity;
        private Transform _transform;

        private void Start()
        {
            if (transform.parent != null)
            {
                if (_pivot == null)
                {
                    GameObject pivotObject = new GameObject("FollowerPivot");
                    pivotObject.transform.position = transform.position;
                    pivotObject.transform.rotation = transform.rotation;

                    _pivot = pivotObject.transform;
                }

                transform.SetParent(null, false);
            }
        }

        private void LateUpdate()
        {
            if (_pivot != null)
            {
                transform.position = Vector3.SmoothDamp(
                    transform.position,
                    _pivot.position,
                    ref _velocity,
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
    }
}
