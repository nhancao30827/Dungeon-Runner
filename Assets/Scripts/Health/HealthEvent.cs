using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthEvent : MonoBehaviour
{
    public event Action<HealthEvent, HealthEventArgs> OnHealthChanged;

    public void CallHealthEventChanged(int healthAmount, int maxHealthAmount, int damageAmount)
    {
        HealthEventArgs healthEventArgs = new HealthEventArgs();
        healthEventArgs.healthAmount = healthAmount;
        healthEventArgs.maxHealthAmount = maxHealthAmount;
        healthEventArgs.damageAmount = damageAmount;

        OnHealthChanged?.Invoke(this, healthEventArgs);
    }

}

public class HealthEventArgs : EventArgs
{
    public int healthAmount;
    public int maxHealthAmount;
    public int damageAmount;
}
