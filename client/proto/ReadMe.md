# Protocol for configs and network transfer

This folder contains the protocol for the configuration of the devices and the network transfer of the data.

* EOT 持续效果
* EOTTrigger 触发器
* Skill 技能
* Projectile 投射物
* Map 地图
* Character 角色

## CSV Files

This folder contains the following CSV files:

* CharacterDataTable.csv
  * id 角色ID
  * name 角色名
  * props 角色属性
  * skills 角色技能

* EffectOverTimeDataTable.csv
  * eot EOTType 效果ID
  * type EOTKind 效果类型 buff/debuff/dot/hot
  * trigger_cd 触发冷却时间
  * duration 持续时间
  * max_stack 最大叠加层数
  * props 属性加减
  * doh 伤害治疗攻击力百分比
  * doh_interval 伤害治疗间隔
  * special_param 特殊属性参数

* EOTKindEnum.csv
  * EOTKind 效果类型 buff/debuff/dot/hot

* EOTTriggerStruct.csv
  * eot EOTType 效果类型 EffectOverTimeDataTable 主键
  * trigger_type EOTTriggerType 触发方式 Always, SkillUsed, SkillHit, Hit, BeHit

* EOTTriggerTypeEnum.csv
  * EOTTriggerType 触发方式 Always, SkillUsed, SkillHit, Hit, BeHit

* EOTTypeEnum.csv
  * EOTType 效果ID

* MapDataTable.csv
  * id 地图ID
  * camp1 阵营1 站位
  * camp2 阵营2 站位

* ProjectileDataTable.csv
  * projectile_type ProjectileType 投射物类型 主键
  * circle_collider_radius 碰撞检测半径
  * speed 飞行速度
  * damage 伤害系数
  * max_life_time 最大时长

* ProjectileTypeEnum.csv
  * ProjectileType 投射物类型

* SkillAffectTypeEnum.csv
  * 技能效果类型 NONE,0,未知类型,Damage,1,伤害技能,Heal,2,治疗技能,Buff,3,增益技能,Debuff,4,减益技能,Dot,5,持续伤害技能,Hot,6,持续治疗技能

* SkillDataTable.csv
  * skill_type SkillType 技能类型 主键
  * init_cd 初始冷却
  * cooldown 冷却时间
  * global_cd 公共冷却
  * target_type SkillTargetType 目标选择类型
  * affect_type SkillAffectType 技能效果
  * anim_dur 动画总时间
  * cast_dur 技能生效时间
  * projectile_type ProjectileType 投射物
  * triggers EOTTrigger 触发器

* SkillTargetTypeEnum.csv
  * SkillTargetType 目标选择类型 TargetSingleEnemy,1,选择单个敌人, TargetSelf,2,选择自体,

* SkillTypeEnum.csv
  * SkillType 技能类型

* StatPropStruct.csv
  * prop_type,StatPropType 属性类型
  * value_type,StatPropValueType,"属性数值类型"  VAL,0,加值 MUL,1,乘值 百分比
  * prop_value,int64,"属性数值 乘值时 1000 为 1"

* StatPropTypeEnum.csv
  * StatPropType 属性类型

* StatPropValueTypeEnum.csv
  * StatPropValueType 属性数值类型

* Vec3Struct.csv
  * x 1000 为 1
  * y 1000 为 1
  * z 1000 为 1

## FBS Files

This folder contains FBS files for network transfer.