using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/EnemyDetails")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    #endregion

    #region Tooltip
    [Tooltip("The name of the enemy.")]
    #endregion
    public string enemyName;

    #region Tooltip
    [Tooltip("The prefab of the enemy.")]
    #endregion
    public GameObject enemyPrefab;

    #region Tooltip
    [Tooltip("The distance at which the enemy will start chasing the player.")]
    #endregion
    public float chaseDistance = 50f;

    #region Tooltip
    [Tooltip("The damage the enemy can inflict.")]
    #endregion
    public int damage;

    #region Tooltip
    [Tooltip("The sound effect played when the enemy walks.")]
    #endregion
    public SoundEffectSO enemyWalkSound;

    #region Tooltip
    [Tooltip("The array containing the health details of the enemy for each level.")]
    #endregion
    public EnemyHealthDetails[] healthDetailsArray;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(damage), damage, false);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyWalkSound), enemyWalkSound);
    }
#endif
    #endregion
}
