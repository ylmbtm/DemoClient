using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

namespace MAP
{
    [UnityEditor.InitializeOnLoad]
    public class MAPInitialize
    {
        static Texture2D mapTexture;

        static MAPInitialize()
        {
            EditorApplication.hierarchyWindowItemOnGUI += OnHierarchyWindowItemOnGUI;
        }

        static void OnHierarchyWindowItemOnGUI(int instanceID, Rect selectionRect)
        {
            GameObject selectedGameObject = EditorUtility.InstanceIDToObject(instanceID) as GameObject;
            if (selectedGameObject == null)
            {
                return;
            }
            MAPWorldMap mw = selectedGameObject.GetComponent<MAPWorldMap>();
            if (mw == null)
            {
                return;
            }
            if (mapTexture == null)
            {
                mapTexture = UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Editor Default Resources/Map/Map.png");
            }
            Rect rect = new Rect(selectionRect.width - 20, selectionRect.y - 8, 32, 32);
            GUI.DrawTexture(rect, mapTexture);
        }
    }
}