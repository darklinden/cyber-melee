using System.Collections;
using System.Collections.Generic;
using App;
using Cysharp.Threading.Tasks;
using Google.FlatBuffers;
using UnityEngine;
using Wtf;

public class TimeSystem : MonoBehaviour, ISystemBase
{
    public enum TimeState
    {
        NORMAL,
        PAUSED,
        SLOWDOWN,
        SPEEDUP
    }

    public delegate void TimescaleChangeStarted(float timescale);
    public event TimescaleChangeStarted OnTimescaleChangeStarted;

    public delegate void TimeScaleChanged(float timescale);
    public event TimeScaleChanged OnTimescaleChanged;

    public Dictionary<string, ISystemBase> SubSystems => null;

    private Transform m_Tm;
    public Transform Tm => m_Tm != null ? m_Tm : m_Tm = transform;

    public bool IsInitialized { get; private set; } = false;

    public void Initialize()
    {
        m_timeChangeRoutine = UnityUtils.StartCoroutine(this, TimeScaleManipulationRoutine());
        OnTimescaleChangeStarted = null;
        OnTimescaleChanged = null;
        SetTimeScale(TIMESCALE_NORMAL);
        AsyncTimeSync().Forget();
        IsInitialized = true;
    }

    public UniTask AsyncInitialize()
    {
        throw new System.NotImplementedException();
    }

    public void Deinitialize()
    {
        IsInitialized = false;

        OnTimescaleChangeStarted = null;
        OnTimescaleChanged = null;

        UnityUtils.StopCoroutine(this, ref m_timeChangeRoutine);

        m_timeScaleState = TimeState.NORMAL;
        m_timeScaleStateInProcess = TimeState.NORMAL;
        m_timeScaleInProcess = TIMESCALE_NORMAL;

        SetTimeScale(TIMESCALE_NORMAL);
    }

    public const float TIMESCALE_NORMAL = 1f;
    public const float TIMESCALE_SLOWDOWN = 0.75f;
    public const float TIMESCALE_SPEEDUP = 1.5f;
    public const int TIMESCALE_CHANGE_FRAME_COUNT = 20;
    public const float TIMESCALE_PAUSED = 0.0001f;

    private Coroutine m_timeChangeRoutine;

    private TimeState m_timeScaleState = TimeState.NORMAL;
    public TimeState State => m_timeScaleState;

    private TimeState m_timeScaleStateInProcess = TimeState.NORMAL;
    private float m_timeScaleInProcess = -1;

    public void ChangeState(TimeState state, bool immediately = false)
    {
        if (m_timeScaleState == state && m_timeScaleStateInProcess == state)
        {
            return;
        }

        Log.D("TimeSystem.ChangeState:", state);
        m_timeScaleStateInProcess = state;
        switch (state)
        {
            case TimeState.NORMAL:
                m_timeScaleInProcess = TIMESCALE_NORMAL;
                break;
            case TimeState.SLOWDOWN:
                m_timeScaleInProcess = TIMESCALE_SLOWDOWN;
                break;
            case TimeState.SPEEDUP:
                m_timeScaleInProcess = TIMESCALE_SPEEDUP;
                break;
            case TimeState.PAUSED:
                m_timeScaleInProcess = TIMESCALE_PAUSED;
                break;
        }

        if (immediately)
        {
            m_timeScaleState = m_timeScaleStateInProcess;
            SetTimeScale(m_timeScaleInProcess);
            SetTimeScaleState(m_timeScaleState);
            return;
        }
    }

    private void SetTimeScale(float timescale)
    {
        if (timescale != Time.timeScale)
        {
            OnTimescaleChangeStarted?.Invoke(timescale);
            Time.timeScale = timescale;
            OnTimescaleChanged?.Invoke(timescale);
        }
    }

    private void SetTimeScaleState(TimeState state)
    {
        m_timeScaleState = state;
    }

    private IEnumerator TimeScaleManipulationRoutine()
    {
        while (true)
        {
            if (m_timeScaleState != m_timeScaleStateInProcess)
            {
                int frameCount = TIMESCALE_CHANGE_FRAME_COUNT;
                float fromScale = Time.timeScale;

                float toScale = m_timeScaleInProcess;
                var toState = m_timeScaleStateInProcess;

                for (int i = 1; i <= frameCount; i++)
                {
                    if (toState != m_timeScaleStateInProcess)
                    {
                        break;
                    }
                    SetTimeScale(Mathf.Lerp(fromScale, toScale, i / (float)frameCount));
                    yield return null;
                }

                SetTimeScale(toScale);
                SetTimeScaleState(toState);
            }
            yield return null;
        }
    }

    public long Ping { get; private set; } = 0;
    [SerializeField, ReadOnly] private long ServerTimeStamp = 0;
    [SerializeField, ReadOnly] private long ServerMatchedClientStamp = 0;
    public long ClientTimeDistanceMs { get; private set; } = 0;
    public bool HasSynced { get; private set; } = false;

    private async UniTask AsyncTimeSync()
    {
        const float SyncDelay = 5f;
        float timeCountDown = 0;
        while (true)
        {
            await UniTask.Yield();
            var ready = Pinus.Network != null && Pinus.Network.IsConnected // Network is connected
                && Pinus.Network.HandshakeEnded; // Handshake is done

            if (!ready)
            {
                continue;
            }

            timeCountDown -= Time.unscaledDeltaTime;
            if (timeCountDown > 0)
            {
                continue;
            }

            // send sync request
            var builder = FlatBufferBuilder.InstanceDefault;
            var rts = Proto.RequestTimeSync.CreateRequestTimeSync(builder, (ulong)TimeUtil.TickTimeMs);
            builder.Finish(rts.Value);

            var result = await Pinus.AsyncRequest(Structs.Lockstep.TimeSync.route, builder);

            var args = Proto.ResponseTimeSync.GetRootAsResponseTimeSync(result);

            var nowMs = TimeUtil.TickTimeMs;
            var backMs = (long)args.ClientTime;

            // 假设 发送耗时 = 接收耗时， 
            // 收到回复时 Ping 值 = (当前 timestamp - 发送时 timestamp) / 2
            Ping = (nowMs - backMs) / 2;

            // 服务器时间 = 收到的服务器时间 + Ping
            ServerTimeStamp = (long)args.ServerTime + Ping;
            ServerMatchedClientStamp = nowMs;

            // 客户端时间差 = 服务器时间 - 当前时间
            ClientTimeDistanceMs = ServerTimeStamp - nowMs;

            HasSynced = true;

            timeCountDown = SyncDelay;
        }
    }

    /// <summary>
    /// Calc Server Time In Milliseconds Since 1970, Need HasSynced
    /// </summary>
    /// <value></value>
    public long ServerTickTimeMs
    {
        get
        {
            if (!HasSynced)
            {
                Log.E("Access ServerTime Before Synced!");
                return -1;
            }

            // 当前 tick time
            return TimeUtil.TickTimeMs + ClientTimeDistanceMs;
        }
    }

    /// <summary>
    /// Calc Server Time In Milliseconds Since 1970, Need HasSynced
    /// </summary>
    /// <value></value>
    public long FrameServerTickTimeMs
    {
        get
        {
            if (!HasSynced)
            {
                Log.E("Access ServerTime Before Synced!");
                return -1;
            }

            // 当前帧的 tick time
            var tickNow = TimeUtil.TickTimeFrameMs;
            return tickNow + ClientTimeDistanceMs;
        }
    }

    public void ResetSync()
    {
        HasSynced = false;
    }

    [ContextMenu("StateToNormal")]
    public void StateToNormal()
    {
        ChangeState(TimeState.NORMAL);
    }

    [ContextMenu("StateToSlowdown")]
    public void StateToSlowdown()
    {
        ChangeState(TimeState.SLOWDOWN);
    }

    [ContextMenu("StateToSpeedup")]
    public void StateToSpeedup()
    {
        ChangeState(TimeState.SPEEDUP);
    }

    [ContextMenu("StateToPaused")]
    public void StateToPaused()
    {
        ChangeState(TimeState.PAUSED);
    }
}
