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
            Instantiate(kometPrefab, titikTembak.position, titikTembak.rotation);
        }
    }
}