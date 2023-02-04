using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BeeController : MonoBehaviour
{
    private Animator animator;
    private CharacterController character;
    public int HealthRegeneration = 40;
    
    void Start()
    {
        this.animator = this.GetComponentInChildren<Animator>();
        this.character = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
    }

    // Update is called once per frame
    void Update()
    {
        if(character.health < 100 && Physics2D.OverlapCircleAll(this.transform.position, 1f).AsQueryable().Any(transform => transform.name.Contains("Character")))
        {
            character.Heal(this.HealthRegeneration);
            Destroy(this.gameObject);
        }
    }
}
