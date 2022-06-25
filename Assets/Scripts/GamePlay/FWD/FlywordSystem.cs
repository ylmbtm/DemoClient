using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using TMPro;

namespace FWD
{
    public class FlywordSystem
    {
        public const string FLYWORD1  = "Guis/HUD/UIFlyword1";
        public const string FLYWORD2  = "Guis/HUD/UIFlyword2";
        public const string FLYWORD3  = "Guis/HUD/UIFlyword3";
        public const string FLYWORD4  = "Guis/HUD/UIFlyword4";
        public const string FLYWORD5  = "Guis/HUD/UIFlyword5";
        public const string FLYWORD6  = "Guis/HUD/UIFlyword6";
        public const string FLYWORD7  = "Guis/HUD/UIFlyword7";
        public const string FLYWORD8  = "Guis/HUD/UIFlyword8";
        public const string FLYWORD9  = "Guis/HUD/UIFlyword9";
        public const string FLYWORD10 = "Guis/HUD/UIFlyword10";
        public const string FLYWORD11 = "Guis/HUD/UIFlyword11";
        public const string FLYWORD12 = "Guis/HUD/UIFlyword12";
        public const string FLYWORD13 = "Guis/HUD/UIFlyword13";
        public const string FLYWORD14 = "Guis/HUD/UIFlyword14";
        public const string FLYWORD15 = "Guis/HUD/UIFlyword15";
        public const string FLYWORD16 = "Guis/HUD/UIFlyword16";

        public void   Play(Int32 value, Vector3 pos, EFlyWordType type)
        {
            value = Mathf.Abs(value);
            string path = GetPath(type);
            GameObject go = GTPoolManager.Instance.GetObject(path);
            if (go == null)
            {
                return;
            }
            UIFlyText flyword = go.GET<UIFlyText>();
            flyword.gameObject.layer = GTLayer.LAYER_UI;
            flyword.Path = path;
            flyword.transform.parent = GTCameraManager.Instance.NGUICamera.transform;
            Vector3 pos_3d = pos;
            Vector2 pos_screen = GTCameraManager.Instance.MainCamera.WorldToScreenPoint(pos_3d);
            Vector3 pos_ui = GTCameraManager.Instance.NGUICamera.ScreenToWorldPoint(pos_screen);
            pos_ui.z = 0;
            switch (type)
            {
                case EFlyWordType.TYPE_HOST_HURT_CRIT:
                    flyword.TextColor = Color.red;
                    flyword.TextEnlarge = 1.5f;
                    flyword.Text = string.Format("爆击 -{0}", value);
                    break;
                case EFlyWordType.TYPE_HOST_HURT_NORM:
                    flyword.TextColor = Color.red;
                    flyword.Text = string.Format("-{0}", value);
                    break;
                case EFlyWordType.TYPE_HOST_HEAL_CRIT:
                    flyword.TextColor = Color.green;
                    flyword.TextEnlarge = 1.5f;
                    flyword.Text = string.Format("+{0}", value);
                    break;
                case EFlyWordType.TYPE_HOST_HEAL_NORM:
                    flyword.TextColor = Color.green;
                    flyword.Text = string.Format("+{0}", value);
                    break;
                case EFlyWordType.TYPE_OTHER_HURT_CRIT:
                    flyword.TextColor = Color.red;
                    flyword.TextEnlarge = 1.5f;
                    flyword.Text = string.Format("爆击 -{0}", value);
                    break;
                case EFlyWordType.TYPE_OTHER_HURT_NORM:
                    flyword.TextColor = Color.red;
                    flyword.Text = string.Format("-{0}", value);
                    break;
                case EFlyWordType.TYPE_OTHER_HEAL_CRIT:
                    flyword.TextColor = Color.green;
                    flyword.TextEnlarge = 1.5f;
                    flyword.Text = string.Format("+{0}", value);
                    break;
                case EFlyWordType.TYPE_OTHER_HEAL_NORM:
                    flyword.TextColor = Color.green;
                    flyword.Text = string.Format("+{0}", value);
                    break;
                case EFlyWordType.TYPE_HOST_SPEED_UP:
                    flyword.TextColor = Color.white;
                    flyword.Text = "加速";
                    break;
                case EFlyWordType.TYPE_HOST_SPEED_DOWN:
                    flyword.TextColor = Color.cyan;
                    flyword.Text = "减速";
                    break;

                case EFlyWordType.TYPE_OTHER_SPEED_UP:
                    flyword.TextColor = Color.white;
                    flyword.Text = "加速";
                    break;
                case EFlyWordType.TYPE_OTHER_SPEED_DOWN:
                    flyword.TextColor = Color.cyan;
                    flyword.Text = "减速";
                    break;
                case EFlyWordType.TYPE_HOST_DODGE:
                    flyword.TextColor = Color.cyan;
                    flyword.Text = "闪避";
                    break;
                case EFlyWordType.TYPE_OTHER_DODGE:
                    flyword.TextColor = Color.cyan;
                    flyword.Text = "闪避";
                    break;
            }
            flyword.Init(pos_ui);
        }

        public string GetPath(EFlyWordType type)
        {
            switch (type)
            {
                case EFlyWordType.TYPE_HOST_HURT_CRIT:
                    return FLYWORD1;
                case EFlyWordType.TYPE_HOST_HURT_NORM:
                    return FLYWORD2;
                case EFlyWordType.TYPE_HOST_HEAL_CRIT:
                    return FLYWORD3;
                case EFlyWordType.TYPE_HOST_HEAL_NORM:
                    return FLYWORD4;
                case EFlyWordType.TYPE_OTHER_HURT_CRIT:
                    return FLYWORD5;
                case EFlyWordType.TYPE_OTHER_HURT_NORM:
                    return FLYWORD6;
                case EFlyWordType.TYPE_OTHER_HEAL_CRIT:
                    return FLYWORD7;
                case EFlyWordType.TYPE_OTHER_HEAL_NORM:
                    return FLYWORD8;
                case EFlyWordType.TYPE_HOST_SPEED_UP:
                    return FLYWORD9;
                case EFlyWordType.TYPE_HOST_SPEED_DOWN:
                    return FLYWORD10;
                case EFlyWordType.TYPE_OTHER_SPEED_UP:
                    return FLYWORD11;
                case EFlyWordType.TYPE_OTHER_SPEED_DOWN:
                    return FLYWORD12;
                case EFlyWordType.TYPE_HOST_DODGE:
                    return FLYWORD13;
                case EFlyWordType.TYPE_OTHER_DODGE:
                    return FLYWORD14;
                default:
                    return string.Empty;
            }
        }
    }
}