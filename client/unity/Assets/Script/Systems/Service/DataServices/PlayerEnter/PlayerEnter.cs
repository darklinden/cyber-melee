using System;
using Cysharp.Threading.Tasks;
using Google.FlatBuffers;
using Proto;
using UserInterface;

namespace Service
{

    public class PlayerEnter : AbstractDataAccess
    {
        public PlayerEnter(ServiceSystem serviceSystem) : base(serviceSystem)
        {
        }

        public override DataServiceAccessType AccessType => DataServiceAccessType.WebSocket;

        public async UniTask<bool> AsyncRequest(string username, int characterId)
        {
            Toast.ShowLoading();

#if SERVICE_LOG
            Log.D("PlayerEnter AsyncRequest");
#endif

            bool wsReady = Ctx.Service.WsConnect.IsReady;

            if (!wsReady)
            {
                // 如果已经在连接 等待连接成功
                while (Ctx.Service.WsConnect.IsConnecting)
                {
                    await UniTask.Delay(50, true, PlayerLoopTiming.TimeUpdate);
                }

                // 如果未连接 则连接
                if (!Ctx.Service.WsConnect.IsConnecting && !Ctx.Service.WsConnect.IsReady)
                {
                    wsReady = await Ctx.Service.WsConnect.AsyncConnect();
                }
            }

            if (!wsReady)
            {
                Toast.HideLoading();
                return false;
            }

            var builder = FlatBufferBuilder.InstanceDefault;
            var nameOffset = builder.CreateString(username);
            var other_info = RequestEnter.CreateOtherInfoVector(builder, new int[] { characterId });

            var offset = RequestEnter.CreateRequestEnter(builder, nameOffset, other_info);
            builder.Finish(offset.Value);

            var bb = await Pinus.AsyncRequest(Structs.Lockstep.PlayerEnter.route, builder);

            var data = ResponseEnter.GetRootAsResponseEnter(bb);

#if SERVICE_LOG
            Log.D("PlayerEnter.OnPlayerEnter", data.UnPack());
#endif

            // 玩家登录成功
            Ctx.Data.PlayerInfo.SetData(data.PlayerId, data.ReconnectSecret, username, characterId);
            Ctx.GameServiceStateChange(GameServiceState.GameLoginSuccess);

            // 字节流 归池
            bb.Dispose();

            return true;
        }

    }
}