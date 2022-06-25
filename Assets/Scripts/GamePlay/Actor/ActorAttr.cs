using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Protocol;

public class ActorAttr
{
    public int[] m_Attrs = new int[(int)EAttrID.EA_ATTR_NUM];
    public int GetAttr(EAttrID id)
    {
        return m_Attrs[(int)id];
    }
    public void SetAttr(EAttrID id, int value)
    {
        m_Attrs[(int)id] = value;
    }
    public void Update(List<int> list)
    {
        if (list == null)
        {
            return;
        }
        for (int i = 0; i < list.Count; i++)
        {
            EAttrID id = (EAttrID)i;
            SetAttr(id, list[i]);
        }
    }
}
   