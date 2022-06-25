using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIRoleGem : GTWindow
{ 
    public UIRoleGem()
    {
        MaskType = EWindowMaskType.None;
        ShowMode = EWindowShowMode.SaveTarget;
        Type = EWindowType.Window;
        Path = "Bag/UIRoleGem";
        Resident = false;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        equipGrid = pivot.Find("Equips/Grid").GetComponent<UIGrid>();
        labPropertys = pivot.Find("Propertys/Text").GetComponent<UILabel>();
        btnOneKeyToDress = pivot.Find("Btn_OneKeyToDress").gameObject;
        btnOneKeyToUnload = pivot.Find("Btn_OneKeyToUnload").gameObject;
        for (int i = 1; i <= 8; i++)
        {
            ItemEquip item = new ItemEquip();
            item.toggle = equipGrid.transform.Find(i.ToString()).GetComponent<UIToggle>();
            item.itemTexture = item.toggle.transform.Find("Texture").GetComponent<UITexture>();
            item.itemQuality = item.toggle.transform.Find("Quality").GetComponent<UISprite>();
            mEItems.Add(item);
        }

        for (int i = 1; i <= 5; i++)
        {
            ItemGem item = new ItemGem();
            item.itemBtn = pivot.Find("Gems/" + i.ToString()).gameObject;
            item.itemTexture = item.itemBtn.transform.Find("Texture").GetComponent<UITexture>();
            item.itemQuality = item.itemBtn.transform.Find("Quality").GetComponent<UISprite>();
            mGItems.Add(item);
        }
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnOneKeyToDress).onClick = OnOneKeyToDressClick;
        UIEventListener.Get(btnOneKeyToUnload).onClick = OneKeyToUnloadClick;

        for (int i = 0; i < mEItems.Count; i++)
        {
            UIEventListener.Get(mEItems[i].toggle.gameObject).onClick = OnEquipItemClick;
        }

        for (int i = 0; i < mGItems.Count; i++)
        {
            UIEventListener.Get(mGItems[i].itemBtn).onClick = OnGemItemClick;
        }
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_GEM_UPDATE_VIEW, OnUpdateGemView);
    }

    protected override void OnEnable()
    {
        ShowEquipsView();
        ShowGemCellsView();
        ShowGemPropertyView();
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_GEM_UPDATE_VIEW, OnUpdateGemView);
    }

    protected override void OnClose()
    {
        mEItems.Clear();
        mGItems.Clear();
    }

    private void OnGemItemClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        ulong guid = go.name.ToUInt64();
        GTWindowManager.Instance.OpenWindow(EWindowID.UIGemInfo);
        UIGemInfo w5 = (UIGemInfo)GTWindowManager.Instance.GetWindow(EWindowID.UIGemInfo);
        w5.ShowViewByGuid(guid, 0);
    }

    private void OnEquipItemClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        mCurIndex = go.name.ToInt32();
        ShowGemCellsView();
        ShowGemPropertyView();
    }

    private void OneKeyToUnloadClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTNetworkSend.Instance.TryOneKeyToUnloadGem(mCurIndex);
    }

    private void OnOneKeyToDressClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTNetworkSend.Instance.TryOneKeyToDressGem(mCurIndex);
    }

    private void ShowEquipsView()
    {
        int group = GTWindowManager.Instance.GetToggleGroupId();
        foreach (var equip in GTEquipData.Dict.Values)
        {
            if (equip.IsUsing <= 0)
            {
                continue;
            }

            DEquip equipDB1 = ReadCfgEquip.GetDataById(equip.EquipID);
            if (equipDB1 == null)
            {
                continue;
            }

            ItemEquip item = mEItems[equipDB1.Pos-1];
            item.toggle.group = group;
          
            item.Show(true);
            GTItemHelper.ShowItemTexture(item.itemTexture, equipDB1.Id);
            GTItemHelper.ShowItemQuality(item.itemQuality, equipDB1.Id);
        }
        mEItems[mCurIndex-1].toggle.value = true;
    }


    private void ShowGemCellsView()
    {
        for (int i = 0; i < 5; i++)
        {
            mGItems[i].Show(false);
        }

        foreach (var item in GTGemData.Dict.Values)
        {
            if (item.Pos <= 0)
            {
                continue;
            }

            if (mCurIndex != item.Pos)
            {
                continue;
            }

            DGem stGem= ReadCfgGem.GetDataById(item.GemID);
            ItemGem cell = mGItems[stGem.Pos -1];
            GTItemHelper.ShowItemTexture(cell.itemTexture, item.GemID);
            GTItemHelper.ShowItemQuality(cell.itemQuality, item.GemID);
            cell.Show(true);
        }

    }

    private void ShowGemPropertyView()
    {
        labPropertys.text = string.Empty;
        //GTItemHelper.ShowPropertyText(labPropertys, attrValues, false);
    }

    public int GetCurIndex()
    {
        return mCurIndex;
    }

     public void OnUpdateGemView()
    {
        ShowGemCellsView();
     }

    private UIGrid          equipGrid;
    private UILabel         labPropertys;
    private GameObject      btnOneKeyToDress;
    private GameObject      btnOneKeyToUnload;
    private int             mCurIndex = 1;
    private List<ItemEquip> mEItems = new List<ItemEquip>();
    private List<ItemGem>   mGItems = new List<ItemGem>();

    class ItemEquip
    {
        public UIToggle     toggle;
        public UITexture    itemTexture;
        public UISprite     itemQuality;

        public void Show(bool hasDress)
        {
            itemTexture.gameObject.SetActive(hasDress);
            itemQuality.gameObject.SetActive(hasDress);
        }
    }

    class ItemGem
    {
        public GameObject   itemBtn;
        public UITexture    itemTexture;
        public UISprite     itemQuality;

        public void Show(bool hasDress)
        {
            itemTexture.gameObject.SetActive(hasDress);
            itemQuality.gameObject.SetActive(hasDress);
        }
    }
}
