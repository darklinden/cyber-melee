using UnityEngine;
namespace App
{
    public class ContextLoader : MonoBehaviour
    {
        void Awake()
        {
            if (Context.Inst == null)
            {
                Instantiate(Resources.Load("AppContext"));
            }
        }
    }
}