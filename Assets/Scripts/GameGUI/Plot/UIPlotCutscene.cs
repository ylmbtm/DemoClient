using UnityEngine;
using System.Collections;
using System;

public class UIPlotCutscene : GTWindow
{
    private GameObject btnSkip;
    private UILabel    content;

    public UIPlotCutscene()
    {
        Resident = false;
        Path = "Plot/UIPlotCutscene";
        Type = EWindowType.Window;
        MaskType = EWindowMaskType.WhiteTransparent;
        ShowMode = EWindowShowMode.HideOther;
    }

    protected override void OnAwake()
    {
        btnSkip = transform.Find("Top/Btn_Skip").gameObject;
        content = transform.Find("Bottom/Content").GetComponent<UILabel>();
    }

    protected override void OnAddButtonListener()
    {
        UIEventListener.Get(btnSkip).onClick = OnBtnSkipClick;
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
        
    }

    private void OnBtnSkipClick(GameObject go)
    {
        GTWorld.Instance.SkipCutscene();
    }
}
