using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    /// <summary>
    /// Builds the path from start to end grid position within the given room.
    /// </summary>
    /// <param name="room">The room in which the path is being built.</param>
    /// <param name="startGridPosition">The starting grid position.</param>
    /// <param name="endGridPosition">The ending grid position.</param>
    /// <returns>A stack of Vector3 representing the path from start to end grid position.</returns>
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHastSet = new HashSet<Node>();

        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHastSet, room.instantiatedRoom);

        if (endPathNode != null)
        {
            return CreatePathStack(endPathNode, room);
        }

        return null;
    }

    /// <summary>
    /// Finds the shortest path from the start node to the target node using the A* algorithm.
    /// </summary>
    /// <param name="startNode">The starting node.</param>
    /// <param name="targetNode">The target node to reach.</param>
    /// <param name="gridNodes">The grid nodes of the room.</param>
    /// <param name="openNodeList">The list of open nodes to be evaluated.</param>
    /// <param name="closedNodeHastSet">The set of closed nodes that have already been evaluated.</param>
    /// <param name="instantiatedRoom">The instantiated room containing the grid and movement penalties.</param>
    /// <returns>The target node if a path is found; otherwise, null.</returns>
    public static Node FindShortestPath(Node startNode, Node targetNode,
        GridNodes gridNodes, List<Node> openNodeList,
        HashSet<Node> closedNodeHastSet, InstantiatedRoom instantiatedRoom)
    {
        openNodeList.Add(startNode);
        while (openNodeList.Count > 0)
        {
            openNodeList.Sort(); // Sort nodes by cost

            Node currentNode = openNodeList[0];
            openNodeList.RemoveAt(0);

            if (currentNode == targetNode)
            {
                return currentNode; // Path found
            }

            closedNodeHastSet.Add(currentNode);

            // Evaluate neighbors of the current node
            EvaluateCurrentNodeNeighbours(currentNode, targetNode, gridNodes, openNodeList, closedNodeHastSet, instantiatedRoom);
        }
        return null; // No path found
    }

    /// <summary>
    /// Evaluates the neighbors of the current node and updates their costs and parent nodes if a better path is found.
    /// </summary>
    /// <param name="currentNode">The current node being evaluated.</param>
    /// <param name="targetNode">The target node to reach.</param>
    /// <param name="gridNodes">The grid nodes of the room.</param>
    /// <param name="openNodeList">The list of open nodes to be evaluated.</param>
    /// <param name="closedNodeHastSet">The set of closed nodes that have already been evaluated.</param>
    /// <param name="instantiatedRoom">The instantiated room containing the grid and movement penalties.</param>
    private static void EvaluateCurrentNodeNeighbours(Node currentNode, Node targetNode,
    GridNodes gridNodes, List<Node> openNodeList,
    HashSet<Node> closedNodeHashSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        foreach (Vector2Int neighborOffset in GetNeighborOffsets())
        {
            Vector2Int neighborPosition = currentNodeGridPosition + neighborOffset;

            Node validNeighbourNode = GetValidNodeNeighbour(neighborPosition.x, neighborPosition.y,
                gridNodes, closedNodeHashSet, instantiatedRoom);

            if (validNeighbourNode != null)
            {
                UpdateNeighborNode(currentNode, targetNode, validNeighbourNode, openNodeList, instantiatedRoom);
            }
        }
    }

    /// <summary>
    /// Returns a list of valid neighbor offsets.
    /// </summary>
    private static IEnumerable<Vector2Int> GetNeighborOffsets()
    {
        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue; // Skip the current node itself
                }
                yield return new Vector2Int(i, j);
            }
        }
    }


    /// <summary>
    /// Updates the neighbor node's costs and parent node if a better path is found.
    /// </summary>
    /// <param name="currentNode">The current node being evaluated.</param>
    /// <param name="targetNode">The target node to reach.</param>
    /// <param name="validNeighbourNode">The valid neighbor node to be updated.</param>
    /// <param name="openNodeList">The list of open nodes to be evaluated.</param>
    /// <param name="instantiatedRoom">The instantiated room containing the grid and movement penalties.</param>
    private static void UpdateNeighborNode(Node currentNode, Node targetNode, Node validNeighbourNode,
    List<Node> openNodeList, InstantiatedRoom instantiatedRoom)
    {
        int movementPenalty = instantiatedRoom.aStarMovementPenalty[validNeighbourNode.gridPosition.x,
            validNeighbourNode.gridPosition.y];

        int newCostToNeighbor = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + movementPenalty;

        if (newCostToNeighbor < validNeighbourNode.gCost || !openNodeList.Contains(validNeighbourNode))
        {
            validNeighbourNode.gCost = newCostToNeighbor;
            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
            validNeighbourNode.parentNode = currentNode;

            if (!openNodeList.Contains(validNeighbourNode))
            {
                openNodeList.Add(validNeighbourNode);
            }
        }
    }

    /// <summary>
    /// Calculates the distance between two nodes using the Manhattan distance formula.
    /// </summary>
    /// <param name="nodeA">The first node.</param>
    /// <param name="nodeB">The second node.</param>
    /// <returns>The distance between the two nodes.</returns>
    public static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);

        return dstX > dstY ? 14 * dstY + 10 * (dstX - dstY) : 14 * dstX + 10 * (dstY - dstX);
    }

    /// <summary>
    /// Gets a valid neighbor node if it is within the grid and not in the closed set.
    /// </summary>
    /// <param name="neighbourNodeXPosition">The X position of the neighbor node.</param>
    /// <param name="neighbourNodeYPosition">The Y position of the neighbor node.</param>
    /// <param name="gridNodes">The grid nodes of the room.</param>
    /// <param name="closedNodeHastSet">The set of closed nodes that have already been evaluated.</param>
    /// <param name="instantiatedRoom">The instantiated room containing the grid and movement penalties.</param>
    /// <returns>The valid neighbor node if it is within the grid and not in the closed set; otherwise, null.</returns>
    private static Node GetValidNodeNeighbour(int neighbourNodeXPosition, int neighbourNodeYPosition,
        GridNodes gridNodes, HashSet<Node> closedNodeHastSet, InstantiatedRoom instantiatedRoom)
    {
        if (neighbourNodeXPosition < 0 || neighbourNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x ||
            neighbourNodeYPosition < 0 || neighbourNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y)
        {
            return null;
        }

        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPosition, neighbourNodeYPosition);

        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighbourNodeXPosition, neighbourNodeYPosition];

        if (closedNodeHastSet.Contains(neighbourNode) || movementPenaltyForGridSpace == 0)
        {
            return null;
        }

        return neighbourNode;
    }

    /// <summary>
    /// Creates a stack representing the path from the target node to the start node.
    /// </summary>
    /// <param name="targetNode">The target node where the path ends.</param>
    /// <param name="room">The room containing the grid and nodes.</param>
    /// <returns>A stack of Vector3 representing the path from the target node to the start node.</returns>
    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();
        Node nextNode = targetNode;
        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            Vector3Int gridPosition = new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0);
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(gridPosition) + cellMidPoint;

            movementPathStack.Push(worldPosition);
            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }
}
