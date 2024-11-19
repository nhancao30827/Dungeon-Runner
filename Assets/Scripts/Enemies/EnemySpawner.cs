using System.Collections;
using UnityEngine;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonobehaviour<EnemySpawner>
{
    private int enemiesToSpawn;
    private int currentEnemyCount;
    private int enemiesSpawedSoFar;
    private int enemyMaxConcurrentSpawnNumber;
    private Room currentRoom;
    private RoomEnemySpawnParameters roomEnemySpawnParameters;
    private Coroutine spawnEnemiesCoroutine; 

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }
    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.T))
        {
            //StopSpawningAndClearEnemies();
        }
    }

    /// <summary>
    /// Handles the event when the room changes.
    /// </summary>
    /// <param name="roomChangedEventArgs">The event arguments containing the new room information.</param>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        ResetEnemyCounts();

        currentRoom = roomChangedEventArgs.room;

        if (IsNonCombatRoom(currentRoom) || currentRoom.isClearedOfEnemies)
            return;

        InitializeRoomEnemyParameters();

        if (enemiesToSpawn == 0)
        {
            currentRoom.isClearedOfEnemies = true;
            return;
        }

        PrepareRoomForCombat();
    }

    /// <summary>
    /// Resets the enemy counts for the current room.
    /// </summary>
    private void ResetEnemyCounts()
    {
        enemiesSpawedSoFar = 0;
        currentEnemyCount = 0;
    }

    /// <summary>
    /// Determines if the specified room is a non-combat room.
    /// </summary>
    /// <param name="room">The room to check.</param>
    /// <returns>True if the room is a corridor or entrance; otherwise, false.</returns>
    private bool IsNonCombatRoom(Room room)
    {
        return room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS || room.roomNodeType.isEntrance;
    }

    /// <summary>
    /// Initializes the parameters for spawning enemies in the current room.
    /// </summary>
    private void InitializeRoomEnemyParameters()
    {
        DungeonLevelSO currentDungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();
        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(currentDungeonLevel);
        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(currentDungeonLevel);
    }

    /// <summary>
    /// Prepares the room for combat by locking doors and starting enemy spawn.
    /// </summary>
    private void PrepareRoomForCombat()
    {
        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();
        currentRoom.instantiatedRoom.LockDoors();
        SpawnEnemies();
    }

    /// <summary>
    /// Initiates the spawning of enemies in the current room.
    /// </summary>
    private void SpawnEnemies()
    {
        if (GameManager.Instance.gameState == GameState.playingLevel)
        {
            GameManager.Instance.previousGameState = GameState.playingLevel;
            GameManager.Instance.gameState = GameState.engagingEnemies;
        }

        spawnEnemiesCoroutine = StartCoroutine(SpawnEnemiesRoutine()); // Assign the coroutine to the variable
    }

    /// <summary>
    /// Coroutine to spawn enemies at intervals.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiatedRoom.grid;
        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        if (currentRoom.spawnPositionArray.Length > 0)
        {
            for (int i = 0; i < enemiesToSpawn; i++)
            {
                while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
                {
                    yield return null;
                }

                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];
                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));
                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

    /// <summary>
    /// Gets the interval between enemy spawns.
    /// </summary>
    /// <returns>A random interval between the minimum and maximum spawn intervals.</returns>
    private float GetEnemySpawnInterval()
    {
        return Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval);
    }

    /// <summary>
    /// Gets the number of concurrent enemies to spawn.
    /// </summary>
    /// <returns>A random number between the minimum and maximum concurrent enemies.</returns>
    private int GetConcurrentEnemies()
    {
        return Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies);
    }

    /// <summary>
    /// Creates an enemy at the specified position.
    /// </summary>
    /// <param name="enemyDetails">The details of the enemy to create.</param>
    /// <param name="position">The position to spawn the enemy at.</param>
    private void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        enemiesSpawedSoFar++;
        currentEnemyCount++;

        DungeonLevelSO currentDungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawedSoFar, currentDungeonLevel);

        enemy.GetComponent<DestroyEvent>().OnDestroy += Enemy_OnDestroy;
    }

    /// <summary>
    /// Handles the event when an enemy is destroyed.
    /// </summary>
    /// <param name="destroyEvent">The destroy event.</param>
    /// <param name="destroyEventArgs">The event arguments containing the destroy information.</param>
    private void Enemy_OnDestroy(DestroyEvent destroyEvent, DestroyEventArgs destroyEventArgs)
    {
        destroyEvent.OnDestroy -= Enemy_OnDestroy;

        currentEnemyCount--;

        if (currentEnemyCount == 0 && enemiesSpawedSoFar == enemiesToSpawn  /*&& currentRoom.roomNodeType.isBossRoom == false*/)
        {
            currentRoom.isClearedOfEnemies = true;
            Debug.Log("Room cleared");
            if (GameManager.Instance.gameState == GameState.engagingEnemies)
            {
                Debug.Log("Switch Stage");
                GameManager.Instance.gameState = GameState.playingLevel;
                GameManager.Instance.previousGameState = GameState.engagingEnemies;
            }

            currentRoom.instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);

            StaticEventHandler.CallEnemyRoomClearedEvent(currentRoom);
        }
    }



}
