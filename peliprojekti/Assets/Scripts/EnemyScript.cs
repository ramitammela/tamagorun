using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class EnemyScript : MonoBehaviour
{
    public enum EnemyType { Basic, Shooter, Trap, Flying, Boss };
    public EnemyType enemyType;
    
    private Rigidbody2D rigidbody;
    private Animator animator;

    private SpriteRenderer[] spriteRenderers;
    private BoxCollider2D[] boxColliders;

    [Header("Common")]
    public PlayerScript player;
    public float jumpPower;
    public float speed;
    public bool playerDetected;
    private Vector2 startPosition;

    public GameObject defeatedGameObject;

    [Header("Basic Enemy")]
    public float jumpStartTime;
    public int startPoint;
    public bool walkDirection;

    private bool jumping;   //  ??

    [Header("Shooter Enemy")]
    public float shootTimer;
    public GameObject bullet;

    private Vector2 bulletStartPosition;
    public int bulletDirection;
    public bool bulletShot;

    public bool bulletActive;   //  ?

    [Header("Trap Enemy")]
    public bool jumpedFromGround;

    [Header("Flying Enemy")]
    public GameObject bomb;
    public Animator bombObject;
    public Animator bombExplosion;

    public BoxCollider2D bombTrigger;
    public ParticleSystem flyParticles;
    public float flyParticlesDirectionX;
    public float flyParticlesDirectionY;
    private Vector2 bombStartPosition;

    //[Header("Boss Enemy")]


    void Start()
    {
        rigidbody = transform.GetComponent<Rigidbody2D>();
        animator = transform.GetComponent<Animator>();
        boxColliders = transform.GetComponentsInChildren<BoxCollider2D>();
        spriteRenderers = transform.GetComponentsInChildren<SpriteRenderer>();

        startPosition = transform.position;

        if (enemyType == EnemyType.Basic)
        {
            InvokeRepeating("StartJumping", jumpStartTime, 3f);
        }

        if (enemyType == EnemyType.Shooter)
        {
            bullet.transform.parent = (null);
            bulletStartPosition = bullet.transform.position;
            bullet.gameObject.SetActive(false);
        }
        if (enemyType == EnemyType.Flying)
        {
            bomb.transform.parent = (null);
            bombStartPosition = bomb.transform.position;
            bomb.gameObject.SetActive(false);

            InvokeRepeating("DropBomb", 0f, 5f);
        }
        if (enemyType == EnemyType.Boss)
        {

        }
    }

    void Update()
    {
        if (enemyType == EnemyType.Basic)
        {
            if (rigidbody.velocity.x == 0)
            {
                animator.SetBool("Walking", false);
            }
            else
            {
                animator.SetBool("Walking", true);
            }
        }

        if (enemyType == EnemyType.Shooter)
        {
            if (player)
            {
                float playerDistance = Vector2.Distance(this.transform.position, player.transform.position);
                //print(d);

                if (playerDistance <= 7.2f)
                {
                    playerDetected = true;
                }
                else
                {
                    playerDetected = false;
                }
            }
            
            if (playerDetected)
            {
                shootTimer += Time.deltaTime;

                if (shootTimer >= 2.5f && !bulletShot)
                {
                    bulletShot = true;
                    shootTimer = 0;

                    bullet.gameObject.SetActive(true);
                    bullet.transform.position = bulletStartPosition;

                    if (transform.position.x > player.transform.position.x) // Player is left side
                    {
                        bulletDirection = 0;
                    }
                    else // Player is right side
                    {
                        bulletDirection = 1;
                    }

                    Shoot();
                }

            if (bulletShot)
            {
                Invoke("ResetBullet", 2f);
            }
            }
            else
            {
                shootTimer = 0;
            }

            //  Bullet Direction

            if (bulletShot)
            {
                if (bulletDirection == 0) // Player is left side
                {
                    bullet.transform.position = new Vector2(bullet.transform.position.x - 0.065f, bullet.transform.position.y);
                }
                else // Player is right side
                {
                    bullet.transform.position = new Vector2(bullet.transform.position.x + 0.065f, bullet.transform.position.y);
                }
            }
        }


        if (enemyType == EnemyType.Trap)
        {
            if (player)
            {
                float playerDistance = Vector2.Distance(this.transform.position, player.transform.position);

                if (playerDistance <= 5f)
                {
                    playerDetected = true;
                    animator.SetBool("Jump", true);
                }
                else
                {
                    playerDetected = false;
                }
            }

            if (playerDetected)
            {
                if (transform.position.x > player.transform.position.x) // Player is left side
                {
                    transform.GetComponent<SpriteRenderer>().flipX = false;
                }
                else // Player is right side
                {
                    transform.GetComponent<SpriteRenderer>().flipX = true;
                }
            }

            if (jumpedFromGround)
            {
                rigidbody.isKinematic = false;
                transform.position = Vector3.MoveTowards(transform.position, new Vector3(player.transform.position.x, transform.position.y, transform.position.z), 0.025f);
                animator.SetFloat("Speed", 1.25f);
            }
            else
            {
                rigidbody.isKinematic = true;
                animator.SetFloat("Speed", 0);
            }
        }

        if (enemyType == EnemyType.Flying)
        {
            if (player)
            {
                float playerDistance = Vector2.Distance(this.transform.position, player.transform.position);

                if (playerDistance <= 7f)
                {   playerDetected = true;  }
                else
                {   playerDetected = false; }
            }

            //  Fly Particles
            if(flyParticles)
            { 
                var vel = flyParticles.velocityOverLifetime;
                vel.x = flyParticlesDirectionX;
                vel.y = flyParticlesDirectionY;
            }
        }
        if (enemyType == EnemyType.Boss)
        {

        }

    }

    void StartJumping()
    {
        StartCoroutine(StartedJumping(2.5f));
    }

    IEnumerator StartedJumping(float delay)
    {
        yield return new WaitForSeconds(delay);

        animator.SetTrigger("Jumping");
        Jump();      
    }

    void Jump() {   rigidbody.AddForce(transform.up * jumpPower, ForceMode2D.Impulse);  }

    void SetKineticOff() {   rigidbody.isKinematic = false;  }


    void Shoot()
    {
        Jump();
    }

    void ResetBullet()
    {
        bulletShot = false;
        bullet.gameObject.SetActive(false);
    }


    #region Flying Enemy

    public void DropBomb()
    {
        if (playerDetected)
        {
            bombTrigger.enabled = false;
            bomb.gameObject.SetActive(true);
            bomb.transform.position = transform.position;

            Invoke("ResetBomb", 2f);
            bombObject.SetBool("Explode", true);
        }
    }

    void ResetBomb()
    {
        bombExplosion.SetBool("Explode", true);
        FindObjectOfType<SoundManager>().Play("Bomb");
        Invoke("HideBomb", 1.35f);
    }
    void HideBomb()
    {
        bombObject.SetBool("Explode", false);
        bombExplosion.SetBool("Explode", false);
        bomb.gameObject.SetActive(false);
    }

    #endregion


    #region Enable/Disable/GameOver/Reset

    void Enable()
    {
        Reset();
    }
    public void Disable()
    {
        foreach(SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = false;
        }
        foreach (BoxCollider2D bc in boxColliders)
        {
            bc.enabled = false;
        }

        if (enemyType == EnemyType.Flying) { flyParticles.gameObject.SetActive(false); }

        defeatedGameObject.SetActive(true);
        defeatedGameObject.transform.position = transform.position;
        Rigidbody2D dfRb = defeatedGameObject.transform.GetComponent<Rigidbody2D>();
        dfRb.simulated = true;
        dfRb.velocity = Vector3.zero;

        if (transform.position.x > player.transform.position.x) // Player is left side
        {
            dfRb.AddForce(transform.right * 5f, ForceMode2D.Impulse);
            dfRb.AddForce(transform.up * 3f, ForceMode2D.Impulse);
        }
        else // Player is right side
        {
            dfRb.AddForce(transform.right * -5f, ForceMode2D.Impulse);
            dfRb.AddForce(transform.up * 3f, ForceMode2D.Impulse);
        }
        
        Invoke("DelayedDisable", 1.5f);

        if (transform.GetComponent<Rigidbody2D>()) { rigidbody.velocity = Vector3.zero; rigidbody.isKinematic = true; transform.GetComponent<Rigidbody2D>().simulated = false; }

        this.enabled = false;
    }

    public void DelayedDisable()
    {
        defeatedGameObject.transform.GetComponent<Rigidbody2D>().simulated = false;
        defeatedGameObject.SetActive(false);
    }

    public void Reset()
    {
        this.enabled = true;
        playerDetected = false;

        transform.gameObject.SetActive(true);
        transform.position = startPosition;

        if (transform.GetComponent<Rigidbody2D>()) { rigidbody.isKinematic = false; transform.GetComponent<Rigidbody2D>().simulated = true; }
        transform.position = startPosition;
        animator.SetTrigger("Reset");

        if (enemyType == EnemyType.Trap) { animator.SetBool("Jump", false); rigidbody.isKinematic = true; jumpedFromGround = false; }
        if (enemyType == EnemyType.Flying) { flyParticles.gameObject.SetActive(true); }

        foreach (SpriteRenderer sr in spriteRenderers)
        {
            sr.enabled = true;
        }
        foreach (BoxCollider2D bc in boxColliders)
        {
            bc.enabled = true;
        }

        transform.position = startPosition;
    }

    #endregion
}