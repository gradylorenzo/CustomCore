using System;
using System.IO;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;
using NCore.Managers;

namespace NCore
{
    /// <summary>
    /// NCore.Config stores various options for NCore, including what the settings file is called, where save files are stored, and contact
    /// details that will be displayed should a critical NDebug.Log be called.
    /// </summary>
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
    /// <summary>
    /// Static class for GameManager, EventManager, etc.
    /// </summary>
    namespace Managers
    {
        using NCore.Settings;
        using UnityEngine.SceneManagement;

        /// <summary>
        /// GameManager will manage things like the state of the game (whether it's paused, playing, etc), loading of scenes,
        /// storage of the current settings.
        /// </summary>
        public static class GameManager
        {
            public static void Awake()
            {
                EventManager.UpdateSettings += UpdateSettings;
                EventManager.SaveFileLoaded += SaveFileLoaded;
            }

            public static void Start(int i)
            {
                LoadSettings();
                LoadDefaultScene(i);
            }
            private static void LoadSettings()
            {
                if (Settings.IO.SettingsExists())
                {
                    Settings.ApplySettings(Settings.IO.Read());
                }
            }
            private static void LoadDefaultScene(int i)
            {
                if(i > 0)
                {
                    LoadScene(i);
                }
            }
            public static void NoticeReadBeginGame()
            {
                Settings.ApplyDefaultSettings();
                LoadDefaultScene(1);
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

            //Event Handlers
            public static void UpdateSettings()
            {

            }

            public static void SaveFileLoaded ()
            {

            }
        }

        public static class EventManager
        {
            public delegate void GenericEvent();
            public static GenericEvent UpdateSettings;
            public static GenericEvent SaveFileLoaded;
        }
    }

    ///<summary>
    ///Namespace for handling settings file reading, writing, default settings, etc.
    namespace Settings
    {
        public static class Settings
        {
            public struct Resolution
            {
                public int width;
                public int height;

                public Resolution(int w, int h)
                {
                    width = w;
                    height = h;
                }
            }
      
            public static SettingsData currentSettings;

            public static void ApplySettings(SettingsData newSettings)
            {
                currentSettings = newSettings;
                currentSettings.Apply();
            }
            public static void LoadSettings()
            {
                currentSettings = IO.Read();
                currentSettings.Apply();
            }
            public static void ApplyDefaultSettings()
            {
                currentSettings = new SettingsData();
                currentSettings.application = new SettingsData.ApplicationSettings(true);
                currentSettings.display = new SettingsData.DisplaySettings(new Resolution(1600, 900), true, FullScreenMode.Windowed);
                currentSettings.graphics = new SettingsData.GraphicsSettings(true, true, true, true, true);
                currentSettings.audio = new SettingsData.AudioSettings(0.1f, 1.0f, 1.0f, 1.0f, 1.0f);

                currentSettings.Apply();
            }

            public struct SettingsData
            {
                public struct ApplicationSettings
                {
                    public bool autosave;

                    public void Apply()
                    {
                        NDebug.Log(new NDebug.Info("Application Settings Applied."));
                    }

                    public ApplicationSettings(bool a)
                    {
                        autosave = a;
                    }
                }
                public struct DisplaySettings
                {
                    public Resolution resolution;
                    public bool vSync;
                    public FullScreenMode fullscreenMode;

                    public void Apply()
                    {
                        int vSyncInt(bool v)
                        {
                            if (v)
                            {
                                return 1;
                            }
                            else
                            {
                                return 0;
                            }
                        }
                        Screen.SetResolution(resolution.width, resolution.height, fullscreenMode, vSyncInt(vSync));
                        NDebug.Log(new NDebug.Info("Display Settings Applied."));
                    }

                    public DisplaySettings(Resolution r, bool v, FullScreenMode f)
                    {
                        resolution = r;
                        vSync = v;
                        fullscreenMode = f;
                    }
                }
                public struct GraphicsSettings
                {
                    public bool enablePostProcessing;
                    public bool enableBloom;
                    public bool enableMotionBlur;
                    public bool enableAO;
                    public bool enableChromaticAberation;

                    public void Apply()
                    {
                        NDebug.Log(new NDebug.Info("Graphics Settings Applied."));
                    }

                    public GraphicsSettings(bool p, bool b, bool m, bool a, bool c)
                    {
                        enablePostProcessing = p;
                        enableBloom = b;
                        enableMotionBlur = m;
                        enableAO = a;
                        enableChromaticAberation = c;
                    }
                }
                public struct AudioSettings
                {
                    public float master;
                    public float effects;
                    public float music;
                    public float primaryDialog;
                    public float secondaryDialog;

                    public void Apply()
                    {
                        NDebug.Log(new NDebug.Info("Audio Settings Applied."));
                    }

                    public AudioSettings(float ma, float e, float mu, float p, float s)
                    {
                        master = ma;
                        effects = e;
                        music = mu;
                        primaryDialog = p;
                        secondaryDialog = s;
                    }
                }
                
                public ApplicationSettings application;
                public DisplaySettings display;
                public GraphicsSettings graphics;
                public AudioSettings audio;

                public void Apply()
                {
                    application.Apply();
                    display.Apply();
                    audio.Apply();
                    graphics.Apply();

                    IO.Write(this);
                    EventManager.UpdateSettings();
                }

                public SettingsData(ApplicationSettings app, DisplaySettings dis, GraphicsSettings gra, AudioSettings aud)
                {
                    application = app;
                    display = dis;
                    graphics = gra;
                    audio = aud;
                }
            }
            public static class IO
            {
                public static void Write(SettingsData settings)
                {
                    XmlSerializer xs = new XmlSerializer(typeof(SettingsData));
                    TextWriter tw = new StreamWriter(Config.SettingsFileName);
                    NDebug.Log(new NDebug.Info("ATT_IO_WRITE_SETTINGS"));
                    try
                    {
                        xs.Serialize(tw, settings);
                        tw.Close();
                    }
                    catch (ApplicationException e)
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_IO_WRITE_SETTINGS: " + e.InnerException));
                    }
                    finally
                    {
                        NDebug.Log(new NDebug.Info("SUC_IO_WRITE_SETTINGS"));
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
    }

    namespace Save
    {
        public static class Save
        {
            private static string currentFileName;
            public static SaveData currentSaveData;
            private static SaveData lastSaveData;

            public static string[] GetSavesList()
            {
                return SavesList();
            }

            public static void LoadSave(string filename)
            {
                currentSaveData = IO.Read(filename);
                lastSaveData = currentSaveData;

                if(currentFileName != "")
                {
                    EventManager.SaveFileLoaded();
                }
            }

            private static class IO
            {
                //Read a file with the name "filename" and pass its data into saveData
                public static SaveData Read(string filename)
                {
                    SaveData newSaveData;

                    if (File.Exists(Config.SavesFileLocation + "/" + filename))
                    {
                        
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
                            
                        }

                        currentFileName = filename;
                        return newSaveData;
                    }
                    else
                    {
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.warning, "ERR_IO_NO_FILE_EXISTS: " + filename));
                        throw new Exception("ERR_NO_FILE_EXISTS");
                        currentFileName = "";
                        return newSaveData;
                    }
                }
            }

                //Write a new file with name "filename"
                public static void WriteNew(string filename, string playername)
                {
                    SaveData newSaveData = new SaveData();

                    if(filename != "" && filename != null)
                    {
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
                }

                //Writes the current save data to the file named "playerName"
                public static void Write()
                {
                    if (File.Exists(Config.SavesFileLocation + "/" + currentFileName))
                    {
                        if (currentFileName != null && currentFileName != "")
                        {
                            XmlSerializer xs = new XmlSerializer(typeof(SaveData));
                            TextWriter tw = new StreamWriter(Config.SavesFileLocation + "/" + currentFileName);

                            xs.Serialize(tw, currentSaveData);
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
                        NDebug.Log(new NDebug.Info(NDebug.DebugType.error, "ERR_NO_FILE_EXISTS: " + currentFileName));
                        throw new Exception("ERR_NO_FILE_EXISTS");
                    }
                }

                //Clears the currently loaded playerName and saveData
                public static void Clear()
                {
                    //Clears save data currently cached in saveData
                    currentFileName = "";

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

        public struct SaveData
        {
            public string playerName;
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