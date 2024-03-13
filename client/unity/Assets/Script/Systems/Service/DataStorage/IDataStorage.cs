using System.Collections.Generic;

namespace Service
{
    public partial interface IDataStorage
    {
        List<IDataStorageObject> DataStorageObjects { get; }

        void Initialize();
        void AddDataObject(IDataStorageObject dataStorageObject);
        void ClearAll();
    }
}