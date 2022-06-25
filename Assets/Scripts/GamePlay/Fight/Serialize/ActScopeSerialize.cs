#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;
using UnityEditor;

namespace ACT
{
    public partial class ActScope : IDrawInspector
    {
       private List<ActBeatFly>         BeatFlys;
       private List<ActFloating>        Floatings;
       private List<Int32>              IndexList;
       private List<string>             TypeList;
       private List<ActResult>          HasOpens = new List<ActResult>();

       private int m_SkillIndex;      //当前技能索引
       private int m_ResultIndex = -1;    //当前结果索引

        string[] ActionNames = { "击中", "击倒", "击退", "击飞" };
        int[] ActionIDs = { 13, 15, 14, 16 };
        public void OnBeforeSerialize()
        {
            BeatFlys       = new List<ActBeatFly>();
            Floatings      = new List<ActFloating>();
            IndexList      = new List<int>();
            TypeList       = new List<string>();

            for (int i = 0; i < Results.Count; i++)
            {
                ActResult item = Results[i];
                BeforeSerialize(BeatFlys,        item);
                BeforeSerialize(Floatings,       item);
            }
        }

        public void OnAfterDeserialize()
        {
            Results.Clear();
            for (int i = 0; i < TypeList.Count; i++)
            {
                string typeName = TypeList[i];
                int    index    = IndexList[i];
                AfterDeserialize(typeName, index, BeatFlys);
                AfterDeserialize(typeName, index, Floatings);
            }
            BeatFlys = null;
            Floatings = null;
        }

        public void BeforeSerialize<T>(List<T> list, ActResult result) where T: ActResult
        {
            if (result is T)
            {
                TypeList.Add(result.GetType().Name);
                IndexList.Add(list.Count);
                list.Add(result as T);
            }
        }

        public void AfterDeserialize<T>(string typeName,int index, List<T> list) where T : ActResult
        {
            if (typeName == typeof(T).Name && list != null)
            {
                Results.Add(list[index]);
            }
        }

        public void DrawInspector()
        {
            this.StTime          = EditorGUILayout.FloatField("开始时间", StTime);
            this.EdTime          = EditorGUILayout.FloatField("结束时间", EdTime);
            this.HitActionID     = EditorGUILayout.IntPopup("被击动作: ", HitActionID, ActionNames, ActionIDs);
            this.HitEffectID     = EditorGUILayout.IntField("被击特效ID", HitEffectID);
            this.HitDistance     = EditorGUILayout.FloatField("被击移动距离", HitDistance);
            this.RangeType       = (ERangeType)EditorGUILayout.EnumPopup("作用范围类型", RangeType);
            switch (RangeType)
            {
                case ERangeType.ERT_CYLINDER:
                    GTTools.AdaptListCount(ref RangeParams, 5);
                    this.RangeParams[0] = EditorGUILayout.FloatField("Radius",    this.RangeParams[0]);
                    this.RangeParams[1] = EditorGUILayout.FloatField("HAngle",    this.RangeParams[1]);
                    this.RangeParams[2] = EditorGUILayout.FloatField("Height",    this.RangeParams[2]);
                    this.RangeParams[3] = EditorGUILayout.FloatField("OffsetX",   this.RangeParams[3]);
                    this.RangeParams[4] = EditorGUILayout.FloatField("OffsetZ",   this.RangeParams[4]);                  
                    break;
                case ERangeType.ERT_BOX:
                    GTTools.AdaptListCount(ref RangeParams, 5);
                    this.RangeParams[0] = EditorGUILayout.FloatField("Length",    this.RangeParams[0]);
                    this.RangeParams[1] = EditorGUILayout.FloatField("Width",     this.RangeParams[1]);
                    this.RangeParams[2] = EditorGUILayout.FloatField("Height",    this.RangeParams[2]);
                    this.RangeParams[3] = EditorGUILayout.FloatField("OffsetX",   this.RangeParams[3]);
                    this.RangeParams[4] = EditorGUILayout.FloatField("OffsetZ",   this.RangeParams[4]);
                    break;
                case ERangeType.ERT_CIRCLE:
                    GTTools.AdaptListCount(ref RangeParams, 4);
                    this.RangeParams[0] = EditorGUILayout.FloatField("Radius",    this.RangeParams[0]);
                    this.RangeParams[1] = EditorGUILayout.FloatField("RanNum",    this.RangeParams[1]);
                    this.RangeParams[2] = EditorGUILayout.FloatField("OffsetX",   this.RangeParams[2]);
                    this.RangeParams[3] = EditorGUILayout.FloatField("OffsetZ",   this.RangeParams[3]);
                    break;
               
            }
            GUILayout.Space(15);
            GUILayout.BeginHorizontal();
            GUI.color = Color.green;
            if (GUILayout.Button("添加子弹", EGUIStyles.ToolbarButton1))
            {
                //EEditorHelper.ShowMenu(typeof(ActResult), (type) =>
                //{
                //    ActResultClassAttribute attr = GTTools.GetAttribute<ActResultClassAttribute>(type);
                //    return (attr != null) ? string.Format("{0}/{1}", attr.Category, attr.Name) : string.Empty;
                // },
                //(obj) =>
                //{
                    AddChild(typeof(ActFlyObject));
                //});
            }
            GUILayout.Space(5);
            if (GUILayout.Button("删除子弹", EGUIStyles.ToolbarButton1))
            {
                //EEditorHelper.ShowMenu(typeof(ActResult), (type) =>
                //{
                //    ActResultClassAttribute attr = GTTools.GetAttribute<ActResultClassAttribute>(type);
                //    return (attr != null) ? string.Format("{0}/{1}", attr.Category, attr.Name) : string.Empty;
                // },
                //(obj) =>
                //{
                if (m_ResultIndex >= 0 && m_ResultIndex < Results.Count)
                {
                    DelChild(Results[m_ResultIndex]);
                }
                //});
            }
            GUILayout.EndHorizontal();
            GUI.color = Color.white;
            GUILayout.Space(10);
            for (int i = 0; i < Results.Count; i++)
            {
                GUILayout.Space(5);

                if(i == m_ResultIndex)
                {
                    GUI.color = Color.cyan;
                }
                else
                {
                    GUI.color = Color.white;
                }

                bool open = false;
                ActResult result = Results[i];
                if (HasOpens.Contains(result))
                {
                    open = true;
                }
                ActResultClassAttribute attr = GTTools.GetAttribute<ActResultClassAttribute>(result.GetType());
                string itemName = attr == null ? Results[i].GetType().Name : attr.Name;
                bool bToggle = GUILayout.Toggle(open, itemName, EGUIStyles.ToolbarDropDown1);
                GUI.color = Color.white;
                if (bToggle)
                {
                    if (open == false)
                    {
                        m_ResultIndex = i;
                        HasOpens.Add(result);
                    }
                    EEditorHelper.DrawObjectInspector(Results[i]);
                }
                else
                {
                    if (open)
                    {
                        HasOpens.Remove(result);
                    }
                }
            }
        }
    }
}
#endif