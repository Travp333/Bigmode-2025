using System.Collections;
using UnityEngine;

namespace Scripting
{
    public class MusicSwitcher : MonoBehaviour
    {
        [SerializeField] private AudioSource RAGEMODE;
        public static MusicSwitcher Instance;
        [SerializeField] private AudioSource dayMan;
        [SerializeField] private AudioSource nightMan;
        [SerializeField] private float transitionTime = 2.0f;
        private bool _switchingToDay;
        private bool _switchingToNight;
        private float _progress;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
            }
            else
            {
                Destroy(this);
            }
        }

        private bool _isFasterMusic;

        private IEnumerator LerpPitch(float targetPitch, float duration)
        {
            var startPitch = dayMan.pitch;
            var elapsedTime = 0f;

            while (elapsedTime < duration)
            {
                elapsedTime += Time.deltaTime;
                dayMan.pitch = Mathf.Lerp(startPitch, targetPitch, elapsedTime / duration);
                yield return null;
            }

            dayMan.pitch = targetPitch; // Ensure final value is set
        }

        private void Update()
        {
            if (!GameManager.Singleton.IsNightTime && GameManager.Singleton.PercentTimeLeft <= 0.25)
            {
                if (!_isFasterMusic)
                {
                    _isFasterMusic = true;
                    StartCoroutine(LerpPitch(1.1f, 0.5f));
                }
            }
            else
            {
                if (_isFasterMusic)
                {
                    _isFasterMusic = false;
                    StartCoroutine(LerpPitch(1f, 0.5f));
                }
            }

            if (_switchingToDay)
            {
                if (_progress > transitionTime)
                {
                    nightMan.Stop(); // = 0.0f;
                    dayMan.volume = 1.0f;
                    _switchingToDay = false;
                }
                else
                {
                    nightMan.volume =
                        Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, (transitionTime - _progress) / transitionTime));
                    dayMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, _progress / transitionTime));
                }
            }
            else if (_switchingToNight)
            {
                if (_progress > transitionTime)
                {
                    dayMan.Stop(); // = 0.0f;
                    nightMan.volume = 1.0f;
                    _switchingToNight = false;
                }
                else
                {
                    dayMan.volume =
                        Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, (transitionTime - _progress) / transitionTime));
                    nightMan.volume = Mathf.Clamp01(Mathf.Lerp(0.0f, 1.0f, _progress / transitionTime));
                }
            }

            _progress += Time.deltaTime;
        }

        public void DayTime()
        {
            RAGEMODE.Stop();
            _switchingToDay = true;
            _progress = 0;
            dayMan.volume = 0.0f;
            dayMan.Play();
        }

        public void NightTime()
        {
            RAGEMODE.Stop();
            _switchingToNight = true;
            _progress = 0;
            nightMan.volume = 0.0f;
            nightMan.Play();
        }

        public void PlayRageMode()
        {
            RAGEMODE.Play();
            dayMan.Stop();
            nightMan.Stop();
        }
    }
}
