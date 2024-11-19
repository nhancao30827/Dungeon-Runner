using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class RoomNodeSO : ScriptableObject
{
    [HideInInspector] public string id;
    [HideInInspector] public List<string> parentRoomNodeIDList = new List<string>();
    [HideInInspector] public List<string> childRoomNodeIDList = new List<string>();
    [HideInInspector] public RoomNodeGraphSO roomNodeGraph;
    public RoomNodeTypeSO roomNodeType;
    [HideInInspector] public RoomNodeTypeListSO roomNodeTypeList;

    #region Editor Code

#if UNITY_EDITOR
    [HideInInspector] public Rect rect;
    [HideInInspector] public bool isLeftClickDragging = false;
    [HideInInspector] public bool isSelected = false;

    /// <summary>
    /// Initializes the room node with the specified parameters.
    /// </summary>
    /// <param name="rect">The rectangle defining the node's position and size.</param>
    /// <param name="nodeGraph">The node graph to which this node belongs.</param>
    /// <param name="roomNodeType">The type of the room node.</param>
    public void Initialise(Rect rect, RoomNodeGraphSO nodeGraph, RoomNodeTypeSO roomNodeType)
    {
        this.rect = rect;
        this.id = Guid.NewGuid().ToString();
        this.name = "RoomNode";
        this.roomNodeGraph = nodeGraph;
        this.roomNodeType = roomNodeType;

        roomNodeTypeList = GameResources.Instance.roomNodeTypeList;
    }

    /// <summary>
    /// Draws the room node using the specified GUI style.
    /// </summary>
    /// <param name="nodeStyle">The GUI style to use for drawing the node.</param>
    public void Draw(GUIStyle nodeStyle)
    {
        GUILayout.BeginArea(rect, nodeStyle);
        EditorGUI.BeginChangeCheck();

        if (parentRoomNodeIDList.Count > 0 || roomNodeType.isEntrance)
        {
            EditorGUILayout.LabelField(roomNodeType.roomNodeTypeName);
        }
        else
        {
            int selected = roomNodeTypeList.list.FindIndex(x => x == roomNodeType);
            int selection = EditorGUILayout.Popup("", selected, GetRoomNodeTypesToDisplay());

            roomNodeType = roomNodeTypeList.list[selection];

            // If the room type selection has changed, child connections may be invalid
            if (HasRoomTypeChanged(selected, selection))
            {
                RemoveInvalidChildConnections();
            }
        }

        if (EditorGUI.EndChangeCheck())
        {
            EditorUtility.SetDirty(this);
        }
        GUILayout.EndArea();
    }

    /// <summary>
    /// Determines if the room type has changed in a way that affects child connections.
    /// </summary>
    /// <param name="selected">The previously selected room type index.</param>
    /// <param name="selection">The newly selected room type index.</param>
    /// <returns>True if the room type change affects child connections; otherwise, false.</returns>
    private bool HasRoomTypeChanged(int selected, int selection)
    {
        return (roomNodeTypeList.list[selected].isCorridor && !roomNodeTypeList.list[selection].isCorridor) ||
               (!roomNodeTypeList.list[selected].isCorridor && roomNodeTypeList.list[selection].isCorridor) ||
               (!roomNodeTypeList.list[selected].isBossRoom && roomNodeTypeList.list[selection].isBossRoom);
    }

    /// <summary>
    /// Removes invalid child connections based on the current room type.
    /// </summary>
    private void RemoveInvalidChildConnections()
    {
        if (childRoomNodeIDList.Count > 0)
        {
            for (int i = childRoomNodeIDList.Count - 1; i >= 0; i--)
            {
                RoomNodeSO childRoomNode = roomNodeGraph.GetRoomNode(childRoomNodeIDList[i]);

                if (childRoomNode != null)
                {
                    RemoveChildRoomNodeIDFromRoomNode(childRoomNode.id);
                    childRoomNode.RemoveParentRoomNodeIDFromRoomNode(id);
                }
            }
        }
    }

    /// <summary>
    /// Gets the room node types to display in the editor.
    /// </summary>
    /// <returns>An array of room node type names to display.</returns>
    public string[] GetRoomNodeTypesToDisplay()
    {
        List<string> roomArray = new List<string>();

        for (int i = 0; i < roomNodeTypeList.list.Count; i++)
        {
            if (roomNodeTypeList.list[i].displayInNodeGraphEditor)
            {
                roomArray.Add(roomNodeTypeList.list[i].roomNodeTypeName);
            }
        }

        return roomArray.ToArray();
    }

    /// <summary>
    /// Processes the current event for the room node.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    public void ProcessEvents(Event currentEvent)
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
    /// Processes a mouse down event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseDownEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDownEvent();
        }
        else if (currentEvent.button == 1)
        {
            ProcessRightClickDownEvent(currentEvent);
        }
    }

    /// <summary>
    /// Processes a left mouse button down event.
    /// </summary>
    private void ProcessLeftClickDownEvent()
    {
        // Highlight the selected node in the inspector
        Selection.activeObject = this;
        isSelected = !isSelected;
    }

    /// <summary>
    /// Processes a right mouse button down event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessRightClickDownEvent(Event currentEvent)
    {
        roomNodeGraph.SetNodeToDrawConnectionLineFrom(this, currentEvent.mousePosition);
    }

    /// <summary>
    /// Processes a mouse up event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseUpEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickUpEvent();
        }
    }

    /// <summary>
    /// Processes a left mouse button up event.
    /// </summary>
    private void ProcessLeftClickUpEvent()
    {
        if (isLeftClickDragging)
        {
            isLeftClickDragging = false;
        }
    }

    /// <summary>
    /// Processes a mouse drag event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessMouseDragEvent(Event currentEvent)
    {
        if (currentEvent.button == 0)
        {
            ProcessLeftClickDragEvent(currentEvent);
        }
    }

    /// <summary>
    /// Processes a left mouse button drag event.
    /// </summary>
    /// <param name="currentEvent">The current event.</param>
    private void ProcessLeftClickDragEvent(Event currentEvent)
    {
        isLeftClickDragging = true;
        DragNode(currentEvent.delta);
        GUI.changed = true;
    }

    /// <summary>
    /// Drags the node by the specified delta.
    /// </summary>
    /// <param name="delta">The amount to drag the node.</param>
    public void DragNode(Vector2 delta)
    {
        rect.position += delta;
        EditorUtility.SetDirty(this);
    }

    /// <summary>
    /// Adds a child room node ID to this room node.
    /// </summary>
    /// <param name="childID">The child room node ID.</param>
    /// <returns>True if the child room node ID was added; otherwise, false.</returns>
    public bool AddChildRoomNodeIDToRoomNode(string childID)
    {
        RoomNodeSO childNode = roomNodeGraph.GetRoomNode(childID);

        if (roomNodeType.isCorridor && childNode.roomNodeType.isCorridor)
        {
            return false;
        }

        if (IsChildRoomValid(childID))
        {
            childRoomNodeIDList.Add(childID);
            return true;
        }

        return false;
    }

    /// <summary>
    /// Determines if the specified child room node ID is valid.
    /// </summary>
    /// <param name="childID">The child room node ID.</param>
    /// <returns>True if the child room node ID is valid; otherwise, false.</returns>
    public bool IsChildRoomValid(string childID)
    {
        RoomNodeSO childNode = roomNodeGraph.GetRoomNode(childID);

        if (IsInvalidChildNode(childNode, childID))
        {
            return false;
        }

        bool isConnectedBossNodeAlready = roomNodeGraph.roomNodeList.Exists(node => node.roomNodeType.isBossRoom && node.parentRoomNodeIDList.Count > 0);

        if (childNode.roomNodeType.isBossRoom && isConnectedBossNodeAlready)
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Determines if the specified child node is invalid.
    /// </summary>
    /// <param name="childNode">The child node.</param>
    /// <param name="childID">The child node ID.</param>
    /// <returns>True if the child node is invalid; otherwise, false.</returns>
    private bool IsInvalidChildNode(RoomNodeSO childNode, string childID)
    {
        return childNode == null ||
               childNode.roomNodeType.isNone ||
               id == childID ||
               childRoomNodeIDList.Contains(childID) ||
               parentRoomNodeIDList.Contains(childID) ||
               childNode.parentRoomNodeIDList.Count > 0 ||
               (!childNode.roomNodeType.isCorridor && !roomNodeType.isCorridor) ||
               (childNode.roomNodeType.isCorridor && childRoomNodeIDList.Count >= Settings.maxChildCorridors) ||
               childNode.roomNodeType.isEntrance ||
               (!childNode.roomNodeType.isCorridor && childRoomNodeIDList.Count > 0);
    }

    /// <summary>
    /// Adds a parent room node ID to this room node.
    /// </summary>
    /// <param name="parentID">The parent room node ID.</param>
    /// <returns>True if the parent room node ID was added; otherwise, false.</returns>
    public bool AddParentRoomNodeIDToRoomNode(string parentID)
    {
        parentRoomNodeIDList.Add(parentID);
        return true;
    }

    /// <summary>
    /// Removes a child room node ID from this room node.
    /// </summary>
    /// <param name="childID">The child room node ID.</param>
    /// <returns>True if the child room node ID was removed; otherwise, false.</returns>
    public bool RemoveChildRoomNodeIDFromRoomNode(string childID)
    {
        return childRoomNodeIDList.Remove(childID);
    }

    /// <summary>
    /// Removes a parent room node ID from this room node.
    /// </summary>
    /// <param name="parentID">The parent room node ID.</param>
    /// <returns>True if the parent room node ID was removed; otherwise, false.</returns>
    public bool RemoveParentRoomNodeIDFromRoomNode(string parentID)
    {
        return parentRoomNodeIDList.Remove(parentID);
    }

#endif

    #endregion
}
