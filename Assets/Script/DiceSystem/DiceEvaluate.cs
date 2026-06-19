using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DiceEvaluate : MonoBehaviour
{
    public Rigidbody rb;
    private Vector3 endPos;
    public DiceFace[] results;

    [Header("Drop Settings")]
    public float force = 5f;
    public float torque = 10f;

    public List<Vector3> standardRotations = new List<Vector3>
    {
        new Vector3(0,0,0),
        new Vector3(90,0,0),
        new Vector3(0,-90,90),
        new Vector3(0,-90,-90),
        new Vector3(-90,0,0),
        new Vector3(180,0,0)
    };

    // Variabel baru untuk jangkauan acak posisi jatuh
    public Vector3 centerPosition = new Vector3(0f, 5f, -45f); // Titik tengah di udara (Y=5)
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

    public DiceFace GetTopFace
    {
        get
        {
            int result = GetTopNumber();
            return results[result - 1];
        }
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

        //StartCoroutine(ForceStopAfter1Second());
        StartCoroutine(ForceFace());
    }

    public IEnumerator ForceStopAfter1Second()
    {
        yield return new WaitForSeconds(1f);

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = false;

        SnapToFace(GetTopNumber());
    }

    public IEnumerator ForceFace()
    {
        yield return new WaitUntil(() => IsMoving());
        yield return new WaitUntil(() => !IsMoving());

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.useGravity = true;

        if (!isFlatOnGround(GetTopNumber()))
        {
            SnapToFace(GetTopNumber());
        }
    }

    bool isFlatOnGround(int i)
    {
        Vector3 dir = Vector3.zero;

        switch (i)
        {
            case 1:
                dir = transform.up;
                break;

            case 2:
                dir = -transform.forward;
                break;

            case 3:
                dir = transform.right;
                break;

            case 4:
                dir = -transform.right;
                break;

            case 5:
                dir = transform.forward;
                break;

            case 6:
                dir = -transform.up;
                break;

        }

        return Vector3.Dot(dir, Vector3.up) > 0.85f;
    }

    void SnapToFace(int face)
    {
        transform.rotation = Quaternion.Euler(standardRotations[face - 1]);
        return;
        /*
        switch (face)
        {
            case 1:
                transform.rotation = Quaternion.Euler(standardRotations[face - 1]);
                break;

            case 2:
                transform.rotation = Quaternion.Euler(standardRotations[face - 1]);
                break;

            case 3:
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.left);
                break;

            case 4:
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.right);
                break;

            case 5:
                transform.rotation = Quaternion.LookRotation(Vector3.up, Vector3.forward);
                break;

            case 6:
                transform.rotation = Quaternion.LookRotation(Vector3.forward, Vector3.down);
                break;
        }
        //*/
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
