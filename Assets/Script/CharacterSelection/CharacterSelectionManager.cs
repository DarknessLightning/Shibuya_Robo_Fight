using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class CharacterSelectionManager : MonoBehaviour
{
    public static CharacterSelectionManager instance;

    public CharacterData[] allCharacter;
    private List<CharacterData> availableCharacter = new();
    public Button chooseCharaButton;
    public Sprite selectBtnSprite;
    public Sprite cancelBtnSprite;
    //private string SpecialSkill = "Special Skill: \n";
    private bool startingFight = false;

    [Header("Player Selection Panel")]
    public CharacterSelectionPanel playerPanel;


    [Header("AI Selection Panel")]
    public CharacterSelectionPanel enemyPanel;

    [Header("Session Data")]
    public GameSessionData sessionData;

    [Header("Sprite Reference")]
    public Sprite[] notSelected;
    public Sprite[] selectedSprite;

    [Header("Audio Reference")]
    public AudioClip SelectSfx;

    private void Awake()
    {
        if(instance == null)
        {
            instance = this;
        }
        if(instance != this)
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        instance = null;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allCharacter = Resources.LoadAll<CharacterData>("Character"); 

        foreach (CharacterData character in allCharacter)
        {
            if (character.characterModel == null) continue;

            availableCharacter.Add(character);
        }

    }

    public void bothPanelConfirmed()
    {
        AudioManager.instance.PlaySfx(SelectSfx);
        if(!playerPanel.confirm || !enemyPanel.confirm) return;
        startingFight = true;

        sessionData.playerCharacter = playerPanel.selectedCharacter;
        sessionData.enemyCharacter = enemyPanel.selectedCharacter;
        StartCoroutine(startFight());
    }

    public bool isStartingFight()
    {
        return startingFight;
    }

    public IEnumerator startFight()
    {
        yield return new WaitForSeconds(2f);

        Debug.Log("Player Choose: " + playerPanel.selectedCharacter.name + "\n" +
            "AI Choose: " + enemyPanel.selectedCharacter.name);

        AudioManager.instance.PlayBattleMusic();
        SceneManager.LoadScene("SimpleFight");

    }

    public void Back2Menu()
    {
        if (startingFight) return;
        SceneManager.LoadScene("MainMenu");
    }

    // Update is called once per frame
    void Update()
    {
        /*
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
        //*/
    }
}
