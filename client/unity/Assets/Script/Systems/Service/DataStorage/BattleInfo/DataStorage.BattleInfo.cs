namespace Service
{
    public partial interface IDataStorage
    {
        public BattleInfo BattleInfo { get; }
    }

    public partial class DataStorage
    {
        private BattleInfo m_BattleInfo = null;
        public BattleInfo BattleInfo
        {
            get
            {
                if (m_BattleInfo == null)
                {
                    m_BattleInfo = new BattleInfo();
                    AddDataObject(m_BattleInfo);
                }
                return m_BattleInfo;
            }
        }
    }
}