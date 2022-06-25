using UnityEngine;
using System.Collections;
using System;
using CFG;
using System.Collections.Generic;
using Protocol;
using TAS;

public class UITaskTalk : GTWindow
{
    class ItemMoney
    {
        public UILabel                            moneyNum;
        public UITexture                          moneyTexture;
    }

    class TalkModel
    {
        public GameObject                         model;
        public string                             path;
    }

    private GameObject                            btnReceive;
    private GameObject                            btnContinue;
    private UILabel                               mTalkContent;
    private Transform                             mRewardsTrans;
    private List<ItemMoney>                       mMoneys           = new List<ItemMoney>();
    private TypewriterEffect                      mTypewriterEffect;
    private Dictionary<ETaskDialogPos, TalkModel> mTalkModels;
    private Dictionary<ETaskDialogPos, Transform> mTalkModelParents = new Dictionary<ETaskDialogPos, Transform>();
    
    public UITaskTalk()
    {
        Path = "Task/UITaskTalk";
        Type = EWindowType.Window;
        ShowMode = EWindowShowMode.HideOther;
        mTalkModels = new Dictionary<ETaskDialogPos, TalkModel>()
        {
           {
                ETaskDialogPos.TYPE_LF,
                new TalkModel() {model = null,path = null }
           },
           {
                ETaskDialogPos.TYPE_RT,
                new TalkModel() {model = null,  path = null }
            }
        };
        MaskType = EWindowMaskType.WhiteTransparent;
    }

    protected override void OnAwake()
    {
        Transform pivot   = transform.Find("Pivot");
        btnReceive        = pivot.Find("Btn_Receive").gameObject;
        btnContinue       = pivot.Find("Btn_Continue").gameObject;
        mTalkContent      = pivot.Find("Content").GetComponent<UILabel>();
        mTypewriterEffect = mTalkContent.GetComponent<TypewriterEffect>();
        mTalkModelParents[ETaskDialogPos.TYPE_LF] = pivot.Find("LF");
        mTalkModelParents[ETaskDialogPos.TYPE_RT] = pivot.Find("RT");
        mRewardsTrans = pivot.Find("Rewards");
        for (int i = 1; i <= 2; i++)
        {
            Transform p = mRewardsTrans.Find("Moneys/" + i);
            ItemMoney tab = new ItemMoney();
            tab.moneyTexture = p.Find("Texture").GetComponent<UITexture>();
            tab.moneyNum = p.Find("Num").GetComponent<UILabel>();
            mMoneys.Add(tab);
        }
    }

    protected override void OnAddButtonListener()
    {
        
    }

    protected override void OnAddHandler()
    {
        
    }

    protected override void OnEnable()
    {
        
    }

    protected override void OnDelHandler()
    {
        
    }

    protected override void OnClose()
    {
        mMoneys.Clear();
        mTalkModelParents.Clear();
    }

    private string ParseTalk(string src)
    {
        XCharacter role = GTData.Main;
        string s = src.Replace("[ROLE]", role.Name);
        return s;
    }
}
