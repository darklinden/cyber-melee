using System;

namespace Lockstep
{
    // 定义播放逻辑
    public interface IGamePlayFrame : IDisposable
    {
        GamePlayFrameType PlayType { get; }
    }
}