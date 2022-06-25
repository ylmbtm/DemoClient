using UnityEngine;
using System.Collections;
using System;
using Protocol;
using ACT;
using System.Collections.Generic;

namespace FLY
{
    public class FlyObject : MonoBehaviour, IActor
    {
        public ulong             GUID
        {
            get; set;
        }

        public Int32             ID
        {
            get; set;
        }

        public Transform         CacheTransform
        {
            get { return transform; }
        }

        public Vector3           Pos
        {
            get { return transform.position; }
            set { transform.position = value; }
        }

        public Vector3           Euler
        {
            get { return transform.eulerAngles; }
            set { transform.eulerAngles = value; }
        }

        public Vector3           Scale
        {
            get { return transform.localScale; }
            set { transform.localScale = value; }
        }

        public DFlyOjbect DB
        {
            get; private set;
        }

        public GameObject        Go
        {
            get; private set;
        }

        public Actor             CasterActor
        {
            get; private set;
        }

        public Actor             TargetActor
        {
            get; private set;
        }

        protected Vector3        TarPos
        {
            get; private set;
        }

        protected Vector3        NowPos
        {
            get; private set;
        }

        protected Vector3        NowEulerAngles
        {
            get; private set;
        }

        protected float          NowSpeed
        {
            get; set;
        }

        protected float         FlyingAcee
        {
            get; set;
        }

        protected float         InitSpeed
        {
            get; set;
        }

        public float            FlyingHAngle
        {
            get; set;
        }
        public float            FlyingVAngle
        {
            get; set;
        }

        protected float         FlyingBackDistance
        {
            get; set;
        }
        protected float          StartTime
        {
            get; private set;
        }

        protected float          LifeTime
        {
            get; private set;
        }

        protected bool           StartFly
        {
            get; private set;
        }

        protected Transform      CasterBind
        {
            get; private set;
        }

        protected Transform      TargetBind
        {
            get; private set;
        }

        protected AudioSource    Source
        {
            get; private set;
        }

        public void Init(BulletItem data)
        {
            this.GUID = data.ObjectGuid;
            this.ID = data.BulletID;
            this.DB = ReadCfgFlyObject.GetDataById(ID);
            this.NowPos = new Vector3(data.X, data.Y, data.Z);
            this.NowEulerAngles = new Vector3(0, data.Angle, 0);
            this.InitSpeed = data.Speed;
            this.NowSpeed = data.Speed;
            this.FlyingAcee = data.AccSpeed;
            this.CasterActor = GTWorld.Instance.GetActor(data.CasterGuid);
            this.TargetActor = GTWorld.Instance.GetActor(data.TargetGuid);
            this.LifeTime = data.LifeTime;
            this.StartTime = Time.realtimeSinceStartup - (data.LifeTime - data.LeftTime)/1000.0f;
            this.StartFly = true;
            this.Startup();
        }

        public void Release()
        {
            if (StartFly)
            {
                StartFly = false;
            }
            if (Source != null)
            {
                GTAudioManager.Instance.EnqueueEffectAudio(Source);
                Source = null;
            }
            if (Go != null)
            {
                DEffect effectDB = ReadCfgEffect.GetDataById(DB.FlyingEffect);
                GTPoolManager.Instance.ReleaseGo(effectDB.Path, Go);
                Go = null;
            }
        }

        protected virtual void Startup()
        {
            this.Go = GTWorld.Instance.Effect.CreateEffectByID(DB.FlyingEffect);
            this.Go.transform.parent = CacheTransform;
            this.Go.transform.localPosition = Vector3.zero;
            this.Go.transform.localEulerAngles = Vector3.zero;
            if (string.IsNullOrEmpty(DB.FlyingSound) == false)
            {
                this.Source = GTAudioManager.Instance.PlayEffectAudio(DB.FlyingSound, 1, 1, DB.FlyingMusicLoop);
            }
            if (CasterActor != null)
            {
                this.CasterBind = CasterActor.Get<ActorAvator>().GetBindTransform(DB.CasterBind);
            }
            if (TargetActor != null)
            {
                this.TargetBind = TargetActor.Get<ActorAvator>().GetBindTransform(DB.TargetBind);
            }
        }

        protected virtual void Execute()
        {
            if (Time.realtimeSinceStartup -this.StartTime >= this.LifeTime)
            {
                GTWorld.Instance.DelFlyObject(this);
            }
        }
        public void CheckTargetObjects()
        {
            Actor casterActor = this.CasterActor;
            List<Actor> list = casterActor.GetAffectTargets(EAffect.Enem, ESelectTargetPolicy.TYPE_SELECT_DEFAULT);
            switch (DB.RangeType)
            {
                case ERangeType.ERT_CIRCLE:
                    {
                        float radius = DB.RangeParams[0];
                        Vector3 hitDir = casterActor.Dir;
                        Vector3 center = Pos;
                        for (int k = 0; k < list.Count; k++)
                        {
                            Actor actor = list[k];
                            if (ActUtility.IsInCircle(actor, radius, 1, center) == false)
                            {
                                continue;
                            }

                            ProcessImpact(casterActor, actor);

                            GTWorld.Instance.DelFlyObject(this);
                        }
                    }
                    break;
               
               
            }
        }

        protected virtual void ProcessImpact(Actor Castor, Actor Targetor)
        {
            if (DB.FlyingImpact <= 0)
            {
                return;
            }
            if (Targetor != null)
            {
                this.TargetBind = Targetor.Get<ActorAvator>().GetBindTransform(DB.TargetBind);
            }

            if(this.TargetBind == null)
            {
                return;
            }

            GameObject go = GTWorld.Instance.Effect.CreateEffectByID(DB.FlyingImpact);
            if (go == null)
            {
                return;
            }

            go.transform.position = TargetBind.position;
            go.transform.parent = TargetBind;
        }

        void Update()
        {
            if (StartFly)
            {
                Execute();
            }
        }
    }

  
}