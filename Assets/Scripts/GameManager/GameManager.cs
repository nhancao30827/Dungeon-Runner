using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header UI ELEMENTS
    [Space(10)]
    [Header("UI ELEMENTS")]
    #endregion
    #region Tooltip
    [Tooltip("Text component for displaying game messages")]
    #endregion
    public TextMeshProUGUI gameMessageText;

    #region Tooltip
    [Tooltip("Canvas group for handling UI fade effects")]
    #endregion
    [SerializeField] private CanvasGroup canvasGroup;

    #region Tooltip
    [Tooltip("Text component for displaying the player's name")]
    #endregion
    [SerializeField] private TextMeshProUGUI playerName;

    #region Tooltip
    [Tooltip("Reference to the pause menu")]
    #endregion
    [SerializeField] private PauseMenu pauseMenu;

    #region Tooltip
    [Tooltip("Reference to the count-up timer")]
    #endregion
    [SerializeField] private CountUpTimer countUpTimer;

    #region Tooltip
    [Tooltip("Reference to the countdown timer")]
    #endregion
    [SerializeField] private CountdownTimer countdownTimer;


    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion
    [SerializeField] private List<DungeonLevelSO> dungeonLevelList;

    #region Tooltip
    [Tooltip("Populate with starting the dungeon level for testing, first level = 0")]
    #endregion
    [SerializeField] private int currentDungeonLevelListIndex;

    //#region Header Tooltip
    //[Tooltip("Populated with pause menu")]
    //#endregion

    private Room currentRoom;
    private Room previousRoom;
    private PlayerDetailsSO playerDetails;
    private Player player;
    private InstantiatedRoom bossRoom;


    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    [HideInInspector] public int CurrentDungeonLevelListIndex => currentDungeonLevelListIndex;
    [HideInInspector] public List<DungeonLevelSO> DungeonLevelList => dungeonLevelList;

    protected override void Awake()
    {
        base.Awake();

        Debug.Log(gameMessageText + " " + canvasGroup);

        playerDetails = GameResources.Instance.currentPlayerSO.playerDetails;

        InstantiatePlayer();
    }

    private void InstantiatePlayer()
    {
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);

        SetPlayerName();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnEnemyRoomCleared += StaticEventHandler_OnEnemyRoomCleared;
        player.destroyEvent.OnDestroy += Player_OnDestroy;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnEnemyRoomCleared -= StaticEventHandler_OnEnemyRoomCleared;
        player.destroyEvent.OnDestroy -= Player_OnDestroy;
    }

    private void StaticEventHandler_OnEnemyRoomCleared(EnemyRoomClearedEventArgs enemyRoomClearedEventArgs)
    {
        LevelComplete();
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
    }

    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;
        //SetPlayerName();
    }

    private void Update()
    {
        HandleGameState();

        if (Input.GetKeyDown(KeyCode.R))
        {
            RestartGame();
        }
    }

    /// <summary>
    /// Handles the completion of the current level.
    /// </summary>
    private void LevelComplete()
    {
        bossRoom = null;

        foreach (var KeyPair in DungeonBuilder.Instance.roomDictionary)
        {
            if (KeyPair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = KeyPair.Value.instantiatedRoom;
                Debug.Log("Boss Room Found");
                break;
            }
        }

        if (bossRoom == null || bossRoom.room.isClearedOfEnemies == true)
        {
            if (currentDungeonLevelListIndex < dungeonLevelList.Count - 1)
            {
                Debug.Log("Level Win");
                gameState = GameState.levelCompleted;
            }
            else
            {
                gameState = GameState.gameWon;
            }
        }
    }

    /// <summary>
    /// Handles the event when the player is destroyed.
    /// </summary>
    /// <param name="destroyEvent">The destroy event.</param>
    /// <param name="destroyEventArgs">The destroy event arguments.</param>
    private void Player_OnDestroy(DestroyEvent destroyEvent, DestroyEventArgs destroyEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
        Debug.Log("Player Destroyed");
    }

    /// <summary>
    /// Handles the game state transitions.
    /// </summary>
    private void HandleGameState()
    {
        switch (gameState)
        {
            case GameState.gameStarted:
                GetPlayer().SetPlayerHealth();
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                break;

            case GameState.playingLevel:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
                break;

            case GameState.engagingEnemies:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
                break;

            case GameState.levelCompleted:
                StartCoroutine(ToNextLevel());
                break;

            case GameState.gameWon:
                if (previousGameState != GameState.gameWon)
                {
                    StartCoroutine(GameWon());
                }
                break;

            case GameState.gameLost:
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines();
                    StartCoroutine(GameLost());
                }
                break;

            case GameState.restartGame:
                RestartGame();
                break;

            case GameState.gamePaused:
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    PauseGame();
                }
                break;
        }
    }

    /// <summary>
    /// Plays the specified dungeon level.
    /// </summary>
    /// <param name="dungeonLeveListIndex">Index of the dungeon level list.</param>
    private void PlayDungeonLevel(int dungeonLeveListIndex)
    {
        bool dungeonBuiltSuccessful = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLeveListIndex]);

        if (!dungeonBuiltSuccessful)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        Vector3 Playerposition = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(Playerposition);

        StartCoroutine(DisplayLevelText());
    }

    /// <summary>
    /// Coroutine to transition to the next level.
    /// </summary>
    private IEnumerator ToNextLevel()
    {
        gameState = GameState.playingLevel;
        yield return new WaitForSeconds(2f);

        Debug.Log("Press Enter to the next level");

        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        yield return StartCoroutine(DisplayMessageText("You have survived. \nPress Enter to proceed to the next level.", Color.white, 2f));

        // Wait for the player to press the Enter key
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        // Proceed to the next level
        currentDungeonLevelListIndex++;
        PlayDungeonLevel(currentDungeonLevelListIndex);
        countdownTimer.ResetTimer();
    }

    /// <summary>
    /// Coroutine to handle the game won state.
    /// </summary>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        countUpTimer.UpdateHighScores(GameResources.Instance.currentPlayerSO.playerName, countUpTimer.GetFinishTime());

        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        yield return StartCoroutine(DisplayMessageText("WELL DONE " + GameResources.Instance.currentPlayerSO.playerName + "! YOU HAVE SURVIVED", Color.white, 2.5f));

        yield return StartCoroutine(DisplayMessageText($"YOUR FINISH TIME: {countUpTimer.GetFinishTimeText()}", Color.white, 2.5f));

        yield return StartCoroutine(DisplayMessageText("PRESS ENTER TO RESTART THE GAME", Color.white, 0f));

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Coroutine to handle the game lost state.
    /// </summary>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        Enemy[] enemyArray = GameObject.FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        string lostText = "NICE TRY " + GameResources.Instance.currentPlayerSO.playerName + "\n BUT YOU LOST!";
        yield return StartCoroutine(DisplayMessageText(lostText, Color.white, 2.5f));

        yield return StartCoroutine(DisplayMessageText("PRESS ENTER TO RESTART GAME", Color.white, 0f));

        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Coroutine to handle the fade effect.
    /// </summary>
    /// <param name="starAlpha">The starting alpha value.</param>
    /// <param name="targetAlpha">The target alpha value.</param>
    /// <param name="fadeSecond">The duration of the fade in seconds.</param>
    /// <param name="backgroundColor">The background color.</param>
    private IEnumerator Fade(float starAlpha, float targetAlpha, float fadeSecond, Color backgroundColor)
    {
        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;

        while (time <= fadeSecond)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(starAlpha, targetAlpha, time / fadeSecond);
            yield return null;
        }
    }

    /// <summary>
    /// Coroutine to display the level text.
    /// </summary>
    private IEnumerator DisplayLevelText()
    {
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        string message = "Level " + (currentDungeonLevelListIndex + 1) + "\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName;

        StartCoroutine(DisplayMessageText(message, Color.white, 3f));

        yield return StartCoroutine(Fade(1f, 0f, 3f, Color.black));
    }

    /// <summary>
    /// Coroutine to display a message text.
    /// </summary>
    /// <param name="message">The message to display.</param>
    /// <param name="color">The color of the message text.</param>
    /// <param name="displayTime">The duration to display the message.</param>
    private IEnumerator DisplayMessageText(string message, Color color, float displayTime)
    {
        gameMessageText.SetText(message);
        gameMessageText.color = color;

        if (displayTime > 0)
        {
            float timeRemaining = displayTime;

            while (timeRemaining > 0 && !Input.GetKeyDown(KeyCode.Return))
            {
                timeRemaining -= Time.deltaTime;
                yield return null;
            }
        }
        else
        {
            while (!Input.GetKeyDown(KeyCode.Return))
            {
                yield return null;
            }
        }

        gameMessageText.SetText("");
    }

    /// <summary>
    /// Pauses or resumes the game.
    /// </summary>
    public void PauseGame()
    {
        if (gameState != GameState.gamePaused)
        {
            player.playerControl.DisablePlayer();
            pauseMenu.TogglePauseMenu();

            previousGameState = gameState;
            gameState = GameState.gamePaused;
        }
        else if (gameState == GameState.gamePaused)
        {
            player.playerControl.EnablePlayer();
            pauseMenu.TogglePauseMenu();

            gameState = previousGameState;
        }
    }

    /// <summary>
    /// Restarts the game by reloading the current scene.
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    /// <summary>
    /// Sets the player's name in the UI.
    /// </summary>
    private void SetPlayerName()
    {
        playerName.text = GameResources.Instance.currentPlayerSO.playerName;
    }

    /// <summary>
    /// Gets the current room.
    /// </summary>
    /// <returns>The current room.</returns>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Sets the current room.
    /// </summary>
    /// <param name="room">The room to set as the current room.</param>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    /// <summary>
    /// Gets the player.
    /// </summary>
    /// <returns>The player.</returns>
    public Player GetPlayer()
    {
        return player;
    }

    /// <summary>
    /// Gets the current dungeon level.
    /// </summary>
    /// <returns>The current dungeon level.</returns>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }
    
    

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion
}
