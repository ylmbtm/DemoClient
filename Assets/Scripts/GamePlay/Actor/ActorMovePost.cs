using UnityEngine;
using System.Collections;
using System;
using Protocol;

public class ActorMovePost : IActorComponent
{
    private Actor       m_Actor;
    private bool        m_IsMove;
    private float       m_SyncInterval;

    private float       m_SyncedTime;
    private float       m_SyncedFace;   //己同步出去的朝向
    private Vector3     m_SyncedPos;    //己同步的位置
    private FSMState    m_SyncedState;  //己同步的状态

    public void Initial(Actor actor)
    {
        m_Actor        = actor;
        m_SyncedTime   = 0;
        m_SyncInterval = 0.5f;
    }

    public void Execute()
    {
        if (m_Actor.ControlID != GTData.Main.GUID && m_Actor.GUID != GTData.Main.GUID)
        {
            //控制人不是自己，所以不能同步信息出去
            return;
        }

        //这个MovePost主要同步称动和停止移动事件，其它的事件都有专门的消息来发送
        if (!m_Actor.IsMove() && !m_Actor.IsIdle())
        {
			m_SyncedState   = m_Actor.FSM;
            return;
        }

        //状态改变需要同步出去
        if(m_SyncedState != m_Actor.FSM)
        {
            GTNetworkSend.Instance.TrySyncAction((EActionType)m_Actor.FSM, m_Actor.GUID, m_Actor.Pos, m_Actor.Face);
            m_SyncedState   = m_Actor.FSM;
            m_SyncedFace    = m_Actor.Face;
            m_SyncedPos     = m_Actor.Pos;
            m_SyncedTime    = Time.realtimeSinceStartup;
            return;
        }

        //方向发生改变，需要同步出去
        if(m_SyncedFace != m_Actor.Face)
        { 
            if (Math.Abs(m_SyncedFace- m_Actor.Face) > 30 || Time.realtimeSinceStartup - m_SyncedTime > 0.2)
            {
                GTNetworkSend.Instance.TrySyncAction((EActionType)m_Actor.FSM, m_Actor.GUID, m_Actor.Pos, m_Actor.Face);
                m_SyncedState = m_Actor.FSM;
                m_SyncedFace = m_Actor.Face;
                m_SyncedPos = m_Actor.Pos;
                m_SyncedTime = Time.realtimeSinceStartup;
            }
        }

        if(m_Actor.IsMove())
        {
            if((Time.realtimeSinceStartup - m_SyncedTime) >= m_SyncInterval)
            {
                GTNetworkSend.Instance.TrySyncAction((EActionType)m_Actor.FSM, m_Actor.GUID, m_Actor.Pos, m_Actor.Face);
                m_SyncedState   = m_Actor.FSM;
                m_SyncedFace    = m_Actor.Face;
                m_SyncedPos     = m_Actor.Pos;
                m_SyncedTime    = Time.realtimeSinceStartup;
                return;
            }
        }
    }

    public void Release()
    {

    }
}