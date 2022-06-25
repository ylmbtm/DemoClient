using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIMountTurned : GTWindow
{
    private UIScrollView    view;
    private UIGrid          grid;
    private GameObject      template;
    private List<ItemMount> mMountItems = new List<ItemMount>();
    private int             mSelectIndex = 0;

    class ItemMount
    {
        public ulong        id;
        public UIToggle   toggle;
        public UISprite   quality;
        public UISprite   icon;
        public GameObject dress;
        public GameObject btn;
    }

    public UIMountTurned()
    {
        MaskType = EWindowMaskType.None;
        Type = EWindowType.Window;
        Resident = false;
        ShowMode = EWindowShowMode.SaveTarget;
        Path = "Mount/UIMountTurned";
    }

    protected override void OnAwake()
    {
        view = transform.Find("View").GetComponent<UIScrollView>();
        grid = transform.Find("View/Grid").GetComponent<UIGrid>();
        template = transform.Find("View/Template").gameObject;
        template.SetActive(false);
    }

    protected override void OnEnable()
    {
        InitMountItems();
        InitView();
    }

    protected override void OnAddButtonListener()
    {

    }

    protected override void OnAddHandler()
    {

    }

    protected override void OnDelHandler()
    {

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
            UIEventListener.Get(tab.btn).onClick = (GameObject go) =>
            {
                if (mSelectIndex == index)
                {
                    return;
                }
                this.mSelectIndex = index;
                UIMount.SetSelectMountID(tab.id);
                this.ShowView();
            };
            mMountItems.Add(tab);
        }

        if(mMountItems.Count > 0)
        {
            mMountItems[0].toggle.value = true;
            UIMount.SetSelectMountID(mMountItems[0].id);
        }
        else
        {
            UIMount.SetSelectMountID(0);
        }
    }

    private void InitView()
    {
        XCharacter role = GTData.Main;
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

    }
}
