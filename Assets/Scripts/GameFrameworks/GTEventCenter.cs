using UnityEngine;
using System;
using System.Collections.Generic;

public class GTEventCenter
{
    static Dictionary<GTEventID, List<Delegate>> mEvents = new Dictionary<GTEventID, List<Delegate>>(new GTEventTypeComparer());

    static void OnListenerAdding(GTEventID e, Delegate d)
    {
        List<Delegate> list = null;
        mEvents.TryGetValue(e, out list);
        if(list == null)
        {
            list = new List<Delegate>();
            mEvents.Add(e, list);
        }
        for (int i = 0; i < list.Count; i++)
        {
            if(list[i] == d)
            {
                string error = string.Format("添加事件监听错误，EventID:{0}，添加的事件{1}", e.ToString(), d.GetType().Name);
                Debug.LogError(error);
                return;
            }
        }
        list.Add(d);
    }

    static void OnListenerRemoved(GTEventID e, Delegate d)
    {
        List<Delegate> list = null;
        mEvents.TryGetValue(e, out list);
        if (list == null || list.Count == 0)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            if (list[i] == d)
            {
                mEvents.Remove(e);
                break;
            }
        }
    }

    public static void AddHandler(GTEventID e, Callback handler)
    {
        OnListenerAdding(e,handler);
    }

    public static void AddHandler<T>(GTEventID e, Callback<T> handler)
    {
        OnListenerAdding(e, handler);
    }

    public static void AddHandler<T, U>(GTEventID e, Callback<T, U> handler)
    {
        OnListenerAdding(e, handler);
    }

    public static void AddHandler<T, U, V>(GTEventID e, Callback<T, U, V> handler)
    {
        OnListenerAdding(e, handler);
    }

    public static void AddHandler<T, U, V, X>(GTEventID e, Callback<T, U, V, X> handler)
    {
        OnListenerAdding(e, handler);
    }

    public static void DelHandler(GTEventID e, Callback handler)
    {
        OnListenerRemoved(e, handler);
    }

    public static void DelHandler<T>(GTEventID e, Callback<T> handler)
    {
        OnListenerRemoved(e, handler);
    }

    public static void DelHandler<T, U>(GTEventID e, Callback<T, U> handler)
    {
        OnListenerRemoved(e, handler);
    }

    public static void DelHandler<T, U, V>(GTEventID e, Callback<T, U, V> handler)
    {
        OnListenerRemoved(e, handler);
    }

    public static void DelHandler<T, U, V, X>(GTEventID e, Callback<T, U, V, X> handler)
    {
        OnListenerRemoved(e, handler);
    }

    public static void FireEvent(GTEventID e)
    {
        List<Delegate> list = null;
        if (mEvents.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Delegate d = list[i];
                Callback callback = d as Callback;
                if (callback != null)
                {
                    callback();
                }
            }
        }
    }

    public static void FireEvent<T>(GTEventID e, T arg1)
    {
        List<Delegate> list = null;
        if (mEvents.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Delegate d = list[i];
                Callback<T> callback = d as Callback<T>;
                if (callback != null)
                {
                    callback(arg1);
                }
            }
        }
    }

    public static void FireEvent<T, U>(GTEventID e, T arg1, U arg2)
    {
        List<Delegate> list = null;
        if (mEvents.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Delegate d = list[i];
                Callback<T, U> callback = d as Callback<T, U>;
                if (callback != null)
                {
                    callback(arg1, arg2);
                }
            }
        }
    }

    public static void FireEvent<T, U, V>(GTEventID e, T arg1, U arg2, V arg3)
    {
        List<Delegate> list = null;
        if (mEvents.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Delegate d = list[i];
                Callback<T, U, V> callback = d as Callback<T, U, V>;
                if (callback != null)
                {
                    callback(arg1, arg2, arg3);
                }
            }
        }
    }

    public static void FireEvent<T, U, V, X>(GTEventID e, T arg1, U arg2, V arg3, X arg4)
    {
        List<Delegate> list = null;
        if (mEvents.TryGetValue(e, out list))
        {
            for (int i = 0; i < list.Count; i++)
            {
                Delegate d = list[i];
                Callback<T, U, V, X> callback = d as Callback<T, U, V, X>;
                if (callback != null)
                {
                    callback(arg1, arg2, arg3, arg4);
                }
            }
        }
    }
}