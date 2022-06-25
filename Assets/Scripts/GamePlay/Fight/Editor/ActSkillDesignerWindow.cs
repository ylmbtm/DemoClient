using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Linq;
using System.Reflection;
using System.Collections.Generic;

namespace ACT
{
    public struct SkillTimeRect
    {
        public float Time;
        public Rect Rect;
    }

    public class ActSkillDesignerWindow : EditorWindow
    {
        [MenuItem("编辑器/技能编辑器")]
        static void OpenSkillScriptEditor()
        {
            ActSkillDesignerWindow window = GetWindow<ActSkillDesignerWindow>(true, "ActSkillDesignerWindow");
            window.Init();
            window.Show();
        }

        private Rect                         m_SkillRect;       //技能
        private Rect                         m_ActionRect;      //事件属性
        private Rect                         m_ActListRect;     //行为列表
        private Rect                         m_TimeRect;
        private GUISkin                      m_DefaultSkin;
        private GUISkin                      m_BehavSkin;
        private int                          m_SkillIndex;      //当前技能索引
        private float                        m_TimeNum = 20;    //时间格数
        private float                        m_TotalTime = 10.0f;//总时长
        private float                        m_TimeWidth;       //每一格时间的宽度
        private float                        m_TimeMuti;
        private ActItem                      m_CurActItem;
        private SkillTimeRect                m_TimelineControl;
        private bool                         m_TimelineDragged = false;
        Vector2                              m_ScrollPosition = new Vector2(0, 0);
        private ActSkillData                 m_SkillData = new ActSkillData();

        public void Init()
        {
            m_SkillData.LoadFromXml();
            this.titleContent = new GUIContent("技能编辑器");
        }
        private void OnEnable()
        {
            
        }
        private void OnGUI()
        {
            LoadResources();
            m_DefaultSkin = GUI.skin;
            GUI.skin = m_BehavSkin;
            m_SkillRect = new Rect(0, 0, 300, position.height);     //左侧
            m_ActListRect = new Rect(0, 0, 300, position.height);
            m_ActionRect = new Rect(305, 0, 300, position.height);  //中间
            m_TimeRect = new Rect(620, 0, position.width-630, position.height);  //右侧的时间
            m_TimeWidth = m_TimeRect.width / m_TimeNum; //每一个时间间隔的宽度
            m_TimeMuti  = m_TotalTime / m_TimeRect.width; //每一个像素所表示的时间
            m_TimelineControl.Rect = new Rect(m_TimeRect.x + m_TimelineControl.Time / m_TimeMuti, 0, 30, 30);
            EditorGUIUtility.labelWidth = 100;
            DrawSkillProperties();
            DrawActionProperties();
            DrawTimePanel();
            BeginWindows();
            //DrawTimeline();
            //DrawTimelineItems();
            //DrawTimelineControl();
            //HandleEvents();
            //EndWindows();
            
            GUI.skin = m_DefaultSkin;
        }

        private void OnInspectorUpdate()
        {
            base.Repaint();
        }

        public void AddSkill()
        {
            m_SkillData.NewSkill();
        }
        public void DelSkill(ActSkill skill)
        {
            m_SkillData.m_List.Remove(skill);
        }
        private void LoadResources()
        {
            if (m_BehavSkin == null)
            {
                this.m_BehavSkin = Resources.Load("BVTSkin") as GUISkin;
            }
        }

        private void DrawSkillProperties()
        {
            GUILayout.BeginArea(m_SkillRect, "", EBVTStyles.BVT_Window);
            EditorGUILayout.Separator();
            GUILayout.BeginHorizontal();
            GUI.color = Color.green;
            if (GUILayout.Button("新建技能", EGUIStyles.Button2))
            {
                 AddSkill();
            }
            GUILayout.Space(20);
            if (GUILayout.Button("删除技能", EGUIStyles.Button2))
            {
                m_SkillData.SaveToXml();
            }

            GUI.color = Color.white;
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            if (m_SkillData != null)
            {
                if (m_SkillIndex > m_SkillData.m_List.Count - 1 || m_SkillIndex < 0)
                {
                    m_SkillIndex = 0;
                }
                string[] array = new string[m_SkillData.m_List.Count];
                for (int i = 0; i < m_SkillData.m_List.Count; i++)
                {
                    array[i] = string.Format("选择技能: {0}", m_SkillData.m_List[i].ID);
                }
                GUI.color = Color.cyan;
                m_SkillIndex = EditorGUILayout.Popup(m_SkillIndex, array, EGUIStyles.ToolbarDropDown1);
                GUI.color = Color.white;
            }
            else
            {
                m_SkillIndex = -1;
            }
            GUI.color = Color.white;


            GUILayout.Space(20);
            ActSkill current = (m_SkillData == null || m_SkillData.m_List.Count == 0) ? null : m_SkillData.m_List[m_SkillIndex];
            if (current != null)
            {
                current.ID            = EditorGUILayout.IntField("技能ID",current.ID);
                current.Name          = EditorGUILayout.TextField("技能名称", current.Name);
                current.CastDistance  = EditorGUILayout.FloatField("技能释放距离",current.CastDistance);
                current.CastType      = (ESkillCastType)EditorGUILayout.EnumPopup("技能释放类型", current.CastType);
                current.Duration      = EditorGUILayout.FloatField("技能时长", current.Duration); ;

                for (int i = 0; i < current.Items.Count; i++)
                {
                    if (current.Duration < current.Items[i].EdTime)
                    {
                        current.Duration = current.Items[i].EdTime;
                    }
                }
                GUILayout.Space(10);
                GUILayout.BeginHorizontal();
                GUI.color = Color.green;

                if (GUILayout.Button("添加行为", EGUIStyles.ToolbarDropDown1))
                {
                    UnityEditor.GenericMenu menu = new UnityEditor.GenericMenu();
                    var types = AppDomain.CurrentDomain.GetAssemblies().SelectMany(item => item.GetTypes())
                           .Where(item => item.IsSubclassOf(typeof(ActItem))).ToList();
                    for (int i = 0; i < types.Count; i++)
                    {
                        Type t = types[i];
                        ActActionClassAttribute attr = GTTools.GetAttribute<ActActionClassAttribute>(t);
                        if (attr != null)
                        {
                            string menuName = string.Format("{0}/{1}", attr.Category, attr.Name);
                            menu.AddItem(new GUIContent(menuName), false, (userData) =>
                            {
                                m_SkillData.m_List[m_SkillIndex].AddChild(t);
                            }, t);
                        }
                    }
                    menu.ShowAsContext();
                }
                GUILayout.Space(10);
                if (GUILayout.Button("删除行为", EGUIStyles.Button2))
                {
                    m_SkillData.m_List[m_SkillIndex].DelChild(m_CurActItem);
                    m_CurActItem = null;
                }
                GUI.color = Color.white;
                GUILayout.EndHorizontal();
                GUILayout.Space(10);
                float scrollHeight = position.height - 240;
                if(scrollHeight < 50)
                {
                    scrollHeight = 50;
                }
                m_ScrollPosition = GUILayout.BeginScrollView(m_ScrollPosition, GUILayout.Width(300), GUILayout.Height(scrollHeight));
                if (current.Items.Contains(m_CurActItem) == false)
                {
                    m_CurActItem = null;
                }
                if (m_CurActItem == null && current.Items.Count > 0)
                {
                    m_CurActItem = current.Items[0];
                }

                GUI.color = Color.green;
                string[] array = new string[current.Items.Count];            
                for (int i = 0; i < current.Items.Count; i++)
                {
                    ActItem item = current.Items[i];
                    ActActionClassAttribute attr = GTTools.GetAttribute<ActActionClassAttribute>(item.GetType());
                    array[i] = attr == null ? item.GetType().Name : attr.Name;
                    GUI.color = m_CurActItem == item ? Color.green : Color.white;
                    GUI.backgroundColor =  m_CurActItem == item ? Color.green : Color.white;
                    if (GUILayout.Button(array[i], EGUIStyles.Button2))
                    {
                        if (m_CurActItem != item)
                        {
                            m_CurActItem = item;
                        }
                    }
                }
                GUI.color = Color.white;
                GUI.backgroundColor = Color.white;
                GUILayout.EndScrollView();
            }
            GUI.color = Color.green;
            if (GUILayout.Button("全部保存", EGUIStyles.Button2))
            {
                m_SkillData.SaveToXml();
            }
            GUI.color = Color.white;
            GUILayout.EndArea();
        }

        private void DrawActionProperties()
        {
            GUILayout.BeginArea(m_ActionRect, "", EBVTStyles.BVT_Window);
            //BeginWindows();
            EditorGUILayout.LabelField("属性", m_BehavSkin.label);
            GUILayout.Space(20);
            EditorGUILayout.Separator();
            if (m_CurActItem != null)
            {
                if (m_CurActItem is IDrawInspector)
                {
                    (m_CurActItem as IDrawInspector).DrawInspector();
                }
                else
                {
                    EEditorHelper.DrawObjectInspector(m_CurActItem);
                }
            }
            //EndWindows();
            GUILayout.EndArea();
        }

        private void DrawTimePanel()
        {
            //GUILayout.BeginArea(m_TimeRect, "", EBVTStyles.BVT_Window);
            BeginWindows();
            DrawTimeline();
            DrawTimelineItems();
            DrawTimelineControl();
            HandleEvents();
            EndWindows();
            //GUILayout.EndArea();
        }
        private void DrawTimeline()
        {
            for (int i = 0; i < m_TimeNum + 1; i++)
            {
                Vector2 p1 = new Vector2(m_TimeRect.x + i * m_TimeWidth, 25);
                Vector2 p2 = new Vector2(m_TimeRect.x + i * m_TimeWidth, position.height);
                UnityEditor.Handles.color = new Color(0.5f, 0.5f, 0.5f, 0.3f);
                UnityEditor.Handles.DrawLine(p1, p2);
                UnityEditor.Handles.color = Color.white;
                Rect timeRect = new Rect(p1.x - 20, 0, 40, 30);
                float time = i * m_TotalTime / m_TimeNum;
                GUILayout.BeginArea(timeRect);
                GUILayout.Label(time.ToString("0.00"), EGUIStyles.Label1);
                GUILayout.EndArea();
            }
        }

        private void DrawTimelineItems()
        {
            if (m_SkillData == null) return;
            if (m_SkillData.m_List.Count == 0) return;
            ActSkill current = m_SkillData.m_List[m_SkillIndex];
            Event e = Event.current;
            for (int i = 0; i < current.Items.Count; i++)
            {
                Rect trackRect = new Rect(m_TimeRect.x, 30 + i * 50, m_TimeRect.width, 45);
                if (e.button == 0 & e.type == EventType.MouseDown && trackRect.Contains(e.mousePosition))
                {
                    m_CurActItem = current.Items[i];
                    e.Use();
                }
                GUI.color = Color.white;
                GUI.color = m_CurActItem == current.Items[i] ? Color.green : Color.white;
                GUI.Box(trackRect, "", EBVTStyles.BVT_WindowShadow);
                GUI.color = Color.gray;
                ActItem item = current.Items[i];
                ActActionClassAttribute attr = GTTools.GetAttribute<ActActionClassAttribute>(item.GetType());
                string itemName = string.Format("<color=#ffffff>{0}</color>", attr == null ? item.GetType().Name : attr.Name);
                float fw = item.Duration > 0 ? (item.Duration / m_TimeMuti) : 4;
                float fh = 35;
                float fx = m_TimeRect.x+item.StTime / m_TimeMuti;
                float fy = trackRect.y + 6;
                GUIStyle s = item.Duration > 0 ? EBVTStyles.BVT_WindowHighlight : EBVTStyles.BVT_EventItem;
                Rect itemRect = new Rect(fx, fy, fw, fh);
                Rect drawRect = GUI.Window(i, itemRect, (id) =>
                {
                    GUILayout.Space(0);
                    Rect winRect = new Rect(0, 0, itemRect.width, itemRect.height);
                    if (e.button == 0 && winRect.Contains(e.mousePosition))
                    {
                        if (e.type == EventType.MouseDown)
                        {
                            m_CurActItem = item;
                        }
                    }
                    GUI.DragWindow();

                }, itemName, s);
                if (drawRect.x < m_TimeRect.x)
                {
                    drawRect.x = m_TimeRect.x;
                }
                if (drawRect.x > m_TimeRect.x + m_TimeRect.width - drawRect.width)
                {
                    drawRect.x = m_TimeRect.x + m_TimeRect.width - drawRect.width;
                }

                item.StTime = (drawRect.x - trackRect.x) * m_TimeMuti;
                item.EdTime = (drawRect.x + drawRect.width - trackRect.x) * m_TimeMuti;
            }
            GUI.color = Color.white;
        }

        private void DrawTimelineControl()
        {
            Vector2 p1 = new Vector2(m_TimelineControl.Rect.x, 30);
            Vector2 p2 = new Vector2(m_TimelineControl.Rect.x, position.height);
            UnityEditor.Handles.color = Color.red;
            UnityEditor.Handles.DrawLine(p1, p2);
            UnityEditor.Handles.color = Color.white;
            Rect rect1 = new Rect(m_TimelineControl.Rect.x - 20, 0, 40, 30);
            Rect rect2 = new Rect(m_TimelineControl.Rect.x -  3, 0,  6, 30);
            GUI.Box(rect2, "", EBVTStyles.BVT_Window);
            Event e = Event.current;
            if (e.button == 0 && e.type == EventType.MouseDown && rect1.Contains(e.mousePosition))
            {
                m_TimelineDragged = true;
            }
            if (m_TimelineDragged && e.button == 0 && e.type == EventType.MouseDrag)
            {
                m_TimelineControl.Time = (e.mousePosition.x - m_TimeRect.x) * m_TimeMuti;
                if (m_TimelineControl.Time < 0) m_TimelineControl.Time = 0;
                if (m_TimelineControl.Time > m_TotalTime) m_TimelineControl.Time = m_TotalTime;
                e.Use();
            }
            if(e.type == EventType.MouseUp)
            {
                m_TimelineDragged = false;
            }
        }

        private void HandleEvents()
        {
            Event e = Event.current;
            if (m_CurActItem != null && e.keyCode == KeyCode.Delete && e.type == EventType.KeyDown)
            {
                for (int i = 0; i < m_SkillData.m_List.Count; i++)
                {
                    ActSkill skill = m_SkillData.m_List[i];
                    for (int k = 0; k < skill.Items.Count; k++)
                    {
                        if (skill.Items[k] == m_CurActItem)
                        {
                            skill.DelChild(m_CurActItem);
                            return;
                        }
                    }
                }
            }
        }
    }
}