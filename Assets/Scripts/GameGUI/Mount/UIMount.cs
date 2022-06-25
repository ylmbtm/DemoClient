using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIMount : GTWindow
{

    enum Func
    {
        Func_None = -1,
        Func_Library,
        Func_Tame,
        Func_Blood,
        Func_Turned
    }

    private UILabel                     mountName;
    private GameObject                  btnClose;
    private UITexture                   modelTexture;
    private UIToggle[]                  menus;

    private ERender                     mRender;
    private ActorAvator                 mAvatar;
    private Func                        mSelectFunc = Func.Func_None;
    private Dictionary<Func, EWindowID> mFuncWindows = new Dictionary<Func, EWindowID>
    {
        { Func.Func_Blood,   EWindowID.UIMountBlood },
        { Func.Func_Library, EWindowID.UIMountLibrary },
        { Func.Func_Tame,    EWindowID.UIMountTame },
        { Func.Func_Turned,  EWindowID.UIMountTurned }
    };

    public UIMount()
    {
        Type = EWindowType.Window;
        Resident = false;
        Path = "Mount/UIMount";
        MaskType = EWindowMaskType.Black;
        ShowMode = EWindowShowMode.HideOther;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        mountName = pivot.Find("MountName").GetComponent<UILabel>();
        modelTexture = pivot.Find("ModelTexture").GetComponent<UITexture>();
        btnClose = pivot.Find("BtnClose").gameObject;
        menus = new UIToggle[4];
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i] = pivot.Find("Menus/Menu_" + (i + 1)).GetComponent<UIToggle>();
        }
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnCloseClick;
        UIEventListener.Get(modelTexture.gameObject).onDrag = OnMountDrag;
        for (int i = 0; i < menus.Length; i++)
        {
            UIToggle menu = menus[i];
            Func func = (Func)i;
            UIEventListener.Get(menu.gameObject).onClick = (GameObject go) =>
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
                Switch(func);
                ShowView();
            };
        }
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_MOUNT_SELECTMOUNT, OnChangeSelectMount);
        GTEventCenter.AddHandler(GTEventID.TYPE_MOUNT_SETUPMOUNT,  OnSetupMount);
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_MOUNT_SELECTMOUNT, OnChangeSelectMount);
        GTEventCenter.DelHandler(GTEventID.TYPE_MOUNT_SETUPMOUNT, OnSetupMount);
    }

    protected override void OnEnable()
    {
        GTCoroutinueManager.Instance.StartCoroutine(FirstOpen());
    }

    protected override void OnClose()
    {
        SetSelectMountID(0);

        if (mRender != null)
        {
            mRender.Release();
            mRender = null;
        }

        if(mAvatar != null)
        {
            GTResourceManager.Instance.DestroyObj(mAvatar.GetRootObj());
            mAvatar = null;
        }
    }

    private void OnMountDrag(GameObject go, Vector2 delta)
    {
        if (mAvatar == null && mAvatar.GetRootObj() != null)
        {
            return;
        }
        ESpin.Get(mAvatar.GetRootObj()).OnSpin(delta, 2);
    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Switch(Func.Func_None);
        Hide();
    }

    private void InitToggles()
    {
        int group = GTWindowManager.Instance.GetToggleGroupId();
        for (int i = 0; i < menus.Length; i++)
        {
            menus[i].group = group;
        }
        menus[(int)mSelectFunc].value = true;
    }

    private void ShowView()
    {
        MountItem tMountItem = GTMountData.GetItem(mSelectMountID);
        if(tMountItem == null)
        {
            return;
        }


        DMount dbMount = ReadCfgMount.GetDataById(tMountItem.MountID);
        if(dbMount == null)
        {
            return;
        }

        DActor db = ReadCfgActor.GetDataById(dbMount.ActorId);
        if (db == null)
        {
            return;
        }
        GTItemHelper.ShowItemName(mountName, db.Quality, db.Name);
    }

    private void ShowModel()
    {
        MountItem tMountItem = GTMountData.GetItem(mSelectMountID);
        if(tMountItem == null)
        {
            return;
        }

        DMount dbMount = ReadCfgMount.GetDataById(tMountItem.MountID);
        if (dbMount == null)
        {
            return;
        }

        if(mRender != null)
        {
            mRender.Release();
            mRender = null;
        }
        
        if (mAvatar != null)
        {
            GTResourceManager.Instance.DestroyObj(mAvatar.GetRootObj());
            mAvatar = null;
        }

        if (mRender == null)
        {
            mRender = ERender.AddRender(modelTexture);
        }

        DActor dbActor = ReadCfgActor.GetDataById(dbMount.ActorId);
        mAvatar = GTWorld.Instance.AddAvatar(dbActor.Model);
        mAvatar.PlayAnim("idle", null);
        GameObject model = mRender.AttachModel(mAvatar.GetRootObj());
        model.transform.localPosition = new Vector3(dbMount.X, dbMount.Y, dbMount.Z);
        model.transform.localEulerAngles = new Vector3(0, 120, 0);
        model.transform.localScale = Vector3.one * dbMount.Scale * 1.2f;
    }

    private void Switch(Func func)
    {
        if (mSelectFunc == func)
        {
            return;
        }
        if (mSelectFunc != Func.Func_None)
        {
            EWindowID id = mFuncWindows[mSelectFunc];
            GTWindowManager.Instance.HideWindow(id);
        }
        mSelectFunc = func;
        if (mSelectFunc != Func.Func_None)
        {
            EWindowID newID = mFuncWindows[mSelectFunc];
            GTWindowManager.Instance.OpenWindow(newID);
        }
    }

    private IEnumerator FirstOpen()
    {
        yield return null;
        Switch(Func.Func_Library);
        InitToggles();
    }

    private void OnChangeSelectMount()
    {
        ShowModel();
        ShowView();
    }

    private void OnSetupMount()
    {
        Switch(Func.Func_None);
        Hide();
    }

    static ulong mSelectMountID;

    public static ulong GetSelectMountID()
    {
        return mSelectMountID;
    }

    public static int SetSelectMountID(ulong value)
    {
        mSelectMountID = value;
        if (value > 0)
        {
            GTEventCenter.FireEvent(GTEventID.TYPE_MOUNT_SELECTMOUNT);
        }
        return 0;
    }
}
