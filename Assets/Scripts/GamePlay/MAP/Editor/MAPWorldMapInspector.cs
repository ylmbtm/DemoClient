using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using CFG;

namespace MAP
{
    [CustomEditor(typeof(MAPWorldMap), true)]
    public class MAPWorldMapInspector : Editor
    {
        public override void OnInspectorGUI()
        {
            GUILayout.Space(10);
            MAPWorldMap worldMap = target as MAPWorldMap;
            base.OnInspectorGUI();
            GUILayout.Space(10);
            GUI.color = Color.green;
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("读取", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                worldMap.Import();
            }

            GUI.color = Color.green;
            if (GUILayout.Button("保存", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                worldMap.Export();
            }
            GUILayout.EndHorizontal();
            GUILayout.Space(10);
        }
    }
}