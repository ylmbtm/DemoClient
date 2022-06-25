using UnityEngine;
using System.Collections;
using UnityEngine.AI;
using System;

public class ActorPathFinding : IActorComponent
{
    private NavMeshPath     m_NavMeshPath   = new NavMeshPath();
    private NavMeshAgent    m_NavMeshAgent  = null;
    private Actor           m_Actor         = null;
    private bool            m_IsFindPath    = false;
    private bool m_IsStartPath = false;
    private float m_LastRemainningDis = 0.0f;
    public Vector3          Destination       { get; set; }
    public Callback         DestReachCallback { get; set; }

    public void Initial(Actor actor)
    {
        this.m_Actor = actor;
        this.SetHeight(actor.Height, actor.Radius, actor.Center);
        this.SetSpeed(actor.CurSpeed);
    }

    public void Execute()
    {
        if (m_IsFindPath == false)
        {
            return;
        }

        if (IsReached())
        {
            if (DestReachCallback != null)
            {
                DestReachCallback();
            }
            this.Stop();
        }
    }

    public void Release()
    {

    }

    public NavMeshHit GetRaycastHit(Vector3 targetPosition)
    {
        NavMeshHit NavHit;
        m_NavMeshAgent.Raycast(targetPosition, out NavHit);
        return NavHit;
    }
   

    public void SetDestination(Vector3 destPosition, Callback reachCallback)
    {
        Destination = destPosition;
        DestReachCallback = reachCallback;
        m_NavMeshAgent.enabled = true;
        m_NavMeshAgent.SetDestination(Destination);
        m_LastRemainningDis = 0;
        m_IsFindPath = true;
        m_IsStartPath = false; 
    }

    public void Stop()
    {
        m_IsFindPath = false;

        if (m_NavMeshAgent == null)
        {
            return;
        }

        if (m_NavMeshAgent.enabled == true)
        {
            m_NavMeshAgent.isStopped = true;
            m_NavMeshAgent.enabled = false;
        }
    }

    public void SetHeight(float height, float radius, Vector3 center)
    {
        m_NavMeshPath = new NavMeshPath();
        m_NavMeshAgent = m_Actor.gameObject.GET<NavMeshAgent>();
        m_NavMeshAgent.enabled = false;
        m_NavMeshAgent.acceleration = 360;
        m_NavMeshAgent.angularSpeed = 3600;
        m_NavMeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        m_NavMeshAgent.height = height;
        m_NavMeshAgent.radius = radius;
    }

    public void SetSpeed(float speed)
    {
        if (m_NavMeshAgent == null)
        {
            return;
        }

        m_NavMeshAgent.speed = speed;
        
    }

    public bool IsReached()
    {
        if (!m_NavMeshAgent.enabled)
        {
            return false;
        }

        if(!m_IsStartPath)
        {
            m_LastRemainningDis = m_NavMeshAgent.remainingDistance;

            if(m_LastRemainningDis > 0.001)
            {
                m_IsStartPath = true;
            }

            return false;
        }

  
        if(m_NavMeshAgent.remainingDistance == m_LastRemainningDis && m_NavMeshAgent.remainingDistance < 0.01f)
        {
            return true;
        }

        m_LastRemainningDis = m_NavMeshAgent.remainingDistance;
        
        return false;
    }

    public bool IsCanReachPosition(Vector3 dest)
    {
        Vector3 position = GTTools.NavSamplePosition(dest);
        m_NavMeshAgent.enabled = true;
        m_NavMeshAgent.CalculatePath(position, m_NavMeshPath);
        if (m_NavMeshPath.status != NavMeshPathStatus.PathPartial)
        {
            return true;
        }
        m_NavMeshAgent.enabled = false;
        return false;
    }
}
