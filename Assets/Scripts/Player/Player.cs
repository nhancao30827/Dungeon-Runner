using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(DestroyEvent))]
[RequireComponent(typeof(Destroy))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(AnimatePlayer))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
#endregion
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public Health health;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;
    [HideInInspector] public DestroyEvent destroyEvent;
    [HideInInspector] public Destroy destroy;

    private void Awake()
    {
        health = GetComponent<Health>();
        healthEvent = GetComponent<HealthEvent>();
        destroyEvent = GetComponent<DestroyEvent>();
        destroy = GetComponent<Destroy>();
        idleEvent = GetComponent<IdleEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        healthEvent = GetComponent<HealthEvent>();
        playerControl = GetComponent<PlayerControl>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChange;
        //StartCoroutine(LosingHealth());
    }
    
    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChange;
        //StopCoroutine(LosingHealth());
    }

    private void HealthEvent_OnHealthChange(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        //Debug.Log("Player health: " + healthEventArgs.healthAmount);
        if (healthEventArgs.healthAmount == 0)
        {
            //Debug.Log("Player is dead");
            destroyEvent.CallDestroyEvent(true);
            //Debug.Log("Event Resolve");
        }   
    }

    //private IEnumerator LosingHealth()
    //{
    //    while (true)
    //    {
    //        yield return new WaitForSeconds(1f);
    //        health.TakeDamage(1);
    //    }
    //}

    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;
        SetPlayerHealth();
    }

    public void SetPlayerHealth()
    {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
    }

    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

}
