namespace Service
{
    public partial interface IDataStorage
    {
        public PlayerInfo PlayerInfo { get; }
    }

    public partial class DataStorage
    {
        private PlayerInfo m_PlayerInfo = null;
        public PlayerInfo PlayerInfo
        {
            get
            {
                if (m_PlayerInfo == null)
                {
                    m_PlayerInfo = new PlayerInfo();
                    AddDataObject(m_PlayerInfo);
                }
                return m_PlayerInfo;
            }
        }
    }
}