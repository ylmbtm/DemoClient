using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GTAsync : GTMonoSingleton<GTAsync>
{
    static List<Callback> m_Asyncs = new List<Callback>();

    public void Run(Callback callback)
    {
        m_Asyncs.Add(callback);
    }

    public void Execute()
    {
        if (m_Asyncs.Count > 0)
        {
            for (int i = 0; i < m_Asyncs.Count; i++)
            {
                m_Asyncs[i]();
            }
            m_Asyncs.Clear();
        }
    }
}
