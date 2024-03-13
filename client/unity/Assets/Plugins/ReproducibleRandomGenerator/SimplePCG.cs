// convert from https://github.com/lordofduct/spacepuppy-unity-framework-3.0/blob/18ce7778875037ea72c1a5df1e5ee79f02688411/SpacepuppyUnityFramework/Utils/RandomUtil.cs

namespace ReproducibleRandomGenerator
{
    /**
    https://zh.wikipedia.org/zh-cn/%E7%BD%AE%E6%8D%A2%E5%90%8C%E4%BD%99%E7%94%9F%E6%88%90%E5%99%A8
    置换同余生成器，简称PCG（英语：Permuted congruential generator）是一个用于产生伪随机数的算法，开发于2014年。
    该算法在线性同余生成器（LCG）的基础上增加了输出置换函数（output permutation function），以此优化LCG算法的统计性能。
    因此，PCG算法在拥有出色的统计性能的同时[1][2]，也拥有LCG算法代码小、速度快、状态小的特性。[3]

    置换同余生成器（PCG）和线性同余生成器（LCG）的差别有三点，在于：
        * LCG的模数以及状态大小比较大，状态大小一般为输出大小的二倍。
        * PCG的核心使用2的N次幂作为模数，以此实现一个非常高效的全周期、无偏差的伪随机数生成器，
        * PCG的状态不会被直接输出，而是经过输出置换函数计算后才输出。
        * 使用2的N次幂作为模数的LCG算法，普遍出现输出低位周期短小的问题，而PCG通过输出置换函数解决了这个问题。

    https://www.pcg-random.org/

     */

    public class SimplePCG : IRandom
    {
        #region Fields
        private ulong _seed; // state into seed
        private ulong _inc = 1;
        #endregion

        #region CONSTRUCTOR
        public SimplePCG() { }
        #endregion

        #region Methods

        public void SetSeed(ulong seed)
        {
            _seed = 0;
            _inc = 1;
            this.GetNext();
            _seed += (ulong)seed;
            this.GetNext();
        }

        public void SeekRand(int count)
        {
            int roll = count;
            while (roll > 0)
            {
                this.GetNext();
                roll--;
            }
        }

        public ulong CurrentSeed()
        {
            return _seed;
        }

        private uint GetNext()
        {
            ulong old = _seed;
            _seed = old * 6364136223846793005 + _inc;
            uint xor = (uint)(((old >> 18) ^ old) >> 27);
            int rot = (int)(old >> 59);
            return (xor >> rot) | (xor << (64 - rot));
        }

        #endregion

        #region IRandom Interface

        public double NextDouble()
        {
            return (double)this.GetNext() / (double)(0x100000000u);
        }

        public float Next()
        {
            return (float)((double)this.GetNext() / (double)(0x100000000u));
        }

        #endregion
    }
}