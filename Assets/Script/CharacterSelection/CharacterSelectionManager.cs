using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class CharacterSelectionUi
{
    public CharacterData character;
    public Transform Selection;
    public Image CharacterSprite;
    public GameObject CharacterShowcase;
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

    [Header("Session Data")]
    public GameSessionData sessionData;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allCharacter = Resources.LoadAll<CharacterData>("Character"); 

        foreach (CharacterData character in allCharacter)
        {
            if (character.characterModel == null) continue;

            availableCharacter.Add(character);
            loadSelection(player, character);
            loadSelection(enemy, character);
        }
        selectChara(availableCharacter[0], player);
        selectChara(availableCharacter[0], enemy);
        selected = player;

    }

    private void loadSelection(CharacterSelectionUi selection, CharacterData chara)
    {
        CharacterData dt = chara;
        Button charaBtn = Instantiate(chooseCharaButton, selection.Selection);
        Transform profile = charaBtn.transform.Find("Profile");
        Image charaImg = profile.GetComponent<Image>();
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
        Destroy(selection.CharacterShowcase.transform.GetChild(0).gameObject);
        GameObject model = Instantiate(chara.characterModel, selection.CharacterShowcase.transform);
        //selection.CharacterSprite.sprite = chara.characterSprite;
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
        Debug.Log("Player Choose: " + player.character.name + "\n" + 
            "AI Choose: " + enemy.character.name);

        sessionData.playerCharacter = player.character;
        sessionData.enemyCharacter = enemy.character;

        SceneManager.LoadScene("SimpleFight");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            index = Mathf.Max(index - 1, 0);
            selectChara(availableCharacter[index], selected);

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = Mathf.Min(index + 1, availableCharacter.Count - 1);
            selectChara(availableCharacter[index], selected);
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
        if (Input.GetKeyDown(KeyCode.Return))
        {
            selected.confirm = !selected.confirm;
            if(selected.confirm)
            {
                selected = selected == player ? enemy : player;
            }
            index = selected.index;
            bothConfirmed();
        }
    }
}
