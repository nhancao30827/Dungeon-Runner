using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

[RequireComponent(typeof(BoxCollider2D))]
[DisallowMultipleComponent]
public class InstantiatedRoom : MonoBehaviour
{
    [HideInInspector] public Room room;
    [HideInInspector] public Grid grid;
    [HideInInspector] public Tilemap groundTilemap;
    [HideInInspector] public Tilemap decoration1Tilemap;
    [HideInInspector] public Tilemap decoration2Tilemap;
    [HideInInspector] public Tilemap frontTilemap;
    [HideInInspector] public Tilemap collisionTilemap;
    [HideInInspector] public Tilemap minimapTilemap;
    [HideInInspector] public int[,] aStarMovementPenalty;
    [HideInInspector] public Bounds roomColliderBounds;

    private BoxCollider2D boxCollider2D;

    private void Awake()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        roomColliderBounds = boxCollider2D.bounds;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.playerTag && room != GameManager.Instance.GetCurrentRoom())
        {

            this.room.isPreviouslyVisisted = true;

            StaticEventHandler.CallRoomChangedEvent(room);
        }
    }

    /// <summary>
    /// Initializes the room by populating tilemap member variables, blocking off unused doorways,
    /// adding obstacles and preferred paths, adding doors to rooms, and disabling the collision tilemap renderer.
    /// </summary>
    /// <param name="roomGameobject">The room game object to initialize.</param>
    public void Initialise(GameObject roomGameobject)
    {
        PopulateTilemapMemberVariables(roomGameobject);
        BlockOffUnusedDoorway();
        AddObstaclesAndPreferredPaths();
        AddDoorToRooms();
        DisableCollisionTilemapRenderer();
    }

    /// <summary>
    /// Populates the tilemap member variables by finding the appropriate tilemaps in the room game object.
    /// </summary>
    /// <param name="roomGameobject">The room game object containing the tilemaps.</param>
    private void PopulateTilemapMemberVariables(GameObject roomGameobject)
    {
        grid = roomGameobject.GetComponentInChildren<Grid>();
        Tilemap[] tilemaps = roomGameobject.GetComponentsInChildren<Tilemap>();
        foreach (var tilemap in tilemaps)
        {
            switch (tilemap.gameObject.tag)
            {
                case "groundTilemap":
                    groundTilemap = tilemap;
                    break;
                case "decoration1Tilemap":
                    decoration1Tilemap = tilemap;
                    break;
                case "decoration2Tilemap":
                    decoration2Tilemap = tilemap;
                    break;
                case "frontTilemap":
                    frontTilemap = tilemap;
                    break;
                case "collisionTilemap":
                    collisionTilemap = tilemap;
                    break;
                case "minimapTilemap":
                    minimapTilemap = tilemap;
                    break;
            }
        }
    }

    /// <summary>
    /// Blocks off unused doorways by filling them with tiles from the appropriate tilemaps.
    /// </summary>
    private void BlockOffUnusedDoorway()
    {
        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.isConnected)
            {
                continue;
            }

            if (groundTilemap != null)
            {
                BlockADorrwayOnTilemapLayer(groundTilemap, doorway);
            }
            if (decoration1Tilemap != null)
            {
                BlockADorrwayOnTilemapLayer(decoration1Tilemap, doorway);
            }
            if (decoration2Tilemap != null)
            {
                BlockADorrwayOnTilemapLayer(decoration2Tilemap, doorway);
            }
            if (frontTilemap != null)
            {
                BlockADorrwayOnTilemapLayer(frontTilemap, doorway);
            }
            if (collisionTilemap != null)
            {
                BlockADorrwayOnTilemapLayer(collisionTilemap, doorway);
            }
            if (minimapTilemap != null)
            {
                BlockADorrwayOnTilemapLayer(minimapTilemap, doorway);
            }
        }
    }

    /// <summary>
    /// Blocks a doorway on the specified tilemap layer based on the doorway's orientation.
    /// </summary>
    /// <param name="tilemap">The tilemap to block the doorway on.</param>
    /// <param name="doorway">The doorway to block.</param>
    private void BlockADorrwayOnTilemapLayer(Tilemap tilemap, Doorway doorway)
    {
        switch (doorway.orientation)
        {
            case Orientation.north:
            case Orientation.south:
                BlockDoorwayHorizontally(tilemap, doorway);
                break;
            case Orientation.east:
            case Orientation.west:
                BlockDoorwayVertically(tilemap, doorway);
                break;
            case Orientation.none:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Blocks a doorway horizontally on the specified tilemap.
    /// </summary>
    /// <param name="tilemap">The tilemap to block the doorway on.</param>
    /// <param name="doorway">The doorway to block.</param>
    private void BlockDoorwayHorizontally(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                tilemap.SetTile(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0),
                    tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + 1 + xPos, startPosition.y - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Blocks a doorway vertically on the specified tilemap.
    /// </summary>
    /// <param name="tilemap">The tilemap to block the doorway on.</param>
    /// <param name="doorway">The doorway to block.</param>
    private void BlockDoorwayVertically(Tilemap tilemap, Doorway doorway)
    {
        Vector2Int startPosition = doorway.doorwayStartCopyPosition;

        for (int xPos = 0; xPos < doorway.doorwayCopyTileWidth; xPos++)
        {
            for (int yPos = 0; yPos < doorway.doorwayCopyTileHeight; yPos++)
            {
                Matrix4x4 transformMatrix = tilemap.GetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0));

                tilemap.SetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0),
                    tilemap.GetTile(new Vector3Int(startPosition.x + xPos, startPosition.y - yPos, 0)));

                tilemap.SetTransformMatrix(new Vector3Int(startPosition.x + xPos, startPosition.y - 1 - yPos, 0), transformMatrix);
            }
        }
    }

    /// <summary>
    /// Adds obstacles and preferred paths to the room by setting movement penalties for the A* algorithm.
    /// </summary>
    private void AddObstaclesAndPreferredPaths()
    {
        aStarMovementPenalty = new int[room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1];

        for (int x = 0; x < (room.templateUpperBounds.x - room.templateLowerBounds.x + 1); x++)
        {
            for (int y = 0; y < (room.templateUpperBounds.y - room.templateLowerBounds.y + 1); y++)
            {
                aStarMovementPenalty[x, y] = Settings.defaultAStarMovementPenalty;

                TileBase tile = collisionTilemap.GetTile(new Vector3Int(x + room.templateLowerBounds.x, y + room.templateLowerBounds.y, 0));

                foreach (TileBase collisionTile in GameResources.Instance.enemyUnwalkableCollisionTilesArray)
                {
                    if (tile == collisionTile)
                    {
                        aStarMovementPenalty[x, y] = 0;
                        break;
                    }
                }

                if (tile == GameResources.Instance.preferredEnemyPathTile)
                {
                    aStarMovementPenalty[x, y] = Settings.preferredPathAStarMovementPenalty;
                }
            }
        }
    }

    /// <summary>
    /// Adds doors to the rooms based on the doorways' positions and orientations.
    /// </summary>
    private void AddDoorToRooms()
    {
        if (room.roomNodeType.isCorridorEW || room.roomNodeType.isCorridorNS) return;

        foreach (Doorway doorway in room.doorWayList)
        {
            if (doorway.doorPrefab != null && doorway.isConnected)
            {
                GameObject door = Instantiate(doorway.doorPrefab, gameObject.transform);
                door.transform.localPosition = GetDoorPosition(doorway);

                // Uncomment and modify the following lines if needed
                // Door doorComponent = door.GetComponent<Door>();
                // if (room.roomNodeType.isBossRoom)
                // {
                //     doorComponent.isBossRoomDoor = true;
                //     doorComponent.LockDoor();
                // }
            }
        }
    }

    /// <summary>
    /// Gets the local position for the door based on the doorway's orientation.
    /// </summary>
    /// <param name="doorway">The doorway to get the position for.</param>
    /// <returns>The local position for the door.</returns>
    private Vector3 GetDoorPosition(Doorway doorway)
    {
        float tileDistance = Settings.tileSizePixels / Settings.pixelsPerUnit;

        switch (doorway.orientation)
        {
            case Orientation.north:
                return new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y + tileDistance, 0f);
            case Orientation.south:
                return new Vector3(doorway.position.x + tileDistance / 2f, doorway.position.y, 0f);
            case Orientation.east:
                return new Vector3(doorway.position.x + tileDistance, doorway.position.y + tileDistance * 1.25f, 0f);
            case Orientation.west:
                return new Vector3(doorway.position.x, doorway.position.y + tileDistance * 1.25f, 0f);
            default:
                return Vector3.zero;
        }
    }

    /// <summary>
    /// Disables the renderer for the collision tilemap.
    /// </summary>
    private void DisableCollisionTilemapRenderer()
    {
        collisionTilemap.gameObject.GetComponent<TilemapRenderer>().enabled = false;
    }

    /// <summary>
    /// Disables the room's box collider.
    /// </summary>
    private void DisableRoomCollider()
    {
        boxCollider2D.enabled = false;
    }

    /// <summary>
    /// Locks all doors in the room and disables the room collider.
    /// </summary>
    public void LockDoors()
    {
        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            door.LockDoor();
        }

        DisableRoomCollider();
    }

    /// <summary>
    /// Unlocks all doors in the room after a specified delay.
    /// </summary>
    /// <param name="delay">The delay in seconds before unlocking the doors.</param>
    public void UnlockDoors(float delay)
    {
        StartCoroutine(UnlockDoorRountine(delay));
    }

    /// <summary>
    /// Coroutine to unlock all doors in the room after a specified delay.
    /// </summary>
    /// <param name="delay">The delay in seconds before unlocking the doors.</param>
    /// <returns>An IEnumerator for the coroutine.</returns>
    private IEnumerator UnlockDoorRountine(float delay)
    {
        if (delay > 0)
        {
            yield return new WaitForSeconds(delay);
        }

        Door[] doorArray = GetComponentsInChildren<Door>();

        foreach (Door door in doorArray)
        {
            door.UnlockDoor();
        }

        EnableRoomCollider();
    }

    /// <summary>
    /// Enables the room's box collider.
    /// </summary>
    private void EnableRoomCollider()
    {
        boxCollider2D.enabled = true;
    }


}
