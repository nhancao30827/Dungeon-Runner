using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomSpawnableObject<T>
{
    private struct chanceBoundaries
    {
        public T spawnableObject;
        public int lowBoundaryValue;
        public int highBoundaryValue;
    }

    private int ratioValueTotal = 0;
    private List<chanceBoundaries> chanceBoundariesList = new List<chanceBoundaries>();
    private List<SpawnableObjectByLevel<T>> spawnableObjectByLevelList;

    /// <summary>
    /// Initializes a new instance of the <see cref="RandomSpawnableObject{T}"/> class with the specified list of spawnable objects by level.
    /// </summary>
    /// <param name="spawnableObjectByLevelList">The list of spawnable objects by level.</param>
    public RandomSpawnableObject(List<SpawnableObjectByLevel<T>> spawnableObjectByLevelList)
    {
        this.spawnableObjectByLevelList = spawnableObjectByLevelList;
    }

    /// <summary>
    /// Gets a random item based on the defined spawn ratios for the current dungeon level.
    /// </summary>
    /// <returns>A randomly selected item of type <typeparamref name="T"/>.</returns>
    public T GetItem()
    {
        InitializeChanceBoundaries();

        if (chanceBoundariesList.Count == 0) return default(T);

        int lookUpValue = Random.Range(0, ratioValueTotal);
        return GetSpawnableObjectByLookupValue(lookUpValue);
    }

    /// <summary>
    /// Initializes the chance boundaries list based on the current dungeon level.
    /// </summary>
    private void InitializeChanceBoundaries()
    {
        int upperBoundary = -1;
        ratioValueTotal = 0;
        chanceBoundariesList.Clear();

        DungeonLevelSO currentDungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        foreach (SpawnableObjectByLevel<T> spawnableObjectByLevel in spawnableObjectByLevelList)
        {
            if (spawnableObjectByLevel.dungeonLevel == currentDungeonLevel)
            {
                foreach (SpawnableObjectRatio<T> spawnableObjectRatio in spawnableObjectByLevel.spawnableObjectRatioList)
                {
                    int lowerBoundary = upperBoundary + 1;
                    upperBoundary = lowerBoundary + spawnableObjectRatio.ratio - 1;
                    ratioValueTotal += spawnableObjectRatio.ratio;

                    chanceBoundariesList.Add(new chanceBoundaries()
                    {
                        spawnableObject = spawnableObjectRatio.dungeonObject,
                        lowBoundaryValue = lowerBoundary,
                        highBoundaryValue = upperBoundary
                    });
                }
            }
        }
    }

    /// <summary>
    /// Gets the spawnable object based on the lookup value.
    /// </summary>
    /// <param name="lookUpValue">The lookup value.</param>
    /// <returns>The spawnable object of type <typeparamref name="T"/>.</returns>
    private T GetSpawnableObjectByLookupValue(int lookUpValue)
    {
        foreach (chanceBoundaries spawnChance in chanceBoundariesList)
        {
            if (lookUpValue >= spawnChance.lowBoundaryValue && lookUpValue <= spawnChance.highBoundaryValue)
            {
                return spawnChance.spawnableObject;
            }
        }
        return default(T);
    }
}
