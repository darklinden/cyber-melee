using System.Runtime.CompilerServices;
using Google.FlatBuffers;
using Proto;
using Cysharp.Threading.Tasks;
using UserInterface;
using System.Collections.Generic;

namespace Service
{
    public class BattleEnded : AbstractDataAccess
    {
        public BattleEnded(ServiceSystem serviceSystem) : base(serviceSystem)
        {
        }

        public override DataServiceAccessType AccessType => DataServiceAccessType.WebSocket;

        public async UniTask AsyncRequest(ulong[] result)
        {
            Toast.ShowLoading();

#if SERVICE_LOG
            Log.D("BattleEnded AsyncRequest", result);
#endif

            if (await PrepareRequest() == false) return;

            var builder = FlatBufferBuilder.InstanceDefault;

            var resultOffset = RequestBattleEnd.CreateWinCampRankVectorBlock(builder, result);
            var reqOffset = RequestBattleEnd.CreateRequestBattleEnd(builder, resultOffset);
            builder.Finish(reqOffset.Value);

            var bb = await Pinus.AsyncRequest(Structs.Lockstep.BattleEnd.route, builder);

            Toast.HideLoading();

            return;
        }
    }
}