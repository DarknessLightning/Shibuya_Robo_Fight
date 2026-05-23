using UnityEngine;

[CreateAssetMenu(fileName = "CurrentGameSession", menuName = "Game/Session")]
public class GameSessionData : ScriptableObject
{
    public CharacterData playerCharacter;
    public CharacterData enemyCharacter;
}
