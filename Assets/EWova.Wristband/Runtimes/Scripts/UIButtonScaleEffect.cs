using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace EWova.Wristband
{
    public class UIButtonScaleEffect : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        public RectTransform TransTarget;
        public Selectable Selectable;
        private RectTransform _targetTransform;

        public float NormalScale = 1.0f;
        public float HoverScale = 1.1f;
        public float PressedScale = 0.9f;

        public float smoothSpeed = 15f;

        private float targetScale;
        private bool isHovering = false;

        private void OnValidate()
        {
            if (Selectable == null)
            {
                Selectable = GetComponent<Selectable>();
            }
        }

        private void Start()
        {
            targetScale = NormalScale;
            _targetTransform = TransTarget != null ? TransTarget : Selectable.transform as RectTransform;
            _targetTransform.localScale = NormalScale * Vector3.one;
        }

        private bool _interactableLastFrame = true;
        private void Update()
        {
            if (Selectable.interactable != _interactableLastFrame)
            {
                _interactableLastFrame = Selectable.interactable;
                if (!_interactableLastFrame)
                    targetScale = NormalScale;
            }

            float currentScale = _targetTransform.localScale.x;
            if (currentScale != targetScale)
            {
                _targetTransform.localScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed) * Vector3.one;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            if (!_interactableLastFrame)
                return;
            isHovering = true;
            targetScale = HoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            if (!_interactableLastFrame)
                return;
            isHovering = false;
            targetScale = NormalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!_interactableLastFrame)
                return;
            targetScale = PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!_interactableLastFrame)
                return;
            targetScale = isHovering ? HoverScale : NormalScale;
        }
    }
}
