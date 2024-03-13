# Lockstep Unity Client

This is the Unity client for the Lockstep Cyber Melee game.

## 脚本结构

* App 游戏根入口
  * Context -> 全局上下文
  * Constants -> 常量
  * i18n -> 国际化

* Configs 读取并存储配置信息
  * CharacterData -> 角色数据
  * MapData -> 双人对战地图站位
  * EffectOverTimeData -> 持续效果数据
  * ProjectileData -> 投射物数据
  * SkillData -> 技能数据

* Scenes 游戏场景相关脚本

* Systems 游戏系统相关脚本
  * AtlasLoader -> 图集加载器
  * Battle -> 战斗系统
  * Lockstep -> 同步系统
  * Properties -> 属性系统
  * Service 与服务器通信相关
  * EventSystem 输入相关系统
  * GameCtrl 游戏控制器
  * TimeSystem 时间系统

* UserInterface 游戏UI相关脚本

## 执行逻辑

```

|battle(client)| -- 用户操作 ---> |back end (server)|
|back end (server)| -- 确认操作 ---> |lockstep(client)|
|lockstep(client)| -- 计算状态 ---> |battle(client)|

时间线 | ------------------------------------- |
逻辑帧 | 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8 | 9 |
      ^播放逻辑帧       ^已计算的逻辑帧
      |   当前时间窗口  |   计算时间窗口, 用于计算下一逻辑帧
             ^ 可接收用户操作, 操作将在下个窗口内生效

```

* Battle 可近似看作状态同步客户端 
* Lockstep 可近似看作状态同步服务器

* LockStep -> 锁定步进逻辑
  * 间隔固定时间计算逻辑帧
  * 每次逻辑帧计算将锁定下一次逻辑帧计算前的所有逻辑
  * 每次逻辑帧接收帧操作并加入计算
  * 锁定步骤相当于在前端建立起了一套使用逻辑帧号确定时序的，使用seed random确定随机数的，状态同步机制，当行进至特定帧时，状态是确定的，已经经历的帧事件是确定的，只需要播放对应的帧事件然后将状态播放至对应状态即可。
  * 在character上存储有长效属性和帧对应属性，可以确定character指定帧的状态；在LockstepManager 存有操作序列，当character update时，可以播放播放序列中的操作

* 不再使用Map生成Character, 而只是生成技能轨道