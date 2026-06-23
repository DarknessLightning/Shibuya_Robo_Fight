using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TokenMovement : MonoBehaviour
{
    public Transform[] FameTiles;
    public Transform[] DestructionTiles;
    public Transform[] EndTiles;

    public GameObject FameToken;
    public GameObject DestructionToken;

    private GameObject selectedToken;

    private int fameIndex = 7;
    private int destructionIndex = 7;

    public float duration = 0f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //fameIndex = Mathf.CeilToInt((float)FameTiles.Length/2);
        //destructionIndex = Mathf.CeilToInt((float)DestructionTiles.Length / 2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public IEnumerator MovePhase(int fameMovement, int destructionMovement)
    {
        fameIndex += fameMovement;
        destructionIndex += destructionMovement;
        
        if(fameIndex > 0 && fameIndex < 14)
        {
            yield return MoveToken(FameToken, FameTiles[fameIndex - 1]);
        }
        else
        {
            yield return MoveToken(FameToken, EndTiles[fameIndex / 14]);
        }
        if (destructionIndex > 0 && destructionIndex < 14)
        {
            yield return MoveToken(DestructionToken, DestructionTiles[destructionIndex - 1]);
        }
        else
        {
            yield return MoveToken(DestructionToken, EndTiles[destructionIndex / 14]);
        }
    }

    public IEnumerator Move(DiceFace face, int delta)
    {
        GameObject token = null;
        List<Transform> target = new();
        int Start = 0;
        if(face == DiceFace.Fame)
        {
            Start = fameIndex;
            fameIndex += delta;
            token = FameToken;
            for(int i = 1; i <= Mathf.Abs(delta); i++)
            {
                int index = Start + i * delta / Mathf.Abs(delta);
                if (index <= 0 || index >= 14)
                {
                    target.Add(EndTiles[index / 14]);
                    break;
                }

                target.Add(FameTiles[index - 1]);
            }
            duration /= target.Count;
            foreach(Transform t in target)
            {
                yield return MoveToken(FameToken, t);
            }
            /*
            if (fameIndex > 0 && fameIndex < 14)
            {
                //yield return MoveToken(FameToken, FameTiles[fameIndex - 1]);
                target = FameTiles[fameIndex - 1];
            }
            else
            {
                //yield return MoveToken(FameToken, EndTiles[fameIndex / 14]);
                target = EndTiles[fameIndex / 14];
            }
            //*/
        }

        else if(face == DiceFace.Destruction)
        {
            Start = destructionIndex;
            destructionIndex += delta;
            token = DestructionToken;
            for (int i = 1; i <= Mathf.Abs(delta); i++)
            {
                int index = Start + i * delta / Mathf.Abs(delta);
                if (index <= 0 || index >= 14)
                {
                    target.Add(EndTiles[index / 14]);
                    break;
                }

                target.Add(DestructionTiles[index - 1]);
            }
            duration /= target.Count;
            foreach (Transform t in target)
            {
                yield return MoveToken(DestructionToken, t);
            }

            /*
            if (destructionIndex > 0 && destructionIndex < 14)
            {
                //yield return MoveToken(DestructionToken, DestructionTiles[destructionIndex - 1]);
                target = FameTiles[destructionIndex - 1];
            }
            else
            {
                //yield return MoveToken(DestructionToken, EndTiles[destructionIndex / 14]);
                target = EndTiles[destructionIndex / 14];
            }
            //*/
        }
    }



    public IEnumerator MoveToken(GameObject token, Transform tile)
    {
        Vector3 start = token.transform.position;
        Vector3 end = new Vector3(tile.position.x, start.y, tile.position.z);

        float elapsed = 0f;
        //float duration = 1f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            token.transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }
        token.transform.position = end;
    }

    public void moveToken(ref int index, bool left, Transform[] tiles)
    {
        index += left ? -1 : 1;
        index = Mathf.Clamp(index, 0, 14);
        if(index > 0 && index < 14)
        {
            selectedToken.transform.position = new Vector3(tiles[index - 1].position.x,
                selectedToken.transform.position.y,
                tiles[index - 1].position.z);
        }
        else
        {
            selectedToken.transform.position = new Vector3(EndTiles[index/14].position.x,
                selectedToken.transform.position.y,
                EndTiles[index/14].position.z);
        }
    }
}
