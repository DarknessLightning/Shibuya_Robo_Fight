using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.GraphicsBuffer;

[System.Serializable]
public class PlayerUI
{
    public Image Profile;
    public Transform ModelPos;
    public AnimationScript ModelAnimator;
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
    public Transform InFrontOfPlayer;
    public Transform InFrontOfAI;

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

    [Header("Other References")]
    public Transform Kamera;
    public Text PhaseAnnounce;
    public GameSessionData sessionData;

    [Header("Player Data")]
    public PlayerData Player;
    public PlayerData AI;
    public PlayerData PlayerTurn = null;

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
        Player.CurrentHP -= 7;
        float fillAmount = (float)Player.CurrentHP/Player.character.hp; 
        Player.ui.HPBar.fillAmount = fillAmount;
        LoadSessionCharacter(AI);

        //ActionPhase();
        PlayerTurn = Player;
        Player.opponent = AI;
        AI.opponent = Player;
        StartCoroutine(MoveCameraSequence());
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
        yield return new WaitForSeconds(0.5f);
        ActionPhase();
    }

    //----------//
    //DICE PHASE//
    //----------//

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

    //------------------//
    //DICE RESOLVE PHASE//
    //------------------//

    public void NextResolve(int index)
    {
        ResolveDiceEffect(index);
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
            case 4: TugOfWarStart();
                break;
            default: //ActionPhase();
                return;
        }
        Enqueue(ResolveNextEffect(index + 1));
    }

    private IEnumerator ResolveNextEffect(int index)
    {
        yield return new WaitForSeconds(1f);
        ResolveDiceEffect(index);
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

        int cost = 0;

        switch (PlayerTurn.character.skill)
        {
            case SpecialSkill.SS001: cost = 3;
                break;
            case SpecialSkill.SS002: cost = 1;
                break;
            case SpecialSkill.SS003: cost = 2;
                break;
            case SpecialSkill.SS004: cost = 3;
                break;
            default:
                break;
        }
        if (count >= cost) Enqueue(executeSpecialSkill(count, PlayerTurn));
    }

    private IEnumerator executeSpecialSkill(int power, PlayerData target)
    {
        yield return new WaitForSeconds(1.5f);
        SetCameraPos(target == Player ? FacingPlayer : FacingAI);
        target.ui.ModelAnimator.PlaySpecialSkill();

        float duration = target.ui.ModelAnimator.charge.length;
        yield return new WaitForSeconds(duration);
        SetCameraPos(target == Player ? InFrontOfPlayer : InFrontOfAI);
        duration = target.ui.ModelAnimator.signal.length;
        yield return new WaitForSeconds(duration);

        switch(PlayerTurn.character.skill)
        {
            case SpecialSkill.SS001:
                MoveTracker(1, 1, PlayerTurn);
                break;
            case SpecialSkill.SS002:
                EnergyMultiplier = power;
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
        target.ui.ModelAnimator.EndSpecialSkill();
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
        int currentHealth = target.CurrentHP;
        target.CurrentHP = Mathf.Clamp(target.CurrentHP += deltaHealth, 0, target.character.hp);
        if (deltaHealth < 0)
        {
            SetCameraPos(target == Player ? OverAIShoulder : OverPlayerShoulder);
            target.opponent.ui.ModelAnimator.PlayAttack();
            float duration = target.opponent.ui.ModelAnimator.attack.length - target.ui.ModelAnimator.timingForAttack;
            yield return new WaitForSeconds(duration);
            target.ui.ModelAnimator.PlayHit();
            //yield return new WaitForSeconds(target.ui.ModelAnimator.timingForHit);
            float fillAmount = (float)target.CurrentHP / target.character.hp;
            target.ui.HPBar.fillAmount = fillAmount;
            yield return new WaitForSeconds(PlayerTurn.ui.ModelAnimator.timingForAttack);
        }
        else if (deltaHealth > 0)
        {
            SetCameraPos(target == Player ? FacingPlayer : FacingAI);
            target.ui.ModelAnimator.PlayHeal();
            float elapsed = 0f;
            float duration = target.ui.ModelAnimator.heal.length;

            int startAmount = currentHealth;
            float fillAmount;
            while (elapsed < duration)
            {
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
        if(deltaHealth != 0)
        {
            yield return new WaitForSeconds(2f);
        }

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
        if (ApAmount != 0)
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
        /*
        while (currentEnergy != target.AbilityPoints)
        {
            currentEnergy = currentEnergy < target.AbilityPoints ? currentEnergy + 1 : currentEnergy - 1;
        }*/

        SetCameraPos(BirdsEyeView);
        if (ApAmount != 0)
        {
            yield return new WaitForSeconds(2f);
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

    //-------------------//
    //CARD DRAFTING PHASE//
    //-------------------//

    public void StartCardDraft()
    {
        if (isRunningCoroutine || GamePhase != 1) return;

        ActionPhase();
    }

    private void CardDraftingPhase()
    {
        cardDraftingSystem.usablePoints = PlayerTurn.AbilityPoints;
        CardDraftingPanel.SetActive(true);
    }

    public void SkipCardDrafting()
    {
        CardDraftingPanel.SetActive(false);
        GamePhase++;
        ActionPhase();
    }

    //------------------//
    //CARD EFFECT HELPER//
    //------------------//

    public void ApplyEffect(PlayerData self, SubjectTarget target, PlayerState state, int value)
    {
        PlayerData targetPlayer;
        if(target == SubjectTarget.Self)
        {
            targetPlayer = self;
        }
        else
        {
            targetPlayer = self == Player ? AI : Player;
        }

        switch (state)
        {
            case PlayerState.HealthPoint: 
                HpChange(targetPlayer, value); 
                break;
            case PlayerState.AbilityPoints: 
                ApChange(targetPlayer, value);
                break;
            case PlayerState.Fame:
                MoveTracker(value, 0, targetPlayer);
                break;
            case PlayerState.Destruction:
                MoveTracker(0, value, targetPlayer);
                break;
            case PlayerState.Reroll:
                targetPlayer.additionalReroll += value;
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
            targetPlayer = self == Player ? AI : Player;
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
    }

    //----------//
    //DICE PHASE//
    //----------//

    private void BuzzTilePhase()
    {

    }

    public void SetCameraPos(Transform pivot)
    {
        Kamera.position = pivot.position;
        Kamera.rotation = pivot.rotation;
    }

    private void LoadSessionCharacter(PlayerData player)
    {
        player.ui.Profile.sprite = player.character.icon;
        player.CurrentHP = player.character.hp;
        Destroy(player.ui.ModelPos.GetChild(0).gameObject);
        GameObject model = Instantiate(player.character.characterModel, player.ui.ModelPos);
        player.ui.ModelAnimator = model.GetComponent<AnimationScript>();
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

        // Deteksi Klik Mouse untuk Lock / Unlock Dadu
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
        if (hit.collider.gameObject.CompareTag("Cards"))
        {
            StartCardDraft();
        }
    }

    //--------------------------//
    //Camera Movement in Opening//
    //--------------------------//

    private IEnumerator MoveCameraSequence()
    {
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
        ActionPhase();
    }
}


