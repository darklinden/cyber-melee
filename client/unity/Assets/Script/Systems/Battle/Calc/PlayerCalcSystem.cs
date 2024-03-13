using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using FixMath;
using Proto;
using UnityEngine;
using Wtf;

namespace Battle
{
    /// <summary>
    /// 角色管理器
    /// 暂定战斗场景地上跑的均为角色
    /// </summary>
    public class PlayerCalcSystem : MonoBehaviour, ISystemBase
    {
        public Dictionary<string, ISystemBase> SubSystems => null;

        public Transform Tm => null;

        public bool IsInitialized { get; private set; }

        public void Initialize()
        {
            IsInitialized = true;
        }

        public UniTask AsyncInitialize()
        {
            throw new System.NotImplementedException();
        }

        public void Deinitialize()
        {
            IsInitialized = false;
        }

        public long CharacterFrameFree { get; set; } = Constants.INVALID_VALUE;

        // 角色池
        public List<ulong> PlayerIds = new List<ulong>();
        public int PlayerCount { get => PlayerIds.Count; }

        // < PlayerId : Player >
        internal Dictionary<ulong, Player> Players = new Dictionary<ulong, Player>();

        // 角色的阵营
        internal Dictionary<int, List<ulong>> CampPlayers = new Dictionary<int, List<ulong>>();

        internal async UniTask InitPlayers(PlayerDisplaySystem playerDisplay, Service.BattleInfo battleInfo)
        {
            Players.Clear();
            PlayerIds.Clear();

            for (var i = 0; i < battleInfo.CampIds.Count; i++)
            {
                var campId = battleInfo.CampIds[i];
                var campPlayerIds = battleInfo.CampPlayerIds[campId];

                for (var j = 0; j < campPlayerIds.Count; j++)
                {
                    var playerId = campPlayerIds[j];
                    var playerInfo = battleInfo.PlayerInfoDict[playerId];
                    var play = InitPlayer(playerInfo, campId);
                    Log.D("InitCharacter SeedRand", play.PlayerId);
                    SeedRandom.InitAllState(play.PlayerId, playerInfo.Seed);
                    SetPlayer(play);
                    SetCampPlayers(play.PlayerId, campId);
                    Log.D("InitCharacter SaveCharacter", play.PlayerId);
                }
                CampPlayers[campId].Sort();
            }

            PlayerIds.Sort();

            Log.AssertIsTrue(battleInfo.CampIds.Count == 2, "CharacterManagerExtData.InitCharacters CampIds.Count Not 2", battleInfo.CampIds.Count);
            Log.AssertIsTrue(PlayerIds.Count == 2, "CharacterManagerExtData.InitCharacters PlayerIds.Count Not 2", PlayerIds.Count);

            var baseMap = Configs.Instance.OrbitMapData.GetData(Constants.MAP_ID);
            Log.AssertIsTrue(baseMap != null, "CharacterManagerExtData.InitCharacters BaseMap Not Found", Constants.MAP_ID);
            Log.AssertIsTrue(baseMap.Count % 2 != 0, "CharacterManagerExtData.InitCharacters BaseMap Count Not Odd", baseMap.Count);
            var middleIndex = (baseMap.Count - 1) / 2;
            var middleOrbit = baseMap[middleIndex];

            for (int i = 0; i < battleInfo.CampIds.Count; i++)
            {
                var campId = battleInfo.CampIds[i];
                var campPlayerIds = battleInfo.CampPlayerIds[campId];
                var player = GetPlayerById(campPlayerIds[0]);

                if (player.OrbitPosList == null) player.OrbitPosList = new List<F64Vec3>();
                player.OrbitPosList.Clear();

                for (int j = 0; j < baseMap.Count; j++)
                {
                    player.OrbitPosList.Add(baseMap[j].OrbitPosList[i]);
                }
                player.OrbitCenter = middleOrbit.OrbitPosList[i];

                Log.D("InitCharacter StandPosition", player.PlayerId, player.OrbitPosList, player.OrbitCenter);
                playerDisplay.InitPlayerPos(player);
                await UniTask.Yield();
            }
            Log.D("CharacterManagerExtData.InitCharacters", PlayerIds.Count);
            SeedRandom.LogSeeds();
        }

        internal void Reset()
        {
            CharacterFrameFree = Constants.INVALID_VALUE;
            PlayerIds.Clear();
            Players.Clear();
            CampPlayers.Clear();
        }

        internal static Player InitPlayer(Proto.PlayerInfoT playerInfo, int campId)
        {
            Log.D("InitPlayer for player", playerInfo.PlayerId, "camp", campId);

            var characterId = playerInfo.OtherInfo[0];
            var characterData = Configs.Instance.CharacterData.GetData(characterId);

            var player = Player.Get();
            player.PlayerId = playerInfo.PlayerId;
            player.CampId = campId;
            player.Frames = XList<long>.Get(64);
            player.FrameProperties.Clear();
            player.CastSkill = characterData.CastSkill;
            player.SpecialSkill = characterData.SpecialSkill;

            Log.AssertIsTrue(characterData.BulletGenCd % Constants.FRAME_LENGTH_MS == 0, "CharacterManagerExtData.InitCharacter BulletGenCd Not Divisible", characterData.BulletGenCd);
            player.BulletGenerateCdFrameCount = characterData.BulletGenCd / Constants.FRAME_LENGTH_MS;
            player.BulletBufferSize = characterData.BulletBufferSize;

            for (int i = 0; i < characterData.Props.Count; i++)
            {
                var propV = characterData.Props[i];
                player.StatProp[(int)propV.PropType] = propV.PropValue;
            }

            // 初始化属性
            var prop = PlayerFrameProp.Get();
            prop.EOTList = XList<EffectOverTime>.Get();

            foreach (var statKV in player.StatProp)
            {
                var k = statKV.Key;
                var propV = statKV.Value;
                prop.StatProp[k] = propV;
            }

            prop.Hp = player.StatProp[(int)Proto.StatPropType.HP];
            prop.BulletBuffer = XList<ProjectileType>.Get();

            Log.D("InitPlayerProp", player.PlayerId, prop.Hp, prop.StatProp);

            PlayerFrameUtil.SetProp(player, 0, prop);

            // 初始化技能池
            // 为角色指定技能列表
            PlayerSkillUtil.InitSkills(player, characterData);

            Log.D("InitCharacter", player.PlayerId, player.StatProp);
            return player;
        }

        public bool IsPlayerGameOver(ulong playerId, long frame)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                var prop = PlayerFrameUtil.GetProp(player, frame);
                if (!prop.IsDead)
                {
                    return false;
                }
            }
            return true;
        }

        public void SetCampPlayers(ulong playerId, int campId)
        {
            if (!CampPlayers.TryGetValue(campId, out List<ulong> playerIds))
            {
                playerIds = new List<ulong>();
                CampPlayers.Add(campId, playerIds);
            }
            if (!playerIds.Contains(playerId))
            {
                playerIds.Add(playerId);
            }
        }

        public List<ulong> GetPlayerIdsByCampId(int campId)
        {
            if (CampPlayers.TryGetValue(campId, out var playerIds))
            {
                return playerIds;
            }
            return null;
        }

        public List<ulong> GetPlayerIdsByNotCampId(int campId)
        {
            List<ulong> playerIds;
            foreach (var kv in CampPlayers)
            {
                if (kv.Key != campId)
                {
                    playerIds = kv.Value;
                    return playerIds;
                }
            }

            return null;
        }

        public bool IsCampGameOver(int campId, long frame)
        {
            if (CampPlayers.TryGetValue(campId, out var playerIds))
            {
                for (var i = 0; i < playerIds.Count; i++)
                {
                    var playerId = playerIds[i];
                    var player = GetPlayerById(playerId);
                    if (player != null)
                    {
                        var prop = PlayerFrameUtil.GetProp(player, frame);
                        if (!prop.IsDead)
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

        public long CampTotalHp(int campId, long frame)
        {
            long totalHp = 0;
            if (CampPlayers.TryGetValue(campId, out var playerIds))
            {
                for (var i = 0; i < playerIds.Count; i++)
                {
                    var playerId = playerIds[i];
                    var player = GetPlayerById(playerId);
                    if (player != null)
                    {
                        var prop = PlayerFrameUtil.GetProp(player, frame);
                        if (!prop.IsDead)
                        {
                            totalHp += prop.Hp;
                        }
                    }
                }
            }
            return totalHp;
        }

        internal void SetPlayer(Player player)
        {
            Log.D("CharacterManagerExtChars.SetCharacter", player.PlayerId);
            Players[player.PlayerId] = player;
            if (PlayerIds.Contains(player.PlayerId) == false)
            {
                PlayerIds.Add(player.PlayerId);
            }
        }

        /// <summary>
        /// 一个场景内的角色id需要唯一
        /// </summary>
        /// <param name="playerId"></param>
        /// <returns></returns>
        internal Player GetPlayerById(ulong playerId)
        {
            if (Players.ContainsKey(playerId))
            {
                return Players[playerId];
            }
            return null;
        }

        /// <summary>
        /// 使用列表序号获取角色
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        internal Player GetPlayerAtIndex(int index)
        {
            if (index >= 0 && index < PlayerIds.Count)
            {
                return GetPlayerById(PlayerIds[index]);
            }
            return null;
        }
    }
}