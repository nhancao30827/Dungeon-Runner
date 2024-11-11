using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[DisallowMultipleComponent]
public class CountUpTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public bool timerIsRunning = false;

    private float elapsedTime = 0f;
    private Dictionary<string, int> storeHighScore = new Dictionary<string, int>();

    private void Awake()
    {
        timerIsRunning = true;
        //LoadHighScoresFromFile();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
            StartTimer();
            StopCountUpTimer();
        }
    }

    private void StartTimer()
    {
        elapsedTime += Time.deltaTime;
        DisplayTime(elapsedTime);
    }

    private void StopCountUpTimer()
    {
        if (GetGameState() == GameState.gameWon || GetGameState() == GameState.gameLost)
        {
            timerIsRunning = false;
        }
    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = "Total " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    private GameState GetGameState()
    {
        return GameManager.Instance.gameState;
    }

    public int GetFinishTime()
    {
        return (int)elapsedTime;
    }

    public string GetFinishTimeText()
    {
        string text = timerText.text;
        if (text.StartsWith("Total "))
        {
            text = text.Substring(6);
        }
        return text.ToUpper();
    }

    public void UpdateHighScores(string playerName, int time)
    {
        Debug.Log("Updating high scores");

        // Load existing high scores from file
        LoadHighScoresFromFile();

        if (storeHighScore.Count >= 10)
        {
            var maxKey = storeHighScore.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
            storeHighScore.Remove(maxKey);
        }

        storeHighScore[playerName] = time;
        SaveHighScoresToFile();
    }

    private void SaveHighScoresToFile()
    {
        string filePath = Application.persistentDataPath + "/highscores.json";
        string json = JsonUtility.ToJson(new Serialization<string, int>(storeHighScore));
        File.WriteAllText(filePath, json);
    }

    private void LoadHighScoresFromFile()
    {
        string filePath = Application.persistentDataPath + "/highscores.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Serialization<string, int> serialization = JsonUtility.FromJson<Serialization<string, int>>(json);
            storeHighScore = serialization.ToDictionary();
        }
    }

    [System.Serializable]
    public class Serialization<TKey, TValue>
    {
        public List<TKey> keys;
        public List<TValue> values;

        public Serialization(Dictionary<TKey, TValue> dict)
        {
            keys = new List<TKey>(dict.Keys);
            values = new List<TValue>(dict.Values);
        }

        public Dictionary<TKey, TValue> ToDictionary()
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();
            for (int i = 0; i < keys.Count; i++)
            {
                dict[keys[i]] = values[i];
            }
            return dict;
        }
    }
}

//[System.Serializable]
//public class Serialization<TKey, TValue>
//{
//    public List<TKey> keys;
//    public List<TValue> values;

//    public Serialization(Dictionary<TKey, TValue> dict)
//    {
//        keys = new List<TKey>(dict.Keys);
//        values = new List<TValue>(dict.Values);
//    }
//}
