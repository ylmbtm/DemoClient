using UnityEngine;
using System.Collections;
using System;

namespace ACT
{
    [ActActionClass("动作", "基类", "")]
    [Serializable]
    public class ActItem : ActNode
    {
        public float StTime = 0.00f;
        public float EdTime = 0.00f;

        public EACTS          Status   { get; private set; }
        public float          StatTime { get; private set; }
        public float          PastTime { get { return Time.realtimeSinceStartup - StatTime; } }
        public float          Duration { get { return EdTime - StTime; } }
        public ActSkill       Skill    { get; set; }

        protected virtual void Startup()
        {
            this.StatTime = Time.realtimeSinceStartup;
        }

        protected virtual bool Trigger()
        {
            return true;
        }

        public virtual ActItem Clone()
        {
            return this;
        }

        protected virtual void Execute()
        {

        }

        protected virtual void End()
        {

        }

        protected virtual void Release()
        {

        }

        public void Loop()
        {
            if (Status == EACTS.INITIAL)
            {
                Startup();
                Status = EACTS.STARTUP;
            }
            if (Status == EACTS.STARTUP)
            {
                if (PastTime >= StTime)
                {
                    Trigger();
                    Status = EACTS.RUNNING;
                }
            }
            if (Status == EACTS.RUNNING)
            {
                Execute();
                if (PastTime >= EdTime)
                {
                    End();
                }
            }
            if (Status == EACTS.SUCCESS)
            {
                Release();
            }
        }

        public void Stop()
        {
            if (Status != EACTS.INITIAL)
            {
                Release();
                Status = EACTS.INITIAL;
            }
        }

        public void Exit()
        {
            if (Status != EACTS.INITIAL)
            {
                Release();
                Status = EACTS.INITIAL;
            }
        }
    }
}