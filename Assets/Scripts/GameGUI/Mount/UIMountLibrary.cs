using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIMountLibrary : GTWindow
{
    private GameObject      btnFight;
    private UIScrollView    view;
    private UIGrid          grid;
    private GameObject      template;

    private List<ItemMount> mMountItems = new List<ItemMount>();
    private int             mSelectIndex = 0;    
    private UILabel         mMountSpeed;
    private UILabel         mMountQuality;

    class ItemMount
    {
        public ulong        id;
        public UIToggle     toggle;
        public UISprite     quality;
        public UISprite     icon;
        public GameObject   dress;
        public GameObject   btn;
    }

    public UIMountLibrary()
    {
        MaskType = EWindowMaskType.None;
        Type = EWindowType.Window;
        Resident = false;
        Path = "Mount/UIMountLibrary";
        ShowMode = EWindowShowMode.SaveTarget;
    }

    protected override void OnAwake()
    {
        btnFight = transform.Find("BtnFight").gameObject;
        view = transform.Find("View").GetComponent<UIScrollView>();
        grid = transform.Find("View/Grid").GetComponent<UIGrid>();
        template = transform.Find("View/Template").gameObject;
        template.SetActive(false);
        mMountSpeed = transform.Find("MountSpeed/Value").GetComponent<UILabel>();
        mMountQuality= transform.Find("MountQuality/Value").GetComponent<UILabel>();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnFight).onClick = OnFightClick;
    }

    protected override void OnAddHandler()
    {
       
    }

    protected override void OnDelHandler()
    {
      
    }

    protected override void OnEnable()
    {
        InitMountItems();
        InitView();
        ShowView();
    }

    protected override void OnClose()
    {
        mSelectIndex = 0;
        mMountItems.Clear();
    }

    private void InitMountItems()
    {
        int group = GTWindowManager.Instance.GetToggleGroupId();

        foreach (var mountitem in GTMountData.Dict.Values)
        {
            GameObject item = NGUITools.AddChild(grid.gameObject, template);
            item.SetActive(true);
            UIToggle toggle = item.GetComponent<UIToggle>();
            toggle.group = group;

            ItemMount tab = new ItemMount();
            tab.id = mountitem.Guid;
            tab.toggle = toggle;
            tab.btn = item;
            tab.quality = item.transform.GetComponent<UISprite>();
            tab.icon = item.transform.Find("Icon").GetComponent<UISprite>();
            tab.dress = item.transform.Find("Dress").gameObject;
            int index = mMountItems.Count;

            if (GTMountData.m_setupMount == tab.id)
            {
                mSelectIndex = index;
            }

            UIEventListener.Get(tab.btn).onClick = (GameObject go) =>
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
               
                mSelectIndex = index;
                UIMount.SetSelectMountID(tab.id) ;
                ShowView();
            };

            mMountItems.Add(tab);
        }

        if(mMountItems.Count > 0)
        {
            mMountItems[mSelectIndex].toggle.value = true;
            UIMount.SetSelectMountID(mMountItems[0].id);
        }
        else
        {
            UIMount.SetSelectMountID(0);
        }
    }

    private void InitView()
    {
        for (int i = 0; i < mMountItems.Count; i++)
        {
            ItemMount tab = mMountItems[i];

            MountItem tMountItem = GTMountData.GetItem(tab.id);
            DMount dbMount = ReadCfgMount.GetDataById(tMountItem.MountID);
            DActor dbActor = ReadCfgActor.GetDataById(dbMount.ActorId);
            tab.dress.SetActive(GTMountData.m_setupMount == tab.id);
            GTItemHelper.ShowActorQuality(tab.quality, dbActor.Id);
            tab.icon.spriteName = dbActor.Icon;
        }
    }

    private void ShowView()
    {
        if(mMountItems.Count <= 0)
        {
            return;
        }

        ItemMount tab = mMountItems[mSelectIndex];

        MountItem tMountItem = GTMountData.GetItem(tab.id);
        DMount dbMount = ReadCfgMount.GetDataById(tMountItem.MountID);
        DActor dbActor = ReadCfgActor.GetDataById(dbMount.ActorId);

        mMountSpeed.text = dbActor.DefSpeed.ToPoint();
        GTItemHelper.ShowItemQuality(mMountQuality, dbActor.Quality);
        XCharacter role = GTData.Main;
        btnFight.SetActive(GTMountData.m_setupMount != tab.id);
    }

    private void OnRecvSetMount()
    {
        InitView();
        ShowView();
    }

    private void OnFightClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);

        GTNetworkSend.Instance.TrySetupMount(UIMount.GetSelectMountID());

        GTEventCenter.FireEvent(GTEventID.TYPE_MOUNT_SETUPMOUNT);
        
    }
}
