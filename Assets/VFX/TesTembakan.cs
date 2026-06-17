using System.Collections;
using UnityEngine;

public enum Partikel
{
    none, 
    shoot,
    heal,
    charge
}

public class TesTembakan : MonoBehaviour
{
    public GameObject kometPrefab; // Slot untuk memasukkan efekmu
    public Transform titikTembak;  // Slot untuk asal keluarnya komet

    public GameObject healVFX;
    public GameObject chargeVFX;
    public Transform player;

    void Update()
    {
        /*
        // Jika menekan tombol Spasi
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Munculkan komet di posisi dan arah Titik Tembak
            GameObject partikel = Instantiate(kometPrefab, titikTembak);
            StartCoroutine(DestroyWhenDone(partikel)); // Mulai coroutine untuk efek tembakan
        }
        else if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            GameObject partikel = Instantiate(healVFX, player);
            StartCoroutine(DestroyWhenDone(partikel));
        }
        else if (Input.GetKeyDown(KeyCode.RightShift))
        {
            GameObject partikel = Instantiate(chargeVFX, player);
            StartCoroutine(DestroyWhenDone(partikel));
        }
        //*/
    }

    public IEnumerator playVFX(Vector3 pos, Partikel partikel)
    {
        GameObject partikelVfx = null;
        switch (partikel)
        {
            case Partikel.shoot:
                partikelVfx = Instantiate(kometPrefab, pos + kometPrefab.transform.position, 
                    Quaternion.Euler(new Vector3(0, 90, 0)));
                yield return new WaitForSeconds(1f);
                break;
            case Partikel.heal:
                yield return new WaitForSeconds(1f);
                partikelVfx = Instantiate(healVFX, pos, Quaternion.Euler(Vector3.zero));
                yield return new WaitForSeconds(1.5f);
                break;
            case Partikel.charge:
                yield return new WaitForSeconds(1f);
                partikelVfx = Instantiate(chargeVFX, pos + chargeVFX.transform.position, Quaternion.Euler(Vector3.zero));
                yield return new WaitForSeconds(5f);
                break;
        }
        Destroy(partikelVfx);
    }

    public IEnumerator DestroyWhenDone(GameObject kometPrefab)
    {
        yield return new WaitForSeconds(1f); // Tunggu setengah detik sebelum menembak
        Destroy(kometPrefab); // Aktifkan efek komet

    }
}