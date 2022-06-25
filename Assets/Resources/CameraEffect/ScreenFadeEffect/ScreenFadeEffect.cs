using UnityEngine;
using System.Collections;

[ExecuteInEditMode]
public class ScreenFadeEffect : MonoBehaviour
{
    public Texture2D  mFadeTexture2D;
    [Range(0,1)]
    public float alpha = 1;
    public Rect  rect = new Rect(0, 0, 2000, 2000);

    void Start()
    {
        mFadeTexture2D = new Texture2D(2, 2, TextureFormat.ARGB32, false);
    }

    void OnGUI()
    {
        if (mFadeTexture2D == null)
        {
            return;
        }
        Color color = Color.black;
        color.a = alpha;
        Graphics.DrawTexture(rect, mFadeTexture2D,rect,0,0,0,0,color);
    }
}
