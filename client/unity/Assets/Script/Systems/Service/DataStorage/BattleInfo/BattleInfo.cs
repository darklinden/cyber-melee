using System.Collections.Generic;
using Proto;

namespace Service
{
    public class BattleInfo : IDataStorageObject
    {
        public long RoomId { get; set; }
        public List<int> CampIds { get; } = new List<int>();
        public int FriendlyCampId { get; set; }
        public Dictionary<int, List<ulong>> CampPlayerIds { get; } = new Dictionary<int, List<ulong>>();
        public int HostileCampId { get; set; }
        public List<ulong> PlayerIds { get; } = new List<ulong>();
        public Dictionary<ulong, PlayerInfoT> PlayerInfoDict { get; } = new Dictionary<ulong, PlayerInfoT>();

        public List<ulong> FriendlyPlayerIds
        {
            get
            {
                if (FriendlyCampId == 0)
                {
                    return null;
                }
                return CampPlayerIds[FriendlyCampId];
            }
        }

        public List<ulong> HostilePlayerIds
        {
            get
            {
                if (HostileCampId == 0)
                {
                    return null;
                }
                return CampPlayerIds[HostileCampId];
            }
        }

        public void Clear()
        {
            RoomId = 0;
            CampIds.Clear();
            FriendlyCampId = 0;
            HostileCampId = 0;
            CampPlayerIds.Clear();
            PlayerIds.Clear();
            PlayerInfoDict.Clear();
        }

        internal void SetData(ServerBroadcastBattleStart result, ulong mePlayerId)
        {
            Clear();
            RoomId = result.RoomId;
            for (var i = 0; i < result.CampsLength; i++)
            {
                var campV = result.Camps(i);
                if (campV.HasValue)
                {
                    var camp = campV.Value;
                    var friendly = false;

                    List<ulong> PlayersOfCamp = new List<ulong>();

                    for (var j = 0; j < camp.PlayersLength; j++)
                    {
                        var playerV = camp.Players(j);
                        if (playerV.HasValue)
                        {
                            var player = playerV.Value.UnPack();
                            PlayerIds.Add(player.PlayerId);
                            PlayerInfoDict.Add(player.PlayerId, player);
                            PlayersOfCamp.Add(player.PlayerId);

                            if (player.PlayerId == mePlayerId)
                            {
                                friendly = true;
                            }
                        }
                    }

                    CampIds.Add(camp.CampId);
                    CampPlayerIds.Add(camp.CampId, PlayersOfCamp);

                    if (friendly)
                    {
                        FriendlyCampId = camp.CampId;
                    }
                    else
                    {
                        HostileCampId = camp.CampId;
                    }
                }
            }
            CampIds.Sort();
            PlayerIds.Sort();
        }
    }
}