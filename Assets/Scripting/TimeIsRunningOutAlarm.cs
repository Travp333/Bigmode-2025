using UnityEngine;

namespace Scripting
{
    public class TimeIsRunningOutAlarm : MonoBehaviour
    {
        [SerializeField] private GameObject timeIsRunningOutText;
        [SerializeField] private GameObject clock;
        [SerializeField] private AudioSource whistleSound;

        [SerializeField] private GameObject pointer;

        private bool _isVisible;

        void Start()
        {
            clock.SetActive(false);
            HideText();
        }

        void Update()
        {
            if (!_isVisible && GameManager.Singleton.PercentTimeLeft < 0.25f && !GameManager.Singleton.IsNightTime)
            {
                _isVisible = true;
                clock.SetActive(true);

                timeIsRunningOutText.SetActive(true);
                whistleSound.Play();
                Invoke(nameof(HideText), 2f);
            }

            if (GameManager.Singleton.IsNightTime)
            {
                if (_isVisible)
                {
                    _isVisible = false;
                    clock.SetActive(false);
                    HideText();
                }
            }
        }

        public void SetDayTime(float value)
        {
            pointer.transform.localRotation = Quaternion.Euler(0, 0, -Mathf.Lerp(0, 360, value));
        }

        private void HideText()
        {
            timeIsRunningOutText.SetActive(false);
        }
    }
}
