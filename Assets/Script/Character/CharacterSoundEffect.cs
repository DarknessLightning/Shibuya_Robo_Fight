using UnityEngine;

[CreateAssetMenu(fileName = "CharacterSoundEffect", menuName = "Game/CharacterSoundEffect")]
public class CharacterSoundEffect : ScriptableObject
{
    public AudioClip Attack;
    public AudioClip Energize;
    public AudioClip Heal;

    public AudioClip Hurt;
    public AudioClip Signal;

    public AudioClip Destruction;
    public AudioClip Fame;
}
