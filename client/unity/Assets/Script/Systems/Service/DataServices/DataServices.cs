using System.Collections.Generic;

namespace Service
{
    public partial class DataServices : IDataServices
    {
        public ServiceSystem Ctx { get; }

        public List<AbstractDataAccess> HttpDAs { get; private set; }

        public List<AbstractDataAccess> WsDAs { get; private set; }


        public DataServices(ServiceSystem serviceSystem)
        {
            Ctx = serviceSystem;
        }

        public void Initialize()
        {
            if (HttpDAs == null)
            {
                HttpDAs = new List<AbstractDataAccess>();
            }

            if (WsDAs == null)
            {
                WsDAs = new List<AbstractDataAccess>();
            }
        }

        public void AddDataAccess(AbstractDataAccess dataAccess)
        {
            switch (dataAccess.AccessType)
            {
                case DataServiceAccessType.WebSocket:
                    WsDAs.Add(dataAccess);
                    break;
                case DataServiceAccessType.Http:
                    HttpDAs.Add(dataAccess);
                    break;
                default:
                    Log.E("Unknown DataAccessServiceType", dataAccess.AccessType);
                    break;
            }
        }

        public void Deinitialize()
        {
            foreach (var httpDa in HttpDAs)
            {
                httpDa.Deinitialize();
            }

            foreach (var wsDa in WsDAs)
            {
                wsDa.Deinitialize();
            }
        }
    }
}