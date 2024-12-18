﻿using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CreateAssetMenu(fileName = "Room_", menuName = "Scriptable Objects/Dungeon/Room")]
public class RoomTemplateSO : ScriptableObject
{
    [HideInInspector] public string guid;

    #region Header ROOM PREFAB

    [Space(10)]
    [Header("ROOM PREFAB")]

    #endregion Header ROOM PREFAB

    #region Tooltip

    [Tooltip("The gameobject prefab for the room (this will contain all the tilemaps for the room and environment game objects")]

    #endregion Tooltip

    public GameObject prefab;

    [HideInInspector] public GameObject previousPrefab; // this is used to regenerate the guid if the so is copied and the prefab is changed


    #region Header ROOM CONFIGURATION

    [Space(10)]
    [Header("ROOM CONFIGURATION")]

    #endregion Header ROOM CONFIGURATION

    #region Tooltip

    [Tooltip("The room node type SO. The room node types correspond to the room nodes used in the room node graph.  The exceptions being with corridors.  In the room node graph there is just one corridor type 'Corridor'.  For the room templates there are 2 corridor node types - CorridorNS and CorridorEW.")]

    #endregion Tooltip

    public RoomNodeTypeSO roomNodeType;

    #region Tooltip

    [Tooltip("If you imagine a rectangle around the room tilemap that just completely encloses it, the room lower bounds represent the bottom left corner of that rectangle. This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that bottom left corner (Note: this is the local tilemap position and NOT world position")]

    #endregion Tooltip

    public Vector2Int lowerBounds;

    #region Tooltip

    [Tooltip("If you imagine a rectangle around the room tilemap that just completely encloses it, the room upper bounds represent the top right corner of that rectangle. This should be determined from the tilemap for the room (using the coordinate brush pointer to get the tilemap grid position for that top right corner (Note: this is the local tilemap position and NOT world position")]

    #endregion Tooltip

    public Vector2Int upperBounds;

    #region Tooltip

    [Tooltip("There should be a maximum of four doorways for a room - one for each compass direction.  These should have a consistent 3 tile opening size, with the middle tile position being the doorway coordinate 'position'")]

    #endregion Tooltip

    [SerializeField] public List<Doorway> doorwayList;

    #region Header ENEMY DETAILS

    [Space(10)]
    [Header("ENEMY DETAILS")]

    #endregion Header ENEMY DETAILS

    #region Tooltip

    [Tooltip("Each possible spawn position (used for enemies and chests) for the room in tilemap coordinates should be added to this array")]

    #endregion Tooltip

    public Vector2Int[] spawnPositionArray;

    #region Tooltip

    [Tooltip("List of enemies by level for this room.")]

    #endregion Tooltip

    public List<SpawnableObjectByLevel<EnemyDetailsSO>> enemiesByLevelList;

    #region Tooltip

    [Tooltip("Parameters for enemy spawning in this room.")]

    #endregion Tooltip

    public List<RoomEnemySpawnParameters> roomEnemySpawnParametersList;


    /// <summary>
    /// Returns the list of Entrances for the room template
    /// </summary>
    public List<Doorway> GetDoorwayList()
    {
        return doorwayList;
    }

    #region Validation
#if UNITY_EDITOR


    /// <summary>
    /// Validates the room template fields and sets a unique GUID if necessary.
    /// </summary>
    private void OnValidate()
    {
        if (SetUniqueGuidIfNecessary())
        {
            ValidateRoomTemplateFields();
            ValidateEnemySpawnParameters();
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(spawnPositionArray), spawnPositionArray);
        }
    }

    /// <summary>
    /// Sets a unique GUID if necessary and returns true if the GUID was set.
    /// </summary>
    /// <returns>True if the GUID was set, otherwise false.</returns>
    private bool SetUniqueGuidIfNecessary()
    {
        bool guidSet = false;

        if (string.IsNullOrEmpty(guid) || previousPrefab != prefab)
        {
            guid = GUID.Generate().ToString();
            previousPrefab = prefab;
            EditorUtility.SetDirty(this);
            guidSet = true;
        }

        return guidSet;
    }

    private void ValidateRoomTemplateFields()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(prefab), prefab);
        HelperUtilities.ValidateCheckNullValue(this, nameof(roomNodeType), roomNodeType);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(doorwayList), doorwayList);
    }

    private void ValidateEnemySpawnParameters()
    {
        if (enemiesByLevelList.Count > 0 || roomEnemySpawnParametersList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemiesByLevelList), enemiesByLevelList);
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(roomEnemySpawnParametersList), roomEnemySpawnParametersList);

            foreach (RoomEnemySpawnParameters roomEnemySpawnParameters in roomEnemySpawnParametersList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(roomEnemySpawnParameters.dungeonLevel), roomEnemySpawnParameters.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minTotalEnemiesToSpawn), roomEnemySpawnParameters.minTotalEnemiesToSpawn,
                    nameof(roomEnemySpawnParameters.maxTotalEnemiesToSpawn), roomEnemySpawnParameters.maxTotalEnemiesToSpawn, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minSpawnInterval), roomEnemySpawnParameters.minSpawnInterval,
                    nameof(roomEnemySpawnParameters.maxSpawnInterval), roomEnemySpawnParameters.maxSpawnInterval, true);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(roomEnemySpawnParameters.minConcurrentEnemies), roomEnemySpawnParameters.minConcurrentEnemies,
                    nameof(roomEnemySpawnParameters.maxConcurrentEnemies), roomEnemySpawnParameters.minConcurrentEnemies, false);

                ValidateEnemyTypesForDungeonLevel(roomEnemySpawnParameters);
            }
        }
    }

    private void ValidateEnemyTypesForDungeonLevel(RoomEnemySpawnParameters roomEnemySpawnParameters)
    {
        bool isEnemyTypesListForDungeonLevel = false;

        foreach (SpawnableObjectByLevel<EnemyDetailsSO> dungeonObjectByLevel in enemiesByLevelList)
        {
            if (dungeonObjectByLevel.dungeonLevel == roomEnemySpawnParameters.dungeonLevel &&
                dungeonObjectByLevel.spawnableObjectRatioList.Count > 0)
            {
                isEnemyTypesListForDungeonLevel = true;
            }

            HelperUtilities.ValidateCheckNullValue(this, nameof(dungeonObjectByLevel.dungeonLevel), dungeonObjectByLevel.dungeonLevel);

            foreach (SpawnableObjectRatio<EnemyDetailsSO> dungeonObjectRatio in dungeonObjectByLevel.spawnableObjectRatioList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(dungeonObjectRatio.dungeonObject), dungeonObjectRatio.dungeonObject);
                HelperUtilities.ValidateCheckPositiveValue(this, nameof(dungeonObjectRatio.ratio), dungeonObjectRatio.ratio, false);
            }
        }

        if (!isEnemyTypesListForDungeonLevel && roomEnemySpawnParameters.dungeonLevel != null)
        {
            Debug.Log(isEnemyTypesListForDungeonLevel);
            Debug.Log("No enemy types specified for dungeon level " +
                roomEnemySpawnParameters.dungeonLevel.levelName + " in gameobject " + this.name);
        }
    }

#endif

    #endregion Validation
}