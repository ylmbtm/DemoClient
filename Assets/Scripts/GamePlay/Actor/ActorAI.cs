using UnityEngine;
using System.Collections;
using System;
using ACT;
using System.Collections.Generic;
using behaviac;

public class ActorAI : behaviac.Agent, IActorComponent
{
	private Actor           m_Actor;
	private ActorCommand    m_ActorCommand;
	private ActorSkill      m_ActorSkill;
	private float           m_LastTime = Time.realtimeSinceStartup;


	//ai 变量
	private ActSkill m_CurSelSkill = null;
	public void Initial(Actor actor)
	{
		this.m_Actor        = actor;
		this.m_ActorCommand = actor.Get<ActorCommand>();
		this.m_ActorSkill   = actor.Get<ActorSkill>();

		//主角对象，也不需要AI
		if (m_Actor.GUID == GTData.Main.GUID)
		{
			return;
		}

		string strNodeName = "Ai_Pet";
		if(m_Actor.Type == Protocol.EObjectType.OT_PET)
		{
			strNodeName = "Ai_Pet";
		}
		else if (m_Actor.Type == Protocol.EObjectType.OT_PARTNER)
		{
			strNodeName = "Ai_Partner";
		}
		else if (m_Actor.Type == Protocol.EObjectType.OT_MONSTER)
		{
			strNodeName = "Ai_Monster";
		}

		bool bRet = this.btload(strNodeName);
		if (bRet)
		{
			this.btsetcurrent(strNodeName);
		}
	}

	public void Execute()
	{
		//1s思考一次
		if((Time.realtimeSinceStartup - m_LastTime) < 1)
		{
			return;
		}

		//控制不是自己, 不需要AI
		if (m_Actor.ControlID != GTData.Main.GUID)
		{
			return;
		}

		//主角对象，也不需要AI
		if (m_Actor.GUID == GTData.Main.GUID)
		{
			return;
		}

		//己死亡也不需要AI
		if(m_Actor.FSM == FSMState.FSM_DEAD)
		{
			return;
		}

		if( (m_Actor.Type == Protocol.EObjectType.OT_PET))
		{
			return;
		}
		m_LastTime = Time.realtimeSinceStartup;
		//this.btexec();
	}

	public void Release()
	{

	}

	public bool Ai_SelectEnemy()
	{
		if (m_Actor.Target != null)
		{
			return true;
		}

		if((m_Actor.Type == Protocol.EObjectType.OT_PET) || (m_Actor.Type == Protocol.EObjectType.OT_PARTNER))
		{
			m_Actor.Target = m_Actor.Host.Target;
			return true;
		}

		Actor selectTarget = m_Actor.GetTargetByPolicy(EAffect.Enem, ESelectTargetPolicy.TYPE_SELECT_BY_LESSDISTANCE);
		if (selectTarget == null)
		{
			return false;
		}

		GTWorld.Instance.SetTarget(m_Actor, selectTarget);

		return true;
	}

	public bool Ai_HasEnemy()
	{
		if (m_Actor.Target != null)
		{
			return true;
		}

		Actor selectTarget = m_Actor.GetTargetByPolicy(EAffect.Enem, ESelectTargetPolicy.TYPE_SELECT_BY_LESSDISTANCE);
		if (selectTarget == null)
		{
			return false;
		}

		GTWorld.Instance.SetTarget(m_Actor, selectTarget);

		return true;
	}

	public bool Ai_SelectSkill()
	{
		m_CurSelSkill = null;
		if (m_Actor.Target == null)
		{
			return false;
		}

		Vector3 pos = m_Actor.Target.Pos;
		Vector3 dir = m_Actor.Pos - pos;
		dir.y = 0;
		float dirSqr = dir.sqrMagnitude;

		for (int i = 0; i < m_ActorSkill.SpecialSkills.Count; i++)
		{
			ActSkill skill = m_ActorSkill.SpecialSkills[i];

			if (skill.CastDistance * skill.CastDistance > dirSqr)
			{
				m_CurSelSkill = skill;
				return true;
			}
		}

		if (m_ActorSkill.GeneralSkills.Count > 0)
		{
			ActSkill skill = m_ActorSkill.GeneralSkills[0];
			if (skill.CastDistance * skill.CastDistance > dirSqr)
			{
				m_CurSelSkill = skill;
				return true;
			}
		}

		return false;
	}

	public bool Ai_HasAvailableSkill()
	{
		if(m_CurSelSkill == null)
		{
			return false;
		}

		return true;
	}

	public EBTStatus Ai_Attack()
	{
		Resp resp = m_Actor.CanUseSkill(m_CurSelSkill);
		if (resp != Resp.TYPE_YES)
		{
			return EBTStatus.BT_SUCCESS;
		}

		m_Actor.DoPreSkill();
		m_Actor.FaceTarget(0);
		List<Actor> targets = m_Actor.Target == null ? null : new List<Actor>()
		{
			m_Actor.Target
		};
		GTNetworkSend.Instance.TryCastSkill(m_CurSelSkill.ID, m_Actor, targets, Vector3.zero);

		if(m_Actor.FSM == FSMState.FSM_SKILL || m_Actor.FSM == FSMState.FSM_PRE_SKILL)
		{
			return EBTStatus.BT_RUNNING;
		}

		return EBTStatus.BT_SUCCESS;
	}

	public bool Ai_MoveToEnemy()
	{
		if(m_Actor.Target == null)
		{
			return false;
		}

		if (GTTools.GetHorizontalDistance(m_Actor.Pos, m_Actor.Target.Pos) <= 1.0f)
		{
			if (m_Actor.IsMove())
			{
				m_ActorCommand.GetCmd<CommandIdle>().Do();
			}

			return true;
		}

		if(!m_Actor.CanMove())
		{
			return false;
		}

		//找不到目标，执行移动
		m_ActorCommand.GetCmd<CommandMove>().Update(m_Actor.Target, () =>
		{
			m_ActorCommand.GetCmd<CommandIdle>().Do();
		}).Do();

		return true;
	}

	public void Ai_FollowHost()
	{
		//己经太近
		if (GTTools.GetHorizontalDistance(m_Actor.Pos, m_Actor.Host.Pos) <= 1.0f)
		{
			if (m_Actor.IsMove())
			{
				m_ActorCommand.GetCmd<CommandIdle>().Do();
			}

			return;
		}

		if (!m_Actor.CanMove())
		{
			return ;
		}

		if(m_Actor.Host == null)
		{
			return ;
		}

		//找不到目标，执行移动
		m_ActorCommand.GetCmd<CommandMove>().Update(m_Actor.Host, () =>
		{
			m_ActorCommand.GetCmd<CommandIdle>().Do();
		}).Do();

		return ;
	}

	public bool Ai_ToFarFromHost()
	{
		//己经太远
		if (GTTools.GetHorizontalDistance(m_Actor.Pos, m_Actor.Host.Pos) >= 10.0f)
		{
			return true;
		}

		return false;
	}
	public void Ai_CancelEnmey()
	{
		GTWorld.Instance.SetTarget(m_Actor, null);
	}

//     public Resp AIAutoTakeHpDrug()
//     {
//         if (GTData.RemoteData.HPDrugItemID == 0)
//         {
//             return Resp.TYPE_ACTOR_NOTSET_HPDRUG;
//         }
//         if (m_Actor.GetHpPercent() > (GTData.RemoteData.HPDrugPercent / 100f))
//         {
//             return Resp.TYPE_NO;
//         }
//         XItem data = GTData.Instance.GetItemDataById(GTData.RemoteData.HPDrugItemID);
//         if (data == null)
//         {
//             return Resp.TYPE_ITEM_NULL;
//         }
//         if (data.Num < 1)
//         {
//             return Resp.TYPE_ITEM_LACK;
//         }
//         DItem db = ReadCfgItem.GetDataById(GTData.RemoteData.HPDrugItemID);
//         if (db.Data1 > 0 && GTTools.GetUtcTime() - data.CDStamp < db.Data1)
//         {
//             return Resp.TYPE_ITEM_CD;
//         }
//         GTNetworkSend.Instance.TryUseItemByPos(data.Pos, 1);
//         return Resp.TYPE_YES;
//     }
//
//     public Resp AIAutoTakeMpDrug()
//     {
//         if (GTData.RemoteData.MPDrugItemID == 0)
//         {
//             return Resp.TYPE_ACTOR_NOTSET_MPDRUG;
//         }
//         if (m_Actor.GetMpPercent() > (GTData.RemoteData.MPDrugPercent / 100f))
//         {
//             return Resp.TYPE_NO;
//         }
//         XItem data = GTData.Instance.GetItemDataById(GTData.RemoteData.MPDrugItemID);
//         if (data == null)
//         {
//             return Resp.TYPE_ITEM_NULL;
//         }
//         if (data.Num < 1)
//         {
//             return Resp.TYPE_ITEM_LACK;
//         }
//         DItem db = ReadCfgItem.GetDataById(GTData.RemoteData.MPDrugItemID);
//         if (db.Data1 > 0 && GTTools.GetUtcTime() - data.CDStamp < db.Data1)
//         {
//             return Resp.TYPE_ITEM_CD;
//         }
//         GTNetworkSend.Instance.TryUseItemByPos(data.Pos, 1);
//         return Resp.TYPE_YES;
//     }
}