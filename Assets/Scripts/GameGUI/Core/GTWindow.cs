using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using DG.Tweening;
using DG.Tweening.Core;

public class GTWindow
{
    public Transform        transform       { get; set; }
    public bool             Resident        { get; set; }
    public string           Path            { get; set; }
    public EWindowID        ID              { get; set; }
    public EWindowType      Type            { get; set; }
    public EWindowHideType  HideType        { get; set; }
    public EWindowMaskType  MaskType        { get; set; }
    public EWindowOpenType  OpenType        { get; set; }
    public EWindowShowMode  ShowMode        { get; set; }
    public EWindowID        TargetID        { get; set; }

    public Action           onMaskClick     { get; set; }
    public Action           onAwake         { get; set; }
    public Action           onEnable        { get; set; }
    public Action           onClose         { get; set; }

    private List<int>       m_TimeKeys = new List<int>();

    protected virtual void OnAwake()
    {
        if (onAwake != null)
        {
            onAwake();
        }
    }

    protected virtual void OnEnable()
    {
        if (onEnable != null)
        {
            onEnable();
        }
    }

    protected virtual void OnClose()
    {
        if (onClose != null)
        {
            onClose();
        }
    }

    protected virtual void OnAddButtonListener()
    {

    }

    protected virtual void OnAddHandler()
    {

    }

    protected virtual void OnDelHandler()
    {

    }

    public bool            IsVisable()
    {
        return transform == null ? false : transform.gameObject.activeSelf;
    }

    public bool            IsResident()
    {
        return Resident;
    }

    public UIPanel         Panel
    {
        get
        {
            if (transform == null)
            {
                return null;
            }
            else
            {
                return transform.GetComponent<UIPanel>();
            }
        }
    }

    public void Show()
    {
        if (transform == null)
        {
            if (LoadAsync())
            {
                OnAwake();
            }
        }
        if (transform)
        {
            OnAddButtonListener();
            OnAddHandler();
            OnEnable();
        }
    }

    public void Hide()
    {
        GTWindowManager.Instance.HideWindow(ID);
    }

    public bool LoadAsync()
    {
        if (string.IsNullOrEmpty(Path))
        {
            Debug.LogError("资源名为空");
            return false;
        }
        string path = string.Format("Guis/{0}.prefab", Path);
        GameObject prefab = GTResourceManager.Instance.Load<GameObject>(path);
        if (prefab == null)
        {
            Debug.LogError(string.Format("加载Window资源失败:{0}", Path));
            return false;
        }
        GameObject go = GameObject.Instantiate(prefab);
        transform = go.transform;
        return true;
    }

    public void HideAsync(bool forceToDestroy)
    {
        TargetID = EWindowID.UINone;
        for (int i = 0; i < m_TimeKeys.Count; i++)
        {
            GTTimerManager.Instance.DelTimer(m_TimeKeys[i]);
        }
        m_TimeKeys.Clear();
        if (transform)
        {
            OnDelHandler();
            if (Resident && forceToDestroy == false)
            {
                SetActive(false);
            }
            else
            {
                SetActive(false);
                OnClose();
                Destroy();
                transform = null;
            }
        }
    }

    public void Destroy()
    {
        TargetID = EWindowID.UINone;
        if (transform)
        {
            GTResourceManager.Instance.DestroyObj(transform.gameObject);
        }
    }

    public void SetActive(bool active)
    {
        if (transform == null)
        {
            return;
        }
        if (active)
        {
            if (transform.gameObject.activeSelf == false)
            {
                transform.gameObject.SetActive(true);
            }
            Vector3 pos = transform.localPosition;
            pos.x = 0;
            transform.localPosition = pos;
        }
        else
        {
            Vector3 pos = transform.localPosition;
            pos.x = 20000;
            transform.localPosition = pos;
        }
    }

    public void PlayInvoke(float sec, Callback callback, int tick = 1)
    {
        int key = GTTimerManager.Instance.AddTimer(sec, callback, tick).key;
        m_TimeKeys.Add(key);
    }

    public void StopInvoke(int key)
    {
        GTTimerManager.Instance.DelTimer(key);
        m_TimeKeys.Remove(key);
    }

    public virtual void OnLoadSubWindows()
    {

    }
}