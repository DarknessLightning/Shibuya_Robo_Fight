using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSoundEffect", menuName = "Game/CharacterSoundEffect")]
public class CharacterSoundEffect : ScriptableObject
{
    [Header("Attack")]
    public AudioClip Attack;
    public float timingForAttack;

    [Header("Charge")]
    public AudioClip Energize;
    public float timingForCharge;

    [Header("Heal")]
    public AudioClip Heal;
    public float timingForHeal;

    [Header("Hurst")]
    public AudioClip Hurt;

    [Header("Signal")]
    public AudioClip Signal;
    public float timingForSignal;

    [Header("Destruction")]
    public AudioClip Destruction;
    public float timingForDestruction;
    [Header("Fame")]
    public AudioClip Fame;
    public float timingForFame;

    [Header("Lose")]
    public AudioClip Lose;
}
