using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using Protocol;

public class UITeamCopy : GTWindow
{
    private GameObject      btnStartBattle;
    private GameObject      btnAutoMatch;
    private GameObject      btnInvite;
    private GameObject      btnCreate;
    private GameObject      btnClose;
    private List<UITexture> mTextureList = new List<UITexture>(3);
    private List<ERender> mRenderList = new List<ERender>(3);
    private List<ActorAvator> mAvatarList = new List<ActorAvator>(3);
    private List<UILabel> mNameList = new List<UILabel>(3);
    public UITeamCopy()
    {
        Path = "Raid/UITeamCopy";
        Resident = false;
        Type = EWindowType.Window;
        MaskType = EWindowMaskType.BlackTransparent;
        ShowMode = EWindowShowMode.SaveTarget;
    }

    protected override void OnAwake()
    {
        Transform pivot = transform.Find("Pivot");
        
        btnClose = pivot.Find("BtnClose").gameObject;
        btnAutoMatch = pivot.Find("Bottom/BtnAutoMatch").gameObject;
        btnStartBattle = pivot.Find("Bottom/BtnStartBattle").gameObject;
        btnInvite = pivot.Find("Bottom/BtnInvite").gameObject;
        btnCreate = pivot.Find("Bottom/BtnCreate").gameObject;

        mTextureList.Add(pivot.Find("RoleModel_1").GetComponent<UITexture>());
        mTextureList.Add(pivot.Find("RoleModel_2").GetComponent<UITexture>());
        mTextureList.Add(pivot.Find("RoleModel_3").GetComponent<UITexture>());

        mNameList.Add(pivot.Find("HeroName_1/Label").GetComponent<UILabel>());
        mNameList.Add(pivot.Find("HeroName_2/Label").GetComponent<UILabel>());
        mNameList.Add(pivot.Find("HeroName_3/Label").GetComponent<UILabel>());
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick = OnCloseClick;
        UIEventListener.Get(btnStartBattle).onClick = OnStartBattleClick;
        UIEventListener.Get(btnAutoMatch).onClick = OnAutoMatchClick;
        UIEventListener.Get(btnInvite).onClick = OnInviteClick;
        UIEventListener.Get(btnCreate).onClick = OnCreateClick;

        UIEventListener.Get(mTextureList[0].gameObject).onDrag = OnHeroTextureDrag0;
        UIEventListener.Get(mTextureList[1].gameObject).onDrag = OnHeroTextureDrag1;
        UIEventListener.Get(mTextureList[2].gameObject).onDrag = OnHeroTextureDrag2;
       
    }

    private void OnHeroTextureDrag0(GameObject go, Vector2 delta)
    {
        ESpin.Get(mAvatarList[0].GetRootObj()).OnSpin(delta, 2);
    }

    private void OnHeroTextureDrag1(GameObject go, Vector2 delta)
    {
        ESpin.Get(mAvatarList[1].GetRootObj()).OnSpin(delta, 2);
    }

    private void OnHeroTextureDrag2(GameObject go, Vector2 delta)
    {
        ESpin.Get(mAvatarList[2].GetRootObj()).OnSpin(delta, 2);
    }
    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_TEAM_UPDATE_VIEW, UpdateView);
    }

    protected override void OnEnable()
    {
        
    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_TEAM_UPDATE_VIEW, UpdateView);
    }

    protected override void OnClose()
    {
        ulong uRoomID = GLTeamCopy.Instance.GetRoomID();
        if (uRoomID > 0)
        {
            GLTeamCopy.Instance.TryLeaveTeamRoom(uRoomID);
        }

        for(int i = 0; i < mAvatarList.Count; i++)
        {
            GTResourceManager.Instance.DestroyObj(mAvatarList[i].GetRootObj());
            mAvatarList[i] = null;
        }

        for (int i = 0; i < mRenderList.Count; i++)
        {
            mRenderList[i].Release();
            mRenderList[i] = null;
        }

        mAvatarList.Clear();
        mRenderList.Clear();
        mTextureList.Clear();
        mNameList.Clear();
    }

    private void OnCreateClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GLTeamCopy.Instance.TryCreateTeamRoom(10001);
    }

    private void OnStartBattleClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);

        ulong uRoomID = GLTeamCopy.Instance.GetRoomID();
        if(uRoomID <= 0)
        {
            return;
        }

        GLTeamCopy.Instance.TryStartTeamRoom(uRoomID);
    }

    private void OnAutoMatchClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GLTeamCopy.Instance.TryJoinTeamRoom(0);
    }

    private void OnInviteClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }
    private void UpdateView()
    {
        for (int i = 0; i < mRenderList.Count; i++)
        {
            mRenderList[i].DetachModel();
            mNameList[i].text = "";
        }

        for (int i = 0; i < mAvatarList.Count; i++)
        {
            GTResourceManager.Instance.DestroyObj(mAvatarList[i].GetRootObj());
            mAvatarList[i] = null;
        }

        mAvatarList.Clear();

        for (int i = 0; i < GLTeamCopy.Instance.m_RoomData.PlayerList.Count; i++)
        {
            Msg_RoomPlayerInfo info = GLTeamCopy.Instance.m_RoomData.PlayerList[i];

            DActor db = ReadCfgActor.GetDataById(info.ActorID);
            ActorAvator actor = GTWorld.Instance.AddAvatar(db.Model);
            mAvatarList.Add(actor);
     
            for (int j = 0; j < 8; j++)
            {
                actor.ChangeAvatar(j + 1, info.Equips[j]);
            }

            actor.PlayAnim("idle", null);

            if( i >= mRenderList.Count)
            {
                ERender render = ERender.AddRender(mTextureList[i]);
                mRenderList.Add(render);
            }

            if (mRenderList[i] != null)
            {
                GameObject model = mRenderList[i].AttachModel(actor.GetRootObj());
                model.transform.localPosition = new Vector3(0, -0.8f, 3.5f);
                model.transform.localEulerAngles = new Vector3(0, 180, 0);
            }
            else
            {
                Debug.LogError("--------------------------");
            }
           

            mNameList[i].text = GTTools.Format("Lv.{0} {1}", info.Level, info.Name);
        }

        if (GLTeamCopy.Instance.m_RoomData.PlayerList.Count > 0)
        {
            Msg_RoomPlayerInfo info = GLTeamCopy.Instance.m_RoomData.PlayerList[0];
            if (info.RoleID == GTWorld.Main.GUID)
            {
                btnStartBattle.SetActive(true);
                btnAutoMatch.SetActive(false);
            }
            else
            {
                btnStartBattle.SetActive(false);
                btnAutoMatch.SetActive(true);
            }
        }
        else
        {
            btnStartBattle.SetActive(false);
            btnAutoMatch.SetActive(true);
        }

    }
    private void ShowView()
    {
        for( int i = 0; i < GLTeamCopy.Instance.m_RoomData.PlayerList.Count; i++)
        {
            Msg_RoomPlayerInfo info = GLTeamCopy.Instance.m_RoomData.PlayerList[i];

            if (mAvatarList[i] == null)
            {
                DActor db = ReadCfgActor.GetDataById(info.ActorID);
                mAvatarList[i] = GTWorld.Instance.AddAvatar(db.Model);
            }

            for (int j = 0; j < 8; j++)
            {
                mAvatarList[i].ChangeAvatar(j + 1, info.Equips[j]);
            }

            mAvatarList[i].PlayAnim("idle", null);
            GameObject model = mRenderList[i].AttachModel(mAvatarList[i].GetRootObj());
            model.transform.localPosition = new Vector3(0, -0.8f, 3.5f);
            model.transform.localEulerAngles = new Vector3(0, 180, 0);
        }

       


    }
}
