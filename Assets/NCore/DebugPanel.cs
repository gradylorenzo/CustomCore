using System.Collections.Generic;
using UnityEngine;
using NCore.Managers;
using UnityEditor;

public class DebugPanel : MonoBehaviour
{
    public bool showDebug = true;
    public Color[] DebugColors;
    public List<string> messages;
    public List<string> successes;
    public List<string> warnings;
    public List<string> errors;

    public enum Display
    {
        messages,
        successes,
        warnings,
        errors
    }
    public Display display;
    public Vector2 scrollPositionLog;

    public Dictionary<string, string> staticDisplayInformation = new Dictionary<string, string>();

    private void Awake()
    {
        NDebug.Log += DebugEvent;
        NDebug.UpdateSDI += UpdateSDI;

        NDebug.Log(NDebug.DebugEventType.message, Time.time, "Initialize messages");
        NDebug.Log(NDebug.DebugEventType.success, Time.time, "Initialize successes");
        NDebug.Log(NDebug.DebugEventType.warning, Time.time, "Initialize warnings");
        NDebug.Log(NDebug.DebugEventType.error, Time.time, "Initialize errors");
    }

    private void DebugEvent(NDebug.DebugEventType type, float time, string message)
    {
        string newString = "[" + time.ToString("0.00") + "] : " + message;
        switch (type)
        {
            case NDebug.DebugEventType.message:
                messages.Add(newString);
                break;
            case NDebug.DebugEventType.success:
                successes.Add(newString);
                break;
            case NDebug.DebugEventType.warning:
                warnings.Add(newString);
                break;
            case NDebug.DebugEventType.error:
                errors.Add(newString);
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
            GUILayout.BeginArea(new Rect(0, 0, 700, 500));
            GUILayout.BeginHorizontal(EditorStyles.helpBox);
            #region View Switches
            GUILayout.BeginVertical(GUILayout.Width(100));
            GUI.color = DebugColors[0];
            if (GUILayout.Button("Messages"))
            {
                display = Display.messages;
                scrollPositionLog.y = Mathf.Infinity;
            }

            GUI.color = DebugColors[1];
            if (GUILayout.Button("Successes"))
            {
                display = Display.successes;
                scrollPositionLog.y = Mathf.Infinity;
            }

            GUI.color = DebugColors[2];
            if (GUILayout.Button("Warnings"))
            {
                display = Display.warnings;
                scrollPositionLog.y = Mathf.Infinity;
            }

            GUI.color = DebugColors[3];
            if (GUILayout.Button("Errors"))
            {
                display = Display.errors;
                scrollPositionLog.y = Mathf.Infinity;
            }
            GUI.color = Color.white;
            GUILayout.EndVertical();
            #endregion

            #region Log View
            GUILayout.BeginVertical(GUILayout.Width(300));
            GUILayout.Space(-5);
            scrollPositionLog = GUILayout.BeginScrollView(scrollPositionLog);
            switch (display)
            {
                case Display.messages:
                    GUI.color = DebugColors[0];
                    foreach (string s in messages)
                    {
                        GUILayout.Label(s);
                    }
                    break;
                case Display.successes:
                    GUI.color = DebugColors[1];
                    foreach (string s in successes)
                    {
                        GUILayout.Label(s);
                    }
                    break;
                case Display.warnings:
                    GUI.color = DebugColors[2];
                    foreach (string s in warnings)
                    {
                        GUILayout.Label(s);
                    }
                    break;
                case Display.errors:
                    GUI.color = DebugColors[3];
                    foreach (string s in errors)
                    {
                        GUILayout.Label(s);
                    }
                    break;
            }
            GUI.color = Color.white;
            GUILayout.EndScrollView();
            GUILayout.EndVertical();
            #endregion

            #region Static Display
            GUILayout.BeginVertical(GUILayout.Width(300));
            foreach(KeyValuePair<string, string> kvp in staticDisplayInformation)
            {
                GUILayout.Label(kvp.Value);
            }
            GUILayout.EndVertical();
            #endregion
            GUILayout.EndHorizontal();
            GUILayout.EndArea();
        }
    }
}
