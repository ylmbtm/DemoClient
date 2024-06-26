﻿using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIGemInfo : GTWindow
{
    private UILabel     gemType;
    private UILabel     gemName;
    private UILabel     gemLevel;
    private UILabel     gemPropertys;

    private UITexture   gemTexture;
    private UISprite    gemQuality;
    private GameObject  gemDress;

    private UILabel     gemSuitName;
    private UILabel     gemSuitText;
    private UILabel     gemFightValue;

    private GameObject  btnClose;
    private GameObject  btnUp;
    private GameObject  btnDress;
    private GameObject  btnUnload;

    private GameObject  notSuit;
    private GameObject  hasSuit;
    private ulong       mGuid;  //宝石的GUID
    private ulong       mBagGuid;

    public UIGemInfo()
    {
        Type = EWindowType.Window;
        Resident = false;
        Path = "Public/UIGemInfo";
        MaskType = EWindowMaskType.BlackTransparent;
        ShowMode = EWindowShowMode.DoNothing;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        Transform left = pivot.Find("Left");
        Transform right = pivot.Find("Right");
        gemName = left.Find("GemName").GetComponent<UILabel>();
        gemType= left.Find("GemType").GetComponent<UILabel>();
        gemFightValue = left.Find("FightValue").GetComponent<UILabel>();
        gemLevel = left.Find("Level").GetComponent<UILabel>();
        gemQuality = left.Find("Item/Quality").GetComponent<UISprite>();
        gemTexture = left.Find("Item/Texture").GetComponent<UITexture>();
        gemDress = left.Find("Item/Dress").gameObject;

        btnClose = pivot.Find("BtnClose").gameObject;
        btnDress = pivot.Find("Btn_Dress").gameObject;
        btnUnload = pivot.Find("Btn_Unload").gameObject;
        btnUp = pivot.Find("Btn_Up").gameObject;
        gemPropertys = left.Find("Propertys").GetComponent<UILabel>();
        gemSuitName = right.Find("SuitName").GetComponent<UILabel>();
        gemSuitText = right.Find("SuitText").GetComponent<UILabel>();
        notSuit = right.Find("NotSuit").gameObject;
        hasSuit = right.Find("HasSuit").gameObject;
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnCloseClick;
        UIEventListener.Get(btnUnload).onClick = OnUnDressClick;
        UIEventListener.Get(btnDress).onClick = OnDressClick;
        UIEventListener.Get(btnUp).onClick = OnUpClick;
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler<int, int>(GTEventID.TYPE_BAG_DRESS_GEM, OnRecvDressGem);
        GTEventCenter.AddHandler<int, int>(GTEventID.TYPE_BAG_UNDRESS_GEM, OnRecvUnDressGem);
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler<int, int>(GTEventID.TYPE_BAG_DRESS_GEM, OnRecvDressGem);
        GTEventCenter.DelHandler<int, int>(GTEventID.TYPE_BAG_UNDRESS_GEM, OnRecvUnDressGem);
    }

    protected override void OnEnable()
    {

    }

    protected override void OnClose()
    {

    }

    private void ShowHasSuit(DGem gemDB)
    {
        notSuit.SetActive(false);
        hasSuit.SetActive(true);
        transform.Find("Pivot/Right/Grid").gameObject.SetActive(true);
        transform.Find("Pivot/Right/Title").gameObject.SetActive(true);
        DGemSuit suitDB = ReadCfgGemSuit.GetDataById(gemDB.Suit);
        gemSuitName.text = suitDB.Name;
    }

    private void ShowNoSuit()
    {
        notSuit.SetActive(true);
        hasSuit.SetActive(false);
        gemSuitName.text = string.Empty;
        gemSuitText.text = string.Empty;
        transform.Find("Pivot/Right/Grid").gameObject.SetActive(false);
        transform.Find("Pivot/Right/Title").gameObject.SetActive(false);
    }

    private void ShowDress(bool isDress)
    {
        btnUnload.SetActive(isDress);
        gemDress.SetActive(isDress);
        btnDress.SetActive(!isDress);
    }

    private void ShowBaseView(int itemID)
    {
        gemType.text = MLGem.Instance.GetGemTypeName(itemID);
        GTItemHelper.ShowItemName(gemName, itemID);
        GTItemHelper.ShowItemTexture(gemTexture, itemID);
        GTItemHelper.ShowItemQuality(gemQuality, itemID);
    }

    private void ShowSuitPropertysView(int activeSuitNum,DGemSuit suitDB)
    {
        gemSuitText.text = string.Empty;
        for (int i = 0; i < suitDB.SuitAttrs.Count; i++)
        {
            Dictionary<int, int> attrValues = suitDB.SuitAttrs[i];
            string s = GTTools.Format(suitDB.SuitDesc, i + 2, attrValues[(int)EAttrID.EA_HP_MAX], attrValues[(int)EAttrID.EA_ATTACK]);
            string str = string.Empty;
            if (activeSuitNum >= i + 2)
            {
                str = GTTools.Format("[00ff00]{0}[-]", s);
            }
            else
            {
                str = GTTools.Format("[777777]{0}[-]", s);
            }
            gemSuitText.Append(str);
        }
    }

    private void ShowSameSuitGemsView(int itemID)
    {
        List<int> list = MLGem.Instance.GetSameSuitIDListByID(itemID);
        Transform parent = transform.Find("Right/Grid");
        for (int i=0;i<list.Count;i++)
        {
            DGem gemDB = ReadCfgGem.GetDataById(list[i]);
            GameObject item = parent.Find((i + 1).ToString()).gameObject;
            item.SetActive(true);
            item.name = gemDB.Id.ToString();
            UILabel itemName = item.transform.Find("Name").GetComponent<UILabel>();
            UITexture itemTexture = item.transform.Find("Texture").GetComponent<UITexture>();
            UISprite itemQuality = item.transform.Find("Quality").GetComponent<UISprite>();
            GTItemHelper.ShowItemName(itemName, gemDB.Id);
            GTItemHelper.ShowItemTexture(itemTexture, gemDB.Id);
            GTItemHelper.ShowItemQuality(itemQuality, gemDB.Id);
        }
    }

    private void OnRecvUnDressGem(int arg1, int arg2)
    {
        Hide();
    }

    private void OnRecvDressGem(int arg1, int arg2)
    {
        Hide();
    }

    private void OnUpClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTItemHelper.ShowGemWindowByGuid(mGuid);
    }

    private void OnDressClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        UIRoleGem window = (UIRoleGem)GTWindowManager.Instance.GetWindow(EWindowID.UIRoleGem);
        GTNetworkSend.Instance.TryDressGem( window.GetCurIndex(), mGuid, mBagGuid);
        Hide();
    }

    private void OnUnDressClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        UIRoleGem window = (UIRoleGem)GTWindowManager.Instance.GetWindow(EWindowID.UIRoleGem);
        GTNetworkSend.Instance.TryUnDressGem(mGuid);
        Hide();
    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    public void ShowViewByGuid(ulong gemGuid, ulong bagGuid)
    {
        this.mGuid = gemGuid;
        this.mBagGuid = bagGuid;

        GemItem item = GTGemData.GetItem(gemGuid);
        if (item == null)
        {
            return;
        }
        ShowDress(item.Pos < 0);
        DGem gemDB = ReadCfgGem.GetDataById(item.GemID);
        gemLevel.text = GTTools.Format("等级 {0}", item.StrengthLvl);
        gemFightValue.text = GTTools.Format("战斗力 {0}", 100);
        ShowBaseView(item.GemID);
        GTItemHelper.ShowGemPropertyText(gemPropertys, item.GemID, item.StrengthLvl);
//         DGemSuit suitDB = ReadCfgGemSuit.GetDataById(gemDB.Suit);
//         int activeSuitNum = isDress ? MLGem.Instance.GetActiveSameSuitsCountByPos(pos) : 0;
//         bool hasSuit = ReadCfgGemSuit.ContainsKey(gemDB.Suit);
//         if (hasSuit)
//         {
//             ShowHasSuit(gemDB);
//             ShowSuitPropertysView(activeSuitNum, suitDB);
//             ShowSameSuitGemsView(gemDB.Id);
//         }
//         else
        {
            ShowNoSuit();
        }
    }

}
