using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class BuzzTilePlacing : MonoBehaviour
{
    public Sprite[] TileSprites;
    public Sprite[] TileTexture;
    private List<PlayerState>states = new() {
        PlayerState.None,
        PlayerState.HealthPoint,
        PlayerState.AbilityPoints,
        PlayerState.Dice
    };
    public Sprite FameToken;
    public Sprite DestructionToken;

    public Image[] FameTilesUI;
    public GameObject[] FameTiles;
    public Image[] DestructionTilesUI;
    public GameObject[] DestructionTiles;

    private PlayerState[] FameStates = new PlayerState[13];
    private PlayerState[] DestructionStates = new PlayerState[13];

    public BuzzTile currentBuzzTile;

    private int selectedIndex = 0;
    private PlayerState[] selectedField;
    private bool kiri = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        for(int i = 0; i < FameStates.Length; i++)
        {
            FameStates[i] = PlayerState.None;
            DestructionStates[i] = PlayerState.None;
        }
        UpdateTileSprites();
    }

    public void UpdateTileSprites()
    {
        for (int i = 0; i < FameStates.Length; i++)
        {
            SetSpriteAndTexture(FameTilesUI[i], FameTiles[i], FameStates[i]);
            SetSpriteAndTexture(DestructionTilesUI[i], DestructionTiles[i], DestructionStates[i]);
        }
    }

    public void SetSpriteAndTexture(Image tileUI, GameObject tileObject, PlayerState state)
    {
        if (!states.Contains(state)) return;
        tileUI.sprite = TileSprites[states.IndexOf(state)];
        tileUI.preserveAspect = true;
        Renderer rend = tileObject.GetComponent<Renderer>();
        if (state == PlayerState.None)
        {
            rend.material.mainTexture = null;
            tileObject.SetActive(false);
        }
        else
        {
            tileObject.SetActive(true);
            rend.material.mainTexture = TileTexture[states.IndexOf(state - 1)].texture;
        }
    }

    public void OpenPanel(int fameIndex, int destructionIndex, BuzzTile bt)
    {
        FameTilesUI[fameIndex - 1].sprite = FameToken;
        DestructionTilesUI[destructionIndex - 1].sprite = DestructionToken;
        currentBuzzTile = bt;

        int fameClosestLeft = GetValidTile(FameStates,
            currentBuzzTile.tiles.Length,
            true,
            fameIndex - 1);
        if(fameClosestLeft > -1)
        {
            FameTilesUI[fameClosestLeft].raycastTarget = true;
        }

        int fameClosestRight = GetValidTile(FameStates,
            currentBuzzTile.tiles.Length,
            false,
            fameIndex - 1);
        if (fameClosestRight > -1)
        {
            FameTilesUI[fameClosestRight].raycastTarget = true;
        }

        int destructClosestLeft = GetValidTile(DestructionStates,
            currentBuzzTile.tiles.Length,
            true,
            destructionIndex - 1);
        if(destructClosestLeft > -1)
        {
            DestructionTilesUI[destructClosestLeft].raycastTarget = true;
        }

        int destructClosestRight = GetValidTile(DestructionStates,
            currentBuzzTile.tiles.Length,
            false,
            destructionIndex - 1);
        if (destructClosestRight > -1)
        {
            DestructionTilesUI[destructClosestRight].raycastTarget = true;
        }

    }

    public int GetValidTile(PlayerState[] state, int length, bool descending, int tokenIndex)
    {
        if (descending)
        {
            for(int i = tokenIndex - 1; i >= 0; i--)
            {
                if (state[i] != PlayerState.None) continue;

                if (length == 1) return i;

                bool validSemua = true;
                for(int j = 1;  j < length; j++)
                {
                    if (i < j || state[i - j] != PlayerState.None)
                    {
                        validSemua = false; 
                        break;
                    }
                }
                if (validSemua)
                {
                    return i;
                }

            }
        }
        else
        {
            for(int i = tokenIndex + 1; i < state.Length; i++)
            {
                if (state[i] != PlayerState.None) continue;

                if (length == 1) return i;

                bool validSemua = true;
                for(int j = 1; j < length; j++)
                {
                    if(i + j >= state.Length || state[i + j] != PlayerState.None)
                    {
                        validSemua = false;
                        break;
                    }
                }
                if (validSemua)
                {
                    return i;
                }
            }
        }

        return -1;
    }

    public void SetSpritePreview()
    {
        if(selectedField == null) return;
        
        if(selectedField == FameStates)
        {
            if(selectedIndex < FightManager.instance.FameIndex - 1)
            {
                kiri = true;
                for(int i = 0; i < currentBuzzTile.tiles.Length; i++)
                {
                    int uiIndex = selectedIndex - i;
                    int tileIndex = currentBuzzTile.tiles.Length - i;
                    SetSprite(FameTilesUI[uiIndex], currentBuzzTile.tiles[tileIndex - 1]);
                }
            }
            else if(selectedIndex > FightManager.instance.FameIndex - 1)
            {
                kiri = false;
                for (int i = 0; i < currentBuzzTile.tiles.Length; i++)
                {
                    int uiIndex = selectedIndex + i;
                    int tileIndex = currentBuzzTile.tiles.Length + i;
                    SetSprite(FameTilesUI[uiIndex], currentBuzzTile.tiles[tileIndex - 1]);
                }
            }
        }
        else if (selectedField == DestructionStates)
        {
            if (selectedIndex < FightManager.instance.DestructionIndex - 1)
            {
                kiri = true;
                for (int i = 0; i < currentBuzzTile.tiles.Length; i++)
                {
                    int uiIndex = selectedIndex - i;
                    int tileIndex = currentBuzzTile.tiles.Length - i;
                    SetSprite(DestructionTilesUI[uiIndex], currentBuzzTile.tiles[tileIndex - 1]);
                }
            }
            else if (selectedIndex > FightManager.instance.DestructionIndex - 1)
            {
                kiri = false;
                for (int i = 0; i < currentBuzzTile.tiles.Length; i++)
                {
                    int uiIndex = selectedIndex - i;
                    int tileIndex = currentBuzzTile.tiles.Length - i;
                    SetSprite(DestructionTilesUI[uiIndex], currentBuzzTile.tiles[tileIndex - 1]);
                }
            }
        }
    }

    public void SetSprite(Image tile, PlayerState state)
    {
        if (!states.Contains(state)) return;
        tile.sprite = TileSprites[states.IndexOf(state)];
        tile.preserveAspect = true;
    }

    public void selectTile(Image tile)
    {
        if (!FameTilesUI.Contains(tile) || !DestructionTilesUI.Contains(tile)) return;

        if (FameTilesUI.Contains(tile))
        {
            selectedIndex = Array.IndexOf(FameTilesUI, tile);
            selectedField = FameStates;
        }
        else if (DestructionTilesUI.Contains(tile))
        {
            selectedIndex = Array.IndexOf(DestructionTilesUI, tile);
            selectedField = DestructionStates;
        }
    }

    public void confirm()
    {
        if (kiri)
        {
            for(int i = 0; i < currentBuzzTile.tiles.Length; i++)
            {
                int fieldIndex = selectedIndex - i;
                int tileIndex = currentBuzzTile.tiles.Length - i;
                selectedField[fieldIndex] = currentBuzzTile.tiles[tileIndex - 1];
            }
        }
        currentBuzzTile = null;
        selectedField = null;
        kiri = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
