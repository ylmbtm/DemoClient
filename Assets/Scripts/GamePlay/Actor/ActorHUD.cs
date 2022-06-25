using UnityEngine;
using System.Collections;
using System;
using HUD;

public class ActorHUD : IActorComponent
{
    private Actor        mOwner;
    private HUDBoard     mBoard = null;

    public void Initial(Actor actor)
    {
        this.mOwner      = actor;
        GameObject go = GTPoolManager.Instance.GetObject(GTPrefabKey.PRE_UIBOARD);
        mBoard = go.GET<HUDBoard>();
        mBoard.transform.parent = null;
        mBoard.transform.parent = actor.CacheTransform;
        NGUITools.SetLayer(go, GTLayer.LAYER_DEFAULT);
        mBoard.SetTarget(actor.CacheTransform);
        mBoard.SetPath(GTPrefabKey.PRE_UIBOARD);
        mBoard.SetVisable(false);

        this.mBoard.name = GTTools.Format("Actor_HUD_{0}_{1}", actor.GUID, actor.ID);
    }

    public bool IsOk()
    {
        if (this.mBoard == null)
        {
            return false;
        }

        return true;
    }

    public void Show(string text1, string text2)
    {
        mBoard.SetVisable(true);
        mBoard.Show(text1, text2, 1);
    }

    public void SetHeight(float height)
    {
        mBoard.SetHeight(height);
    }

    public void SetVisable(bool active)
    {
        mBoard.SetVisable(active);
    }

    public void Release()
    {
        if(mBoard != null)
        {
            mBoard.Release();
        }
    }

    public void Execute()
    {

    }
}