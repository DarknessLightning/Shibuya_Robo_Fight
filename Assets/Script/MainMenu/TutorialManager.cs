using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    public GameObject tutorialPanel;
    public GameObject PrevBtn;
    public GameObject NextBtn;
    public Image panel;
    public Text title;
    public Sprite[] cards;
    public string[] cardsName;

    private int index;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panel.sprite = cards[0];
    }

    public void NextCard()
    {
        index = Mathf.Min(index + 1, cards.Length - 1);
        panel.sprite = cards[index];
        title.text = cardsName[index];
        if (index == cards.Length - 1)
        {
            NextBtn.SetActive(false);
        }
        if (index > 0)
        {
            PrevBtn.SetActive(true);
        }
    }

    public void PrevCard()
    {
        index = Mathf.Max(index - 1, 0);
        panel.sprite = cards[index];
        title.text = cardsName[index];
        if (index == 0)
        {
            PrevBtn.SetActive(false);
        }
        if (index < cards.Length - 1)
        {
            NextBtn.SetActive(true);
        }
    }

    public void OpenPanel()
    {
        tutorialPanel.SetActive(true);
        panel.sprite = cards[0];
        title.text = cardsName[0];
        PrevBtn.SetActive(false);
        NextBtn.SetActive(true);
        index = 0;
    }

    public void ClosePanel()
    {
        tutorialPanel.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
