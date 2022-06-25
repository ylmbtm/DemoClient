using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;
using Protocol;
using ACT;

public class GTInputManager : GTMonoSingleton<GTInputManager>
{
    public override void SetRoot(Transform parent)
    {
        base.SetRoot(parent);
    }

    void Update()
    {
        OnEscape();
        OnKeyDown();
    }

    void OnEscape()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }
    }

    void OnKeyDown()
    {
        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_2);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_3);
        }
        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_4);
        }
        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_5);
        }
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_6);
        }
        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_7);
        }
        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_8);
        }
        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            GTWorld.Main.Get<ActorCommand>().ManualUseSkill(ESkillPos.Skill_9);
        }
        if (Input.GetKeyDown(KeyCode.P))
        {
            GTCameraManager.Instance.CameraCtrl.PlayShake(600, 0.2f);
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPaused = false;
#endif
        }
    }
}