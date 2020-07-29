using System.Collections.Generic;
using UnityEngine;
using NCore.Managers;
using UnityEditor;
using System;
using NCore;

public class NDebugPanel: MonoBehaviour
{
    public bool enableAutoDebug = true;
    public Texture2D background;
    public List<NDebug.Info> messages = new List<NDebug.Info>();
    public List<NDebug.Info> successes = new List<NDebug.Info>();
    public List<NDebug.Info> warnings = new List<NDebug.Info>();
    public List<NDebug.Info> errors = new List<NDebug.Info>();
    public List<NDebug.Info> critical = new List<NDebug.Info>();
    public Dictionary<string, string> staticDisplayInformation = new Dictionary<string, string>();

    private Color[] DebugColors = new Color[]
    {
        new Color(0.0f, 1.0f, 1.0f, 1.0f),
        new Color(1.0f, 1.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 1.0f, 1.0f)
    };
    private GUIStyle[] style = new GUIStyle[] { new GUIStyle(), new GUIStyle() };
    private NDebug.DebugType display;
    private Vector2 logScrollPosition;
    private Vector2 sdiScrollPosition;
    private bool showDebug = true;
    private bool switchOnWarning = false;
    private bool switchOnError = false;
    private bool switchOnCritical = false;

    private bool canToggleDebug = true;
    private string screenSize;
    private float fpsRefreshRate = 0.25f;
    private float fpsLastRefresh = -1.0f;
    

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        NDebug.Log += DebugEvent;
        NDebug.UpdateSDI += UpdateSDI;

        GetSystemSpecs();
        SetStyle();
    }

    private void Start()
    {
        NDebug.Log(NDebug.DebugType.message, new NDebug.Info(Time.time, "NDebug is active!", this));
    }

    private void Update()
    {
        ToggleDebug();
        UpdateScreenSize();
        MeasureFPS();
    }

    private void GetSystemSpecs()
    {
        NDebug.UpdateSDI("system.os", "OS: " + SystemInfo.operatingSystem);
        NDebug.UpdateSDI("system.cpu", "CPU: " + SystemInfo.processorType);
        NDebug.UpdateSDI("system.sram", "System RAM: " + SystemInfo.systemMemorySize);
        NDebug.UpdateSDI("system.gpu", "GPU: " + SystemInfo.graphicsDeviceName);
        NDebug.UpdateSDI("system.gram", "GPU RAM: " + SystemInfo.graphicsMemorySize);
        NDebug.UpdateSDI("system.gapi", "Graphics API: " + SystemInfo.graphicsDeviceType);
        NDebug.UpdateSDI("system.glevel", "Shader Level: " + SystemInfo.graphicsShaderLevel);
        NDebug.UpdateSDI("system.maxtexsize", "Max Texture Size: " + SystemInfo.maxTextureSize);
        NDebug.UpdateSDI("system.shadows", "Shadows: " + SystemInfo.supportsShadows);
        NDebug.UpdateSDI("system.accel", "Accelerometer: " + SystemInfo.supportsAccelerometer);
        NDebug.UpdateSDI("system.gyro", "Gyroscope: " + SystemInfo.supportsGyroscope);
        NDebug.UpdateSDI("space", " ");
    }

    private void SetStyle()
    {
        style[0].normal.textColor = new Color(0.8f, 0.8f, 0.8f, 1.0f);
        style[0].hover.textColor = Color.white;
        style[0].active.textColor = Color.white;
        style[0].wordWrap = true;
        style[0].fontSize = 14;

        style[1].normal.background = background;
        style[1].contentOffset = new Vector2(0, 5);
    }

    private void ToggleDebug()
    {
        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.F12))
        {
            if (canToggleDebug)
            {
                showDebug = !showDebug;
                canToggleDebug = false;
            }
        }
        else
        {
            canToggleDebug = true;
        }
    }

    private void UpdateScreenSize()
    {
        if((Screen.width + " x " + Screen.height) != screenSize)
        {
            screenSize = Screen.width + " x " + Screen.height;
            NDebug.UpdateSDI("system.res", "Resolution: " + screenSize + ", " + Screen.fullScreenMode.ToString());
        }
    }

    private void MeasureFPS()
    {
        if(Time.time > fpsLastRefresh + fpsRefreshRate)
        {
            string fps = "FPS: " + (1f / Time.unscaledDeltaTime).ToString("0.0");
            NDebug.UpdateSDI("fps", fps);
            fpsLastRefresh = Time.time;
        }

        NDebug.UpdateSDI("time", "Time.time: " + Time.time.ToString("0.##"));
    }

    private void DebugEvent(NDebug.DebugType type, NDebug.Info info)
    {
        switch (type)
        {
            case NDebug.DebugType.message:
                messages.Add(info);
                break;
            case NDebug.DebugType.warning:
                warnings.Add(info);
                if (switchOnWarning && enableAutoDebug)
                {
                    display = NDebug.DebugType.warning;
                    logScrollPosition.y = Mathf.Infinity;
                    showDebug = true;
                }
                break;
            case NDebug.DebugType.error:
                errors.Add(info);
                if (switchOnError && enableAutoDebug)
                {
                    display = NDebug.DebugType.error;
                    logScrollPosition.y = Mathf.Infinity;
                    showDebug = true;
                }
                break;
            case NDebug.DebugType.critical:
                info.description += " : please send a screenshot of this to the developer : " + Config.DeveloperID;
                critical.Add(info);
                if (switchOnCritical && enableAutoDebug)
                {
                    display = NDebug.DebugType.critical;
                    logScrollPosition.y = Mathf.Infinity;
                    showDebug = true;
                }
                break;
        }
    }

    private void UpdateSDI(string name, string info)
    {
        if (staticDisplayInformation.ContainsKey(name))
        {
            staticDisplayInformation[name] = info;
        }
        else
        {
            staticDisplayInformation.Add(name, info);
        }
    }

    private void OnGUI()
    {
        if (showDebug)
        {
            GUILayout.BeginArea(new Rect(0, 0, 650, Screen.height), style[1]);
                GUILayout.BeginHorizontal();
                    #region View Switches
                    GUILayout.BeginVertical();

                        GUILayout.BeginHorizontal();
                            GUI.color = DebugColors[0];
                            if (GUILayout.Button("Message", GUILayout.Width(75)))
                            {
                                display = NDebug.DebugType.message;
                                logScrollPosition.y = Mathf.Infinity;
                            }

                            GUI.color = DebugColors[1];
                            if (GUILayout.Button("Warning", GUILayout.Width(75)))
                            {
                                display = NDebug.DebugType.warning;
                                logScrollPosition.y = Mathf.Infinity;
                            }

                            GUI.color = DebugColors[2];
                            if (GUILayout.Button("Error", GUILayout.Width(75)))
                            {
                                display = NDebug.DebugType.error;
                                logScrollPosition.y = Mathf.Infinity;
                            }

                            GUI.color = DebugColors[3];
                            if (GUILayout.Button("Critical", GUILayout.Width(75)))
                            {
                                display = NDebug.DebugType.critical;
                                logScrollPosition.y = Mathf.Infinity;
                            }
                            GUI.color = Color.white;
                        GUILayout.EndHorizontal();

                        GUILayout.BeginHorizontal();
                            switchOnWarning = GUILayout.Toggle(switchOnWarning, "Warning Switch");
                            switchOnError = GUILayout.Toggle(switchOnError, "Error Switch");
                            switchOnCritical = GUILayout.Toggle(switchOnCritical, "Critical Switch");
                        GUILayout.EndHorizontal();
                    #endregion

                    #region Log View
                            GUILayout.BeginVertical(GUILayout.Width(300));
                                logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
                                switch (display)
                                {
                                    case NDebug.DebugType.message:
                                        GUI.color = DebugColors[0];
                                        foreach (NDebug.Info i in messages)
                                        {
                                            if(GUILayout.Button(i.timestamp.ToString("[0.00] ") + i.source + " : " +  i.description, style[0]))
                                            {
                                                PingObject(i.source.gameObject);
                                            }
                                        }
                                        break;
                                    case NDebug.DebugType.warning:
                                        GUI.color = DebugColors[2];
                                        foreach (NDebug.Info i in warnings)
                                        {
                                            if (GUILayout.Button(i.timestamp.ToString("[0.00] ") + i.source + " : " + i.description))
                                            {
                                                PingObject(i.source.gameObject);
                                            }
                                        }
                                        break;
                                    case NDebug.DebugType.error:
                                        GUI.color = DebugColors[3];
                                        foreach (NDebug.Info i in errors)
                                        {
                                            if (GUILayout.Button(i.timestamp.ToString("[0.00] ") + i.source + " : " + i.description))
                                            {
                                                PingObject(i.source.gameObject);
                                            }
                                        }
                                        break;
                                    case NDebug.DebugType.critical:
                                        GUI.color = DebugColors[3];
                                        foreach (NDebug.Info i in critical)
                                        {
                                            if (GUILayout.Button(i.timestamp.ToString("[0.00] ") + i.source + " : " + i.description))
                                            {
                                                PingObject(i.source.gameObject);
                                            }
                                        }
                                        break;
                                }
                                GUI.color = Color.white;
                                GUILayout.EndScrollView();
                            GUILayout.EndVertical();
                        GUILayout.EndVertical();
                    #endregion

                    #region Static Display
                    GUILayout.BeginVertical(GUILayout.Width(500));
                        sdiScrollPosition = GUILayout.BeginScrollView(sdiScrollPosition);
                        foreach (KeyValuePair<string, string> kvp in staticDisplayInformation)
                        {
                            GUILayout.Label(kvp.Value, style[0]);
                        }
                        GUILayout.EndScrollView();
                    GUILayout.EndVertical();
                #endregion
                GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }

    private void PingObject(GameObject go)
    {
        #if UNITY_EDITOR
        EditorGUIUtility.PingObject(go);
        #endif
    }
}

public static class NDebug
{
    public struct Info
    {
        public float timestamp;
        public string description;
        public MonoBehaviour source;

        public Info(float t, string d, MonoBehaviour s)
        {
            timestamp = t;
            description = d;
            source = s;
        }

        public Info(float t, string d)
        {
            timestamp = t;
            description = d;
            source = null;
        }
    }

    public enum DebugType
    {
        message = 0,
        warning = 1,
        error = 2,
        critical = 3
    }

    public delegate void GenericDebugEvent(DebugType type, Info info);
    public static GenericDebugEvent Log;

    public delegate void StaticDisplayInformationEvent(string name, string info);
    public static StaticDisplayInformationEvent UpdateSDI;
}
