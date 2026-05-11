using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

namespace EWova.Wristband
{
    [AddComponentMenu("Layout/Circle Group Layout")]
    public class CircleGroupLayout : LayoutGroup
    {
        public float radius = 100f;          // 半徑
        public float startAngle = 0f;        // 起始角度（度）
        public bool clockwise = true;        // 是否順時針
        public bool evenlySpaced = true;     // 是否平均分布

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal();
            Arrange();
        }

        public override void CalculateLayoutInputVertical()
        {
            Arrange();
        }

        public override void SetLayoutHorizontal() { }
        public override void SetLayoutVertical() { }

        private void Arrange()
        {
            int count = rectChildren.Count;
            if (count == 0) return;

            float angleStep = evenlySpaced ? 360f / count : 0f;

            for (int i = 0; i < count; i++)
            {
                float angle = startAngle;

                if (evenlySpaced)
                {
                    angle += (clockwise ? -1 : 1) * angleStep * i;
                }

                float rad = angle * Mathf.Deg2Rad;

                float x = Mathf.Cos(rad) * radius;
                float y = Mathf.Sin(rad) * radius;

                RectTransform child = rectChildren[i];

                SetChildAlongAxis(child, 0, x - child.rect.width * child.pivot.x);
                SetChildAlongAxis(child, 1, y - child.rect.height * child.pivot.y);
            }
        }
    }
}