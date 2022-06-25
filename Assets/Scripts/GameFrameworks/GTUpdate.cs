using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GTUpdate : GTSingleton<GTUpdate>
{
    private List<System.Action> mUpdateCallbacks     = new List<System.Action>();
    private List<System.Action> mLateUpdateCallbacks = new List<System.Action>();

    public void AddUpdate(System.Action callback)
    {
        mUpdateCallbacks.Add(callback);
    }

    public void DelListener(System.Action callback)
    {
        mUpdateCallbacks.Remove(callback);
    }

    public void AddListenerLateUpdate(System.Action callback)
    {
        mLateUpdateCallbacks.Add(callback);
    }

    public void DelListenerLateUpdate(System.Action callback)
    {
        mLateUpdateCallbacks.Remove(callback);
    }

    public void Execute()
    {
        for (int i = 0; i < mUpdateCallbacks.Count; i++)
        {
            mUpdateCallbacks[i].Invoke();
        }
    }

    public void ExecuteLateUpdate()
    {
        for (int i = 0; i < mLateUpdateCallbacks.Count; i++)
        {
            mLateUpdateCallbacks[i].Invoke();
        }
    }
}