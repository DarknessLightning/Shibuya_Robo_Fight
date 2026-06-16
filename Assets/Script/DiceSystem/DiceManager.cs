using System.Collections.Generic;
using UnityEngine;

public enum DiceFace
{
    Attack,       // Index 0 (Hasil dadu 5)
    Heal,         // Index 1 (Hasil dadu 2)
    Charge,       // Index 2 (Hasil dadu 3)
    Destruction,  // Index 3 (Hasil dadu 4)
    Fame,         // Index 4 (Hasil dadu 5)
    Power         // Index 5 (Hasil dadu 6)
}

public class DiceManager : MonoBehaviour
{
    public GameObject dicePrefab;
    public List<DiceEvaluate> allDices = new();
    private List<DiceEvaluate> lockedDices = new();
    public int diceAmount = 6;
    public Transform[] lockedPlaces;
    public GameObject rerollButton;
    
    public List<Vector3> standardRotations = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(90,0,0),
        new Vector3(0,-90,90),
        new Vector3(0,-90,-90),
        new Vector3(-90,0,0),
        new Vector3(180,0,0)
    };

    public List<DiceFace> diceFaces = new List<DiceFace>
    {
        DiceFace.Charge, 
        DiceFace.Heal,
        DiceFace.Attack,
        DiceFace.Power,
        DiceFace.Fame,
        DiceFace.Destruction
    };

    public List<DiceFace> result = new();


    // Variabel baru untuk jangkauan acak posisi jatuh
    public Vector3 centerPosition = new Vector3(15f, 5f, 0f); // Titik tengah di udara (Y=5)
    public float dropRadius = 1.5f; // Seberapa luas jangkauan acak area jatuh
    public Transform diceTray;
    private int reroll = 3;

    [Header("Testing Purpose")]
    public bool forTest = false;
    public DiceFace[] targetFace;
    public int[] numberOfDiceForFace;

    public AudioClip DiceRollSfx;
    public AudioClip DiceLockSfx;
    void Start()
    {
        centerPosition = new Vector3(diceTray.position.x, 5f, diceTray.position.z);
        // Inisialisasi jika diperlukan
    }

    public void Init()
    {
        centerPosition = new Vector3(diceTray.position.x, 5f, diceTray.position.z);
    }

    void Update()
    {
        if (FightManager.instance != null && FightManager.instance.PlayerTurn.isAI) return; 
        // Jika dadu masih menggelinding/bergerak, hentikan pembacaan input
        if (IsMoving()) return;


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

    void CheckClick(Vector2 screenPos)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // object kena
            checkHit(hit);
        }
    }

    public void startDice()
    {
        reroll = 3 + FightManager.instance.PlayerTurn.additionalReroll;
        if(allDices.Count == 0) 
        {

            for (int i = allDices.Count; i < Mathf.Min(diceAmount, lockedPlaces.Length); i++)
            {

                // 1. Buat posisi acak di udara agar tidak dilahirkan di titik (0,0,0) yang sama
                Vector2 randomOffset = Random.insideUnitCircle * dropRadius;
                Vector3 spawnPos = new Vector3(
                    centerPosition.x + randomOffset.x,
                    centerPosition.y + (i * 0.1f), // Beri sedikit jeda tinggi (Y) antar dadu agar tidak jepit-jepitan
                    centerPosition.z + randomOffset.y
                );

                // 2. Buat rotasi acak total sejak awal lahir
                Quaternion randomRot = Quaternion.Euler(Vector3.zero);

                GameObject dice = Instantiate(dicePrefab, spawnPos, randomRot);
                DiceEvaluate newDice = dice.GetComponent<DiceEvaluate>();
                allDices.Add(newDice);
                newDice.centerPosition = centerPosition;
                newDice.Init();
            }
        }
        else
        {
            foreach(DiceEvaluate dice in lockedDices)
            {
                dice.locked = false;
                dice.rb.useGravity = true;
                dice.rb.isKinematic = false;
            }
            lockedDices.Clear();
        }
        RollAllDice();
    }

    public void Reroll()
    {
        if (IsMoving()) return;

        AudioManager.instance.PlaySfx(DiceRollSfx);
        RollAllDice();
    }

    public void RollAllDice()
    {
        if (reroll <= 0) return;
        foreach (DiceEvaluate dice in allDices)
        {
            if (dice.locked) continue;

            // Jika dadu berada di bawah (sudah di meja), kembalikan statusnya ke atas sebelum di-roll
            if (dice.transform.position.y < 3f)
            {
                dice.back2Starting(); // Sekarang fungsi ini otomatis mengacak posisi di centerPosition langit
                dice.resetEnd();
            }

            // Jalankan fungsi lempar/jatuhkan dadu
            dice.RollDice();
        }
        reroll--;
        if(reroll <= 0)
        {
            rerollButton.SetActive(false);
        }
    }

    public void ResolveDice()
    {
        if (IsMoving())
        {
            return;
        }
        result.Clear();
        for (int i = 0; i < allDices.Count; i++)
        {
            int faceResult = allDices[i].GetTopNumber(); // Mengembalikan angka 1 - 6

            // PERBAIKAN 1: Kurangi 1 angka agar sinkron dengan urutan Enum (0 - 5)
            int enumIndex = faceResult - 1;

            result.Add(diceFaces[enumIndex]);
        }
        rerollButton.SetActive(true);
        if(FightManager.instance != null)
        {
            FightManager.instance.ResolveDice(result);
        }
    }

    public void checkHit(RaycastHit hit)
    {
        if (FightManager.instance.PlayerTurn.isAI) return;
        // Pastikan tidak bisa nge-lock dadu yang posisinya masih di langit/sedang jatuh
        if (hit.collider.transform.position.y > 3f) return;

        // Mengambil script dari parent collider (pastikan collider ada di objek anak atau sesuaikan)
        DiceEvaluate dice = hit.collider.GetComponentInParent<DiceEvaluate>();
        if (dice == null) return;

        lockDice(dice);
    }

    public void lockDice(DiceEvaluate dice)
    {
        dice.locked = !dice.locked;

        AudioManager.instance.PlaySfx(DiceLockSfx);
        if (dice.locked)
        {
            dice.setEnd(); // Simpan posisi terakhir di meja sebelum dipindah ke slot kunci

            if (!lockedDices.Contains(dice))
                lockedDices.Add(dice);

            int currentIndex = lockedDices.IndexOf(dice);
            if (currentIndex < lockedPlaces.Length)
            {
                // PERBAIKAN 2: Matikan physics agar dadu diam rapi di slot penyimpanan dan tidak jatuh/membal
                dice.rb.useGravity = false;
                dice.rb.linearVelocity = Vector3.zero;
                dice.rb.angularVelocity = Vector3.zero;
                dice.rb.isKinematic = true;

                // Ambil rotasi standar berdasarkan angka teratas (faceResult - 1)
                int rotationIndex = Mathf.Clamp(dice.GetTopNumber() - 1, 0, standardRotations.Count - 1);
                SetDiceForTesting(ref rotationIndex, lockedDices.IndexOf(dice));
                dice.setPosAndRot(
                    lockedPlaces[currentIndex].position,
                    standardRotations[rotationIndex]
                );
            }
        }
        else
        {
            // PERBAIKAN 3: Aktifkan kembali fisika saat dikembalikan dari slot penyimpanan ke meja
            dice.rb.isKinematic = false;
            dice.rb.useGravity = true;

            dice.back2End(); // Kembalikan ke posisi koordinat meja sebelum di-lock tadi

            int starting = lockedDices.IndexOf(dice);
            lockedDices.Remove(dice);
            moveDicesToPreviousIndexPlace(starting);
        }
    }

    public void SetDiceForTesting(ref int rotationIndex, int lockedDiceIndex)
    {
        if (!forTest) return;

        int startIndex = 0;
        for(int i = 0; i < numberOfDiceForFace.Length; i++)
        {
            if(lockedDiceIndex < startIndex + numberOfDiceForFace[i])
            {
                rotationIndex = diceFaces.IndexOf(targetFace[i]);
                return;
            }
            startIndex += numberOfDiceForFace[i];
        }
        if(targetFace.Length > numberOfDiceForFace.Length)
        {
            rotationIndex = diceFaces.IndexOf(targetFace[numberOfDiceForFace.Length]);
        }
    }

    public void moveDicesToPreviousIndexPlace(int starting)
    {
        // Merapikan barisan dadu yang tersisa di slot penyimpanan agar bergeser maju jika ada yang dilepas
        for (int i = starting; i < lockedDices.Count; i++)
        {
            if (i < lockedPlaces.Length)
            {
                lockedDices[i].transform.position = lockedPlaces[i].position;
            }
        }
    }

    public bool IsMoving()
    {
        // Cek apakah masih ada satu saja dadu yang menggelinding di arena pertarungan
        for (int i = 0; i < allDices.Count; i++)
        {
            if (allDices[i].IsMoving()) return true;
        }
        return false;
    }

    public void addReroll(int n = 0)
    {
        reroll += n;
    }
}