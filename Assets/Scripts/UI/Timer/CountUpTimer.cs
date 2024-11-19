using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic;
using System.Linq;
using System.IO;

[DisallowMultipleComponent]
public class CountUpTimer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    
    private bool timerIsRunning = false;
    private float elapsedTime = 0f;
    private Dictionary<string, int> storeHighScore = new Dictionary<string, int>();

    /// <summary>
    /// Initializes the timer and starts it.
    /// </summary>
    private void Awake()
    {
        timerIsRunning = true;
    }

    /// <summary>
    /// Updates the timer each frame if it is running.
    /// </summary>
    private void Update()
    {
        if (timerIsRunning)
        {
            StartTimer();
            StopCountUpTimer();
        }
    }

    /// <summary>
    /// Starts the timer and updates the elapsed time.
    /// </summary>
    private void StartTimer()
    {
        elapsedTime += Time.deltaTime;
        DisplayTime(elapsedTime);
    }

    /// <summary>
    /// Stops the timer if the game is won or lost.
    /// </summary>
    private void StopCountUpTimer()
    {
        if (GetGameState() == GameState.gameWon || GetGameState() == GameState.gameLost)
        {
            timerIsRunning = false;
        }
    }

    /// <summary>
    /// Displays the elapsed time in minutes and seconds.
    /// </summary>
    /// <param name="timeToDisplay">The time to display.</param>
    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = "Total " + string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Gets the current game state from the GameManager.
    /// </summary>
    /// <returns>The current game state.</returns>
    private GameState GetGameState()
    {
        return GameManager.Instance.gameState;
    }

    /// <summary>
    /// Gets the finish time in seconds.
    /// </summary>
    /// <returns>The finish time in seconds.</returns>
    public int GetFinishTime()
    {
        return (int)elapsedTime;
    }

    /// <summary>
    /// Gets the finish time as a formatted string.
    /// </summary>
    /// <returns>The finish time as a formatted string.</returns>
    public string GetFinishTimeText()
    {
        string text = timerText.text;
        if (text.StartsWith("Total "))
        {
            text = text.Substring(6);
        }
        return text.ToUpper();
    }

    /// <summary>
    /// Updates the high scores with the player's name and time.
    /// </summary>
    /// <param name="playerName">The player's name.</param>
    /// <param name="time">The player's time.</param>
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

    /// <summary>
    /// Saves the high scores to a file.
    /// </summary>
    private void SaveHighScoresToFile()
    {
        string filePath = Application.persistentDataPath + "/highscores.json";
        string json = JsonUtility.ToJson(new Serialization<string, int>(storeHighScore));
        File.WriteAllText(filePath, json);
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
            storeHighScore = serialization.ToDictionary();
        }
    }

    [System.Serializable]
    public class Serialization<TKey, TValue>
    {
        public List<TKey> keys;
        public List<TValue> values;

        /// <summary>
        /// Initializes a new instance of the <see cref="Serialization{TKey, TValue}"/> class.
        /// </summary>
        /// <param name="dict">The dictionary to serialize.</param>
        public Serialization(Dictionary<TKey, TValue> dict)
        {
            keys = new List<TKey>(dict.Keys);
            values = new List<TValue>(dict.Values);
        }

        /// <summary>
        /// Converts the serialized data back into a dictionary.
        /// </summary>
        /// <returns>The deserialized dictionary.</returns>
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

