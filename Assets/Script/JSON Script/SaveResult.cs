using UnityEngine;
using UnityEngine.UI;

public class SaveResult : MonoBehaviour
{
    public InputField inputName;

    public Text dataPreview;
    public Text Placeholder;

    public string placeholder;
    public Color placeholderColor;

    public void saveResult()
    {
        SaveManager.instance.SaveRecord(inputName.text,
            FightManager.instance.playTime,
            FightManager.instance.playerWin);
        FightManager.instance.ExitFight();
    }

    public void Preview(GameObject panel)
    {
        if(inputName.text == "")
        {
            EnterName();
            return;
        }
        panel.SetActive(true);
        string win = FightManager.instance.playerWin ? "Win" : "Lose"; 
        int minutes = Mathf.FloorToInt(FightManager.instance.playTime/ 60);
        int seconds = Mathf.FloorToInt(FightManager.instance.playTime % 60);
        int milliseconds = Mathf.FloorToInt((FightManager.instance.playTime * 100) % 100);

        string display =
            $"{minutes:00} : {seconds:00}.{milliseconds:00}";


        dataPreview.text = ": " + inputName.text + "\n: " +
            display + "\n: " +
            win;
    }

    public void EnterName()
    {
        Placeholder.text = "Name is Empty. Enter Your Name";
        Placeholder.color = Color.red;
    }

    public void PlaceholderReset()
    {
        if (Placeholder.text == placeholder) return;
        Placeholder.text = placeholder;
        Placeholder.color = placeholderColor;
    }

    public void ClosePanel(GameObject panel)
    {
        panel.SetActive(false);
    }

    public void OpenPanel(GameObject panel)
    {
        panel.SetActive(true);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
