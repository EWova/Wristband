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
                    GameObject pivotObject = new("FollowerPivot");
                    pivotObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
                    pivotObject.transform.SetParent(transform.parent, true);
                    _pivot = pivotObject.transform;
                }

                bool worldPositionStays = true; // 保持Scale不變
                transform.SetParent(null, worldPositionStays);
            }
        }

        private void LateUpdate()
        {
            if (_pivot != null)
            {
                if (smoothTime < 0.0001f)
                {
                    transform.position = _pivot.position;
                }
                else
                {
                    transform.position = Vector3.SmoothDamp(
                    transform.position,
                    _pivot.position,
                    ref _velocity,
                    smoothTime);
                }
            }

            if (followRotation)
            {
                if (LookAt == null)
                    LookAt = Camera.main.transform;

                if (LookAt == null)
                    return;

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
