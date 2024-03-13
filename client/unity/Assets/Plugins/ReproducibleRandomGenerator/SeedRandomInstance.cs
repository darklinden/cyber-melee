using System;

namespace ReproducibleRandomGenerator
{
    public class SeedRandomInstance
    {
        public int SeedType;
        public ulong StartSeed { get; private set; }
        public Int64 Index { get; private set; }
        private IRandom Random { get; set; }

        private ValueProcessor m_ValueProcessor = null;
        public ValueProcessor Value
        {
            get
            {
                if (Random == null)
                {
                    Log.E("SeedRandomInstance Access ValueProcessor Before Random Setted!");
                }
                if (m_ValueProcessor == null)
                {
                    m_ValueProcessor = new ValueProcessor();
                    m_ValueProcessor.ValueFunc = NextDouble;
                }
                return m_ValueProcessor;
            }
        }

        public ulong CurrentSeed { get => Random.CurrentSeed(); }

        public double NextDouble()
        {
            Index++;
            return Random.NextDouble();
        }

        public SeedRandomInstance(int seedType)
        {
            SeedType = seedType;
            Random = new SimplePCG();
        }

        public void ResetSeed(ulong seed)
        {
            StartSeed = seed;
            Index = 0;
            Random.SetSeed(seed);
        }
    }
}