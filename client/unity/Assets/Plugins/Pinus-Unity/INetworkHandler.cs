using System;
using UnityWebSocket;
using Wtf;

namespace PinusUnity
{
    internal interface INetworkHandler
    {
        public void ConnectTimeout();
        public void OnOpen();
        public void OnRecv(XBuffer data);
        public void OnError(string err);
        public void OnClose();
    }
}