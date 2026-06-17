using System.Linq;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    public GameObject leaderboardPanel;
    public Transform content;

    public LeaderBoardRow rowPrefab;

    private void Start()
    {

    }

    public void Init()
    {
        leaderboardPanel.SetActive(true);
        ShowLeaderboard();
        leaderboardPanel.SetActive(false);
    }

    public void OpenAndClose(bool state)
    {
        leaderboardPanel.SetActive(state);
    }

    public void UpdateLeaderboard()
    {
        if(content.childCount > 0)
        {
            foreach(Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        ShowLeaderboard();
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
                data.records[i],
                this
            );
        }
    }

    public void DeleteRow(int rank)
    {
        var data = SaveManager.instance.LoadRecords();
        data.records = data.records
            .OrderByDescending (x => x.isWin)
            .ThenBy (x => x.playTime)
            .ToList();

        data.records.RemoveAt(rank);

        string json = JsonUtility.ToJson(data);

        SaveManager.instance.UpdateRecord(json);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
