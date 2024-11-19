using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;


public class RoomNodeGraphEditor : EditorWindow
{
    private GUIStyle roomNodeStyle;
    private GUIStyle roomNodeSelectedStyle;
    private static RoomNodeGraphSO currentRoomNodeGraph;

    private Vector2 graphOffset;
    private Vector2 graphDrag;

    private RoomNodeSO currentRoomNode = null;
    private RoomNodeTypeListSO roomNodeTypeList;

    private const float nodeWidth = 160f;
    private const float nodeHeight = 75f;
    private const int nodePadding = 25;
    private const int nodeBorder = 12;

    private const float connectingLineWidth = 3f;
    private const float connectingLineArrowSize = 6f;

    private const float gridLarge = 100f;
    private const float gridSmall = 25f;




    private static void OpenWindow()
    {
        GetWindow<RoomNodeGraphEditor>("Room Node Graph Editor");
        Debug.Log("Room Node Graph Editor Opened");
    }


    private void OnEnable()
    {
        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;

        Selection.selectionChanged += InspectorSelectionChanged;

        InitializeRoomNodeStyles();

    }

    /// <summary>
    /// Initializes the styles for room nodes.
    /// </summary>
    private void InitializeRoomNodeStyles()
    {
        roomNodeStyle = CreateRoomNodeStyle("node3", Color.white);
        roomNodeSelectedStyle = CreateRoomNodeStyle("node3 on", Color.white);
    }

    /// <summary>
    /// Creates a GUIStyle for room nodes.
    /// </summary>
    /// <param name="backgroundResource">The resource name for the background texture.</param>
    /// <param name="textColor">The color of the text.</param>
    /// <returns>A new GUIStyle instance.</returns>
    private GUIStyle CreateRoomNodeStyle(string backgroundResource, Color textColor)
    {
        GUIStyle style = new GUIStyle();
        style.normal.background = EditorGUIUtility.Load(backgroundResource) as Texture2D;
        style.normal.textColor = textColor;
        style.padding = new RectOffset(nodePadding, nodePadding, nodePadding, nodePadding);
        style.border = new RectOffset(nodeBorder, nodeBorder, nodeBorder, nodeBorder);
        return style;
    }

    private void OnDisable()
    {
        Selection.selectionChanged -= InspectorSelectionChanged;
    }


    /// <summary>
    /// Handles the double-click event on an asset in the Project window.
    /// </summary>
    /// <param name="instanceId">The instance ID of the asset.</param>
    /// <param name="line">The line number (not used).</param>
    /// <returns>True if the asset is a RoomNodeGraphSO, otherwise false.</returns>
    [OnOpenAsset(0)]
    public static bool OnDoubleClickAsset(int instanceId, int line)
    {
        RoomNodeGraphSO roomNodeGraph = EditorUtility.InstanceIDToObject(instanceId) as RoomNodeGraphSO;

        if (roomNodeGraph != null)
        {
            OpenWindow();
            currentRoomNodeGraph = roomNodeGraph;

            return true;
        }
        return false;
    }


    private void OnGUI()
    {
        if (currentRoomNodeGraph != null)
        {

            DrawBackgroundGrid(gridSmall, 0.2f, Color.gray);
            DrawBackgroundGrid(gridLarge, 0.3f, Color.gray);

            DrawDraggedLine();

            ProcessEvents(Event.current);

            DrawRoomConnections();

            DrawRoomNodes();
        }

        if (GUI.changed)
        {
            Repaint();
        }
    }

    /// <summary>
    /// Draws a background grid for the room node graph editor.
    /// </summary>
    /// <param name="gridSize">The size of the grid cells.</param>
    /// <param name="gridOpacity">The opacity of the grid lines.</param>
    /// <param name="gridColor">The color of the grid lines.</param>
    private void DrawBackgroundGrid(float gridSize, float gridOpacity, Color gridColor)
    {
        int verticalLineCount = Mathf.CeilToInt((position.width + gridSize) / gridSize);
        int horizontalLineCount = Mathf.CeilToInt((position.height + gridSize) / gridSize);

        Handles.color = new Color(gridColor.r, gridColor.g, gridColor.b, gridOpacity);

        graphOffset += graphDrag * 0.5f;

        Vector3 gridOffset = new Vector3(graphOffset.x % gridSize, graphOffset.y % gridSize, 0);

        DrawGridLines(verticalLineCount, horizontalLineCount, gridSize, gridOffset);

        Handles.color = Color.white;
    }

    /// <summary>
    /// Draws the grid lines for the background grid.
    /// </summary>
    /// <param name="verticalLineCount">The number of vertical lines.</param>
    /// <param name="horizontalLineCount">The number of horizontal lines.</param>
    /// <param name="gridSize">The size of the grid cells.</param>
    /// <param name="gridOffset">The offset of the grid lines.</param>
    private void DrawGridLines(int verticalLineCount, int horizontalLineCount, float gridSize, Vector3 gridOffset)
    {
        for (int i = 0; i < verticalLineCount; i++)
        {
            Handles.DrawLine(new Vector3(gridSize * i, -gridSize, 0) + gridOffset,
                             new Vector3(gridSize * i, position.height + gridSize, 0f) + gridOffset);
        }

        for (int j = 0; j < horizontalLineCount; j++)
        {
            Handles.DrawLine(new Vector3(-gridSize, gridSize * j, 0) + gridOffset,
                             new Vector3(position.width + gridSize, gridSize * j, 0f) + gridOffset);
        }
    }

    /// <summary>
    /// Draws the dragged line for connecting room nodes.
    /// </summary>
    private void DrawDraggedLine()
    {
        if (currentRoomNodeGraph.linePosition != Vector2.zero)
        {
            Handles.DrawBezier(currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition,
                currentRoomNodeGraph.roomNodeToDrawLineFrom.rect.center, currentRoomNodeGraph.linePosition, Color.white, null, connectingLineWidth);
        }
    }

    /// <summary>
    /// Processes the events for the room node graph editor.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessEvents(Event currentEvent)
    {
        graphDrag = Vector2.zero;

        if (currentRoomNode == null || currentRoomNode.isLeftClickDragging == false)
        {
            currentRoomNode = IsMouseOverRoomNode(currentEvent);
        }

        if (currentRoomNode == null || currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            ProcessRoomNodeGraphEvents(currentEvent);
        }
        else
        {
            currentRoomNode.ProcessEvents(currentEvent);
        }
    }

    /// <summary>
    /// Checks if the mouse is over a room node.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    /// <returns>The room node under the mouse, or null if none.</returns>
    private RoomNodeSO IsMouseOverRoomNode(Event currentEvent)
    {
        List<RoomNodeSO> roomNodeList = currentRoomNodeGraph.roomNodeList;

        for (int i = roomNodeList.Count - 1; i >= 0; i--)
        {
            if (roomNodeList[i].rect.Contains(currentEvent.mousePosition))
            {
                return roomNodeList[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Processes the events for the room node graph.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessRoomNodeGraphEvents(Event currentEvent)
    {
        switch (currentEvent.type)
        {
            case EventType.MouseDown:
                ProcessMouseDownEvent(currentEvent);
                break;

            case EventType.MouseUp:
                ProcessMouseUpEvent(currentEvent);
                break;

            case EventType.MouseDrag:
                ProcessMouseDragEvent(currentEvent);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Processes the mouse down event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ShowContextMenu(currentEvent.mousePosition);
        }

        else if (currentEvent.button == 0)
        {
            ClearLineDrag();
            ClearAllSelectedRoomNodes();
        }
    }

    /// <summary>
    /// Clears the selection of all room nodes in the current room node graph.
    /// </summary>
    private void ClearAllSelectedRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected)
            {
                roomNode.isSelected = false;
                GUI.changed = true;
            }
        }
    }

    /// <summary>
    /// Shows the context menu at the specified mouse position.
    /// </summary>
    /// <param name="mousePosition">The position of the mouse.</param>
    private void ShowContextMenu(Vector2 mousePosition)
    {
        GenericMenu menu = new GenericMenu();
        menu.AddItem(new GUIContent("Create Room Node"), false, CreateRoomNode, mousePosition);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Select All Room Nodes"), false, SelectAllRoomNodes);
        menu.AddItem(new GUIContent("Deselect All Room Nodes"), false, DeSelectAllRoomNodes);
        menu.AddSeparator("");
        menu.AddItem(new GUIContent("Delete Selected Room Node Links"), false, DeleteSelectedRoomNodeLinks);
        menu.AddItem(new GUIContent("Delete Selected Room Node"), false, DeleteSelectedRoomNodes);

        menu.ShowAsContext();
    }

    /// <summary>
    /// Creates a room node at the specified mouse position.
    /// </summary>
    /// <param name="mousePositionObject">The position of the mouse.</param>
    private void CreateRoomNode(object mousePositionObject)
    {
        if (currentRoomNodeGraph.roomNodeList.Count == 0)
        {
            CreateRoomNode(new Vector2(20f, 300f), roomNodeTypeList.list.Find(x => x.isEntrance));
        }

        CreateRoomNode(mousePositionObject, roomNodeTypeList.list.Find(x => x.isCorridor));
    }

    /// <summary>
    /// Creates a room node of the specified type at the specified mouse position.
    /// </summary>
    /// <param name="mousePositionObject">The position of the mouse.</param>
    /// <param name="roomNodeType">The type of the room node.</param>
    private void CreateRoomNode(object mousePositionObject, RoomNodeTypeSO roomNodeType)
    {
        Vector2 mousePosition = (Vector2)mousePositionObject;

        RoomNodeSO roomNode = ScriptableObject.CreateInstance<RoomNodeSO>();
        currentRoomNodeGraph.roomNodeList.Add(roomNode);
        Rect rect = new Rect(mousePosition, new Vector2(nodeWidth, nodeHeight));
        roomNode.Initialise(rect, currentRoomNodeGraph, roomNodeType);

        AssetDatabase.AddObjectToAsset(roomNode, currentRoomNodeGraph);
        AssetDatabase.SaveAssets();

        currentRoomNodeGraph.OnValidate();
    }

    /// <summary>
    /// Selects all room nodes in the current room node graph.
    /// </summary>
    private void SelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = true;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// Deselects all room nodes in the current room node graph.
    /// </summary>
    private void DeSelectAllRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.isSelected = false;
        }
        GUI.changed = true;
    }

    /// <summary>
    /// Deletes the links of the selected room nodes in the current room node graph.
    /// </summary>
    private void DeleteSelectedRoomNodeLinks()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && roomNode.childRoomNodeIDList.Count > 0)
            {
                for (int i = roomNode.childRoomNodeIDList.Count - 1; i >= 0; i--)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(roomNode.childRoomNodeIDList[i]);

                    if (childRoomNode != null && childRoomNode.isSelected)
                    {
                        roomNode.RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        ClearAllSelectedRoomNodes();
    }

    /// <summary>
    /// Deletes the selected room nodes that are not entrance nodes.
    /// </summary>
    private void DeleteSelectedRoomNodes()
    {
        Queue<RoomNodeSO> roomNodeDeletionQueue = new Queue<RoomNodeSO>();

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.isSelected && !roomNode.roomNodeType.isEntrance)
            {
                roomNodeDeletionQueue.Enqueue(roomNode);

                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    RoomNodeSO childRoomNode = currentRoomNodeGraph.GetRoomNode(childRoomNodeID);
                    if (childRoomNode != null)
                    {
                        childRoomNode.RemoveParentRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }

                foreach (string parentRoomNodeID in roomNode.parentRoomNodeIDList)
                {
                    RoomNodeSO parentRoomNode = currentRoomNodeGraph.GetRoomNode(parentRoomNodeID);
                    if (parentRoomNode != null)
                    {
                        parentRoomNode.RemoveChildRoomNodeIDFromRoomNode(roomNode.id);
                    }
                }
            }
        }

        while (roomNodeDeletionQueue.Count > 0)
        {
            RoomNodeSO roomNodeToDelete = roomNodeDeletionQueue.Dequeue();
            currentRoomNodeGraph.roomNodeDictionary.Remove(roomNodeToDelete.id);
            currentRoomNodeGraph.roomNodeList.Remove(roomNodeToDelete);
            DestroyImmediate(roomNodeToDelete, true);
            AssetDatabase.SaveAssets();
        }
    }

    /// <summary>
    /// Processes the mouse up event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 1 && currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            RoomNodeSO roomNode = IsMouseOverRoomNode(currentEvent);
            if (roomNode != null)
            {
                if (currentRoomNodeGraph.roomNodeToDrawLineFrom.AddChildRoomNodeIDToRoomNode(roomNode.id))
                {
                    roomNode.AddParentRoomNodeIDToRoomNode(currentRoomNodeGraph.roomNodeToDrawLineFrom.id);
                }
            }

            ClearLineDrag();
        }
    }

    /// <summary>
    /// Processes the mouse drag event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 1)
        {
            ProcessRightMouseDragEvent(currentEvent);
        }
        else if (currentEvent.button == 0)
        {
            ProcessLeftMouseDragEvent(currentEvent.delta);
        }
    }

    /// <summary>
    /// Processes the right mouse drag event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessRightMouseDragEvent(Event currentEvent)
    {
        if (currentRoomNodeGraph.roomNodeToDrawLineFrom != null)
        {
            DragConnectingLine(currentEvent.delta);
            GUI.changed = true;
        }
    }

    /// <summary>
    /// Processes the left mouse drag event.
    /// </summary>
    /// <param name="dragDelta">The drag delta.</param>
    private void ProcessLeftMouseDragEvent(Vector2 dragDelta)
    {
        graphDrag = dragDelta;

        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.DragNode(dragDelta);
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Drags the connecting line.
    /// </summary>
    /// <param name="delta">The delta.</param>
    public void DragConnectingLine(Vector2 delta)
    {
        currentRoomNodeGraph.linePosition += delta;
    }

    /// <summary>
    /// Clears the line drag.
    /// </summary>
    public void ClearLineDrag()
    {
        currentRoomNodeGraph.roomNodeToDrawLineFrom = null;
        currentRoomNodeGraph.linePosition = Vector2.zero;
        GUI.changed = true;
    }

    /// <summary>
    /// Draws the room connections.
    /// </summary>
    private void DrawRoomConnections()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            if (roomNode.childRoomNodeIDList.Count > 0)
            {
                foreach (string childRoomNodeID in roomNode.childRoomNodeIDList)
                {
                    if (currentRoomNodeGraph.roomNodeDictionary.ContainsKey(childRoomNodeID))
                    {
                        DrawConnectionLine(roomNode, currentRoomNodeGraph.roomNodeDictionary[childRoomNodeID]);
                        GUI.changed = true;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Draws a connection line between the parent and child room nodes.
    /// </summary>
    /// <param name="parentRoomNode">The parent room node.</param>
    /// <param name="childRoomNode">The child room node.</param>
    private void DrawConnectionLine(RoomNodeSO parentRoomNode, RoomNodeSO childRoomNode)
    {
        Vector2 startPosition = parentRoomNode.rect.center;
        Vector2 endPosition = childRoomNode.rect.center;

        Vector2 midPosition = (endPosition + startPosition) / 2f;
        Vector2 direction = endPosition - startPosition;

        Vector2 arrowTailPoint1 = midPosition - new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;
        Vector2 arrowTailPoint2 = midPosition + new Vector2(-direction.y, direction.x).normalized * connectingLineArrowSize;

        Vector2 arrowHeadPoint = midPosition + direction.normalized * connectingLineArrowSize;

        Handles.DrawBezier(arrowTailPoint1, arrowHeadPoint, arrowTailPoint1, arrowHeadPoint, Color.white, null, connectingLineWidth);
        Handles.DrawBezier(arrowTailPoint2, arrowHeadPoint, arrowTailPoint2, arrowHeadPoint, Color.white, null, connectingLineWidth);

        Handles.DrawBezier(startPosition, endPosition, startPosition, endPosition, Color.white, null, connectingLineWidth);

        GUI.changed = true;
    }

    /// <summary>
    /// Draws all room nodes in the current room node graph.
    /// </summary>
    private void DrawRoomNodes()
    {
        foreach (RoomNodeSO roomNode in currentRoomNodeGraph.roomNodeList)
        {
            roomNode.Draw(roomNode.isSelected ? roomNodeSelectedStyle : roomNodeStyle);
        }

        GUI.changed = true;
    }

    /// <summary>
    /// Handles the selection change in the inspector.
    /// </summary>
    private void InspectorSelectionChanged()
    {
        if (Selection.activeObject is RoomNodeGraphSO roomNodeGraph)
        {
            currentRoomNodeGraph = roomNodeGraph;
            GUI.changed = true;
        }
    }
}


