using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using NCore;

public class NDebugPanel : MonoBehaviour
{
    #region Fields
    public bool enableAutoDebug = true;
    public Texture2D background;
    public int panelWidth = 380;
    public List<NDebug.Info> allMessages = new List<NDebug.Info>();
    public Dictionary<string, string> staticDisplayInformation = new Dictionary<string, string>();
    private string systemInfo;
    private string screenSize()
    {
        return Screen.width + " x " + Screen.height + ", " + Screen.fullScreenMode.ToString();
    }

    private bool showDebug = true;
    private bool switchOnWarning = false;
    private bool switchOnError = false;
    private bool switchOnCritical = true;
    private bool displayAll = true;

    private NDebug.DebugType display;
    private Vector2 logScrollPosition;
    private Vector2 sdiScrollPosition;

    private Color[] DebugColors = new Color[]
    {
        new Color(0.0f, 1.0f, 1.0f, 1.0f),
        new Color(1.0f, 1.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 0.0f, 1.0f),
        new Color(1.0f, 0.0f, 1.0f, 1.0f)
    };
    private GUIStyle[] style = new GUIStyle[] { new GUIStyle(), new GUIStyle() };

    private bool canToggleDebug = true;
    
    private float fpsRefreshRate = 0.25f;
    private float fpsLastRefresh = -1.0f;
    #endregion

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        NDebug.Log += DebugEvent;
        NDebug.UpdateSDI += UpdateSDI;
        Application.logMessageReceived += DebugEvent;

        GetSystemSpecs();
        SetStyle();
    }

    private void Start()
    {
        NDebug.Log(new NDebug.Info(NDebug.DebugType.message, "NDebug is active!", this));
    }

    private void Update()
    {
        ClampPanelWidth();
        ToggleDebug();
        MeasureFPS();
    }

    private void GetSystemSpecs()
    {
        string cpu()
        {
            string edited = SystemInfo.processorType;
            edited = edited.Replace("Intel", "");
            edited = edited.Replace("AMD", "");
            edited = edited.Replace("(TM)", "");
            edited = edited.Replace("(R)", "");
            edited = edited.Replace("@ ", "");
            edited = edited.Replace("CPU ", "");
            edited = edited + "\n";
            return "CPU: " + edited;
        }

        string ram (int r)
        {
            return (r / 1000f).ToString("0.# GB");
        }

        string gpu()
        {
            string edited = SystemInfo.graphicsDeviceName;
            return "GPU: " + edited + "\n";
        }

        string os = "OS: " + SystemInfo.operatingSystem + "\n";
        string sram = "RAM: " + ram(SystemInfo.systemMemorySize) + "\n";
        string gram = "GPU RAM: " + ram(SystemInfo.graphicsMemorySize) + "\n";
        string gapi = "API: " + SystemInfo.graphicsDeviceType.ToString() + "     ";
        string shader = "SL: " +SystemInfo.graphicsShaderLevel.ToString() + "\n";
        string maxtex = "MAXT: " + SystemInfo.maxTextureSize.ToString() + "     ";
        string suppshadows = "SHAD: " + SystemInfo.supportsShadows.ToString() + "\n";
        string suppaccel = "ACCL: " + SystemInfo.supportsAccelerometer.ToString() + "     ";
        string suppgyro = "GYRO: " + SystemInfo.supportsGyroscope.ToString() + "\n";
        string res = "RES: ";

        systemInfo = os + cpu() + sram + gpu() + gram + gapi + shader + maxtex + suppshadows + suppaccel + suppgyro + res;
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

    private void ClampPanelWidth()
    {
        panelWidth = Mathf.Clamp(panelWidth, 380, Screen.width);
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

    private void MeasureFPS()
    {
        if(Time.time > fpsLastRefresh + fpsRefreshRate)
        {
            string fps = "FPS: " + (1f / Time.unscaledDeltaTime).ToString("0.0");
            NDebug.UpdateSDI("fps", fps);
            fpsLastRefresh = Time.time;
        }
    }

    private void DebugEvent(NDebug.Info info)
    {
        allMessages.Add(info);
        if (enableAutoDebug)
        {
            if(switchOnWarning && info.type == NDebug.DebugType.warning)
            {
                display = NDebug.DebugType.warning;
            }

            if(switchOnError && info.type == NDebug.DebugType.error)
            {
                display = NDebug.DebugType.error;
            }

            if(switchOnCritical && info.type == NDebug.DebugType.critical)
            {
                display = NDebug.DebugType.critical;
            }

            displayAll = false;
            showDebug = true;
        }
        logScrollPosition.y = Mathf.Infinity;
    }

    private void DebugEvent(string logString, string stackTrace, LogType type)
    {
        NDebug.Info newInfo = new NDebug.Info();
        newInfo.type = NDebug.DebugType.critical;
        newInfo.timestamp = Time.time;
        newInfo.description = logString + "  |  " + stackTrace;
        newInfo.source = null;

        allMessages.Add(newInfo);

        if (enableAutoDebug)
        {
            showDebug = true;
        }

        logScrollPosition.y = Mathf.Infinity;
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
            GUILayout.BeginArea(new Rect(0, 0, panelWidth, Screen.height), style[1]);
            GUILayout.BeginVertical();
            #region Static Display

            GUILayout.BeginHorizontal();
            GUILayout.Label(systemInfo + screenSize(), GUILayout.Width(210));

            GUILayout.BeginVertical();
            foreach(KeyValuePair<string, string> kvp in staticDisplayInformation)
            {
                GUILayout.Label(kvp.Value);
                GUILayout.Space(-10);
            }
            GUILayout.EndVertical();

            GUILayout.EndHorizontal();
            #endregion

            #region Log View
            logScrollPosition = GUILayout.BeginScrollView(logScrollPosition);
            GUILayout.ExpandHeight(true);
            GUILayout.ExpandWidth(true);
            if (displayAll)
            {
                foreach (NDebug.Info i in allMessages)
                {
                    GUI.color = DebugColors[(int)i.type];
                    if (i.source != null)
                    {
                        if (GUILayout.Button(i.timestamp.ToString("[0.00] <") + i.source + "> " + i.description, style[0]))
                        {
                            PingObject(i.source.gameObject);
                        }
                    }
                    else
                    {
                        GUILayout.Label(i.timestamp.ToString("[0.00] ") + i.description, style[0]);
                    }
                    GUILayout.Space(5);
                    GUI.color = Color.white;
                }
            }
            else
            {
                GUI.color = DebugColors[(int)display];
                foreach (NDebug.Info i in allMessages)
                {
                    if (i.type == display)
                    {
                        if (i.source != null)
                        {
                            if (GUILayout.Button(i.timestamp.ToString("[0.00] <") + i.source + "> " + i.description, style[0]))
                            {
                                PingObject(i.source.gameObject);
                            }
                        }
                        else
                        {
                            GUILayout.Label(i.timestamp.ToString("[0.00] ") + i.description, style[0]);
                        }
                        GUILayout.Space(5);
                    }
                }
                GUI.color = Color.white;
            }
            GUILayout.EndScrollView();
            #endregion

            #region Autoswitch Toggles
            GUILayout.BeginHorizontal(GUILayout.Width(300));
            switchOnWarning = GUILayout.Toggle(switchOnWarning, "Warning Switch");
            switchOnError = GUILayout.Toggle(switchOnError, "Error Switch");
            switchOnCritical = GUILayout.Toggle(switchOnCritical, "Critical Switch");
            GUILayout.EndHorizontal();
            #endregion

            #region View switches
            GUILayout.BeginHorizontal(GUILayout.Width(panelWidth - 10));
            if (GUILayout.Button("All"))
            {
                displayAll = true;
                logScrollPosition.y = Mathf.Infinity;
            }
            GUI.color = DebugColors[0];
            if (GUILayout.Button("Message"))
            {
                displayAll = false;
                display = NDebug.DebugType.message;
                logScrollPosition.y = Mathf.Infinity;
            }
            GUI.color = DebugColors[1];
            if (GUILayout.Button("Warning"))
            {
                displayAll = false;
                display = NDebug.DebugType.warning;
                logScrollPosition.y = Mathf.Infinity;
            }
            GUI.color = DebugColors[2];
            if (GUILayout.Button("Error"))
            {
                displayAll = false;
                display = NDebug.DebugType.error;
                logScrollPosition.y = Mathf.Infinity;
            }
            GUI.color = DebugColors[3];
            if (GUILayout.Button("Critical"))
            {
                displayAll = false;
                display = NDebug.DebugType.critical;
                logScrollPosition.y = Mathf.Infinity;
            }
            GUI.color = Color.white;
            GUILayout.EndHorizontal();
            #endregion

            GUILayout.EndVertical();
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
        public DebugType type;
        public float timestamp;
        public string description;
        public MonoBehaviour source;
        
        public Info(DebugType t, string d, MonoBehaviour s)
        {
            type = t;
            timestamp = Time.time;
            if (t == DebugType.critical)
            {
                description = d + " : please send a screenshot to : " + Config.DeveloperID;
            }
            else
            {
                description = d;
            }
            source = s;
        }

        public Info(DebugType t, string d)
        {
            type = t;
            timestamp = Time.time;
            if (t == DebugType.critical)
            {
                description = d + " : please send a screenshot to : " + Config.DeveloperID;
            }
            else
            {
                description = d;
            }
            source = null;
        }

        public Info(string d)
        {
            type = DebugType.message;
            timestamp = Time.time;
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

    public delegate void GenericDebugEvent(Info info);
    public static GenericDebugEvent Log;

    public delegate void StaticDisplayInformationEvent(string name, string info);
    public static StaticDisplayInformationEvent UpdateSDI;
}
