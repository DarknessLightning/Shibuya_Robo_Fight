using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class PlayerUI
{
    public Image Profile;
    public Image HPBar;
    public Text APText;
    public Text SPText;
    public Text CardAmount;

}

[System.Serializable]
public class PlayerData
{
    public PlayerUI ui;
    [HideInInspector]public CharacterData character;
    public int CurrentHP;
    public int AbilityPoints = 0;
    public int SkillPoints = 0;
    public List<AbilityCard> Cards;
    //public List<BuzzTile> Tiles;
    public int additionalReroll = 0;
    public int additionalDice = 0;
    public bool isAI;
}

public class FightManager : MonoBehaviour
{
    public static FightManager instance;
    [Header("Panel References")]
    public GameObject CardDraftingPanel;
    public GameObject BuyCardPanel;
    public GameObject ChooseFieldPanel;
    public GameObject PlaceTilePanel;

    [Header("Camera Pivots")]
    public Transform BirdsEyeView;
    public Transform FacingPlayer;
    public Transform FacingAI;
    public Transform OverPlayerShoulder;
    public Transform OverAIShoulder;

    [Header("Script References")]
    public DiceManager diceManager;

    [Header("Other References")]
    public Transform Camera;
    public Text PhaseAnnounce;
    public GameSessionData sessionData;

    [Header("Player Data")]
    public PlayerData Player;
    public PlayerData AI;
    private PlayerData PlayerTurn;

    private int GamePhase = -1;
    public int FameIndex = 7;
    public int DestructionIndex = 7;
    public int EnergyMultiplier = 1;
    public int GamePhaseLimit = 9;

    private readonly Queue<IEnumerator> routineQueue = new();
    private bool isRunningCoroutine = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (instance == null)
        {
            instance = this;
        }
        EnergyMultiplier = 1;
        Player.character = sessionData.playerCharacter;
        AI.character = sessionData.enemyCharacter;

        LoadSessionCharacter(Player);
        LoadSessionCharacter(AI);

        ActionPhase();
    }

    

    private void OnDestroy()
    {
        instance = null;
    }

    private void ActionPhase()
    {
        if (isRunningCoroutine) return;

        GamePhase++;
        if(GamePhase > GamePhaseLimit)
        {
            GamePhase = 0;
            //ChangeTurn();
        }
        string Phase = "";
        switch (GamePhase)
        {
            case 0: StartDicePhase(); Phase = "Dice Roll Phase";
                break;
            case 1: ResolveDiceEffect(0); Phase = "Resolve Dice Effect";
                break;
            case 2: CardDraftingPhase(); Phase = "Buy Card Phase";
                break;
            case 3: BuzzTilePhase(); Phase = "Placing Buzz Tile";
                break;
            default:
                break;
        }
        PhaseAnnounce.text = Phase;
    }

    private void StartDicePhase()
    {

    }

    private void ResolveDiceEffect(int index)
    {
        bool stillResolving = true;
        switch (index)
        {
            case 0: //specialSkill
                break;
            case 1: //HPManipulation(Heal)
                break;
            case 2: //HPManipulation(Attack)
                break;
            case 3: //Charge
                break;
            case 4: //Tug Of War
                break;
            default: ActionPhase();
                stillResolving = false;
                break;
        }
        if (stillResolving)
        {
            ResolveDiceEffect(index + 1);
        }
    }

    private void CardDraftingPhase()
    {

    }

    private void BuzzTilePhase()
    {

    }

    public void SetCameraPos(Transform pivot)
    {
        Camera.position = pivot.position;
        Camera.rotation = pivot.rotation;
    }

    private void LoadSessionCharacter(PlayerData player)
    {
        player.ui.Profile.sprite = player.character.icon;
        player.CurrentHP = player.character.hp;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
