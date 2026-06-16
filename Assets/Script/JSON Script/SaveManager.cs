using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class PlayerRecord
{
    public string name;
    public float playTime;
    public bool isWin;
    public winCondition condition;
}

[Serializable]
public class PlayerRecordList
{
    public List<PlayerRecord> records = new();
}

public class SaveManager : MonoBehaviour
{
    public static SaveManager instance;

    private string filePath;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
        filePath = Path.Combine(Application.persistentDataPath, "records.json");
        Debug.Log(filePath);
    }

    public PlayerRecordList LoadRecords()
    {
        if(!File.Exists(filePath))
        {
            return new PlayerRecordList();
        }

        string json = File.ReadAllText(filePath);
        PlayerRecordList list = JsonUtility.FromJson<PlayerRecordList>(json);

        foreach(PlayerRecord record in list.records ) 
        {
            if(record.condition == winCondition.None)
            {
                record.condition = winCondition.Kill;
            }
        }

        return list;
    }

    public void SaveRecord(string playerName, float playTime, bool playerWon, winCondition condition)
    {
        PlayerRecordList data = LoadRecords();

        PlayerRecord record = new PlayerRecord();

        record.name = playerName;
        record.playTime = playTime;
        record.isWin = playerWon;
        record.condition = condition;

        data.records.Add(record);

        string json = JsonUtility.ToJson(data, true);

        File.WriteAllText(filePath, json);
    }

    public void UpdateRecord(string json)
    {
        File.WriteAllText(filePath, json);
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
