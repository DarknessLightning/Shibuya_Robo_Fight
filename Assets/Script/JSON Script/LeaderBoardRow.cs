using UnityEngine;
using UnityEngine.UI;

public class LeaderBoardRow : MonoBehaviour
{
    public Text rankText;
    public Text nameText;
    public Text timeText;
    public Text resultText;

    public int rank;

    private LeaderBoard lb;

    public void Setup(
        int rank,
        PlayerRecord data, 
        LeaderBoard leaderboard)
    {
        lb = leaderboard;
        this.rank = rank;
        rankText.text = rank.ToString();

        nameText.text =
            data.name;


        int minutes = Mathf.FloorToInt(data.playTime / 60);
        int seconds = Mathf.FloorToInt(data.playTime % 60);
        int milliseconds = Mathf.FloorToInt((data.playTime * 100) % 100);

        string display =
            $"{minutes:00} : {seconds:00}.{milliseconds:00}";
        timeText.text =
            display;

        resultText.text =
            data.isWin
            ? "WIN"
            : "LOSE";
    }

    public void DeleteRow()
    {
        lb.DeleteRow(rank - 1);
        Destroy(gameObject);
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
