#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

namespace ACT
{
    public partial class ActSkill
    {
        private List<ActAnim>          Anims;
        private List<ActAudio>         Audios;
        private List<ActCameraEffect>  CameraEffects;
        private List<ActShake>         CameraShakes;
        private List<ActEffect>        Effects;
        private List<ActHideWeapon>    HideWeapons;
        private List<ActScope>         Scopes;
        private List<ActTimeScale>     TimeScales;
        private List<Int32>            IndexList;
        private List<string>           TypeList;

        public void OnBeforeSerialize()
        {
            TypeList       = new List<string>();
            IndexList      = new List<int>();
            Anims          = new List<ActAnim>();
            Audios         = new List<ActAudio>();
            CameraEffects  = new List<ActCameraEffect>();
            CameraShakes   = new List<ActShake>();
            Effects        = new List<ActEffect>();
            HideWeapons    = new List<ActHideWeapon>();
            TimeScales     = new List<ActTimeScale>();
            Scopes         = new List<ActScope>();
 
            for (int i = 0; i < Items.Count; i++)
            {
                ActItem item = Items[i];
                BeforeSerialize(Anims,         item);
                BeforeSerialize(Audios,        item);
                BeforeSerialize(CameraEffects, item);
                BeforeSerialize(CameraShakes,  item);
                BeforeSerialize(Effects,       item);
                BeforeSerialize(HideWeapons,   item);
                BeforeSerialize(TimeScales,    item);
                BeforeSerialize(Scopes,        item);
            }
        }

        public void OnAfterDeserialize()
        {
            Items.Clear();
            for (int i = 0; i < TypeList.Count; i++)
            {
                string typeName = TypeList[i];
                int    index    = IndexList[i];
                AfterDeserialize(typeName, index, Anims);
                AfterDeserialize(typeName, index, Audios);
                AfterDeserialize(typeName, index, CameraEffects);
                AfterDeserialize(typeName, index, CameraShakes);
                AfterDeserialize(typeName, index, Effects);
                AfterDeserialize(typeName, index, HideWeapons);
                AfterDeserialize(typeName, index, TimeScales);
                AfterDeserialize(typeName, index, Scopes);
            }
            Anims = null;
            Audios = null;
            CameraEffects = null;
            CameraShakes = null;
            Effects = null;
            HideWeapons = null;
            TimeScales = null;
            Scopes = null;
        }

        public void BeforeSerialize<T>(List<T> list, ActItem item) where T : ActItem
        {
            if (item is T)
            {
                TypeList.Add(item.GetType().Name);
                IndexList.Add(list.Count);
                list.Add(item as T);
            }
        }

        public void AfterDeserialize<T>(string typeName, int index, List<T> list) where T : ActItem
        {
            if (typeName == typeof(T).Name && list != null)
            {
                Items.Add(list[index]);
            }
        }
    }
}
#endif