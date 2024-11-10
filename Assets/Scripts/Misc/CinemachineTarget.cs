using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;


[RequireComponent(typeof(CinemachineTargetGroup))]
public class CinemachineTarget : MonoBehaviour
{
    private CinemachineTargetGroup cinemachineTargetGroup;

    #region Tooltip
    [Tooltip("Transform representing the cursor target")]
    #endregion
    [SerializeField] private Transform cursorTarget;


    private void Awake()
    {
        cinemachineTargetGroup = GetComponent<CinemachineTargetGroup>();
    }

    void Start()
    {
        SetCinemachineTargetGroup();
    }

    /// <summary>
    /// Sets the targets for the Cinemachine target group.
    /// </summary>
    private void SetCinemachineTargetGroup()
    {
        CinemachineTargetGroup.Target cinemachineGroupTarget_player = new CinemachineTargetGroup.Target
        {
            weight = 1f,
            radius = 2.5f,
            target = GameManager.Instance.GetPlayer().transform
        };

        CinemachineTargetGroup.Target cinemachineGroupTarget_cursor = new CinemachineTargetGroup.Target
        {
            weight = 0.43f,
            radius = 2f,
            target = cursorTarget
        };

        CinemachineTargetGroup.Target[] cinemachineTargetArray = new CinemachineTargetGroup.Target[]
        {
            cinemachineGroupTarget_player,
            cinemachineGroupTarget_cursor
        };

        cinemachineTargetGroup.m_Targets = cinemachineTargetArray;
    }

    /// <summary>
    /// Update is called once per frame.
    /// </summary>
    private void Update()
    {
        cursorTarget.position = HelperUtilities.GetMouseWorldPosition();
    }
}
