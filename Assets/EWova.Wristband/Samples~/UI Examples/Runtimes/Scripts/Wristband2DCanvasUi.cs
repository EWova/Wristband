using UnityEngine;

namespace EWova.Wristband.Samples.UiExamples
{
    public class Wristband2DCanvasUi : MonoBehaviour
    {
        public Wristband Wristband;
        public RectTransform RectTransform;

        public Vector2 DisableScreenSpacePosition = new Vector2(0.045f, 0.92f);
        public Vector2 EnableScreenSpacePosition = new Vector2(0.4f, 0.5f);

        public float DisableScale = 0.3f;
        public float EnableScale = 1.0f;

        private float _value;
        private float _size;

        private void Awake()
        {
            _size = RectTransform.localScale.x;
            Set(0);
        }

        private void LateUpdate()
        {
            float value = Wristband.MenuTransitionValue;
            if (Mathf.Approximately(value, _value))
                return;
            _value = value;
            Set(value);
        }

        private void Set(float oriValue)
        {
            float screenSpacePos = Mathf.Clamp01(oriValue);
            RectTransform.anchorMax = Vector2.LerpUnclamped(DisableScreenSpacePosition, EnableScreenSpacePosition, screenSpacePos);
            RectTransform.anchorMin = Vector2.LerpUnclamped(DisableScreenSpacePosition, EnableScreenSpacePosition, screenSpacePos);
            var scale = Mathf.LerpUnclamped(_size * DisableScale, _size * EnableScale, screenSpacePos);
            RectTransform.localScale = new Vector3(scale, scale, scale);
        }
    }
}