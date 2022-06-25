using UnityEngine;
using System.Collections;
using UnityEditor;

namespace PLT
{
    [CustomPropertyDrawer(typeof(PlayableParent))]
    public class PlayableParentDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return base.GetPropertyHeight(property, label) - 16;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {

        }
    }
}