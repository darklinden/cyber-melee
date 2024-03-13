namespace Service
{
    public partial interface IDataServices
    {
        public PlayerEnter PlayerEnter { get; }
    }

    public partial class DataServices
    {
        // 添加自动加载的DAO
        private PlayerEnter m_PlayerEnter;
        public PlayerEnter PlayerEnter
        {
            get
            {
                if (m_PlayerEnter == null)
                {
                    m_PlayerEnter = new PlayerEnter(Ctx);
                    AddDataAccess(m_PlayerEnter);
                }
                return m_PlayerEnter;
            }
        }
    }
}