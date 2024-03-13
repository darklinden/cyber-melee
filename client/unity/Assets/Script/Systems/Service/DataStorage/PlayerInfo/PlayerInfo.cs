using System;
using System.Collections.Generic;
using System.Text;
using App;
using Proto;

namespace Service
{
    public class PlayerInfo : IDataStorageObject
    {
        public ulong PlayerId { get; set; } = 0;
        public ulong Secret { get; set; } = 0;
        public string NickName { get; set; }
        public int CharacterId { get; set; }

        public bool IsValid => PlayerId != 0;

        public void Clear()
        {
            PlayerId = 0;
            Secret = 0;
            NickName = null;
        }

        internal void SetData(ulong playerId, ulong reconnectSecret, string username, int characterId)
        {
#if SERVICE_LOG
            Log.D("PlayerInfo SetData PlayerInfo");
#endif

            PlayerId = playerId;
            Secret = reconnectSecret;
            NickName = username;
            CharacterId = characterId;
        }
    }
}