using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class CountUpTimer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public bool timerIsRunning = false;
    //public TimerEvent timerEvent;

    private float elapsedTime = 0f;

    private void Awake()
    {
        timerIsRunning = true;
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
        if (GetGameSate() == GameState.gameWon || GetGameSate() == GameState.gameLost)
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

    private GameState GetGameSate()
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



}
