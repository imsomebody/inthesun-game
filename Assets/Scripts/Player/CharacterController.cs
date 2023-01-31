using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Int32 maxHealth;
    public Int32 minHealth;
    public Int32 health;

    private Int32 remainder;

    public float multiplier;

    public void TakeDamage(int damage, float mul = 1)
    {
        var damageCalculation = (int)Math.Round(damage * mul);

        this.health = this.health - damageCalculation;

        if (health <= 0)
        {
            // Save the remaining damage, reset
            this.remainder = Math.Abs(this.health);
            this.health = 0;

            Die();
        }
    }

    private void Die()
    {
        throw new NotImplementedException("Cant die.");
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
