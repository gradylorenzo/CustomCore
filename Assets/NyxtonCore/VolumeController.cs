using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NyxtonCore;
using System;
using NyxtonCore.Settings;
using NyxtonCore.Managers;

[RequireComponent(typeof(AudioSource))]
public class VolumeController : MonoBehaviour
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
        masterVolume = Settings.Audio.masterVolume;

        switch (audioType)
        {
            case AudioType.effect:
                settingsVolume = Settings.Audio.effectsVolume;
                break;
            case AudioType.music:
                settingsVolume = Settings.Audio.musicVolume;
                break;
            case AudioType.primaryDialog:
                settingsVolume = Settings.Audio.primaryDialogVolume;
                break;
            case AudioType.secondaryDialog:
                settingsVolume = Settings.Audio.secondaryDialogVolume;
                break;
        }

        source.volume = (baseVolume * settingsVolume) * masterVolume;
    }
}
