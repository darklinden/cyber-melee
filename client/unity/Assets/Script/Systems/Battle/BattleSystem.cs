using System;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Proto;
using UnityEngine;
using UserInterface;
using Wtf;

namespace Battle
{
    public class BattleSystem : MonoBehaviour, ISystemBase
    {
        private Transform m_Tm = null;
        public Transform Tm => m_Tm != null ? m_Tm : (m_Tm = transform);

        public bool IsInitialized { get; private set; } = false;

        internal PlayerCalcSystem PlayerCalc { get; private set; } = null;
        internal PlayerDisplaySystem PlayerDisplay { get; private set; } = null;
        internal ProjectileCalcSystem ProjectileCalc { get; private set; } = null;
        internal ProjectileDisplaySystem ProjectileDisplay { get; private set; } = null;
        public Dictionary<string, ISystemBase> SubSystems { get; } = new Dictionary<string, ISystemBase>();

        internal bool IsBattleRunning { get; set; } = false;

        public void Initialize()
        {
        }

        public async UniTask AsyncInitialize()
        {
            // 角色计算
            PlayerCalc = this.AddSystem<PlayerCalcSystem>();
            // 投射物计算
            ProjectileCalc = this.AddSystem<ProjectileCalcSystem>();

            // 角色展示
            PlayerDisplay = this.AddSystem<PlayerDisplaySystem>();
            // 投射物展示
            ProjectileDisplay = this.AddSystem<ProjectileDisplaySystem>();

            await this.InitializeSystems();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            PlayerCalc = null;
            ProjectileCalc = null;
            PlayerDisplay = null;
            ProjectileDisplay = null;

            this.DeinitializeSystems();

            IsBattleRunning = false;
            IsInitialized = false;
        }

        private Service.ServiceSystem m_ServiceSystem = null;
        private Service.ServiceSystem ServiceSystem => m_ServiceSystem != null ? m_ServiceSystem : (m_ServiceSystem = Context.Inst.GetSystem<Service.ServiceSystem>());

        internal void CheckBattleEnded(long frame)
        {
            if (IsBattleRunning)
            {
                var battleInfo = ServiceSystem.Data.BattleInfo;
                var friendlyOver = PlayerCalc.IsCampGameOver(battleInfo.FriendlyCampId, frame);
                var hostileOver = PlayerCalc.IsCampGameOver(battleInfo.HostileCampId, frame);
                if (friendlyOver || hostileOver)
                {
                    IsBattleRunning = false;
                    Log.E("Battle End", friendlyOver && hostileOver ? "Draw" : friendlyOver ? "Lose" : "Win");
                    var charCount = PlayerCalc.PlayerCount;
                    for (int i = 0; i < charCount; i++)
                    {
                        var one = PlayerCalc.GetPlayerAtIndex(i);
                        if (one != null)
                        {
                            var property = PlayerFrameUtil.GetProp(one, frame);
                            Log.D("Win Character",
                                one.PlayerId,
                                property.Hp,
                                property.StatProp[(int)Proto.StatPropType.HP]);
                        }
                    }
                    SeedRandom.LogSeeds();
                    List<ulong> result = new List<ulong>();
                    if (friendlyOver && hostileOver)
                    {
                        // draw
                        result.Add(Constants.IS_DRAW);
                        for (int i = 0; i < battleInfo.CampIds.Count; i++)
                        {
                            var campId = battleInfo.CampIds[i];
                            result.Add((ulong)campId);
                            result.AddRange(battleInfo.CampPlayerIds[campId]);
                        }
                    }
                    else if (friendlyOver)
                    {
                        result.Add(Constants.IS_NOT_DRAW);
                        result.Add((ulong)battleInfo.HostileCampId);
                        result.AddRange(battleInfo.HostilePlayerIds);
                        result.Add((ulong)battleInfo.FriendlyCampId);
                        result.AddRange(battleInfo.FriendlyPlayerIds);
                    }
                    else
                    {
                        result.Add(Constants.IS_NOT_DRAW);
                        result.Add((ulong)battleInfo.FriendlyCampId);
                        result.AddRange(battleInfo.FriendlyPlayerIds);
                        result.Add((ulong)battleInfo.HostileCampId);
                        result.AddRange(battleInfo.HostilePlayerIds);
                    }
                    AsyncBattleEndSendResult(result).Forget();
                }
            }
        }

        internal void CheckBattleTimeOver(long frame)
        {
            if (IsBattleRunning)
            {
                var battleInfo = ServiceSystem.Data.BattleInfo;
                var friendlyHp = PlayerCalc.CampTotalHp(battleInfo.FriendlyCampId, frame);
                var hostileHp = PlayerCalc.CampTotalHp(battleInfo.HostileCampId, frame);

                IsBattleRunning = false;

                Log.D("Battle TimeOver", friendlyHp == hostileHp ? "Draw" : friendlyHp < hostileHp ? "Lose" : "Win");

                var charCount = PlayerCalc.PlayerCount;
                for (int i = 0; i < charCount; i++)
                {
                    var one = PlayerCalc.GetPlayerAtIndex(i);
                    if (one != null)
                    {
                        var property = PlayerFrameUtil.GetProp(one, frame);
                        Log.D("Win Character",
                            one.PlayerId,
                            property.Hp,
                            property.StatProp[(int)Proto.StatPropType.HP]);
                    }
                }

                SeedRandom.LogSeeds();

                List<ulong> result = new List<ulong>();
                if (friendlyHp == hostileHp)
                {
                    // draw
                    result.Add(Constants.IS_DRAW);
                    for (int i = 0; i < battleInfo.CampIds.Count; i++)
                    {
                        var campId = battleInfo.CampIds[i];
                        result.Add((ulong)campId);
                        result.AddRange(battleInfo.CampPlayerIds[campId]);
                    }
                }
                else if (friendlyHp < hostileHp)
                {
                    result.Add(Constants.IS_NOT_DRAW);
                    result.Add((ulong)battleInfo.HostileCampId);
                    result.AddRange(battleInfo.HostilePlayerIds);
                    result.Add((ulong)battleInfo.FriendlyCampId);
                    result.AddRange(battleInfo.FriendlyPlayerIds);
                }
                else
                {
                    result.Add(Constants.IS_NOT_DRAW);
                    result.Add((ulong)battleInfo.FriendlyCampId);
                    result.AddRange(battleInfo.FriendlyPlayerIds);
                    result.Add((ulong)battleInfo.HostileCampId);
                    result.AddRange(battleInfo.HostilePlayerIds);
                }
                AsyncBattleEndSendResult(result).Forget();
            }
        }

        private async UniTask AsyncBattleEndSendResult(List<ulong> result)
        {
            var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();
            await serviceSystem.Service.BattleEnded.AsyncRequest(result.ToArray());
        }

        internal void BattleFinished(ServerBroadcastBattleFinished args)
        {
            if (args.ResultSame)
            {
                GameResultType gameResultType = GameResultType.Unknown;
                var isDraw = args.WinCampRank(0) == Constants.IS_DRAW;
                if (isDraw)
                {
                    gameResultType = GameResultType.Draw;
                }
                else
                {
                    var winCamp = (int)args.WinCampRank(1);
                    var battleInfo = ServiceSystem.Data.BattleInfo;

                    if (winCamp == battleInfo.FriendlyCampId)
                    {
                        gameResultType = GameResultType.Win;
                    }
                    else if (winCamp == battleInfo.HostileCampId)
                    {
                        gameResultType = GameResultType.Lose;
                    }
                }

                AsyncShowBattleResult(gameResultType).Forget();
            }
            else
            {
                Log.E("BattleFinished", "Result not same", args.UnPack());
            }
        }

        private async UniTask AsyncShowBattleResult(GameResultType result)
        {
            var data = new PanelTips.Data
            {
                Title = "Battle Result",
                Content = result.ToString(),
            };
            var panel = await UILoader.AsyncShow(PanelTips.PanelPath, data);
            await panel.AwaitClose();
        }

        internal async UniTask AsyncBattleStart()
        {
            var service = Context.Inst.GetSystem<Service.ServiceSystem>();
            await PlayerCalc.InitPlayers(
                PlayerDisplay,
                service.Data.BattleInfo);
        }
    }
}
