using System.Diagnostics;
using System.Collections.Generic;
using System;
using ReproducibleRandomGenerator;

public class SeedRandom
{
    private static SeedRandom sm_Instance { get; set; }
    private static SeedRandom Instance
    {
        get
        {
            if (sm_Instance == null)
            {
                sm_Instance = new SeedRandom();
            }
            return sm_Instance;
        }
    }

    private Dictionary<ulong, RandomContainer> RandomContainers { get; set; }
    private List<RandomContainer> ContainerPool { get; set; }
    private SeedRandom()
    {
        ContainerPool = new List<RandomContainer>();
        RandomContainers = new Dictionary<ulong, RandomContainer>();
    }

    public static RandomContainer GetOrAddContainer(ulong userId)
    {
        RandomContainer container = null;
        if (!Instance.RandomContainers.TryGetValue(userId, out container))
        {
            if (Instance.ContainerPool.Count > 0)
            {
                container = Instance.ContainerPool[0];
                Instance.ContainerPool.RemoveAt(0);
            }
            else
            {
                container = new RandomContainer();
            }
            Instance.RandomContainers.Add(userId, container);
        }
        return container;
    }

    public static void ClearContainer(ulong userId)
    {
        RandomContainer container = null;
        if (Instance.RandomContainers.TryGetValue(userId, out container))
        {
            container.Reset();
            Instance.RandomContainers.Remove(userId);
            Instance.ContainerPool.Add(container);
        }
    }

    public static void InitState(ulong userId, SeedType seedType, ulong seed)
    {
        GetOrAddContainer(userId).SetSeed((int)seedType, seed);
    }

    public static void InitAllState(ulong userId, ulong seed)
    {
        var container = GetOrAddContainer(userId);
        foreach (SeedType seedType in Enum.GetValues(typeof(SeedType)))
        {
            if (seedType == SeedType.None) continue;
            container.SetSeed((int)seedType, seed);
        }
    }

    public static SeedRandomInstance Rand(ulong userId, SeedType seedType)
    {
        return GetOrAddContainer(userId).GetSeedRandomInstance((int)seedType);
    }

    public static ValueProcessor Value(ulong userId, SeedType seedType)
    {
        return GetOrAddContainer(userId).GetSeedRandomInstance((int)seedType).Value;
    }

    [Conditional("DEBUG")]
    public static void LogSeeds(ulong userId, SeedType seedType)
    {
        var container = GetOrAddContainer(userId);
        var seed = container.GetSeed((int)seedType);
        var index = container.GetIndex((int)seedType);
        Log.D("SeedRandom LogSeeds", userId, seedType, seed, index);
    }

    [Conditional("DEBUG")]
    public static void LogSeeds()
    {
        foreach (var item in Instance.RandomContainers)
        {
            foreach (SeedType seedType in Enum.GetValues(typeof(SeedType)))
            {
                if (seedType == SeedType.None) continue;
                var seed = item.Value.GetSeed((int)seedType);
                var index = item.Value.GetIndex((int)seedType);
                if (seed != 0 && index != 0)
                {
                    Log.D("SeedRandom LogSeeds", item.Key, seedType, seed, index);
                }
            }
        }
    }
}