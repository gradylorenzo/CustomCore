using NCore.Settings;
using System;
using System.Collections.Generic;
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
    private Resolution[] supportedResolutions;
    private FullScreenMode[] windowModes = new FullScreenMode[]
    {
        FullScreenMode.Windowed,
        FullScreenMode.MaximizedWindow,
        FullScreenMode.FullScreenWindow,
        FullScreenMode.ExclusiveFullScreen
    };
    private int currentResolutionIndex = 0;

    private void StartSettingsEditor()
    {
        newSettings = Settings.currentSettings;
        SetElements();
        GetResolutions();
        GetWindowmodes();
    }

    private void SetElements()
    {
        settingsElements.vSyncToggle.isOn = newSettings.display.vSync;

        settingsElements.postprocessingToggle.isOn = newSettings.graphics.enablePostProcessing;
        settingsElements.bloomToggle.isOn = newSettings.graphics.enableBloom;
        settingsElements.motionblurToggle.isOn = newSettings.graphics.enableMotionBlur;
        settingsElements.ambientOcclusionToggle.isOn = newSettings.graphics.enableAO;
        settingsElements.chromaticToggle.isOn = newSettings.graphics.enableChromaticAberation;

        settingsElements.masterSlider.value = newSettings.audio.master;
        settingsElements.effectsSlider.value = newSettings.audio.effects;
        settingsElements.musicSlider.value = newSettings.audio.music;
        settingsElements.primaryDialogSlider.value = newSettings.audio.primaryDialog;
        settingsElements.secondaryDialogSlider.value = newSettings.audio.secondaryDialog;
    }

    private void GetResolutions()
    {
        List<string> res = new List<string>();
        supportedResolutions = Screen.resolutions;
        for(int i = 0; i < supportedResolutions.Length; i++)
        {
            string option = supportedResolutions[i].width + "x" + supportedResolutions[i].height;
            res.Add(option);

            if(supportedResolutions[i].width == newSettings.display.resolution.width && supportedResolutions[i].height == newSettings.display.resolution.height)
            {
                currentResolutionIndex = i;
            }
        }

        settingsElements.resolutionDropdown.ClearOptions();
        settingsElements.resolutionDropdown.AddOptions(res);
        settingsElements.resolutionDropdown.value = currentResolutionIndex;
        settingsElements.resolutionDropdown.RefreshShownValue();
    }

    public void SelectedResolutionChanged(int index)
    {
        Resolution newResolution = supportedResolutions[index];
        newSettings.display.resolution = new Settings.Resolution(newResolution.width, newResolution.height);

        NDebug.Log(new NDebug.Info("Selected resolution changed to " + newSettings.display.resolution.width + " x " + newSettings.display.resolution.height));
    }

    public void GetWindowmodes()
    {
        int currentMode = 0;
        List<string> modes = new List<string>();
        settingsElements.windowmodeDropdown.ClearOptions();
        
        for(int i = 0; i < windowModes.Length; i++)
        {
            modes.Add(windowModes[i].ToString());

            if(windowModes[i] == Screen.fullScreenMode)
            {
                currentMode = i;
            }
        }

        settingsElements.windowmodeDropdown.AddOptions(modes);
        settingsElements.windowmodeDropdown.value = currentMode;
        settingsElements.windowmodeDropdown.RefreshShownValue();
    }

    public void SelectedWindowModeChanged(int index)
    {
        newSettings.display.fullscreenMode = windowModes[index];

        NDebug.Log(new NDebug.Info("Selected fullscreen mode changed to " + newSettings.display.fullscreenMode.ToString()));
    }

    public void ApplySettings()
    {
        newSettings.application.autosave = settingsElements.autosaveToggle.isOn;

        newSettings.display.vSync = settingsElements.vSyncToggle.isOn;

        newSettings.graphics.enablePostProcessing = settingsElements.postprocessingToggle.isOn;
        newSettings.graphics.enableBloom = settingsElements.bloomToggle.isOn;
        newSettings.graphics.enableMotionBlur = settingsElements.motionblurToggle.isOn;
        newSettings.graphics.enableAO = settingsElements.ambientOcclusionToggle.isOn;
        newSettings.graphics.enableChromaticAberation = settingsElements.chromaticToggle.isOn;

        newSettings.audio.master = settingsElements.masterSlider.value;
        newSettings.audio.effects = settingsElements.effectsSlider.value;
        newSettings.audio.music = settingsElements.musicSlider.value;
        newSettings.audio.primaryDialog = settingsElements.primaryDialogSlider.value;
        newSettings.audio.secondaryDialog = settingsElements.secondaryDialogSlider.value;
        Settings.ApplySettings(newSettings);

        NDebug.Log(new NDebug.Info(newSettings.graphics.enablePostProcessing.ToString()));
    }
}
