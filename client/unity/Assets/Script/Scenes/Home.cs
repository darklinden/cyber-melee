using System.Collections;
using System.Collections.Generic;
using App;
using TMPro;
using UnityEngine;
using UserInterface;
using Wtf;
using Cysharp.Threading.Tasks;

public class Home : MonoBehaviour
{
    [SerializeField] private TMP_InputField InputFieldName;
    [SerializeField] private TMP_Dropdown DropdownCharacter;

    public void OnBtnStartClicked()
    {
        Log.D("Home", "OnBtnStartClicked");

        if (string.IsNullOrEmpty(InputFieldName.text))
        {
            UILoader.Show(PanelTips.PanelPath, new PanelTips.Data { Content = "请输入用户名" });
            return;
        }

        var dropdownIndex = DropdownCharacter.value;
        var characterId = CharacterIds[dropdownIndex];

        var game = Context.Inst.GetSystem<GameCtrl>();
        game.RequestEnterBattle(InputFieldName.text, characterId).Forget();
    }

    public void OnDropdownCharacterChanged()
    {
        var index = DropdownCharacter.value;
        Log.D("Home", "OnDropdownCharacterChanged", index);

        var characterId = CharacterIds[index];
        var character = Configs.Instance.CharacterData.CharacterDataDict[characterId];
        Log.D("Home", "OnDropdownCharacterChanged", character.Name);
    }

    private List<int> CharacterIds = new List<int>();
    void Start()
    {
        Log.D("Home", "Start");

        InputFieldName.text = Constants.DefaultNames[Random.Range(0, Constants.DefaultNames.Length)];

        DropdownCharacter.ClearOptions();
        var characters = Configs.Instance.CharacterData.CharacterDataDict;
        CharacterIds.Clear();
        CharacterIds.AddRange(characters.Keys);

        foreach (var characterId in CharacterIds)
        {
            var character = characters[characterId];
            DropdownCharacter.options.Add(new TMP_Dropdown.OptionData(character.Name));
        }

        DropdownCharacter.value = -1;
        DropdownCharacter.value = 0;

#if UNITY_SERVER
        Log.D("Home", "UNITY_SERVER Start");

        var game = Context.Inst.GetSystem<GameCtrl>();
        game.OnClickStart();

#endif
    }
}
