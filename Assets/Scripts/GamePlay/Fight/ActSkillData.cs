using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ACT
{
    [System.Serializable]
    public class ActSkillData
    {
        public int m_MaxID = 0;
        public List<ActSkill> m_List  = new List<ActSkill>();
        public void SaveToXml()
        {
            string file = string.Format("{0}/Resources/Text/Data/Battle_Skill.xml", Application.dataPath);
            XmlDocument doc = new XmlDocument();
            FileStream fs = File.Create(file);
            XmlNode root = doc.CreateElement("Root");
            doc.AppendChild(root);
            for (int i = 0; i < m_List.Count; i++)
            {
                XmlElement child = doc.CreateElement("Skill");
                root.AppendChild(child);
                m_List[i].Save(doc, child);
                if(m_List[i].ID > m_MaxID)
                {
                    m_MaxID = m_List[i].ID;
                }
            }
            fs.Close();
            fs.Dispose();
            doc.Save(file);
        }
        public int NewSkill()
        {
            ActSkill skill = new ActSkill();
            skill.ID = m_MaxID + 1;
            m_List.Add(skill);
            m_MaxID = skill.ID;
            return skill.ID;
        }
        public void LoadFromXml()
        {
            TextAsset asset = GTResourceManager.Instance.Load<TextAsset>("Text/Data/Battle_Skill");
            if (asset == null)
            {
                return;
            }

            XmlDocument doc = new XmlDocument();
            doc.LoadXml(asset.text);
            XmlElement root = doc.FirstChild as XmlElement;
            XmlElement child = root.FirstChild as XmlElement;
            while (child != null)
            {
                ActSkill data = new ActSkill();
                data.Load(child);
                m_List.Add(data);

                if(data.ID > m_MaxID)
                {
                    m_MaxID = data.ID;
                }

                child = child.NextSibling as XmlElement;
            }
        }
    }
}