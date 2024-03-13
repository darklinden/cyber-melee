using System.Collections.Generic;

namespace Service
{
    public partial interface IDataServices
    {
        List<AbstractDataAccess> HttpDAs { get; }
        List<AbstractDataAccess> WsDAs { get; }

        void Initialize();
        void AddDataAccess(AbstractDataAccess dataAccess);
        void Deinitialize();
    }
}