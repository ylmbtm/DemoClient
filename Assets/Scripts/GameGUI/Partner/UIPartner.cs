using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIPartner : GTWindow
{
    private GameObject        btnClose;
    private List<UIPartnerElem> mPartnerDress = new List<UIPartnerElem>();
    private List<UIPartnerItem> mPartnerArray = new List<UIPartnerItem>();
    private UIScrollView      mScroll;
    private UIGrid            mGrid;

    class UIPartnerElem
    {
        public UISprite       quality;
        public UISprite       icon;
        public UILabel        name;
        public GameObject     btn;
        public UISprite       type;
    }

    class UIPartnerItem
    {
        public UIPartnerElem    elem = new UIPartnerElem();
        public UILabel        type1;
        public UILabel        type2;
        public UILabel        type3;
        public GameObject     btnBattle;
        public ulong          guid;
        public GameObject     btn;
    }

    public UIPartner()
    {
        Type = EWindowType.Window;
        Resident = false;
        Path = "Partner/UIPartner";
        MaskType = EWindowMaskType.Black;
        ShowMode = EWindowShowMode.HideOther;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        btnClose = pivot.Find("BtnClose").gameObject;
        Transform transDress = pivot.Find("PartnerDress");
        Transform transList  = pivot.Find("PartnerList");
        for (int i = 1; i <= 2; i++)
        {
            Transform trans = transDress.Find("Partner"+i);
			UIPartnerElem tab = new UIPartnerElem();
            tab.icon = trans.Find("Icon").GetComponent<UISprite>();
            tab.quality = trans.Find("Quality").GetComponent<UISprite>();
            tab.name = trans.Find("Name").GetComponent<UILabel>();
            tab.btn = trans.gameObject;
            tab.type= trans.Find("Type").GetComponent<UISprite>();
            mPartnerDress.Add(tab);
        }
			
        mScroll = transList.Find("View").GetComponent<UIScrollView>();
        mGrid   = transList.Find("View/Grid").GetComponent<UIGrid>();
        GameObject temp = transList.Find("View/Temp").gameObject;
        temp.SetActive(false);
		for (int i = 0; i < 30; i++)
        {
            GameObject go = NGUITools.AddChild(mGrid.gameObject, temp);
            go.SetActive(false);
            Transform trans = go.transform;
            UIPartnerItem tab = new UIPartnerItem();
            tab.elem.icon = trans.Find("Icon").GetComponent<UISprite>();
            tab.elem.quality = trans.Find("Quality").GetComponent<UISprite>();
            tab.elem.name = trans.Find("Name").GetComponent<UILabel>();
            tab.elem.btn = trans.gameObject;
            tab.elem.type = trans.Find("Type").GetComponent<UISprite>();
			tab.guid = (ulong)0;
            tab.type1 = trans.Find("Type1/Value").GetComponent<UILabel>();
            tab.type2 = trans.Find("Type2/Value").GetComponent<UILabel>();
            tab.type3 = trans.Find("Type3/Value").GetComponent<UILabel>();
            tab.btnBattle = trans.Find("Btn_Battle").gameObject;
            tab.btn = trans.gameObject;
            mPartnerArray.Add(tab);
        }
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnReturnClick;

        for (int i = 0; i < mPartnerDress.Count; i++)
        {
            int index = i;
            UIPartnerElem tab = mPartnerDress[i];
            UIEventListener.Get(tab.btn).onClick = (GameObject go) =>
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
                if (GTPartnerData.m_setupPartner[index] == 0)
                {
                    return;
                }
                UIPartnerStrength window = (UIPartnerStrength)GTWindowManager.Instance.OpenWindow(EWindowID.UIPartnerStrength);
                window.SetID(GTPartnerData.m_setupPartner[index]);
            };
        }
        for (int i = 0; i < mPartnerArray.Count; i++)
        {
            UIPartnerItem tab = mPartnerArray[i];
            UIEventListener.Get(tab.btnBattle).onClick = (GameObject go) =>
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
                UIPartnerBattle window=(UIPartnerBattle)GTWindowManager.Instance.OpenWindow(EWindowID.UIPartnerBattle);
                window.SetID(tab.guid);
            };

            UIEventListener.Get(tab.btn).onClick = (GameObject go) =>
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
                UIPartnerStrength window = (UIPartnerStrength)GTWindowManager.Instance.OpenWindow(EWindowID.UIPartnerStrength);
                window.SetID(tab.guid);
            };
        }
    }

    protected override void OnAddHandler()
    {
		GTEventCenter.AddHandler(GTEventID.TYPE_PARTNER_UPDATE_VIEW, OnRecvUpdatePartnerView);
    }

    protected override void OnDelHandler()
    {
		GTEventCenter.DelHandler(GTEventID.TYPE_PARTNER_UPDATE_VIEW, OnRecvUpdatePartnerView);
    }

    protected override void OnEnable()
    {
        ShowPartnerDressView();
        ShowPartnerArrayView();
    }

    protected override void OnClose()
    {
        mPartnerArray.Clear();
        mPartnerDress.Clear();
    }

    private void ShowPartnerDressView()
    {
		ulong[] array = GTPartnerData.m_setupPartner;
        for (int i=0;i<mPartnerDress.Count;i++)
        {
            UIPartnerElem tab = mPartnerDress[i];
            ulong guid = array[i];
            tab.icon.enabled = guid > 0;
            tab.name.enabled = guid > 0;
            if (guid > 0)
            {
				PartnerItem item = GTPartnerData.GetItem(guid);
				DPartner dbPartner = ReadCfgPartner.GetDataById(item.PartnerID);
				DActor dbActor = ReadCfgActor.GetDataById (dbPartner.ActorId);
				GTItemHelper.ShowQualityText(tab.name, dbActor.Name,dbActor.Quality);
				GTItemHelper.ShowActorQuality(tab.quality, dbPartner.ActorId);
				tab.icon.spriteName = dbActor.Icon;
            }
            else
            {
                GTItemHelper.ShowActorQuality(tab.quality, 0);
            }
        }
    }

    private void ShowPartnerArrayView()
    {
		int nIndex = 0;
		foreach (var partner in GTPartnerData.Dict.Values)
		{
			if (nIndex >= mPartnerArray.Count) {
				break;
			}
			
			UIPartnerItem tab = mPartnerArray[nIndex];
			tab.guid = partner.Guid;
			nIndex++;
		}
		
        for (int i = 0; i < mPartnerArray.Count; i++)
        {
            UIPartnerItem tab = mPartnerArray[i];
			if (tab.guid <= 0) 
			{
				break;
			}
			PartnerItem data = GTPartnerData.GetItem (tab.guid);
			DPartner db = ReadCfgPartner.GetDataById(data.PartnerID);
			DActor dbActor = ReadCfgActor.GetDataById (db.ActorId);
			GTItemHelper.ShowQualityText(tab.elem.name, dbActor.Name, dbActor.Quality);
            GTItemHelper.ShowActorQuality(tab.elem.quality, (int)tab.guid);
			tab.elem.icon.spriteName = dbActor.Icon;
			tab.type1.text = data == null ? "1" : data.StrengthLvl.ToString();
			tab.type2.text = data == null ? "0" : data.StarLevel.ToString();
			tab.type3.text = data == null ? "0" : data.RefineLevel.ToString();
			bool isBattle = tab.guid == GTPartnerData.m_setupPartner[0] || tab.guid == GTPartnerData.m_setupPartner[1];
            tab.btnBattle.SetActive(!isBattle);
			tab.btn.SetActive (true);
        }
    }

    private void OnReturnClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnRecvUpdatePartnerView()
    {
        ShowPartnerDressView();
        ShowPartnerArrayView();
    }
}
