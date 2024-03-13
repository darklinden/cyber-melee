using System.Collections.Generic;

namespace Service
{
    public partial class DataStorage : IDataStorage
    {
        public ServiceSystem Ctx { get; }

        public List<IDataStorageObject> DataStorageObjects { get; private set; } = new List<IDataStorageObject>(16);

        public DataStorage(ServiceSystem serviceSystem)
        {
            Ctx = serviceSystem;
        }

        public void Initialize()
        {
            ClearAll();
        }

        public void AddDataObject(IDataStorageObject dataStorageObject)
        {
            DataStorageObjects.Add(dataStorageObject);
        }

        public void ClearAll()
        {
            foreach (var dataStorageObject in DataStorageObjects)
            {
                dataStorageObject.Clear();
            }
        }

    }
}