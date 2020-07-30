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

        //Some identifier where the user can send a screenshot
        //should a critical NDebug.Log be raised
        public const string DeveloperID = "Discord @Nyxton#6759";
    }

    namespace Managers
    {
        using NCore.Settings;
        using UnityEngine.SceneManagement;

        public static class GameManager
        {
            public static void Start(int i)
            {
                LoadSettings();
                LoadDefaultScene(i);
            }

            public static SettingsData currentSettings { get; private set; }
            private static void LoadSettings()
            {
                currentSettings = IO.Read();
                Settings.Apply(currentSettings);
            }

            private static void LoadDefaultScene(int i)
            {
                if(i > 0)
                {
                    LoadScene(i);
                }
            }
            public static void LoadScene(int index)
            {
                SceneManager.LoadScene(index);
            }
            public static void LoadScene(string name)
            {
                SceneManager.LoadScene(name);
            }

            public enum GameStates
            {
                playing = 0,
                paused = 1
            }
            public static GameStates gameState { get; private set; }
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

            public static void Apply(SettingsData data)
            {
                Display.SetResolution(data.display.width, data.display.height, data.display.fullscreenMode, data.display.vSync);
                Audio.SetVolume(data.audio.master, data.audio.effects, data.audio.music, data.audio.dialogPrimary, data.audio.dialogSecondary);
                Graphics.SetGraphics(data.graphics.enablePostProcessing, data.graphics.enableBloom, data.graphics.enableMotionBlur, data.graphics.enableAO, data.graphics.enableChromaticAberation);
                NDebug.Log(new NDebug.Info(NDebug.DebugType.message, "SUC_SETTINGS_APPLIED"));
                EventManager.UpdateSettings();
            }
        }

        public class SettingsData
        {
            public class ApplicationSettings
            {
                public bool autosave = true;
            }
            public class DisplaySettings
            {
                public int width = 1600;
                public int height = 900;
                public int vSync = 0;
                public FullScreenMode fullscreenMode = FullScreenMode.ExclusiveFullScreen;
            }
            public class AudioSettings
            {
                public float master = 0.0f;
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

            public ApplicationSettings application;
            public DisplaySettings display;
            public AudioSettings audio;
            public GraphicsSettings graphics;

            public SettingsData()
            {
                application = new ApplicationSettings();
                display = new DisplaySettings();
                audio = new AudioSettings();
                graphics = new GraphicsSettings();
            }
        }

        public static class IO
        {
            public static void Write(SettingsData settings)
            {
                XmlSerializer xs = new XmlSerializer(typeof(SettingsData));
                TextWriter tw = new StreamWriter(Config.SettingsFileName);
                try
                {
                    xs.Serialize(tw, settings);
                    tw.Close();
                }
                catch(ApplicationException e)
                {
                    NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_IO_WRITE_SETTINGS_FAILED: " + e.InnerException));
                }
            }

            public static bool SettingsExists()
            {
                return File.Exists(Config.SettingsFileName);
            }

            public static SettingsData Read()
            {
                SettingsData newSettings = new SettingsData();
                if (File.Exists(Config.SettingsFileName))
                {
                    SettingsData tmp = new SettingsData();

                    XmlSerializer xs = new XmlSerializer(typeof(SettingsData));
                    TextReader tr = new StreamReader(Config.SettingsFileName);

                    try
                    {
                        tmp = (SettingsData)xs.Deserialize(tr);
                    }
                    catch (ApplicationException e)
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_IO_CANNOT_DESERIALIZE_SETTINGS"));
                        Debug.LogError(e.InnerException);
                        tr.Close();
                    }
                    finally
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.message, "SUC_IO_DESERIALIZED_SETTINGS"));
                        tr.Close();
                        newSettings = tmp;
                    }
                }
                else
                {
                    NDebug.Log(new NDebug.Info(NDebug.DebugType.warning, "ERR_IO_SETTINGS_NOT_FOUND_WRITING_NEW"));
                    newSettings = new SettingsData();
                    Write(newSettings);
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
                            NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_IO_FAILED_TO_DESERIALIZE: " + filename));
                            Debug.LogError(e.InnerException);
                            tr.Close();
                        }
                        finally
                        {
                            NDebug.Log( new NDebug.Info(NDebug.DebugType.message, "SUC_IO_DESERIALIZED: " + filename));
                            tr.Close();
                            newSaveData = tmp;
                            playerName = filename;
                        }

                        saveData = newSaveData;
                    }
                    else
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.warning, "ERR_IO_NO_FILE_EXISTS: " + filename));
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
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.warning, "ERR_FILE_ALREADY_EXISTS: " + filename));
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
                            NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_IO_NO_SAVE_DATA_TO_WRITE"));
                            throw new Exception("ERR_IO_NO_SAVE_DATA_TO_WRITE");
                        }
                    }
                    else
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_NO_FILE_EXISTS: " + playerName));
                        throw new Exception("ERR_NO_FILE_EXISTS");
                    }
                }

                //Clears the currently loaded playerName and saveData
                public static void Clear()
                {
                    //Clears save data currently cached in saveData
                    saveData = null;
                    playerName = null;

                    NDebug.Log(new NDebug.Info(NDebug.DebugType.message, "SUC_IO_CLEARED_CACHE"));
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
                            NDebug.Log(new NDebug.Info(NDebug.DebugType.error, e.InnerException.ToString()));
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

    namespace MoreTransform
    {
        [Serializable]
        public struct DoubleVector3
        {
            #region Members
            public double x;
            public double y;
            public double z;
            private double _magnitude { get; set; }
            #endregion

            #region Properties
            public double magnitude
            {
                get { return _magnitude; }
                set { _magnitude = value; }
            }
            #endregion

            #region Constructors
            public DoubleVector3(double X, double Y, double Z)
            {
                x = X;
                y = Y;
                z = Z;
                _magnitude = Math.Sqrt(x * x + y * y + z * z);
            }
            #endregion

            #region Static Properties
            public static DoubleVector3 zero
            {
                get { return new DoubleVector3(0, 0, 0); }
            }

            public static DoubleVector3 up
            {
                get { return new DoubleVector3(0, 1, 0); }
            }

            public static DoubleVector3 right
            {
                get { return new DoubleVector3(1, 0, 0); }
            }

            public static DoubleVector3 forward
            {
                get { return new DoubleVector3(0, 0, 1); }
            }
            #endregion

            #region Static Methods

            //Lerp
            public static DoubleVector3 Lerp(DoubleVector3 a, DoubleVector3 b, float c)
            {
                double x = (a.x + c * (b.x - a.x));
                double y = (a.y + c * (b.y - a.y));
                double z = (a.z + c * (b.z - a.z));

                return new DoubleVector3(x, y, z);
            }

            //MoveTowards
            public static DoubleVector3 MoveTowards(DoubleVector3 a, DoubleVector3 b, double c)
            {
                DoubleVector3 newPos = b - a;
                double magnitude = newPos.magnitude;
                if (magnitude <= c || magnitude == 0f)
                    return b;
                return a + newPos / magnitude * c;
            }

            //FromSingleV3
            public static DoubleVector3 FromVector3(Vector3 v)
            {
                return new DoubleVector3(v.x, v.y, v.z);
            }

            //Distance
            public static double Distance(DoubleVector3 a, DoubleVector3 b)
            {
                return Math.Sqrt(((b.x - a.x) * (b.x - a.x)) + ((b.y - a.y) * (b.y - a.y)) + ((b.z - a.z) * (b.z - a.z)));
            }

            //ToSingleV3
            public static Vector3 ToVector3(DoubleVector3 v)
            {
                float x = Convert.ToSingle(v.x);
                float y = Convert.ToSingle(v.y);
                float z = Convert.ToSingle(v.z);

                return new Vector3(x, y, z);
            }


            #endregion

            #region Operators
            public static DoubleVector3 operator +(DoubleVector3 a, DoubleVector3 b)
            {
                DoubleVector3 v = new DoubleVector3(a.x + b.x, a.y + b.y, a.z + b.z);
                return v;
            }
            public static DoubleVector3 operator -(DoubleVector3 a, DoubleVector3 b)
            {
                DoubleVector3 v = new DoubleVector3(a.x - b.x, a.y - b.y, a.z - b.z);
                return v;
            }
            public static DoubleVector3 operator *(DoubleVector3 a, DoubleVector3 b)
            {
                DoubleVector3 v = new DoubleVector3(a.x * b.x, a.y * b.y, a.z * b.z);
                return v;
            }
            public static DoubleVector3 operator *(DoubleVector3 a, float b)
            {
                DoubleVector3 v = new DoubleVector3(a.x * b, a.y * b, a.z * b);
                return v;
            }

            public static DoubleVector3 operator *(DoubleVector3 a, double b)
            {
                DoubleVector3 v = new DoubleVector3(a.x * b, a.y * b, a.z * b);
                return v;
            }
            public static DoubleVector3 operator /(DoubleVector3 a, DoubleVector3 b)
            {
                DoubleVector3 v = new DoubleVector3(a.x / b.x, a.y / b.y, a.z / b.z);
                return v;
            }
            public static DoubleVector3 operator /(DoubleVector3 a, double b)
            {
                DoubleVector3 v = new DoubleVector3(a.x / b, a.y / b, a.z / b);
                return v;
            }
            public static bool operator ==(DoubleVector3 a, DoubleVector3 b)
            {
                if (a.x == b.x && a.y == b.y && a.z == b.z)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            public static bool operator !=(DoubleVector3 a, DoubleVector3 b)
            {
                if (a.x != b.x || a.y != b.y || a.z != b.z)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            #endregion

            #region Overrides
            public override bool Equals(object obj)
            {
                if (obj == null || this.GetType() != obj.GetType())
                {
                    return false;
                }
                return (this.x == ((DoubleVector3)obj).x && this.y == ((DoubleVector3)obj).y && this.z == ((DoubleVector3)obj).z);
            }
            public override int GetHashCode()
            {
                return this.GetHashCode();
            }
            #endregion
        }
    }
}