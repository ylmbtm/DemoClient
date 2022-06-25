using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Protocol;

public class SyncNode
{
    public Vector3 Pos;
    public EActionType ActType;
    public Quaternion SyncRotation;

}


public class ActorMoveSync : IActorComponent
{
    private Actor             m_Actor;
    private List<SyncNode>     m_SyncMoveList    = new List<SyncNode>();
    private float             m_LerpRate;
    private float             m_SlerpRate;
    private float             m_SyncMinDistance = 0.1f;
    private float             m_NormalLerpRate  = 16f;
    private float             m_FasterLerpRate  = 32f;
    private float             m_NormalSlerpRate = 4;
    private float             m_FasterSlerpRate = 8;
    private EActionType       m_LastAction = EActionType.AT_IDLE;

    public void Initial(Actor actor)
    {
        m_Actor = actor;
    }

    public void Execute()
    {
        if (m_Actor.IsDead())
        {
            m_SyncMoveList.Clear();
            m_LastAction = EActionType.AT_DEAD;
        }

        if (m_SyncMoveList.Count > 0)
        {
            if (m_SyncMoveList[0].ActType != (EActionType)m_Actor.FSM)
            {
                m_Actor.DoAction((FSMState)m_SyncMoveList[0].ActType);
            }

            Quaternion oriRotation = m_Actor.CacheTransform.rotation;
            m_Actor.CacheTransform.rotation = Quaternion.Slerp(oriRotation, m_SyncMoveList[0].SyncRotation, Time.deltaTime * m_SlerpRate);
            m_Actor.Pos = Vector3.Lerp(m_Actor.Pos, m_SyncMoveList[0].Pos, Time.deltaTime * m_LerpRate);
            if (Vector3.Distance(m_Actor.Pos, m_SyncMoveList[0].Pos) < m_SyncMinDistance)
            {
                m_Actor.Pos = m_SyncMoveList[0].Pos;
                m_Actor.CacheTransform.rotation = m_SyncMoveList[0].SyncRotation;
                m_SyncMoveList.RemoveAt(0);
            }

            if (m_SyncMoveList.Count > 1)
            {
                m_LerpRate = m_FasterLerpRate;
                m_SlerpRate = m_FasterSlerpRate;
            }
            else if (m_SyncMoveList.Count == 1)
            {
                m_LerpRate = m_NormalLerpRate;
                m_SlerpRate = m_NormalSlerpRate;
            }
            else
            {
                //如果己经没有需要同步的点，则以最后的状态持续,但如果最后的状态是移动则继续移动
                if (m_Actor.FSM == FSMState.FSM_WALK || m_Actor.FSM == FSMState.FSM_RUN || m_Actor.FSM == FSMState.FSM_FLY)
                {
                    m_Actor.DoMoveForward();
                }
            }
        }
        else
        {
            //如果己经没有需要同步的点，则以最后的状态持续,但如果最后的状态是移动则继续移动
            if (m_Actor.FSM == FSMState.FSM_WALK || m_Actor.FSM == FSMState.FSM_RUN || m_Actor.FSM == FSMState.FSM_FLY)
            {
                m_Actor.DoMoveForward();
            }
        }
    }

    public void Release()
    {
        m_SyncMoveList.Clear();
    }

    public void SyncMove(Vector3 target, Vector3 euler, EActionType eActType)
    {
        if (m_Actor.ControlID == GTData.Main.GUID || m_Actor.GUID == GTData.Main.GUID)
        {
            //控制人是自己，这个不能同步位置
            return;
        }

        SyncNode tNode = new SyncNode();
        tNode.Pos = target;
        tNode.SyncRotation = Quaternion.Euler(euler);
        tNode.ActType = eActType;

        if (EActionType.AT_NONE == eActType)
        {
            if (m_SyncMoveList.Count > 0)
            {
                tNode.ActType = m_SyncMoveList[0].ActType;
            }
            else
            {
                tNode.ActType = (EActionType)m_Actor.FSM;
            }
        }

        this.m_SyncMoveList.Add(tNode);
    }
}
