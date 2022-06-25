using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using ACT;
using ECT;
using System.Linq;

public class ActorEffect : IActorComponent
{
    private Actor      m_Actor      = null;

    private Dictionary<ulong, Effect> m_Effects = new Dictionary<ulong, Effect>();

    public void Initial(Actor actor)
    {
        m_Actor = actor;
    }

    public void Execute()
    {
        foreach (var current in m_Effects.ToList())
        {
            Effect d = current.Value;
            if (d.IsEnd())
            {
                d.Release();
                m_Effects.Remove(current.Key);
                return;
            }
        }
    }

    public ulong AddEffect(int id, Transform parent, float lifeTime, bool retain, Vector3 offset, Vector3 eulerAngles, Vector3 scale)
    {
        if (id <= 0)
        {
            return 0;
        }

        DEffect db = ReadCfgEffect.GetDataById(id);
        if (db == null)
        {
            Debug.LogError(string.Format("找不到特效ID {1}", id));
            return 0;
        }

        GameObject go = GTPoolManager.Instance.GetObject(db.Path);
        if (go == null)
        {
            return 0;
        }

        NGUITools.SetLayer(go, GTLayer.LAYER_DEFAULT);

        Effect ef = go.GET<Effect>();

        ef.Init(GTData.NewGUID, id, parent, offset, eulerAngles, scale, lifeTime, retain);

        m_Effects.Add(ef.GUID, ef);

        return ef.GUID;
    }

    public ulong AddEffect(int id, Transform parent, float lifeTime, bool retain)
    {
        return AddEffect(id, parent, lifeTime, retain, Vector3.zero, Vector3.zero, Vector3.one);
    }

    public void DelEffect(ulong guid)
    {
        Effect d;

        if(!m_Effects.TryGetValue(guid, out d))
        {
            return;
        }

        if (d == null)
        {
            return;
        }

        d.Release();

        m_Effects.Remove(guid);
    }

    public void Release()
    {
        Dictionary<ulong, Effect>.Enumerator em = m_Effects.GetEnumerator();
        while (em.MoveNext())
        {
            em.Current.Value.Release();
        }
        em.Dispose();
        m_Effects.Clear();
    }
}
