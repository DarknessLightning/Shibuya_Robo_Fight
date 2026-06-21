using UnityEngine;
using UnityEngine.UI;

public class CardDraftingSystem : MonoBehaviour
{
    public AbilityCard[] allCards;
    public AbilityCard[] shuffledCards;
    public AbilityCard[] openedCards;

    public Image[] openedCardsUI;
    public GameObject[] openedCardsObject;

    public Image ChosenCard;
    private AbilityCard selectedCard = null;
    private int nextIndex = 0;
    private int selected = -1;

    public int usablePoints = -1;

    public AudioClip buySfx;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        allCards = Resources.LoadAll<AbilityCard>("AbilityCards");

        refreshCards();
        showCards(openedCardsUI, openedCards, openedCardsObject);
    }

    private void refreshCards()
    {
        nextIndex = 0;
        shuffledCards = (AbilityCard[])allCards.Clone();
        shuffle(shuffledCards);

        openedCards = new AbilityCard[openedCardsUI.Length];
        for(int i = 0; i < openedCardsUI.Length; i++)
        {
            openedCards[i] = shuffledCards[i];
            nextIndex++;
        }
    }

    private void shuffle(AbilityCard[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int rand = Random.Range(i, array.Length);
            AbilityCard temp = array[i];
            array[i] = array[rand];
            array[rand] = temp;
        }
    }

    private void showCards(Image[] ui, AbilityCard[] cards, GameObject[] objects)
    {
        for (int i = 0; i < Mathf.Min(ui.Length, cards.Length); i++)
        {
            if (cards[i] != null)
            {
                ui[i].sprite = cards[i].cardSprite;
                Renderer rend = objects[i].GetComponent<Renderer>();
                rend.material.SetColor("_BaseColor", Color.white);
                //rend.material.color = Color.white;
                rend.material.SetTexture("_BaseMap", cards[i].cardSprite.texture);
                //rend.material.mainTexture = cards[i].cardSprite.texture;
            }
            setImage(cards[i] != null, ui[i], objects[i]);
        }
        if (ui.Length > cards.Length)
        {
            for (int i = cards.Length; i < ui.Length; i++)
            {
                setImage(false, ui[i], objects[i]);
            }
        }
    }



    public void setImage(bool withImage, Image ui, GameObject obj)
    {
        float alpha = 0f;
        obj.SetActive(withImage);
        if (withImage)
        {
            alpha = 1f;
        }
        Color temp = ui.color;
        temp.a = alpha;
        ui.color = temp;
        ui.raycastTarget = withImage;
    }

    public void selectCard(int index)
    {
        if (usablePoints < openedCards[index].cost)
        {
            return;
        }
        FightManager.instance.buyCardPhase(true);
        selected = index;
        selectedCard = openedCards[index];
        ChosenCard.sprite = selectedCard.cardSprite;
        FightManager.instance.selectCardPhase(false);
    }

    public void closeBuyPanel()
    {
        selected = -1;
        selectedCard = null;
        FightManager.instance.buyCardPhase(false);
        FightManager.instance.selectCardPhase(true);
        //FightManager.instance.SetCameraPos(FightManager.instance.BirdsEyeView);
    }

    public void closeCardDraftPanel()
    {
        FightManager.instance.CardDraftingPanel.SetActive(false);
    }

    public void buyCard()
    {
        AudioManager.instance.PlaySfx(buySfx);
        if (!FightManager.instance.PlayerTurn.isAI)
        {
            FightManager.instance.buyCard(selectedCard);
        }
        usablePoints -= selectedCard.cost;
        ExecuteCardEffect(selectedCard);
        //add or apply card

        openedCards[selected] = null;
        if(nextIndex < shuffledCards.Length)
        {
            openedCards[selected] = shuffledCards[nextIndex];
            nextIndex++;
        }
        if (openIsEmpty())
        {
            //refreshCards();
        }
        showCards(openedCardsUI, openedCards, openedCardsObject);
        closeBuyPanel();
        closeCardDraftPanel();
        FightManager.instance.UpdateAbilityPoints(usablePoints);
    }

    public void AIBuyCard(AbilityCard card)
    {
        int index = System.Array.IndexOf(openedCards, card);
        if (index < 0)
            return;

        selected = index;
        selectedCard = card;

        buyCard();
    }

    public void ExecuteCardEffect(AbilityCard card)
    {
        int value = card.effectValue;
        if (card.effectType == EffectType.Subtract) value *= -1;

        int repeat = -1;
        if (card.useForEach)
        {
            repeat = FightManager.instance.GetStateAmount(FightManager.instance.PlayerTurn, card.forEachTarget, card.forEachState);
            if(card.forEachMax > 0)
            {
                repeat = Mathf.Min(card.forEachMax, repeat);
            }
            if(repeat > -1)
            {
                value *= repeat;
            }
        }

        if(card.triggerEvent == TriggerEvent.None)
        {
            FightManager.instance.ApplyEffect(
                FightManager.instance.PlayerTurn,
                card.effectTarget,
                card.effectState, 
                value
                );
        }
        else
        {
            FightManager.instance.GivePermaCard(card);
        }

        if(card.tile != null)
        {
            FightManager.instance.GiveBuzzTile(card.tile);
        }
    }

    public void ResetPool()
    {
        if(usablePoints > -1 && usablePoints < 2)
        {
            return;
        }

        usablePoints -= 2;

        for(int i = 0; i < openedCards.Length; i++)
        {
            openedCards[i] = null;

            if(nextIndex < shuffledCards.Length)
            {
                openedCards[i] = shuffledCards[nextIndex];
                nextIndex++;
            }
        }
        if (openIsEmpty())
        {
            refreshCards();
        }
        showCards(openedCardsUI, openedCards, openedCardsObject);
        closeBuyPanel();
        closeCardDraftPanel();
        FightManager.instance.UpdateAbilityPoints(usablePoints);
    }

    public bool openIsEmpty()
    {
        for(int i = 0; i < openedCards.Length; i++)
        {
            if (openedCards[i] != null)
            {
                return false;
            }
        }

        return true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
