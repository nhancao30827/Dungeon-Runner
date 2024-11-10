using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;

[DisallowMultipleComponent]
public class PoolManager : SingletonMonobehaviour<PoolManager>
{
    #region Tooltip
    [Tooltip("Number of prefabs add to the pool and number of gameobjects created")]
    #endregion
    [SerializeField] private Pool[] poolArray = null;
    private Transform objectPoolTransform;
    private Dictionary<int, Queue<Component>> poolDictionary = new Dictionary<int, Queue<Component>>();

    [System.Serializable]
    public struct Pool
    {
        public GameObject prefab;
        public int poolSize;
        public string componentType;
    }

    /// <summary>
    /// Initializes the pool manager and creates pools for the specified prefabs.
    /// </summary>
    private void Start()
    {
        objectPoolTransform = this.gameObject.transform;

        for (int i = 0; i < poolArray.Length; i++)
        {
            CreatePool(poolArray[i].prefab, poolArray[i].poolSize, poolArray[i].componentType);
        }
    }

    /// <summary>
    /// Creates a pool for the specified prefab.
    /// </summary>
    /// <param name="prefab">The prefab to pool.</param>
    /// <param name="poolSize">The size of the pool.</param>
    /// <param name="componentType">The type of component to pool.</param>
    private void CreatePool(GameObject prefab, int poolSize, string componentType)
    {
        int poolKey = prefab.GetInstanceID();
        string prefabName = prefab.name;
        GameObject parentGameObject = new GameObject(prefabName + "Anchor");
        parentGameObject.transform.SetParent(objectPoolTransform);

        if (!poolDictionary.ContainsKey(poolKey))
        {
            poolDictionary.Add(poolKey, new Queue<Component>());

            for (int i = 0; i < poolSize; i++)
            {
                GameObject newObject = Instantiate(prefab, parentGameObject.transform) as GameObject;
                newObject.SetActive(false);
                poolDictionary[poolKey].Enqueue(newObject.GetComponent(Type.GetType(componentType)));
            }
        }
    }

    /// <summary>
    /// Reuses a component from the pool.
    /// </summary>
    /// <param name="prefab">The prefab to reuse.</param>
    /// <param name="position">The position to place the reused component.</param>
    /// <param name="rotation">The rotation to apply to the reused component.</param>
    /// <returns>The reused component.</returns>
    public Component ReuseComponent(GameObject prefab, Vector3 position, Quaternion rotation)
    {
        int poolKey = prefab.GetInstanceID();

        if (poolDictionary.ContainsKey(poolKey))
        {
            Component componentToReuse = GetComponentFromPool(poolKey);
            ResetObject(position, rotation, componentToReuse, prefab);
            return componentToReuse;
        }
        else
        {
            Debug.Log($"This {prefab} has no pool");
            return null;
        }
    }

    /// <summary>
    /// Gets a component from the pool.
    /// </summary>
    /// <param name="poolKey">The key of the pool.</param>
    /// <returns>The component to reuse.</returns>
    private Component GetComponentFromPool(int poolKey)
    {
        Component componentToReuse = poolDictionary[poolKey].Dequeue();
        poolDictionary[poolKey].Enqueue(componentToReuse);

        if (componentToReuse.gameObject.activeSelf)
        {
            componentToReuse.gameObject.SetActive(false);
        }

        return componentToReuse;
    }

    /// <summary>
    /// Resets the object to the specified position and rotation.
    /// </summary>
    /// <param name="position">The position to place the object.</param>
    /// <param name="rotation">The rotation to apply to the object.</param>
    /// <param name="componentToReuse">The component to reuse.</param>
    /// <param name="prefab">The prefab to reset.</param>
    private void ResetObject(Vector3 position, Quaternion rotation, Component componentToReuse, GameObject prefab)
    {
        componentToReuse.transform.position = position;
        componentToReuse.transform.rotation = rotation;
        componentToReuse.gameObject.transform.localScale = prefab.transform.localScale;
    }

    #region Validation
#if UNITY_EDITOR
    /// <summary>
    /// Validates the pool array in the editor.
    /// </summary>
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(poolArray), poolArray);
    }
#endif
    #endregion
}
