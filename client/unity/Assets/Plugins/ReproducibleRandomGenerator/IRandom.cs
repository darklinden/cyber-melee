namespace ReproducibleRandomGenerator
{
    public interface IRandom
    {
        void SetSeed(ulong seed);
        ulong CurrentSeed();
        void SeekRand(int count);
        double NextDouble();
        float Next();
    }
}
