using UnityEngine;
using System.Collections;

namespace ACT
{
    public enum EACTS
    {
        INITIAL,//初始化
        STARTUP,//开始
        RUNNING,//运行
        SUCCESS,//成功
    }

    public enum ESkillPos
    {
        Skill_0 = 1,
        Skill_1,
        Skill_2,
        Skill_3,
        Skill_4,
        Skill_5,
        Skill_6,
        Skill_7,
        Skill_8,
        Skill_9,
    }
    public enum ESkillCastType
    {
        SKILL_SELECT_TYPE_INSTANT       = 0,    // 瞬发（无选择）
        SKILL_SELECT_TYPE_TARGET        = 1,    // 对象选取辅助（目标）
        SKILL_SELECT_TYPE_DIRECTION     = 2,    // 方向贴花辅助（朝向）
        SKILL_SELECT_TYPE_POS           = 3,    // 地面贴花辅助（AOE区域）
        SKILL_SELECT_TYPE_DIRECTIONARC  = 4,    // 扇形方向贴花辅助（朝向）
        SKILL_SELECT_TYPE_EFFECT        = 5,    // 模型特效辅助（召唤）
        SKILL_SELECT_TYPE_SIGHT         = 6,    // 准星UI变化辅助（射击）
        SKILL_SELECT_TYPE_PARABOLA      = 7,    // 抛物线辅助（投掷）
        SKILL_SELECT_TYPE_DRAGLINE      = 8,    // 拉线辅助（墙类召唤)
        SKILL_SELECT_TYPE_GESTURE       = 9,    // 鼠标手势辅助（多段线条传入点）
    }

    public enum ESkillCostType
    {
        NO = 0,
        MP = 1,                          //魔法
        HP = 2,                          //生命
        XP = 3,                          //经验
        IT = 4,                          //道具
    }

    public enum ERangeType
    {
        ERT_OBJECTS     = 1,    //客户端指定的目标
        ERT_CYLINDER    = 2,    //扇形圆柱
        ERT_CIRCLE      = 3,    //圆形圆柱
        ERT_BOX         = 4,    //矩形区域
        ERT_LINK        = 5,    //链式目标
    }

    public enum ESelectTargetPolicy
    {
        TYPE_SELECT_DEFAULT         = 0,//默认
        TYPE_SELECT_BY_MOREHEALTH   = 1,//按血量比例最高
        TYPE_SELECT_BY_LESSHEALTH   = 2,//按血量比例最低
        TYPE_SELECT_BY_MOREDISTANCE = 3,//按距离比例最高
        TYPE_SELECT_BY_LESSDISTANCE = 4,//按距离比例最低
    }

    public enum EBuffOverlayType
    {
        UnChange             = 0,//不改变
        Overlay              = 1,//叠加
        Refresh              = 2,//刷新
        OverlayAndRefresh    = 3,//叠加且刷新
    }

    public enum EBuffTimeType
    {
        TIME_GAMETIME    = 0,//计算在线时间
        TIME_TRUETIME    = 1,//计算真实时间（包括下线时间）
        TIME_USERCtrl    = 2,//用户脚本控制
    }

    public enum EBuffType
    {
        TYPE_NONE   = 0,
        TYPE_BUFF   = 1,//增益Buff
        TYPE_Nerf   = 2,//减益Buff
    }

    public enum EBuffDispelType
    {
        TYPE_BUFF   = 1,//驱散增益Buff
        TYPE_Nerf   = 2,//驱散减益Buff
    }

    public enum EFlyObjectType
    {
        TYPE_CHASE                 = 0,//追踪型飞弹
        TYPE_FIXDIRECTION          = 1,//固定方向型飞弹
        TYPE_FIXTARGETPOS          = 2,//固定目标点飞弹
        TYPE_POINT                 = 3,//固定点飞弹
        TYPE_LINK                  = 4,//连接飞弹
        TYPE_ANNULAR               = 5,//环形飞弹
        TYPE_BACK                  = 6,//回旋飞弹
        TYPE_BOUNDCE               = 7,//弹跳飞弹
        TYPE_WAVE                  = 8,//冲击波
    }

    public enum EFlyObjectStartPos
    {
        TYPE_CENTER                = 0,//以中心点为起始点
        TYPE_CASTER                = 1,//以施法者为起始点
        TYPE_TARGET                = 2,//以影响目标为起始点
    }
}