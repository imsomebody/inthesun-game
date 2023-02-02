using System;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;

public class CharacterController : MonoBehaviour
{
    [Header("Base Health Values")]
    public Int32 maxHealth = 100;
    public Int32 minHealth = 0;
    public Int32 health;
    private Int32 remainder;

    [Header("Base Movement Values")]
    public float horizontalInput;
    public float speedModifier = 8f;
    public float jumpModifier = 16f;
    public Int32 maxStamina = 100;
    public Int32 minStamina = 0;
    public Int32 stamina;
    public bool shouldRegenerateStamina = true;
    public int secondsBeforeStaminaRegen = 1;
    private bool isFacingRight = true;

    [Header("Movement Dependencies]")]
    [SerializeField]
    private Rigidbody2D rigidbody;
    [SerializeField]
    private Transform groundContact;
    [SerializeField]
    private LayerMask groundLayer;

    [Header("Health Dependencies")]
    public Healthbar healthBarUi;
    public Stamina staminaBarUi;

    public Animator CharacterAnimator;

    private bool isGrounded
    {
        get
        {
            return Physics2D.OverlapCircle(this.groundContact.position, 0.2f, this.groundLayer);
        }
    }

    public bool isSprinting
    {
        get
        {
            return Input.GetKey(KeyCode.LeftShift);
        }
    }

    public bool HasStaminaToSprint
    {
        get
        {
            return this.stamina > (this.minStamina * 0.10);
        }
    }

    void SyncHealthWithUi()
    {
        this.healthBarUi.SetHealth(this.health);
    }

    void SyncStaminaWithUi()
    {
        this.staminaBarUi.SetStamina(this.stamina);
    }

    public void TakeDamage(int damage, float mul = 1)
    {
        var damageCalculation = (int)Math.Round(damage * mul);

        this.health = this.health - damageCalculation;
        this.SyncHealthWithUi();

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
        CharacterAnimator = GameObject.FindWithTag("PlayerVFX").GetComponent<Animator>();

        this.ResetHealth();
        this.ResetStamina();

        if (this.shouldRegenerateStamina)
        {
            StartCoroutine(PassiveStaminaRegen());
        }
    }

    public void StopPassiveStaminaRegen()
    {
        StopCoroutine(PassiveStaminaRegen());
    }

    public void ResetHealth()
    {
        this.health = this.maxHealth;
        // Sync with healthbar
        this.SyncHealthWithUi();
    }

    public void ResetStamina()
    {
        this.stamina = this.maxStamina;
        this.SyncStaminaWithUi();
    }

    public void DepleteStamina(int deplete)
    {
        this.stamina -= deplete;
        this.staminaBarUi.SetStamina(this.stamina);
    }

    void JumpBasic()
    {
        this.rigidbody.velocity = new Vector2(this.rigidbody.velocity.x, this.jumpModifier);
    }

    void JumpContinuous()
    {
        this.rigidbody.velocity = new Vector2(this.rigidbody.velocity.x, this.rigidbody.velocity.y * 0.5f);
    }

    IEnumerator PassiveStaminaRegen()
    {
        while (true)
        {
            if (this.stamina < this.maxStamina)
            {
                this.stamina++;
                this.SyncStaminaWithUi();
            }

            yield return new WaitForSeconds(this.secondsBeforeStaminaRegen);
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.horizontalInput = Input.GetAxisRaw("Horizontal");

        // If grounded, jump on tap
        if (Input.GetButtonDown("Jump") && this.isGrounded)  
        {
            this.JumpBasic();
        }

        // // If grounded, jump on hold, but more
        // if (Input.GetButtonDown("Jump") && this.rigidbody.velocity.y > 0f)
        // {
        //     this.JumpContinuous();
        // }

        Flip();
    }

    private void FixedUpdate()
    {
        var speed = this.horizontalInput * this.speedModifier;

        if (this.isSprinting && this.HasStaminaToSprint)
        {
            speed *= 1f;
            this.DepleteStamina(1);
        }


        if (speed != 0.0) CharacterAnimator.SetBool("IsWalking", true);
        else CharacterAnimator.SetBool("IsWalking", false);

        this.rigidbody.velocity = new Vector2(speed, rigidbody.velocity.y);
    }

    private void Flip()
    {
        if (isFacingRight && this.horizontalInput < 0f || !isFacingRight && this.horizontalInput > 0f)
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }
    }
}
