using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    [SerializeField] private MovementDetailsSO movementDetails;

    private Enemy enemy;
    private Stack<Vector3> movementSteps = new Stack<Vector3>();
    private Vector3 playerReferencePosition;
    private Coroutine moveEnemyRoutine;
    private float currentEnemyPathRebuildCooldown;
    private WaitForFixedUpdate waitForFixedUpdate;
    private bool chasePlayer = false;

    [HideInInspector] public int updateFrameNumber = 1;
    [HideInInspector] public float moveSpeed;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
    }

    private void Update()
    {
        MoveEnemy();
    }

    /// <summary>
    /// Moves the enemy towards the player if within chase distance and rebuilds the path if necessary.
    /// </summary>
    private void MoveEnemy()
    {
        float chaseDistance = enemy.enemyDetails.chaseDistance;
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        if (!chasePlayer && IsPlayerWithinChaseDistance(playerPosition, chaseDistance))
        {
            chasePlayer = true;
        }

        if (!chasePlayer) return;

        if (ShouldRebuildPath(playerPosition))
        {
            RebuildPath(playerPosition);
        }
    }

    private bool IsPlayerWithinChaseDistance(Vector3 playerPosition, float chaseDistance)
    {
        return Vector3.Distance(transform.position, playerPosition) < chaseDistance;
    }

    private bool ShouldRebuildPath(Vector3 playerPosition)
    {
        return Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver == updateFrameNumber &&
               (currentEnemyPathRebuildCooldown <= 0f ||
                Vector3.Distance(playerReferencePosition, playerPosition) > Settings.playerMoveDistanceToRebuildPath);
    }

    private void RebuildPath(Vector3 playerPosition)
    {
        currentEnemyPathRebuildCooldown = Settings.enemyPathRebulidCooldown;
        playerReferencePosition = playerPosition;

        CreatePath();

        if (movementSteps != null)
        {
            if (moveEnemyRoutine != null)
            {
                enemy.idleEvent.CallIdleEvent();
                StopCoroutine(moveEnemyRoutine);
            }

            moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
        }
    }

    /// <summary>
    /// Coroutine to move the enemy along the path defined by movement steps.
    /// </summary>
    /// <param name="movementSteps">Stack of positions to move to.</param>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition,
                    transform.position, moveSpeed,
                    (nextPosition - transform.position).normalized, false);
                yield return waitForFixedUpdate;
            }

            yield return waitForFixedUpdate;
        }

        enemy.idleEvent.CallIdleEvent();
    }

    /// <summary>
    /// Creates a path for the enemy to follow towards the player.
    /// </summary>
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
        Grid grid = currentRoom.instantiatedRoom.grid;

        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);
        Vector3Int playerGridPosition = GetPlayerGridPosition(currentRoom, playerPosition, grid);

        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            enemy.idleEvent.CallIdleEvent();
        }
    }

    private Vector3Int GetPlayerGridPosition(Room currentRoom, Vector3 playerPosition, Grid grid)
    {
        Vector3Int playerCellPosition = grid.WorldToCell(playerPosition);
        return GetNearestNonObstaclePlayerPosition(currentRoom, playerPosition, playerCellPosition);
    }
    

    /// <summary>
    /// Sets the frame number to update the enemy's pathfinding.
    /// </summary>
    /// <param name="updateFrameNumber">Frame number to update pathfinding.</param>
    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }

    /// <summary>
    /// Gets the nearest non-obstacle position to the player.
    /// </summary>
    /// <param name="currentRoom">Current room the enemy is in.</param>
    /// <param name="playerPosition">Current position of the player.</param>
    /// <param name="playerCellPosition">Grid cell position of the player.</param>
    /// <returns>Nearest non-obstacle grid cell position to the player.</returns>
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom, Vector3 playerPosition, Vector3Int playerCellPosition)
    {
        Vector2Int adjustedPlayerCellPosition = AdjustPlayerCellPosition(currentRoom, playerCellPosition);
        int obstacle = GetObstaclePenalty(currentRoom, adjustedPlayerCellPosition);

        if (obstacle == 0)
        {
            return FindNearestNonObstaclePosition(currentRoom, playerCellPosition, adjustedPlayerCellPosition);
        }

        return playerCellPosition;
    }

    /// <summary>
    /// Adjusts the player's cell position relative to the room's template lower bounds.
    /// </summary>
    /// <param name="currentRoom">The current room the enemy is in.</param>
    /// <param name="playerCellPosition">The grid cell position of the player.</param>
    /// <returns>The adjusted player cell position.</returns>
    private Vector2Int AdjustPlayerCellPosition(Room currentRoom, Vector3Int playerCellPosition)
    {
        return new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x,
                              playerCellPosition.y - currentRoom.templateLowerBounds.y);
    }

    /// <summary>
    /// Gets the movement penalty for the specified cell position.
    /// </summary>
    /// <param name="currentRoom">The current room the enemy is in.</param>
    /// <param name="adjustedPlayerCellPosition">The adjusted player cell position.</param>
    /// <returns>The movement penalty for the specified cell position.</returns>
    private int GetObstaclePenalty(Room currentRoom, Vector2Int adjustedPlayerCellPosition)
    {
        return currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];
    }

    /// <summary>
    /// Finds the nearest non-obstacle position to the player.
    /// </summary>
    /// <param name="currentRoom">The current room the enemy is in.</param>
    /// <param name="playerCellPosition">The grid cell position of the player.</param>
    /// <param name="adjustedPlayerCellPosition">The adjusted player cell position.</param>
    /// <returns>The nearest non-obstacle grid cell position to the player.</returns>
    private Vector3Int FindNearestNonObstaclePosition(Room currentRoom, Vector3Int playerCellPosition, Vector2Int adjustedPlayerCellPosition)
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0) continue;

                if (IsNonObstaclePosition(currentRoom, adjustedPlayerCellPosition, i, j))
                {
                    return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0);
                }
            }
        }

        return playerCellPosition;
    }

    /// <summary>
    /// Checks if the specified position is a non-obstacle position.
    /// </summary>
    /// <param name="currentRoom">The current room the enemy is in.</param>
    /// <param name="adjustedPlayerCellPosition">The adjusted player cell position.</param>
    /// <param name="offsetX">The x offset to check.</param>
    /// <param name="offsetY">The y offset to check.</param>
    /// <returns>True if the position is a non-obstacle position, otherwise false.</returns>
    private bool IsNonObstaclePosition(Room currentRoom, Vector2Int adjustedPlayerCellPosition, int offsetX, int offsetY)
    {
        try
        {
            int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + offsetX, adjustedPlayerCellPosition.y + offsetY];
            return obstacle != 0;
        }
        catch
        {
            return false;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
