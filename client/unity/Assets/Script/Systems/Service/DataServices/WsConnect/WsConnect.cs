using App;
using Cysharp.Threading.Tasks;

namespace Service
{
    public class WsConnect : AbstractDataAccess
    {
        public override DataServiceAccessType AccessType => DataServiceAccessType.WebSocket;
        public bool IsInitialized { get; protected set; } = false;
        public bool IsReady { get; protected set; } = false;
        public bool IsConnecting { get; protected set; } = false;

        public WsConnect(ServiceSystem serviceSystem) : base(serviceSystem)
        {
        }

        public override void Initialize()
        {
            if (IsInitialized) return;
            IsInitialized = true;

            Pinus.EventBus.OnHandshakeOver += OnShouldSendMessage;
            Pinus.EventBus.OnClosed += OnClosed;
            Pinus.EventBus.OnError += OnError;
            Pinus.EventBus.OnHandshakeError += OnHandshakeError;
            Pinus.EventBus.OnBeenKicked += OnBeenKicked;
        }

        public override void Deinitialize()
        {
            if (IsInitialized == false) return;
            IsInitialized = false;

            IsReady = false;
            IsConnecting = false;

            Pinus.EventBus.OnHandshakeOver -= OnShouldSendMessage;
            Pinus.EventBus.OnClosed -= OnClosed;
            Pinus.EventBus.OnError -= OnError;
            Pinus.EventBus.OnHandshakeError -= OnHandshakeError;
            Pinus.EventBus.OnBeenKicked -= OnBeenKicked;
        }

        // ---------------- Pinus Error ----------------
        private void OnBeenKicked(string url)
        {
            Log.W("WsConnect", "OnBeenKicked");
            Ctx.GameServiceStateChange(GameServiceState.GameKicked);
            IsReady = false;
            IsConnecting = false;

            // App.CmdKickToLogin.ExecuteStatic();
        }

        private void OnHandshakeError(string url, string e)
        {
            Log.W("WsConnect", "OnHandshakeError", e);
            Ctx.GameServiceStateChange(GameServiceState.GameLoginFailed);
            IsReady = false;
            IsConnecting = false;
        }

        private void OnError(string url, string e)
        {
            Log.W("WsConnect", "OnError", e);
            Ctx.GameServiceStateChange(GameServiceState.GameLoginFailed);
            IsReady = false;
            IsConnecting = false;
        }

        private void OnClosed(string url)
        {
            Log.W("WsConnect", "OnClosed");
            Ctx.GameServiceStateChange(GameServiceState.Disconnected);
            IsReady = false;
            IsConnecting = false;
        }

        // ---------------- Pinus Error ----------------

        private void OnShouldSendMessage(string url)
        {
#if SERVICE_LOG
            Log.D("WsConnect.OnShouldSendMessage", url);
#endif

            IsReady = true;
            IsConnecting = false;
        }

        public async UniTask<bool> AsyncConnect()
        {

#if SERVICE_LOG
            Log.D("WsConnect AsyncConnect");
#endif

            Initialize();

            IsReady = false;
            IsConnecting = true;

            Ctx.GameServiceStateChange(GameServiceState.GameLogin);
            Pinus.Connect(Constants.SERVICE_URL, false);

            while (IsConnecting)
            {
                await UniTask.Yield();
            }

            return IsReady;
        }

        public async UniTask AsyncDisconnect()
        {
#if SERVICE_LOG
            Log.D("WsConnect AsyncDisconnect");
#endif

            Deinitialize();
            Pinus.Disconnect();
            Ctx.GameServiceStateChange(GameServiceState.Disconnected);
            await UniTask.Yield();
        }
    }
}