using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;
using UnityStandardAssets.CrossPlatformInput;

public class PlayerController : Unit
{

    [SerializeField]
    float jumpSpeed = 1f;
    [SerializeField]
    float playerScale = 5f;
    [SerializeField]
    float rollSpeed = 10f;

    public LayerMask attackCollision;

    public float rollCD = 2f;
    public float defaultImpulsePower = 3f;
    private float defaultMoveSpeed;
    private float impulsePower = 10f;

    [HideInInspector]
    public bool rollCheck = true;

    Rigidbody2D playerRigidBody;

    Collider2D playerCollider2D;

    // Use this for initialization
    void Awake()
    {
        Physics2D.IgnoreLayerCollision(10,12,true);
        Physics2D.IgnoreLayerCollision(9,12,true);
    }
    void Start()
    {
        anim = GetComponent<Animator>();
        playerRigidBody = GetComponent<Rigidbody2D>();
        playerCollider2D = GetComponent<Collider2D>();
        defaultMoveSpeed = GetComponent<Unit>().moveSpeed;
    }

    // Update is called once per frame
    void Update()
    {
        if (attackCheck && alive)
        {
            SkillControls();
            Run();
            Jump();
        }

        if (stunned || !alive)
        {
            Impulse();
        }

        if (!rollCheck && !attackCheck && stunned)
        {
            RollMovement();
        }
        ExitGame();
    }


    // Player horizontal movement
    private void Run()
    {
        {
            float controlThrow = CrossPlatformInputManager.GetAxis("Horizontal");
            Vector2 playerVelocity = new Vector2(controlThrow * moveSpeed, playerRigidBody.velocity.y);
            playerRigidBody.velocity = playerVelocity;

            bool playerHasHorizontalSpeed = Mathf.Abs(playerRigidBody.velocity.x) > Mathf.Epsilon;
            anim.SetBool("running", playerHasHorizontalSpeed);
            // Flips player sprite if velocity x < 0
            if (playerHasHorizontalSpeed)
            {
                this.transform.localScale = new Vector2(playerScale * Mathf.Sign(playerRigidBody.velocity.x), playerScale);
            }
        }
    }

    // Player vertical movement
    private void Jump()
    {
        if (!playerCollider2D.IsTouchingLayers(LayerMask.GetMask("Ground")))
        {
            anim.SetBool("jumping", true);
            return;
        }
        if (CrossPlatformInputManager.GetButtonDown("Jump"))
        {
            Vector2 jumpVelocityAdd = new Vector2(0f, jumpSpeed);
            playerRigidBody.velocity += jumpVelocityAdd;
        }
        else
        {
            anim.SetBool("jumping", false);
        }
    }

    // Player skill controls
    private void SkillControls()
    {
        if (CrossPlatformInputManager.GetButtonDown("Fire1"))
        {
            GetDamage();
        }
        else if (CrossPlatformInputManager.GetButtonDown("Fire2"))
        {
            AttackSpecial1();
        }
        else if (CrossPlatformInputManager.GetButtonDown("Fire3"))
        {
            AttackSpecial2();
        }
        else if (CrossPlatformInputManager.GetButtonDown("Fire4"))
        {
            Roll();
        }
    }

    public override void GetDamage()
    {
        if (!attackCheck || stunned) return;
        anim.SetTrigger("attack1");
        attackCheck = false;
        // Change attack animation depending on set parameter
        anim.speed = 1 / attackSpeed;
    }
    private void AttackSpecial1()
    {
        if (!attackCheck || stunned) return;
        anim.SetTrigger("attack2");
        attackCheck = false;
        // Change attack animation depending on set parameter
        anim.speed = 1 / attackSpeed;
    }
    private void AttackSpecial2()
    {
        if (!attackCheck || stunned) return;
        anim.SetTrigger("attack3");
        attackCheck = false;
        // Change attack animation depending on set parameter
        anim.speed = 1 / attackSpeed;
    }
    public void ResetAttackCheck()
    {
        attackCheck = true;
        anim.speed = 1;
    }

    // Creates 2D Raycast hit vector 
    public void CreateAttackVector(int attackModifier)
    {
        direction = Mathf.Sign(transform.localScale.x);
        Vector2 targetVector = new Vector2(direction, 0);
        Vector2 rayOrigin = new Vector2(transform.position.x, transform.position.y + 0.7f);

        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, targetVector, attackRange, attackCollision);

        if (hit)
        {
            hit.transform.GetComponent<Unit>().SetDamage(attack, direction, attackModify);
        }
    }

    public override void SetDamage(float damage, float impulseDirection, bool[] attackModify)
    {
        if (invulnerability) return;
        ReduceHP(damage);
//        SetStun(impulseDirection);
        anim.SetTrigger("attackable");
    }

    public override void SetStun(float direction)
    {
        stunned = true;
    }

    public void ResetStunCheck()
    {
        moveSpeed = defaultMoveSpeed;
        stunned = false;
        SetImpulsePower(defaultImpulsePower);
    }

    private void RollMovement()
    {
        playerRigidBody.velocity = new Vector2(Mathf.Sign(transform.localScale.x) * rollSpeed, playerRigidBody.velocity.y);
    }
    private void Roll()
    {
        if (stunned || !rollCheck) return;
        rollCheck = false;
        invulnerability = true;
        SetImpulsePower(50f);
        stunned = true;
        Physics2D.IgnoreLayerCollision(9, 10, true);
        attackCheck = false;
        anim.SetTrigger("skill");
        StartCoroutine("RollCD");
    }

    public void StopRoll()
    {
        ResetStunCheck();
        playerRigidBody.velocity = new Vector2(0, playerRigidBody.velocity.y);
        Physics2D.IgnoreLayerCollision(9, 10, false);
        invulnerability = false;
    }

    IEnumerator RollCD()
    {
        yield return new WaitForSeconds(rollCD);
        rollCheck = true;
    }

    void Impulse()
    {
        moveSpeed = Mathf.Sqrt(Time.deltaTime) * impulsePower;
    }
    public override void Die()
    {
        anim.SetTrigger("death");
        attackCheck = false;
        alive = false;
        Physics2D.IgnoreLayerCollision(9,10,true);
    }

    void ReduceHP(float damage)
    {
        if (health <= damage)
        {
            Die();
        }

        health -= damage;
    }

    public void SetImpulsePower(float value)
    {
        impulsePower = value;
    }

    private void ExitGame()
    {
        if (CrossPlatformInputManager.GetButtonDown("Cancel"))
        {
            Application.Quit();
        }
    }
}
