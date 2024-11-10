using UnityEngine;
using UnityEngine.UI;  
using TMPro;  

public class CountdownTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public bool timerIsRunning = false;

    private float startTime;
    private float remainingTime;

    private void Awake()
    {
        SetStartTime();
    }

    private void Start()
    {
        remainingTime = startTime;
        timerIsRunning = true;
    }

    private void Update()
    {
        if (!timerIsRunning) return;

        UpdateRemainingTime();
        if (remainingTime > 0)
        {
            DisplayTime(remainingTime);
        }
        else
        {
            HandleTimerEnd();
        }
    }

    /// <summary>
    /// Updates the remaining time by subtracting the elapsed time.
    /// </summary>
    private void UpdateRemainingTime()
    {
        remainingTime -= Time.deltaTime;
        remainingTime = Mathf.Clamp(remainingTime, 0, startTime);
    }

    /// <summary>
    /// Handles the actions to be taken when the timer ends.
    /// </summary>
    private void HandleTimerEnd()
    {
        timerIsRunning = false;
        GameManager.Instance.gameState = GameState.gameLost;
    }


    

    /// <summary>
    /// Sets the start time based on the current dungeon level index.
    /// </summary>
    private void SetStartTime()
    {
        startTime = GameManager.Instance.GetCurrentDungeonLevel().timeLimit;

    }

    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        if (timeToDisplay <= 10)
        {
            timerText.color = Color.red;  
        }
        else
        {
            timerText.color = Color.white;  
        }

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void ResetTimer()
    {
        remainingTime = startTime;
        timerIsRunning = true;
        DisplayTime(remainingTime); // Update the display immediately
    }
}
