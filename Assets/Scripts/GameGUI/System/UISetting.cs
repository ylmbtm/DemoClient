using UnityEngine;
using System.Collections;
using System;

public class UISetting : GTWindow
{
    private Transform  pivot;
    private GameObject btnClose;
    private GameObject btnSure;
    private GameObject btnLogout;

    private UIToggle   musicToggle;
    private UIToggle   soundToggle;
    private UIToggle   effectToggle;
    private UIToggle   rectSortToggle;

    public UISetting()
    {
        MaskType = EWindowMaskType.Black;
        Type = EWindowType.Window;
        Resident = false;
        Path = "System/UISetting";
        ShowMode = EWindowShowMode.HideOther;
    }

    protected override void OnAwake()
    {
        this.pivot        = this.transform.Find("Pivot");
        this.btnClose     = this.pivot.Find("BtnClose").gameObject;
        this.btnLogout    = this.pivot.Find("BtnLogout").gameObject;
        this.btnSure      = this.pivot.Find("BtnSure").gameObject;
        this.musicToggle  = this.pivot.Find("Music").GetComponent<UIToggle>();
        this.soundToggle  = this.pivot.Find("Sound").GetComponent<UIToggle>();
        this.effectToggle = this.pivot.Find("Effect").GetComponent<UIToggle>();
        this.rectSortToggle = this.pivot.Find("RectSort").GetComponent<UIToggle>();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnClose).onClick  = OnCloseClick;
        UIEventListener.Get(btnLogout).onClick = OnLogoutClick;
        UIEventListener.Get(btnSure).onClick   = OnSureClick;
        UIEventListener.Get(musicToggle.gameObject).onClick = OnMusicToggle;
        UIEventListener.Get(soundToggle.gameObject).onClick = OnSoundToggle;
        UIEventListener.Get(effectToggle.gameObject).onClick = OnEffectToggle;
        UIEventListener.Get(rectSortToggle.gameObject).onClick = OnRectSortToggle;
    }

    protected override void OnAddHandler()
    {
        
    }

    protected override void OnClose()
    {
        
    }

    protected override void OnDelHandler()
    {
        
    }

    protected override void OnEnable()
    {
        InitView();
    }

    private void InitView()
    {
        this.musicToggle.value  = GTAudioManager.Instance.MusicActive;
        this.soundToggle.value  = GTAudioManager.Instance.SoundActive;
        this.rectSortToggle.value = GTData.NativeData.RectSort;
    }

    private void OnSureClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        Hide();
    }

    private void OnLogoutClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        NetworkManager.Instance.Close();
        GTLauncher.Instance.LoadScene(GTCopyKey.SCENE_LOGIN);

    }

    private void OnCloseClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLOSE);
        Hide();
    }

    private void OnEffectToggle(GameObject go)
    {

    }

    private void OnSoundToggle(GameObject go)
    {
        GTAudioManager.Instance.SetSoundActive(!GTAudioManager.Instance.SoundActive);
    }

    private void OnMusicToggle(GameObject go)
    {
        GTAudioManager.Instance.SetMusicActive(!GTAudioManager.Instance.MusicActive);
        if (GTAudioManager.Instance.MusicActive)
        {
            DCopy db = ReadCfgCopy.GetDataById(GTData.CopyID);
            GTAudioManager.Instance.PlayMusic(db.SceneMusic);
        }
    }

    private void OnRectSortToggle(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTData.NativeData.RectSort = !GTData.NativeData.RectSort;
        GTEventCenter.FireEvent(GTEventID.TYPE_UPDATE_RECTSORT);
    }
}
