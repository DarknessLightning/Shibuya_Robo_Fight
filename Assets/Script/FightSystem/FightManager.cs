using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[System.Serializable]
public class PlayerUI
{
    public Image Profile;
    public Transform ModelPos;
    public AnimationScript ModelAnimator;
    public Image HPBar;
    public Text APText;
    public Text CardAmount;
    public GameObject MainModel;
    public GameObject VictoryModel;
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
    public BuzzTile Tile = null;
    //public List<BuzzTile> Tiles;
    public int additionalReroll = 0;
    public int additionalDice = 0;
    public bool isAI;
    public PlayerData opponent;
    public Sprite win;
    public int EndTileIndex;
}

public enum winCondition
{
    None, 
    Kill, 
    Fame,
    Destruction,
    Spotlight, 
    Surrender
}

public class FightManager : MonoBehaviour
{
    public static FightManager instance;
    [Header("Panel References")]
    public GameObject CardDraftingPanel;
    public GameObject BuyCardPanel;
    public GameObject PlaceTilePanel;
    public GameObject DicePanel;
    public GameObject HpPanel;
    public GameObject PhaseAnnouncePanel;
    public GameObject SpecialSkillPanel;
    public GameObject GameOverPanel;
    public GameObject SkillPointPopUp;
    public GameObject PausePanel;
    public GameObject ExitConfirmationPanel;
    public GameObject PlayerInputBlocker;
    public GameObject PlayerCardButtonPanel;
    public GameObject PlayerBuyButtonPanel;

    [Header("Camera Pivots")]
    public Transform BirdsEyeView;
    public Transform FacingPlayer;
    public Transform FacingAI;
    public Transform OverPlayerShoulder;
    public Transform OverAIShoulder;
    public Transform AboveDiceTray;
    public Transform InFrontOfPlayer;
    public Transform InFrontOfAI;
    public Transform BehindPlayer;
    public Transform BehindAI;
    public Transform PlayerLoseShot;
    public Transform AILoseShot;
    public Transform PlayerLost;
    public Transform AILost;

    [Header("Orbit Settings")]
    public float orbitRadius = 5f;
    public float orbitSpeed = 45f; // Derajat per detik
    public float orbitDuration = 5f;

    [Header("Transition Settings")]
    public float transitionDuration = 3f;

    [Header("Script References")]
    public DiceManager diceManager;
    public CardDraftingSystem cardDraftingSystem;
    public TokenMovement tokenMovement;
    public BuzzTilePlacing buzzTilePlacing;
    public AILogic aiLogic;

    [Header("Other References")]
    public Transform Kamera;
    public Image PhaseAnnounce;
    public GameSessionData sessionData;
    public Sprite[] PhaseAnnounceSprite;
    public Image GameOver;
    public Text SkillPointText;
    public AudioClip AbilityCardEffect;
    public AudioClip ChangeTurnSfx;
    public Text TurnAnnounce;
    public AudioClip PlayerWin;
    public AudioClip PlayerLose;
    public AudioClip fameSfx;
    public AudioClip destructionSfx;

    [Header("Player Data")]
    public PlayerData Player;
    public PlayerData AI;
    public PlayerData PlayerTurn = null;

    private int GamePhase = -1;
    public int FameIndex = 7;
    public int DestructionIndex = 7;
    public int EnergyMultiplier = 1;
    public int GamePhaseLimit = 9;
    public float playTime = 0f;
    public bool playerWin = false;

    private readonly Queue<IEnumerator> routineQueue = new();
    private bool isRunningCoroutine = false;
    private List<DiceFace> results = new();
    private bool gameOver = false;
    private PlayerData winner = null;
    public winCondition win = winCondition.None;

    [Header("Game Over Text and Sprite")]
    public Sprite PlayerPlayTime;
    public Sprite EnemyPlayTime;
    public Sprite PlayerWinCon;
    public Sprite EnemyWinCon;
    public Sprite VictorySprite;
    public Sprite DefeatSprite;
    //Win Condition Sprite;
    public Sprite HPWincon; //Attack
    public Sprite FameWinCon; //Fame
    public Sprite DestrucWinCon; //Destruction
    public Sprite SpotLightWinCon; //Fame and Destruction

    [Header("Game Over Panel UI Elements")]
    public Image PlayTimeBox;
    public Text PlayTimeText;
    public Image WinConBox;
    public Image WinConSymbol;
    public Text WinConText;
    public Image PlayerWinState;

    [Header("Simulasi")]
    public bool simulasi = false;
    public bool slowDown = false;
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
        
        /*
        Player.CurrentHP = 4;
        Player.ui.HPBar.fillAmount = (float)Player.CurrentHP / Player.character.hp;
        AI.CurrentHP = 4;
        AI.ui.HPBar.fillAmount = (float)AI.CurrentHP / AI.character.hp;
        */

        //ActionPhase();
        PlayerTurn = Player;
        Player.opponent = AI;
        AI.opponent = Player;
        if (simulasi)
        {
            ActionPhase();
        }
        else if (slowDown)
        {
            ActionPhase();
        }
        else
        {
            StartCoroutine(MoveCameraSequence());
        }
    }
    
    private void OnDestroy()
    {
        instance = null;
    }

    //------------//
    //PHASE CHANGE//
    //------------//

    private void ActionPhase()
    {

        GamePhase++;
        if(GamePhase > GamePhaseLimit)
        {
            ChangeTurn();
        }
        switch (GamePhase)
        {
            case 0: StartTurn(); 
                break;
            case 1: 
                ResolveDiceEffect(DiceFace.Power); 
                break;
            case 2: CardDraftingPhase();
                break;
            case 3: BuzzTilePhase();
                break;
            default:
                break;
        }
        PhaseAnnounce.sprite = PhaseAnnounceSprite[Mathf.Min(GamePhase, PhaseAnnounceSprite.Length - 1)];
    }

    public IEnumerator TurnAnnouncer()
    {
        TurnAnnounce.text = PlayerTurn == Player ? "YOUR TURN" : "ENEMY TURN";

        Color temp = TurnAnnounce.color;
        temp.a = 0;
        float elapsed = 0f;
        while (elapsed < 1)
        {
            elapsed += Time.deltaTime;
            temp.a = elapsed;
            TurnAnnounce.color = temp;
            yield return null;
        }
        temp.a = 1;
        TurnAnnounce.color = temp;
        yield return new WaitForSeconds(1f);
        while(elapsed > 0)
        {
            elapsed -= Time.deltaTime;
            temp.a = elapsed;
            TurnAnnounce.color = temp;
            yield return null;
        }
        temp.a = 0;
        TurnAnnounce.color = temp;
    }

    public IEnumerator NextPhase()
    {
        SetCameraPos(BirdsEyeView);
        yield return new WaitForSeconds(0.5f);
        ActionPhase();
    }

    public IEnumerator WaitCoroutine()
    {
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(
            () => !isRunningCoroutine);

        if (gameOver) yield break;
        yield return NextPhase();
    }

    public void StartTurn()
    {
        PlayerInputBlocker.SetActive(PlayerTurn.isAI);
        TriggerCardEffect(PlayerTurn, TriggerEvent.TurnStart, PlayerState.Turn);
        Enqueue(startDice());
    }

    public IEnumerator startDice()
    {
        Debug.Log("Start Dice");
        StartDicePhase();
        StartCoroutine(TurnAnnouncer());
        yield return null;
    }

    //----------//
    //DICE PHASE//
    //----------//

    private void StartDicePhase()
    {
        SetCameraPos(AboveDiceTray);
        HpPanel.SetActive(false);
        if(diceManager.centerPosition.x != diceManager.diceTray.position.x)
        {
            diceManager.Init();
        }

        diceManager.startDice();

        if (!PlayerTurn.isAI)
        {
            DicePanel.SetActive(true);
        }
        else
        {
            aiLogic.Init(PlayerTurn);
            StartCoroutine(AIDicePhase());
        }

    }

    public void ResolveDice(List<DiceFace> diceFaces)
    {
        foreach(DiceFace result in diceFaces)
        {
            results.Add(result);
        }
        DicePanel.SetActive(false);
        HpPanel.SetActive(true);
        PlayerTurn.additionalReroll = 0;
        PlayerTurn.additionalDice = 0;
        StartCoroutine(NextPhase());
        //ActionPhase();
    }

    //----------------//
    //DICE - AI HELPER//
    //----------------//

    public IEnumerator AIDicePhase()
    {
        yield return new WaitUntil(
            () => !diceManager.ResultReady
            );
        yield return new WaitUntil(
            () => diceManager.ResultReady
            );
        yield return new WaitForSeconds(0.5f);

        aiLogic.currentDice = diceManager.allDices;
        aiLogic.StartAction(AIState.DiceRoll);
    }

    public void AIReply(List<DiceEvaluate> locked, bool reroll)
    {
        //Enqueue(AILockDice(locked));
        //Enqueue(AIRerollOrResolve(locked.Count == PlayerTurn.additionalDice + 6));
        
        foreach(var dice in locked)
        {
            if (dice.locked) continue;

            diceManager.lockDice(dice);
        }

        if (reroll)
        {

            diceManager.Reroll();
            StartCoroutine(AIDicePhase());
        }
        else
        {
            diceManager.ResolveDice();
        }
        //*/
    }

    public IEnumerator AILockDice(List<DiceEvaluate> locked)
    {
        foreach(DiceEvaluate dice in locked)
        {
            diceManager.lockDice(dice);
            yield return new WaitForSeconds(0.5f);
        }
    }

    public IEnumerator AIRerollOrResolve(bool reroll)
    {
        if (reroll)
        {
            diceManager.Reroll();
            yield return new WaitForSeconds(1f);
            StartCoroutine(AIDicePhase());
        }
        else
        {
            yield return new WaitForSeconds(0.5f);
            diceManager.ResolveDice();
        }

    }

    //------------------//
    //DICE RESOLVE PHASE//
    //------------------//

    public void NextResolve(DiceFace next)
    {
        ResolveDiceEffect(next);
    }

    public int Count(DiceFace face)
    {
        int count = 0;
        foreach(DiceFace result in results)
        {
            if(result == face) count++;
        }
        return count;
    }

    private void ResolveDiceEffect(DiceFace face)
    {
        int count = Count(face);
        DiceFace next = DiceFace.Power;
        SetCameraPos(BirdsEyeView);

        switch (face)
        {

            case DiceFace.Power: next = DiceFace.Heal;
                if (count == 0) break;
                specialSkill(count);
                break;
            case DiceFace.Heal: next = DiceFace.Attack;
                if (count == 0) break;
                HpChange(PlayerTurn, count);
                break;
            case DiceFace.Attack: next = DiceFace.Charge;
                if (count == 0) break;
                HpChange(PlayerTurn.opponent, -count);
                break;
            case DiceFace.Charge: next = DiceFace.Fame;
                if (count == 0) break;
                ApChange(PlayerTurn, count);
                break;
            case DiceFace.Fame: next = DiceFace.Destruction;
                if (count == 0) break;
                TrackerMove(PlayerTurn, DiceFace.Fame, Mathf.Max(count - 2, 0));
                break;
            case DiceFace.Destruction: next = face;
                if (count == 0) break;
                TrackerMove(PlayerTurn, DiceFace.Destruction, Mathf.Max(count - 2, 0));
                break;
        }
        if(next == face)
        {
            StartCoroutine(WaitCoroutine());
            return;
        }
        Enqueue(ResolveNextEffect(next));
        //NextResolve(next);
    }

    private IEnumerator ResolveNextEffect(DiceFace face)
    {
        yield return null;
        ResolveDiceEffect(face);
    }

    private void specialSkill(int count)
    {
        if (count == 0) return;
        int cost = 0;

        switch (PlayerTurn.character.skill)
        {
            case SpecialSkill.SS001: cost = 3;
                break;
            case SpecialSkill.SS002: cost = 2;
                break;
            case SpecialSkill.SS003: cost = 2;
                break;
            case SpecialSkill.SS004: cost = 3;
                break;
            default:
                break;
        }
        Enqueue(executeSpecialSkill(count, PlayerTurn, cost));
    }

    private IEnumerator executeSpecialSkill(int power, PlayerData target, int cost)
    {
        yield return new WaitForSeconds(1.5f);
        SetCameraPos(target == Player ? FacingPlayer : FacingAI);
        SkillPointPopUp.SetActive(true);
        target.ui.ModelAnimator.PlaySpecialSkill(power >= cost);
        //AudioManager.instance.PlaySfx(target.character.soundEffects.Energize);
        SkillPointText.text = "0";

        int startPower = 0;
        int targetPower = power;

        float elapsed = 0f;
        float duration = target.ui.ModelAnimator.charge.length;

        //bool sfxPlayed = false;
        while (elapsed < duration)
        {
            /*
            if (elapsed >= target.character.soundEffects.timingForCharge && !sfxPlayed)
            {
                AudioManager.instance.PlaySfx(target.character.soundEffects.Energize);
                sfxPlayed = true;
            }
            //*/
            elapsed += Time.deltaTime;

            // Hitung persentase progress (0.0 sampai 1.0)
            float percentage = elapsed / duration;

            // Berpindah mulus dari startEnergy ke targetEnergy berdasarkan persentase waktu
            target.SkillPoints = Mathf.RoundToInt(Mathf.Lerp(startPower, targetPower, percentage));

            SkillPointText.text = target.SkillPoints.ToString();
            yield return null;
        }
        SkillPointPopUp.SetActive(false);

        if(power >= cost)
        {
            SetCameraPos(target == Player ? InFrontOfPlayer : InFrontOfAI);
            duration = target.ui.ModelAnimator.signal.length;
            SpecialSkillPanel.SetActive(true);
            Image specialSkillImage = SpecialSkillPanel.GetComponent<Image>();
            specialSkillImage.sprite = target.character.specialSkill;
            //yield return new WaitForSeconds(target.character.soundEffects.timingForSignal);
            //AudioManager.instance.PlaySfx(target.character.soundEffects.Signal);
            yield return new WaitForSeconds(duration);

            switch (PlayerTurn.character.skill)
            {
                case SpecialSkill.SS001:
                    TrackerMove(PlayerTurn, DiceFace.Fame, 1);
                    TrackerMove(PlayerTurn, DiceFace.Destruction, 1);
                    break;
                case SpecialSkill.SS002:
                    Charge(PlayerTurn, power - 1);
                    break;
                case SpecialSkill.SS003:
                    HpChange(PlayerTurn.opponent, -3);
                    break;
                case SpecialSkill.SS004:
                    PlayerTurn.additionalDice += 1;
                    break;
                default:
                    break;
            }
        }
        SetCameraPos(BirdsEyeView);
        SpecialSkillPanel.SetActive(false);
        target.ui.ModelAnimator.EndSpecialSkill();
    }

    public void Charge(PlayerData target, int power)
    {
        int energy = 0;
        foreach(DiceFace result in results)
        {
            if(result == DiceFace.Charge) energy++;
        }
        ApChange(target, energy*power);
    }

    public void HpChange(PlayerData target, int count, bool resolvingDice = false)
    {
        if (count == 0) return;
        Enqueue(HPBarChange(target, count));
        if (!resolvingDice)
        {
            TriggerCardEffect(target,
                count > 0 ? TriggerEvent.OnAdd : TriggerEvent.OnSubtract,
                PlayerState.HealthPoint,
                Mathf.Abs(count));
        }
    }

    private IEnumerator HPBarChange(PlayerData target, int deltaHealth)
    {
        int currentHealth = target.CurrentHP;
        target.CurrentHP = Mathf.Clamp(target.CurrentHP += deltaHealth, 0, target.character.hp);
        if (deltaHealth < 0)
        {
            if(slowDown)
                Time.timeScale = 0.5f;
            SetCameraPos(target == Player ? OverAIShoulder : OverPlayerShoulder);
            target.opponent.ui.ModelAnimator.PlayAttack();
            //yield return new WaitForSeconds(target.opponent.character.soundEffects.timingForAttack);
            //AudioManager.instance.PlaySfx(target.opponent.character.soundEffects.Attack);
            float duration = target.opponent.ui.ModelAnimator.attack.length - target.ui.ModelAnimator.timingForAttack;
            yield return new WaitForSeconds(duration);
                //target.opponent.character.soundEffects.timingForAttack
            target.ui.ModelAnimator.PlayHit();
            //AudioManager.instance.PlaySfx(target.character.soundEffects.Hurt);
            //yield return new WaitForSeconds(target.ui.ModelAnimator.timingForHit);
            float fillAmount = (float)target.CurrentHP / target.character.hp;
            target.ui.HPBar.fillAmount = fillAmount;
            yield return new WaitForSeconds(1.5f);
        }
        else if (deltaHealth > 0)
        {
            SetCameraPos(target == Player ? FacingPlayer : FacingAI);
            target.ui.ModelAnimator.PlayHeal();
            float elapsed = 0f;
            float duration = target.ui.ModelAnimator.heal.length;

            int startAmount = currentHealth;
            float fillAmount;
            //bool sfxPlayed = false;
            while (elapsed < duration)
            {
                /*
                if(elapsed >= target.character.soundEffects.timingForHeal && !sfxPlayed)
                {
                    AudioManager.instance.PlaySfx(target.character.soundEffects.Heal);
                    sfxPlayed = true;
                }
                //*/
                elapsed += Time.deltaTime;
                float percentage = elapsed / duration;

                currentHealth = Mathf.RoundToInt(Mathf.Lerp(startAmount, target.CurrentHP, percentage));

                fillAmount = (float)currentHealth / target.character.hp;
                target.ui.HPBar.fillAmount = fillAmount;
                yield return null;
            }
            fillAmount = (float)target.CurrentHP / target.character.hp;
            target.ui.HPBar.fillAmount = fillAmount;
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
        Debug.Log("Done");
        SetCameraPos(BirdsEyeView);
        yield return new WaitForSeconds(0.5f);

        CheckWinCondition();

    }

    public void ApChange(PlayerData target, int ApAmount, bool resolvingDice = false)
    {
        if (ApAmount == 0) return;
        Enqueue(charge(target, ApAmount));

        if (!resolvingDice || ApAmount < 0)
        {
            TriggerCardEffect(target,
                ApAmount > 0 ? TriggerEvent.OnAdd : TriggerEvent.OnSubtract,
                PlayerState.AbilityPoints,
                Mathf.Abs(ApAmount));
        }
    }

    public IEnumerator charge(PlayerData target, int ApAmount)
    {
        if (ApAmount > 0)
        {
            SetCameraPos(target == Player ? FacingPlayer : FacingAI);
            target.ui.ModelAnimator.PlayCharge();

            // 1. Hitung dulu berapa TOTAL energi yang mau ditambahkan
            int totalTambah = Mathf.RoundToInt(ApAmount * EnergyMultiplier);
            int currentEnergy = target.AbilityPoints;
            target.AbilityPoints += totalTambah;

            float elapsed = 0;
            float duration = target.ui.ModelAnimator.charge.length;

            // 2. Tentukan titik mulai dan titik akhir
            int startEnergy = currentEnergy;
            int targetEnergy = currentEnergy + totalTambah;

            //bool sfxPlayed = false;

            while (elapsed < duration)
            {
                /*
                if(elapsed >= target.character.soundEffects.timingForCharge && !sfxPlayed)
                {
                    AudioManager.instance.PlaySfx(target.character.soundEffects.Energize);
                    sfxPlayed = true;
                }
                //*/
                elapsed += Time.deltaTime;

                // Hitung persentase progress (0.0 sampai 1.0)
                float percentage = elapsed / duration;

                // Berpindah mulus dari startEnergy ke targetEnergy berdasarkan persentase waktu
                currentEnergy = Mathf.RoundToInt(Mathf.Lerp(startEnergy, targetEnergy, percentage));

                target.ui.APText.text = currentEnergy.ToString();
                yield return null;
            }

            // 3. Pastikan angka terakhirnya tepat di target setelah loop selesai
            currentEnergy = targetEnergy;
            target.ui.APText.text = currentEnergy.ToString();
        }
        else if (ApAmount < 0)
        {
            int totalTambah = Mathf.RoundToInt(ApAmount * EnergyMultiplier);
            int currentEnergy = target.AbilityPoints;
            target.AbilityPoints = Mathf.Max(target.AbilityPoints + totalTambah, 0);

            float elapsed = 0;
            float duration = 2f;
            
            int startEnergy = currentEnergy;
            int targetEnergy = target.AbilityPoints;

            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;

                // Hitung persentase progress (0.0 sampai 1.0)
                float percentage = elapsed / duration;

                // Berpindah mulus dari startEnergy ke targetEnergy berdasarkan persentase waktu
                currentEnergy = Mathf.RoundToInt(Mathf.Lerp(startEnergy, targetEnergy, percentage));

                target.ui.APText.text = currentEnergy.ToString();
                yield return null;
            }

            // 3. Pastikan angka terakhirnya tepat di target setelah loop selesai
            currentEnergy = targetEnergy;
            target.ui.APText.text = currentEnergy.ToString();
        }
        

        EnergyMultiplier = 1;

        SetCameraPos(BirdsEyeView);
        yield return new WaitForSeconds(0.5f);

    }

    public void TrackerMove(PlayerData target, DiceFace face, int delta, bool resolvingDice = false)
    {
        if (delta == 0) return;
        if (target == Player)
        {
            delta *= -1;
        }

        if(face == DiceFace.Fame)
        {
            FameIndex = Mathf.Clamp(FameIndex + delta, 0, 14);
        }
        else if(face == DiceFace.Destruction)
        {
            DestructionIndex = Mathf.Clamp(DestructionIndex + delta, 0, 14);
        }
        Enqueue(Move(target, face, delta, resolvingDice));

        PlayerState state = face == DiceFace.Fame ? PlayerState.Fame : PlayerState.Destruction;
        if (!resolvingDice)
        {
            TriggerCardEffect(target,
                TriggerEvent.OnAdd,
                state,
                Mathf.Abs(delta));
        }
    }

    public IEnumerator Move(PlayerData target, DiceFace face, int delta, bool resolvingDice)
    {
        PhaseAnnouncePanel.SetActive(false);
        HpPanel.SetActive(false);

        SetCameraPos(target == Player ? FacingPlayer : FacingAI);
        float waitDuration = 0f;
        if (face == DiceFace.Fame)
        {
            target.ui.ModelAnimator.PlayFame();
            AudioManager.instance.PlaySfx(fameSfx);
            waitDuration = target.ui.ModelAnimator.brag.length - target.ui.ModelAnimator.timingForLaugh;
            //yield return new WaitForSeconds(target.character.soundEffects.timingForFame);
            //AudioManager.instance.PlaySfx(target.character.soundEffects.Fame);
            //waitDuration -= target.character.soundEffects.timingForFame;
        }
        else if (face == DiceFace.Destruction)
        {
            target.ui.ModelAnimator.PlayDestruction();
            AudioManager.instance.PlaySfx(destructionSfx);
            waitDuration = target.ui.ModelAnimator.destruction.length - target.ui.ModelAnimator.timingForLaugh;
            //yield return new WaitForSeconds(target.character.soundEffects.timingForDestruction);
            //AudioManager.instance.PlaySfx(target.character.soundEffects.Destruction);
            //waitDuration -= target.character.soundEffects.timingForDestruction;
        }

        yield return new WaitForSeconds(waitDuration);

        SetCameraPos(target == Player ? BehindPlayer : BehindAI);
        tokenMovement.duration = target.ui.ModelAnimator.timingForLaugh;
        yield return tokenMovement.Move(face, delta);
        //yield return new WaitForSeconds(target.ui.ModelAnimator.timingForLaugh - 1);

        SetCameraPos(BirdsEyeView);

        PhaseAnnouncePanel.SetActive(true);
        HpPanel.SetActive(true);

        CheckWinCondition();
        
        int index = face == DiceFace.Fame ? FameIndex : DestructionIndex;
        switch(buzzTilePlacing.GetTileState(index - 1, face == DiceFace.Fame))
        {
            case PlayerState.HealthPoint:
                yield return HPBarChange(target, 1);
                break;
            case PlayerState.AbilityPoints:
                yield return charge(target, 1); 
                break;
            case PlayerState.Dice:
                target.additionalDice += 1;
                break;
        }
    }

    public void CheckWinCondition()
    {
        if (Player.CurrentHP == 0)
        {
            gameOver = true;
            winner = AI;
            win = winCondition.Kill;
        }
        else if (AI.CurrentHP == 0)
        {
            gameOver = true;
            winner = Player;
            win = winCondition.Kill;
        }
        else if (FameIndex == 0 || DestructionIndex == 0)
        {
            gameOver = true; 
            winner = Player;
            win = FameIndex == 0 ? winCondition.Fame : winCondition.Destruction;

        }
        else if (FameIndex == 14 || DestructionIndex == 14)
        {
            gameOver = true; 
            winner = AI;
            win = FameIndex == 14 ? winCondition.Fame : winCondition.Destruction;

        }
        else if (FameIndex <= 2 && DestructionIndex <= 2)
        {
            gameOver = true; 
            winner = Player;
            win = winCondition.Spotlight;

        }
        else if(FameIndex >= 12 && DestructionIndex >= 12)
        {
            gameOver = true; 
            winner = AI;
            win = winCondition.Spotlight;

        }
        if(gameOver)
        {
            Over();
        }
    }
    
    public void Over()
    {
        playerWin = winner == Player;
        HpPanel.SetActive(false);
        DicePanel.SetActive(false);
        CardDraftingPanel.SetActive(false);
        PlaceTilePanel.SetActive(false);
        BuyCardPanel.SetActive(false);
        SkillPointPopUp.SetActive(false);
        TurnAnnounce.gameObject.SetActive(false);
        WinPose(winner);
        StartCoroutine(GameOverAnimation());
    }

    public void WinPose(PlayerData winPlayer)
    {
        winPlayer.ui.MainModel.SetActive(false);
        winPlayer.ui.VictoryModel.SetActive(true);
    }

    public IEnumerator GameOverAnimation()
    {
        SetCameraPos(winner == Player ? AILoseShot : PlayerLoseShot);
        winner.opponent.ui.ModelAnimator.PlayLose();
        //AudioManager.instance.PlaySfx(winner.opponent.character.soundEffects.Lose);
        float duration = winner.opponent.ui.ModelAnimator.lose.length;
        yield return new WaitForSeconds(duration);

        GameOverPanel.SetActive(true);
        SetCameraPos(winner == Player ? AILost : PlayerLost);
        OpenGameOverPanel();
        AudioManager.instance.PlaySfx(playerWin ? PlayerWin : PlayerLose);
    }

    public void OpenGameOverPanel()
    {
        PlayTimeBox.sprite = playerWin ? PlayerPlayTime : EnemyPlayTime;
        int minutes = Mathf.FloorToInt(playTime / 60);
        int seconds = Mathf.FloorToInt(playTime % 60);
        int milliseconds = Mathf.FloorToInt((playTime * 100) % 100);
        
        string display =
            $"{minutes:00} : {seconds:00}.{milliseconds:00}";
        PlayTimeText.text = display;

        WinConBox.sprite = playerWin ? PlayerWinCon : EnemyWinCon;
        switch (win)
        {
            case winCondition.Kill:
                WinConSymbol.sprite = HPWincon;
                WinConText.text = playerWin ? "Defeated Enemy" : "Defeated by Enemy";
                break;
            case winCondition.Fame:
                WinConSymbol.sprite = FameWinCon;
                WinConText.text = playerWin ? "Ruling Shibuya" : "Enemy Ruling Shibuya";
                break;
            case winCondition.Destruction:
                WinConSymbol.sprite = DestrucWinCon;
                WinConText.text = playerWin ? "Destroying Shibuya" : "Enemy Destroying Shibuya";
                break;
            case winCondition.Spotlight:
                WinConSymbol.sprite = SpotLightWinCon;
                WinConText.text = playerWin ? "Entered Spotlight" : "Enemy Entered Spotlight";
                break;
            case winCondition.Surrender:
                WinConSymbol.sprite = HPWincon;
                WinConText.text = playerWin ? "Enemy Surrendering" : "Surrendering";
                break;
        }

        PlayerWinState.sprite = playerWin ? VictorySprite : DefeatSprite;
    }

    //-------------------//
    //CARD DRAFTING PHASE//
    //-------------------//


    private void CardDraftingPhase()
    {
        if (gameOver) return;
        cardDraftingSystem.usablePoints = PlayerTurn.AbilityPoints;

        if (!PlayerTurn.isAI)
        {
            selectCardPhase(true);
            PlayerCardButtonPanel.SetActive(true);
            PlayerBuyButtonPanel.SetActive(true);
        }
        else
        {
            aiLogic.StartAction(AIState.CardBuy);
            PlayerCardButtonPanel.SetActive(false);
            PlayerBuyButtonPanel.SetActive(false);
        }
    }

    public void buyCard(AbilityCard card)
    {
        Enqueue(buyCardAnimation(card));
        TriggerCardEffect(PlayerTurn, TriggerEvent.OnAdd, PlayerState.AbilityCard, 1);
    }

    public IEnumerator buyCardAnimation(AbilityCard card)
    {
        SetCameraPos(BirdsEyeView);
        buyCardPhase(false);
        yield return new WaitForSeconds(0.5f);

        HpPanel.SetActive(false);
        SetCameraPos(PlayerTurn == Player ? InFrontOfPlayer : InFrontOfAI);
        PlayerTurn.ui.ModelAnimator.PlayBuyCard();
        float duration = PlayerTurn.ui.ModelAnimator.signal.length;
        SpecialSkillPanel.SetActive(true);
        Image specialSkillImage = SpecialSkillPanel.GetComponent<Image>();
        specialSkillImage.sprite = card.cardSprite;
        yield return new WaitForSeconds(duration);


        SetCameraPos(BirdsEyeView);
        SpecialSkillPanel.SetActive(false);
        HpPanel.SetActive(true);
    }


    public void buyCardPhase(bool active)
    {
        if (active)
        {
            SetCameraPos(PlayerTurn == Player ? InFrontOfPlayer : InFrontOfAI);
        }
        BuyCardPanel.SetActive(active);

    }

    public void selectCardPhase(bool active)
    {
        CardDraftingPanel.SetActive(active);
    }

    public void UpdateAbilityPoints(int AP)
    {
        PlayerTurn.AbilityPoints = AP;
        PlayerTurn.ui.APText.text = AP.ToString();
        if (!PlayerTurn.isAI)
        {
            StartCoroutine(WaitCoroutine());
        }
    }

    public void SkipCardDrafting()
    {
        PlayerTurn.AbilityPoints += 1;
        PlayerTurn.ui.APText.text = PlayerTurn.AbilityPoints.ToString();
        CardDraftingPanel.SetActive(false);
        GamePhase++;
        ActionPhase();
    }

    //-------------------------//
    //CARD DRAFTING - AI HELPER//
    //-------------------------//

    public void OnAISelectedCard(AbilityCard card)
    {
        Enqueue(ShowAIAction(card));
        //cardDraftingSystem.AIBuyCard(card);
        //StartCoroutine(WaitCoroutine());
    }

    public IEnumerator ShowAIAction(AbilityCard card)
    {
        selectCardPhase(true);

        yield return new WaitForSeconds(1.5f);

        selectCardPhase(false);
        buyCardPhase(true);

        cardDraftingSystem.ChosenCard.sprite =
            card.cardSprite;

        SetCameraPos(
            PlayerTurn == Player ?
            InFrontOfPlayer :
            InFrontOfAI);

        yield return new WaitForSeconds(1.5f);

        // BARU BELI DI SINI
        cardDraftingSystem.AIBuyCard(card);

        yield return buyCardAnimation(card);

        StartCoroutine(WaitCoroutine());
    }


    public void AISkipCardDrafting()
    {
        Enqueue(ShowAISkipAction());
    }

    public IEnumerator ShowAISkipAction()
    {
        selectCardPhase(true);
        yield return new WaitForSeconds(1.5f);

        selectCardPhase(false);
        SkipCardDrafting();
    }

    //------------------//
    //CARD EFFECT HELPER//
    //------------------//

    public void ApplyEffect(PlayerData self, SubjectTarget target, PlayerState state, int value)
    {
        AudioManager.instance.PlaySfx(AbilityCardEffect);
        PlayerData targetPlayer = self;
        if(target == SubjectTarget.Opponent)
        {
            targetPlayer = self.opponent;
        }

        switch (state)
        {
            case PlayerState.HealthPoint: 
                HpChange(targetPlayer, value, true); 
                break;
            case PlayerState.AbilityPoints: 
                ApChange(targetPlayer, value, true);
                break;
            case PlayerState.Fame:
                TrackerMove(targetPlayer, DiceFace.Fame, value, true);
                break;
            case PlayerState.Destruction:
                TrackerMove(targetPlayer, DiceFace.Destruction, value, true);
                break;
            case PlayerState.Reroll:
                targetPlayer.additionalReroll += value;
                break;
            case PlayerState.Dice:
                targetPlayer.additionalDice += value;
                break;
            default: return;
        }
    }

    public int GetStateAmount(PlayerData self, SubjectTarget target, PlayerState state)
    {
        PlayerData targetPlayer;
        if(target == SubjectTarget.Self)
        {
            targetPlayer = self;
        }
        else
        {
            targetPlayer = self.opponent;
        }

        switch (state)
        {
            case PlayerState.HealthPoint: return targetPlayer.CurrentHP;
            case PlayerState.AbilityPoints: return targetPlayer.AbilityPoints;
            case PlayerState.AbilityCard: return targetPlayer.Cards.Count;
            default: return -1;
        }
    }

    public void GivePermaCard(AbilityCard card)
    {
        PlayerTurn.Cards.Add(card);
        PlayerTurn.ui.CardAmount.text = PlayerTurn.Cards.Count.ToString();
        /*
        TriggerCardEffect(PlayerTurn,
            TriggerEvent.OnAdd,
            PlayerState.AbilityCard,
            1);
        //*/
    }

    public void GiveBuzzTile(BuzzTile buzzTile)
    {
        PlayerTurn.Tile = buzzTile;
    }

    public void TriggerCardEffect(PlayerData target, TriggerEvent trig, PlayerState trigState, int value = 0)
    {
        Debug.Log("Trigger " + trigState.ToString());
        PlayerData opponent = target.opponent;

        foreach(AbilityCard permaCard in target.Cards)
        {
            if (permaCard.triggerEvent == trig &&
                permaCard.triggerState == trigState &&
                permaCard.triggerSubject == SubjectTarget.Self)
            {
                ResolveCard(target, permaCard, value);
            }
        }
        foreach(AbilityCard permaCard in opponent.Cards)
        {
            if (permaCard.triggerEvent == trig &&
                permaCard.triggerState == trigState &&
                permaCard.triggerSubject == SubjectTarget.Opponent)
            {
                ResolveCard(opponent, permaCard, value);
            }
        }
    }

    public void ResolveCard(PlayerData owner, AbilityCard card, int delta )
    {
        if(!card.useCondition ||
            evaluateCondition(card.conditionComparative, card.conditionValue, delta))
        {
            int value = card.effectValue;
            if (card.effectType == EffectType.Subtract) value *= -1;

            ApplyEffect(owner, card.effectTarget, card.effectState, value);
        }
    }

    public bool evaluateCondition(Comparative evaluation, int conditionValue, int value)
    {
        switch (evaluation)
        {
            case Comparative.More: return value > conditionValue;
            case Comparative.Less: return value < conditionValue;
            case Comparative.Equal: return value == conditionValue;
            case Comparative.AtLeast: return value >= conditionValue;
            case Comparative.NoMore: return value <= conditionValue;
            default: return false;
        }
    }

    //---------------//
    //BUZZ TILE PHASE//
    //---------------//

    private void BuzzTilePhase()
    {
        if (gameOver) return;
        if (PlayerTurn.Tile == null)
        {
            ActionPhase(); 
            return;
        }

        HpPanel.SetActive(false);
        PlaceTilePanel.SetActive(true);
        buzzTilePlacing.OpenPanel(FameIndex, DestructionIndex, PlayerTurn.Tile);

        if (PlayerTurn.isAI)
        {
            AIBuzzTilePhase();
        }
    }

    public void EndBuzzTilePlacing()
    {
        PlaceTilePanel.SetActive(false);
        ActionPhase();
    }

    //---------------------//
    //BUZZ TILE - AI HELPER//
    //---------------------//

    public void AIBuzzTilePhase()
    {
        aiLogic.options = buzzTilePlacing.options;
        aiLogic.StartAction(AIState.BuzzTile);
    }

    public void OnAIBuzzTileDecide(TilePlacementData selected, BuzzTile tile)
    {
        buzzTilePlacing.AIPlaceTile(selected.isFame, selected.index, tile, selected.kiri);
        Enqueue(ShowAIBuzzTile());
    }

    public IEnumerator ShowAIBuzzTile()
    {
        yield return new WaitForSeconds(1.5f);

        buzzTilePlacing.SetSpritePreview();

        yield return new WaitForSeconds(1.5f);

        buzzTilePlacing.confirm();
    }

    //-------------//
    //MISCELLANEOUS//
    //-------------//

    public void SetCameraPos(Transform pivot)
    {
        Kamera.position = pivot.position;
        Kamera.rotation = pivot.rotation;
    }

    private void LoadSessionCharacter(PlayerData player)
    {
        player.ui.Profile.sprite = player.character.icon;
        player.CurrentHP = player.character.hp; if (player.ui.ModelPos.childCount > 0)
        {
            Destroy(player.ui.ModelPos.GetChild(0).gameObject);
        }
        GameObject model = Instantiate(player.character.characterModel, player.ui.ModelPos);
        player.ui.MainModel = model;
        GameObject VModel = Instantiate(player.character.victoryModel, player.ui.ModelPos);
        player.ui.VictoryModel = VModel;
        VModel.SetActive(false);
        player.ui.ModelAnimator = model.GetComponent<AnimationScript>();
        if(player == AI && AI.character == Player.character)
        {
            shiftModelHue(player);
        }
    }

    private void shiftModelHue(PlayerData player)
    {
        Renderer mainRend = player.ui.MainModel.GetComponentInChildren<Renderer>();
        Renderer victoryRend = player.ui.VictoryModel.GetComponentInChildren<Renderer>();
        Material[] mainMats = mainRend.materials;
        Material[] vMats = victoryRend.materials;
        if (player.character.alternativeColors.Length == 0) return;

        Color[] colors = (Color[])player.character.alternativeColors.Clone();
        for(int i = 0; i < colors.Length; i++)
        {
            mainMats[i].SetColor("_BaseColor", colors[i]);
            //mainMats[i].color = colors[i];
            vMats[i].SetColor("_BaseColor", colors[i]);
        }

        /*
        List<Material> materials = new();
        Renderer mainRend = player.ui.MainModel.GetComponentInChildren<Renderer>();
        Renderer victoryRend = player.ui.VictoryModel.GetComponentInChildren<Renderer>();

        foreach(Material mat in mainRend.materials)
        {
            float h, s, v;
            Color color = mat.GetColor("_BaseColor");
            Color.RGBToHSV(mat.GetColor("_BaseColor"), out h, out s, out v);

            if (s < 0.01f) continue;

            h = (h + 0.5f) % 1f;
            color = Color.HSVToRGB(h, s, v);

            mat.SetColor("_BaseColor", Color.HSVToRGB(h, s, v));
        }

        //*/
    }

    private void ChangeTurn()
    {
        AudioManager.instance.PlaySfx(ChangeTurnSfx);
        GamePhase = 0;
        PlayerTurn.SkillPoints = 0;
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
            if (gameOver)
            {
                routineQueue.Clear();
                break;
            }
            yield return StartCoroutine(routineQueue.Dequeue());
        }
        isRunningCoroutine = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (!gameOver)
        {
            playTime += Time.deltaTime;
        }
        // Deteksi Klik Mouse untuk Lock / Unlock Dadu
        /*
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                CheckClick(touch.position);
            }
        }
        else if (Input.GetMouseButtonDown(0))
        {
            CheckClick(Input.mousePosition);
        }
        */
    }

    private void CheckClick(Vector2 position)
    {

        Ray ray = Camera.main.ScreenPointToRay(position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // object kena
            checkHit(hit);
        }
    }

    private void checkHit(RaycastHit hit)
    {
        if (PlayerTurn.isAI) return;
        if (hit.collider.gameObject.CompareTag("Cards"))
        {
            //StartCardDraft();
        }
        else if (hit.transform.root.CompareTag("Tug of War"))
        {
            //StartBuzzTilePlacing();
        }
    }

    //--------------------------//
    //Camera Movement in Opening//
    //--------------------------//

    private IEnumerator MoveCameraSequence()
    {
        PhaseAnnouncePanel.SetActive(false);
        // Fase 1: Orbit mengelilingi Titik A
        float timer = 0f;
        float currentAngle = 0f;

        while (timer < orbitDuration)
        {
            timer += Time.deltaTime;
            currentAngle += orbitSpeed * Time.deltaTime;

            // Menghitung posisi baru secara fisik di koordinat X dan Z
            Quaternion rotation = Quaternion.Euler(0f, currentAngle, 0f);
            Vector3 offset = rotation * new Vector3(0f, 45f, -orbitRadius);

            // Kamera benar-benar pindah posisi
            Kamera.position = transform.position + offset;

            // Kamera selalu menghadap ke Titik A
            Kamera.LookAt(transform.position);

            yield return null;
        }

        // Fase 2: Bergerak dari posisi terakhir ke Titik B
        Vector3 startTransitionPos = Kamera.position;
        Quaternion startTransitionRot = Kamera.rotation;
        timer = 0f;

        while (timer < transitionDuration)
        {
            timer += Time.deltaTime;
            float progress = timer / transitionDuration;

            // Menggunakan SmoothStep agar gerakan transisi lebih mulus
            float t = Mathf.SmoothStep(0f, 1f, progress);

            // Interpolasi posisi dan rotasi menuju Titik B
            Kamera.position = Vector3.Lerp(startTransitionPos, AboveDiceTray.position, t);
            Kamera.rotation = Quaternion.Slerp(startTransitionRot, AboveDiceTray.rotation, t);

            yield return null;
        }
        PhaseAnnouncePanel.SetActive(true);
        ActionPhase();
    }

    public void PauseGame()
    {
        if (gameOver) return;
        PausePanel.SetActive(true);
        Time.timeScale = 0f;
    }

    public void ResumeGame()
    {
        PausePanel.SetActive(false);
        Time.timeScale = 1f;
    }

    public void About2Exit(bool active)
    {
        ExitConfirmationPanel.SetActive(active);
    }

    public void ExitFight()
    {
        Time.timeScale = 1f;
        AudioManager.instance.PlayMainMenuMusic();
        SceneManager.LoadScene("MainMenu");
    }

    public void PlayAgain()
    {
        Time.timeScale = 1f;
        AudioManager.instance.PlayMainMenuMusic();
        SceneManager.LoadScene("CharacterSelection");
    }

    public void Surrender()
    {
        gameOver = true;
        winner = AI;
        StopAllCoroutines();
        routineQueue.Clear();
        isRunningCoroutine = false;
        StopAnimation();
        ResumeGame();
        win = winCondition.Surrender;
        Over();
    }

    public void StopAnimation()
    {
        Player.ui.ModelAnimator.animator.Rebind();
        AI.ui.ModelAnimator.animator.Rebind();
    }

    public void EndFight(GameObject panel)
    {
        if (simulasi)
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else
        {
            panel.SetActive(true);
        }
    }
}


