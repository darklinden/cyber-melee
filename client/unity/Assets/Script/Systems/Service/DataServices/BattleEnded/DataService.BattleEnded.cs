
namespace Service
{
    public partial interface IDataServices
    {
        public BattleEnded BattleEnded { get; }
    }

    public partial class DataServices
    {
        // 添加自动加载的DAO
        private BattleEnded m_BattleEnded;
        public BattleEnded BattleEnded
        {
            get
            {
                if (m_BattleEnded == null)
                {
                    m_BattleEnded = new BattleEnded(Ctx);
                    AddDataAccess(m_BattleEnded);
                }
                return m_BattleEnded;
            }
        }
    }
}