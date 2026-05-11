using System;

using TMPro;

using UnityEngine;

namespace EWova.Wristband
{
    public class WristbandUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI MinuteTMP;
        [SerializeField] private TextMeshProUGUI HourTMP;
        [SerializeField] private TextMeshProUGUI MidTMP;

        private void Update()
        {
            UpdateTimeDisplay();
        }

        private const float UpdateInterval = 0.5f;
        private float _timer;
        private bool _dotFresh;
        private void UpdateTimeDisplay()
        {
            _timer += Time.deltaTime;

            if (_timer < UpdateInterval)
                return;

            _timer -= UpdateInterval;

            var color = Color.white;
            color.a = _dotFresh ? 0.7f : 0.2f;
            MidTMP.color = color;
            _dotFresh = !_dotFresh;

            TimeSpan time = DateTime.Now.TimeOfDay;

            HourTMP.text = time.Hours.ToString("00");
            MinuteTMP.text = time.Minutes.ToString("00");
        }
    }
}
