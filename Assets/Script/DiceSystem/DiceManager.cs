using NUnit.Framework;
using System.Collections.Generic;
using System.Net;
using UnityEngine;

public class DiceManager : MonoBehaviour
{
    public DiceEvaluate[] dices;
    private List<DiceEvaluate> lockedDices = new();
    public Transform[] lockedPlaces;
    public bool isMoving;
    public Vector3[] standardRotation;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if(IsMoving()) return;
        if (Input.GetKeyDown(KeyCode.X))
        {
            string msg = "";
            for(int i = 0; i < dices.Length; i++)
            {
                msg += "Dice " + (i+1) + " Result: " + dices[i].GetTopNumber() + "\n";
                Debug.Log("Dice " + i + " Result: " + dices[i].GetTopNumber());
            }
            Debug.Log(msg);
        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            foreach(DiceEvaluate dice in dices)
            {
                if(dice.locked) continue;
                if (dice.transform.position.y < 3)
                {
                    dice.back2Starting();
                    dice.resetEnd();
                    dice.rb.useGravity = false; 
                }
                dice.RollDice();
            }
        }
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                Debug.Log("Klik: " + hit.collider.name);
                lockDice(hit);
            }
        }

    }

    public void lockDice(RaycastHit hit)
    {
        if (hit.collider.transform.position.y > 3) return;
        DiceEvaluate dice = hit.collider.transform.parent.GetComponent<DiceEvaluate>();
        if (dice == null) return;
        
        dice.locked = !dice.locked;
        if (dice.locked)
        {
            dice.setEnd();
            if(!lockedDices.Contains(dice)) lockedDices.Add(dice);
            if (lockedDices.IndexOf(dice) < lockedPlaces.Length)
            {
                dice.setPosAndRot(
                    lockedPlaces[lockedDices.IndexOf(dice)].position,
                    standardRotation[dice.GetTopNumber() - 1]);
                dice.rb.useGravity = true;
            }
        }
        else
        {
            dice.back2End();
            dice.rb.useGravity = true;
            int starting = lockedDices.IndexOf(dice);
            lockedDices.Remove(dice);
            moveDicesToPreviousIndexPlace(starting);
        }
        
    }

    public void moveDicesToPreviousIndexPlace(int starting)
    {
        for(int i = starting; i < lockedDices.Count; i++)
        {
            lockedDices[i].transform.position = lockedPlaces[i].position;
        }
    }

    public bool IsMoving()
    {
        for(int i = 0; i < dices.Length; i++)
        {
            if (dices[i].IsMoving()) return true;
        }

        return false;
    }
}
