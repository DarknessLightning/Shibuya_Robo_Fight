using UnityEngine;

[CreateAssetMenu(fileName = "BuzzTile", menuName = "Game/BuzzTile")]
public class BuzzTile : ScriptableObject
{
    public PlayerState[] tiles;

    public BuzzTile createNewBuzzTile(PlayerState[] tile)
    {
        BuzzTile buzzTile = new BuzzTile();
        buzzTile.tiles = (PlayerState[])tile.Clone();
        return buzzTile;
    }
}