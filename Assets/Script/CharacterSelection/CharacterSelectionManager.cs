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
}

public class CharacterSelectionManager : MonoBehaviour
{
    public CharacterData[] availableChara;
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
        availableChara = Resources.LoadAll<CharacterData>("Character");
        foreach(Transform t in player.Selection)
        {
            Destroy(t.gameObject);
        }
        foreach (Transform t in enemy.Selection)
        {
            Destroy(t.gameObject);
        }
        foreach(CharacterData character in availableChara)
        {
            if (character.characterSprite == null) continue;
            loadSelection(player, character);
            loadSelection(enemy, character);
        }
        selectChara(availableChara[0], player);
        selectChara(availableChara[0], enemy);
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
            selectChara(availableChara[index], selected);

        }
        if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            index = Mathf.Min(index + 1, availableChara.Length - 1);
            selectChara(availableChara[index], selected);
        }
        if(Input.GetKeyDown(KeyCode.LeftArrow))
        {
            selected = player;
        }
        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            selected = enemy;
        }
    }
}
