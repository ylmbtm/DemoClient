using System;
using System.Collections.Generic;
using UnityEngine;

public class GTTimerManager : GTSingleton<GTTimerManager>
{
    private Dictionary<int, GTTimer> m_Timers       = new Dictionary<int, GTTimer>();
    private List<GTTimer>            m_AddBuffer    = new List<GTTimer>();
    private List<int>                m_DelBuffer    = new List<int>();
    private int                      m_Index        = 0;
    private GTTimer                  m_EmptyTimer   = new GTTimer();

    public GTTimer AddTimer(float callTime, Callback callback, int tick = 1)
    {
        if (callback == null)
        {
            return m_EmptyTimer;
        }
        if (callTime <= 0)
        {
            return m_EmptyTimer;
        }
        m_Index++;
        GTTimer item = new GTTimer();
        item.key = m_Index;
        item.callTime = callTime;
        item.callback = callback;
        item.tick = tick;
        item.currTick = 0;
        item.startTime = Time.realtimeSinceStartup;
        item.currTime = item.startTime;
        m_AddBuffer.Add(item);
        return item;
    }

    public void DelTimer(Callback callback)
    {
        Dictionary<int, GTTimer>.Enumerator em = m_Timers.GetEnumerator();
        while (em.MoveNext())
        {
            if (em.Current.Value.callback == callback)
            {
                m_DelBuffer.Add(em.Current.Key);
            }
        }
        em.Dispose();
    }

    public void DelTimer(GTTimer timer)
    {
        if (timer.key != 0)
        {
            m_DelBuffer.Add(timer.key);
        }
    }

    public void DelTimer(int timerKey)
    {
        if (timerKey != 0)
        {
            m_DelBuffer.Add(timerKey);
        }
    }

    public void Startup()
    {

    }

    public void Execute()
    {
        for (int i = 0; i < m_AddBuffer.Count; i++)
        {
            GTTimer item = m_AddBuffer[i];
            m_Timers.Add(item.key, item);
        }
        m_AddBuffer.Clear();
        if(m_Timers.Count > 0)
        {
            Dictionary<int, GTTimer>.Enumerator em = m_Timers.GetEnumerator();
            while (em.MoveNext())
            {
                GTTimer item = em.Current.Value;
                item.currTime = Time.realtimeSinceStartup;
                if (Time.realtimeSinceStartup - item.startTime >= item.callTime)
                {
                    if (item.callback != null)
                    {
                        item.callback();
                    }
                    item.startTime = Time.realtimeSinceStartup;
                    if (item.tick > 0)
                    {
                        item.currTick++;
                        if (item.tick == item.currTick)
                        {
                            m_DelBuffer.Add(item.key);
                        }
                    }
                }
                if (item.pause == true)
                {
                    m_DelBuffer.Add(item.key);
                }
            }
            em.Dispose();
        }
        for (int i = 0; i < m_DelBuffer.Count; i++)
        {
            m_Timers.Remove(m_DelBuffer[i]);
        }
        m_DelBuffer.Clear();
    }

    public void Release()
    {
        m_Timers.Clear();
        m_AddBuffer.Clear();
        m_DelBuffer.Clear();
    }
}