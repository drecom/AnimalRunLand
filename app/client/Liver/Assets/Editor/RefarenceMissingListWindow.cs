using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace OtherScripts.Editor.RefarenceMissingList
{
public class RefarenceMissingListWindow : EditorWindow
{
    private static string[] extensions = {".prefab", ".mat", ".controller", ".cs", ".shader", ".mask", ".asset"};
    private static List<AssetParameterData> missingList = new List<AssetParameterData>();
    private Vector2 scrollPos;

    [MenuItem("Assets/Find Missing in Assets")]
    private static void ShowMissingList()
    {
        // Missingがあるアセットを検索
        Search();
        // ウィンドウを表示
        var window = GetWindow<RefarenceMissingListWindow>();
        window.minSize = new Vector2(900, 300);
    }

    [MenuItem("Assets/Find Missing in GameObject in Scene")]
    private static void ShowMissingListInScene()
    {
        SearchObjectInScene();
        // ウィンドウを表示
        var window = GetWindow<RefarenceMissingListWindow>();
        window.minSize = new Vector2(900, 300);
    }

    /// <summary>
    /// Missingがあるアセットを検索
    /// </summary>
    private static void Search()
    {
        // 全てのアセットのファイルパスを取得
        string[] allPaths = AssetDatabase.GetAllAssetPaths();
        int length = allPaths.Length;

        for (int i = 0; i < length; i++)
        {
            if (extensions.Contains(Path.GetExtension(allPaths [i])))
            {
                SearchMissing(allPaths [i]);
            }
        }
    }

    /// <summary>
    /// 指定アセットにMissingのプロパティがあれば、それをmissingListに追加する
    /// </summary>
    /// <param name="path">Path.</param>
    private static void SearchMissing(string path)
    {
        // 指定パスのアセットを全て取得
        IEnumerable<UnityEngine.Object> assets = AssetDatabase.LoadAllAssetsAtPath(path);

        // 各アセットについて、Missingのプロパティがあるかチェック
        foreach (UnityEngine.Object obj in assets)
        {
            if (obj == null)
            {
                continue;
            }

            if (obj.name == "Deprecated EditorExtensionImpl")
            {
                continue;
            }

            // SerializedObjectを通してアセットのプロパティを取得する
            SerializedObject sobj = new SerializedObject(obj);
            SerializedProperty property = sobj.GetIterator();

            while (property.Next(true))
            {
                // プロパティの種類がオブジェクト（アセット）への参照で、
                // その参照がnullなのにもかかわらず、参照先インスタンスIDが0でないものはMissing状態！
                if (property.propertyType == SerializedPropertyType.ObjectReference &&
                        property.objectReferenceValue == null &&
                        property.objectReferenceInstanceIDValue != 0)
                {
                    // Missing状態のプロパティリストに追加する
                    missingList.Add(new AssetParameterData()
                    {
                        obj = obj,
                        path = path,
                        property = property
                    });
                }
            }
        }
    }

    /// <summary>
    /// Missingのリストを表示
    /// </summary>
    private void OnGUI()
    {
        // 列見出し
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Asset", GUILayout.Width(200));
        EditorGUILayout.LabelField("Path");
        EditorGUILayout.EndHorizontal();
        // リスト表示
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        foreach (AssetParameterData data in missingList)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(data.obj, data.obj.GetType(), true, GUILayout.Width(200));
            //            EditorGUILayout.TextField(data.property.displayName, GUILayout.Width(200));
            EditorGUILayout.TextField(data.path);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.EndScrollView();
    }

    /// <summary>
    /// シーン内のゲームオブジェクトでコンポーネントのプロパティがMissingになっているものを表示
    /// </summary>
    private static void SearchObjectInScene()
    {
        Object[] objs = UnityEngine.Resources.FindObjectsOfTypeAll(typeof(GameObject));
        int length = objs.Length;
        int count = 0;

        // Typeで指定した型の全てのオブジェクトを配列で取得し,その要素数分繰り返す.
        for (int i = 0; i < length; i++)
        {
            string path = AssetDatabase.GetAssetOrScenePath(objs[i]);
            bool isScene = path.Contains(".unity");

            if (isScene)
            {
                checkObject(objs[i], path);
            }

            count++;
        }
    }

    private static void checkObject(UnityEngine.Object obj, string path)
    {
        if (obj == null)
        {
            return;
        }

        if (obj.name == "Deprecated EditorExtensionImpl")
        {
            return;
        }

        Debug.Log("LOG OF " + obj.name);
        string log = obj.name;

        if (obj is GameObject)
        {
            var go = obj as GameObject;
            List<SerializedObject> sObjs = new List<SerializedObject>();
            sObjs.AddRange(go.GetComponents<Component>()
                           .Where(x => x != null)
                           .Select(x => new SerializedObject(x)));

            foreach (SerializedObject so in sObjs)
            {
                log += "\ntarget: " + so.targetObject;
                SerializedProperty property = so.GetIterator();

                while (property.Next(true))
                {
                    log += "\n" + property.displayName + " (" + property.propertyType + ")";

                    // プロパティがオブジェクト参照　かつ
                    // 参照の値がnull　かつ
                    // 参照のインスタンスIDが0以外　であれば、「Missing」と判定
                    if (property.propertyType == SerializedPropertyType.ObjectReference &&
                            property.objectReferenceValue == null   &&
                            property.objectReferenceInstanceIDValue != 0)
                    {
                        Debug.Log("<color=red>MISSING!!</color>\nobj: " + obj.name
                                  + "\nproperty displayname: " + property.displayName + "\n"
                                  + "\nproperty name: " + property.name);
                        // Missing状態のプロパティリストに追加する
                        AssetParameterData data = new AssetParameterData()
                        {
                            obj = obj,
                            path = path,
                            property = property
                        };

                        // そのままだとPrefab Parent Objectがnullのものまで返して見づらいので、省く
                        if (data.property.displayName == "Prefab Parent Object")
                        {
                            continue;
                        }

                        missingList.Add(data);
                        break;
                    }
                }
            }
        }

        Debug.Log(log);
    }
    public class AssetParameterData
    {
        public UnityEngine.Object obj
        {
            get;    //!< アセットのObject自体
            set;
        }
        public string path
        {
            get;    //!< アセットのパス
            set;
        }
        public SerializedProperty property
        {
            get;    //!< プロパティ
            set;
        }
    }
}
}

