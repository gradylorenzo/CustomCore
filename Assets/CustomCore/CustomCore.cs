using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CustomCore
{
    public static class Audio
    {
        public static float masterVolume = 0.25f;
        public static float effectsVolume = 1.0f;
        public static float musicVolume = 1.0f;
        public static float primaryDialogVolume = 1.0f;
        public static float secondaryDialogVolume = 1.0f;

        public static void SetVolume(float ma, float fx, float mu, float pr, float se)
        {
            masterVolume = ma;
            effectsVolume = fx;
            musicVolume = mu;
            primaryDialogVolume = pr;
            secondaryDialogVolume = se;
        }
    }

    public static class Graphics
    {

    }

    public class SettingsData
    {
        public class DisplaySettings
        {
            public int width = 1600;
            public int height = 900;
            public int vSync = 0;
            public FullScreenMode fullscreenMode = FullScreenMode.ExclusiveFullScreen;
        }
        public class AudioSettings
        {
            public float master = 0.25f;
            public float effects = 1.0f;
            public float music = 1.0f;
            public float dialogPrimary = 1.0f;
            public float dialogSecondary = 1.0f;
        }
        public class GraphicsSettings
        {
            public bool enablePostProcessing = false;
            public bool enableBloom = false;
            public bool enableMotionBlur = false;
            public bool enableAO = false;
            public bool enableChromaticAberation = false;
        }

        public DisplaySettings display;
        public AudioSettings audio;
        public GraphicsSettings graphics;

        public void Apply()
        {
            //Apply Display Settings
            Screen.SetResolution(display.width, display.height, display.fullscreenMode, display.vSync);

            //Apply Audio Settings
            Audio.SetVolume(audio.master, audio.effects, audio.music, audio.dialogPrimary, audio.dialogSecondary);
        }
    }

    public static class IO
    {
        
    }
}