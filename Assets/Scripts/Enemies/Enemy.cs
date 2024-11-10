using System.Collections;
using System.Collections.Generic;
using System.Drawing.Text;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(DestroyEvent))]
[RequireComponent(typeof(Destroy))]
[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AnimateEnemy))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]

#endregion REQUIRE COMPONENTS
[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public Animator animator;

    private EnemyMovementAI enemyMovementAI;
    private CircleCollider2D circleCollider2D;
    private PolygonCollider2D polygonCollider2D;
    private HealthEvent healthEvent;
    private Health health;

    [SerializeField] private SoundEffectSO enemySound;
    private void Awake()
    {
        health = GetComponent<Health>();
        healthEvent = GetComponent<HealthEvent>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        idleEvent = GetComponent<IdleEvent>();

        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
        StartCoroutine(ReduceHealthOverTime());
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
        StopCoroutine(ReduceHealthOverTime());
    }

    /// <summary>
    /// Handles the health lost event.
    /// </summary>
    /// <param name="healthEvent">The health event.</param>
    /// <param name="healthEventArgs">The health event arguments.</param>
    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        //Debug.Log(healthEventArgs.healthAmount);
        if (healthEventArgs.healthAmount == 0)
        {
            DestroyEnemy();
        }
    }

    /// <summary>
    /// Destroys the enemy.
    /// </summary>
    private void DestroyEnemy()
    {
        DestroyEvent destroyEvent = GetComponent<DestroyEvent>();
        //Debug.Log("Kabooommm");
        destroyEvent.CallDestroyEvent(false);
    }

    /// <summary>
    /// Initializes the enemy with the specified details.
    /// </summary>
    /// <param name="enemyDetails">The enemy details.</param>
    /// <param name="enemySpawnNumber">The enemy spawn number.</param>
    /// <param name="dungeonLevel">The dungeon level.</param>
    public void EnemyInitialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;

        SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyAnimationSpeed();
        SetEnemyStartingHealth();
    }

    /// <summary>
    /// Sets the starting health of the enemy.
    /// </summary>
    private void SetEnemyStartingHealth()
    {
        foreach (var enemyHealthDetails in enemyDetails.healthDetailsArray)
        {
            if (enemyHealthDetails.levelSO == GameManager.Instance.GetCurrentDungeonLevel())
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }

        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }

    /// <summary>
    /// Sets the update frame number for enemy movement.
    /// </summary>
    /// <param name="enemySpawnNumber">The enemy spawn number.</param>
    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathfindingOver);
    }

    /// <summary>
    /// Sets the animation speed of the enemy.
    /// </summary>
    private void SetEnemyAnimationSpeed()
    {
        animator.speed = enemyMovementAI.moveSpeed / Settings.baseSpeedForEnemyAnimation;
    }

    /// <summary>
    /// Coroutine to reduce health over time.
    /// </summary>
    /// <returns>IEnumerator.</returns>
    private IEnumerator ReduceHealthOverTime()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            StartCoroutine(EnemyMakeSound());
            health.TakeDamage(5);
            //Debug.Log("Enemy health: " + health.GetCurrentHealth());
        }
    }

    private IEnumerator EnemyMakeSound()
    {
        SoundEffectManager.Instance.PlaySoundEffect(enemySound);
        yield return new WaitForSeconds(2f);

    }
}
