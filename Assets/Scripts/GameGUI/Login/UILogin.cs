
using UnityEngine;
using System.Collections;
using System;
using Protocol;

public class UILogin : GTWindow
{
    private GameObject       state3;

    private GameObject       btnNotice;
    private GameObject       btnLoginGame;
    private GameObject       btnServer;

    private UILabel          versionNumber;
    private UILabel          curServerName;
    private UILabel          curAccount;


    public UILogin()
    {
        Resident = false;
        Path = "Login/UILogin";
        Type = EWindowType.Window;
        MaskType = EWindowMaskType.None;
    }

    protected override void OnAwake()
    {
        state3 = transform.Find("State3").gameObject;

        btnNotice = state3.transform.Find("Btn_Notice").gameObject;
        btnLoginGame = state3.transform.Find("Btn_LoginGame").gameObject;
        btnServer  = state3.transform.Find("Btn_Server").gameObject;

        curServerName = btnServer.transform.Find("Label").GetComponent<UILabel>();
        versionNumber = transform.Find("Bottom/Version").GetComponent<UILabel>();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnNotice).onClick          = OnNoticeClick;
        UIEventListener.Get(btnLoginGame).onClick       = OnLoginGameClick;
        UIEventListener.Get(btnServer).onClick          = OnServerClick;
    }



    protected override void OnAddHandler()
    {
        GTEventCenter.AddHandler(GTEventID.TYPE_LOGIN_SELECTSERVER,       OnRecvLoginGame);
        GTEventCenter.AddHandler(GTEventID.TYPE_LOGIN_ACCOUNT_LOGIN,      OnRecvAccLogin);
        GTEventCenter.AddHandler(GTEventID.TYPE_LOGIN_GETSERVERLIST,      OnRecvGetServers);
        GTEventCenter.AddHandler(GTEventID.TYPE_LOGIN_SELECTSERVER,       OnRecvSelectServer);

    }

    protected override void OnDelHandler()
    {
        GTEventCenter.DelHandler(GTEventID.TYPE_LOGIN_SELECTSERVER,       OnRecvLoginGame);
        GTEventCenter.DelHandler(GTEventID.TYPE_LOGIN_ACCOUNT_LOGIN,      OnRecvAccLogin);
        GTEventCenter.DelHandler(GTEventID.TYPE_LOGIN_GETSERVERLIST,      OnRecvGetServers);
        GTEventCenter.DelHandler(GTEventID.TYPE_LOGIN_SELECTSERVER,       OnRecvSelectServer);

    }

    protected override void OnEnable()
    {
        this.InitView();
        this.InitGameModeView();
        this.ShowCurrServer();
    }

    protected override void OnClose()
    {

    }

    private void InitView()
    {
        versionNumber.text = GTTools.Format("版本号：{0}", Application.version);
        GTXmlHelper.CanWrite = false;
    }

    private void InitGameModeView()
    {
        state3.SetActive(true);
        btnNotice. SetActive(true);
        btnServer. SetActive(true);
    }

    private void OnServerClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTNetworkSend.Instance.TryGetSvrList();
    }

    private void OnLoginGameClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        ClientServerNode node = MLLogin.Instance.GetCurrServer();
        GTNetworkSend.Instance.TrySelectServer(node == null ? 0 : node.SvrID);
    }

    private void OnAccountClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTWindowManager.Instance.OpenWindow(EWindowID.UIAccount);
    }

    private void OnAccountLoginClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
    }

    private void OnNoticeClick(GameObject go)
    {
        GTAudioManager.Instance.PlayEffectAudio(GTAudioKey.SOUND_UI_CLICK);
        GTWindowManager.Instance.OpenWindow(EWindowID.UINotice);
    }

    private void OnRecvLoginGame()
    {
        this.Hide();
    }

    private void OnRecvAccLogin()
    {
        this.InitGameModeView();
        this.ShowCurrServer();
    }

    private void OnRecvGetServers()
    {
        GTWindowManager.Instance.OpenWindow(EWindowID.UIServer);
    }

    private void OnRecvSelectServer()
    {
        ShowCurrServer();
    }

    public void ShowCurrServer()
    {
        ClientServerNode data = MLLogin.Instance.GetCurrServer();
        curServerName.text = data == null ? "服务器" : data.SvrName;
    }
}
