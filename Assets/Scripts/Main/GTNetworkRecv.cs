using UnityEngine;
using System.Collections;
using Protocol;
using ProtoBuf;
using System.Collections.Generic;
using MAP;
using System;
using ACT;

public class GTNetworkRecv : GTSingleton<GTNetworkRecv>
{
    public void AddListener()
    {
        NetworkManager.AddListener(MessageID.MSG_ACCOUNT_REG_ACK,       OnAck_AccountRegister);
        NetworkManager.AddListener(MessageID.MSG_ACCOUNT_LOGIN_ACK,     OnAck_AccountLogin);
        NetworkManager.AddListener(MessageID.MSG_SELECT_SERVER_ACK,     OnAck_SelectServer);
        NetworkManager.AddListener(MessageID.MSG_ROLE_LIST_ACK,         OnAck_GetRoleList);
        NetworkManager.AddListener(MessageID.MSG_SERVER_LIST_ACK,       OnAck_GetServerList);
        NetworkManager.AddListener(MessageID.MSG_ROLE_CREATE_ACK,       OnAck_CreateRole);
        NetworkManager.AddListener(MessageID.MSG_ROLE_LOGIN_ACK,        OnAck_RoleLoginAck);
        NetworkManager.AddListener(MessageID.MSG_NOTIFY_INTO_SCENE,     OnAck_NotifyIntoScene);
        NetworkManager.AddListener(MessageID.MSG_ENTER_SCENE_ACK,       OnAck_EnterScene);
        NetworkManager.AddListener(MessageID.MSG_OBJECT_NEW_NTF,        OnNtf_ObjectNew);
        NetworkManager.AddListener(MessageID.MSG_OBJECT_REMOVE_NTF,     OnNtf_ObjectRemove);
        NetworkManager.AddListener(MessageID.MSG_OBJECT_CHANGE_NTF,     OnNtf_ObjectChange);
        NetworkManager.AddListener(MessageID.MSG_BULLET_NEW_NTF,        OnNtf_NewBullet);
        NetworkManager.AddListener(MessageID.MSG_BAG_CHANGE_NTY,        OnNtf_BagChange);
        NetworkManager.AddListener(MessageID.MSG_EQUIP_CHANGE_NTY,      OnNtf_EquipChange);
        NetworkManager.AddListener(MessageID.MSG_PET_CHANGE_NTY,        OnNtf_PetChange);
        NetworkManager.AddListener(MessageID.MSG_PARTNER_CHANGE_NTY,    OnNtf_PartnerChange);
        NetworkManager.AddListener(MessageID.MSG_MOUNT_CHANGE_NTY,      OnNtf_MountChange);
        NetworkManager.AddListener(MessageID.MSG_GEM_CHANGE_NTY,        OnNtf_GemChange);
        NetworkManager.AddListener(MessageID.MSG_SETUP_EQUIP_ACK,       OnAck_SetupEquip);
        NetworkManager.AddListener(MessageID.MSG_UNSET_EQUIP_ACK,     	OnAck_UnsetEquip);
        NetworkManager.AddListener(MessageID.MSG_SKILL_CAST_NTF,        OnNtf_ActorSkillCast); //技能释放成功
		NetworkManager.AddListener(MessageID.MSG_SKILL_CAST_ACK,        OnAck_ActorSkillCast); //技能释放失败
        NetworkManager.AddListener(MessageID.MSG_STORE_BUY_ACK,         OnAck_BuyStore);
        NetworkManager.AddListener(MessageID.MSG_SETUP_GEM_ACK,         OnAck_SetupGem);
        NetworkManager.AddListener(MessageID.MSG_UNSET_GEM_ACK,       	OnAck_UnsetGem);
		NetworkManager.AddListener(MessageID.MSG_OBJECT_DIE_NOTIFY,     OnNtf_ObjectDead);
        NetworkManager.AddListener(MessageID.MSG_USE_ITEM_ACK,          OnAck_UseItem);
        NetworkManager.AddListener(MessageID.MSG_ACTOR_HITEFFECT_NTF,   OnNtf_ActorHitEffect);
        NetworkManager.AddListener(MessageID.MSG_RANDOM_NAME_ACK,       OnAck_RandomName);
        NetworkManager.AddListener(MessageID.MSG_MAIN_COPY_ACK,         OnAck_MainCopy);
        NetworkManager.AddListener(MessageID.MSG_MAINCOPY_RESULT_NTY,   OnAck_MainCopyResult);

        GLTeamCopy.Instance.AddListener();
    }

    public void DelListener()
    {
    
    }

    private void OnAck_AccountRegister(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        AccountRegAck ack = Serializer.Deserialize<AccountRegAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_ACCOUNT_REGISTER);
    }

    private void OnAck_AccountLogin(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        AccountLoginAck ack = Serializer.Deserialize<AccountLoginAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        ClientServerNode newNode = new ClientServerNode();
        newNode.SvrID = ack.LastSvrID;
        newNode.SvrName = ack.LastSvrName;
        MLLogin.Instance.SetCurrServer(newNode);
        MLLogin.Instance.LastAccountID = ack.AccountID;
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_ACCOUNT_LOGIN);
    }

    private void OnAck_GetServerList(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        ClientServerListAck ack = Serializer.Deserialize<ClientServerListAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        MLLogin.Instance.SetServerList(ack.SvrNode);
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_GETSERVERLIST);
    }

    private void OnAck_SelectServer(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        SelectServerAck ack = Serializer.Deserialize<SelectServerAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_SELECTSERVER);
        NetworkManager.Instance.Close();
        NetworkManager.Instance.ConnectServer(ack.ServerAddr, ack.ServerPort, () =>
        {
            RoleListReq req = new RoleListReq();
            req.AccountID = MLLogin.Instance.LastAccountID;
            req.LoginCode = ack.LoginCode;
            NetworkManager.Instance.Send(MessageID.MSG_ROLE_LIST_REQ, req);
        });
    }

    private void OnAck_GetRoleList(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        RoleListAck ack = Serializer.Deserialize<RoleListAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        for (int i = 0; i < ack.RoleList.Count; i++)
        {
            RoleItem item = ack.RoleList[i];
            XCharacter data = DataDBSCharacter.GetDataById(item.Carrer);
            if (data == null)
            {
                data = new XCharacter();
            }
            data.Carrer = item.Carrer;
            data.Id = ReadCfgRole.GetDataById(item.Carrer).ActorID;
            data.GUID = item.RoleID;
            data.Level = item.Level;
            data.Name = item.Name;
            data.ActorType = EObjectType.OT_PLAYER;
            data.Camp = (int)0;
            DataDBSCharacter.Update(data.Carrer, data);
        }
        GTLauncher.Instance.LoadScene(GTCopyKey.SCENE_Role);
    }

    private void OnAck_CreateRole(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        RoleCreateAck ack = Serializer.Deserialize<RoleCreateAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        XCharacter data = new XCharacter();
        data.Carrer = ack.Carrer;
        data.Id = ReadCfgRole.GetDataById(ack.Carrer).ActorID;
        data.GUID = ack.RoleID;
        data.Name = ack.Name;
        data.Level = 1;
        data.ActorType = EObjectType.OT_PLAYER;
        data.Camp = (int)ack.RoleID;
        DataDBSCharacter.Insert(data.Carrer, data);
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_ROLECRATE);
    }

    private void OnAck_RoleLoginAck(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        RoleLoginAck ack = Serializer.Deserialize<RoleLoginAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        GTData.Main.Carrer = ack.Carrer;
        GTData.Main.GUID   = ack.RoleID;
        for (int i = 0; i < ack.ActionList.Count; i++)
        {
			ActionItem item = ack.ActionList[i];
            int id = i + 101;
			GTActionData.AddItem(id, item);
        }

        for (int i = 0; i < ack.BagItemList.Count; i++)
        {
            BagItem item = ack.BagItemList[i];
            GTBagData.AddItem(item.Guid, item);
        }


        for (int i = 0; i < ack.EquipList.Count; i++)
        {
            EquipItem item = ack.EquipList[i];
            GTEquipData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < ack.GemList.Count; i++)
        {
            GemItem item = ack.GemList[i];
            GTGemData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < ack.PetList.Count; i++)
        {
            PetItem item = ack.PetList[i];
            GTPetData.AddItem(item.Guid, item);

            if (item.IsUsing == 1)
            {
                GTPetData.m_setupPet = item.Guid;
            }
        }

        for (int i = 0; i < ack.PartnerList.Count; i++)
        {
            PartnerItem item = ack.PartnerList[i];
            GTPartnerData.AddItem(item.Guid, item);

			if (item.SetPos > 0) 
			{
				GTPartnerData.m_setupPartner [item.SetPos - 1] = item.Guid;
			}
        }

        for (int i = 0; i < ack.MountList.Count; i++)
        {
            MountItem item = ack.MountList[i];
            GTMountData.AddItem(item.Guid, item);

            if(item.IsUsing == 1)
            {
                GTMountData.m_setupMount = item.Guid;
            }
        }

        for (int i = 0; i < ack.MailList.Count; i++)
        {
            MailItem item = ack.MailList[i];
            GTMailData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < ack.SkillList.Count; i++)
        {
            SkillItem item = ack.SkillList[i];
            GTSkillData.AddItem(item.SkillID, item);
        }

        for (int i = 0; i < ack.CopyList.Count; i++)
        {
            CopyItem item = ack.CopyList[i];
            GTCopyData.AddItem((int)item.CopyID, item);
        }

        for (int i = 0; i < ack.ChapterList.Count; i++)
        {
            ChapterItem item = ack.ChapterList[i];
            GTChapterData.AddItem((int)item.ChapterID, item);
        }

        NetworkManager.Instance.SetMainGuid(GTData.Main.GUID);

        GTWorld.Instance.EnterGuide();

        GTWorld.Instance.Guide.UseGuide = GTLauncher.Instance.UseGuide;

        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_ROLELOGIN);
    }

    private void OnAck_EnterScene(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        EnterSceneAck ack = Serializer.Deserialize<EnterSceneAck>(ms);
        if (GTItemHelper.ShowNetworkError(ack.RetCode) == false)
        {
            return;
        }
        XCharacter data = DataDBSCharacter.GetDataById(GTData.Main.Carrer);
        if (data == null)
        {
            GTItemHelper.ShowTip("职业错误");
            return;
        }
        if (data.CurAttrs.Count == 0)
        {
			GTTools.InitListWithDefalut(ref data.CurAttrs, (int)EAttrID.EA_ATTR_NUM);
        }
        GTData.Main                                 = data;
        GTData.Main.GUID                            = ack.RoleID;
        GTData.Main.Id                              = ack.ActorID;
        GTData.Main.PosX                            = ack.X;
        GTData.Main.PosY                            = ack.Y;
        GTData.Main.PosZ                            = ack.Z;
        GTData.Main.Face                            = ack.Ft;
        GTData.Main.Camp                            = ack.Camp;
        GTData.Main.CurAttrs[(int)EAttrID.EA_HP_MAX]   = ack.HpMax;
        GTData.Main.CurAttrs[(int)EAttrID.EA_MP_MAX]   = ack.MpMax;
        GTData.Main.CurAttrs[(int)EAttrID.EA_HP]       = ack.Hp;
        GTData.Main.CurAttrs[(int)EAttrID.EA_MP]       = ack.Mp;
        GTData.Main.CurAttrs[(int)EAttrID.EA_SPEED]    = ack.Speed;
        GTData.Main.Mount = ack.MountID;


        for(int i = 0; i < 8; i++)
        {
            GTData.Main.CurEquips.Add(ack.Equips[i]);
        }

        for (int i = 0; i < ack.Skills.Count; i++)
        {
            SkillItem skillItem = ack.Skills[i];
            GTData.Main.CurSkills.Add(skillItem);
        }

        GTData.CopyGUID = ack.CopyGuid;

        GTWorld.Instance.EnterWorld(ack.CopyID);
    }

    private void OnAck_NotifyIntoScene(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        NotifyIntoScene ack = Serializer.Deserialize<NotifyIntoScene>(ms);
        GTData.CopyGUID = 0;
        GTLauncher.Instance.LoadScene(ack.CopyID, () =>
        {
            GTNetworkSend.Instance.TryEnterScene(ack.RoleID, ack.CopyID, ack.CopyGuid, ack.ServerID);
        });
    }
    private void OnAck_UseItem(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        UseItemAck ack = Serializer.Deserialize<UseItemAck>(ms);
    }
    private void OnAck_RandomName(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        MsgGetRandomNameAck ack = Serializer.Deserialize<MsgGetRandomNameAck>(ms);
        GTEventCenter.FireEvent(GTEventID.TYPE_LOGIN_RANDOM_NAME, ack.Name);
    }
    private void OnAck_MainCopyResult(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        MainCopyResultNty ack = Serializer.Deserialize<MainCopyResultNty>(ms);

        MainResultData.CopyID = ack.CopyID;
        MainResultData.StarNum = ack.StarNum;
        MainResultData.LastTime = ack.LastTime;

        for(int i = 0; i < ack.ItemList.Count; i++)
        {
            MainResultData.ItemList.Add(new EItem(ack.ItemList[i].ItemID, ack.ItemList[i].ItemNum));
        }

        GTWindowManager.Instance.OpenWindow(EWindowID.UIMainResult);
        UIMainResult window = (UIMainResult)GTWindowManager.Instance.GetWindow(EWindowID.UIMainResult);
        window.ShowView();

        UIHome wndHome = (UIHome)GTWindowManager.Instance.GetWindow(EWindowID.UIHome);
        wndHome.Hide();
    }

    private void OnAck_ComposeChip(MessageRecvData obj)
    {
       
    }

    private void OnAck_UnloadGem(MessageRecvData obj)
    {
    }

    private void OnAck_SetupGem(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
		SetupGemAck ack = Serializer.Deserialize<SetupGemAck>(ms);
    }

    private void OnAck_UnsetGem(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        UnsetGemAck ack = Serializer.Deserialize<UnsetGemAck>(ms);;
    }


	private void OnNtf_ObjectDead(MessageRecvData obj)
	{
		System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
		ObjectDieNotify ntf = Serializer.Deserialize<ObjectDieNotify>(ms);;

		Actor cc = GTWorld.Instance.GetActor(ntf.ObjectGuid);
        if(cc == null)
        {
            Debug.LogError("Receive Message ObjectDead : " + ntf.ObjectGuid.ToString());
            return;
        }

		cc.DoDead(0);
	}


 	private void OnAck_UnsetEquip(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        UnsetEquipAck ack = Serializer.Deserialize<UnsetEquipAck>(ms);
        GTEventCenter.FireEvent(GTEventID.TYPE_BAG_UNDRESS_EQUIP, 0, 0);
    }

    private void OnAck_SetupEquip(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        SetupEquipAck ack = Serializer.Deserialize<SetupEquipAck>(ms);
        GTEventCenter.FireEvent(GTEventID.TYPE_BAG_DRESS_EQUIP, 0, 0);
    }


    private void OnAck_UpStarEquip(MessageRecvData obj)
    {

       
    }

    private void OnAck_AdvanceEquip(MessageRecvData obj)
    {

    }

    private void OnAck_StrengthEquip(MessageRecvData obj)
    {

    }

    private void OnAck_StrengthGem(MessageRecvData obj)
    {

    }

    private void OnNtf_ObjectNew(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        ObjectNewNty nty = Serializer.Deserialize<ObjectNewNty>(ms);
        for (int i = 0; i < nty.NewList.Count; i++)
        {
            NewItem item = nty.NewList[i];
            XCharacter data = new XCharacter();
            data.Id = item.ActorID;
            data.GUID = item.ObjectGuid;
            data.Level = item.Level;
            data.Name = item.Name;
            data.Camp = item.Camp;
            data.PosX = item.X;
            data.PosY = item.Y;
            data.PosZ = item.Z;
            data.Face = item.Ft;
            GTTools.InitListWithDefalut(ref data.CurAttrs, (int)EAttrID.EA_ATTR_NUM);
            data.CurAttrs[(int)EAttrID.EA_HP_MAX]    = item.HpMax;
            data.CurAttrs[(int)EAttrID.EA_MP_MAX]    = item.MpMax;
            data.CurAttrs[(int)EAttrID.EA_HP]        = item.Hp;
            data.CurAttrs[(int)EAttrID.EA_MP]        = item.Mp;
            data.CurAttrs[(int)EAttrID.EA_SPEED]     = item.Speed ;
            data.ControlID                        = item.ControlerID;
            data.ActionID                         = (EActionType)item.ActionID;
            data.HostID                           = item.HostGuid;
            data.ActorStatus = item.ObjectStatus;
            data.Mount = item.MountID;

            for (int j = 0; j < item.Skills.Count; j++)
            {
                data.CurSkills.Add(item.Skills[j]);
            }

            switch ((EObjectType)item.ObjType)
            {
                case EObjectType.OT_PLAYER:
                    {
                        data.ActorType = EObjectType.OT_PLAYER;
                        for (int j = 0; j < 8; j++)
                        {
                            data.CurEquips.Add(item.Equips[j]);
                        }

                        GTWorld.Instance.AddPlayer(data, true);
                    }
                    break;

                case EObjectType.OT_MONSTER:
                    {
                        data.ActorType = EObjectType.OT_MONSTER;
                        GTWorld.Instance.AddMonster(data);
                    }
                    break;
                case EObjectType.OT_PET:
                    {
                        data.ActorType = EObjectType.OT_PET;
                        GTWorld.Instance.AddMonster(data);
                    }
                    break;
                case EObjectType.OT_PARTNER:
                    {
                        data.ActorType = EObjectType.OT_PARTNER;
                        GTWorld.Instance.AddMonster(data);
                    }
                    break;
                case EObjectType.OT_SUMMON:
                    {
                        data.ActorType = EObjectType.OT_SUMMON;
                        GTWorld.Instance.AddMonster(data);
                    }
                    break;
            }


            /*        此地添加特效     */
            if (GTWorld.Instance.WorldMapID == 10001)
            {
                GameObject effect = GTWorld.Instance.Effect.CreateEffectByID(65002);
                effect.transform.parent = GTWorld.Instance.transform;
                effect.transform.localPosition = new Vector3(data.PosX, data.PosY, data.PosZ);
                effect.transform.localRotation = Quaternion.identity;
                effect.transform.localScale = Vector3.one;
            }
        }
    }

    private void OnNtf_ObjectRemove(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        ObjectRemoveNty Nty = Serializer.Deserialize<ObjectRemoveNty>(ms);
        for (int i = 0; i < Nty.RemoveList.Count; i++)
        {
            GTWorld.Instance.DelActor(Nty.RemoveList[i]);
        }
    }

    private void OnNtf_ObjectChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        ObjectActionNty nty = Serializer.Deserialize<ObjectActionNty>(ms);
        for (int i = 0; i < nty.ActionList.Count; i++)
        {
            ActionNtyItem item = nty.ActionList[i];
            Actor cc = GTWorld.Instance.GetActor(item.ObjectGuid);
            if (cc == null)
            {
                continue;
            }

            cc.Attr.SetAttr(EAttrID.EA_HP, item.Hp);
            cc.Attr.SetAttr(EAttrID.EA_MP, item.Mp);

            if (item.HpMax != 0)
            {
                cc.Attr.SetAttr(EAttrID.EA_HP_MAX, item.HpMax);
            }

            if (item.MpMax != 0)
            {
                cc.Attr.SetAttr(EAttrID.EA_MP_MAX, item.MpMax);
            }
            cc.ActorStatus = item.ObjectStatus;

            if (cc.GUID != GTData.Main.GUID)
            {
                Vector3 toTarget = new Vector3(item.HostX, item.HostY, item.HostZ);
                Vector3 toFace = new Vector3(0, item.HostFt, 0);
                cc.Get<ActorMoveSync>().SyncMove(toTarget, toFace, (EActionType)item.ActionID);
            }

            if(item.ActorID != 0)
            {
                cc.ID = item.ActorID;
            }

            if (item.Level > 0)
            {
                cc.Level = item.Level;
            }

            if(item.Equips.Count > 0)
            {
                for (int j = 0; j < 8; j++)
                {
                    cc.Get<ActorAvator>().ChangeAvatar(j + 1, item.Equips[j]);
                }
            }

            if(item.ControlerID != 0)
            {
                cc.ControlID = item.ControlerID;
            }

            if(item.MountID != 0)
            {
                cc.MountID = item.MountID;

                cc.UpdateMountStatus();

                GTEventCenter.FireEvent(GTEventID.TYPE_UPDATE_RIDING_STATUS);
            }


            GTWorld.Instance.HudBlood.AddHudBlood(cc);
        }
    }

    private void OnNtf_BagChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        BagChangeNty nty = Serializer.Deserialize<BagChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            BagItem item = nty.ChangeList[i];

            GTBagData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTBagData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_BAG_UPDATE_VIEW);
    }

    private void OnNtf_EquipChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        EquipChangeNty nty = Serializer.Deserialize<EquipChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            EquipItem item = nty.ChangeList[i];

            GTEquipData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTEquipData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_EQUIP_UPDATE_VIEW);
    }

    private void OnNtf_GemChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        GemChangeNty nty = Serializer.Deserialize<GemChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            GemItem item = nty.ChangeList[i];

            GTGemData.AddItem(item.Guid, item);
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTGemData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_GEM_UPDATE_VIEW);
    }

    private void OnNtf_PetChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        PetChangeNty nty = Serializer.Deserialize<PetChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            PetItem item = nty.ChangeList[i];

            GTPetData.AddItem(item.Guid, item);

            if (item.IsUsing > 0)
            {
                GTPetData.m_setupPet = item.Guid;
            }
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTPetData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_PET_UPDATE_VIEW);
    }

    private void OnNtf_PartnerChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        PartnerChangeNty nty = Serializer.Deserialize<PartnerChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            PartnerItem item = nty.ChangeList[i];

            GTPartnerData.AddItem(item.Guid, item);

			if (item.SetPos > 0) {
				GTPartnerData.m_setupPartner [item.SetPos - 1] = item.Guid;
			}
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTPartnerData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_PARTNER_UPDATE_VIEW);
    }

    private void OnNtf_MountChange(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        MountChangeNty nty = Serializer.Deserialize<MountChangeNty>(ms);
        for (int i = 0; i < nty.ChangeList.Count; i++)
        {
            MountItem item = nty.ChangeList[i];

            GTMountData.AddItem(item.Guid, item);

            if (item.IsUsing > 0)
            {
                GTMountData.m_setupMount = item.Guid;
            }
        }

        for (int i = 0; i < nty.RemoveList.Count; i++)
        {
            GTMountData.RemoveItem(nty.RemoveList[i]);
        }

        GTEventCenter.FireEvent(GTEventID.TYPE_MOUNT_UPDATE_VIEW);
    }

    private void OnAck_SetMount(MessageRecvData obj)
    {

    }

    private void OnAck_AdvancePartner(MessageRecvData obj)
    {
        //System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        //AckAdvancePartner ack = Serializer.Deserialize<AckAdvancePartner>(ms);

        //XPartner partner = DataDBSPartner.GetDataById(ack.ID);
        //if (partner == null)
        //{
        //    partner = new XPartner();
        //    partner.Id = ack.ID;
        //    partner.Advance = 1;
        //}
        //else
        //{
        //    partner.Advance++;
        //}
        //DataDBSPartner.Update(ack.ID, partner);
        //GTEventCenter.FireEvent(GTEventID.TYPE_PARTNER_ADVANCE);
        //GTEventCenter.FireEvent(GTEventID.TYPE_CHANGE_FIGHTVALUE);
    }

    private void OnAck_UpgradePartner(MessageRecvData obj)
    {
        //System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        //AckUpgradePartner ack = Serializer.Deserialize<AckUpgradePartner>(ms);

        //XPartner partner = DataDBSPartner.GetDataById(ack.ID);
        //if (partner == null)
        //{
        //    partner = new XPartner();
        //    partner.Id = ack.ID;
        //    partner.Level = 1;
        //}
        //else
        //{
        //    partner.Level++;
        //}
        //DataDBSPartner.Update(ack.ID, partner);
        //GTEventCenter.FireEvent(GTEventID.TYPE_PET_UPGRADE);
        //GTEventCenter.FireEvent(GTEventID.TYPE_CHANGE_FIGHTVALUE);
    }

    private void OnAck_ChangePartner(MessageRecvData obj)
    {
//         System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
//         AckChangePartner ack = Serializer.Deserialize<AckChangePartner>(ms);
// 
//         XCharacter role = GTData.Main;
//         switch (ack.Pos)
//         {
//             case 1:
//                 role.Partner1 = ack.ID;
//                 break;
//             case 2:
//                 role.Partner2 = ack.ID;
//                 break;
//         }
//         DataDBSCharacter.Update(role.Carrer, role);
//         if (!DataDBSPartner.ContainsKey(ack.ID))
//         {
//             XPartner xp = new XPartner();
//             xp.Id = ack.ID;
//             xp.Level = 1;
//             xp.Star = 0;
//             xp.Wake = 0;
//             xp.Advance = 1;
//             xp.Exp = 0;
//             DataDBSPartner.Insert(ack.ID, xp);
//         }
// 
//         GTEventCenter.FireEvent(GTEventID.TYPE_PARTNER_CHANGE, ack.Pos, ack.ID);
//         GTEventCenter.FireEvent(GTEventID.TYPE_CHANGE_FIGHTVALUE);
    }

    private void OnAck_UpgradePet(MessageRecvData obj)
    {
        //System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        //AckUpgradePet ack = Serializer.Deserialize<AckUpgradePet>(ms);
        //int id = ack.ID;
        //List<XItem> items = ack.UseItems;

        GTEventCenter.FireEvent(GTEventID.TYPE_PET_UPGRADE);
        GTEventCenter.FireEvent(GTEventID.TYPE_CHANGE_FIGHTVALUE);
    }

    private void OnAck_UnloadPet(MessageRecvData obj)
    {
    }

    private void OnAck_MainCopy(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        MainCopyAck ack = Serializer.Deserialize<MainCopyAck>(ms);
        if (ack.RetCode != 0)
        {
            GTItemHelper.ShowNetworkError(ack.RetCode);
            return;
        }
    }

    private void OnAck_UpgradeRelics(MessageRecvData obj)
    {

    }

    private void OnAck_ChargeRelics(MessageRecvData obj)
    {
    }

    private void OnAck_UnloadRelics(MessageRecvData obj)
    {

    }

    private void OnAck_BattleRelics(MessageRecvData obj)
    {

    }

    private void OnAck_BuyStore(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        StoreBuyAck ack = Serializer.Deserialize<StoreBuyAck>(ms);
        GTEventCenter.FireEvent(GTEventID.TYPE_STORE_BUYSUCCESS);
    }

    private void OnNtf_ActorHitEffect(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
		HitEffectNtf ack = Serializer.Deserialize<HitEffectNtf>(ms);
        for (int i = 0; i < ack.ItemList.Count; i++)
        {
			HitEffectItem data = ack.ItemList[i];
            ulong targetGUID = data.TargetGuid;
            Actor actor = GTWorld.Instance.GetActor(targetGUID);
            if (actor == null)
            {
                continue;
            }
            actor.DoActorHitEffect(data);
        }

        
    }

    private void OnNtf_ActorSkillCast(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        SkillCastReq Req = Serializer.Deserialize<SkillCastReq>(ms);
        Actor cc = GTWorld.Instance.GetActor(Req.ObjectGuid);
        if (cc != null)
        {
            cc.DoSkill(Req.SkillID);
        }
    }

	private void OnAck_ActorSkillCast(MessageRecvData obj)
	{
		System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
		SkillCastAck Ack = Serializer.Deserialize<SkillCastAck>(ms);
		GTCommonHelper.ShowErrorCodeInfo (Ack.RetCode);
        Actor cc = GTWorld.Instance.GetActor(Ack.ObjectGuid);
        if (cc != null)
        {
            cc.DoIdle();
        }
    }

    private void OnNtf_NewBullet(MessageRecvData obj)
    {
        System.IO.MemoryStream ms = new System.IO.MemoryStream(obj.Data);
        BulletNewNtf ntf = Serializer.Deserialize<BulletNewNtf>(ms);

        for (int i = 0; i < ntf.ItemList.Count; i++)
        {
            BulletItem data = ntf.ItemList[i];

            GTWorld.Instance.AddFlyObject(data);
        }
    }
}
