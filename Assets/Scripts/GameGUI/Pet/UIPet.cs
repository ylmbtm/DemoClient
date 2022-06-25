using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIPet : GTWindow
{
    private GameObject        btnClose;
    private GameObject        btnUpgrade;
    private GameObject        btnBattle;
    private GameObject        btnUnload;
    private GameObject        btnAutoToAddFood;

    private UISlider          expSlider;
    private UILabel           expNum;

    private UIScrollView      petView;
    private UIGrid            petGrid;
    private UICenterOnChild   petCenterOnChild;
    private GameObject        petTemp;

    private UILabel           petCurrPropertyText1;
    private UILabel           petCurrPropertyText2;
    private UILabel           petMainPropertyText1;
    private UILabel           petMainPropertyText2;
    private UILabel           petDesc;
    private UILabel           petName;
    private UITexture         petTexture;

    private List<FoodItem>    mFoods = new List<FoodItem>();
    private List<TempItem>    mTemps = new List<TempItem>();
    private List<XItem>       mItems = new List<XItem>();
    private ulong             mCurPetGuid;
    private ERender           mRender;
    private ActorAvator       mAvatar;

    class FoodItem
    {
        public GameObject     itemBtn;
        public UITexture      itemTexture;
        public UISprite       itemQuality;
    }

    class TempItem
    {
        public ulong          guid;
        public GameObject     btn;
        public GameObject     dress;
        public UIToggle       toggle;
        public UISprite       icon;
        public UISprite       quality;
        public UILabel        name;
    }

    public UIPet()
    {
        Type = EWindowType.Window;
        Resident = false;
        Path = "Pet/UIPet";
        MaskType = EWindowMaskType.Black;
        ShowMode = EWindowShowMode.HideOther;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        btnClose = pivot.Find("BtnClose").gameObject;
        btnUpgrade = pivot.Find("Bottom/BtnUpgrade").gameObject;
        btnBattle = pivot.Find("Bottom/BtnBattle").gameObject;
        btnUnload = pivot.Find("Bottom/BtnUnload").gameObject;
        btnAutoToAddFood = pivot.Find("Bottom/BtnAutoToAddFood").gameObject;

        Transform foodTrans = pivot.Find("Bottom/Foods");
        for (int i = 1; i <= 6; i++)
        {
            FoodItem tab = new FoodItem();
            Transform trans = foodTrans.Find(i.ToString());
            tab.itemBtn = trans.gameObject;
            tab.itemTexture = trans.Find("Texture").GetComponent<UITexture>();
            tab.itemQuality = trans.Find("Quality").GetComponent<UISprite>();
            mFoods.Add(tab);
        }

        expSlider = pivot.Find("Bottom/Progress").GetComponent<UISlider>();
        expNum = pivot.Find("Bottom/Progress/Num").GetComponent<UILabel>();

        petView = pivot.Find("View").GetComponent<UIScrollView>();
        petGrid = pivot.Find("View/Grid").GetComponent<UIGrid>();
        petCenterOnChild = pivot.Find("View/Grid").GetComponent<UICenterOnChild>();
        petCenterOnChild.enabled = false;
        petTemp = pivot.Find("View/Temp").gameObject;
        petTemp.SetActive(false);

        petCurrPropertyText1 = pivot.Find("1/Text1").GetComponent<UILabel>();
        petCurrPropertyText2 = pivot.Find("1/Text2").GetComponent<UILabel>();
        petMainPropertyText1 = pivot.Find("2/Text1").GetComponent<UILabel>();
        petMainPropertyText2 = pivot.Find("2/Text2").GetComponent<UILabel>();
        petDesc = pivot.Find("3/Desc").GetComponent<UILabel>();

        petTexture = pivot.Find("ModelTexture").GetComponent<UITexture>();
        InitItems();
        ShowRender();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnReturnClick;
        UIEventListener.Get(btnUpgrade).onClick = OnUpgradeClick;
        UIEventListener.Get(btnBattle).onClick = OnBattleClick;
        UIEventListener.Get(btnUnload).onClick = OnUnloadClick;
        UIEventListener.Get(btnAutoToAddFood).onClick = OnAutoToAddFoodClick;
        UIEventListener.Get(petTexture.gameObject).onDrag = OnDragModel;
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_PET_UPGRADE, OnRecvUpgrade);
        GTEventCenter.AddHandler(GTEventID.TYPE_PET_BATTLE, OnRecvBattlePet);
        GTEventCenter.AddHandler(GTEventID.TYPE_PET_UNLOAD, OnRecvUnLoadPet);

        GTEventCenter.AddHandler(GTEventID.TYPE_PET_UPDATE_VIEW, OnRecvUpdatePetView);
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_PET_UPGRADE, OnRecvUpgrade);
        GTEventCenter.DelHandler(GTEventID.TYPE_PET_BATTLE, OnRecvBattlePet);
        GTEventCenter.DelHandler(GTEventID.TYPE_PET_UNLOAD, OnRecvUnLoadPet);

        GTEventCenter.DelHandler(GTEventID.TYPE_PET_UPDATE_VIEW, OnRecvUpdatePetView);
    }

    protected override void OnEnable()
    {
        ShowView();
        ShowListView();
    }

    protected override void OnClose()
    {
        mCurPetGuid = 0;
        mFoods.Clear();
        mItems.Clear();
        mTemps.Clear();
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

    private void OnDragModel(GameObject go, Vector2 delta)
    {
        if (mAvatar == null && mAvatar.GetRootObj() != null)
        {
            return;
        }
        ESpin.Get(mAvatar.GetRootObj()).OnSpin(delta, 2);
    }

    private void OnUnloadClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTNetworkSend.Instance.TryUnsetPet(mCurPetGuid);
    }

    private void OnBattleClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTNetworkSend.Instance.TrySetupPet(mCurPetGuid);
    }

    private void OnAutoToAddFoodClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        if (mItems.Count > 0)
        {
            return;
        }
        MLPet.Instance.GetItemListToOneKeyUpgrade(ref mItems);
        if (mItems.Count == 0)
        {
            GTItemHelper.ShowTip("缺少食物");
            return;
        }
        ShowView();
    }

    private void OnUpgradeClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        if (mItems.Count == 0)
        {
            GTItemHelper.ShowTip("请添加食物");
            return;
        }
    }

    private void OnReturnClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void InitItems()
    {
        int group = GTWindowManager.Instance.GetToggleGroupId();
        int num = 0;
        foreach (var item in GTPetData.Dict.Values)
        {
            GameObject go = NGUITools.AddChild(petGrid.gameObject, petTemp);
            petGrid.AddChild(go.transform);
            go.SetActive(true);

            ulong guid = item.Guid;
            go.name = guid.ToString();

            TempItem tab = new TempItem();
            tab.guid = guid;
            tab.btn = go;
            tab.toggle= go.GetComponent<UIToggle>();
            tab.icon = go.transform.Find("Icon").GetComponent<UISprite>();
            tab.name = go.transform.Find("Name").GetComponent<UILabel>();
            tab.dress = go.transform.Find("Dress").gameObject;
            tab.quality = go.transform.Find("Quality").GetComponent<UISprite>();
            mTemps.Add(tab);

            num++;
            if (num == 1)
            {
                tab.toggle.value = true;
                mCurPetGuid = item.Guid;
            }

            int index = num;

            UIEventListener.Get(go).onClick = delegate (GameObject obj)
            {
                GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
                if (index > 2 && index < GTPetData.GetCount() - 2)
                {
                    petCenterOnChild.enabled = true;
                    petCenterOnChild.CenterOn(go.transform);
                    petCenterOnChild.onCenter = delegate (GameObject centerObj)
                    {
                        petCenterOnChild.enabled = false;
                    };
                }
                Switch(guid);
            };

        }
        petGrid.repositionNow = true;
    }

    private void Switch(ulong guid)
    {
        if (mCurPetGuid == guid)
        {
            return;
        }
        mCurPetGuid = guid;
        ShowRender();
        ShowView();
    }

    private void OnRecvUpgrade()
    {
        mItems.Clear();
        ShowView();
    }

    private void OnRecvUnLoadPet()
    {
        ShowView();
        ShowListView();
    }

    private void OnRecvUpdatePetView()
    {
        ShowView();
        ShowListView();
    }

    private void OnRecvBattlePet()
    {
        ShowView();
        ShowListView();
    }

    private void ShowRender()
    {
        PetItem item = GTPetData.GetItem(mCurPetGuid);
        if (item == null)
        {
            return;
        }

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

        DPet dbPet = ReadCfgPet.GetDataById(item.PetID);
        if (dbPet == null)
        {
            return;
        }
        
        mRender = ERender.AddRender(petTexture);
        DActor dbActor = ReadCfgActor.GetDataById(dbPet.ActorId);
    
        mAvatar = GTWorld.Instance.AddAvatar(dbActor.Model);
        mAvatar.PlayAnim("idle", null);
        GameObject model = mRender.AttachModel(mAvatar.GetRootObj());
        model.transform.localPosition = new Vector3(dbPet.X, dbPet.Y, dbPet.Z);
        model.transform.localEulerAngles = new Vector3(0, 120, 0);
        model.transform.localScale = Vector3.one * dbPet.Scale;
    }

    private void ShowListView()
    {
        for (int i = 0; i < mTemps.Count; i++)
        {
            TempItem tab = mTemps[i];
            PetItem item = GTPetData.GetItem(tab.guid);
            DPet dbPet = ReadCfgPet.GetDataById(item.PetID);
            DActor dbActor = ReadCfgActor.GetDataById(dbPet.ActorId);


            GTItemHelper.ShowQualityText(tab.name, dbPet.Name, dbActor.Quality);
            GTItemHelper.ShowActorQuality(tab.quality, dbActor.Id);
            tab.icon.spriteName = dbActor.Icon;
            tab.dress.SetActive(tab.guid == GTPetData.m_setupPet);
        }
    }

    private void ShowView()
    {
        PetItem data = GTPetData.GetItem(mCurPetGuid);
        if (data == null)
        {
            return;
        }

        DPet dbPet = ReadCfgPet.GetDataById(data.PetID);
        DActor dbActor = ReadCfgActor.GetDataById(dbPet.ActorId);

        DPetLevel levelDB = ReadCfgPetLevel.GetDataById(dbActor.Quality * 1000 + data.RefineLevel);
        GTItemHelper.ShowProgressSlider(expSlider, data.RefineLevel, levelDB.Exp);
        GTItemHelper.ShowProgressText(expNum, data.RefineExp, levelDB.Exp);
        for (int i = 0; i < mFoods.Count; i++)
        {
            FoodItem tab = mFoods[i];
            if (i < mItems.Count)
            {
                XItem itemData = mItems[i];
                GTItemHelper.ShowItemTexture(tab.itemTexture, itemData.Id);
                GTItemHelper.ShowItemQuality(tab.itemQuality, itemData.Id);
            }
            else
            {
                tab.itemQuality.gameObject.SetActive(false);
                tab.itemTexture.gameObject.SetActive(false);
            }
        }
        string str = GTTools.Format("{0}    +{1}", dbActor.Name, data.RefineLevel);
        GTItemHelper.ShowQualityText(petName, str, dbActor.Quality);
        ShowPropertyView(mCurPetGuid, data.RefineLevel);
        btnBattle.SetActive(GTPetData.m_setupPet != mCurPetGuid);
        btnUnload.SetActive(GTPetData.m_setupPet == mCurPetGuid);
    }

    private void ShowPropertyView(ulong guid, int level)
    {
        PetItem data = GTPetData.GetItem(mCurPetGuid);
        if (data == null)
        {
            return;
        }
        DPet dbPet = ReadCfgPet.GetDataById(data.PetID);
        DActor dbActor = ReadCfgActor.GetDataById(dbPet.ActorId);
        petCurrPropertyText1.text = string.Empty;
        petCurrPropertyText2.text = string.Empty;
        petMainPropertyText1.text = string.Empty;
        petMainPropertyText2.text = string.Empty;
    }

}
