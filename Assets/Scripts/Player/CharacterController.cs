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
    public float currentSpeed = 0;
    public float horizontalInput;
    public float speedModifier = 8f;
    public float jumpModifier = 16f;
    public Int32 maxStamina = 100;
    public Int32 minStamina = 0;
    public Int32 stamina;
    public bool shouldRegenerateStamina = true;
    public int secondsBeforeStaminaRegen = 1;
    private bool isFacingRight = true;
    public AudioSource runAudio;

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

    private Animator characterAnimator;
    private RespawnHandler respawnHandler;
    private bool isGrounded;

    private bool RaycastHitGround
    {
        get
        {
            return Physics2D.Raycast(this.transform.position, -Vector3.up, GetComponent<CapsuleCollider2D>().bounds.extents.y + 0.3f);
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

        // todo: request animation frames
    }

    private void Die()
    {
        // todo: requisitar animação ao animator aqui
        // todo: ui ao morrer

        this.respawnHandler.Respawn();
        this.ResetHealth();
    }

    public void BeAfk()
    {
        this.characterAnimator.SetBool("IsWalking", false);
        this.characterAnimator.SetBool("IsAFK", true);
    }

    // Start is called before the first frame update
    void Start()
    {   
        this.characterAnimator = GameObject.FindWithTag("PlayerVFX").GetComponent<Animator>();
        this.respawnHandler = GameObject.FindWithTag("Respawn").GetComponent<RespawnHandler>();
        this.runAudio = GetComponent<AudioSource>();

        this.ResetHealth();
        this.ResetStamina();

        if (this.shouldRegenerateStamina)
        {
            StartCoroutine(PassiveStaminaRegen());
        }

        StartCoroutine("HandleAfkSpriteChange");
        this.BeAfk();
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

    IEnumerator HandleAfkSpriteChange()
    {
        while(true)
        {
            var isAfk = this.characterAnimator.GetBool("IsAFK");

            if (currentSpeed == 0f && this.rigidbody.velocity.y == 0f && !isAfk)
            {
                isAfk = !isAfk;

                if(this.characterAnimator.GetBool("IsWalking"))
                {
                    this.characterAnimator.SetBool("IsWalking", false);
                }

                this.characterAnimator.SetBool("IsAFK", isAfk);
            }

            yield return new WaitForSeconds(6f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.horizontalInput = Input.GetAxisRaw("Horizontal");


        // If grounded, jump on tap
        if (Input.GetButtonDown("Jump") && this.RaycastHitGround)  
        {
            this.JumpBasic();
        }

        // If grounded, jump on hold, but more
        if (Input.GetButtonDown("Jump") && this.rigidbody.velocity.y > 0f)
        {
            this.JumpContinuous();
        }

        if(!this.isGrounded && this.rigidbody.velocity.y == 0f)
        {
            this.isGrounded = false;
        } else if(this.rigidbody.velocity.y > 0f)
        {
            this.isGrounded = true;
        }

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


        if (speed != 0.0)
        {
            if(this.characterAnimator.GetBool("IsAFK"))
            {
                this.characterAnimator.SetBool("IsAFK", false);
            }

            if(this.RaycastHitGround)
            {
                this.characterAnimator.SetBool("IsWalking", true);

                if(!this.runAudio.isPlaying)
                {
                    this.runAudio.Play();
                }
            } else
            {
                // TODO: animação freefall
                this.runAudio.Stop();
            }
        }
        else
        {
            this.characterAnimator.SetBool("IsWalking", false);
            this.runAudio.Stop();
        }

        this.currentSpeed = speed;
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
