using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

public class BeeController : MonoBehaviour
{
    private Animator animator;
    private CharacterController character;
    public int HealthRegeneration = 40;
    float denyRate = 3f;
    float nextDenyTime = 0f;

    void Start()
    {
        this.animator = this.GetComponentInChildren<Animator>();
        this.character = GameObject.FindGameObjectWithTag("Player").GetComponent<CharacterController>();
    }

    IEnumerator WaitDestroySelf()
    {
        yield return new WaitForSeconds(0.5f);
        Destroy(this.gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        animator.ResetTrigger("DenyHeal");
        animator.ResetTrigger("Heal");


        bool characterIsNear = Physics2D.OverlapCircleAll(this.transform.position, 1f).AsQueryable().Any(transform => transform.name.Contains("Character"));

        if (!characterIsNear) return;

        if (character.health < 100)
        {
            animator.SetTrigger("Heal");
            character.Heal(this.HealthRegeneration);
            StartCoroutine(WaitDestroySelf());
        }
        else if (Time.time > nextDenyTime)
        {
            animator.SetTrigger("DenyHeal");
            nextDenyTime = Time.time + denyRate;
        }
    }
}
