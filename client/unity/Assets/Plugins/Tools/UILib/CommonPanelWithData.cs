using UnityEngine;

namespace Wtf
{
    public abstract class CommonPanelWithData<DataType> : CommonPanel
    {
        public abstract void SetData(DataType data);
    }
}