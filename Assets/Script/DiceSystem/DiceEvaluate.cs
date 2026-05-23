using UnityEngine;

public class DiceEvaluate : MonoBehaviour
{
    public Rigidbody rb;
    private Vector3 endPos;
    public DiceFace[] results;

    [Header("Drop Settings")]
    public float force = 5f;
    public float torque = 10f;

    // Variabel baru untuk jangkauan acak posisi jatuh
    public Vector3 centerPosition = new Vector3(15f, 5f, 0f); // Titik tengah di udara (Y=5)
    public float dropRadius = 1.5f; // Seberapa luas jangkauan acak area jatuh

    public bool locked = false;

    public int GetTopNumber()
    {
        float maxDot = -Mathf.Infinity;
        int topNumber = -1;

        CheckSide(transform.up, 1, ref maxDot, ref topNumber);
        CheckSide(-transform.up, 6, ref maxDot, ref topNumber);

        CheckSide(transform.forward, 5, ref maxDot, ref topNumber);
        CheckSide(-transform.forward, 2, ref maxDot, ref topNumber);

        CheckSide(transform.right, 3, ref maxDot, ref topNumber);
        CheckSide(-transform.right, 4, ref maxDot, ref topNumber);

        return topNumber;
    }

    public DiceFace GetTopFace(int index)
    {
        return results[index];
    }

    void CheckSide(Vector3 dir, int number, ref float maxDot, ref int topNumber)
    {
        float dot = Vector3.Dot(dir, Vector3.up);

        if (dot > maxDot)
        {
            maxDot = dot;
            topNumber = number;
        }
    }

    void Start()
    {
        Init();
    }

    public void Init()
    {
        if(rb == null)
        {
            rb = GetComponent<Rigidbody>();
            rb.useGravity = false;

        }
        resetEnd();
    }

    private void OnMouseDown()
    {
        Debug.Log("Mouse is on Dice. Result: " + GetTopNumber().ToString());
    }

    // ====================================================
    // PERUBAHAN UTAMA: ACAK POSISI AWAL SEBELUM DIJATUHKAN
    // ====================================================
    public void RollDice()
    {
        // 1. Acak posisi awal di udara (X dan Z) di dalam lingkaran radius tertentu
        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        Vector3 randomSpawnPos = new Vector3(
            centerPosition.x + randomCircle.x,
            centerPosition.y + Random.Range(-0.2f, 0.2f), // Sedikit variasi tinggi agar tidak tabrakan di udara
            centerPosition.z + randomCircle.y
        );

        // Terapkan posisi acak dan rotasi acak total sebelum jatuh
        transform.localPosition = randomSpawnPos;
        transform.localRotation = Quaternion.Euler(Random.Range(0f, 360f), Random.Range(0f, 360f), Random.Range(0f, 360f));

        // 2. Aktifkan Fisika
        rb.useGravity = true;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // 3. Beri gaya dorong ke bawah dan sedikit dorongan acak ke samping agar memantul alami
        Vector2 randomSpread = Random.insideUnitCircle * 1.5f;
        Vector3 dropForce = new Vector3(randomSpread.x, -force, randomSpread.y);
        rb.AddForce(dropForce, ForceMode.Impulse);

        // Beri efek putaran acak
        Vector3 randomTorque = Random.insideUnitSphere * torque;
        rb.AddTorque(randomTorque, ForceMode.Impulse);
    }

    // Mengembalikan ke posisi acak baru di atas (bukan posisi awal yang kaku)
    public void back2Starting()
    {
        rb.useGravity = false;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        // Langsung acak posisi baru lagi di langit agar siap untuk roll berikutnya
        Vector2 randomCircle = Random.insideUnitCircle * dropRadius;
        transform.localPosition = new Vector3(centerPosition.x + randomCircle.x, centerPosition.y, centerPosition.y + randomCircle.y);
        transform.localRotation = Quaternion.identity;
    }

    public void back2End()
    {
        transform.localPosition = endPos;
    }

    public void setPosAndRot(Vector3 pos, Vector3 rot)
    {
        transform.localPosition = pos;
        transform.localRotation = Quaternion.Euler(rot);
    }

    public bool IsMoving()
    {
        return rb.linearVelocity.sqrMagnitude > 0.01f || rb.angularVelocity.sqrMagnitude > 0.01f;
    }

    public void setEnd()
    {
        endPos = transform.localPosition;
    }

    public void resetEnd()
    {
        // Karena startingPos dihapus, endPos default disetel ke centerPosition di awal game
        endPos = centerPosition;
    }
}
