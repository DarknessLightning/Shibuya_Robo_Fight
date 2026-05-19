using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class CharacterSelectionUi
{
    public CharacterData character;
    public Transform Selection;
    public Image CharacterSprite;
    public Text CharacterHP;
    public Text SkillDescript;
    public bool confirm;
    public int index = 0;
}

public class CharacterSelectionManager : MonoBehaviour
{
    public CharacterData[] allCharacter;
    private List<CharacterData> availableCharacter = new();
    public Button chooseCharaButton;
    private string SpecialSkill = "Special Skill: \n";
    private int index = 0;
    private CharacterSelectionUi selected;

    [Header("Player Selection Panel")]
    public CharacterSelectionUi player;


    [Header("AI Selection Panel")]
    public CharacterSelectionUi enemy;


    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allCharacter = Resources.LoadAll<CharacterData>("Character");
        foreach(Transform t in player.Selection)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in enemy.Selection)
        {
            Destroy(t.gameObject);
        }
        foreach(CharacterData character in allCharacter)
        {
            if (character.characterSprite == null) continue;
            availableCharacter.Add(character);
            loadSelection(player, character);
            loadSelection(enemy, character);
        }
        selectChara(allCharacter[0], player);
        selectChara(allCharacter[0], enemy);
        selected = player;

    }

    private void loadSelection(CharacterSelectionUi selection, CharacterData chara)
    {
        CharacterData dt = chara;
        Button charaBtn = Instantiate(chooseCharaButton, selection.Selection);
        Image charaImg = charaBtn.GetComponent<Image>();
        charaImg.sprite = dt.icon;
        charaImg.preserveAspect = true;
        charaBtn.onClick.AddListener(() =>
        {
            selectChara(dt, selection);
        });
    }

    public void selectChara(CharacterData chara, CharacterSelectionUi selection)
    {
        if (selection.confirm) return;
        selection.character = chara;
        selection.CharacterSprite.sprite = chara.characterSprite;
        selection.CharacterHP.text = chara.hp.ToString();
        selection.SkillDescript.text = SpecialSkill + chara.skillDescription.ToString();
        selection.index = availableCharacter.IndexOf(chara);
    }

    public void confirmPlayer()
    {
        if (player.character == null) return;
        player.confirm = !player.confirm;
        bothConfirmed();
        
    }

    public void confirmEnemy()
    {
        if (enemy.character == null) return;
        enemy.confirm = !enemy.confirm;
        bothConfirmed();
        
    }

    public void bothConfirmed()
    {
        if(!player.confirm || !enemy.confirm) return;
        Debug.Log("Player Choose: " + player.character.name);
        Debug.Log("AI Choose: " + enemy.character.name);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            index = Mathf.Max(index - 1, 0);
            selectChara(allCharacter[index], selected);

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = Mathf.Min(index + 1, availableCharacter.Count - 1);
            selectChara(allCharacter[index], selected);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selected = player;
            index = selected.index;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selected = enemy;
            index = selected.index;
        }
    }
}
