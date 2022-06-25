using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UIRoleEquip : GTWindow
{
    private GameObject                      btnFetter;
    private GameObject                      btnStrong;
    private UIGrid                          propertyGrid;
    private GameObject                      propertyTemplate;
    private UITexture                       modelTexture;
    private List<ItemEquip>                 mEquipCells    = new List<ItemEquip>();
    private Dictionary<int, ItemProperty>   mPropertyItems = new Dictionary<int, ItemProperty>();
    private ERender                         mRender;
    private ActorAvator mAvatar;

    class ItemEquip
    {
        public UITexture  itemTexture;
        public UISprite   itemQuality;
        public GameObject itemBtn;

        public void Show(bool hasDress)
        {
            itemTexture.gameObject.SetActive(hasDress);
            itemQuality.gameObject.SetActive(hasDress);
        }
    }

    class ItemProperty
    {
        public UILabel propertyName;
        public UILabel propertyNum;
    }

    public UIRoleEquip()
    {
        MaskType = EWindowMaskType.None;
        Type = EWindowType.Window;
        ShowMode = EWindowShowMode.SaveTarget;
        Path = "Bag/UIRoleEquip";
        Resident = false;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        btnFetter = pivot.Find("Buttons/Btn_Fetter").gameObject;
        btnStrong = pivot.Find("Buttons/Btn_Strong").gameObject;
        propertyGrid = pivot.Find("Propertys/Grid").GetComponent<UIGrid>();
        propertyTemplate = pivot.Find("Propertys/Template").gameObject;
        modelTexture = pivot.Find("ModelTexture").GetComponent<UITexture>();
        for (int i = 1; i <= 8; i++)
        {
            ItemEquip item = new ItemEquip();
            item.itemBtn = pivot.Find("Equips/" + i).gameObject;
            item.itemBtn.name = "0";
            item.itemTexture = item.itemBtn.transform.Find("Texture").GetComponent<UITexture>();
            item.itemQuality = item.itemBtn.transform.Find("Quality").GetComponent<UISprite>();
            mEquipCells.Add(item);
        }
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnFetter).onClick = OnFetterClick;
        UIEventListener.Get(btnStrong).onClick = OnStrongClick;
        for (int i = 0; i < mEquipCells.Count; i++)
        {
            int index = i;
            UIEventListener.Get(mEquipCells[index].itemBtn).onClick = OnEquipCellClick;
        }
        UIEventListener.Get(modelTexture.gameObject).onDrag = OnHeroTextureDrag;
    }

    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_CHANGE_FIGHTVALUE,      OnRecvChangeFightValue);
        GTEventCenter.AddHandler(GTEventID.TYPE_EQUIP_UPDATE_VIEW,      OnRecvUpdateEquipView);
        
    }

    protected override void OnEnable()
    {
        ShowModelView();
        ShowEquipsView();
        ShowPropertys();
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_CHANGE_FIGHTVALUE,      OnRecvChangeFightValue);
        GTEventCenter.DelHandler(GTEventID.TYPE_EQUIP_UPDATE_VIEW,      OnRecvUpdateEquipView);
    }

    protected override void OnClose()
    {
        mEquipCells.Clear();
        mPropertyItems.Clear();
       
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

    private void OnHeroTextureDrag(GameObject go, Vector2 delta)
    {
        if (mAvatar == null && mAvatar.GetRootObj() != null)
            return;
        ESpin.Get(mAvatar.GetRootObj()).OnSpin(delta, 2);
    }

    private void OnEquipCellClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        ulong guid = go.name.ToUInt64();
        if (guid <= 0)
        {
            return;
        }
        GTWindowManager.Instance.OpenWindow(EWindowID.UIEquipInfo);
        UIEquipInfo w1 = (UIEquipInfo)GTWindowManager.Instance.GetWindow(EWindowID.UIEquipInfo);
        w1.ShowViewByGuid(guid, 0);
    }

    private void OnStrongClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
    }

    private void OnFetterClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
    }

    private void ShowModelView()
    {
        if (mAvatar == null)
        {
            XCharacter role = GTData.Main;
            DActor db = ReadCfgActor.GetDataById(role.Id);
            mRender = ERender.AddRender(modelTexture);
            mAvatar = GTWorld.Instance.AddAvatar(db.Model);
        }


        if (mAvatar == null)
        {
            return;
        }

        int[] equipArray = new int[8];

        foreach (var equip in GTEquipData.Dict.Values)
        {
            if (equip.IsUsing <=0)
            {
                continue;
            }

            DEquip equipDB1 = ReadCfgEquip.GetDataById(equip.EquipID);
            if (equipDB1 == null)
            {
                return;
            }

            equipArray[equipDB1.Pos - 1] = equipDB1.Id;
            
        }

        for (int i = 0; i < 8; i++)
        {
            mAvatar.ChangeAvatar(i + 1, equipArray[i]);
        }

        mAvatar.PlayAnim("idle", null);
        GameObject model = mRender.AttachModel(mAvatar.GetRootObj());
        model.transform.localPosition = new Vector3(0, -0.8f, 3.5f);
        model.transform.localEulerAngles = new Vector3(0, 180, 0);
    }

    private void ShowPropertys()
    {
        propertyTemplate.SetActive(false);
        XCharacter cc = GTData.Main;
        List<int> attrs = cc.CurAttrs;
        for (int i = 0; i < attrs.Count; i++)
        {
            if (i <= 0)
            {
                continue;
            }
            int value = attrs[i];
            ItemProperty item = null;
            if(!mPropertyItems.TryGetValue(i, out item))
            {
                item = new ItemProperty();
                GameObject go     = NGUITools.AddChild(propertyGrid.gameObject, propertyTemplate);
                item.propertyNum  = go.transform.Find("Num").GetComponent<UILabel>();
                item.propertyName = go.transform.Find("Name").GetComponent<UILabel>();
                go.SetActive(true);
                mPropertyItems.Add(i, item);
            }
            DProperty db = ReadCfgProperty.GetDataById(i);
            item.propertyName.text = db.Name;
            item.propertyNum.text = db.IsPercent == false ? value.ToString() : (value / 100f).ToPercent();
        }
    }

    private void ShowEquipsView()
    {
        for (int i = 0; i < 8; i++)
        {
            ItemEquip cell = mEquipCells[i];
            cell.Show(false);
        }

        foreach (var equip in GTEquipData.Dict.Values)
        {
            if (equip.IsUsing <= 0)
            {
                continue;
            }

            DEquip equipDB1 = ReadCfgEquip.GetDataById(equip.EquipID);
            if (equipDB1 == null)
            {
                return;
            }

            ItemEquip cell = mEquipCells[equipDB1.Pos - 1];
            cell.Show(true);
            cell.itemBtn.name = equip.Guid.ToString();
            GTItemHelper.ShowItemTexture(cell.itemTexture, equipDB1.Id);
            GTItemHelper.ShowItemQuality(cell.itemQuality, equipDB1.Id);
        }
    }

    private void OnRecvChangeFightValue()
    {
        ShowPropertys();
    }

    private void OnRecvUpdateEquipView()
    {
        ShowEquipsView();
        ShowModelView();
    }
}
