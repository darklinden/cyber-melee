namespace Service
{
    public partial interface IDataServices
    {
        public WsConnect WsConnect { get; }
    }

    public partial class DataServices
    {
        // 添加自动加载的DAO
        private WsConnect m_WsConnect;
        public WsConnect WsConnect
        {
            get
            {
                if (m_WsConnect == null)
                {
                    m_WsConnect = new WsConnect(Ctx);
                    AddDataAccess(m_WsConnect);
                }
                return m_WsConnect;
            }
        }
    }
}