using UnityEngine;
using UnityEditor;

namespace EDT
{
    [CustomEditor(typeof(GTLauncher), false)]
    public class EditorLauncher : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
        }
    }
}