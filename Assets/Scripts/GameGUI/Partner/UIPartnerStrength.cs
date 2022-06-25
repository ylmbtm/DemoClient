using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIPartnerStrength : GTWindow
{
    private ulong           selectID;
    private UITexture       modelTexture;
    private GameObject      model;
    private GameObject      btnClose;

    private UIToggle        menuProperty;
    private UIToggle        menuSkill;
    private UIToggle        menuUpStar;
    private UIToggle        menuAwake;
    private UIToggle        menuWash;
    private GameObject      btnUpLevel;

    private UILabel         partnerLevel;
    private UILabel         partnerExp;
    private UILabel         partnerName;
    private UISlider        partnerExpValue;

    private ERender         mRender;
    private ActorAvator     mAvatar;

    public UIPartnerStrength()
    {
        Type = EWindowType.Window;
        Resident = false;
        ShowMode = EWindowShowMode.HideOther;
        MaskType = EWindowMaskType.Black;
        Path = "Partner/UIPartnerStrength";
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        modelTexture = pivot.Find("Texture").GetComponent<UITexture>();
        btnClose = pivot.Find("BtnClose").gameObject;

        Transform menuTrans = pivot.Find("Menus");
        menuProperty = menuTrans.Find("Tab_Property").GetComponent<UIToggle>();
        menuSkill = menuTrans.Find("Tab_Skill").GetComponent<UIToggle>();
        menuUpStar = menuTrans.Find("Tab_UpStar").GetComponent<UIToggle>();
        menuAwake = menuTrans.Find("Tab_Awake").GetComponent<UIToggle>();
        menuWash= menuTrans.Find("Tab_Wash").GetComponent<UIToggle>();

        Transform levelTrans = pivot.Find("Level");
        btnUpLevel = levelTrans.Find("Btn_UpLevel").gameObject;
        partnerExp = levelTrans.Find("Exp/Value").GetComponent<UILabel>();
        partnerExpValue = levelTrans.Find("Exp").GetComponent<UISlider>();
        partnerName = levelTrans.Find("Name").GetComponent<UILabel>();
        partnerLevel = levelTrans.Find("Level").GetComponent<UILabel>();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnCloseClick;
        UIEventListener.Get(modelTexture.gameObject).onDrag = OnTextureDrag;
        UIEventListener.Get(btnUpLevel).onClick = OnUpLevelClick;
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
        if (mRender != null)
        {
            mRender.Release();
            mRender = null;
        }

        if (mAvatar != null)
        {
            GTResourceManager.Instance.DestroyObj(mAvatar.GetRootObj());
            mAvatar = null;
        }
    }

    private void ShowView()
    {
        PartnerItem data = GTPartnerData.GetItem(selectID);
        if (data == null)
        {
            return;
        }
        partnerLevel.text = GTTools.Format("等级 {0}", data.RefineLevel);
        partnerExp.text = GTTools.Format("{0}/{1}", data.RefineExp, 2500);
        partnerExpValue.value = 0;

		DPartner dbPartner = ReadCfgPartner.GetDataById (data.PartnerID);

		DActor db = ReadCfgActor.GetDataById(dbPartner.ActorId);
        GTItemHelper.ShowQualityText(partnerName, db.Name, db.Quality);
    }

    private void OnUpLevelClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnTextureDrag(GameObject go, Vector2 delta)
    {
        if (model == null)
        {
            return;
        }
        ESpin.Get(model).OnSpin(delta, 2);
    }

    private void InitModel()
    {
        PartnerItem item = GTPartnerData.GetItem(selectID);
        DPartner db = ReadCfgPartner.GetDataById(item.PartnerID);
		DActor dbActor = ReadCfgActor.GetDataById (db.ActorId);
        mRender = ERender.AddRender(modelTexture);
        if (mAvatar != null)
        {
            GTResourceManager.Instance.DestroyObj(mAvatar.GetRootObj());
            mAvatar = null;
        }
        mAvatar = GTWorld.Instance.AddAvatar(dbActor.Model);
        mAvatar.PlayAnim("idle", null);
        GameObject model = mRender.AttachModel(mAvatar.GetRootObj());
        model.transform.localPosition = new Vector3(0f, -0.4f, 1.2f);
        model.transform.localEulerAngles = new Vector3(0, 180, 0);
        model.transform.localScale = Vector3.one * 0.3f;
    }

    public void SetID(ulong  guid)
    {
        this.selectID = guid;
        this.InitModel();
        this.ShowView();
    }
}
