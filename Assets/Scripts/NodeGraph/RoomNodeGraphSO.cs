using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "RoomNodeGraph", menuName = "Scriptable Objects/Dungeon/Room Node Graph")]
public class RoomNodeGraphSO : ScriptableObject {
    [HideInInspector] public RoomNodeTypeSO roomNodeTypeList;
    [HideInInspector] public List<RoomNodeSO> roomNodeList = new List<RoomNodeSO>();
    [HideInInspector] public Dictionary<string, RoomNodeSO> roomNodeDictionary = new Dictionary<string, RoomNodeSO>();

    private void Awake(){
        LoadRoomNodeDictionary();
    }

    /// <summary>
    /// Loads the room nodes into the dictionary for quick lookup.
    /// </summary>
    private void LoadRoomNodeDictionary()
    {
        roomNodeDictionary.Clear();

        foreach (RoomNodeSO node in roomNodeList)
        {
            roomNodeDictionary[node.id] = node;
        }
    }

    /// <summary>
    /// Gets the room node of the specified type.
    /// </summary>
    /// <param name="roomNodeType">The type of the room node to find.</param>
    /// <returns>The room node of the specified type, or null if not found.</returns>
    public RoomNodeSO GetRoomNode(RoomNodeTypeSO roomNodeType)
    {
        foreach (RoomNodeSO node in roomNodeList)
        {
            if (node.roomNodeType == roomNodeType)
            {
                return node;
            }
        }
        return null;
    }

    /// <summary>
    /// Gets the child room nodes of the specified parent room node.
    /// </summary>
    /// <param name="parentRoomNode">The parent room node.</param>
    /// <returns>An enumerable collection of child room nodes.</returns>
    public IEnumerable<RoomNodeSO> GetChildRoomNodes(RoomNodeSO parentRoomNode)
    {
        foreach (string childRoomNodeID in parentRoomNode.childRoomNodeIDList)
        {
            yield return GetRoomNode(childRoomNodeID);
        }
    }

    /// <summary>
    /// Gets the room node with the specified ID.
    /// </summary>
    /// <param name="roomNodeID">The ID of the room node to find.</param>
    /// <returns>The room node with the specified ID, or null if not found.</returns>
    public RoomNodeSO GetRoomNode(string roomNodeID)
    {
        if (roomNodeDictionary.TryGetValue(roomNodeID, out RoomNodeSO roomNode))
        {
            return roomNode;
        }
        return null;
    }

    #region Editor Code
#if UNITY_EDITOR
    [HideInInspector] public RoomNodeSO roomNodeToDrawLineFrom = null;
    [HideInInspector] public Vector2 linePosition;

    public void OnValidate(){
        LoadRoomNodeDictionary();
    }
    
    public void SetNodeToDrawConnectionLineFrom(RoomNodeSO node, Vector2 position){
        roomNodeToDrawLineFrom = node;
        linePosition = position;
    }
#endif
    #endregion
}
