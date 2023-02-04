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
    public float rollModifider = .1f / 5;
    public bool rollLock = false;
    public Int32 maxStamina = 100;
    public Int32 minStamina = 0;
    public Int32 stamina;
    public bool shouldRegenerateStamina = true;
    public float secondsBeforeStaminaRegen = 0.7f;
    private bool isFacingRight = true;
    private bool isInvulnerable = false;
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

    private DateTime lastMovement;

    private bool RaycastHitGround
    {
        get
        {
            return Physics2D.Raycast(this.transform.position, -Vector3.up, GetComponent<CapsuleCollider2D>().bounds.extents.y + 0.4f, this.groundLayer);
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
            return this.stamina > (this.minStamina + 45f);
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
        if (isInvulnerable) return;
        isInvulnerable = true;
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

        characterAnimator.SetTrigger("DamageTrigger");
        StartCoroutine(InvulnerableInterval());
    }

    IEnumerator InvulnerableInterval()
    {
        yield return new WaitForSeconds(.5f);
        this.isInvulnerable = false;
    }

    void Roll()
    {
        if (!this.rollLock && this.RaycastHitGround)
        {
            this.rollLock = true;
            this.characterAnimator.SetTrigger("IsRolling");

            if (this.rigidbody.velocity.x > 0)
            {
                this.rigidbody.velocity = new Vector2(this.rigidbody.velocity.x + this.rollModifider / 3f, rigidbody.velocity.y);
            }
            else if (this.rigidbody.velocity.x < 0)
            {
                this.rigidbody.velocity = new Vector2(this.rigidbody.velocity.x + (-this.rollModifider / 3f), rigidbody.velocity.y);
            }

            this.DepleteStamina(40);

            StartCoroutine(ClearRollBlocker());
        }
    }

    IEnumerator ClearRollBlocker()
    {
        yield return new WaitForSeconds(.7f);
        this.rollLock = false;
    }

    private void Die()
    {
        // todo: requisitar anima��o ao animator aqui
        // todo: ui ao morrer

        this.respawnHandler.Respawn();
        this.ResetHealth();
    }

    public void BeAfk()
    {
        if (!this.characterAnimator.GetBool("IsAFK"))
        {
            this.characterAnimator.SetBool("IsAFK", true);
        }
    }

    public void StopAfk()
    {
        if (this.characterAnimator.GetBool("IsAFK"))
        {
            this.characterAnimator.SetBool("IsAFK", false);
        }
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
        this.SyncHealthWithAnimator();
    }

    void SyncHealthWithAnimator()
    {
        this.characterAnimator.SetInteger("HealthPoints", this.health);
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
            if (this.stamina < this.maxStamina && !this.isSprinting)
            {
                this.stamina += 15;
                this.SyncStaminaWithUi();
            }

            yield return new WaitForSeconds(this.secondsBeforeStaminaRegen);
        }
    }

    IEnumerator HandleAfkSpriteChange()
    {
        while (true)
        {
            var isAfk = this.characterAnimator.GetBool("IsAFK");

            if (lastMovement != null)
            {
                var difference = DateTime.Now - lastMovement;

                if (difference.Seconds < 5)
                {
                    yield return new WaitForSeconds(1f);
                }
            }

            if (currentSpeed == 0f && this.rigidbody.velocity.y == 0f && !isAfk)
            {
                isAfk = !isAfk;

                if (this.characterAnimator.GetBool("IsWalking"))
                {
                    this.characterAnimator.SetBool("IsWalking", false);
                }

                this.characterAnimator.SetBool("IsAFK", isAfk);
            }

            yield return new WaitForSeconds(5f);
        }
    }

    void StartJumping()
    {
        if (!this.characterAnimator.GetBool("IsJumping"))
        {
            this.StopFalling();
            this.characterAnimator.SetBool("IsJumping", true);
        }
    }

    void StopJumping()
    {
        if (this.characterAnimator.GetBool("IsJumping"))
        {
            this.characterAnimator.SetBool("IsJumping", false);
        }
    }

    void StartFalling()
    {
        this.StopJumping();
        this.characterAnimator.SetBool("IsFalling", true);
    }

    void StopFalling()
    {
        if (this.characterAnimator.GetBool("IsFalling"))
        {
            this.characterAnimator.SetBool("IsFalling", false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        this.horizontalInput = Input.GetAxisRaw("Horizontal");


        // If grounded, jump on tap
        if (Input.GetButtonDown("Jump") && this.RaycastHitGround)
        {
            StartJumping();
            this.JumpBasic();
            this.lastMovement = DateTime.Now;
        }

        // If grounded, jump on hold, but more
        if (Input.GetButtonDown("Jump") && this.rigidbody.velocity.y > 0f)
        {
            StartJumping();
            this.JumpContinuous();
            this.lastMovement = DateTime.Now;
        }

        if (this.rigidbody.velocity.y < -1f)
        {
            this.StartFalling();
        }

        Flip();

        // absolute cat, mf wont leave falling state while speed = 0 and i got no sanity left marhaba
        if (this.rigidbody.velocity.x == 0 && this.RaycastHitGround && this.rigidbody.velocity.y == 0)
        {
            this.StopFalling();
            this.characterAnimator.SetBool("IsWalking", false);
        }
    }

    void StartWalking()
    {
        if (!this.characterAnimator.GetBool("IsWalking"))
        {
            this.characterAnimator.SetBool("IsWalking", true);
        }
    }

    private void FixedUpdate()
    {
        var speed = this.horizontalInput * this.speedModifier;

        // no input while dashing
        if(rollLock)
        {
            return;
        }

        if (this.isSprinting && this.HasStaminaToSprint)
        {
            Roll();
            return;
        }


        if (speed != 0.0)
        {
            this.lastMovement = DateTime.Now;

            if (this.characterAnimator.GetBool("IsAFK"))
            {
                this.characterAnimator.SetBool("IsAFK", false);
            }

            if (this.RaycastHitGround && this.rigidbody.velocity.y < 0.3f)
            {
                this.StartWalking();
            }
        }
        else
        {
            this.characterAnimator.SetBool("IsWalking", false);
            this.runAudio.Stop();
        }

        if (this.RaycastHitGround && speed != 0)
        {
            this.StopJumping();
            this.StopFalling();

            if (!this.runAudio.isPlaying)
            {
                this.runAudio.Play();
            }
        }
        else
        {
            // TODO: anima��o freefall
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
