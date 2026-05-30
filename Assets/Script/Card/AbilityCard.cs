using UnityEngine;

[CreateAssetMenu(fileName = "AbilityCard", menuName = "Game/AbilityCard")]
public class AbilityCard : ScriptableObject
{
    [Header("Cost")]
    public int cost;

    // =========================
    // TRIGGER (KAPAN AKTIF)
    // =========================
    [Header("Trigger")]
    public TriggerEvent triggerEvent;   // None = instant
    public SubjectTarget triggerSubject;
    public PlayerState triggerState;

    // =========================
    // CONDITION (OPTIONAL)
    // =========================
    [Header("Condition")]
    public bool useCondition;

    public SubjectTarget conditionSubject;
    public PlayerState conditionState;
    public Comparative conditionComparative;
    public int conditionValue;

    // =========================
    // EFFECT
    // =========================
    [Header("Effect")]
    public SubjectTarget effectTarget;
    public EffectType effectType;
    public PlayerState effectState;
    public int effectValue;

    // Scaling (ForEach)
    public bool useForEach;
    public SubjectTarget forEachTarget;
    public PlayerState forEachState;
    public int forEachMax;

    //Buzz Tile
    public BuzzTile tile;

    [Header("Reference")]
    public Sprite cardSprite;
    public string cardName;
}

public enum TriggerEvent
{
    None,
    TurnStart,
    OnAdd,
    OnSubtract
}

public enum SubjectTarget
{
    Self,
    Opponent
}

public enum Comparative
{
    More,
    Equal,
    Less,
    AtLeast,
    NoMore
}

public enum EffectType
{
    Add,
    Subtract
}

public enum PlayerState
{
    HealthPoint,
    AbilityPoints,
    AbilityCard,
    Dice,
    Fame,
    Destruction,
    Turn,
    Reroll,
    None
}

