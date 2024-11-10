using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Animator))]
[DisallowMultipleComponent]
public class Door : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion

    #region Tooltip
    [Tooltip("BoxCollider2D for DoorCollider gameobject")]
    #endregion
    [SerializeField] private BoxCollider2D doorCollider;

    [HideInInspector] public bool isBossRoomDoor = false;
    private BoxCollider2D doorTrigger;
    private bool isOpen = false;
    private bool previouslyOpened = false;
    private Animator animator;

    /// <summary>
    /// Initializes the door components.
    /// </summary>
    private void Awake()
    {
        doorCollider.enabled = false;
        animator = GetComponent<Animator>();
        doorTrigger = GetComponent<BoxCollider2D>();
    }

    /// <summary>
    /// Opens the door when the player or player's weapon enters the trigger.
    /// </summary>
    /// <param name="collision">The collider that entered the trigger.</param>
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            OpenDoor();
        }
    }

    /// <summary>
    /// Resets the animator state when the door is enabled.
    /// </summary>
    private void OnEnable()
    {
        // When the player moves far enough away from the room, the animator state gets reset
        animator.SetBool(Settings.open, isOpen);
    }

    /// <summary>
    /// Opens the door and plays the open door sound effect.
    /// </summary>
    public void OpenDoor()
    {
        if (!isOpen)
        {
            isOpen = true;
            previouslyOpened = true;
            doorCollider.enabled = false;
            doorTrigger.enabled = false;

            animator.SetBool(Settings.open, true);

            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.doorOpenCloseSoundEffect);
        }
    }

    /// <summary>
    /// Locks the door, preventing it from being opened.
    /// </summary>
    public void LockDoor()
    {
        isOpen = false;
        doorCollider.enabled = true;
        doorTrigger.enabled = false;

        animator.SetBool(Settings.open, false);
    }

    /// <summary>
    /// Unlocks the door, allowing it to be opened if it was previously opened.
    /// </summary>
    public void UnlockDoor()
    {
        doorCollider.enabled = false;
        doorTrigger.enabled = true;

        if (previouslyOpened)
        {
            isOpen = false;
            OpenDoor();
        }
    }

    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// Validates the doorCollider field in the Unity Editor.
    /// </summary>
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(doorCollider), doorCollider);
    }
#endif
    #endregion
}
