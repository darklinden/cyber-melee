using System;
using Wtf;

namespace Lockstep
{
    public class ListGamePlayFrame : IDisposable
    {
        public XList<IGamePlayFrame> Plays;

        public static ListGamePlayFrame Get()
        {
            var ins = AnyPool<ListGamePlayFrame>.Get();
            ins.Plays = XList<IGamePlayFrame>.Get(1);
            return ins;
        }

        public void Dispose()
        {
            Plays.Dispose();
            AnyPool<ListGamePlayFrame>.Return(this);
        }
    }
}