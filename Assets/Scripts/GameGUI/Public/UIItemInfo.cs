
using UnityEngine;
using System.Collections;
using System;
using Protocol;

public class UIItemInfo : GTWindow
{
    private GameObject btnUse;
    private GameObject btnBatchUse;
    private GameObject btnClose;
    private GameObject btnSure;
    private GameObject btnDress;
    private UILabel    itemName;
    private UILabel    itemDesc;
    private UILabel    itemNum;
    private UITexture  itemTexture;
    private UISprite   itemQuality;
    private ulong      mGuid;  //格子的GUID
    private Transform  pivot;

    public UIItemInfo()
    {
        Resident = true;
        Path = "Public/UIItemInfo";
        Type = EWindowType.Window;
        MaskType = EWindowMaskType.BlackTransparent;
    }

    protected override void OnAwake()
    {
        pivot = transform.Find("Pivot");
        btnClose = pivot.Find("BtnClose").gameObject;
        btnUse = pivot.Find("BtnUse").gameObject;
        btnBatchUse = pivot.Find("BtnBatchUse").gameObject;
        btnSure = pivot.Find("BtnSure").gameObject;
        btnDress = pivot.Find("BtnDress").gameObject;
        itemDesc = pivot.Find("ItemDesc").GetComponent<UILabel>();
        itemName = pivot.Find("Item/Name").GetComponent<UILabel>();
        itemNum = pivot.Find("Item/Num").GetComponent<UILabel>();
        itemTexture = pivot.Find("Item/Texture").GetComponent<UITexture>();
        itemQuality = pivot.Find("Item/Quality").GetComponent<UISprite>();
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler<int, int>(GTEventID.TYPE_BAG_USE_ITEM, OnRecvUseItem);
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler<int, int>(GTEventID.TYPE_BAG_USE_ITEM, OnRecvUseItem);
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnCloseClick;
        UIEventListener.Get(btnUse).onClick = OnUseClick;
        UIEventListener.Get(btnBatchUse).onClick = OnBatchUseClick;
        UIEventListener.Get(btnSure).onClick = OnSureClick;
        UIEventListener.Get(btnDress).onClick = OnDressClick;
    }

    protected override void OnClose()
    {
        mGuid = 0;
    }

    protected override void OnEnable()
    {

    }

    private void OnBatchUseClick(GameObject go)
    {
//         GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
//         GTItemHelper.ShowItemUseDialogByPos(pos);
    }

    private void OnUseClick(GameObject go)
    {
//         GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
//         GTNetworkSend.Instance.TryUseItemByPos(pos, 1);
    }

    private void OnDressClick(GameObject go)
    {
        BagItem item = GTBagData.GetItem(mGuid);
        DItem itemDB = ReadCfgItem.GetDataById(item.ItemID);
        switch(itemDB.Data3)
        {
            case 1:
               // GTData.RemoteData.HPDrugItemID = item.ItemID;
                break;
            case 2:
               // GTData.RemoteData.MPDrugItemID = item.ItemID;
                break;
        }
        GTEventCenter.FireEvent(GTEventID.TYPE_ACTOR_CHANGE_AUTODRUG);
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnSureClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnRecvUseItem(int id,int num)
    {
        Hide();
    }

    public void ShowViewById(int itemID)
    {
        DItem itemDB = ReadCfgItem.GetDataById(itemID);
        itemDesc.text = itemDB.Desc;
        itemNum.text = string.Empty;
        GTItemHelper.ShowItemTexture(itemTexture, itemID);
        GTItemHelper.ShowItemName(itemName, itemID);
        GTItemHelper.ShowItemQuality(itemQuality, itemID);
        btnBatchUse.SetActive(false);
        btnUse.SetActive(false);
        btnSure.SetActive(true);
    }

    public void ShowViewByGuid(ulong guid)
    {
        this.mGuid = guid;
        BagItem item = GTBagData.GetItem(guid);
        int itemID = item.ItemID;
        DItem itemDB = ReadCfgItem.GetDataById(itemID);
        itemDesc.text = itemDB.Desc;
        itemNum.text = GTTools.Format("拥有数量：{0}", item.ItemNum);
        GTItemHelper.ShowItemTexture(itemTexture, itemID);
        GTItemHelper.ShowItemName(itemName, itemID);
        GTItemHelper.ShowItemQuality(itemQuality, itemID);
        switch(itemDB.ItemType)
        {
            case EItemType.EIT_DRUG:
                btnDress.SetActive(true);
                btnBatchUse.SetActive(false);
                btnUse.SetActive(true);
                btnSure.SetActive(false);
                break;
            case EItemType.EIT_ITEM:
                btnBatchUse.SetActive(false);
                btnDress.SetActive(false);
                btnUse.SetActive(false);
                btnSure.SetActive(true);
                break;
            default:
                btnBatchUse.SetActive(true);
                btnDress.SetActive(false);
                btnUse.SetActive(true);
                btnSure.SetActive(false);
                break;
        }
    }
}
