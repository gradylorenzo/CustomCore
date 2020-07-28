using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using NCore.Managers;

namespace NCore
{
    public static class Config
    {
        //Name of the file used to store settings in XML format.
        //File will be placed next to the game's executable.
        public const string SettingsFileName = "settings";

        //Directory that save files will be placed in.
        public const string SavesFileLocation = "saves";
    }

    namespace Managers
    {
        public static class GameManager
        {
            public enum GameStates
            {
                playing = 0,
                paused = 1
            }

            public static GameStates gameState { get; private set; }

            //Changes the state of the game to the state provided.
            public static void ChangeGameState(GameStates newState)
            {
                gameState = newState;
            }
        }

        public static class EventManager
        {
            public delegate void GenericEvent();
            public static GenericEvent UpdateSettings;
        }

        public static class NDebug
        {
            public enum DebugEventType
            {
                message = 0,
                success = 1,
                warning = 2,
                error = 3
            }

            public delegate void GenericDebugEvent(DebugEventType type, float time, string message);
            public static GenericDebugEvent Log;

            public delegate void StaticDisplayInformationEvent(string name, string info);
            public static StaticDisplayInformationEvent UpdateSDI;
        }
    }

    namespace Settings
    {
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

        public static class IO
        {
            public static void Write(SettingsData settings)
            {
                XmlSerializer xs = new XmlSerializer(typeof(SettingsData));
                TextWriter tw = new StreamWriter(Config.SettingsFileName);

                xs.Serialize(tw, settings);
                tw.Close();
            }

            public static bool SettingsExists()
            {
                return File.Exists(Config.SettingsFileName);
            }

            public static SettingsData Read()
            {
                SettingsData newSettings = new SettingsData();
                SettingsData tmp = new SettingsData();

                XmlSerializer xs = new XmlSerializer(typeof(Settings));
                TextReader tr = new StreamReader(Config.SettingsFileName);

                try
                {
                    tmp = (SettingsData)xs.Deserialize(tr);
                }
                catch (ApplicationException e)
                {
                    NDebug.Log(NDebug.DebugEventType.error, Time.time, "Failed to deserialize settings");
                    Debug.LogError(e.InnerException);
                    tr.Close();
                }
                finally
                {
                    NDebug.Log(NDebug.DebugEventType.success, Time.time, "Successfully deserialized settings");
                    tr.Close();
                    newSettings = tmp;
                }

                return newSettings;
            }
        }
    }

    namespace Save
    {
        public static class Save
        {
            private static string playerName;
            public static SaveData saveData;
            public static class IO
            {
                //Read a file with the name "filename" and pass its data into saveData
                public static void Read(string filename)
                {
                    if (File.Exists(Config.SavesFileLocation + "/" + filename))
                    {
                        SaveData newSaveData = new SaveData();
                        SaveData tmp = new SaveData();

                        XmlSerializer xs = new XmlSerializer(typeof(SaveData));
                        TextReader tr = new StreamReader(Config.SavesFileLocation + "/" + filename);

                        try
                        {
                            tmp = (SaveData)xs.Deserialize(tr);
                        }
                        catch (ApplicationException e)
                        {
                            NDebug.Log(NDebug.DebugEventType.error, Time.time, "Failed to deserialize save data: " + filename);
                            Debug.LogError(e.InnerException);
                            tr.Close();
                        }
                        finally
                        {
                            NDebug.Log(NDebug.DebugEventType.success, Time.time, "Failed to deserialize save data: " + filename);
                            tr.Close();
                            newSaveData = tmp;
                            playerName = filename;
                        }

                        saveData = newSaveData;
                    }
                    else
                    {
                        NDebug.Log(NDebug.DebugEventType.warning, Time.time, "ERR_NO_FILE_EXISTS: " + filename);
                        throw new Exception("ERR_NO_FILE_EXISTS");
                    }
                }

                //Write a new file with name "filename"
                public static void WriteNew(string filename, string playername)
                {
                    SaveData newSaveData = new SaveData();

                    if (!File.Exists(Config.SavesFileLocation + "/" + filename))
                    {
                        XmlSerializer xs = new XmlSerializer(typeof(SaveData));
                        TextWriter tw = new StreamWriter(Config.SavesFileLocation + "/" + filename);

                        xs.Serialize(tw, newSaveData);
                        tw.Close();
                    }
                    else
                    {
                        NDebug.Log(NDebug.DebugEventType.warning, Time.time, "ERR_ALREADY_EXISTS: " + filename);
                        throw new Exception("ERR_FILE_ALREADY_EXISTS");
                    }
                }

                //Writes the current save data to the file named "playerName"
                public static void Write()
                {
                    if (File.Exists(Config.SavesFileLocation + "/" + playerName))
                    {
                        if (saveData != null && (playerName != null && playerName != ""))
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(SaveData));
                            TextWriter tw = new StreamWriter(Config.SavesFileLocation + "/" + playerName);

                            xs.Serialize(tw, saveData);
                            tw.Close();
                        }
                        else
                        {
                            NDebug.Log(NDebug.DebugEventType.error, Time.time, "ERR_NO_SAVE_DATA");
                            throw new Exception("ERR_NO_SAVE_DATA");
                        }
                    }
                    else
                    {
                        NDebug.Log(NDebug.DebugEventType.error, Time.time, "ERR_NO_FILE_EXISTS: " + playerName);
                        throw new Exception("ERR_NO_FILE_EXISTS");
                    }
                }

                //Clears the currently loaded playerName and saveData
                public static void Clear()
                {
                    //Clears save data currently cached in saveData
                    saveData = null;
                    playerName = null;

                    NDebug.Log(NDebug.DebugEventType.message, Time.time, "Cleared save cache");
                }

                //Get a list of filenames that can successfully be deserialized.
                public static string[] SavesList()
                {
                    string[] foundFiles = Directory.GetFiles(Config.SavesFileLocation);
                    List<string> validSaves = new List<string>();

                    foreach(string f in foundFiles)
                    {
                        try
                        {
                            SaveData tmpData = new SaveData();
                            XmlSerializer xs = new XmlSerializer(typeof(SaveData));
                            TextReader tr = new StreamReader(Config.SavesFileLocation + "/" + f);

                            tmpData = (SaveData)xs.Deserialize(tr);

                            tr.Close();
                        }
                        catch(ApplicationException e)
                        {
                            NDebug.Log(NDebug.DebugEventType.error, Time.time, e.InnerException.ToString());
                            Debug.LogError(e.InnerException);
                        }
                        finally
                        {
                            validSaves.Add(f);
                        }
                    }

                    return validSaves.ToArray();
                }
            }
        }

        public class SaveData
        {
            //DATA YOU WANT TO SAVE GOES HERE
        }
    }
}