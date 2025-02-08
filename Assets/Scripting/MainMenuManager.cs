using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Scripting
{
    public class MainMenuManager : MonoBehaviour
    {
        [Header("Objects")]
        [SerializeField] private Image fade;

        [Header("Cam Stuff")]
        [SerializeField] private CinemachineSplineDolly initDolly;

        [SerializeField] private CinemachineCamera optionsCamera;
        [SerializeField] private CinemachineSplineDolly optionsDolly;

        [SerializeField] private CinemachineCamera creditsCamera;
        [SerializeField] private CinemachineSplineDolly creditsDolly;

        [FormerlySerializedAs("optionsMenu")]
        [Header("Menu")]
        [SerializeField] private GameObject optionsPanel;

        [SerializeField] private GameObject creditsMenu;
        [SerializeField] private GameObject mainMenuButtons;
        [SerializeField] private GameObject uiBlock;

        [Header("Options")]
        [SerializeField] private AudioMixer audioMixer;
    
        [Header("Sliders")]
        [SerializeField] private Slider masterSlider;
        [SerializeField] private Slider musicSlider;
        [SerializeField] private Slider sfxSlider;
        [SerializeField] private Slider speechSlider;

        private float _oldMasterVolume = 1.0f;
        private float _oldMusicVolume = 1.0f;
        private float _oldSfxVolume = 1.0f;
        private float _oldSpeechVolume = 1.0f;

        private float _newMasterVolume;
        private float _newMusicVolume;
        private float _newSfxVolume;
        private float _newSpeechVolume;

        private void Awake()
        {
            mainMenuButtons.SetActive(false);
            optionsPanel.SetActive(false);
            creditsMenu.SetActive(false);

            audioMixer.SetFloat("master", (float) Math.Log10(_oldMasterVolume) * 20f);
            audioMixer.SetFloat("music", (float) Math.Log10(_oldMusicVolume) * 20f);
            audioMixer.SetFloat("sfx", (float) Math.Log10(_oldSfxVolume) * 20f);
            audioMixer.SetFloat("speech", (float) Math.Log10(_oldSpeechVolume) * 20f);
        
            masterSlider.value = _oldMasterVolume;
            musicSlider.value = _oldMusicVolume;
            sfxSlider.value = _oldSfxVolume;
            speechSlider.value = _oldSpeechVolume;
        
            fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, 1f);
            StartCoroutine(InitSpline());
        }

        private IEnumerator InitSpline()
        {
            var elapsedTime = 0f;
            while (elapsedTime < 0.99f)
            {
                elapsedTime += Time.deltaTime / 4f;

                var fadeValue = 1.0f - elapsedTime * 3;
                if (fadeValue < 0) fadeValue = 0;

                fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, fadeValue);

                initDolly.CameraPosition = elapsedTime;

                yield return null;
            }

            mainMenuButtons.SetActive(true);
        }

        private IEnumerator FadeOut(Action callback)
        {
            mainMenuButtons.SetActive(false);

            var elapsedTime = 0f;
            while (elapsedTime < 0.99f)
            {
                elapsedTime += Time.deltaTime / 4f;

                var fadeValue = elapsedTime * 3;
                if (fadeValue < 0) fadeValue = 0;

                fade.color = new Color(fade.color.r, fade.color.g, fade.color.b, fadeValue);

                yield return null;
            }

            callback();
        }

        public void SetMasterVolume(float volume)
        {
            _newMasterVolume = volume;
            audioMixer.SetFloat("master", (float) Math.Log10(volume) * 20f);
            Debug.Log(volume);
        }

        public void SetMusicVolume(float volume)
        {
            _newMusicVolume = volume;
            audioMixer.SetFloat("music", (float) Math.Log10(volume) * 20f);
        }

        public void SetSfxVolume(float volume)
        {
            _newSfxVolume = volume;
            audioMixer.SetFloat("sfx", (float) Math.Log10(volume) * 20f);
        }

        public void SetSpeechVolume(float volume)
        {
            _newSpeechVolume = volume;
            audioMixer.SetFloat("speech", (float) Math.Log10(volume) * 20f);
        }

        public void CancelSettings()
        {
            audioMixer.SetFloat("master", (float) Math.Log10(_oldMasterVolume) * 20f);
            audioMixer.SetFloat("music", (float) Math.Log10(_oldMusicVolume) * 20f);
            audioMixer.SetFloat("sfx", (float) Math.Log10(_oldSfxVolume) * 20f);
            audioMixer.SetFloat("speech", (float) Math.Log10(_oldSpeechVolume) * 20f);
        
            masterSlider.value = _oldMasterVolume;
            musicSlider.value = _oldMusicVolume;
            sfxSlider.value = _oldSfxVolume;
            speechSlider.value = _oldSpeechVolume;
        
            GoBackSettings();
        }

        public void GoBackCredits()
        {
            mainMenuButtons.SetActive(true);
            creditsMenu.SetActive(false);

            LerpTo(creditsDolly, -1f);
        
            Invoke(nameof(SetBackCameraFromCreditsIDontKnowHowIShouldCallThisFunctionSoItsJustSuperLongHelloTravis), 0.5f);
        }

        private void SetBackCameraFromCreditsIDontKnowHowIShouldCallThisFunctionSoItsJustSuperLongHelloTravis()
        {
            creditsCamera.Priority = 0;
        }
    
        private void GoBackSettings()
        {
            mainMenuButtons.SetActive(true);
            optionsPanel.SetActive(false);

            LerpTo(optionsDolly, -1f);
        }

        public void ConfirmSettings()
        {
            mainMenuButtons.SetActive(true);
            optionsPanel.SetActive(false);

            _oldMasterVolume = _newMasterVolume;
            _oldMusicVolume = _newMusicVolume;
            _oldSfxVolume = _newSfxVolume;
            _oldSpeechVolume = _newSpeechVolume;

            GoBackSettings();
        }

        public void OptionsClicked()
        {
            mainMenuButtons.SetActive(false);
            optionsCamera.Priority = 2;

            LerpTo(optionsDolly, 1f);
            Invoke(nameof(ShowOptionsPanel), 0.5f);
        }

        private void ShowOptionsPanel()
        {
            optionsPanel.SetActive(true);
        }

        private void LerpTo(CinemachineSplineDolly dolly, float speed)
        {
            dolly.AutomaticDolly = new SplineAutoDolly
            {
                Enabled = true,
                Method = new SplineAutoDolly.FixedSpeed
                {
                    Speed = speed
                }
            };
        }

        public void StartGame()
        {
            StartCoroutine(FadeOut(SwitchScene));
        }

        private void SwitchScene()
        {
            SceneManager.LoadScene("Office", LoadSceneMode.Single);
        }

        public void ShowCredits()
        {
        
            mainMenuButtons.SetActive(false);
        
            creditsCamera.Priority = 3;
        
            LerpTo(creditsDolly, 1f);
        
            Invoke(nameof(ShowCreditsPanel), 0.5f);
        }

        private void ShowCreditsPanel()
        {
            creditsMenu.SetActive(true);
        }

        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
