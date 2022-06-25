using UnityEngine;
using System.Collections;

namespace PLT
{
    [ExecuteInEditMode]
    public class PlotTextComponent : MonoBehaviour
    {
        [HideInInspector]
        public        string   content;
        [HideInInspector]
        public static GUIStyle contentStyle;
        [HideInInspector]
        public Texture2D       texture;
        [HideInInspector][SerializeField]
        public bool            isEnableBlack;

        void OnGUI()
        {
            if (isEnableBlack)
            {
                if (texture == null)
                {
                    texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
                }
                texture.SetPixel(0, 0, Color.black);
                texture.Apply();
                float height1 = Screen.height * 0.15f;
                Rect rect1 = new Rect(0, 0, Screen.width, height1);
                Rect rect2 = new Rect(0, Screen.height - height1, Screen.width, height1);
                GUI.DrawTexture(rect1, texture, ScaleMode.StretchToFill);
                GUI.DrawTexture(rect2, texture, ScaleMode.StretchToFill);
            }

            if (string.IsNullOrEmpty(content) == false)
            {
                if (contentStyle == null)
                {
                    contentStyle = new GUIStyle(GUI.skin.label);
                    contentStyle.fixedHeight = 30;
                    contentStyle.fontSize = 24;
                    contentStyle.alignment = TextAnchor.MiddleCenter;
                }
                float height2 = Screen.height * 0.1f;
                Rect rect3 = new Rect(0, Screen.height - height2, Screen.width, height2);
                GUI.Label(rect3, content, contentStyle);
            }
        }
    }
}