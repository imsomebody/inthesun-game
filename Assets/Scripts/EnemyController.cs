using Pathfinding;

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Unity.VisualScripting;

using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [Header("Health")]
    public int minHealth = 0;
    public int maxHealth = 100;
    public int health;
    private bool isInvulnerable = false;

    [Header("Pathfinding")]
    public Transform target;
    public float activateDistance = 50f;
    public float pathUpdateInterval = 0.5f;

    [Header("Movement")]
    public float speed = 200f;
    public float nextWaypointDistance = 3f;
    public float jumpNodeHeightMinimum = 0.8f;
    public float jumpModifier = 0.3f;
    public float jumpCheckOffset = 0.1f;

    [Header("Brains")]
    public bool willMove = true;
    public bool willJump = true;
    public bool willFlip = true;
    public int damageToPlayer = 30;

    [SerializeField]
    private LayerMask playerLayer;

    [Header("A* Brains")]
    public Path path;
    public int currentWaypoint = 0;
    bool isGrounded = false;
    public Seeker seeker;
    Rigidbody2D rb;
    CharacterController player;

    public bool TargetInDistance 
    {  
        get
        {
            return Vector2.Distance(transform.position, target.transform.position) < activateDistance;
        }
    }

    public bool CanDamagePlayer
    {
        get
        {
            var layer = LayerMask.NameToLayer("Player");
            var intersect = Physics2D.OverlapCircleAll(this.rb.position, .5f).AsQueryable().FirstOrDefault(collider => collider.name.Contains("Character"));

            return intersect != null;
        }
    }

    private void Awake()
    {
        this.playerLayer = LayerMask.NameToLayer("Player");
    }

    public void ResetHealth()
    {
        this.health = this.maxHealth;
    }

    void Die()
    {
        Console.WriteLine($"Enemy of name {this.name} has died and was removed from the game area");
        Destroy(this.gameObject);
    }

    public void TakeDamage(int damage, int mul = 1, bool positive = true)
    {
        if (isInvulnerable) return;
        this.isInvulnerable = true;

        var damageCalc = damage;

        this.health -= damageCalc;

        if(this.health < this.minHealth)
        {
            this.Die();
            return;
        }

        StartCoroutine(InvulnerableInterval());
    }

    IEnumerator InvulnerableInterval()
    {
        yield return new WaitForSeconds(.5f);
        this.isInvulnerable = false;
    }

    void Start()
    {
        seeker = GetComponent<Seeker>();
        rb = GetComponent<Rigidbody2D>();
        player = GameObject.FindWithTag("Player").GetComponent<CharacterController>();

        InvokeRepeating("UpdatePath", 0f, pathUpdateInterval);
        this.ResetHealth();
    }

    private void FixedUpdate()
    {
        if (this.TargetInDistance && this.willMove)
        {
            PathFollow();
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(this.transform.position, .8f);
    }

    void UpdatePath()
    {
        if (this.willMove && this.TargetInDistance && seeker.IsDone())
        {
            seeker.StartPath(rb.position, target.position, OnPathComplete);
        }
    }

    void OnPathComplete(Path p)
    {
        if(!p.error)
        {
            path = p;
            currentWaypoint = 0;
        }
    }

    private void Update()
    {
        if (CanDamagePlayer)
        {
            player.TakeDamage(this.damageToPlayer);
        }
    }

    void PathFollow()
    {
        if(this.path == null)
        {
            return;
        }

        if(this.currentWaypoint >= path.vectorPath.Count)
        {
            return;
        }

        this.isGrounded = Physics2D.Raycast(this.transform.position, -Vector3.up, GetComponent<CapsuleCollider2D>().bounds.extents.y + jumpCheckOffset);

        var direction = ((Vector2)this.path.vectorPath[currentWaypoint] - rb.position).normalized;
        var force = direction * speed * Time.deltaTime;

        if (this.willJump && this.isGrounded)
        {
            if(direction.y > jumpNodeHeightMinimum)
            {
                this.rb.AddForce(Vector2.up * speed * jumpModifier);
            }
        }

        this.rb.AddForce(force);

        float distance = Vector2.Distance(this.rb.position, path.vectorPath[currentWaypoint]);

        if(distance < nextWaypointDistance)
        {
            currentWaypoint++;
        }

        if (this.willFlip)
        {
            if(rb.velocity.x > 0.05f)
            {
                transform.localScale = new Vector3(-1f * Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
            else if(rb.velocity.x < -0.05f)
            {
                transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
            }
        }
    }
}
