using System.Collections;
using UnityEngine;

public class TesTembakan : MonoBehaviour
{
    public GameObject kometPrefab; // Slot untuk memasukkan efekmu
    public Transform titikTembak;  // Slot untuk asal keluarnya komet

    void Update()
    {
        // Jika menekan tombol Spasi
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Munculkan komet di posisi dan arah Titik Tembak
            GameObject partikel = Instantiate(kometPrefab, titikTembak.position, titikTembak.rotation);
            StartCoroutine(TembakanCoroutine(partikel)); // Mulai coroutine untuk efek tembakan
        }
    }

    public IEnumerator TembakanCoroutine(GameObject kometPrefab)
    {
        yield return new WaitForSeconds(1f); // Tunggu setengah detik sebelum menembak
        Destroy(kometPrefab); // Aktifkan efek komet

    }
}