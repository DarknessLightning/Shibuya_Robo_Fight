using System.Linq;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public Transform content;

    public LeaderBoardRow rowPrefab;

    private void Start()
    {
        ShowLeaderboard();
        leaderboardPanel.SetActive(false);
    }

    public void ShowLeaderboard()
    {
        var data =
            SaveManager.instance.LoadRecords();

        data.records =
            data.records
            .OrderByDescending(x => x.isWin)
            .ThenBy(x => x.playTime)
            .ToList();

        for (int i = 0; i < data.records.Count; i++)
        {
            LeaderBoardRow row =
                Instantiate(
                    rowPrefab,
                    content
                );

            row.Setup(
                i + 1,
                data.records[i]
            );
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
