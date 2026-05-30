using System.Collections.Generic;
using UnityEngine;

public enum AIState
{
    Idle,
    DiceRoll,
    CardBuy,
    BuzzTile,
    EndTurn
}

public class AILogic : MonoBehaviour
{
    [Header("AI State")]
    public AIState currentState = AIState.Idle;

    public PlayerData self;
    public PlayerData enemy;

    public List<DiceFace> currentDice = new();

    int rerollRemain = 2;

    public List<DiceFace> lockedDice = new();

    public void StartInput()
    {

    }

    public void StartAction(AIState state)
    {
        currentState = state;
        ExcecuteAction();
    }

    public void ExcecuteAction()
    {
        switch (currentState)
        {
            case AIState.Idle:
                break;
            case AIState.DiceRoll:
                EvaluateState();
                break;
            case AIState.CardBuy:
                DecideBuyCard();
                break;
            case AIState.BuzzTile:
                DecideBuzzTile();
                break;
            case AIState.EndTurn:
                EndTurn();
                break;
            default:
                break;
        }
    }

    void EvaluateState()
    {
        DecideDiceLock();

        if (rerollRemain > 0 && lockedDice.Count < 6)
        {
            RerollDice();
        }
        else
        {
            ResolveDice();
        }
    }


    void DecideDiceLock()
    {
        Dictionary<DiceFace, float> utility =
            CalculateDiceUtility();

        foreach (var dice in currentDice)
        {
            float score = utility[dice];

            if (score >= 70)
            {
                lockedDice.Add(dice);
            }
        }
    }


    Dictionary<DiceFace, float>
    CalculateDiceUtility()
    {
        Dictionary<DiceFace, float> score =
        new();

        score[DiceFace.Attack] = 20;
        score[DiceFace.Heal] = 20;
        score[DiceFace.Charge] = 20;
        score[DiceFace.Fame] = 20;
        score[DiceFace.Destruction] = 20;
        score[DiceFace.Power] = 20;


        //---------------------
        // SURVIVAL MODE
        //---------------------

        if (self.CurrentHP < 5)
        {
            score[DiceFace.Heal] += 80;
        }


        //---------------------
        // LETHAL MODE
        //---------------------

        if (enemy.CurrentHP <= 4)
        {
            score[DiceFace.Attack] += 100;
        }


        //---------------------
        // TUG OF WAR MODE
        //---------------------

        if (FightManager.instance.FameIndex >= 12)
        {
            score[DiceFace.Fame] += 90;
        }

        if (FightManager.instance.DestructionIndex >= 12)
        {
            score[DiceFace.Destruction] += 90;
        }


        //---------------------
        // CHARACTER BONUS
        //---------------------

        switch (self.character.skill)
        {
            case SpecialSkill.SS001:

                score[DiceFace.Power] += 50;
                break;

            case SpecialSkill.SS002:

                score[DiceFace.Charge] += 30;
                score[DiceFace.Power] += 40;
                break;

            case SpecialSkill.SS003:

                score[DiceFace.Attack] += 40;
                score[DiceFace.Power] += 40;
                break;

            case SpecialSkill.SS004:

                score[DiceFace.Power] += 30;
                break;
        }


        return score;
    }


    void RerollDice()
    {
        rerollRemain--;

        Debug.Log("AI reroll");

        // panggil sistem dadu
    }



    void ResolveDice()
    {
        Debug.Log("Resolve");
    }



    void DecideBuyCard()
    {
        AbilityCard bestCard = null;

        float highest = -999;

        foreach (var card in FightManager.instance.cardDraftingSystem.openedCards)
        {
            if (card.cost > self.AbilityPoints)
                continue;

            float score =
                EvaluateCard(card);

            if (score > highest)
            {
                highest = score;
                bestCard = card;
            }
        }

        if (bestCard != null)
        {
            Buy(bestCard);
        }
    }


    float EvaluateCard(AbilityCard card)
    {
        float score = 0;

        // gak bisa beli
        if (card.cost > self.AbilityPoints)
            return -999;


        //-----------------------------------
        // EFFECT VALUE
        //-----------------------------------

        int value = card.effectValue;


        //-----------------------------------
        // FOREACH SCALING
        //-----------------------------------

        if (card.useForEach)
        {
            int amount =
                GetStateValue(
                    card.forEachTarget,
                    card.forEachState
                );

            amount = Mathf.Min(
                amount,
                card.forEachMax
            );

            value *= amount;
        }



        //-----------------------------------
        // EFFECT PRIORITY
        //-----------------------------------

        switch (card.effectState)
        {
            case PlayerState.HealthPoint:

                if (card.effectTarget ==
                    SubjectTarget.Self)
                {
                    // heal lebih penting saat sekarat
                    if (self.CurrentHP < 5)
                        score += value * 20;
                    else
                        score += value * 5;
                }
                else
                {
                    // damage ke lawan

                    if (enemy.CurrentHP <= value)
                        score += 200;
                    else
                        score += value * 10;
                }

                break;



            case PlayerState.AbilityPoints:

                score += value * 8;
                break;



            case PlayerState.Fame:

                score += value * 15;

                if (FightManager.instance.FameIndex >= 12)
                    score += 100;

                break;



            case PlayerState.Destruction:

                score += value * 15;

                if (FightManager.instance.DestructionIndex >= 12)
                    score += 100;

                break;



            case PlayerState.Dice:

                score += value * 25;
                break;


            case PlayerState.Reroll:

                score += value * 20;
                break;



            case PlayerState.AbilityCard:

                score += value * 12;
                break;
        }



        //-----------------------------------
        // BUZZ BONUS
        //-----------------------------------
        
        if (card.tile != null)
        {
            score += 30;
        }

        

        //-----------------------------------
        // CONDITION CHECK
        //-----------------------------------

        if (card.useCondition)
        {
            bool condition =
                EvaluateCondition(card);

            if (!condition)
                score *= 0.5f;
        }


        //-----------------------------------
        // PERMANENT BONUS
        //-----------------------------------

        if (card.triggerEvent !=
            TriggerEvent.None)
        {
            score *= 1.3f;
        }

        return score;
    }

    bool EvaluateCondition(AbilityCard card)
    {
        int value =
            GetStateValue(
                card.conditionSubject,
                card.conditionState
            );

        switch (card.conditionComparative)
        {
            case Comparative.More:

                return value >
                    card.conditionValue;

            case Comparative.Equal:

                return value ==
                    card.conditionValue;

            case Comparative.Less:

                return value <
                    card.conditionValue;

            case Comparative.AtLeast:

                return value >=
                    card.conditionValue;

            case Comparative.NoMore:

                return value <=
                    card.conditionValue;
        }

        return true;
    }



    void DecideBuzzTile()
    {/*
        BuzzTile bestTile =
            BoardManager.instance
            .GetBestTile();

        if (bestTile != null)
        {
            PlaceBuzz(bestTile);
        }*/
    }



    void Buy(AbilityCard card)
    {
        Debug.Log("Buy " + card.name);
    }
    /*
    void PlaceBuzz(BuzzTile tile)
    {
        Debug.Log("Place " + tile.name);
    }*/


    void EndTurn()
    {
        Debug.Log("End Turn");
    }

    int GetStateValue(
    SubjectTarget target,
    PlayerState state)
    {
        PlayerData actor =
            target ==
            SubjectTarget.Self
            ? self
            : enemy;
        int endPoint =
            actor == FightManager.instance.Player ? 0 : 14;
        switch (state)
        {
            case PlayerState.HealthPoint:
                return actor.CurrentHP;

            case PlayerState.AbilityPoints:
                return actor.AbilityPoints;

            case PlayerState.Fame:
                return Mathf.Abs(FightManager.instance.FameIndex - endPoint);

            case PlayerState.Destruction:
                return Mathf.Abs(FightManager.instance.DestructionIndex - endPoint);

            case PlayerState.Dice:
                return actor.additionalDice;

            case PlayerState.Reroll:
                return actor.additionalReroll;
        }

        return 0;
    }
}
