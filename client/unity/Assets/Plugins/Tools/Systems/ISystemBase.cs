using System.Collections;
using Cysharp.Threading.Tasks;


namespace Wtf
{
    public interface ISystemBase
    {
        System.Collections.Generic.Dictionary<string, ISystemBase> SubSystems { get; }

        UnityEngine.Transform Tm { get; }

        void Initialize();
        UniTask AsyncInitialize();

        void Deinitialize();

        bool IsInitialized { get; }
    }
}