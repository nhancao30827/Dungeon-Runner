using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DungeonBuilder : SingletonMonobehaviour<DungeonBuilder>
{
    public Dictionary<string, Room> roomDictionary = new Dictionary<string, Room>();
    private Dictionary<string, RoomTemplateSO> roomTemplateDictionary = new Dictionary<string, RoomTemplateSO>();
    private List<RoomTemplateSO> roomTemplateList = null;
    private RoomNodeTypeListSO roomNodeTypeList;
    private bool dungeonBuildSuccessful;

    private void OnEnable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 0f);
    }

    private void OnDisable()
    {
        GameResources.Instance.dimmedMaterial.SetFloat("Alpha_Slider", 1f);
    }

    protected override void Awake()
    {
        base.Awake();

        LoadRoomNodeTypeList();

    }

    /// <summary>
    /// Loads the room node type list from the game resources.
    /// </summary>
    private void LoadRoomNodeTypeList()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Generates a dungeon based on the provided dungeon level.
    /// </summary>
    /// <param name="currentDungeonLevel">The current dungeon level.</param>
    /// <returns>True if the dungeon was successfully generated, otherwise false.</returns>
    public bool GenerateDungeon(DungeonLevelSO currentDungeonLevel)
    {
        roomTemplateList = currentDungeonLevel.roomTemplateList;
        LoadRoomTemplatesIntoDictionary();

        dungeonBuildSuccessful = false;
        int dungeonBuildAttempts = 0;

        while (!dungeonBuildSuccessful && dungeonBuildAttempts < Settings.maxDungeonBuildAttempts)
        {
            dungeonBuildAttempts++;
            RoomNodeGraphSO roomNodeGraph = SelectRandomRoomNodeGraph(currentDungeonLevel.roomNodeGraphList);

            if (TryBuildDungeon(roomNodeGraph))
            {
                InstantiateRoomGameobjects();
                return true;
            }
        }
        Debug.Log("Failed to build dungeon");
        return false;
    }

    /// <summary>
    /// Attempts to build the dungeon using the specified room node graph.
    /// </summary>
    /// <param name="roomNodeGraph">The room node graph to use for building the dungeon.</param>
    /// <returns>True if the dungeon was successfully built, otherwise false.</returns>
    private bool TryBuildDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        int dungeonRebuildAttemptsForNodeGraph = 0;

        while (dungeonRebuildAttemptsForNodeGraph <= Settings.maxDungeonRebuildAttemptsForRoomGraph)
        {
            ClearDungeon();
            dungeonRebuildAttemptsForNodeGraph++;

            if (AttemptToBuildRandomDungeon(roomNodeGraph))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Selects a random room node graph from the provided list.
    /// </summary>
    /// <param name="roomNodeGraphList">The list of room node graphs.</param>
    /// <returns>A randomly selected room node graph.</returns>
    private RoomNodeGraphSO SelectRandomRoomNodeGraph(List<RoomNodeGraphSO> roomNodeGraphList)
    {
        if (roomNodeGraphList.Count > 0)
        {
            return roomNodeGraphList[UnityEngine.Random.Range(0, roomNodeGraphList.Count)];
        }
        else
        {
            Debug.Log("No Room Node Graphs");
            return null;
        }
    }

    /// <summary>
    /// Loads room templates into the dictionary from list.
    /// </summary>
    private void LoadRoomTemplatesIntoDictionary()
    {
        roomTemplateDictionary.Clear();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (!roomTemplateDictionary.ContainsKey(roomTemplate.guid))
            {
                roomTemplateDictionary.Add(roomTemplate.guid, roomTemplate);
            }
            else
            {
                Debug.Log("Duplicate Room Template ID: " + roomTemplate.guid);
            }
        }
    }

    /// <summary>
    /// Attempts to build a random dungeon based on the provided room node graph.
    /// </summary>
    /// <param name="roomNodeGraph">The room node graph to use for building the dungeon.</param>
    /// <returns>True if the dungeon was successfully built without overlaps, otherwise false.</returns>
    private bool AttemptToBuildRandomDungeon(RoomNodeGraphSO roomNodeGraph)
    {
        Queue<RoomNodeSO> openRoomNodeQueue = new Queue<RoomNodeSO>();

        RoomNodeSO entranceNode = roomNodeGraph.GetRoomNode(roomNodeTypeList.list.Find(x => x.isEntrance));

        if (entranceNode != null)
        {
            openRoomNodeQueue.Enqueue(entranceNode);
        }
        else
        {
            Debug.Log("No Entrance");
            return false;
        }

        bool noRoomOverlaps = true;
        noRoomOverlaps = ProcessRommsInOpenRoomNodeQueue(roomNodeGraph, openRoomNodeQueue, noRoomOverlaps);

        return openRoomNodeQueue.Count == 0 && noRoomOverlaps;
    }

    /// <summary>
    /// Processes rooms in the open room node queue to build the dungeon.
    /// </summary>
    /// <param name="roomNodeGraph">The room node graph to use for building the dungeon.</param>
    /// <param name="openRoomNodeQueue">The queue of open room nodes to process.</param>
    /// <param name="noRoomOverlaps">Indicates whether there are no room overlaps.</param>
    /// <returns>True if there are no room overlaps, otherwise false.</returns>
    private bool ProcessRommsInOpenRoomNodeQueue(RoomNodeGraphSO roomNodeGraph, Queue<RoomNodeSO> openRoomNodeQueue, bool noRoomOverlaps)
    {
        while (openRoomNodeQueue.Count > 0 && noRoomOverlaps)
        {
            RoomNodeSO roomNode = openRoomNodeQueue.Dequeue();

            foreach (RoomNodeSO childRoomNode in roomNodeGraph.GetChildRoomNodes(roomNode))
            {

                if (childRoomNode != null)
                {
                    openRoomNodeQueue.Enqueue(childRoomNode);
                }
            }

            if (roomNode.roomNodeType.isEntrance)
            {
                RoomTemplateSO roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
                Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);
                room.isPositioned = true;

                roomDictionary.Add(room.id, room);
            }
            else
            {
                Room parentRoom = roomDictionary[roomNode.parentRoomNodeIDList[0]];

                noRoomOverlaps = CanPlaceRoomWithNoOverlaps(roomNode, parentRoom);
            }
        }

        return noRoomOverlaps;
    }

    /// <summary>
    /// Attempts to place a room connected to the parent room without any overlaps.
    /// </summary>
    /// <param name="roomNode">The room node to be placed.</param>
    /// <param name="parentRoom">The parent room to which the new room will be connected.</param>
    /// <returns>True if the room was placed successfully without overlaps, otherwise false.</returns>
    private bool CanPlaceRoomWithNoOverlaps(RoomNodeSO roomNode, Room parentRoom)
    {
        while (true)
        {
            List<Doorway> unconnectedAvailableParentDoorways = GetUnconnectedAvailableDoorways(parentRoom.doorWayList).ToList();
            if (unconnectedAvailableParentDoorways.Count == 0)
            {
                return false; // room overlaps
            }

            Doorway doorwayParent = unconnectedAvailableParentDoorways[UnityEngine.Random.Range(0, unconnectedAvailableParentDoorways.Count)];

            RoomTemplateSO roomTemplate = GetRandomForRoomConsistentWithParent(roomNode, doorwayParent);

            Room room = CreateRoomFromRoomTemplate(roomTemplate, roomNode);

            if (PlaceTheRoom(parentRoom, doorwayParent, room))
            {
                room.isPositioned = true;
                roomDictionary.Add(room.id, room);
                return true;
            }
        }
    }

    /// <summary>
    /// Gets the unconnected and available doorways from the provided list of doorways.
    /// </summary>
    /// <param name="roomDoorwayList">The list of doorways to check.</param>
    /// <returns>An enumerable of unconnected and available doorways.</returns>
    private IEnumerable<Doorway> GetUnconnectedAvailableDoorways(List<Doorway> roomDoorwayList)
    {
        foreach (Doorway doorway in roomDoorwayList)
        {
            if (!doorway.isConnected && !doorway.isUnavailable)
            {
                yield return doorway;
            }
        }
    }

    /// <summary>
    /// Gets a random room template that is consistent with the parent room's doorway orientation.
    /// </summary>
    /// <param name="roomNode">The room node for which the template is needed.</param>
    /// <param name="doorwayParent">The parent room's doorway.</param>
    /// <returns>A random room template that matches the parent room's doorway orientation.</returns>
    private RoomTemplateSO GetRandomForRoomConsistentWithParent(RoomNodeSO roomNode, Doorway doorwayParent)
    {
        RoomTemplateSO roomTemplate = null;
        if (roomNode.roomNodeType.isCorridor)
        {
            switch (doorwayParent.orientation)
            {
                case Orientation.north:
                case Orientation.south:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorNS));
                    break;

                case Orientation.east:
                case Orientation.west:
                    roomTemplate = GetRandomRoomTemplate(roomNodeTypeList.list.Find(x => x.isCorridorEW));
                    break;

                case Orientation.none:
                    break;
                default:
                    break;
            }
        }
        else
        {
            roomTemplate = GetRandomRoomTemplate(roomNode.roomNodeType);
        }

        return roomTemplate;
    }

    /// <summary>
    /// Attempts to place a room connected to the parent room without any overlaps.
    /// </summary>
    /// <param name="parentRoom">The parent room to which the new room will be connected.</param>
    /// <param name="doorwayParent">The parent room's doorway.</param>
    /// <param name="room">The room to be placed.</param>
    /// <returns>True if the room was placed successfully without overlaps, otherwise false.</returns>
    private bool PlaceTheRoom(Room parentRoom, Doorway doorwayParent, Room room)
    {
        Doorway doorway = GetOppositeDoorway(doorwayParent, room.doorWayList);

        if (doorway == null)
        {
            doorwayParent.isUnavailable = true;
            return false;
        }

        // Calculate 'world' grid parent doorway
        Vector2Int parentDoorwayPosition = parentRoom.lowerBounds + doorwayParent.position - parentRoom.templateLowerBounds;

        Vector2Int adjustment = Vector2Int.zero;

        switch (doorway.orientation)
        {
            case Orientation.north:
                adjustment = new Vector2Int(0, -1);
                break;
            case Orientation.east:
                adjustment = new Vector2Int(-1, 0);
                break;
            case Orientation.south:
                adjustment = new Vector2Int(0, 1);
                break;
            case Orientation.west:
                adjustment = new Vector2Int(1, 0);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }

        //room lowerbounds, upper bound
        room.lowerBounds = parentDoorwayPosition + adjustment + room.templateLowerBounds - doorway.position;
        room.upperBounds = room.lowerBounds + room.templateUpperBounds - room.templateLowerBounds;

        Room overlappingRoom = CheckForRoomOverlap(room);

        if (overlappingRoom == null)
        {
            doorwayParent.isConnected = true;
            doorwayParent.isUnavailable = true;

            doorway.isConnected = true;
            doorway.isUnavailable = true;
            return true;
        }
        else
        {
            doorwayParent.isUnavailable = true;
            return false;
        }
    }

    /// <summary>
    /// Checks if the given room overlaps with any existing rooms in the dungeon.
    /// </summary>
    /// <param name="roomToTest">The room to test for overlaps.</param>
    /// <returns>The overlapping room if found, otherwise null.</returns>
    private Room CheckForRoomOverlap(Room roomToTest)
    {
        foreach (KeyValuePair<string, Room> keyValuePair in roomDictionary)
        {
            Room room = keyValuePair.Value;

            if (room.id == roomToTest.id || !room.isPositioned)
            {
                continue;
            }
            if (IsOverLappingRoom(roomToTest, room))
            {
                return room;
            }
        }

        return null;
    }
    /// <summary>
    /// Determines if two rooms overlap.
    /// </summary>
    /// <param name="room1">The first room.</param>
    /// <param name="room2">The second room.</param>
    /// <returns>True if the rooms overlap, otherwise false.</returns>
    private bool IsOverLappingRoom(Room room1, Room room2)
    {
        bool isOverlappingX = IsOverLappingInterval(room1.lowerBounds.x, room1.upperBounds.x, room2.lowerBounds.x, room2.upperBounds.x);
        bool isOverlappingY = IsOverLappingInterval(room1.lowerBounds.y, room1.upperBounds.y, room2.lowerBounds.y, room2.upperBounds.y);
        return isOverlappingX && isOverlappingY;
    }

    /// <summary>
    /// Determines if two intervals overlap.
    /// </summary>
    /// <param name="imin1">The minimum value of the first interval.</param>
    /// <param name="imax1">The maximum value of the first interval.</param>
    /// <param name="imin2">The minimum value of the second interval.</param>
    /// <param name="imax2">The maximum value of the second interval.</param>
    /// <returns>True if the intervals overlap, otherwise false.</returns>
    private bool IsOverLappingInterval(int imin1, int imax1, int imin2, int imax2)
    {
        return Mathf.Max(imin1, imin2) <= Mathf.Min(imax1, imax2);
    }

    /// <summary>
    /// Gets the opposite doorway from the provided list that matches the orientation of the parent doorway.
    /// </summary>
    /// <param name="doorwayParent">The parent doorway.</param>
    /// <param name="doorWayList">The list of doorways to search.</param>
    /// <returns>The opposite doorway if found, otherwise null.</returns>
    private Doorway GetOppositeDoorway(Doorway doorwayParent, List<Doorway> doorWayList)
    {
        foreach (Doorway doorway in doorWayList)
        {
            if (doorwayParent.orientation == Orientation.east && doorway.orientation == Orientation.west)
            {
                return doorway;
            }
            else if (doorwayParent.orientation == Orientation.west && doorway.orientation == Orientation.east)
            {
                return doorway;
            }
            else if (doorwayParent.orientation == Orientation.north && doorway.orientation == Orientation.south)
            {
                return doorway;
            }
            else if (doorwayParent.orientation == Orientation.south && doorway.orientation == Orientation.north)
            {
                return doorway;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets a random room template that matches the specified room node type.
    /// </summary>
    /// <param name="roomNodeType">The room node type to match.</param>
    /// <returns>A random room template that matches the specified room node type, or null if no matching template is found.</returns>
    private RoomTemplateSO GetRandomRoomTemplate(RoomNodeTypeSO roomNodeType)
    {
        List<RoomTemplateSO> matchingRoomTemplateList = new List<RoomTemplateSO>();

        foreach (RoomTemplateSO roomTemplate in roomTemplateList)
        {
            if (roomTemplate.roomNodeType == roomNodeType)
            {
                matchingRoomTemplateList.Add(roomTemplate);
            }
        }

        if (matchingRoomTemplateList.Count == 0)
        {
            return null;
        }

        return matchingRoomTemplateList[UnityEngine.Random.Range(0, matchingRoomTemplateList.Count)];
    }

    /// <summary>
    /// Creates a room from the specified room template and room node.
    /// </summary>
    /// <param name="roomTemplate">The room template to use.</param>
    /// <param name="roomNode">The room node to use.</param>
    /// <returns>The created room.</returns>
    private Room CreateRoomFromRoomTemplate(RoomTemplateSO roomTemplate, RoomNodeSO roomNode)
    {
        Room room = new Room();

        room.id = roomNode.id;
        room.templateID = roomTemplate.guid;
        room.prefab = roomTemplate.prefab;
        room.roomNodeType = roomTemplate.roomNodeType;
        room.lowerBounds = roomTemplate.lowerBounds;
        room.upperBounds = roomTemplate.upperBounds;
        room.spawnPositionArray = roomTemplate.spawnPositionArray;
        room.enemiesByLevelList = roomTemplate.enemiesByLevelList;
        room.roomEnemySpawnParametersList = roomTemplate.roomEnemySpawnParametersList;
        room.templateLowerBounds = roomTemplate.lowerBounds;
        room.templateUpperBounds = roomTemplate.upperBounds;

        room.childRoomIDList = CopyStringList(roomNode.childRoomNodeIDList);
        room.doorWayList = CopyDoorwayList(roomTemplate.doorwayList);

        if (roomNode.parentRoomNodeIDList.Count == 0)
        {
            room.parentRoomID = "";
            room.isPreviouslyVisisted = true;

            GameManager.Instance.SetCurrentRoom(room);
        }
        else
        {
            room.parentRoomID = roomNode.parentRoomNodeIDList[0];
        }

        if (room.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel()) == 0)
        {
            room.isClearedOfEnemies = true;
        }

        return room;
    }

    /// <summary>
    /// Creates a copy of the provided list of child room node IDs.
    /// </summary>
    /// <param name="childRoomNodeIDList">The list of child room node IDs to copy.</param>
    /// <returns>A copy of the provided list of child room node IDs.</returns>
    private List<string> CopyStringList(List<string> childRoomNodeIDList)
    {
        List<string> copyStringList = new List<string>();
        foreach (string childRoomNodeID in childRoomNodeIDList)
        {
            copyStringList.Add(childRoomNodeID);
        }
        return copyStringList;
    }

    /// <summary>
    /// Creates a copy of the provided list of doorways.
    /// </summary>
    /// <param name="doorwayList">The list of doorways to copy.</param>
    /// <returns>A copy of the provided list of doorways.</returns>
    private List<Doorway> CopyDoorwayList(List<Doorway> doorwayList)
    {
        List<Doorway> newDoorwayList = new List<Doorway>();

        foreach (Doorway doorway in doorwayList)
        {
            Doorway newDoorway = new Doorway();

            newDoorway.position = doorway.position;
            newDoorway.orientation = doorway.orientation;
            newDoorway.doorPrefab = doorway.doorPrefab;
            newDoorway.isConnected = doorway.isConnected;
            newDoorway.isUnavailable = doorway.isUnavailable;
            newDoorway.doorwayStartCopyPosition = doorway.doorwayStartCopyPosition;
            newDoorway.doorwayCopyTileHeight = doorway.doorwayCopyTileHeight;
            newDoorway.doorwayCopyTileWidth = doorway.doorwayCopyTileWidth;

            newDoorwayList.Add(newDoorway);
        }

        return newDoorwayList;
    }


    /// <summary>
    /// Instantiates the room game objects for the dungeon.
    /// </summary>
    private void InstantiateRoomGameobjects()
    {
        foreach (KeyValuePair<string, Room> keyValuePair in roomDictionary)
        {
            Room room = keyValuePair.Value;

            Vector3 roomPosition = new Vector3(room.lowerBounds.x - room.templateLowerBounds.x, room.lowerBounds.y - room.templateLowerBounds.y, 0f);

            GameObject roomGameobject = Instantiate(room.prefab, roomPosition, Quaternion.identity, transform);

            InstantiatedRoom instantiatedRoom = roomGameobject.GetComponentInChildren<InstantiatedRoom>();

            instantiatedRoom.room = room;

            instantiatedRoom.Initialise(roomGameobject);

            room.instantiatedRoom = instantiatedRoom;
        }
    }

    /// <summary>
    /// Gets the room template by its ID.
    /// </summary>
    /// <param name="roomTemplateID">The ID of the room template.</param>
    /// <returns>The room template if found, otherwise null.</returns>
    public RoomTemplateSO GetRoomTemplate(string roomTemplateID)
    {
        if (roomTemplateDictionary.TryGetValue(roomTemplateID, out RoomTemplateSO roomTemplate))
        {
            return roomTemplate;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Gets the room by its ID.
    /// </summary>
    /// <param name="roomID">The ID of the room.</param>
    /// <returns>The room if found, otherwise null.</returns>
    public Room GetRoomByRoomID(string roomID)
    {
        if (roomDictionary.TryGetValue(roomID, out Room room))
        {
            return room;
        }
        else
        {
            return null;
        }
    }

    /// <summary>
    /// Clears the dungeon by destroying all instantiated room game objects and clearing the room dictionary.
    /// </summary>
    private void ClearDungeon()
    {
        if (roomDictionary.Count > 0)
        {
            foreach (KeyValuePair<string, Room> keyvaluepair in roomDictionary)
            {
                Room room = keyvaluepair.Value;
                if (room.instantiatedRoom != null)
                {
                    Destroy(room.instantiatedRoom.gameObject);
                }
            }
        }

        roomDictionary.Clear();
    }
}
