using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using System;
#if UNITY_EDITOR
using UnityEditor;
#endif // UNITY_EDITOR

[RequireComponent(typeof(RawImage))]
public class Monitor : MonoBehaviour
{
    [Serializable]
    public class CameraSetting
    {
        public Vector3 position = new Vector3(0, 1, -10);
        public Vector3 rotation = Vector3.zero;
        public bool orthographic = false;
        public float fieldOfView = 60;
        public float orthographicSize = 5;
    }

    public CameraSetting cameraSetting = new CameraSetting();
    private GameObject _MonitorRoot;
    public GameObject MonitorRoot
    {
        get
        {
            if (_MonitorRoot == null) { CreateMonitorObject(); }

            return _MonitorRoot;
        }
    }
    public Camera Camera { get; private set; }

    RenderTexture renderTexture;
    int key = -1;

    private void Update()
    {
        if (renderTexture != null) { return; }

        CreateMonitorObject();
        var transform = GetComponent<RectTransform>();
        Vector2 size = Vector2.Scale(transform.rect.size, transform.lossyScale);
        renderTexture = RenderTexture.GetTemporary((int)size.x, (int)size.y, 24, RenderTextureFormat.ARGB32);
        GetComponent<RawImage>().texture = renderTexture;
        Camera.targetTexture = renderTexture;
    }

    private void CreateMonitorObject()
    {
        if (key >= 0) { return; }   // 初期化完了の場合、弾く!!

        key = MonitorKeyGen.GenKey();
        _MonitorRoot = new GameObject(string.Format("Monitor:{0}", key));
        MonitorRoot.transform.position = MonitorKeyGen.PositionFromKey(key);
        Camera = new GameObject("Camera").AddComponent<Camera>();
        Camera.clearFlags = CameraClearFlags.Color;
        Camera.backgroundColor = Color.clear;
        Camera.transform.SetParent(MonitorRoot.transform, false);
        Camera.transform.localPosition = cameraSetting.position;
        Camera.transform.rotation = Quaternion.Euler(cameraSetting.rotation);
        Camera.fieldOfView = cameraSetting.fieldOfView;
        Camera.orthographicSize = cameraSetting.orthographicSize;
        Camera.orthographic = cameraSetting.orthographic;
    }

    private void OnDestroy()
    {
        Debug.Log("OnDestroy");
        MonitorKeyGen.ReleaseKey(key);

        if (_MonitorRoot != null) { GameObject.Destroy(_MonitorRoot); }

        // 明示的に破棄
        if (this.renderTexture) { RenderTexture.ReleaseTemporary(this.renderTexture); }
    }

    public void Clear()
    {
        foreach (Transform t in MonitorRoot.transform)
        {
            if (t == Camera.transform) { continue; }

            GameObject.Destroy(t.gameObject);
        }
    }
}

public class MonitorSetting
{
    public static Vector3 MonitorPosition = new Vector3(10000, 10000, 0);
    public static Vector3 MonitorOffset = new Vector3(1500, 0, 0);
}

static class MonitorKeyGen
{
    static List<int> keys = new List<int>();
    public static int GenKey()
    {
        keys.Sort();

        for (int i = 0; i < keys.Count; i++)
        {
            if (keys[i] == i) { continue; }

            keys.Add(i);
            return i;
        }

        keys.Add(keys.Count);
        return keys.Last();
    }
    public static void ReleaseKey(int key)
    {
        keys.Remove(key);
    }
    public static Vector3 PositionFromKey(int key)
    {
        return MonitorSetting.MonitorPosition + MonitorSetting.MonitorOffset * key;
    }
}



#if UNITY_EDITOR
// インスペクタ拡張
[CustomEditor(typeof(Monitor))]
public class MonitorEditor : Editor
{
    enum Projection
    {
        Perspective,
        Orthographic,
    }

    public override void OnInspectorGUI()
    {
        var monitor = target as Monitor;
        var s = monitor.cameraSetting;
        s.position = EditorGUILayout.Vector3Field("Position", s.position);
        s.rotation = EditorGUILayout.Vector3Field("Rotation", s.rotation);
        var projection = (Projection)EditorGUILayout.EnumPopup("Projection", s.orthographic ? Projection.Orthographic : Projection.Perspective);
        s.orthographic = projection == Projection.Orthographic;

        if (s.orthographic)
        {
            s.orthographicSize = EditorGUILayout.FloatField("Size", s.orthographicSize);
        }
        else
        {
            s.fieldOfView = EditorGUILayout.IntSlider("Field Of View", (int)s.fieldOfView, 1, 179);
        }
    }
}

#endif // UNITY_EDITOR

