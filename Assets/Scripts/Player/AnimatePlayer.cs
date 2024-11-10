using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimatePlayer : MonoBehaviour
{
    private Player player;
    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {

        player.movementByVelocityEvent.OnMovementByVelocity += MovementByVelocityEvent_OnMovementByVelocity;

        player.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;

        player.idleEvent.OnIdle += IdleEvent_OnIdle;

        player.aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;

    }

    private void OnDisable()
    {

        player.movementByVelocityEvent.OnMovementByVelocity -= MovementByVelocityEvent_OnMovementByVelocity;

        player.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;

        player.idleEvent.OnIdle -= IdleEvent_OnIdle;

        player.aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;

    }

    /// <summary>
    /// Handles the event when the player moves by velocity.
    /// </summary>
    /// <param name="movementByVelocityEvent">The event that triggered the movement by velocity.</param>
    /// <param name="movementByVelocityArgs">The arguments containing the movement details.</param>
    private void MovementByVelocityEvent_OnMovementByVelocity(MovementByVelocityEvent movementByVelocityEvent, MovementByVelocityArgs movementByVelocityArgs)
    {
        SetMovementAnimationParameters();
    }

    /// <summary>
    /// Handles the event when the player moves to a position.
    /// </summary>
    /// <param name="movementToPositionEvent">The event that triggered the movement to position.</param>
    /// <param name="movementToPositionArgs">The arguments containing the movement details.</param>
    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        InitializeAimAnimationParameters();
    }

    /// <summary>
    /// Handles the event when the player is idle.
    /// </summary>
    /// <param name="idleEvent">The event that triggered the idle state.</param>
    private void IdleEvent_OnIdle(IdleEvent idleEvent)
    {
        SetIdleAnimationParameters();
    }

    /// <summary>
    /// Handles the event when the player aims a weapon.
    /// </summary>
    /// <param name="aimWeaponEvent">The event that triggered the weapon aim.</param>
    /// <param name="aimWeaponEventArgs">The arguments containing the aim details.</param>
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        InitializeAimAnimationParameters();
        SetAimWeaponAnimationParamters(aimWeaponEventArgs.aimDirection);
    }

    /// <summary>
    /// Sets the animation parameters for the idle state.
    /// </summary>
    private void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
    }

    /// <summary>
    /// Initializes the animation parameters for aiming.
    /// </summary>
    private void InitializeAimAnimationParameters()
    {
        player.animator.SetBool(Settings.aimUp, false);
        player.animator.SetBool(Settings.aimUpRight, false);
        player.animator.SetBool(Settings.aimUpLeft, false);
        player.animator.SetBool(Settings.aimRight, false);
        player.animator.SetBool(Settings.aimLeft, false);
        player.animator.SetBool(Settings.aimDown, false);
    }

    /// <summary>
    /// Sets the animation parameters for the movement state.
    /// </summary>
    private void SetMovementAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);
    }

    /// <summary>
    /// Sets the animation parameters for aiming the weapon based on the aim direction.
    /// </summary>
    /// <param name="aimDirection">The direction in which the player is aiming.</param>
    private void SetAimWeaponAnimationParamters(AimDirection aimDirection)
    {
        switch (aimDirection)
        {
            case AimDirection.Up:
                player.animator.SetBool(Settings.aimUp, true);
                break;
            case AimDirection.UpRight:
                player.animator.SetBool(Settings.aimUpRight, true);
                break;
            case AimDirection.UpLeft:
                player.animator.SetBool(Settings.aimUpLeft, true);
                break;
            case AimDirection.Right:
                player.animator.SetBool(Settings.aimRight, true);
                break;
            case AimDirection.Left:
                player.animator.SetBool(Settings.aimLeft, true);
                break;
            case AimDirection.Down:
                player.animator.SetBool(Settings.aimDown, true);
                break;
        }
    }
}
