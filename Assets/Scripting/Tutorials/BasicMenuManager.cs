using System;
using Scripting.Player;
using UnityEngine;
using UnityEngine.Audio;

namespace Scripting.Tutorials
{
    public class BasicMenuManager : MonoBehaviour
    {
        [SerializeField]
        GameObject successScreen;

        [Header("Objects")]
        [SerializeField] private GameObject optionsPanel;

        [SerializeField] private GameObject mainMenuButtons;

        [Header("Mixer")]
        [SerializeField] private AudioMixer audioMixer;

        [SerializeField] private Movement movement;

        private float _oldMasterVolume;
        private float _oldMusicVolume;
        private float _oldSfxVolume;
        private float _oldSpeechVolume;

        private float _newMasterVolume;
        private float _newMusicVolume;
        private float _newSfxVolume;
        private float _newSpeechVolume;

        private float _oldSensitivity;
        private float _newSensitivity;


        private enum PauseStates
        {
            Paused,
            Options,
            Unpaused
        }

        private PauseStates state;

        public void Pause()
        {
            state = PauseStates.Unpaused;
            Time.timeScale = 1;
            mainMenuButtons.SetActive(false);
            optionsPanel.SetActive(false);
            movement.enabled = true;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            optionsPanel.SetActive(false);
        }

        private void Awake()
        {
            _oldSensitivity = _newSensitivity = movement.MouseSpeed / 6f;
            mainMenuButtons.SetActive(false);
            optionsPanel.SetActive(false);
            state = PauseStates.Unpaused;
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape) && !successScreen.activeInHierarchy)
            {
                if (state == PauseStates.Paused)
                {
                    if(!movement.IsSeated){
                        Cursor.lockState = CursorLockMode.Locked;
                        Cursor.visible = false;
                    }
                    else{
                        Cursor.lockState = CursorLockMode.None;
                        Cursor.visible = false;
                    }
                    state = PauseStates.Unpaused;
                    Time.timeScale = 1;
                    mainMenuButtons.SetActive(false);
                    optionsPanel.SetActive(false);
                    movement.enabled = true;
                    GameManager.Singleton.SetIsPauseMenu(false);
                }
                else if (state == PauseStates.Unpaused)
                {
                    state = PauseStates.Paused;
                    mainMenuButtons.SetActive(true);
                    Time.timeScale = 0;
                    movement.enabled = false;
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    GameManager.Singleton.SetIsPauseMenu(true);
                }
            }
        }


        public void SetMasterVolume(float volume)
        {
            _oldMasterVolume = volume;
            audioMixer.SetFloat("master", (float) Math.Log10(volume) * 20f);
        }

        public void SetMusicVolume(float volume)
        {
            _oldMusicVolume = volume;
            audioMixer.SetFloat("music", (float) Math.Log10(volume) * 20f);
        }

        public void SetSfxVolume(float volume)
        {
            _oldSfxVolume = volume;
            audioMixer.SetFloat("sfx", (float) Math.Log10(volume) * 20f);
        }

        public void SetSpeechVolume(float volume)
        {
            _oldSpeechVolume = volume;
            audioMixer.SetFloat("speech", (float) Math.Log10(volume) * 20f);
        }

        public void SetSensitivity(float sensitivity)
        {
            _oldSensitivity = sensitivity;
            movement.MouseSpeed = (sensitivity * 6f);
        }

        public void GoBackSettings()
        {
            mainMenuButtons.SetActive(true);
            optionsPanel.SetActive(false);
            mainMenuButtons.SetActive(true);
        }

        public void ConfirmSettings()
        {
            mainMenuButtons.SetActive(true);
            optionsPanel.SetActive(false);

            _oldMasterVolume = _newMasterVolume;
            _oldMusicVolume = _newMusicVolume;
            _oldSfxVolume = _newSfxVolume;
            _oldSpeechVolume = _newSpeechVolume;
            _oldSensitivity = _newSensitivity;

            GoBackSettings();
        }

        public void OptionsClicked()
        {
            mainMenuButtons.SetActive(false);
            Debug.Log("click");
            ShowOptionsPanel();
        }

        private void ShowOptionsPanel()
        {
            optionsPanel.SetActive(true);
        }
    
        public void ExitGame()
        {
            Application.Quit();
        }
    }
}
