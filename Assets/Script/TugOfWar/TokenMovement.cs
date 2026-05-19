using Unity.VisualScripting.ReorderableList;
using UnityEngine;

public class TokenMovement : MonoBehaviour
{
    public Transform[] FameTiles;
    public Transform[] DestructionTiles;
    public Transform[] EndTiles;

    public GameObject FameToken;
    public GameObject DestructionToken;

    private GameObject selectedToken;

    private int fameIndex = 4;
    private int destructionIndex = 4;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F))
        {
            selectedToken = FameToken;
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            selectedToken = DestructionToken;
        }
        if(selectedToken != null)
        {
            if(Input.GetKeyDown(KeyCode.LeftArrow))
            {
                if(selectedToken == FameToken) 
                {
                    moveToken(ref fameIndex, true, FameTiles);
                }
                else if (selectedToken == DestructionToken)
                {
                    moveToken(ref destructionIndex, true, DestructionTiles);
                }
            }
            else if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                if (selectedToken == FameToken)
                {
                    moveToken(ref fameIndex, false, FameTiles);
                }
                else if (selectedToken == DestructionToken)
                {
                    moveToken(ref destructionIndex, false, DestructionTiles);
                }
            }
        }
    }

    public void moveToken(ref int index, bool left, Transform[] tiles)
    {
        index += left ? -1 : 1;
        index = Mathf.Clamp(index, 0, 8);
        if(index > 0 && index < 8)
        {
            selectedToken.transform.position = new Vector3(tiles[index - 1].position.x,
                selectedToken.transform.position.y,
                tiles[index - 1].position.z);
        }
        else
        {
            selectedToken.transform.position = new Vector3(EndTiles[index/8].position.x,
                selectedToken.transform.position.y,
                EndTiles[index/8].position.z);
        }
    }
}
