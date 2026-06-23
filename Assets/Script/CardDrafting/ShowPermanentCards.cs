using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPermanentCards : MonoBehaviour
{
    public GameObject panel;
    public GameObject prevBtn;
    public GameObject nextBtn;

    public Image middle;
    public Image left;
    public Image right;

    public Text cardEffect;

    private int index = 0;
    private List<AbilityCard> cards;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OpenPanel(int playerIndex)
    {
        Time.timeScale = 0f;
        PlayerData owner = playerIndex == 0 ? FightManager.instance.Player : FightManager.instance.AI;

        panel.SetActive(true);
        cards = owner.Cards;
        index = 0;
        SetSprite();
    }

    public void ChangeCard(bool left)
    {
        int dir = left ? -1 : 1;
        index = Mathf.Clamp(index + dir, 0, cards.Count - 1);
        SetSprite();

    }

    public void SetSprite()
    {
        if (cards == null || cards.Count == 0)
        {
            middle.gameObject.SetActive(false);
            left.gameObject.SetActive(false);
            right.gameObject.SetActive(false);
            cardEffect.text = "";
            prevBtn.SetActive(false);
            nextBtn.SetActive(false);
            return;
        }

        middle.sprite = cards[index].cardSprite;
        middle.gameObject.SetActive(true);

        if (index <= 0)
        {
            left.gameObject.SetActive(false);
            prevBtn.SetActive(false);
        }
        else
        {
            left.sprite = cards[index - 1].cardSprite;
            left.gameObject.SetActive(true);
            prevBtn.SetActive(true);
        }

        if (index >= cards.Count - 1)
        {
            right.gameObject.SetActive(false);
            nextBtn.SetActive(false);
        }
        else
        {
            right.sprite = cards[index + 1].cardSprite;
            right.gameObject.SetActive(true);
            nextBtn.SetActive(true);
        }

        cardEffect.text = cards[index].cardEffect;
    }

    public void ClosePanel()
    {
        panel.SetActive(false);
        Time.timeScale = 1.0f;
    }
}
