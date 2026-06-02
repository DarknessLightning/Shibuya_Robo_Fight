using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public GameObject PrevBtn;
    public GameObject NextBtn;
    public Image panel;
    public Sprite[] cards;

    private int index = 0;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        panel.sprite = cards[0];
    }

    public void NextCard()
    {
        index = Mathf.Min(index + 1, cards.Length - 1);
        panel.sprite = cards[index];
        if(index == cards.Length - 1)
        {
            NextBtn.SetActive(false);
        }
        if(index > 0)
        {
            PrevBtn.SetActive(true);
        }
    }

    public void PrevCard()
    {
        index = Mathf.Max(index - 1, 0);
        panel.sprite = cards[index];
        if(index == 0)
        {
            PrevBtn.SetActive(false);
        }
        if (index < cards.Length - 1)
        {
            NextBtn.SetActive(true);
        }
    }

    public void ChangeScene(string sceneName)
    {
        if (sceneName == "")
        {
            Debug.Log("Play!");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }

    public void OpenPanel(GameObject panel = null)
    {
        if (panel == null)
        {
            Debug.Log("Panel Opened!");
            return;
        }
        panel.SetActive(true);
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void ExitGame()
    {
        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
