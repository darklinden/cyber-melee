using System;
using Cysharp.Threading.Tasks;
using Google.FlatBuffers;
using Proto;
using UserInterface;

namespace Service
{

    public class PlayerReconnect : AbstractDataAccess
    {
        public PlayerReconnect(ServiceSystem serviceSystem) : base(serviceSystem)
        {
        }

        public override DataServiceAccessType AccessType => DataServiceAccessType.WebSocket;

        public async UniTask<bool> AsyncRequest()
        {
            Toast.ShowLoading();

#if SERVICE_LOG
            Log.D("PlayerReconnect AsyncRequest");
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
            var offset = RequestReconnect.CreateRequestReconnect(builder, Ctx.Data.PlayerInfo.PlayerId, Ctx.Data.PlayerInfo.Secret);
            builder.Finish(offset.Value);

            var bb = await Pinus.AsyncRequest(Structs.Lockstep.BattleReconnect.route, builder);

            var data = ResponseReconnect.GetRootAsResponseReconnect(bb);

#if SERVICE_LOG
            Log.D("PlayerReconnect.OnPlayerReconnect", data.UnPack());
#endif

            // 玩家登录成功
            Ctx.GameServiceStateChange(GameServiceState.GameLoginSuccess);

            // 字节流 归池
            bb.Dispose();

            return true;
        }

    }
}