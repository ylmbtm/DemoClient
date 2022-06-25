using UnityEngine;
using System.Collections;
using System;

public class ActorMoveArrow : IActorComponent
{
    private GameObject m_MoveArrowObject;

    public void Initial(Actor actor)
    {
        m_MoveArrowObject = GTResourceManager.Instance.Load<GameObject>("Effect/other/jiantou", true);
        m_MoveArrowObject.transform.parent           = actor.CacheTransform;
        m_MoveArrowObject.transform.localPosition    = Vector3.zero;
        m_MoveArrowObject.transform.localScale       = Vector3.one;
        m_MoveArrowObject.transform.localEulerAngles = Vector3.one;
    }

    public void Release()
    {
        if (m_MoveArrowObject != null)
        {
            GameObject.DestroyImmediate(m_MoveArrowObject);
            m_MoveArrowObject = null;
        }
    }

    public void Execute()
    {

    }
}