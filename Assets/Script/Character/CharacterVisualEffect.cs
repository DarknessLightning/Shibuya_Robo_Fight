using UnityEngine;

[CreateAssetMenu(fileName = "CharacterVisualEffect", menuName = "Game/CharacterVisualEffect")]
public class CharacterVisualEffect : ScriptableObject
{
    [Header("Heal")]
    public GameObject healFx;
    public float healDelay;

    [Header("Attack")]
    public GameObject attackFx;
    public float attackDelay;

    [Header("Energize")]
    public GameObject energizeFx;
    public float energizeDelay;



}
