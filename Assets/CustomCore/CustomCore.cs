using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

namespace CustomCore
{
    public static class Config
    {
        public const string SettingsFile = "settings";
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
            Settings.Display.SetResolution(display.width, display.height, display.fullscreenMode, display.vSync);
            Settings.Audio.SetVolume(audio.master, audio.effects, audio.music, audio.dialogPrimary, audio.dialogSecondary);
            Settings.Graphics.SetGraphics(graphics.enablePostProcessing, graphics.enableBloom, graphics.enableMotionBlur, graphics.enableAO, graphics.enableChromaticAberation);
        }
    }

    public static class Settings
    {
        public static class Display
        {
            public static void SetResolution(int w, int h, FullScreenMode m, int v)
            {
                Screen.SetResolution(w, h, m, v);
            }
        }
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
            public static bool enablePostProcessing;
            public static bool enableBloom;
            public static bool enableMotionBlur;
            public static bool enableAO;
            public static bool enableChromaticAberation;

            public static void SetGraphics(bool p, bool b, bool m, bool a, bool c)
            {
                enablePostProcessing = p;
                enableBloom = b;
                enableMotionBlur = m;
                enableAO = a;
                enableChromaticAberation = c;
            }
        }
    }

    public static class IO
    {
        public static void SaveSettings(SettingsData settings)
        {
            XmlSerializer xs = new XmlSerializer(typeof(SettingsData));
            TextWriter tw = new StreamWriter(Config.SettingsFile);

            xs.Serialize(tw, settings);
            tw.Close();
        }

        public static SettingsData LoadSettings()
        {
            SettingsData newSettings = new SettingsData();
            SettingsData tmp = new SettingsData();

            XmlSerializer xs = new XmlSerializer(typeof(Settings));
            TextReader tr = new StreamReader("Settings");

            try
            {
                tmp = (SettingsData)xs.Deserialize(tr);
            }
            catch (ApplicationException e)
            {
                //Notify.Notify.Error("Failed to deserialize Settings , see console for more details");
                Debug.LogError(e.InnerException);
                tr.Close();
            }
            finally
            {
                //Notify.Notify.Success("Successfully deserialized " + name);
                tr.Close();
                newSettings = tmp;
            }

            return newSettings;
        }
    }
}