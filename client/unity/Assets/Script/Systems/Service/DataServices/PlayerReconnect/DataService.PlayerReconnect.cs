namespace Service
{
    public partial interface IDataServices
    {
        public PlayerReconnect PlayerReconnect { get; }
    }

    public partial class DataServices
    {
        // 添加自动加载的DAO
        private PlayerReconnect m_PlayerReconnect;
        public PlayerReconnect PlayerReconnect
        {
            get
            {
                if (m_PlayerReconnect == null)
                {
                    m_PlayerReconnect = new PlayerReconnect(Ctx);
                    AddDataAccess(m_PlayerReconnect);
                }
                return m_PlayerReconnect;
            }
        }
    }
}