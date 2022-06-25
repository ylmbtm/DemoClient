using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using CFG;

namespace MAP
{
    [MAPActionClass("触发/波次", true)]
    public class MAPWave : MAPElement, IMAPContainer
    {
        [MAPFieldAttri] public List<MAPMonster> Monsters        = new List<MAPMonster>();
        public Vector4 TriggerBox;
        public int TriggerTime;
        public ETriggerType TriggerType = ETriggerType.ETT_NORMAL;

        public override void Trigger()
        {
            base.Trigger();
        }

        public override void Import(DCFG cfg)
        {
            DTWave data = cfg as DTWave;
            this.ID          = data.ID;
            this.TriggerType = (ETriggerType)data.TriggerType;
            this.TriggerBox = data.TriggerBox;
            this.TriggerTime = data.TriggerTime;
            for (int i = 0; i < data.Monsters.Count; i++)
            {
                DTMonster d   = data.Monsters[i];
                MAPMonster g   = new GameObject().AddComponent<MAPMonster>();
                g.transform.parent = transform;
                g.Import(d);
                Monsters.Add(g);
            }
        }

        public override DCFG Export()
        {
            DTWave data = new DTWave();
            data.ID         = this.ID;
            data.TriggerType = (int)this.TriggerType;
            data.TriggerBox = this.TriggerBox;
            data.TriggerTime = this.TriggerTime;
            for (int i = 0; i < Monsters.Count; i++)
            {
                MAPMonster   g = Monsters[i];
                DTMonster   d = (DTMonster)g.Export();
                data.Monsters.Add(d);
            }
            return data;
        }

        public void          OnGainElementOnGround(Vector3 pos, Vector3 eulerAngles)
        {
            MAPMonster t        = new GameObject(typeof(MAPMonster).Name).AddComponent<MAPMonster>();
            t.transform.parent      = transform;
            t.transform.position    = pos;
            t.transform.eulerAngles = eulerAngles;
            Monsters.Add(t);
        }

        public override void OnDrawInspector()
        {
#if UNITY_EDITOR
            GUILayout.Space(10);
            this.ID = UnityEditor.EditorGUILayout.IntField("ID", this.ID);
            GUILayout.Space(10);
            this.TriggerType = (ETriggerType)UnityEditor.EditorGUILayout.EnumPopup("TriggerType", this.TriggerType);
            switch (this.TriggerType)
            {
                case ETriggerType.ETT_NORMAL:
                    {
                    }
                    break;

                case ETriggerType.ETT_TRIBOX:
                    {
                        this.TriggerBox = UnityEditor.EditorGUILayout.Vector4Field("TriggerBox", this.TriggerBox);
                    }
                    break;

                case ETriggerType.ETT_TIME:
                    {
                        this.TriggerTime = UnityEditor.EditorGUILayout.IntField("TriggerTime", this.TriggerTime);
                    }
                    break;
            }

            
            GUILayout.Space(10);
            GUI.color = Color.green;

            if (GUILayout.Button("添加元素", EGUIStyles.Button1, GUILayout.Height(35)))
            {
                OnGainElementOnGround(Vector3.zero, Vector3.zero);
            }
            GUI.color = Color.green;
            GUILayout.Space(10);
            if (GUILayout.Button("元素贴地", EGUIStyles.Button1, GUILayout.Height(35)))
            {
               OnMoveElementToGround();
            }
            GUILayout.Space(10);
            GUILayout.Space(5);
#endif
        }

        public override void OnMoveElementToGround()
        {
            for (int i = 0; i < Monsters.Count; i++)
            {
                Monsters[i].OnMoveElementToGround();
            }
        }

        public override void OnDrawGizmos()
        {
            base.OnDrawGizmos();
            for (int i = 0; i < Monsters.Count; i++)
            {
                if(Monsters[i] == null)
                {
                    Monsters.RemoveAt(i);
                    break;
                }
            }
        }

        void Start()
        {

        }

        void OnDestroy()
        {

        }
    }
}