using UnityEngine;

public enum SpecialSkill
{
    SS001,
    SS002, 
    SS003,
    SS004,
}

[CreateAssetMenu(fileName = "CharacterData", menuName = "Game/Character")]
public class CharacterData : ScriptableObject
{
    public string characterName;

    public int hp;
    public SpecialSkill skill;
    [TextArea(3, 10)]
    public string skillDescription;

    public GameObject characterModel;
    public Sprite characterSprite;
    public Sprite icon;
    public Sprite info;
    public Sprite specialSkill;

    public CharacterSoundEffect soundEffects;
}
