using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public enum ELanguage
{
    Chinese,
    English
}

public enum EDataKeyType
{
    Id,
    Instance,
    Pos,
    Carrer,
}

public enum ERuneType
{
    LIT = 1,//小雕文
    MID = 2,//中雕文
    BIG = 3 //大雕文
}

public enum ECopyType :int
{
    TYPE_LOAD   = -1,
    TYPE_INIT   =  0,
    TYPE_LOGIN  =  1,
    TYPE_ROLE   =  2,
    TYPE_CITY   =  3,
    TYPE_PVE    =  4,
    TYPE_WORLD  =  5,
    TYPE_AREA   =  6,
    TYPE_RIPPLE =  7,
}

public enum EStarCondition
{
    TYPE_PASSCOPY    = 0,
    TYPE_MAIN_HEALTH = 1,
    TYPE_TIME_LIMIT  = 2,
}

public enum EAffect
{
    Self = 0, //影响自己
    Lock = 1, //影响锁定目标
    Host = 2, //影响主人
    ESef = 3, //影响除自己以外
    Atkr = 4, //影响攻击者
    Enem = 5, //影响敌方
    Ally = 6, //影响友方
    Team = 7, //影响小队
    Tuan = 8, //影响团队
    Guid = 9, //影响公会
    Camp = 10,//影响阵营
    Each = 11,//影响所有
}

public enum EDropType
{
    ONERAN = 1,//随机掉落一定数量的单个物品
    MULFIX = 2,//固定掉落多个物品
    MULRAN = 3,//随机掉落多个物品
}

public enum ERecvType
{
    TYPE_RECV_ALL,//领取所有的
    TYPE_RECV_ONE,//几选一
}

public enum ERewardState
{
    NOT_RECEIVE = 0,
    CAN_RECEIVE = 1,
    HAS_RECEIVE = 2,
}

public enum EDialoguePos
{
    LF = 0,
    RT = 1
}

public enum EDialogueContentShowType
{
    Normal = 0,
    Effect = 1
}

public enum EDialogueContentType
{
    TYPE_NONE   = 0,
    TYPE_PLAYER = 1,
    TYPE_ACTOR  = 2,
    TYPE_ITEM   = 3,
    TYPE_MAP    = 4
}

public enum ETaskType
{
    NONE,
    THREAD = 1,   //主线任务
    BRANCH = 2,   //支线任务
    DAILY  = 3,   //日常任务
}

public enum ETaskTargetType
{
    TYPE_NONE,
    TYPE_KILL_COPYBOSS  = 1,//击杀副本BOSS
    TYPE_MAIN_PASSCOPY  = 2,//通关副本（次数）
    TYPE_UPEQUIP        = 3,//升级装备
    TYPE_UPPET          = 4,//升级宠物
    TYPE_UPGEM          = 5,//升级星石
    TYPE_UPPARTNER      = 6,//升级伙伴
    TYPE_UPSKILL        = 7,//升级角色技能
    TYPE_TALK           = 8,//对话
    TYPE_ROB_TREASURE   = 9,//夺宝
    TYPE_AREAE          = 10,//竞技场战斗
    TYPE_PASS_ELITECOPY = 11,//通关精英副本
    TYPE_CHARGE_RELICE  = 12,//神器充能
    TYPE_EQUIPSTAR      = 13,//装备星级
    TYPE_XHJJC          = 14,//虚幻竞技场
    TYPE_KILLRACE       = 15,//杀死种族怪物
}


public enum ETaskCycleType
{
    TYPE_NONE,
    TYPE_DAILY,   //每日重置
    TYPE_WEEKLY,  //每周重置
    TYPE_SCENE,   //副本重置
}

public enum ETaskState
{
    QUEST_NONE,           //无类型
    QUEST_DOING,          //正在进行任务
    QUEST_CANSUBMIT,      //可提交
    QUEST_FAILED,         //任务失败
    QUEST_HASSUBMIT,      //已经提交
}

public enum Resp : ushort
{
    TYPE_YES                              = 0,   //成功
    TYPE_NO                               = 1,   //不做
    TYPE_NO_COMPONENT                     = 2,   //没有该组件

    TYPE_ACTOR_FSMLAYER1                  = 3,   //
    TYPE_ACTOR_FSMLAYER2                  = 4,   //
    TYPE_ACTOR_FSMLAYER3                  = 5,   //角色被控制，无法操控
    TYPE_ACTOR_CANNOT_MOVE                = 6,   //角色无法移动 
    TYPE_ACTOR_CANNOT_MOVETODEST          = 7,   //无法到达此位置 

    TYPE_ACTOR_DEAD                       = 21,  //角色已死亡
    TYPE_ACTOR_CANNOTBE_ATTACK            = 22,  //角色无法被攻击

    TYPE_ACTOR_KONGZHAN                   = 25,  //角色在空战中
    TYPE_ACTOR_SWIM                       = 26,  //角色在游泳中
    TYPE_ACTOR_JUMP                       = 27,  //角色在跳跃中
    TYPE_ACTOR_RIDE                       = 28,  //角色在骑乘中
    TYPE_ACTOR_FLY                        = 29,  //角色在飞行中
    TYPE_ACTOR_NOTSET_HPDRUG              = 30,  //没有设置HPDrug
    TYPE_ACTOR_NOTSET_MPDRUG              = 31,  //没有设置MPDrug

    TYPE_SKILL_LACKHP                     = 41,  //缺少HP
    TYPE_SKILL_LACKMP                     = 42,  //缺少MP
    TYPE_SKILL_LACKSP                     = 43,  //缺少SP
    TYPE_SKILL_CASTING                    = 44,  //正在释放技能
    TYPE_SKILL_LEADING                    = 45,  //正在引导技能
    TYPE_SKILL_CD                         = 46,  //技能还未冷却
    TYPE_SKILL_NOTFIND                    = 47,  //找不到这个技能
    TYPE_SKILL_NOTDOATSCENE               = 48,  //当前场景无法使用技能
    TYPE_SKILL_LACKITEM                   = 49,  //缺少Item
    TYPE_SKILL_SILENT                     = 50,  //沉默
    TYPE_SKILL_CANNOT_ATTACK_WITH_STEALTH = 51,  //隐身
    TYPE_SKILL_TOONEAR                    = 52,  //太近
    TYPE_SKILL_TOOFAR                     = 53,  //太远
    TYPE_SKILL_COLLIDE                    = 54,  //直线距离之间有碰撞
    TYPE_SKILL_NOT_INRECTANGLE            = 55,  //不在矩形范围内
    TYPE_SKILL_NOT_INARC                  = 56,  //不在弧形范围内
    TYPE_SKILL_NOT_INTRIANGLE             = 57,  //不在三角形范围内
    TYPE_SKILL_CANNOT_ATTACK_TARGET       = 58,  //目标无法攻击
    TYPE_SKILL_TARGET_NULL                = 59,  //需要目标释放

    TYPE_RIDE_ING                         = 71,  //骑乘中
    TYPE_RIDE_NOTDOATSCENE                = 72,  //当前场景无法使用坐骑
    TYPE_RIDE_NOTDOATFSM                  = 73,  //当前状态无法使用坐骑 
    TYPE_RIDE_NONE                        = 74,  //当前你还没有坐骑  
}

public enum EFlyType
{
    TYPE_FIXDIRECTION = 0,
    TYPE_FIXPOSITION  = 1,
    TYPE_CHASE        = 2,
    TYPE_CHASEPARABOL = 3,
}

public enum EMoveType : byte
{
    SeekPosition,
    SeekTransform,
    SeekActor,
    MoveForce,
}

public enum FSMState : int
{
    FSM_EMPTY,

    //==========需要同步的状态============
    FSM_IDLE,                //待机
    FSM_FIXBODY,             //定身
    FSM_WALK,                //行走
    FSM_RUN,                 //跑动
    FSM_FLY,                 //飞行
    
    //=========主动操作的状态=============
    FSM_SKILL,               //技能
    FSM_MINE,                //采集
    FSM_ROLL,                //翻滚
    FSM_JUMP,                //跳跃
    FSM_BORN,                //出生
    FSM_DANCE,               //跳舞
    FSM_PRE_SKILL,           //等待服务器技能处理

    //=========被动状态=================
    FSM_DEAD,                //死亡
    FSM_WOUND,               //受击
    FSM_BEATBACK,            //击退
    FSM_BEATDOWN,            //击倒
    FSM_BEATFLY,             //击飞
    FSM_FLOATING,            //浮空

}

public enum EActorNature
{
    CAN_MOVE,          //可移动
    CAN_KILL,          //可击杀
    CAN_MANUALATTACK,  //可主动攻击
    CAN_TURN,          //可转向
    CAN_STUN,          //可击晕
    CAN_BEATBACK,      //可击退
    CAN_BEATFLY,       //可击飞
    CAN_BEATDOWN,      //可击倒
    CAN_WOUND,         //可受击
    CAN_REDUCESPEED,   //可减速
    CAN_FIXBODY,       //可定身
    CAN_SLEEP,         //可睡眠
    CAN_VARISTION,     //可变形
    CAN_PARALY,        //可麻痹
    CAN_FEAR,          //可恐惧
}

public enum EActorSex
{
    B,//男
    G,//女
    X,//未知
}

//怪物类型
public enum EActorSort
{
    None   = 0,
    Normal = 1,   //正常
    Elite  = 2,   //精英
    Rare   = 3,   //稀有
    Boss   = 4,   //Boss
    World  = 5,   //世界Boss
    Chest  = 6,   //宝箱
    Tower  = 7,   //水晶塔
    Cage   = 8,   //囚笼
}

//怪物种族
public enum EActorRace
{
    TYPE_NONE    = 1,    //宝箱、囚笼等
    TYPE_HUMAN   = 2,    //人类
    TYPE_SPIRIT  = 3,    //精灵
    TYPE_ORC     = 4,    //兽人
    TYPE_GHOST   = 5,    //亡灵
    TYPE_DEVIL   = 6,    //恶魔
    TYPE_ELEM    = 7,    //元素
    TYPE_GIANT   = 8,    //巨人
    TYPE_MACHINE = 9,    //机械
    TYPE_BEAST   = 10,   //野兽
    TYPE_DRAGON  = 11,   //龙类
}

public enum EDeadReason
{
    Normal       = 0,   //正常死亡
    Kill         = 1,   //机制秒杀
    Plot         = 2    //剧情杀
}

public enum EAIExecuteType
{
    TYPE_HANDLE     = 0,//手动
    TYPE_AUTO       = 1,//自动
    TYPE_AUTOPAUSED = 2,//自动被暂停
}

public enum ESkillTalentType
{
    TYPE_NONE                  = 0,
    TYPE_STRENG_SKILL          = 1,
    TYPE_NEW_SKILL             = 2,
    TYPE_NEW_AND_REPLACE_SKILL = 3,
}

public enum EFlyWordType
{
    //HOST   自己
    //OTHER  他人

    //HURT   扣血
    //HEAL   加血

    //CRIT   爆击伤害
    //NORM   普通伤害
    TYPE_NONE = 0,

    TYPE_HOST_HURT_CRIT = 1,
    TYPE_HOST_HURT_NORM = 2,

    TYPE_HOST_HEAL_CRIT = 3,
    TYPE_HOST_HEAL_NORM = 4,

    TYPE_OTHER_HURT_CRIT = 5,
    TYPE_OTHER_HURT_NORM = 6,

    TYPE_OTHER_HEAL_CRIT = 7,
    TYPE_OTHER_HEAL_NORM = 8,

    TYPE_HOST_SPEED_UP = 9,
    TYPE_HOST_SPEED_DOWN = 10,

    TYPE_OTHER_SPEED_UP = 11,
    TYPE_OTHER_SPEED_DOWN = 12,

    TYPE_HOST_DODGE = 13,
    TYPE_OTHER_DODGE = 14,
}


//特效绑定位置
public enum EBind
{
    None  = -1,
    Head  = 0, //出现在头部位置
    Body  = 1, //出现在身体位置
    Foot  = 2, //出现在脚部位置
    HandL = 3, //出现在左手上
    HandR = 4, //出现在右手上
    Buff  = 5, //Buff点
}

public enum EAnimatorLayerType
{
    TYPE_ANIMATOR_LAYER0  = 0,
    TYPE_ANIMATOR_LAYER1  = 1,
    TYPE_ANIMATOR_LAYER2  = 2,
}