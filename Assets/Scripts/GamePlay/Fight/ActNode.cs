using UnityEngine;
using System.Collections;
using System;
using System.Xml;
using System.Reflection;
using System.Collections.Generic;

namespace ACT
{
    public class ActNode
    {
        public virtual void Load(XmlElement element)
        {
            FieldInfo[] fields = this.GetType().GetFields();
            for (int i = 0; i < fields.Length; i++)
            {
                FieldInfo field = fields[i];
                Type t = field.FieldType;
                string value = element.GetAttribute(field.Name);
                if (string.IsNullOrEmpty(value))
                {
                    continue;
                }
                object v = default(object);
                if (t.BaseType == typeof(Enum))
                {
                    v = Enum.ToObject(t, value.ToInt32());
                }
                else if (t == typeof(bool))
                {
                    v = value == "true" || value == "1";
                }
                else if (t == typeof(Int16))
                {
                    v = value.ToInt32();
                }
                else if (t == typeof(UInt16))
                {
                    v = value.ToUInt32();
                }
                else if (t == typeof(Int32))
                {
                    v = value.ToInt32();
                }
                else if (t == typeof(UInt32))
                {
                    v = value.ToUInt32();
                }
                else if (t == typeof(Int64))
                {
                    v = value.ToInt64();
                }
                else if (t == typeof(UInt64))
                {
                    v = value.ToUInt64();
                }
                else if (t == typeof(float))
                {
                    v = value.ToFloat();
                }
                else if (t == typeof(string))
                {
                    v = value;
                }
                else if (t == typeof(Vector3))
                {
                    v = value.ToVector3();
                }
                else if (t == typeof(Vector2))
                {
                    v = value.ToVector2();
                }
                else if (t == typeof(List<string>))
                {
                    string[] array = value.Split('~');
                    List<string> list = (List<string>)field.GetValue(this);
                    list.AddRange(array);
                    v = list;
                }
                else if (t == typeof(List<float>))
                {
                    string[] array = value.Split('~');
                    List<float> list = (List<float>)field.GetValue(this);
                    for (int k = 0; k < array.Length; k++)
                    {
                        float kk = array[k].ToFloat();
                        list.Add(kk);
                    }
                    v = list;
                }
                else if (t == typeof(List<Int32>))
                {
                    string[] array = value.Split('~');
                    List<Int32> list = (List<Int32>)field.GetValue(this);
                    for (int k = 0; k < array.Length; k++)
                    {
                        Int32 kk = array[k].ToInt32();
                        list.Add(kk);
                    }
                    v = list;
                }
                field.SetValue(this, v);
            }
        }

        public virtual void Save(XmlDocument doc, XmlElement element)
        {
            FieldInfo[]     fields    = this.GetType().GetFields();
            List<FieldInfo> fieldList = new List<FieldInfo>();
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name == "StTime")
                {
                    fieldList.Add(fields[i]);
                }
            }
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name == "EdTime")
                {
                    fieldList.Add(fields[i]);
                }
            }
            for (int i = 0; i < fields.Length; i++)
            {
                if (fields[i].Name != "StTime" || fields[i].Name != "EdTime")
                {
                    fieldList.Add(fields[i]);
                }
            }

            for (int i = 0; i < fieldList.Count; i++)
            {
                FieldInfo field = fieldList[i];
                Type t = field.FieldType;
                if (t.BaseType == typeof(Enum))
                {
                    element.SetAttribute(field.Name, ((int)field.GetValue(this)).ToString());
                }
                else if (t == typeof(bool))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString().ToLower());
                }
                else if (t == typeof(Int32))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(UInt32))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(Int64))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(UInt64))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(Int16))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(UInt16))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(float))
                {
                    element.SetAttribute(field.Name, ((float)field.GetValue(this)).ToString("0.000"));
                }
                else if (t == typeof(string))
                {
                    element.SetAttribute(field.Name, field.GetValue(this).ToString());
                }
                else if (t == typeof(Vector3))
                {
                    Vector3 vector3 = (Vector3)field.GetValue(this);
                    element.SetAttribute(field.Name, GTTools.Vector3ToString(vector3));
                }
                else if (t == typeof(Vector3))
                {
                    Vector2 vector3 = (Vector2)field.GetValue(this);
                    element.SetAttribute(field.Name, GTTools.Vector2ToString(vector3));
                }
                else if (t == typeof(List<string>))
                {
                    List<string> list = field.GetValue(this) as List<string>;
                    if (list != null)
                    {
                        string s = string.Empty;
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (k == 0)
                            {
                                s += list[k];
                            }
                            else
                            {
                                s += "~" + list[k];
                            }
                        }
                        element.SetAttribute(field.Name, s);
                    }   
                }
                else if (t == typeof(List<Int32>))
                {
                    List<Int32> list = field.GetValue(this) as List<Int32>;
                    if (list != null)
                    {
                        string s = string.Empty;
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (k == 0)
                            {
                                s += list[k];
                            }
                            else
                            {
                                s += "~" + list[k];
                            }
                        }
                        element.SetAttribute(field.Name, s);
                    }
                }
                else if (t == typeof(List<float>))
                {
                    List<float> list = field.GetValue(this) as List<float>;
                    if (list != null)
                    {
                        string s = string.Empty;
                        for (int k = 0; k < list.Count; k++)
                        {
                            if (k == 0)
                            {
                                s += list[k];
                            }
                            else
                            {
                                s += "~" + list[k];
                            }
                        }
                        element.SetAttribute(field.Name, s);
                    }
                }
            }
        }
    }
}