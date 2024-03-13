using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using FixMath;
using Lockstep;
using UnityEngine;
using UnityEngine.AddressableAssets;
using Wtf;

namespace App
{
    public class Context : MonoBehaviourSingleton<Context>, ISystemBase
    {
        protected Transform m_Tm;
        public Transform Tm
        {
            get
            {
                if (m_Tm == null)
                {
                    m_Tm = transform;
                }
                return m_Tm;
            }
        }

        public bool IsInitialized { get; private set; } = false;

        protected override void OnAwake()
        {
            Log.D("Game Starting");

            Time.fixedDeltaTime = 0.02f;
            Time.maximumDeltaTime = 0.1f;

            Application.targetFrameRate = -1;

            Screen.sleepTimeout = SleepTimeout.NeverSleep;

            QualitySettings.antiAliasing = 0;

            Screen.sleepTimeout = -1;

            this.DoInitialize().Forget();
        }

        public Splash Splash { get; private set; }

        public EventBus EventBus { get; } = new EventBus();

        public Dictionary<string, ISystemBase> SubSystems { get; } = new Dictionary<string, ISystemBase>();

        public void Initialize() { }

        private async UniTask InitSplash()
        {
            if (Splash == null)
            {
                var prefab = await Resources.LoadAsync<GameObject>("Splash") as GameObject;
                var go = Instantiate(prefab, Tm, false);
                Splash = go.GetComponent<Splash>();
            }
            Splash.SetVisible(true); // 显示Splash
        }

        public async UniTask AsyncInitialize()
        {
            Log.D("AppContext.AsyncInitialize");

            await Addressables.InitializeAsync();

            this.AddSystem<AtlasLoader>();
            await this.GetSystem<AtlasLoader>().DoInitialize();

            await i18n.Configure();

            var ver = await Resources.LoadAsync<TextAsset>("Version") as TextAsset;
            if (ver != null)
            {
                Constants.VERSION = ver.text.Trim();
                Log.D("Version:", Constants.VERSION);
            }

            var versions = Constants.VERSION.Split('-');
            var MainVersion = versions[0];

            // 初始化Splash
            await InitSplash();

            await UserInterface.Toast.AsyncInitialize();

            Log.SetErrToast(null);

            Splash.UpdateBuildInfoVersion(Constants.VERSION);

            // 加载配置
            await Configs.Instance.AsyncLoadStart();


            Splash.AnimToProgress(0.99f, 2f);

            // EventSystem
            var eventSystem = await LoadUtil.AsyncLoad<GameObject>("Assets/Addrs/en-US/Prefabs/EventSystem.prefab");
            this.AddExistSystem<EventSystemComponent>(eventSystem);

            // 加载时间管理器
            this.AddSystem<TimeSystem>();

            // 加载游戏控制器
            this.AddSystem<GameCtrl>();

            // 加载服务系统
            this.AddSystem<Service.ServiceSystem>();

            // 加载战斗系统
            this.AddSystem<Battle.BattleSystem>();

            this.AddSystem<Battle.ProjectileCalcSystem>();

            this.AddSystem<LockStepSystem>();

            await this.InitializeSystems();

            // 加载 Home 页
            await LoadUtil.AsyncLoadScene("Assets/Addrs/en-US/Scenes/Home.unity");

            await Splash.AsyncAnimateToTransparent();

            IsInitialized = true;
        }

        public void Deinitialize()
        {
            Pinus.CleanUp();
            this.DeinitializeSystems();
            IsInitialized = false;
        }

        public override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            Deinitialize();
        }
    }
}
