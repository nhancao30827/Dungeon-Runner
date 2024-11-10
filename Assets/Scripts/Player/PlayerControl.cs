using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] private MovementDetailsSO movementDetails;
    [SerializeField] private SoundEffectSO movementSoundEffect;

    private Player player;
    private float moveSpeed;
    private bool isPlayerMovementDisabled;
    private WaitForFixedUpdate waitForFixedUpdate;
    private Coroutine movingSoundCoroutine;

    private void Awake()
    {
        player = GetComponent<Player>();
        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();
        SetPlayerAnimationSpeed();
    }

    /// <summary>
    /// Sets the player's animation speed based on the movement speed.
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimation;
    }

    /// <summary>
    /// Updates the player's state every frame. Handles movement and weapon input.
    /// </summary>
    private void Update()
    {
        if (!isPlayerMovementDisabled)
        {
            MovementInput();
            WeaponInput();
        }
    }

    /// <summary>
    /// Handles the player's movement input.
    /// </summary>
    private void MovementInput()
    {
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");

        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

        if (horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction *= 0.7f;
        }

        if (direction != Vector2.zero)
        {
            if (movingSoundCoroutine == null)
            {
                movingSoundCoroutine = StartCoroutine(MovingSound());
            }
            player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
        }
        else
        {
            if (movingSoundCoroutine != null)
            {
                StopCoroutine(movingSoundCoroutine);
                movingSoundCoroutine = null;
            }
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Coroutine to play the moving sound effect at intervals.
    /// </summary>
    /// <returns>IEnumerator for the coroutine.</returns>
    private IEnumerator MovingSound()
    {
        while (true)
        {
            SoundEffectManager.Instance.PlaySoundEffect(movementSoundEffect);
            yield return new WaitForSeconds(0.25f);
        }
    }

    /// <summary>
    /// Handles the player's weapon input.
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);
    }

    /// <summary>
    /// Processes the input for aiming the weapon.
    /// </summary>
    /// <param name="weaponDirection">The direction in which the weapon is aimed.</param>
    /// <param name="weaponAngleDegrees">The angle of the weapon in degrees.</param>
    /// <param name="playerAngleDegrees">The angle of the player in degrees.</param>
    /// <param name="playerAimDirection">The direction in which the player is aiming.</param>
    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        weaponDirection = (mouseWorldPosition);

        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    /// <summary>
    /// Enables player movement.
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    /// <summary>
    /// Disables player movement and sets the player to idle state.
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.idleEvent.CallIdleEvent();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
}
