using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ActorAnimation : IActorComponent
{
    private Actor                              m_Actor;
    private Animator                           m_Animator;
    private AnimatorStateInfo                  m_AnimatorState;
    private RuntimeAnimatorController          m_RuntimeAnimatorController;
    private float                              m_PlaySpeed  = 1f;
    private float                              m_PlayTimer  = 0f;
    private float                              m_CurAnimLength = 0f;
    private Callback                           m_CurFinishCallback;
    private string                             m_CurAnimName;
    private bool                               m_IsStart;
    private bool                               m_IsLoop;
    private bool                               m_IsFinish;
    private Dictionary<string, AnimationClip>  m_AnimClips      = new Dictionary<string, AnimationClip>();
    private static HashSet<string>             m_NoFadeList     = new HashSet<string> { "idle", "run", "walk", "fly" };

    public void Initial(Actor actor)
    {
        this.m_Actor = actor;
        this.m_Animator = actor.Obj.GetComponent<Animator>();
        this.m_Animator.enabled = true;
        this.m_Animator.applyRootMotion = false;
        this.m_Animator.cullingMode = AnimatorCullingMode.CullUpdateTransforms;
        this.m_RuntimeAnimatorController = m_Animator.runtimeAnimatorController;
        if (m_Animator == null || m_RuntimeAnimatorController == null)
        {
            return;
        }
        for (int i = 0; i < m_RuntimeAnimatorController.animationClips.Length; i++)
        {
            AnimationClip clip = m_RuntimeAnimatorController.animationClips[i];
            m_AnimClips[clip.name] = clip;
        }
        m_IsStart = false;
    }

    public void Execute()
    {
        if (!m_Animator.enabled)
        {
            return;
        }
        m_AnimatorState = m_Animator.GetNextAnimatorStateInfo(0);
        if (!m_IsStart)
        {
            if (m_AnimatorState.IsName(m_CurAnimName))
            {
                m_Animator.SetBool(m_CurAnimName, false);
                m_IsStart = true;
            }
        }

        if (Time.time - m_PlayTimer > m_CurAnimLength * m_PlaySpeed)
        {
            if (m_CurFinishCallback != null)
            {
                m_CurFinishCallback();
                m_CurFinishCallback = null;
            }
            m_IsFinish = true;
        }
    }

    public void Release()
    {
        
    }

    public void  Play(string animName, Callback onFinish = null, bool isLoop = false, float speed = 1f, float lastTime = 0f)
    {
        if (m_Animator == null || !m_Animator.enabled || string.IsNullOrEmpty(animName))
        {
            return;
        }

        if(m_CurAnimName == animName && m_NoFadeList.Contains(animName))
        {
            //如果是走，跑，飞，这种状态动作，断续播就行了。
            //否则就要立即播新的动画
            return;
        }

        if (!string.IsNullOrEmpty(m_CurAnimName))
        {
            m_Animator.SetBool(m_CurAnimName, false);
        }

        m_Animator.SetBool(animName, true);
        if (!m_NoFadeList.Contains(m_CurAnimName) && isLoop == false)
        {
            m_Animator.CrossFade(animName, 0.3f);
        }
        m_PlayTimer = Time.time;
        m_PlaySpeed = speed;
        m_CurAnimName = animName;
        m_CurFinishCallback = onFinish;
        m_CurAnimName = animName;
        m_IsStart = false;
        m_IsFinish = false;
        m_IsLoop = isLoop;
        m_CurAnimLength = lastTime > 0 ? lastTime : GetLength(m_CurAnimName);
    }

    public void  Play(string[] animQueue, Callback onFinish, float speed = 1f)
    {
        GTCoroutinueManager.Instance.StartCoroutine(PlayQueue(animQueue, onFinish, speed));
    }

    IEnumerator  PlayQueue(string[] animQueue, Callback onFinish, float speed = 1f)
    {
        for (int i = 0; i < animQueue.Length; i++)
        {
            Play(animQueue[i], null, false, speed);
            while (!m_IsFinish)
            {
                yield return null;
            }
        }
        if (onFinish != null)
        {
            onFinish.Invoke();
        }
    }

    public void  Break()
    {
        m_CurFinishCallback = null;
        if (string.IsNullOrEmpty(m_CurAnimName))
        {
            return;
        }
        if (m_Animator != null)
        {
            m_Animator.SetBool(m_CurAnimName, false);
        }
    }

    public void  SetSpeed(float speed)
    {
        m_PlaySpeed = speed;
    }

    public float GetLength(string animName)
    {
        if (m_AnimClips.ContainsKey(animName))
        {
            return m_AnimClips[animName].length;
        }
        return 0;
    }

    public float GetSpeed()
    {
        return m_PlaySpeed;
    }
}