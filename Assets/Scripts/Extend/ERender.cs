using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ERender
{
    static Dictionary<int, ERender> Renders = new Dictionary<int, ERender>();
    public Camera                   RenderCamera  { get; private set; }
    public RenderTexture            RenderTexture { get; private set; }
    public int                      Layer         { get; private set; }
    public UITexture                ViewTexture   { get; private set; }

    public ERender(Camera cam, UITexture uiTexture, int layer)
    {
        this.RenderCamera = cam;
        this.Layer = layer;
        this.ViewTexture = uiTexture;
        this.RenderCamera.gameObject.layer = layer;
        this.RenderCamera.cullingMask = 1 << layer;
        this.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
        this.RenderCamera.orthographic = false;
        this.RenderCamera.clearFlags = CameraClearFlags.SolidColor;
        this.RenderTexture = new RenderTexture(1024, 1024, 24);
        this.RenderCamera.targetTexture = this.RenderTexture;
        this.ViewTexture.mainTexture = this.RenderTexture;
        Color color = this.RenderCamera.backgroundColor;
        color.a = 0;
        this.RenderCamera.backgroundColor = color;
    }

    public static ERender AddRender(UITexture uiTexture)
    {
        if (uiTexture == null)
        {
            return null;
        }
        ERender render = null;
        for (int i = GTLayer.LAYER_RENDER_START; i < 32; i++)
        {
            if (Renders.ContainsKey(i)) continue;
            Camera cam = GTCameraManager.Instance.CreateCamera("RenderCamera_" + i.ToString());
            cam.transform.localPosition = new Vector3(0, 0, 10000);
            render = new ERender(cam,uiTexture, i);
            Renders.Add(i, render);
            break;
            
        }
        return render;
    }

    public GameObject AttachModel(GameObject model)
    {
        if(model == null)
        {
            return null;
        }

        if (RenderCamera == null)
        {
            return null;
        }

        model.transform.parent = this.RenderCamera.transform;
        NGUITools.SetLayer(model, Layer);
     
        return model;
    }

    public void DetachModel()
    {
        if (RenderCamera == null)
        {
            return;
        }

        RenderCamera.transform.DetachChildren();
    }


    public void Release()
    {
        DetachModel();
        Renders.Remove(Layer);
        RenderTexture.Release();
        Object.Destroy(RenderTexture);
        Object.Destroy(RenderCamera.gameObject);
        RenderTexture = null;
        RenderCamera = null;
    }

}
