using UnityEngine;
using UnityEngine.UI;  
using TMPro;  

public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Text component to display the timer.")]
    public TextMeshProUGUI timerText;

    [Tooltip("Indicates whether the timer is running.")]
    public bool timerIsRunning = false;

    private float startTime;
    private float remainingTime;

    private void Awake()
    {
        SetStartTime();
    }

    private void Start()
    {
        StartTimer();
    }

    private void Update()
    {
        if (timerIsRunning)
        {
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
    }

    /// <summary>
    /// Starts the timer.
    /// </summary>
    private void StartTimer()
    {
        remainingTime = startTime;
        timerIsRunning = true;
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
    public void SetStartTime()
    {
        startTime = GameManager.Instance.GetCurrentDungeonLevel().timeLimit;
    }

    /// <summary>
    /// Displays the remaining time in the timer text component.
    /// </summary>
    /// <param name="timeToDisplay">Time to display.</param>
    private void DisplayTime(float timeToDisplay)
    {
        int minutes = Mathf.FloorToInt(timeToDisplay / 60);
        int seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.color = timeToDisplay <= 10 ? Color.red : Color.white;
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    /// <summary>
    /// Resets the timer to the start time and updates the display.
    /// </summary>
    public void ResetTimer()
    {
        SetStartTime();
        remainingTime = startTime;
        timerIsRunning = true;
        DisplayTime(remainingTime); // Update the display immediately
        Debug.Log($"Timer reset: startTime={startTime}, remainingTime={remainingTime}");
    }
}
