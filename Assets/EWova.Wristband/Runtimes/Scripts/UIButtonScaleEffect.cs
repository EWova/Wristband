using System.Collections;

using UnityEngine;
using UnityEngine.EventSystems;

namespace EWova.Wristband
{
    public class UIButtonScaleEffect : MonoBehaviour,
        IPointerEnterHandler,
        IPointerExitHandler,
        IPointerDownHandler,
        IPointerUpHandler
    {
        public RectTransform Target;

        public float NormalScale = 1.0f;
        public float HoverScale = 1.1f;
        public float PressedScale = 0.9f;

        public float smoothSpeed = 15f;

        private float targetScale;
        private bool isHovering = false;

        private void OnValidate()
        {
            if (Target == null)
            {
                Target = GetComponent<RectTransform>();
            }
        }

        private void Start()
        {
            targetScale = NormalScale;
            Target.localScale = NormalScale * Vector3.one;
        }

        private void Update()
        {
            float currentScale = Target.localScale.x;
            if (currentScale != targetScale)
            {
                Target.localScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * smoothSpeed) * Vector3.one;
            }
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            isHovering = true;
            targetScale = HoverScale;
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            isHovering = false;
            targetScale = NormalScale;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            targetScale = PressedScale;
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            targetScale = isHovering ? HoverScale : NormalScale;
        }
    }
}
