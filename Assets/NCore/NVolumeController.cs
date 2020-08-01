using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NCore;
using System;
using NCore.Settings;
using NCore.Managers;

[RequireComponent(typeof(AudioSource))]
public class NVolumeController : MonoBehaviour
{
    public enum AudioType
    {
        effect,
        music,
        primaryDialog,
        secondaryDialog
    }

    public AudioType audioType;
    private float baseVolume;
    private float settingsVolume;
    private float masterVolume;
    private AudioSource source;

    private void Awake()
    {
        EventManager.UpdateSettings += UpdateSettings;
    }

    private void Start()
    {
        source = GetComponent<AudioSource>();
        baseVolume = source.volume;
        UpdateSettings();
    }

    public void UpdateSettings()
    {
        masterVolume = Settings.currentSettings.audio.master;

        switch (audioType)
        {
            case AudioType.effect:
                settingsVolume = Settings.currentSettings.audio.effects;
                break;
            case AudioType.music:
                settingsVolume = Settings.currentSettings.audio.music;
                break;
            case AudioType.primaryDialog:
                settingsVolume = Settings.currentSettings.audio.primaryDialog;
                break;
            case AudioType.secondaryDialog:
                settingsVolume = Settings.currentSettings.audio.secondaryDialog;
                break;
        }

        source.volume = (baseVolume * settingsVolume) * masterVolume;
    }
}
