using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
    public BuzzTile Tile = null;
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
    public GameObject PhaseAnnouncePanel;
    public GameObject SpecialSkillPanel;

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
    private bool isInActivePhase = false;
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
            ChangeTurn();
        }
        switch (GamePhase)
        {
            case 0: StartDicePhase(); 
                break;
            case 1: ResolveDiceEffect(0); 
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
        //Enqueue(NextPhase());
        ActionPhase();
    }

    //----------------//
    //DICE - AI HELPER//
    //----------------//

    public IEnumerator AIDicePhase()
    {
        yield return new WaitForSeconds(1f);
        yield return new WaitUntil(
            () => !diceManager.IsMoving()
            );
        yield return new WaitForSeconds(0.5f);

        aiLogic.currentDice = diceManager.allDices;
        aiLogic.StartAction(AIState.DiceRoll);
    }

    public void AIReply(List<DiceEvaluate> locked, bool reroll)
    {
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
    }

    public IEnumerator GoToCardDrafting()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(
            () => !isRunningCoroutine);

        StartCardDraft();
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
        if (index == 0) isInActivePhase = true;
        int power = 0;
        int heal = 0;
        int attack = 0;
        int energy = 0;
        int fame = 0;
        int destruction = 0;
        foreach(DiceFace result in results)
        {
            if (result == DiceFace.Power) power++;
            else if (result == DiceFace.Heal) heal++;
            else if (result == DiceFace.Attack) attack++;
            else if (result == DiceFace.Charge) energy++;
            else if (result == DiceFace.Fame) fame++;
            else if (result == DiceFace.Destruction) destruction++;
        }
        SetCameraPos(BirdsEyeView);
        switch (index)
        {
            case 0: 
                specialSkill(power);
                break;
            case 1: 
                HpChange(PlayerTurn, heal);
                break;
            case 2: 
                HpChange(PlayerTurn.opponent, -attack);
                break;
            case 3: 
                ApChange(PlayerTurn, energy);
                break;
            case 4:
                TrackerMove(PlayerTurn, DiceFace.Fame, Mathf.Max(fame - 2, 0));
                break;
            case 5:
                TrackerMove(PlayerTurn, DiceFace.Destruction, Mathf.Max(destruction - 2, 0));
                break;
            default: //ActionPhase();
                if (PlayerTurn.isAI)
                {
                    StartCoroutine(GoToCardDrafting());
                }
                isInActivePhase = false;
                return;
        }
        Enqueue(ResolveNextEffect(index + 1));
    }

    private IEnumerator ResolveNextEffect(int index)
    {
        yield return new WaitForSeconds(1f);
        ResolveDiceEffect(index);
    }

    private void specialSkill(int count)
    {

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
        target.ui.ModelAnimator.PlaySpecialSkill(power >= cost);

        int startPower = 0;
        int targetPower = power;

        float elapsed = 0f;
        float duration = target.ui.ModelAnimator.charge.length;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;

            // Hitung persentase progress (0.0 sampai 1.0)
            float percentage = elapsed / duration;

            // Berpindah mulus dari startEnergy ke targetEnergy berdasarkan persentase waktu
            target.SkillPoints = Mathf.RoundToInt(Mathf.Lerp(startPower, targetPower, percentage));

            target.ui.SPText.text = target.SkillPoints.ToString();
            yield return null;
        }

        if(power >= cost)
        {
            SetCameraPos(target == Player ? InFrontOfPlayer : InFrontOfAI);
            duration = target.ui.ModelAnimator.signal.length;
            SpecialSkillPanel.SetActive(true);
            Image specialSkillImage = SpecialSkillPanel.GetComponent<Image>();
            specialSkillImage.sprite = target.character.specialSkill;
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

    public void HpChange(PlayerData target, int count)
    {
        if (count == 0) return;
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

    public void ApChange(PlayerData target, int ApAmount)
    {
        if (ApAmount == 0) return;
        Enqueue(charge(target, ApAmount));
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

    public void TrackerMove(PlayerData target, DiceFace face, int delta)
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
        Enqueue(Move(target, face, delta));
    }

    public IEnumerator Move(PlayerData target, DiceFace face, int delta)
    {
        PhaseAnnouncePanel.SetActive(false);

        SetCameraPos(target == Player ? FacingPlayer : FacingAI);
        target.ui.ModelAnimator.PlayDestruction();

        float waitDuration = target.ui.ModelAnimator.destruction.length - target.ui.ModelAnimator.timingForDestruction;
        yield return new WaitForSeconds(waitDuration);

        SetCameraPos(target == Player ? BehindPlayer : BehindAI);
        yield return tokenMovement.Move(face, delta);

        SetCameraPos(BirdsEyeView);

        PhaseAnnouncePanel.SetActive(true);
    }

    //-------------------//
    //CARD DRAFTING PHASE//
    //-------------------//

    public void StartCardDraft()
    {
        if (isRunningCoroutine || GamePhase != 1 ||isInActivePhase) return;

        ActionPhase();
    }

    private void CardDraftingPhase()
    {
        isInActivePhase = true;
        cardDraftingSystem.usablePoints = PlayerTurn.AbilityPoints;

        if (!PlayerTurn.isAI)
        {
            selectCardPhase(true);
        }
        else
        {
            aiLogic.StartAction(AIState.CardBuy);
        }
    }

    public IEnumerator Next()
    {
        yield return null;
        StartBuzzTilePlacing();

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
        isInActivePhase = false;
    }

    public void SkipCardDrafting()
    {
        CardDraftingPanel.SetActive(false);
        GamePhase++;
        ActionPhase();
    }

    //-------------------------//
    //CARD DRAFTING - AI HELPER//
    //-------------------------//

    public void OnAISelectedCard(AbilityCard card)
    {
        cardDraftingSystem.AIBuyCard(card);
        StartCoroutine(GoToBuzzTilePlacing());
    }

    public IEnumerator GoToBuzzTilePlacing()
    {
        yield return new WaitForSeconds(0.5f);
        yield return new WaitUntil(
            () => !isRunningCoroutine
            );
        StartBuzzTilePlacing();
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
            targetPlayer = self.opponent;
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
                TrackerMove(targetPlayer, DiceFace.Fame, value);
                break;
            case PlayerState.Destruction:
                TrackerMove(targetPlayer, DiceFace.Destruction, value);
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
    }

    //---------------//
    //BUZZ TILE PHASE//
    //---------------//

    public void StartBuzzTilePlacing()
    {
        if (isRunningCoroutine || GamePhase != 2 || 
            isInActivePhase) return;
        ActionPhase();
    }

    private void BuzzTilePhase()
    {
        if (PlayerTurn.Tile == null)
        {
            ActionPhase(); 
            return;
        }

        PlaceTilePanel.SetActive(true);
        buzzTilePlacing.OpenPanel(FameIndex, DestructionIndex, PlayerTurn.Tile);
    }

    public void EndBuzzTilePlacing()
    {
        PlaceTilePanel.SetActive(false);
        ActionPhase();
    }

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
        player.ui.ModelAnimator = model.GetComponent<AnimationScript>();
    }

    private void ChangeTurn()
    {
        GamePhase = 0;
        PlayerTurn.SkillPoints = 0;
        PlayerTurn.ui.SPText.text = "0";
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
        if (PlayerTurn.isAI) return;
        if (hit.collider.gameObject.CompareTag("Cards"))
        {
            StartCardDraft();
        }
        else if (hit.transform.root.CompareTag("Tug of War"))
        {
            StartBuzzTilePlacing();
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
}


