using UnityEngine;
[System.Serializable]
public class RoomEnemySpawnParameters
{
    #region Tooltip
    [Tooltip("The dungeon level for which these enemy spawn parameters apply")]
    #endregion
    public DungeonLevelSO dungeonLevel;

    #region Tooltip
    [Tooltip("The minimum total number of enemies to spawn in the room")]
    #endregion
    public int minTotalEnemiesToSpawn;

    #region Tooltip
    [Tooltip("The maximum total number of enemies to spawn in the room")]
    #endregion
    public int maxTotalEnemiesToSpawn;

    #region Tooltip
    [Tooltip("The minimum number of enemies that can be present in the room at the same time")]
    #endregion
    public int minConcurrentEnemies;

    #region Tooltip
    [Tooltip("The maximum number of enemies that can be present in the room at the same time")]
    #endregion
    public int maxConcurrentEnemies;

    #region Tooltip
    [Tooltip("The minimum interval (in seconds) between enemy spawns")]
    #endregion
    public int minSpawnInterval;

    #region Tooltip
    [Tooltip("The maximum interval (in seconds) between enemy spawns")]
    #endregion
    public int maxSpawnInterval;
    
}
