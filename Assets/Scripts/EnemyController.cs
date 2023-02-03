using Pathfinding;

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    public int minHealth = 0;
    public int maxHealth = 100;
    public int health;

    public void ResetHealth()
    {
        this.health = this.maxHealth;
    }

    void Die()
    {
        Console.WriteLine($"Enemy of name {this.name} has died and was removed from the game area");
        Destroy(this);
    }

    public void TakeDamage(int damage, int mul = 1)
    {
        var damageCalc = damage * mul;

        this.health -= damageCalc;

        if(this.health < this.minHealth)
        {
            this.Die();
        }
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
