using NCore.Settings;
using System;
using UnityEngine;
using UnityEngine.UI;

public class NMainMenuController : MonoBehaviour
{
    public enum MenuState
    {
        none = 0,
        resume = 1,
        confirm_resume = 2,
        new_game = 3,
        confirm_new_game = 4,
        all_saves = 5,
        confirm_delete_save = 6,
        settings = 7,
        credits = 8,
        confirm_exit = 9,
        settings_application = 10,
        settings_display = 11,
        settings_audio = 12,
        settings_graphics = 13
    }
    public MenuState currentState = MenuState.none;
    public MenuState previousState = MenuState.none;

    public Text descriptor;
    public Text subdescriptor;
    public GameObject settingsPanel;
    public GameObject settingsApplicationPanel;
    public GameObject settingsDisplayPanel;
    public GameObject settingsGraphicsPanel;
    public GameObject settingsAudioPanel;

    public GameObject confirmResume;
    public GameObject confirmNewGame;
    public GameObject confirmExit;

    private void Start()
    {
        UpdateState(MenuState.none);
    }

    private void UpdateState(MenuState state)
    {
        switch (state)
        {
            case MenuState.none:
                descriptor.text = "";
                subdescriptor.text = "";
                break;
            case MenuState.new_game:
                 descriptor.text = "> NEW GAME";
                subdescriptor.text = "";
                break;
            case MenuState.all_saves:
                descriptor.text = "> SAVES";
                subdescriptor.text = "";
                break;
            case MenuState.settings:
                descriptor.text = "> SETTINGS";
                subdescriptor.text = "";
                StartSettingsEditor();
                break;
            case MenuState.credits:
                descriptor.text = "";
                subdescriptor.text = "";
                break;
            case MenuState.settings_application:
                subdescriptor.text = "> APPLICATION";
                break;
            case MenuState.settings_display:
                subdescriptor.text = "> DISPLAY";
                break;
            case MenuState.settings_audio:
                subdescriptor.text = "> AUDIO";
                break;
            case MenuState.settings_graphics:
                subdescriptor.text = "> GRAPHICS";
                break;
            case MenuState.resume:
                confirmResume.GetComponent<Animator>().SetBool("show", true);
                descriptor.text = "";
                break;
            case MenuState.confirm_new_game:
                confirmNewGame.GetComponent<Animator>().SetBool("show", true);
                descriptor.text = "";
                break;
            case MenuState.confirm_exit:
                confirmExit.GetComponent<Animator>().SetBool("show", true);
                descriptor.text = "";
                break;

        }

        previousState = currentState;
        currentState = state;

        bool settingsVisible()
        {
            return (currentState == MenuState.settings || currentState == MenuState.settings_application || currentState == MenuState.settings_display
                || currentState == MenuState.settings_graphics || currentState == MenuState.settings_audio);
        }
        settingsPanel.SetActive             (settingsVisible());
        settingsApplicationPanel.SetActive  (currentState == MenuState.settings_application);
        settingsDisplayPanel.SetActive      (currentState == MenuState.settings_display);
        settingsGraphicsPanel.SetActive     (currentState == MenuState.settings_graphics);
        settingsAudioPanel.SetActive        (currentState == MenuState.settings_audio);
    }

    #region Button calls
    public void ButtonCall(int state)
    {
        if (state >= 0)
        {
            UpdateState((MenuState)state);
        }
        else
        {
            confirmResume.GetComponent<Animator>().SetBool("show", false);
            confirmNewGame.GetComponent<Animator>().SetBool("show", false);
            confirmExit.GetComponent<Animator>().SetBool("show", false);
            UpdateState(previousState);
        }
    }
    public void Confirm()
    {
        switch (currentState)
        {
            case MenuState.confirm_resume:
                //resume game
                break;
            case MenuState.confirm_new_game:
                //new game
                break;
            case MenuState.confirm_exit:
                Application.Quit();
                break;
        }
    }

    public void ApplySettings()
    {
        Settings.currentSettings = newSettings;
    }
    #endregion

    [Serializable]
    public class SettingsPanelComponents
    {
        public Toggle autosaveToggle;

        public Dropdown resolutionDropdown;
        public Dropdown windowmodeDropdown;
        public Toggle vSyncToggle;

        public Toggle postprocessingToggle;
        public Toggle bloomToggle;
        public Toggle motionblurToggle;
        public Toggle ambientOcclusionToggle;
        public Toggle chromaticToggle;

        public Slider masterSlider;
        public Slider effectsSlider;
        public Slider musicSlider;
        public Slider primaryDialogSlider;
        public Slider secondaryDialogSlider;
    }

    public SettingsPanelComponents settingsElements;
    private Settings.SettingsData newSettings;
    private void StartSettingsEditor()
    {
        newSettings = Settings.currentSettings;

        newSettings.display.resolution = Settings.supportedResolutions[settingsElements.resolutionDropdown.value];
        newSettings.display.fullscreenMode = (FullScreenMode)settingsElements.windowmodeDropdown.value;
        newSettings.display.vSync = settingsElements.vSyncToggle;

        newSettings.graphics.enablePostProcessing = settingsElements.postprocessingToggle;
        newSettings.graphics.enableBloom = settingsElements.bloomToggle;
        newSettings.graphics.enableMotionBlur = settingsElements.motionblurToggle;
        newSettings.graphics.enableAO = settingsElements.ambientOcclusionToggle;
        newSettings.graphics.enableChromaticAberation = settingsElements.chromaticToggle;

        newSettings.audio.master = settingsElements.masterSlider.value;
        newSettings.audio.effects = settingsElements.effectsSlider.value;
        newSettings.audio.music = settingsElements.musicSlider.value;
        newSettings.audio.primaryDialog = settingsElements.primaryDialogSlider.value;
        newSettings.audio.secondaryDialog = settingsElements.secondaryDialogSlider.value;
    }
}
