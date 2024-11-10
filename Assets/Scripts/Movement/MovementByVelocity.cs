using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[DisallowMultipleComponent]
public class MovementByVelocity : MonoBehaviour
{
    private Rigidbody2D rigidBody2D;
    private MovementByVelocityEvent movementByVelocityEvent;

    private void Awake()
    {
        rigidBody2D = GetComponent<Rigidbody2D>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
    }

    private void OnEnable()
    {
        movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;
    }

    private void Disable()
    {
        movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;
    }

    /// <summary>
    /// Handles the movement by velocity event.
    /// </summary>
    /// <param name="movementByVelocityEvent">The movement by velocity event.</param>
    /// <param name="movementByVelocityArgs">The movement by velocity arguments.</param>
    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        MoveRigidBody(movementByVelocityArgs.moveDirection, movementByVelocityArgs.moveSpeed);
    }

    /// <summary>
    /// Moves the rigidbody based on the given move direction and move speed.
    /// </summary>
    /// <param name="moveDirection">The direction of movement.</param>
    /// <param name="moveSpeed">The speed of movement.</param>
    private void MoveRigidBody(Vector2 moveDirection, float moveSpeed)
    {
        rigidBody2D.velocity = moveDirection * moveSpeed;
    }
}
