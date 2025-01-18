using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace HUD
{
public class HpData
{
    protected float m_LastHpValue = -1;
    protected Vector3 m_LastPosition = Vector3.zero;
    public Actor m_Actor = null;
    public WidgetProxy m_WidgetBack = null;
    public WidgetProxy m_WidgetFront = null;


    public void InitSprites (UIAtlas atlas, string back, string front)
    {
        int hpWidth = HUDBlood.sHpWidth;
        if (m_Actor.Height >= 1.8f || m_Actor.Radius >= 0.9f)
        {
            hpWidth = (int)(hpWidth * 1.5f);
        }

        m_WidgetBack = WidgetProxy.Create<UISpriteProxy> ();
        UISprite sprite = m_WidgetBack.widget<UISprite> ();
        sprite.pivot = UIWidget.Pivot.Center;
        sprite.atlas = atlas;
        sprite.spriteName = back;
        sprite.type = UIBasicSprite.Type.Sliced;
        sprite.SetDimensions (hpWidth, HUDBlood.sHpHeight);

        m_WidgetFront = WidgetProxy.Create<UISpriteProxy> ();
        sprite = m_WidgetFront.widget<UISprite> ();
        sprite.pivot = UIWidget.Pivot.Center;
        sprite.atlas = atlas;
        sprite.spriteName = front;
        sprite.type = UIBasicSprite.Type.Sliced;
        sprite.SetDimensions (hpWidth, HUDBlood.sHpHeight);
    }

    public virtual void UpdatePercent ()
    {
        if (m_Actor.GetCurrentHP () == m_LastHpValue)
        {
            return;
        }

        m_LastHpValue = m_Actor.GetCurrentHP ();
        float per = m_LastHpValue / (float)m_Actor.GetMaxHP ();
        if (per < 0.0f)
        {
            per = 0;
        }
        else if (per > 1)
        {
            per = 1;
        }

        m_WidgetFront.widget<UISprite> ().SetDimensions ((int)((m_WidgetBack.widget<UISprite> ().width - 18) * per + 18), HUDBlood.sHpHeight);

        Matrix4x4 matrix = m_WidgetBack.matrix;
        Matrix4x4 matrix1 = WidgetProxy.BuildMatrix (new Vector3 ((m_WidgetFront.widget<UISprite> ().width - m_WidgetBack.widget<UISprite> ().width) * 0.5f, 0, 0), Quaternion.identity, Vector3.one);
        matrix *= matrix1;
        m_WidgetFront.matrix = matrix;

    }

    public bool UpdateGeometry ()
    {
        if (m_Actor.CacheTransform == null)
        {
            return false;
        }

        if (HUDBlood.m_bCameraChanged || m_LastPosition != m_Actor.CacheTransform.localPosition)
        {
            m_LastPosition = m_Actor.CacheTransform.localPosition;
            Vector3 pos = m_LastPosition;
            pos.y += (m_Actor.Height - 0.5f);
            Matrix4x4 matrix = WidgetProxy.BuildMatrix (pos, HUDBlood.m_CameraRotation, HUDBlood.mScale);
            m_WidgetBack.matrix = matrix;
            Matrix4x4 matrix1 = WidgetProxy.BuildMatrix (new Vector3 ((m_WidgetFront.widget<UISprite> ().width - m_WidgetBack.widget<UISprite> ().width) * 0.5f, 0, 0), Quaternion.identity, Vector3.one);
            matrix *= matrix1;
            m_WidgetFront.matrix = matrix;
        }

        if (WidgetProxy.s_ProxyPanel == null)
        {
            return false;
        }

        if (m_WidgetFront.m_widget.panel == null)
        {
            m_WidgetFront.m_widget.panel = WidgetProxy.s_ProxyPanel;
        }

        if (m_WidgetBack.m_widget.panel == null)
        {
            m_WidgetBack.m_widget.panel = WidgetProxy.s_ProxyPanel;
        }

        return m_WidgetFront.UpdateGeometry () | m_WidgetBack.UpdateGeometry ();
    }

    public virtual void FillGeometry (List<Vector3> verts, List<Vector2> uvs, List<Color> cols)
    {
        m_WidgetBack.FillGeometry (verts, uvs, cols);
        if (m_LastHpValue > 0)
        {
            m_WidgetFront.FillGeometry (verts, uvs, cols);
        }
    }

    public virtual void Release ()
    {
        m_WidgetBack.Release ();
        m_WidgetFront.Release ();
    }
}


public class HUDBlood : MonoBehaviour
{
    void Awake ()
    {
    }

    void Start ()
    {
        if (m_Mesh != null)
        {
            return;
        }

        gameObject.layer = (int)GTLayer.LAYER_DEFAULT;

        m_Mesh = gameObject.AddComponent<MeshFilter> ();
        m_Mesh.sharedMesh = new Mesh ();
        m_Mesh.sharedMesh.MarkDynamic ();

        m_Render = gameObject.AddComponent<MeshRenderer> ();
        m_Render.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        m_Render.receiveShadows = false;

        LoadAtlas ();
    }

    void OnDestroy ()
    {
        Clear ();
    }

    public void AddHudBlood (Actor actor)
    {
        if (actor == null)
        {
            return;
        }

        HpData hpObj;
        if (m_HpsDic.TryGetValue (actor, out hpObj) && hpObj != null)
        {
            hpObj.UpdatePercent ();
            return;
        }

        if (actor.IsDead ())
        {
            return;
        }

        hpObj = new HpData ();
        hpObj.m_Actor = actor;

        if (m_Atlas == null)
        {
            return;
        }

        if(m_HpsList.Count % 2 == 1)
        {
            hpObj.InitSprites(m_Atlas, "Red_Back", "Red_Front");
        }
        else
        {
            hpObj.InitSprites(m_Atlas, "Blue_Back", "Blue_Front");
        }

        hpObj.UpdatePercent ();

        m_HpsDic [actor] = hpObj;

        m_HpsList.Add (hpObj);

        m_bRebuild = true;
    }


    public void RemoveHudBlood (Actor actor)
    {
        HpData ob = null;
        if (m_HpsDic.TryGetValue (actor, out ob) && ob != null)
        {
            ob.Release ();
            m_HpsDic.Remove (actor);
            m_HpsList.Remove (ob);
            m_bRebuild = true;
        }
    }

    public UIAtlas LoadAtlas ()
    {
        GameObject obj = GTResourceManager.Instance.Load<GameObject> ("Atlas/Atlas_Hud/Atlas_Hud.prefab", true);
        if (obj != null)
        {
            obj.transform.SetParent (this.transform);
            m_Atlas = obj.GetComponent<UIAtlas> ();
            m_Render.sharedMaterial = m_Atlas.spriteMaterial;
        }

        return m_Atlas;
    }


    /**清除所有血条显示;*/
    public void Clear ()
    {
        for (int i = 0; i < m_HpsList.Count; i++)
        {
            m_HpsList [i].Release ();
        }
        m_HpsList.Clear ();
        m_HpsDic.Clear ();
    }

    void LateUpdate ()
    {
        m_bCameraChanged = false;
        Transform CameraTrans = GTCameraManager.Instance.CameraCtrl.transform;
        if (CameraTrans != null)
        {
            Quaternion ratation = CameraTrans.rotation;
            if (ratation != m_CameraRotation)
            {
                m_CameraRotation = ratation;
                m_bCameraChanged = true;
            }
        }



        if (m_HpsList.Count > 0)
        {
            for (int i = m_HpsList.Count - 1; i >= 0; i--)
            {
                HpData hpO = m_HpsList [i];
                m_bRebuild |= hpO.UpdateGeometry ();
            }
        }

        if(m_bRebuild)
        {
            m_Mesh.sharedMesh.Clear ();
            if (m_HpsList.Count > 0)
            {
                mVerts.Clear ();
                mUvs.Clear ();
                mCols.Clear ();
                for (int i = 0; i < m_HpsList.Count; i++)
                {
                    HpData hpO = m_HpsList [i];
                    hpO.FillGeometry (mVerts, mUvs, mCols);
                }
                m_Mesh.sharedMesh.vertices = mVerts.ToArray ();
                m_Mesh.sharedMesh.uv = mUvs.ToArray ();
                m_Mesh.sharedMesh.colors = mCols.ToArray ();
                m_Mesh.sharedMesh.triangles = WidgetProxy.GenerateCachedIndexBuffer (mVerts.Count, mVerts.Count * 3 / 2);
            }

            m_bRebuild = false;
        }
    }

    public static readonly float m_UiScale = 1 / 65f;

    //readonly int m_FontSize = 14;
    public static readonly int sHpWidth = 60;
    public static readonly int sHpHeight = 10;
    //static float s_HpSideWidth = 0f;

    public static bool m_bCameraChanged = false;
    public static Quaternion m_CameraRotation = Quaternion.identity;
    public static Vector3 mScale = new Vector3 (m_UiScale, m_UiScale, m_UiScale);

    List<Vector3>   mVerts = new List<Vector3> ();
    List<Vector2>   mUvs = new List<Vector2> ();
    List<Color> mCols = new List<Color> ();

    List<HpData>    m_HpsList = new List<HpData> ();
    Dictionary<Actor, HpData> m_HpsDic = new Dictionary<Actor, HpData> ();

    UIAtlas m_Atlas = null;
    MeshFilter m_Mesh = null;
    MeshRenderer    m_Render = null;
    public bool m_bRebuild = false;
}
}