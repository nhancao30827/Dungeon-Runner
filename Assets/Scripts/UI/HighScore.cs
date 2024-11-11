using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using TMPro;
using UnityEngine;


public class HighScore : MonoBehaviour
{
    public TextMeshProUGUI highScoreText;

    // Dictionary now stores player names as keys and times as values
    private Dictionary<string, int> highScoresDict = new Dictionary<string, int>();

    private void Start()
    {
        LoadHighScoresFromFile();
        ShowHighScore();
    }

    /// <summary>
    /// Displays the high scores on the UI.
    /// </summary>
    private void ShowHighScore()
    {
        // Sort the dictionary by time (ascending order) to display the fastest times
        string leaderboardOutput = "Leaderboard:\n\n";
        int rank = 1;

        // Sort the dictionary by value (time)
        foreach (var entry in highScoresDict.OrderBy(kvp => kvp.Value))
        {
            int minutes = entry.Value / 60;
            int seconds = entry.Value % 60;
            leaderboardOutput += $"{rank}. {entry.Key} (Time: {minutes:00}:{seconds:00})\n\n";
            rank++;
        }

        if (highScoreText != null)
        {
            highScoreText.text = leaderboardOutput;
        }
        else
        {
            Debug.LogWarning("highScoreText is null. Cannot display high scores.");
        }
    }

    /// <summary>
    /// Loads the high scores from a file.
    /// </summary>
    private void LoadHighScoresFromFile()
    {
        string filePath = Application.persistentDataPath + "/highscores.json";
        if (File.Exists(filePath))
        {
            string json = File.ReadAllText(filePath);
            Serialization<string, int> serialization = JsonUtility.FromJson<Serialization<string, int>>(json);
            highScoresDict = serialization.ToDictionary();
        }
    }

    ///// <summary>
    ///// Saves the high scores to a file.
    ///// </summary>
    //private void SaveHighScoresToFile()
    //{
    //    string filePath = Application.persistentDataPath + "/highscores.json";
    //    string json = JsonUtility.ToJson(new Serialization<string, int>(highScoresDict));
    //    File.WriteAllText(filePath, json);
    //}

    ///// <summary>
    ///// Adds a new high score to the dictionary and saves it to the file.
    ///// </summary>
    ///// <param name="playerName">The name of the player.</param>
    ///// <param name="time">The time achieved by the player.</param>
    //public void AddHighScore(string playerName, int time)
    //{
    //    if (!highScoresDict.ContainsKey(playerName) || highScoresDict[playerName] > time)
    //    {
    //        highScoresDict[playerName] = time;
    //        SaveHighScoresToFile();
    //        ShowHighScore();
    //    }
    //}

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


