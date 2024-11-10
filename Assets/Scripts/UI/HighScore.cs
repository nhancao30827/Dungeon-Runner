using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;


public class HighScore : SingletonMonobehaviour<HighScore>
{
    public TextMeshProUGUI highScoreText;

    // Dictionary now stores player names as keys and times as values
    private Dictionary<string, int> highScoresDict = new Dictionary<string, int>();

    protected override void Awake()
    {
        base.Awake();
        Debug.Log(highScoresDict.Count);
        LoadHighScores();
        UpdateHighScores("Hien tai", 120); // Load high scores from PlayerPrefs when the game starts
        ShowHighScore();
    }

    private void ShowHighScore()
    {
        // Sort the dictionary by time (ascending order) to display the fastest times
        string leaderboardOutput = "Leaderboard:\n";
        int rank = 1;

        // Sort the dictionary by value (time)
        foreach (var entry in highScoresDict.OrderBy(kvp => kvp.Value))
        {
            leaderboardOutput += $"{rank}. {entry.Key} (Time: {entry.Value}s)\n";
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

    public void UpdateHighScores(string playerName, int time)
    {
        Debug.Log("Updating high scores");

        // If there are already 10 scores, remove the worst score (the highest time)
        if (highScoresDict.Count >= 10)
        {
            var maxKey = highScoresDict.OrderByDescending(kvp => kvp.Value).FirstOrDefault().Key;
            highScoresDict.Remove(maxKey);
        }

        // Add or update the player's score
        highScoresDict[playerName] = time;

        // Save high scores after updating
        SaveHighScores();
    }

    private void SaveHighScores()
    {
        int index = 0;
        foreach (var entry in highScoresDict)
        {
            PlayerPrefs.SetString("HighScore_Player_" + index, entry.Key);   // Player name
            PlayerPrefs.SetInt("HighScore_Time_" + index, entry.Value);       // Time
            index++;
        }
        PlayerPrefs.SetInt("HighScore_Count", highScoresDict.Count);
        PlayerPrefs.Save();
    }

    public void LoadHighScores()
    {
        highScoresDict.Clear();
        int count = PlayerPrefs.GetInt("HighScore_Count", 0);

        for (int i = 0; i < count; i++)
        {
            string playerName = PlayerPrefs.GetString("HighScore_Player_" + i);
            int time = PlayerPrefs.GetInt("HighScore_Time_" + i);
            highScoresDict[playerName] = time;
        }

        ShowHighScore();
    }
}


