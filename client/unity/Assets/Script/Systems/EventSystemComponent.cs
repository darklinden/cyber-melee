using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;
using Wtf;

public class EventSystemComponent : SystemAbstractInit
{
    public UnityEngine.EventSystems.EventSystem EventSystem;
    public UnityEngine.EventSystems.BaseInputModule InputModule;

    private void Awake()
    {
        EventSystem = GetComponent<UnityEngine.EventSystems.EventSystem>();
        InputModule = GetComponent<UnityEngine.EventSystems.BaseInputModule>();
    }
}
