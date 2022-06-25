using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

#if UNITY_EDITOR
public class EEditorHelper
{
    public static Color     LIGHT_ORANGE = new Color(1, 0.9f, 0.4f);
    public static Color     LIGHT_BLUE   = new Color(0.8f, 0.8f, 1);
    public static Color     LIGHT_RED    = new Color(1, 0.5f, 0.5f, 0.8f);
    public static Texture2D TEX          = new Texture2D(1, 1);

    public static void DrawObjectInspector(object obj)
    {
        FieldInfo[] fields = obj.GetType().GetFields();
        for (int i = 0; i < fields.Length; i++)
        {
            FieldInfo field = fields[i];
            Type t = field.FieldType;
            if (t.IsPublic == false)
            {
                continue;
            }
            object v = field.GetValue(obj);
            if (t.BaseType == typeof(Enum))
            {
                v = UnityEditor.EditorGUILayout.EnumPopup(field.Name, (Enum)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(int))
            {
                v = UnityEditor.EditorGUILayout.IntField(field.Name, (int)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(bool))
            {
                v = UnityEditor.EditorGUILayout.Toggle(field.Name, (bool)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(float))
            {
                v = UnityEditor.EditorGUILayout.FloatField(field.Name, (float)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(double))
            {
                v = UnityEditor.EditorGUILayout.DoubleField(field.Name, (double)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(string))
            {
                v = UnityEditor.EditorGUILayout.TextField(field.Name, (string)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(Vector3))
            {
                v = UnityEditor.EditorGUILayout.Vector3Field(field.Name, (Vector3)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(Vector2))
            {
                v = UnityEditor.EditorGUILayout.Vector2Field(field.Name, (Vector2)v);
                field.SetValue(obj, v);
            }
            else if (t == typeof(UnityEngine.Object))
            {
                v = UnityEditor.EditorGUILayout.ObjectField(field.Name, obj as UnityEngine.Object, t.GetType(), true);
                field.SetValue(obj, v);
            }
        }
    }

    public static void DrawCoolLabel(string text)
    {
        GUI.skin.label.richText = true;
        GUI.color = LIGHT_ORANGE;
        GUILayout.Label("<b><size=16>" + text + "</size></b>");
        GUI.color = Color.white;
    }

    public static void DrawCoolTitle(string text)
    {
        GUILayout.Space(5);
        GUI.skin.label.richText = true;
        GUI.color = LIGHT_ORANGE;
        GUILayout.Label("<b><size=16>" + text + "</size></b>");
        GUI.color = Color.white;
        GUILayout.Space(2);
    }

    public static void DrawSeparator()
    {
        GUI.backgroundColor = Color.black;
        GUILayout.Box("", GUILayout.MaxWidth(Screen.width), GUILayout.Height(2));
        GUI.backgroundColor = Color.white;
    }

    public static void DrawBoldSeparator()
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(14);
        GUI.color = new Color(0, 0, 0, 0.25f);
        GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 4), TEX);
        GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 1), TEX);
        GUI.DrawTexture(new Rect(0, lastRect.yMax + 9, Screen.width, 1), TEX);
        GUI.color = Color.white;
    }

    public static void DrawEndOfInspector()
    {
        Rect lastRect = GUILayoutUtility.GetLastRect();
        GUILayout.Space(8);
        GUI.color = new Color(0, 0, 0, 0.4f);
        GUI.DrawTexture(new Rect(0, lastRect.yMax + 6, Screen.width, 4), TEX);
        GUI.DrawTexture(new Rect(0, lastRect.yMax + 4, Screen.width, 1), TEX);
        GUI.color = Color.white;
    }

    public static void DrawTitledSeparator(string title, bool startOfInspector)
    {
        if (!startOfInspector)
            DrawBoldSeparator();
        else
            UnityEditor.EditorGUILayout.Space();
        DrawCoolLabel(title + " ▼");
        DrawSeparator();
    }

    public static void ShowMenu(Type type, UnityEditor.GenericMenu.MenuFunction2 menuFunction2)
    {
        UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(item => item.GetTypes())
               .Where(item => item.IsSubclassOf(type)).ToList();
        for (int i = 0; i < types.Count; i++)
        {
            Type t = types[i];
            menu.AddItem(new GUIContent(t.Name), false, menuFunction2, t);
        }
        menu.ShowAsContext();
    }

    public static void ShowMenu(Type type, Func<Type, string> selectFunction, UnityEditor.GenericMenu.MenuFunction2 menuFunction2)
    {
        UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
        var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(item => item.GetTypes())
               .Where(item => item.IsSubclassOf(type)).ToList();
        for (int i = 0; i < types.Count; i++)
        {
            Type t = types[i];
            string content = selectFunction(t);
            if (string.IsNullOrEmpty(content) == false)
            {
                menu.AddItem(new GUIContent(content), false, menuFunction2, t);
            }
        }
        menu.ShowAsContext();
    }

    public static bool IsSubClassOf(Type type, Type baseType)
    {
        var b = type.BaseType;
        while (b != null)
        {
            if (b.Equals(baseType))
            {
                return true;
            }
            b = b.BaseType;
        }
        return false;
    }

    public static TAtr GetAttribute<TAtr>(Type type) where TAtr : Attribute
    {
        TAtr[] attrs = (TAtr[])type.GetCustomAttributes(typeof(TAtr), false);
        if (attrs != null && attrs.Length > 0)
        {
            TAtr attr = attrs[0];
            return attr;
        }
        return null;
    }

    public static T CreateAsset<T>(bool displayFilePanel) where T : ScriptableObject
    {
        return (T)CreateAsset(typeof(T), displayFilePanel);
    }

    public static T CreatePrefab<T>(bool displayFilePanel) where T : MonoBehaviour
    {
        T asset = null;
        Type type = typeof(T);
        var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                    "Create Asset of type " + type.ToString(),
                       type.Name, "prefab", "");
        asset = new GameObject(type.Name).AddComponent<T>();
        UnityEngine.Object prefab = UnityEditor.PrefabUtility.CreateEmptyPrefab(path);
        UnityEditor.PrefabUtility.ReplacePrefab(asset.gameObject, prefab, UnityEditor.ReplacePrefabOptions.ConnectToPrefab);
        UnityEditor.AssetDatabase.SaveAssets();
        return asset;
    }

    public static ScriptableObject CreateAsset(System.Type type, bool displayFilePanel)
    {
        ScriptableObject asset = null;
        var path = UnityEditor.EditorUtility.SaveFilePanelInProject(
                    "Create Asset of type " + type.ToString(),
                       type.Name + ".asset",
                    "asset", "");
        asset = CreateAsset(type, path);
        return asset;
    }

    public static ScriptableObject CreateAsset(System.Type type)
    {
        var asset = CreateAsset(type, GetAssetUniquePath(type.Name + ".asset"));
        return asset;
    }

    public static ScriptableObject CreateAsset(System.Type type, string path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }
        ScriptableObject data = null;
        data = ScriptableObject.CreateInstance(type);
        UnityEditor.AssetDatabase.CreateAsset(data, path);
        UnityEditor.AssetDatabase.SaveAssets();
        return data;
    }

    public static string           GetAssetUniquePath(string fileName)
    {
        var path = UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject);
        if (path == "")
        {
            path = "Assets";
        }
        if (System.IO.Path.GetExtension(path) != "")
            path = path.Replace(System.IO.Path.GetFileName(UnityEditor.AssetDatabase.GetAssetPath(UnityEditor.Selection.activeObject)), "");
        return UnityEditor.AssetDatabase.GenerateUniqueAssetPath(path + "/" + fileName);
    }

    public static ScriptableObject AddScriptableComponent(GameObject target, System.Type type)
    {
        return null;
    }
}
#endif