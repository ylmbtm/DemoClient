using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml;
using System.IO;

namespace ACT
{
    public class BattleSkillData
    {
        public static Dictionary<int,ActSkill> mapSkill = new Dictionary<int, ActSkill>();

        public static void SaveDoc()
        {
            string file = string.Format("{0}/Resources/Text/Data/Battle_Skill.xml", Application.dataPath);
            XmlDocument doc = new XmlDocument();
            FileStream fs = File.Create(file);
            XmlNode root = doc.CreateElement("Root");
            doc.AppendChild(root);

            foreach (var item in mapSkill.Values)
            {
                XmlElement child = doc.CreateElement("Skill");
                root.AppendChild(child);
                item.Save(doc, child);
            }

            fs.Close();
            fs.Dispose();
            doc.Save(file);
        }
        public static ActSkill GetActSkill(int id)
        {
            if (!mapSkill.ContainsKey(id))
            {
                return null;
            }

            return mapSkill[id];
        }
        public static  void LoadDoc()
        {
            TextAsset asset = GTResourceManager.Instance.Load<TextAsset>(string.Format("Text/Data/Battle_Skill"));
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
                if(data == null)
                {
                    return;
                }
                data.Load(child);
                mapSkill[data.ID] = data;
                child = child.NextSibling as XmlElement;
            }
        }
    }
}