using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public class DebugLogWindow : MonoBehaviour
{
    static DebugLogWindow instance = null;
    static bool dontDestroy = false;

    public static void Load()
    {
        // load prefab if not exists
        if (instance == null)
        {
            instance = GameObject.FindObjectOfType<DebugLogWindow>();

            if (instance == null)
            {
                var go = Resources.Load("UI/DebugLogWindow") as GameObject;

                if (go != null)
                {
                    var inst = GameObject.Instantiate<GameObject>(go);
                    inst.name = "DebugLogWindow";
                    instance = inst.GetComponent<DebugLogWindow>();
                }
            }

            if (!dontDestroy)
            {
                if (instance != null)
                {
                    DontDestroyOnLoad(instance.gameObject);
                    dontDestroy = true;
                }
            }
        }
    }
    public static DebugLogWindow Instance
    {
        get
        {
            Load();
            return instance;
        }
    }

    const int SCREEN_WIDTH = 720;
    const int SCREEN_HEIGHT = 1280;
    const int CLOSED_WINDOW_WIDTH = 200;
    const int CLOSED_WINDOW_HEIGHT = 128;
    const int OPENED_WINDOW_WIDTH = 360 + 180;
    const int OPENED_WINDOW_HEIGHT = 640 + 320;
    bool viewWindow = false;
    int fontSize = 26;
    Rect windowRect = new Rect(0, 0, CLOSED_WINDOW_WIDTH, CLOSED_WINDOW_HEIGHT);
    float windowWidth = CLOSED_WINDOW_WIDTH;
    float windowHeight = CLOSED_WINDOW_HEIGHT;
    bool debugOpened = false;
    struct Log
    {
        public string Condition;
        public string StackTrace;
        public LogType Type;
    }
    List<Log> debugLogs;
    const int MAX_LOG_COUNT = 1000;
    Vector2 scrollPos;
    bool showStackTrace;
    bool wordWrap;
    bool stopLogs;
    int logFilterSelection = 2;
    IEnumerator Start()
    {
        var dt = DateTime.Now;

        while (true)
        {
            if (Input.GetMouseButtonDown(0))
            {
                dt = DateTime.Now;
            }

            if (Input.GetMouseButton(0))
            {
                if (DateTime.Now - dt > TimeSpan.FromSeconds(5))
                {
                    viewWindow = !viewWindow;
                    dt = DateTime.Now + TimeSpan.FromDays(1);
                }
            }

            yield return null;
        }
    }
    void OnGUI()
    {
        if (!viewWindow) { return; }

        if (debugLogs == null)
        {
            debugLogs = new List<Log>();
            Application.logMessageReceived += (string condition, string stackTrace, LogType type) =>
            {
                if (stopLogs) { return; }

                if (debugLogs.Count > MAX_LOG_COUNT)
                {
                    debugLogs.RemoveAt(0);
                }

                debugLogs.Add(new Log()
                {
                    Condition = condition,
                    StackTrace = stackTrace,
                    Type = type
                });
            };
        }

        Vector2 resizeRatio = new Vector2((float)Screen.width / SCREEN_WIDTH, (float)Screen.height / SCREEN_HEIGHT);

        if (GUI.skin.font == null)
        {
            GUI.skin.font = Font.CreateDynamicFontFromOSFont(Font.GetOSInstalledFontNames(), fontSize);
        }
        else if (GUI.skin.font.fontSize != fontSize)
        {
            GUI.skin.font = Font.CreateDynamicFontFromOSFont(GUI.skin.font.name, fontSize);
        }

        GUI.matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, new Vector3(resizeRatio.x, resizeRatio.y, 1.0f));
        GUI.skin.verticalScrollbar.fixedWidth = 32;
        GUI.skin.verticalScrollbarThumb.fixedWidth = 32;
        GUI.skin.horizontalScrollbar.fixedHeight = 32;
        GUI.skin.horizontalScrollbarThumb.fixedHeight = 32;
        windowRect.width = windowWidth;
        windowRect.height = windowHeight;
        windowRect = GUI.Window(0, windowRect, _ =>
        {
            if (GUI.Button(new Rect(0, 32, 32, 32), "-"))
            {
                fontSize = (int)(fontSize * (1.0f / 1.1f));
            }

            if (GUI.Button(new Rect(32, 32, 32, 32), "+"))
            {
                fontSize = (int)(fontSize * 1.1f);
            }

            if (!debugOpened)
            {
                if (GUI.Button(new Rect(0, 64, 200, 64), new GUIContent("Open")))
                {
                    debugOpened = true;
                    windowWidth = OPENED_WINDOW_WIDTH;
                    windowHeight = OPENED_WINDOW_HEIGHT;
                }
            }
            else
            {
                if (GUI.Button(new Rect(windowWidth - 200, 64, 200, 64), new GUIContent("Close")))
                {
                    debugOpened = false;
                    windowWidth = CLOSED_WINDOW_WIDTH;
                    windowHeight = CLOSED_WINDOW_HEIGHT;
                }

                GUILayout.BeginVertical();
                GUILayout.Space(128);
                logFilterSelection = GUILayout.SelectionGrid(logFilterSelection, new [] { "Debug", "Warning", "Error" }, 3);
                GUILayout.BeginHorizontal();
                showStackTrace = GUILayout.Toggle(showStackTrace, "Detail");
                wordWrap = GUILayout.Toggle(wordWrap, "Word Wrap");
                stopLogs = GUILayout.Toggle(stopLogs, "Stop");
                GUILayout.EndHorizontal();
                scrollPos = GUILayout.BeginScrollView(scrollPos);

                foreach (var log in debugLogs)
                {
                    if (logFilterSelection == 1 && log.Type == LogType.Log)
                    {
                        continue;
                    }

                    if (logFilterSelection == 2 && (log.Type == LogType.Log || log.Type == LogType.Warning))
                    {
                        continue;
                    }

                    var style = new GUIStyle();
                    style.wordWrap = wordWrap;

                    //style.richText = false;
                    if (log.Type == LogType.Error || log.Type == LogType.Exception)
                    {
                        style.normal.textColor = Color.red;
                    }
                    else if (log.Type == LogType.Warning)
                    {
                        style.normal.textColor = Color.yellow;
                    }
                    else
                    {
                        style.normal.textColor = Color.white;
                    }

                    GUILayout.Label(log.Condition, style);

                    if (showStackTrace)
                    {
                        var lines = log.StackTrace.Split('\n');

                        foreach (var line in lines)
                        {
                            GUILayout.Label(">>> " + line, style);
                        }
                    }
                }

                GUILayout.EndScrollView();
                GUILayout.EndVertical();
            }

            GUI.DragWindow();
        }, new GUIContent("Debug Window"));
    }
}
