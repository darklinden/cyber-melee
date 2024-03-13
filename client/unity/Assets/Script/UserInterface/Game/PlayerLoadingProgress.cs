using System;
using System.Collections.Generic;
using App;
using UnityEngine;
using UnityEngine.UI;
using Wtf;

namespace Battle
{
    public class PlayerLoadingProgress : MonoBehaviour
    {
        [SerializeField] private VerticalLayoutGroup HostileLoadProgressGroup;
        [SerializeField] private VerticalLayoutGroup FriendlyLoadProgressGroup;
        [SerializeField] private PlayerLoadingProgressCell LoadProgressPrefab;

        private Dictionary<ulong, PlayerLoadingProgressCell> PlayerLoadProgressDict
            = new Dictionary<ulong, PlayerLoadingProgressCell>();

        private void Awake()
        {
            Context.Inst.EventBus.OnBattleStartLoading += OnBattleStartLoading;
            Context.Inst.EventBus.OnBattleLoadingProgress += OnBattleLoadingProgress;
        }

        private void OnDestroy()
        {
            var eventBus = Context.Inst?.EventBus;
            if (eventBus != null)
            {
                eventBus.OnBattleStartLoading -= OnBattleStartLoading;
                eventBus.OnBattleLoadingProgress -= OnBattleLoadingProgress;
            }
        }

        private void OnBattleStartLoading()
        {
            var serviceSystem = Context.Inst.GetSystem<Service.ServiceSystem>();

            foreach (var playerId in serviceSystem.Data.BattleInfo.HostilePlayerIds)
            {
                if (serviceSystem.Data.BattleInfo.PlayerInfoDict.TryGetValue(playerId, out var player))
                {
                    var progressCell = Instantiate(LoadProgressPrefab, HostileLoadProgressGroup.transform);
                    progressCell.gameObject.SetActive(true);
                    progressCell.Setup(player.Name);
                    PlayerLoadProgressDict[playerId] = progressCell;
                }
            }

            foreach (var playerId in serviceSystem.Data.BattleInfo.FriendlyPlayerIds)
            {
                if (serviceSystem.Data.BattleInfo.PlayerInfoDict.TryGetValue(playerId, out var player))
                {
                    var progressCell = Instantiate(LoadProgressPrefab, FriendlyLoadProgressGroup.transform);
                    progressCell.gameObject.SetActive(true);
                    progressCell.Setup(player.Name);
                    PlayerLoadProgressDict[playerId] = progressCell;
                }
            }
        }

        private void OnBattleLoadingProgress(ulong playerId, int progress)
        {
            if (PlayerLoadProgressDict.TryGetValue(playerId, out var p))
            {
                p.SetProgress(progress);
            }
        }
    }
}