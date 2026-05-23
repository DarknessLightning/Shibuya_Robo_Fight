using System.Collections.Generic;
using UnityEngine;

public enum DiceFace
{
    Attack,       // Index 0 (Hasil dadu 1)
    Heal,         // Index 1 (Hasil dadu 2)
    Charge,       // Index 2 (Hasil dadu 3)
    Destruction,  // Index 3 (Hasil dadu 4)
    Fame,         // Index 4 (Hasil dadu 5)
    Power         // Index 5 (Hasil dadu 6)
}

public class DiceManager : MonoBehaviour
{
    public DiceEvaluate[] dices;
    private List<DiceEvaluate> lockedDices = new();
    public Transform[] lockedPlaces;
    public Vector3[] standardRotation; // Pastikan ukurannya ada 6 di Inspector

    public List<DiceFace> result = new();

    void Start()
    {
        // Inisialisasi jika diperlukan
    }

    void Update()
    {
        // Jika dadu masih menggelinding/bergerak, hentikan pembacaan input
        if (IsMoving()) return;

        // Tekan X untuk evaluasi hasil akhir dadu
        if (Input.GetKeyDown(KeyCode.X))
        {
            result.Clear();
            string msg = "";
            for (int i = 0; i < dices.Length; i++)
            {
                int faceResult = dices[i].GetTopNumber(); // Mengembalikan angka 1 - 6

                // PERBAIKAN 1: Kurangi 1 angka agar sinkron dengan urutan Enum (0 - 5)
                int enumIndex = faceResult - 1;

                msg += "Dice " + (i + 1) + " Result: " + (DiceFace)enumIndex + " (" + faceResult + ")\n";
                result.Add((DiceFace)enumIndex);
            }
            Debug.Log(msg);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            RollAllDice();
        }

        // Deteksi Klik Mouse untuk Lock / Unlock Dadu
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                lockDice(hit);
            }
        }
    }

    public void RollAllDice()
    {
        foreach (DiceEvaluate dice in dices)
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
    }

    public void lockDice(RaycastHit hit)
    {
        // Pastikan tidak bisa nge-lock dadu yang posisinya masih di langit/sedang jatuh
        if (hit.collider.transform.position.y > 3f) return;

        // Mengambil script dari parent collider (pastikan collider ada di objek anak atau sesuaikan)
        DiceEvaluate dice = hit.collider.GetComponentInParent<DiceEvaluate>();
        if (dice == null) return;

        dice.locked = !dice.locked;

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
                int rotationIndex = Mathf.Clamp(dice.GetTopNumber() - 1, 0, standardRotation.Length - 1);

                dice.setPosAndRot(
                    lockedPlaces[currentIndex].position,
                    standardRotation[rotationIndex]
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
        for (int i = 0; i < dices.Length; i++)
        {
            if (dices[i].IsMoving()) return true;
        }
        return false;
    }
}