using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using CON;

namespace MAP
{
    public class MAPAction : MAPElement
    {
        [MAPFieldAttri]
        public List<CheckBase> Conditions = new List<CheckBase>();

        public override void Startup()
        {
            this.State = ENTS.TRIGGER;
        }

        public override void Trigger()
        {
            GTEventCenter.FireEvent(GTEventID.TYPE_MAP_EVENT, ID);
            this.State = ENTS.RUNNING;
        }

        public override void Release()
        {
            this.State = ENTS.SUCCESS;
        }

        public override void Execute()
        {

        }

        void Update()
        {
            if (State == ENTS.TRIGGER)
            {
                if (Check())
                {
                    Trigger();
                }
            }
            if (State == ENTS.RUNNING)
            {
                Execute();
            }
        }

        bool Check()
        {
            for (int i = 0; i < Conditions.Count; i++)
            {
                if (Conditions[i].Check() == false)
                {
                    return false;
                }
            }
            return true;
        }
    }
}