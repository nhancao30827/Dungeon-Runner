using System.Collections.Generic;
using UnityEngine;

public static class AStar
{
    // Builds the path from start to end grid position within the given room
    public static Stack<Vector3> BuildPath(Room room, Vector3Int startGridPosition, Vector3Int endGridPosition)
    {
        // Adjust grid positions relative to the room's template lower bounds
        startGridPosition -= (Vector3Int)room.templateLowerBounds;
        endGridPosition -= (Vector3Int)room.templateLowerBounds;

        List<Node> openNodeList = new List<Node>();
        HashSet<Node> closedNodeHastSet = new HashSet<Node>();

        // Initialize grid nodes based on room template bounds
        GridNodes gridNodes = new GridNodes(room.templateUpperBounds.x - room.templateLowerBounds.x + 1,
            room.templateUpperBounds.y - room.templateLowerBounds.y + 1);

        Node startNode = gridNodes.GetGridNode(startGridPosition.x, startGridPosition.y);
        Node targetNode = gridNodes.GetGridNode(endGridPosition.x, endGridPosition.y);

        // Find the shortest path from start node to target node
        Node endPathNode = FindShortestPath(startNode, targetNode, gridNodes, openNodeList, closedNodeHastSet, room.instantiatedRoom);

        if (endPathNode != null)
        {
            // Create and return the path stack if a path is found
            return CreatePathStack(endPathNode, room);
        }

        return null;
    }

    // Finds the shortest path from start node to target node using A* algorithm
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
        HashSet<Node> closedNodeHastSet, InstantiatedRoom instantiatedRoom)
    {
        Vector2Int currentNodeGridPosition = currentNode.gridPosition;

        for (int i = -1; i <= 1; i++)
        {
            for (int j = -1; j <= 1; j++)
            {
                if (i == 0 && j == 0)
                {
                    continue; // Skip the current node itself
                }

                // Get valid neighbor node
                Node validNeighbourNode = GetValidNodeNeighbour(currentNodeGridPosition.x + i, currentNodeGridPosition.y + j,
                    gridNodes, closedNodeHastSet, instantiatedRoom);

                if (validNeighbourNode != null)
                {
                    UpdateNeighborNode(currentNode, targetNode, validNeighbourNode, openNodeList, instantiatedRoom);
                }
            }
        }
    }

    private static void UpdateNeighborNode(Node currentNode, Node targetNode, Node validNeighbourNode,
        List<Node> openNodeList, InstantiatedRoom instantiatedRoom)
    {
        // Get movement penalty for the grid space
        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[validNeighbourNode.gridPosition.x,
            validNeighbourNode.gridPosition.y];

        // Calculate new cost to neighbor
        int newCostToNeighbour = currentNode.gCost + GetDistance(currentNode, validNeighbourNode) + movementPenaltyForGridSpace;

        bool isValidNeighbourNodeInOpenList = openNodeList.Contains(validNeighbourNode);

        // Update neighbor node costs and parent if a better path is found
        if (newCostToNeighbour < validNeighbourNode.gCost || !isValidNeighbourNodeInOpenList)
        {
            validNeighbourNode.gCost = newCostToNeighbour;
            validNeighbourNode.hCost = GetDistance(validNeighbourNode, targetNode);
            validNeighbourNode.parentNode = currentNode;

            if (!isValidNeighbourNodeInOpenList)
            {
                openNodeList.Add(validNeighbourNode);
            }
        }
    }

    // Calculates the distance between two nodes
    public static int GetDistance(Node nodeA, Node nodeB)
    {
        int dstX = Mathf.Abs(nodeA.gridPosition.x - nodeB.gridPosition.x);
        int dstY = Mathf.Abs(nodeA.gridPosition.y - nodeB.gridPosition.y);
        int dst = 14 * dstX + 10 * (dstY - dstX);

        if (dstX > dstY)
        {
            dst = 14 * dstY + 10 * (dstX - dstY);
        }
        return dst;
    }

    // Gets a valid neighbor node if it is within the grid and not in the closed set
    private static Node GetValidNodeNeighbour(int neighbourNodeXPosition, int neighbourNodeYPosition,
        GridNodes gridNodes, HashSet<Node> closedNodeHastSet, InstantiatedRoom instantiatedRoom)
    {
        // Check if the neighbor node is beyond the grid bounds
        if (neighbourNodeXPosition >= instantiatedRoom.room.templateUpperBounds.x - instantiatedRoom.room.templateLowerBounds.x || neighbourNodeXPosition < 0 ||
            neighbourNodeYPosition >= instantiatedRoom.room.templateUpperBounds.y - instantiatedRoom.room.templateLowerBounds.y || neighbourNodeYPosition < 0)
        {
            return null;
        }

        Node neighbourNode = gridNodes.GetGridNode(neighbourNodeXPosition, neighbourNodeYPosition);

        // Get movement penalty for the grid space
        int movementPenaltyForGridSpace = instantiatedRoom.aStarMovementPenalty[neighbourNodeXPosition, neighbourNodeYPosition];

        // Return null if the node is in the closed set or has a movement penalty of 0
        if (closedNodeHastSet.Contains(neighbourNode) || movementPenaltyForGridSpace == 0)
        {
            return null;
        }
        else
        {
            return neighbourNode;
        }
    }

    // Creates a stack of world positions representing the path from the target node to the start node
    private static Stack<Vector3> CreatePathStack(Node targetNode, Room room)
    {
        Stack<Vector3> movementPathStack = new Stack<Vector3>();

        Node nextNode = targetNode;

        Vector3 cellMidPoint = room.instantiatedRoom.grid.cellSize * 0.5f;
        cellMidPoint.z = 0f;

        while (nextNode != null)
        {
            // Convert grid position to world position
            Vector3 worldPosition = room.instantiatedRoom.grid.CellToWorld(new Vector3Int(nextNode.gridPosition.x + room.templateLowerBounds.x,
                nextNode.gridPosition.y + room.templateLowerBounds.y, 0));

            worldPosition += cellMidPoint;

            movementPathStack.Push(worldPosition);

            nextNode = nextNode.parentNode;
        }

        return movementPathStack;
    }
}
