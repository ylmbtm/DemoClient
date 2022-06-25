using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;
using ACT;

public class GTConfigManager : GTSingleton<GTConfigManager>
{
    public override void Init()
    {
        ReadCfgEffect.                Read("Text/Data/Data_Effect");
        ReadCfgRandomName.            Read("Text/Data/Data_RandomNames");
        ReadCfgLocalString.           Read("Text/Data/Data_Language");
        ReadCfgProperty.              Read("Text/Data/Data_Property");
        ReadCfgQuality.               Read("Text/Data/Data_Quality");
        ReadCfgItem.                  Read("Text/Data/Data_Item");
        ReadCfgRole.                  Read("Text/Data/Data_Role");
        ReadCfgRoleLevel.             Read("Text/Data/Data_RoleLevel");
        ReadCfgEquip.                 Read("Text/Data/Data_Equip");
        ReadCfgEquipAdvance.          Read("Text/Data/Data_EquipAdvance");
        ReadCfgEquipAdvanceCost.      Read("Text/Data/Data_EquipAdvanceCost");
        ReadCfgEquipStreng.           Read("Text/Data/Data_EquipStrengthen");
        ReadCfgEquipStrengLevel.      Read("Text/Data/Data_EquipStrengthenLevel");
        ReadCfgEquipStar.             Read("Text/Data/Data_EquipStar");
        ReadCfgEquipSuit.             Read("Text/Data/Data_EquipSuit");

        ReadCfgGem.                   Read("Text/Data/Data_Gem");
        ReadCfgGemLevel.              Read("Text/Data/Data_GemLevel");
        ReadCfgGemSuit.               Read("Text/Data/Data_GemSuit");
        ReadCfgAward.                 Read("Text/Data/Data_Award");
        ReadCfgCopyMainChapter.       Read("Text/Data/Data_CopyMainChapter");
        ReadCfgCopy.                  Read("Text/Data/Data_Copy");
        ReadCfgRelics.                Read("Text/Data/Data_Relics");
        ReadCfgMachine.               Read("Text/Data/Data_Machine");

        ReadCfgActorGroup.            Read("Text/Data/Data_ActorGroup");
        ReadCfgActor.                 Read("Text/Data/Data_Actor");
        ReadCfgActorRace.             Read("Text/Data/Data_ActorRace");
        ReadCfgActorModel.            Read("Text/Data/Data_ActorModel");

        ReadCfgStore.                 Read("Text/Data/Data_Store");
        ReadCfgStoreType.             Read("Text/Data/Data_StoreType");

        ReadCfgPet.                   Read("Text/Data/Data_Pet");
        ReadCfgPetLevel.              Read("Text/Data/Data_PetLevel");

        ReadCfgPartner.               Read("Text/Data/Data_Partner");
        ReadCfgPartnerLevel.          Read("Text/Data/Data_PartnerLevel");
        ReadCfgPartnerWake.           Read("Text/Data/Data_PartnerWake");
        ReadCfgPartnerAdvance.        Read("Text/Data/Data_PartnerAdvance");
        ReadCfgPartnerWash.           Read("Text/Data/Data_PartnerWash");
        ReadCfgPartnerFetter.         Read("Text/Data/Data_PartnerFetter");
        ReadCfgPartnerStar.           Read("Text/Data/Data_PartnerStar");

        ReadCfgMount.                 Read("Text/Data/Data_Mount");

        ReadCfgAdventure.             Read("Text/Data/Data_Adventure");
        ReadCfgSkillTalent.           Read("Text/Data/Data_SkillTalent");
        ReadCfgMine.                  Read("Text/Data/Data_Mine");
        ReadCfgSkill.                 Read("Text/Data/Data_Skill");
        ReadCfgVideo.                 Read("Text/Data/Data_Video");
        ReadCfgDialogue.              Read("Text/Data/Data_Dialogue");
        ReadCfgBuff.                  Read("Text/Data/Data_Buff");
        ReadCfgBuffItem.              Read("Text/Data/Data_BuffItem");
        ReadCfgBuffBehaviour.         Read("Text/Data/Data_BuffBehaviour");
        ReadCfgPlayable.              Read("Text/Data/Data_Playable");
        ReadCfgFlyObject.             Read("Text/Data/Data_FlyObject");

  

        BattleSkillData.              LoadDoc();
    }
}