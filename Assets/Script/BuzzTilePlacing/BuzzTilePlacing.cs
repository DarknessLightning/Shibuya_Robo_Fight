using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public struct TilePlacementData
{
    public bool isFame;
    public int index;
    public bool kiri;
}

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
    public Sprite Selectable;

    public Image[] FameTilesUI;
    public GameObject[] FameTiles;
    public Image[] DestructionTilesUI;
    public GameObject[] DestructionTiles;

    private PlayerState[] FameStates = new PlayerState[13];
    private PlayerState[] DestructionStates = new PlayerState[13];

    private List<Image> option = new();
    public List<TilePlacementData> options = new(); 

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
        if(FightManager.instance != null)
        {
            /*OpenPanel(
                FightManager.instance.FameIndex,
                FightManager.instance.DestructionIndex,
                FightManager.instance.Player.Tile);
            */
        }
    }

    public PlayerState GetTileState(int index, bool isFame)
    {
        PlayerState state = PlayerState.None;
        if (isFame)
        {
            state = FameStates[index];
        }
        else
        {
            state = DestructionStates[index];
        }
        return state;
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
            rend.material.mainTexture = TileTexture[states.IndexOf(state) - 1].texture;
        }
    }

    public void OpenPanel(int fameIndex, int destructionIndex, BuzzTile bt)
    {
        option.Clear();
        UpdateTileSprites();
        FameTilesUI[fameIndex - 1].sprite = FameToken;
        DestructionTilesUI[destructionIndex - 1].sprite = DestructionToken;
        currentBuzzTile = bt;

        int fameClosestLeft = GetValidTile(FameStates,
            currentBuzzTile.tiles.Length,
            true,
            fameIndex - 1);
        if(fameClosestLeft > -1)
        {
            option.Add(FameTilesUI[fameClosestLeft]);
            options.Add(new TilePlacementData
            {
                isFame = true,
                index = fameClosestLeft,
                kiri = true
            });
        }


        int fameClosestRight = GetValidTile(FameStates,
            currentBuzzTile.tiles.Length,
            false,
            fameIndex - 1);
        if (fameClosestRight > -1)
        {
            option.Add(FameTilesUI[fameClosestRight]);
            options.Add(new TilePlacementData
            {
                isFame = true,
                index = fameClosestRight,
                kiri = false
            });
        }

        int destructClosestLeft = GetValidTile(DestructionStates,
            currentBuzzTile.tiles.Length,
            true,
            destructionIndex - 1);
        if(destructClosestLeft > -1)
        {
            option.Add(DestructionTilesUI[destructClosestLeft]);
            options.Add(new TilePlacementData
            {
                isFame = false,
                index = destructClosestLeft,
                kiri = true
            });
        }

        int destructClosestRight = GetValidTile(DestructionStates,
            currentBuzzTile.tiles.Length,
            false,
            destructionIndex - 1);
        if (destructClosestRight > -1)
        {
            option.Add(DestructionTilesUI[destructClosestRight]);
            options.Add(new TilePlacementData
            {
                isFame = false,
                index = destructClosestRight,
                kiri = false
            });
        }

        if(option.Count == 0)
        {
            fameClosestLeft = GetBestValid(FameStates,
            currentBuzzTile.tiles.Length,
            true,
            fameIndex - 1);
            if (fameClosestLeft > -1)
            {
                option.Add(FameTilesUI[fameClosestLeft]);
                options.Add(new TilePlacementData
                {
                    isFame = true,
                    index = fameClosestLeft,
                    kiri = true
                });
            }

            fameClosestRight = GetBestValid(FameStates,
                currentBuzzTile.tiles.Length,
                false,
                fameIndex - 1);
            if (fameClosestRight > -1)
            {
                option.Add(FameTilesUI[fameClosestRight]);
                options.Add(new TilePlacementData
                {
                    isFame = true,
                    index = fameClosestRight,
                    kiri = false
                });
            }

            destructClosestLeft = GetBestValid(DestructionStates,
                currentBuzzTile.tiles.Length,
                true,
                destructionIndex - 1);
            if (destructClosestLeft > -1)
            {
                option.Add(DestructionTilesUI[destructClosestLeft]);
                options.Add(new TilePlacementData
                {
                    isFame = false,
                    index = destructClosestLeft,
                    kiri = true
                });
            }

            destructClosestRight = GetBestValid(DestructionStates,
                currentBuzzTile.tiles.Length,
                false,
                destructionIndex - 1);
            if (destructClosestRight > -1)
            {
                option.Add(DestructionTilesUI[destructClosestRight]);
                options.Add(new TilePlacementData
                {
                    isFame = false,
                    index = destructClosestRight,
                    kiri = false
                });
            }
        }

        foreach(Image tile in option)
        {
            SelectableTiles(tile);
        }
    }

    public void SelectableTiles(Image closestTile)
    {
        if (!FameTilesUI.Contains(closestTile) && !DestructionTilesUI.Contains(closestTile)) return;

        Image[] field = FameTilesUI.Contains(closestTile) ? FameTilesUI : DestructionTilesUI;
        int index = FameTilesUI.Contains(closestTile) ?
            FightManager.instance.FameIndex - 1:
            FightManager.instance.DestructionIndex - 1;
        int closestIndex = Array.IndexOf(field, closestTile);
        int dir = closestIndex < index ? -1 : 1;

        for(int i = 0; i < currentBuzzTile.tiles.Length; i++)
        {
            field[closestIndex + (i * dir)].sprite = Selectable;
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

    public int GetBestValid(PlayerState[] state, int length, bool descending, int tokenIndex)
    {
        int bestIndex = -1;
        int leastOverlap = int.MaxValue;

        if (descending)
        {
            for (int i = tokenIndex - 1; i >= 0; i--)
            {
                if (state[i] != PlayerState.None)
                {
                    continue;
                }

                int overlap = 0;
                bool outOfBounds = false;

                for (int j = 1; j < length; j++)
                {
                    int checkIndex = i - j;

                    if (checkIndex < 0)
                    {
                        outOfBounds = true;
                        break;
                    }

                    if (state[checkIndex] != PlayerState.None)
                    {
                        overlap += (length - j) * (length - j);
                    }
                }

                // Muat penuh
                if (!outOfBounds && overlap == 0)
                {
                    return i;
                }

                // Abaikan yang mentok ujung
                if (outOfBounds)
                {
                    continue;
                }

                if (overlap < leastOverlap)
                {
                    leastOverlap = overlap;
                    bestIndex = i;
                }
            }
        }
        else
        {
            for (int i = tokenIndex + 1; i < state.Length; i++)
            {
                if (state[i] != PlayerState.None)
                {    
                    continue;
                }

                int overlap = 0;
                bool outOfBounds = false;

                for (int j = 1; j < length; j++)
                {
                    int checkIndex = i + j;

                    if (checkIndex >= state.Length)
                    {
                        outOfBounds = true;
                        break;
                    }

                    if (state[checkIndex] != PlayerState.None)
                    {
                        overlap += (length - j) * (length - j);
                    }
                }

                if (!outOfBounds && overlap == 0)
                {
                    return i;
                }

                if (outOfBounds)
                {
                    continue;
                }

                if (overlap < leastOverlap)
                {
                    leastOverlap = overlap;
                    bestIndex = i;
                }
            }
        }

        return bestIndex;
    }

    public void SetSpritePreview()
    {
        if(selectedField == null) return;

        int index = selectedField == FameStates ?
            FightManager.instance.FameIndex - 1 :
            FightManager.instance.DestructionIndex - 1;
        Image[] field = selectedField == FameStates ?
            FameTilesUI :
            DestructionTilesUI;
        int dir = selectedIndex < index ? -1 : 1;
        int start = dir == -1 ? currentBuzzTile.tiles.Length - 1 : 0;
        kiri = dir == -1;
        for(int i = 0; i < currentBuzzTile.tiles.Length; i++)
        {
            SetSprite(field[selectedIndex + (i * dir)], currentBuzzTile.tiles[start + (i * dir)]);
        }

        /*
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
        */
    }

    public void SetSprite(Image tile, PlayerState state)
    {
        if (!states.Contains(state)) return;
        tile.sprite = TileSprites[states.IndexOf(state)];
        tile.preserveAspect = true;
    }

    public void selectTile(Image tile)
    {
        if (FightManager.instance.PlayerTurn.isAI) return;
        if (!FameTilesUI.Contains(tile) && !DestructionTilesUI.Contains(tile)) return;
        if (!option.Contains(tile)) return;

        foreach (Image tiles in option)
        {
            SelectableTiles(tiles);
        }
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
        SetSpritePreview();
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
        else
        {
            for (int i = 0; i < currentBuzzTile.tiles.Length; i++)
            {
                int fieldIndex = selectedIndex + i;
                selectedField[fieldIndex] = currentBuzzTile.tiles[i];
            }
        }

        UpdateTileSprites();
        currentBuzzTile = null;
        selectedField = null;
        kiri = false;

        FightManager.instance.PlayerTurn.Tile = null;
        FightManager.instance.EndBuzzTilePlacing();
    }

    public void AIPlaceTile(bool fameField, int index, BuzzTile tile, bool kiri)
    {
        selectedField = fameField ? FameStates : DestructionStates;
        selectedIndex = index;
        currentBuzzTile = tile;
        this.kiri = kiri;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
