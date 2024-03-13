using System;
using System.Linq;
using System.Collections.Generic;

namespace ReproducibleRandomGenerator
{
    public class RandomContainer
    {
        protected Dictionary<int, SeedRandomInstance> m_SeedRandoms = new Dictionary<int, SeedRandomInstance>();

        public SeedRandomInstance GetSeedRandomInstance(int seedType)
        {
            SeedRandomInstance instance = null;
            if (!m_SeedRandoms.TryGetValue(seedType, out instance)) instance = null;
            return instance;
        }

        protected List<int> m_SeedSetted = new List<int>();

        public void Reset()
        {
            m_SeedSetted.Clear();
        }

        public void SetSeed(int seedType, ulong seed)
        {
            var ri = GetSeedRandomInstance(seedType);
            if (ri == null)
            {
                ri = new SeedRandomInstance(seedType);
                m_SeedRandoms.Add(seedType, ri);
            }
            if (!m_SeedSetted.Contains(seedType))
            {
                m_SeedSetted.Add(seedType);
            }
            ri.ResetSeed(seed);
        }

        public Int64 GetIndex(int seedType)
        {
            if (!m_SeedSetted.Contains(seedType))
            {
                return 0;
            }
            var ri = GetSeedRandomInstance(seedType);
            if (ri != null) return ri.Index;
            return 0;
        }

        public ulong GetSeed(int seedType)
        {
            if (!m_SeedSetted.Contains(seedType))
            {
                return 0;
            }
            var ri = GetSeedRandomInstance(seedType);
            if (ri != null) return ri.CurrentSeed;
            return 0;
        }

        public ValueProcessor Value(int seedType)
        {
            if (!m_SeedSetted.Contains(seedType))
            {
                throw new System.Exception("RandomContainer Value [" + seedType + "] Need To Set Seed First");
            }

            var ri = GetSeedRandomInstance(seedType);
            if (ri == null) throw new System.Exception("RandomContainer Value [" + seedType + "] No Container Found");

            return ri.Value;
        }
    }
}