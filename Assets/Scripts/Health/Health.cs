using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    private int startingHealth;
    private int currentHealth;
    private HealthEvent healthEvent;

    /// <summary>
    /// Gets the current health.
    /// </summary>
    /// <returns>The current health value.</returns>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Initializes the HealthEvent component.
    /// </summary>
    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
    }

    /// <summary>
    /// Calls the health event to update the UI at the start.
    /// </summary>
    private void Start()
    {
        CallHealthEvent(0); // Call the event to update the UI
    }

    /// <summary>
    /// Reduces the current health by the specified damage amount and calls the health event.
    /// </summary>
    /// <param name="damageAmount">The amount of damage to take.</param>
    public void TakeDamage(int damageAmount)
    {
        if (currentHealth > damageAmount)
        {
            currentHealth -= damageAmount;
        }
        else
        {
            currentHealth = 0;
        }
        CallHealthEvent(damageAmount);
    }

    /// <summary>
    /// Calls the health event to notify listeners of a health change.
    /// </summary>
    /// <param name="damageAmount">The amount of damage taken.</param>
    private void CallHealthEvent(int damageAmount)
    {
        healthEvent.CallHealthEventChanged(currentHealth, startingHealth, damageAmount);
    }

    /// <summary>
    /// Sets the starting health and initializes the current health to the same value.
    /// </summary>
    /// <param name="startingHealth">The starting health value.</param>
    public void SetStartingHealth(int startingHealth)
    {
        this.startingHealth = startingHealth;
        currentHealth = startingHealth;
        Debug.Log("Starting health: " + startingHealth);
    }

    /// <summary>
    /// Gets the starting health.
    /// </summary>
    /// <returns>The starting health value.</returns>
    public int GetStartingHealth()
    {
        return startingHealth;
    }
}
