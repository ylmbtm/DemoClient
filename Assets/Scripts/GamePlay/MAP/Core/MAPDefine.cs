using UnityEngine;
using System.Collections;

namespace MAP
{
    public enum EAreaShape
    {
        TYPE_CIRCLE   = 0,
        TYPE_RECT     = 1,
    }

    public enum ETriggerType
    {
        ETT_NORMAL,     //直接触发
        ETT_TRIBOX,     //触发盒触发
        ETT_TIME,       //时间触发
    }

    public enum EPathNodeType
    {
        Linear        = 0,
        Bezier        = 1,
    }

    public enum EMapGroup
    {
        Barrier       = 1,
        Born          = 2,
        Mine          = 3,
        Boss          = 4,
        Monster       = 5,
        Npc           = 6,
        Path          = 7,
        Portal        = 8,
        Area          = 9,
    }

    public enum EChunkType
    {
        TYPE_GROUND   = 0,
        TYPE_LIGHT    = 1,
        TYPE_BUILD    = 2,
        TYPE_CLOUD    = 3,
        TYPE_WATER    = 4,
        TYPE_TREE     = 5,
        TYPE_GRASS    = 6,
        TYPE_STONE    = 7,
        TYPE_EFFECT   = 8,
        TYPE_SOUND    = 9,
    }

    public enum EWeatherType
    {
        None          = 0,
        Sunny         = 1,   //晴天
        Thunder       = 2,   //打雷
        Rain          = 4,   //下雨
        Snow          = 8,   //下雪
        Hail          = 16,  //冰雹
        Storm         = 32,  //风暴
    }

    public enum EOpenConditionRelation
    {
        And           = 0,
        Or            = 1,
    }

    public enum EWinCondition
    {
        EWC_NONE,
        EWC_KILL_ALL,       //击杀全部怪物
        EWC_KILL_NUM,       //击杀指定数量怪物
        EWC_DESTINATION,    //达到目的地
        EWC_PLAYER_ALIVE,   //存活下来
        EWC_NPC_ALIVE,      //护送npc
        EWC_END
    };

    public enum EMapActionType
    {
        MapActBegin                 = 0,
        MapActTriggerArea           = 1,  //触发一个区域
        MapActDestroyArea           = 2,  //销毁一个区域
        MapActTriggerBarrier        = 3,  //触发光墙
        MapActDestroyBarrier        = 4,  //销毁光墙
        MapActTriggerSkill          = 5,  //触发关卡技能
        MapActTriggerSound          = 6,  //触发声音
        MapActTriggerStep           = 7,  //触发副本阶段
        MapActTriggerTask           = 8,  //触发任务
        MapActTriggerTeleport       = 9,  //触发传送
        MapActTriggerTimer          = 10, //触发计时器
        MapActDestroyTimer          = 11, //停止定时器
        MapActKillMe                = 12, //被秒杀
        MapActTriggerEffect         = 13, //触发特效
        MapActDestroyEffect         = 14, //销毁特效
        MapActDestroyAllMonsters    = 15, //销毁所有怪物
        MapActDestroyAllObjects     = 16, //销毁副本所有特效、怪物、矿石、Npc等等
        MapActDestroyPlayerEffect   = 17, //销毁在玩家身上的特效
        MapActTriggerPlayerEffect   = 18, //创建在玩家身上的特效

        MapActParallel              = 31,
        MapActRandomce              = 32,
        MapActSequence              = 33,
        MapActWeight                = 34,    
        MapActLoop                  = 35,
        MapActEnter                 = 36,
        MapActExit                  = 37,

        MapActCameraEffect          = 51, //相机特效
        MapActCG                    = 52, //CG动画
        MapActCutscene              = 53, //剧情动画
        MapActDelay                 = 54, //延迟
        MapActDialogue              = 55, //对话
        MapActReadbar               = 56, //读条操作   
        MapActMissionCollectMine    = 57, //收集指定矿石
        MapActMissionKillMonster    = 58, //杀死指定怪物
        MapActConvoy                = 59, //护送
        MapActAnswer                = 60, //答题
        MapActWave                  = 61, //波次任务，波次怪物被击杀才算该任务结束
    }

    public enum ENTS
    {
        INITIAL = 0,
        TRIGGER = 1,
        FAILURE = 2,
        SUCCESS = 3,
        RUNNING = 4,
    }
}
