using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharacterController : MonoBehaviour
{
    public Int32 maxHealth;
    public Int32 minHealth;
    public Int32 health;

    public Healthbar bar;

    private Int32 remainder;

    public float multiplier;

    public void TakeDamage(int damage, float mul = 1)
    {
        var damageCalculation = (int)Math.Round(damage * mul);

        this.health = this.health - damageCalculation;
        this.bar.SetHealth(this.health);

        if (health <= 0)
        {
            // Save the remaining damage, reset
            this.remainder = Math.Abs(this.health);
            this.health = 0;


            Die();
        }

        // request animation frames
    }

    private void Die()
    {
        throw new NotImplementedException("Cant die.");
    }

    // Start is called before the first frame update
    void Start()
    {
        this.health = this.maxHealth;
        // Sync with healthbar
        this.bar.SetMaxHealth(this.health);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            this.TakeDamage(5);
        }
    }
}
