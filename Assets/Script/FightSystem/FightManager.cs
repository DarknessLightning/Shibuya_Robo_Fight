using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.GraphView.GraphView;

[System.Serializable]
public class PlayerUI
{
    public Image Profile;
    public Transform ModelPos;
    public Animator ModelAnimator;
    public Image HPBar;
    public Text APText;
    public Text SPText;
    public Text CardAmount;

}

[System.Serializable]
public class PlayerData
{
    public PlayerUI ui;
    public CharacterData character;
    public int CurrentHP;
    public int AbilityPoints = 0;
    public int SkillPoints = 0;
    public List<AbilityCard> Cards;
    //public List<BuzzTile> Tiles;
    public int additionalReroll = 0;
    public int additionalDice = 0;
    public bool isAI;
    public PlayerData opponent;
}

public class FightManager : MonoBehaviour
{
    public static FightManager instance;
    [Header("Panel References")]
    public GameObject CardDraftingPanel;
    public GameObject BuyCardPanel;
    public GameObject ChooseFieldPanel;
    public GameObject PlaceTilePanel;
    public GameObject DicePanel;
    public GameObject HpPanel;

    [Header("Camera Pivots")]
    public Transform BirdsEyeView;
    public Transform FacingPlayer;
    public Transform FacingAI;
    public Transform OverPlayerShoulder;
    public Transform OverAIShoulder;
    public Transform AboveDiceTray;

    [Header("Script References")]
    public DiceManager diceManager;
    public CardDraftingSystem cardDraftingSystem;
    public TokenMovement tokenMovement;

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
    private List<DiceFace> results = new();
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
        PlayerTurn = Player;
        Player.opponent = AI;
        AI.opponent = Player;
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
            ChangeTurn();
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

    public IEnumerator NextPhase()
    {
        yield return new WaitForSeconds(2f);
        ActionPhase();
    }

    private void StartDicePhase()
    {
        SetCameraPos(AboveDiceTray);
        HpPanel.SetActive(false);
        DicePanel.SetActive(true);
        if(diceManager.centerPosition.x != diceManager.diceTray.position.x)
        {
            diceManager.Init();
        }
        diceManager.startDice();
    }

    public void ResolveDice(List<DiceFace> diceFaces)
    {
        foreach(DiceFace result in diceFaces)
        {
            results.Add(result);
        }
        DicePanel.SetActive(false);
        HpPanel.SetActive(true);
        //Enqueue(NextPhase());
        ActionPhase();
    }

    private void ResolveDiceEffect(int index)
    {
        SetCameraPos(BirdsEyeView);
        switch (index)
        {
            case 0: specialSkill();
                break;
            case 1: HpManipulation(DiceFace.Heal);
                break;
            case 2: HpManipulation(DiceFace.Attack);
                break;
            case 3: Charge();
                break;
            case 4: //Tug Of War
                break;
            default: ActionPhase();
                return;
        }
        ResolveDiceEffect(index + 1);
    }

    private void specialSkill()
    {
        int count = 0;
        foreach(DiceFace result in results)
        {
            if(result == DiceFace.Power)
            {
                count++;
            }
        }

        switch (PlayerTurn.character.skill)
        {
            case SpecialSkill.SS001:
                if (count >= 3) MoveTracker(1, 1, PlayerTurn);
                break;
            case SpecialSkill.SS002:
                if (count >= 2) HpChange(PlayerTurn.opponent, -3);
                break;
            case SpecialSkill.SS003:
                if (count >= 1) EnergyMultiplier = count;
                break;
            case SpecialSkill.SS004:
                if(count >= 3) PlayerTurn.additionalDice += 1;
                break;
            default:
                break;
        }
    }

    private void HpManipulation(DiceFace face)
    {
        int multiply = 1;
        PlayerData target = PlayerTurn;
        if(face == DiceFace.Attack)
        {
            multiply = -1;
            target = PlayerTurn.opponent;
        }
        int count = 0;
        foreach(DiceFace result in results)
        {
            if(result == face)
            {
                count++;
            }
        }
        count *= multiply;
        Enqueue(HPBarChange(target, count));
    }

    public void HpChange(PlayerData target, int count)
    {
        Enqueue(HPBarChange(target, count));
    }

    private IEnumerator HPBarChange(PlayerData target, int deltaHealth)
    {
        int tempHealth = target.CurrentHP;

        target.CurrentHP += deltaHealth;
        while (Mathf.Abs(tempHealth - target.CurrentHP) > 0)
        {
            tempHealth = tempHealth < target.CurrentHP ? tempHealth + 1 : tempHealth - 1;
            float fillAmount = (float)tempHealth / target.character.hp;
            target.ui.HPBar.fillAmount = fillAmount;
            yield return new WaitForSeconds(0.5f);
        }
        /*
        if (deltaHealth != 0)
        {
            triggerCardEffect(
                deltaHealth > 0 ? TriggerEvent.OnAdd : TriggerEvent.OnSubtract,
                target == players[playerIndex] ? playerIndex : (playerIndex + 1) % 2,
                PlayerState.HealthPoint,
                Mathf.Abs(deltaHealth));
        }
        */
    }

    public void Charge()
    {
        int count = 0;
        foreach(DiceFace result in results)
        {
            if(result == DiceFace.Charge) { count++; }
        }
        Enqueue(charge(PlayerTurn, count));
    }

    public void ApChange(PlayerData target, int ApAmount)
    {
        Enqueue(charge(target, ApAmount));
    }

    public IEnumerator charge(PlayerData target, int ApAmount)
    {
        int currentEnergy = target.AbilityPoints;
        target.AbilityPoints = ApAmount * EnergyMultiplier;

        EnergyMultiplier = 1;

        while(currentEnergy != target.AbilityPoints)
        {
            currentEnergy = currentEnergy < target.AbilityPoints ? currentEnergy + 1 : currentEnergy - 1;
            target.ui.APText.text = currentEnergy.ToString();
            yield return new WaitForSeconds(0.2f);
        }
    }

    public void TugOfWarStart()
    {
        int FameCount = 0;
        int DestructionCount = 0;
        foreach(DiceFace result in results)
        {
            if(result == DiceFace.Fame) { FameCount++; }
            else if(result == DiceFace.Destruction) { DestructionCount++; }
        }

        int FamePoint = Mathf.Max(FameCount - 2, 0);
        int DestructionPoint = Mathf.Max(DestructionCount - 2, 0);

        MoveTracker(FamePoint, DestructionPoint, PlayerTurn);
    }

    public void MoveTracker(int deltaFame, int deltaDestruction, PlayerData target)
    {
        if(target == Player)
        {
            deltaFame *= -1;
            deltaDestruction *= -1;
        }

        FameIndex = Mathf.Clamp(FameIndex + deltaFame, 0, 14);
        DestructionIndex = Mathf.Clamp(DestructionIndex + deltaDestruction, 0, 14); 

        if(deltaFame != 0 ||  deltaDestruction != 0)
        {
            Enqueue(tokenMovement.MovePhase(deltaFame, deltaDestruction));
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
        Destroy(player.ui.ModelPos.GetChild(0).gameObject);
        GameObject model = Instantiate(player.character.characterModel, player.ui.ModelPos);
        player.ui.ModelAnimator = model.GetComponent<Animator>();
    }

    private void ChangeTurn()
    {
        PlayerTurn = (PlayerTurn == null || PlayerTurn == AI) ? Player : AI;
        results.Clear();
        EnergyMultiplier = 1;
    }

    public void Enqueue(IEnumerator routine)
    {
        if (routine == null) return;

        routineQueue.Enqueue(routine);

        if (!isRunningCoroutine)
        {
            StartCoroutine(ProcessQueue());
        }
    }

    public IEnumerator ProcessQueue()
    {
        isRunningCoroutine = true;
        
        while (routineQueue.Count > 0) 
        { 
            yield return StartCoroutine(routineQueue.Dequeue());
        }
        isRunningCoroutine = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
