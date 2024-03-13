using System.Collections;
using Cysharp.Threading.Tasks;

namespace FlatConfigs
{
    public interface IConfigLoader
    {
        // 加载优先级，越小越先加载
        int Priority { get; }
        bool LoadOnStart { get; }
        bool DataLoaded { get; }

        void Initialize(Handler configs);

        UniTask AsyncLoad();


        // 获取配置数据不声明函数返回值类型，因为不同的配置数据类型不同
        // UniTask<object> GetDataAsync(int id);
        // object GetDataDirect(int id);
    }
}