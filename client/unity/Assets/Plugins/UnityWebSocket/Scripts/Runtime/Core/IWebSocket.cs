using System;
using Wtf;

namespace UnityWebSocket
{
    public interface IWebSocket
    {
        void ConnectAsync();

        void CloseAsync();

        void SendAsync(XBuffer data);

        string Address { get; }

        string[] SubProtocols { get; }

        WebSocketState ReadyState { get; }

        event EventHandler<WSEventArgs> OnMessage;
    }
}
