using UnityEngine;
using System.Collections;

namespace PLT
{
    [ExecuteInEditMode]
    public class PlotFadeComponent : MonoBehaviour
    {
        public static Texture2D      texture { get; set; }
        [HideInInspector]
        public        Color          color   { get; set; }

        void OnGUI()
        {
            if (!texture)
            {
                texture = new Texture2D(1, 1, TextureFormat.ARGB32, false);
            }
            texture.SetPixel(0, 0, new Color(color.r, color.g, color.b, color.a));
            texture.Apply();
            GUI.DrawTexture(new Rect(0, 0, Screen.width, Screen.height), texture, ScaleMode.StretchToFill);
        }
    }
}