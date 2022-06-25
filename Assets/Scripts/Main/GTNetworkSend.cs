using UnityEngine;
using System.Collections;
using System;
using ACT;
using Protocol;
using System.Collections.Generic;

public class GTNetworkSend : GTSingleton<GTNetworkSend>
{
	public void TryRegister(string username, string password)
	{
		AccountRegReq req = new AccountRegReq();
		req.AccountName = username;
		req.Password = password;
		MLLogin.Instance.LastUsername = username;
		MLLogin.Instance.LastPassword = password;
		req.Channel = 1;
		req.RegLog = new AccountLog();
		req.RegLog.Channel = 1;
		req.RegLog.Version = "1.0.1";
		req.RegLog.Uuid = SystemInfo.deviceUniqueIdentifier;
		req.RegLog.Idfa = SystemInfo.deviceUniqueIdentifier;
		req.RegLog.Imodel = SystemInfo.deviceModel;
		req.RegLog.Imei = "0";
		NetworkManager.Instance.Send(MessageID.MSG_ACCOUNT_REG_REQ, req);
	}

	public void TryLogin(string username, string password)
	{
		AccountLoginReq req = new AccountLoginReq();
		req.AccountName = username;
		req.Password = password;
		req.FromChannel = false;
		req.Channel = 1;
		req.LoginLog = new AccountLog();
		req.LoginLog.Channel = 1;
		req.LoginLog.Version = "1.0.1";
		req.LoginLog.Uuid = SystemInfo.deviceUniqueIdentifier;
		req.LoginLog.Idfa = SystemInfo.deviceUniqueIdentifier;
		req.LoginLog.Imodel = SystemInfo.deviceModel;
		req.LoginLog.Imei = "0";
		NetworkManager.Instance.Send(MessageID.MSG_ACCOUNT_LOGIN_REQ, req);
	}

	public void TryGetSvrList()
	{
		ClientServerListReq req = new ClientServerListReq();
		req.AccountID = MLLogin.Instance.LastAccountID;
		req.Channel = 2;
		req.Version = "1.8.0";
		NetworkManager.Instance.Send(MessageID.MSG_SERVER_LIST_REQ, req);
	}

	//确认当前服务器
	public void TrySelectServer(int ServerID)
	{
		SelectServerReq req = new SelectServerReq();
		req.ServerID = ServerID;
		req.AccountID = MLLogin.Instance.LastAccountID;
		NetworkManager.Instance.Send(MessageID.MSG_SELECT_SERVER_REQ, req);
	}

	//创建角色
	public void TryCreateRole(string name, int carrerID, ulong accountID)
	{
		if (string.IsNullOrEmpty(name))
		{
			GTItemHelper.ShowTip("名字不能为空");
			return;
		}
		RoleCreateReq req = new RoleCreateReq();
		req.Name = name;
		req.AccountID = accountID;
		req.Carrer = carrerID;
		NetworkManager.Instance.Send(MessageID.MSG_ROLE_CREATE_REQ, req);
	}

	//取角色列表
	public void TryGetRoleList()
	{
		RoleListReq req = new RoleListReq();
		req.AccountID = MLLogin.Instance.LastAccountID;
		req.LoginCode = 0x111;
		NetworkManager.Instance.Send(MessageID.MSG_ROLE_LIST_REQ, req);
	}

	//登录角色
	public void TryRoleLogin(UInt64 roleGUID)
	{
		RoleLoginReq req = new RoleLoginReq();
		req.AccountID = MLLogin.Instance.LastAccountID;
		req.RoleID = roleGUID;
		req.LoginCode = 0x111;
		NetworkManager.Instance.Send(MessageID.MSG_ROLE_LOGIN_REQ, req, roleGUID, 0);
	}

	//进入副本场景或者主城场景
	public void TryEnterScene(UInt64 roleID, Int32 copyID, Int32 copyGUID, Int32 ServerID)
	{
		EnterSceneReq req = new EnterSceneReq();
		req.RoleID   = roleID;
		req.CopyID   = copyID;
		req.ServerID = ServerID;
		req.CopyGuid = copyGUID;
		NetworkManager.Instance.Send(MessageID.MSG_ENTER_SCENE_REQ, req, (UInt32)ServerID, (UInt32)copyGUID);
	}

	public void TryBackToMainCity()
	{
		BackToCityReq req = new BackToCityReq();
		NetworkManager.Instance.Send<BackToCityReq>(MessageID.MSG_BACK_TO_CITY_REQ, req);
	}

	public void TryTestSomething (string cmd)
	{
		ChatMessageReq req = new ChatMessageReq();
		req.Content = "@@" + cmd;
		NetworkManager.Instance.Send<ChatMessageReq>(MessageID.MSG_CHAT_MESSAGE_REQ, req);
	}


	public void TrySetupMount(ulong mountGuid)
	{
		SetupMountReq req = new SetupMountReq();
		req.MountGuid = mountGuid;
		req.TargetPos = 0;
		NetworkManager.Instance.Send<SetupMountReq>(MessageID.MSG_SETUP_MOUNT_REQ, req);
	}

	public void TrySetupPartner(int pos, ulong guid)
	{
		SetupPartnerReq req = new SetupPartnerReq();
		req.PartnerGuid = guid;
		req.TargetPos = pos;
		NetworkManager.Instance.Send<SetupPartnerReq>(MessageID.MSG_SETUP_PARTNER_REQ, req);
	}


	public void TryGetRandomName(int sex)
	{
		MsgGetRandomNameReq req = new MsgGetRandomNameReq();
		req.Sex = sex;
		NetworkManager.Instance.Send<MsgGetRandomNameReq>(MessageID.MSG_RANDOM_NAME_REQ, req);
	}

	public void TryUpgradePartner(int id)
	{
		//ReqUpgradePartner req = new ReqUpgradePartner();
		//req.ID = id;
		//NetworkManager.Instance.Send<ReqUpgradePartner>(MessageID.MSG_REQ_UPGRADE_PARTNER, req);
	}

	public void TryAdvancePartner(int id)
	{
		//ReqAdvancePartner req = new ReqAdvancePartner();
		//req.ID = id;
		//NetworkManager.Instance.Send<ReqAdvancePartner>(MessageID.MSG_REQ_ADVANVE_PARTNER, req);
	}

	public void TryUpgradePet(ulong guid, List<XItem> items)
	{
		/*
		if (items == null || items == null)
		{
		    GTItemHelper.ShowTip("缺少食物");
		    return;
		}
		for (int i = 0; i < items.Count; i++)
		{
		    if (MLPet.Instance.IsFood(items[i].Id) == false)
		    {
		        GTItemHelper.ShowTip("加入的食品列表有误");
		        return;
		    }
		}

		PetItem data = GTPetData.GetItem(guid);

		DActor db = ReadCfgActor.GetDataById(data.PetID);

		if (data != null)
		{
		    int count = 0;
		    foreach (var current in ReadCfgPetLevel.Dict)
		    {
		        if (current.Value.Quality == db.Quality)
		        {
		            count++;
		        }
		    }

		    if (data.StrengthLvl >= count)
		    {
		        GTItemHelper.ShowTip("宠物等级已满");
		        return;
		    }
		}

		ReqUpgradePet req = new ReqUpgradePet();
		req.ID = id;
		req.UseItems.AddRange(items);
		NetworkManager.Instance.Send<ReqUpgradePet>(MessageID.MSG_REQ_UPGRADE_PET, req);*/
	}

	public void TrySetupPet(ulong guid)
	{
		SetupPetReq req = new SetupPetReq();
		req.PetGuid = guid;
		NetworkManager.Instance.Send<SetupPetReq>(MessageID.MSG_SETUP_PET_REQ, req);
	}

	public void TryUnsetPet(ulong guid)
	{
		UnsetPetReq req = new UnsetPetReq();
		req.PetGuid = guid;
		NetworkManager.Instance.Send<UnsetPetReq>(MessageID.MSG_UNSET_PET_REQ, req);

	}

	public void TrySortBag(EBagType bagType)
	{

	}

	public void TryDressEquip(ulong Guid, ulong bagGuid)
	{
		SetupEquipReq req = new SetupEquipReq();
		req.EquipGuid = Guid;
		req.BagGuid = bagGuid;
		NetworkManager.Instance.Send<SetupEquipReq>(MessageID.MSG_SETUP_EQUIP_REQ, req);
	}

	public void TryUnDressEquip(ulong guid)
	{
//         if (GTItemHelper.CheckBagFull(1, EBagType.ITEM))
//         {
//             return;
//         }
		UnsetEquipReq req = new UnsetEquipReq();
		req.EquipGuid = guid;
		NetworkManager.Instance.Send<UnsetEquipReq>(MessageID.MSG_UNSET_EQUIP_REQ, req);
	}

	public void TryDressGem(int index, ulong gemguid, ulong bagguid)
	{
		SetupGemReq req = new SetupGemReq();
		req.BagGuid = bagguid;
		req.GemGuid = gemguid;
		req.TargetPos = index;
		NetworkManager.Instance.Send<SetupGemReq>(MessageID.MSG_SETUP_GEM_REQ, req);
	}

	public void TryUnDressGem(ulong gemguid)
	{
		UnsetGemReq req = new UnsetGemReq();
		req.GemGuid = gemguid;
		NetworkManager.Instance.Send<UnsetGemReq>(MessageID.MSG_UNSET_GEM_REQ, req);
	}

	public void TryOneKeyToDressGem(int index)
	{
		//ReqOneKeyToDressGem req = new ReqOneKeyToDressGem();
		//req.Index = index;
		//NetworkManager.Instance.Send<ReqOneKeyToDressGem>(MessageID.MSG_REQ_ONEKEYTODRESSGEM, req);
	}

	public void TryOneKeyToUnloadGem(int index)
	{
		//ReqOneKeyToUnloadGem req = new ReqOneKeyToUnloadGem();
		//req.Index = index;
		//NetworkManager.Instance.Send<ReqOneKeyToUnloadGem>(MessageID.MSG_REQ_ONEKEYTOUNLOADGEM, req);
	}

	public void TryAdvanceEquip(EquipItem equip)
	{
//         if (MLEquip.Instance.IsFullAdvanceLevel(equip))
//         {
//             GTItemHelper.ShowTip("进阶等级已满");
//             return;
//         }
//         DEquip cfg = ReadCfgEquip.GetDataById(equip.Id);
//         int advanceID = cfg.Quality * 1000 + equip.AdvanceLevel + 1;
//         DEquipAdvanceCost db = ReadCfgEquipAdvanceCost.GetDataById(advanceID);
//
//         if (!GTItemHelper.CheckItemEnongh(db.CostMoneyId, db.CostMoneyNum))
//         {
//             return;
//         }
//         if (!GTItemHelper.CheckItemEnongh(db.CostItemId, db.CostItemNum))
//         {
//             return;
//         }
//
//         List<XItem> list;
//         if (db.CostEquipNum > 0)
//         {
//             list = MLEquip.Instance.GetBagSameEquipList(equip);
//             if (list.Count < db.CostEquipNum)
//             {
//                 GTItemHelper.ShowTip("缺少同样的装备");
//                 return;
//             }
//             list.RemoveRange(db.CostEquipNum, list.Count - db.CostEquipNum);
//         }
//         else
//         {
//             list = new List<XItem>();
//         }
//
//         ReqAdvanceEquip req = new ReqAdvanceEquip();
//         req.TarEquip = equip;
//         req.UseItems.AddRange(list);
//         NetworkManager.Instance.Send<ReqAdvanceEquip>(MessageID.MSG_REQ_ADVANCE_EQUIP, req);
	}

	public void TryUpStarEquip(EquipItem equip)
	{
//         if (MLEquip.Instance.IsFullStarLevel(equip))
//         {
//             GTItemHelper.ShowTip("装备星级已满");
//             return;
//         }
//         DEquip equipDB = ReadCfgEquip.GetDataById(equip.Id);
//         int starID = equipDB.Quality * 1000 + equip.StarLevel + 1;
//         DEquipStar db = ReadCfgEquipStar.GetDataById(starID);
//         if (!GTItemHelper.CheckItemEnongh(db.CostMoneyId, db.CostMoneyNum))
//         {
//             return;
//         }
//         if (!GTItemHelper.CheckItemEnongh(db.CostItemId, db.CostItemNum))
//         {
//             return;
//         }
//
//         ReqUpStarEquip req = new ReqUpStarEquip();
//         req.TarEquip = equip;
//         NetworkManager.Instance.Send<ReqUpStarEquip>(MessageID.MSG_REQ_UPSTAR_EQUIP, req);
	}

	public void TryChargeRelics(int relicsID, int index)
	{
//         DRelics db = ReadCfgRelics.GetDataById(relicsID);
//         if (db == null)
//         {
//             GTItemHelper.ShowTip("非法物品");
//             return;
//         }
//         if (index > 3 || index < 1)
//         {
//             GTItemHelper.ShowTip("非法索引" + index.ToString());
//             return;
//         }
//         XRelics relics = DataDBSRelics.GetDataById(relicsID);
//         if (relics != null)
//         {
//             if (MLRelics.Instance.GetExp(relics, index) >= db.LevelExps[relics.Level])
//             {
//                 GTItemHelper.ShowTip("你不能对此充能了");
//                 return;
//             }
//         }
//         if (GTItemHelper.CheckItemEnongh(db.ArtificeCostIDs[index - 1], 1) == false)
//         {
//             return;
//         }
//
//         ReqChargeRelics req = new ReqChargeRelics();
//         req.RelicsID = relicsID;
//         req.Index = index;
//         NetworkManager.Instance.Send(MessageID.MSG_REQ_CHARGE_RELICS, req);
	}

	public void TryUpgradeRelics(int relicsID)
	{
//         DRelics db = ReadCfgRelics.GetDataById(relicsID);
//         if (db == null)
//         {
//             GTItemHelper.ShowTip("非法物品");
//             return;
//         }
//         XRelics relics = DataDBSRelics.GetDataById(relicsID);
//         if (relics == null)
//         {
//             GTItemHelper.ShowTip("非法物品");
//             return;
//         }
//         if (relics.Level == db.LevelExps.Length)
//         {
//             GTItemHelper.ShowTip("已升到最大等级，无法继续升级");
//             return;
//         }
//         for (int i = 0; i < 3; i++)
//         {
//             if (MLRelics.Instance.GetExp(relics, i + 1) < db.LevelExps[relics.Level])
//             {
//                 GTItemHelper.ShowTip("神器未获得足够的充能");
//                 return;
//             }
//         }
//         ReqUpgradeRelics req = new ReqUpgradeRelics();
//         req.RelicsID = relicsID;
//         NetworkManager.Instance.Send(MessageID.MSG_REQ_UPGRADE_RELICS, req);
	}

	public void TryBattleRelics(int relicsID)
	{

	}

	public void TryUnloadRelics(int relicsID)
	{

	}

	public void TryBuyStore(int storeType, int storeID, int num)
	{
		DStore storeDB = ReadCfgStore.GetDataById(storeID);
		if (storeDB == null)
		{
			GTItemHelper.ShowTip("非法物品");
			return;
		}

		StoreBuyReq req = new StoreBuyReq();
		req.StoreType = storeType;
		req.StoreID = storeID;
		req.BuyNum = num;
		NetworkManager.Instance.Send<StoreBuyReq>(MessageID.MSG_STORE_BUY_REQ, req);
	}

	public void TrySyncAction(EActionType eActType, ulong Guid, Vector3 Pos, float ft)
	{
		if(GTData.CopyGUID == 0)
		{
			return;
		}

		ObjectActionReq req = GTTools.GetExtensionObj<ObjectActionReq>(0);
		ActionReqItem data = GTTools.GetExtensionObj<ActionReqItem>(0);

		data.ActionID = (int)eActType;
		data.HostX  = Pos.x;
		data.HostY  = Pos.y;
		data.HostZ  = Pos.z;
		data.HostFt = ft;
		data.ObjectGuid = Guid;

		req.ActionList.Add(data);
		NetworkManager.Instance.Send(MessageID.MSG_OBJECT_ACTION_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
		req.ActionList.Clear();
	}


	public void TrySyncMove()
	{
		ActionReqItem data = GTTools.GetExtensionObj<ActionReqItem>(0);
		data.ActionID   = (int)EActionType.AT_RUN;
		data.HostX      = GTWorld.Main.Pos.x;
		data.HostY      = GTWorld.Main.Pos.y;
		data.HostZ      = GTWorld.Main.Pos.z;
		data.HostFt     = GTWorld.Main.Face;
		data.ObjectGuid = GTWorld.Main.GUID;
		ObjectActionReq req = GTTools.GetExtensionObj<ObjectActionReq>(0);
		req.ActionList.Add(data);
		NetworkManager.Instance.Send(MessageID.MSG_OBJECT_ACTION_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
		req.ActionList.Clear();
	}

	public void TrySyncPosition()
	{
		ActionReqItem data = GTTools.GetExtensionObj<ActionReqItem>(1);
		data.ActionID = (int)EActionType.AT_WALK;
		data.HostX = GTWorld.Main.Pos.x;
		data.HostY = GTWorld.Main.Pos.y;
		data.HostZ = GTWorld.Main.Pos.z;
		data.HostFt = GTWorld.Main.Face;
		data.ObjectGuid = GTWorld.Main.GUID;
		ObjectActionReq req = GTTools.GetExtensionObj<ObjectActionReq>(1);
		req.ActionList.Add(data);
		NetworkManager.Instance.Send(MessageID.MSG_OBJECT_ACTION_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
		req.ActionList.Clear();
	}

	public void TrySyncIdle()
	{
		ActionReqItem data = GTTools.GetExtensionObj<ActionReqItem>(2);
		data.ActionID   = (int)EActionType.AT_IDLE;
		data.HostX      = GTWorld.Main.Pos.x;
		data.HostY      = GTWorld.Main.Pos.y;
		data.HostZ      = GTWorld.Main.Pos.z;
		data.HostFt     = GTWorld.Main.Face;
		data.ObjectGuid = GTWorld.Main.GUID;
		ObjectActionReq req = GTTools.GetExtensionObj<ObjectActionReq>(2);
		req.ActionList.Clear();
		req.ActionList.Add(data);
		NetworkManager.Instance.Send(MessageID.MSG_OBJECT_ACTION_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
		req.ActionList.Clear();
	}

	public void TryJump()
	{
		ActionNtyItem data = GTTools.GetExtensionObj<ActionNtyItem>(3);
	}

	public void TryCastSkill(int id, Actor Castor, List<Actor> Targets, Vector3 targetPos)
	{
		SkillCastReq req = new SkillCastReq();
		req.SkillID    = id;
		req.ObjectGuid = Castor.GUID;
		req.HostX      = Castor.Pos.x;
		req.HostY      = Castor.Pos.y;
		req.HostZ      = Castor.Pos.z;
		req.HostFt     = Castor.Face;
		req.TargetFt   = 0;
		req.TargetX    = targetPos.x;
		req.TargetY    = targetPos.y;
		req.TargetZ    = targetPos.z;
		if (Targets != null)
		{
			for (int i = 0; i < Targets.Count; i++)
			{
				req.TargetObjects.Add(Targets[i].GUID);
			}
		}
		NetworkManager.Instance.Send(MessageID.MSG_SKILL_CAST_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
	}

	public void TryRideOnMount(ulong guid )
	{
		Msg_RidingMountReq req = new Msg_RidingMountReq();
		req.ObjectGuid = guid;
		NetworkManager.Instance.Send(MessageID.MSG_MOUNT_RIDING_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
	}

	public void TryRideOffMount(ulong guid)
	{
		Msg_RidingMountReq req = new Msg_RidingMountReq();
		req.ObjectGuid = guid;
		NetworkManager.Instance.Send(MessageID.MSG_MOUNT_RIDING_REQ, req, GTData.Main.GUID, (uint)GTData.CopyGUID);
	}

	public void TryMoveJoystick(float x, float y)
	{
		if (GTWorld.Main == null || GTWorld.Main.IsFSMLayer1())
		{
			return;
		}
		Camera cam = GTCameraManager.Instance.MainCamera;
		Quaternion q = new Quaternion();
		float r = Mathf.Deg2Rad * cam.transform.eulerAngles.y * 0.5f;
		q.w = Mathf.Cos(r);
		q.x = 0;
		q.y = Mathf.Sin(r);
		q.z = 0;
		Vector3 motion = q * new Vector3(x, 0, y);
		GTWorld.Main.Get<ActorCommand>().GetCmd<CommandMove>().Update(motion).Do();
	}

	public void TryStopJoystick()
	{
		if (GTWorld.Main == null || GTWorld.Main.IsFSMLayer1())
		{
			return;
		}
		GTWorld.Main.Get<ActorCommand>().GetCmd<CommandIdle>().Do();
	}

	public void TryReborn(UInt64 roleGUID, Int32 rebornType)
	{
		Msg_RoleRebornReq req = new Msg_RoleRebornReq();
		req.RebornType = rebornType;
		req.ObjectGuid = roleGUID;
		NetworkManager.Instance.Send<Msg_RoleRebornReq>(MessageID.MSG_ROLE_REBORN_REQ, req, roleGUID, (uint)GTData.CopyGUID);
	}

	public void TryMainCopyReq(UInt64 roleGUID, Int32 dwCopyID)
	{
		DCopy copyDB = ReadCfgCopy.GetDataById(dwCopyID);
//       if (GTItemHelper.CheckItemEnongh(copyDB.CostActionId, copyDB.CostActionNum) == false)
//       {
//           return;
//       }
		MainCopyReq req = new MainCopyReq();
		req.CopyID = dwCopyID;
		NetworkManager.Instance.Send<MainCopyReq>(MessageID.MSG_MAIN_COPY_REQ, req, roleGUID, 0);
	}

	public void TryMainCopyPass(int chapter, int copyID, int starNum)
	{

	}

	public void TryMainCopyReceiveReward(int chapter, int index)
	{
//         ERewardState rewardState = MLRaid.Instance.GetChapterRewardStateByAwardIndex(chapter, index);
//         switch (rewardState)
//         {
//             case ERewardState.NOT_RECEIVE:
//                 GTItemHelper.ShowTip("未达成条件");
//                 return;
//             case ERewardState.HAS_RECEIVE:
//                 GTItemHelper.ShowTip("奖励已领取");
//                 return;
//         }
//         DCopyMainChapter chapterDB = ReadCfgCopyMainChapter.GetDataById(chapter);
//         DAward awardDB = ReadCfgAward.GetDataById(chapterDB.Awards[index]);
//         if (GTItemHelper.CheckBagFull(awardDB.MaxDropNum))
//         {
//             return;
//         }
	}
}
