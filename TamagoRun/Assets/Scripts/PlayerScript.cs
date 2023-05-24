using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PlayerScript : MonoBehaviour
{
    public bool flying;
    public float flyingJumpPower;
    public GameObject balloon;

    public bool bonusLevel;

    [Header("Components")]
    public SpriteRenderer spriteRenderer;
    public SpriteMask spriteMask;

    private CameraScript cameraScript;
    private Rigidbody2D rigidbody;
    private SpriteRenderer characterSprite;
    private Animator animator;

    [Header("Common")]
    public bool lose;
    public bool canMove;
    public float walkSpeed; private float startWalkSpeed;
    public float currentSpeed;
    [Space(10)]

    public bool isGrounded;
    public float jumpPower; private float startJumpPower;
    [Space(10)]

    public bool walking;
    public bool running;
    [Space(10)]

    public bool powerUpActive;
    [Space(10)]

    public SpriteRenderer sweat;
    public ParticleSystem runParticles;

    private Vector2 startPosition;
    [Space(10)]

    public int lifes;
    public Image[] lifesIcons;

    private int previouslyLifes;

    [Header("Score & UI")]
    public int score;
    public int highScore;

    public TextMeshProUGUI scoreText;
    public TextMeshProUGUI highScoreText;

    [Header("Controls")]
    public Button bButton;  private bool bButtonPressed;
    public Button jumpButton; public bool jumpButtonPressed; public float jumpButtonPressedTime;
    public bool jumped;

    public bool dpadLeft, dpadRight;


    [Header("Timer")]
    public TextMeshProUGUI timerText;
    public float timeRemaining = 10;
    public bool timerIsRunning = false;

    public bool bonusLevelFallingOrFlying;


    void Start()
    {
        Application.targetFrameRate = 60;   //Screen.SetResolution(1080, 2520, true);

        rigidbody = transform.GetComponent<Rigidbody2D>();
        characterSprite = transform.GetComponent<SpriteRenderer>();
        animator = transform.GetComponent<Animator>();
        spriteRenderer = transform.GetComponent<SpriteRenderer>();
        cameraScript = (CameraScript)FindObjectOfType(typeof(CameraScript));

        startPosition = transform.position;
        startWalkSpeed = walkSpeed;
        startJumpPower = jumpPower;

        previouslyLifes = lifes;

        bButton.onClick.AddListener(BButtonPressed);
        //jumpButton.onClick.AddListener(JumpButtonPressed);

        SetHighScore();
        UpdateLifes();

        timerIsRunning = true;

        StartCoroutine(DelayedCanMove(2.1f));
    }

    void Update()
    {
        //  Timer
        if (!lose && !bonusLevel && !bonusLevelFallingOrFlying)
        {
            if (timerIsRunning)
            {
                if (timeRemaining > 0)
                {
                    timeRemaining -= Time.deltaTime;
                    DisplayTime(timeRemaining);
                }
                else
                {
                    Debug.Log("Time has run out!");
                    timeRemaining = 0;
                    timerIsRunning = false;

                    GameManager.Instance.ShowMessage("TIME OUT", 1.5f);
                    Lose();
                }
            }
        }

        //  Going or Leaving the Bonus Level 
        if (bonusLevelFallingOrFlying)
        {
            walkSpeed = 0f;
            jumpPower = 0f;
            animator.SetBool("Stop", true);
        }
        else
        {
            walkSpeed = startWalkSpeed;
            jumpPower = startJumpPower;
            animator.SetBool("Stop", false);
        }


        #region Controls

        if (Input.GetButtonDown("Debug"))
        {
            DebugTest();
        }

        if (!flying && canMove)
        {
            rigidbody.mass = 1f;
            rigidbody.drag = 0f;

            if (Input.GetButton("DPAD Left") || dpadLeft == true)    // Left
            {
                rigidbody.AddForce(-transform.right * walkSpeed, ForceMode2D.Impulse);
                characterSprite.flipX = true;
            }
            else if (Input.GetButton("DPAD Right") || dpadRight == true)   // Right
            {
                rigidbody.AddForce(transform.right * walkSpeed, ForceMode2D.Impulse);
                characterSprite.flipX = false;
            }
            if (Input.GetButtonDown("A Button") && isGrounded)    // Jump
            {
                animator.SetBool("Jump", true);
                jumped = true;
                Invoke("Jump", 0.25f);
            }
            
            #if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_WEBGL
            if (Input.GetButton("A Button") && jumped)
            {
                jumpButtonPressedTime += Time.deltaTime;
            }
            #endif
            
            #if UNITY_ANDROID
            if (jumpButtonPressed && isGrounded)
            {
                if (jumpButtonPressedTime <= 0.25f)
                {
                    jumpButtonPressedTime += Time.deltaTime;
                }
            }
            #endif
        }

        if (flying && canMove)
        {
            rigidbody.mass = 0.38f;
            rigidbody.drag = 3.5f;

            if (Input.GetButton("DPAD Left") || dpadLeft == true)    // Left
            {
                rigidbody.AddForce(-transform.right * walkSpeed, ForceMode2D.Impulse);
                characterSprite.flipX = true;
            }
            else if (Input.GetButton("DPAD Right") || dpadRight == true)   // Right
            {
                rigidbody.AddForce(transform.right * walkSpeed, ForceMode2D.Impulse);
                characterSprite.flipX = false;
            }
            if (Input.GetButtonDown("A Button")) // Up
            {
                Invoke("Jump", 0.25f);
            }
        }

        if (Input.GetButtonDown("Reset All"))    // Reset Score
        {
            PlayerPrefs.SetInt("Highscore", 0);

            SetHighScore();
        }
        if (Input.GetButtonDown("Debug Lose"))   //  Reset
        {
            previouslyLifes -= 1; lifes -= 1;
            UpdateLifes();
            GameOver();
        }

#endregion


        animator.SetFloat("Speed", rigidbody.velocity.magnitude);
        animator.SetFloat("AnimationSpeed", rigidbody.velocity.magnitude * 3f);

        var vel = runParticles.velocityOverLifetime;
        if (rigidbody.velocity.x > 2f && isGrounded)    //  Left
        {
            runParticles.startSize = 0.5f;
            vel.x = -1.5f;
        }
        else if (rigidbody.velocity.x < -2f && isGrounded)   //  Right
        {
            runParticles.startSize = 0.5f;
            vel.x = 1.5f;
        }
        if (rigidbody.velocity.x == 0f)
        {
            runParticles.startSize = 0f;
        }

        if (rigidbody.velocity.y < -3.5f)
        {
            sweat.enabled = true;
            isGrounded = false;
        }
        if (rigidbody.velocity.y == 0f)
        {
            sweat.enabled = false;
            isGrounded = true;
        }

        spriteMask.sprite = spriteRenderer.sprite;

        if (spriteRenderer.flipX == true)
        {
            spriteMask.transform.eulerAngles = new Vector3(0, 180, 0);
        }
        else
        {
            spriteMask.transform.eulerAngles = new Vector3(0, 0, 0);
        }
    }

#region Buttons

    public void DpadPressed(int i)
    {
        if (i == 0) { } //  0
        if (i == 1) { } //  Up
        if (i == 2) { } //  Down
        if (i == 3) //  Left
        {
            dpadLeft = true;
            rigidbody.AddForce(-transform.right * walkSpeed, ForceMode2D.Impulse);
            characterSprite.flipX = true;
        }
        if (i == 4) //  Right
        {
            dpadRight = true;
            rigidbody.AddForce(transform.right * walkSpeed, ForceMode2D.Impulse);
            characterSprite.flipX = false;
        }
    }

    public void DpadNotPressed(int i)
    {
        if (i == 0) { } //  0
        if (i == 1) { } //  Up
        if (i == 2) { } //  Down
        if (i == 3) //  Left
        { dpadLeft = false; }
        if (i == 4) //  Right
        { dpadRight = false; }
    }

    public void BButtonPressed()
    {
        bButtonPressed = true;
        Invoke("BButtonNotPressed", 0.2f);
    }
    private void BButtonNotPressed() { bButtonPressed = false; }

    public void JumpButtonPressed()     // A - Button
    {
        animator.SetBool("Jump", true);
        jumped = true;
        Invoke("Jump", 0.2f);
    }

    public void JumpButtonPressedDown() { if (isGrounded) { jumpButtonPressed = true; JumpButtonPressed(); }}
    public void JumpButtonNotPressedDown() { if (isGrounded) { jumpButtonPressed = false; jumpButtonPressedTime = 0f; }}

#endregion


    IEnumerator DelayedCanMove(float delay)
    {
        yield return new WaitForSeconds(delay);
        canMove = true;
    }

    public void Jump()
    {
        jumped = false; 
        EventSystem.current.SetSelectedGameObject(null);

        if(!flying && canMove && !bonusLevelFallingOrFlying)
        { 
            if (isGrounded && rigidbody.velocity.y > -0.1f)
            {
                
                if (jumpButtonPressedTime > 0.2f)   //  Long Jump
                {
                    rigidbody.AddForce(transform.up * jumpPower * 0.5f, ForceMode2D.Impulse);
                }
                else if (jumpButtonPressedTime < 0.2f)  //  Short Jump
                {
                    rigidbody.AddForce(transform.up * jumpPower * 0.4f, ForceMode2D.Impulse);
                }
                
                isGrounded = false;
                animator.SetBool("Jump", true);

                FindObjectOfType<SoundManager>().Play("Jump");
            }
        }
        if (flying && canMove && !bonusLevelFallingOrFlying)
        {
            rigidbody.AddForce(transform.up * flyingJumpPower, ForceMode2D.Impulse);
            animator.SetBool("Jump", true);

            FindObjectOfType<SoundManager>().Play("Jump");
        }

        jumpButtonPressedTime = 0f;
    }

    public void UpdateLifes()
    {
        previouslyLifes = lifes;

        if (lifes == 0) 
        {
            lifesIcons[0].enabled = false;
            lifesIcons[1].enabled = false;
            lifesIcons[2].enabled = false;
            lifesIcons[3].enabled = false;
            lifesIcons[4].enabled = false;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 1) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = false;
            lifesIcons[2].enabled = false;
            lifesIcons[3].enabled = false;
            lifesIcons[4].enabled = false;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 2) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = true;
            lifesIcons[2].enabled = false;
            lifesIcons[3].enabled = false;
            lifesIcons[4].enabled = false;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 3) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = true;
            lifesIcons[2].enabled = true;
            lifesIcons[3].enabled = false;
            lifesIcons[4].enabled = false;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 4) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = true;
            lifesIcons[2].enabled = true;
            lifesIcons[3].enabled = true;
            lifesIcons[4].enabled = false;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 5) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = true;
            lifesIcons[2].enabled = true;
            lifesIcons[3].enabled = true;
            lifesIcons[4].enabled = true;
            lifesIcons[5].enabled = false;
        }
        if (lifes == 6) 
        {
            lifesIcons[0].enabled = true;
            lifesIcons[1].enabled = true;
            lifesIcons[2].enabled = true;
            lifesIcons[3].enabled = true;
            lifesIcons[4].enabled = true;
            lifesIcons[5].enabled = true;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.transform.tag == "Ground")
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.transform.tag == "Ground")
        {
            isGrounded = true;
            animator.SetBool("Jump", false);
        }

        if (other.tag == "Points")
        {
            AddPoints(other.transform.GetComponent<Collectables>().points, other.transform);
            other.transform.GetComponent<Collectables>().Disable();
            FindObjectOfType<SoundManager>().Play("Pick");
        }

        if (other.tag == "Enemy" && !lose)
        {
            if (powerUpActive)
            {
                AddPoints(50, other.transform);
                other.transform.GetComponent<EnemyScript>().Disable();
            }
            else
            {
                Lose();
            }
        }
        if (other.tag == "EnemyJumpTrigger" && !isGrounded && !lose)
        {
            AddPoints(50, other.transform.parent.transform);
            other.transform.parent.transform.GetComponent<EnemyScript>().Disable();
        }

        if (other.tag == "Breakable")
        {
            animator.SetBool("Jump", false);

            other.transform.GetComponent<SpriteRenderer>().enabled = false;
            other.transform.GetComponentInChildren<ParticleSystem>().Play();
            other.transform.GetComponent<Blocks>().Disable();

            AddPoints(10, other.transform);
        }

        if (other.tag == "PowerUp")
        {
            AddPoints(100, other.transform);
            spriteMask.transform.GetComponentInChildren<Animator>().SetBool("PowerUpActive", true);

            other.transform.GetComponent<Collectables>().Disable();
            powerUpActive = true;

            StartCoroutine(DisablePowerUp(5f));
        }
        if (other.tag == "LifeUp")
        {
            AddPoints(100, other.transform);

            other.transform.GetComponent<Collectables>().Disable();
            
            if (lifes < 6) { previouslyLifes += 1; lifes += 1; }
            UpdateLifes();
        }
        if (other.tag == "LevelCollectible")
        {
            AddPoints(500, other.transform);

            other.transform.GetComponent<Collectables>().Collect();
            other.transform.GetComponent<Collectables>().Disable();
        }
        if (other.tag == "BonusLevelDeathBox" && bonusLevel)
        {
            bonusLevel = false;    
            CameraScript.Instance.StartShake();
            CameraScript.Instance.StartColorHit();
            bonusLevelFallingOrFlying = true;
            StartCoroutine(DelayedStartReturnToGameLevel());

            GameManager.Instance.PlayerRestart();

            GameManager.Instance.ShowMessage("BONUS LEVEL" + "\n" + "FAIL", 3f);
        }
        if (other.transform.tag == "BonusLevelEnd" && bonusLevel)
        {
            bonusLevel = false;
            //transform.position = new Vector2(balloonStartPosition.x, balloonStartPosition.y + 10.0f);
            bonusLevelFallingOrFlying = true; 

            StartCoroutine(DelayedStartReturnToGameLevel());

            //StartCoroutine(ReturnToGameLevel());
            //GameManager.Instance.BonusLevelClouds(false);
            GameManager.Instance.ShowMessage("BONUS LEVEL" + "\n" + "CLEAR", 3f);
        }

        if (other.transform.tag == "Teleport")
        {
            transform.position = other.transform.GetComponent<Teleport>().target.position;
        }
        if (other.transform.tag == "Finish")
        {
            GameManager.Instance.Win();
        }
    }





    IEnumerator DelayedStartReturnToGameLevel()
    {
        yield return new WaitForSeconds(3f);
        transform.position = new Vector2(balloonStartPosition.x, balloonStartPosition.y + 10.0f);

        
        StartCoroutine(ReturnToGameLevel());
        GameManager.Instance.BonusLevelClouds(false);
    }








    private Vector2 balloonStartPosition;
    private Vector2 moveToTarget;

    void OnTriggerStay2D (Collider2D other)
    {
        if (other.tag == "PowerUpBalloon")
        {
            if (bButtonPressed || Input.GetButtonDown("B Button"))
            {
                flying = true;
                other.transform.parent = this.transform;
                other.transform.position = new Vector2(transform.position.x, transform.position.y + 1.3f);
                balloon = other.transform.gameObject;

                other.transform.GetComponent<BoxCollider2D>().enabled = false;

                if (other.transform.GetComponent<Collectables>().goToBonusLevel && !bonusLevel)
                {
                    moveToTarget = other.transform.GetComponent<Collectables>().bonusLevelTargetPosition.transform.position;
                    balloonStartPosition = other.transform.position;

                    bonusLevelFallingOrFlying = true;
                    StartCoroutine(GoToBonusLevel());
                    GameManager.Instance.BonusLevelClouds(true);
                }
            }
        }
    }


    float duration = 5f;

    IEnumerator GoToBonusLevel()
    {
        float time = 0;
        Vector2 startPosition = transform.position;

        cameraScript.followY = true;

        rigidbody.simulated = false;
        rigidbody.velocity = Vector3.zero;

        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, moveToTarget, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        bonusLevel = true;
        transform.position = moveToTarget;

        //  Camera - Reset
        cameraScript.followY = false;
        cameraScript.ChangeZoom(15f);

        bonusLevelFallingOrFlying = false;
        rigidbody.simulated = true;

        Jump(); Jump();

        GameManager.Instance.ShowMessage("BONUS LEVEL", 1.5f);
        GameManager.Instance.BonusLevelTimeCountdownStart(20, 2);
    }

    
    IEnumerator ReturnToGameLevel()
    {
        float time = 0;
        Vector2 startPosition = transform.position;

        cameraScript.followY = true;

        rigidbody.simulated = false;
        rigidbody.velocity = Vector3.zero;

        while (time < duration)
        {
            transform.position = Vector2.Lerp(startPosition, balloonStartPosition, time / duration);
            time += Time.deltaTime;
            yield return null;
        }

        //  Balloon-Flying - Reset
        transform.position = balloonStartPosition;

        balloon.transform.GetComponent<Collectables>().Reset();
        balloon.transform.GetComponent<Collectables>().Disable();
        flying = false;

        //  Camera - Reset
        cameraScript.ResetX();
        cameraScript.ResetY();
        cameraScript.ResetZoom();
        cameraScript.followY = false;

        bonusLevelFallingOrFlying = false;
        rigidbody.simulated = true;

        bonusLevel = false;
    }

    public void BonusLevelFail()
    {
        bonusLevel = false;    
        CameraScript.Instance.StartShake();
        CameraScript.Instance.StartColorHit();  
        bonusLevelFallingOrFlying = true;
        StartCoroutine(DelayedStartReturnToGameLevel());

        /*
        transform.position = new Vector2(balloonStartPosition.x, balloonStartPosition.y + 10.0f);

        bonusLevelFallingOrFlying = true;

        StartCoroutine(ReturnToGameLevel());

        GameManager.Instance.BonusLevelClouds(false);
        GameManager.Instance.PlayerRestart();
        */
    }

    
    IEnumerator DisablePowerUp(float delay)
    {
        yield return new WaitForSeconds(delay);
        spriteMask.transform.GetComponentInChildren<Animator>().SetBool("PowerUpActive", false);

        StartCoroutine(DisablePowerUpDelayed(1.5f)); 
    }

    IEnumerator DisablePowerUpDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        powerUpActive = false;
    }

    IEnumerator DelayedDisable(float delay, GameObject target)
    {
        yield return new WaitForSeconds(delay);
        target.SetActive(false);
    }

    void DisplayTime(float timeToDisplay)
    {
        timeToDisplay += 1;

        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timerText.text = timeRemaining.ToString("F0");
    }

    public void AddPoints(int p, Transform target)
    {
        score += p;

        if (score < 10) { scoreText.text = "00000" + score; }
        if (score >= 10) { scoreText.text = "0000" + score; }
        if (score >= 100) { scoreText.text = "000" + score; }
        if (score >= 1000) { scoreText.text = "00" + score; }
        if (score >= 10000) { scoreText.text = "0" + score; }
        if (score > 100000) { scoreText.text = "" + score; }

        SetHighScore();

        if (score >= highScore)
        {
            PlayerPrefs.SetInt("Highscore", score);

            SetHighScore();
        }

        GameObject effect = ObjectPooler.SharedInstance.GetPooledObject();
        if(effect != null)
        {
            effect.transform.position = new Vector2(Random.Range(target.position.x - 0.5f, target.position.x + 0.5f), Random.Range(target.position.y + 0.8f, target.position.y + 1.1f));
            effect.SetActive(true);
            effect.transform.GetComponentInChildren<TextMeshProUGUI>().text = p.ToString();
        }
    }

    public void SetHighScore()
    {
        highScore = PlayerPrefs.GetInt("Highscore");
        highScoreText.text = "" + highScore;

        if (highScore < 10) { highScoreText.text = "00000" + highScore; }
        if (highScore >= 10) { highScoreText.text = "0000" + highScore; }
        if (highScore >= 100) { highScoreText.text = "000" + highScore; }
        if (highScore >= 1000) { highScoreText.text = "00" + highScore; }
        if (highScore >= 10000) { highScoreText.text = "0" + highScore; }
        if (highScore > 100000) { highScoreText.text = "" + highScore; }
    }


    public void Lose()
    {
        lose = true;
        animator.SetTrigger("Lose");
        canMove = false;

        gameObject.layer = LayerMask.NameToLayer("PlayerHit");

        rigidbody.velocity = Vector3.zero;

        if (lifes > 0) { previouslyLifes -= 1; lifes -= 1; }
        UpdateLifes();

        CameraScript.Instance.StartShake();
        CameraScript.Instance.StartColorHit();

        Invoke("GameOver", 3f);
    }

    public void GameOver()
    {
        if (lifes == 0) { GameManager.Instance.GameOver(); }
        else
        {
            GameManager.Instance.ResetAll();
        }
    }

    public void Reset()
    {
        UpdateLifes();
        GameManager.Instance.PlayerRestart();

        //  Time & Scores
        timeRemaining = 300f;

        lose = false;
        timerIsRunning = true;


        //  Movement
        this.gameObject.SetActive(true);
        this.transform.position = startPosition;
        canMove = true;

        //  Graphics
        animator.SetTrigger("Reset");
        spriteRenderer.flipX = false;

        //  Balloon-Flying
        flying = false;
        cameraScript.ResetX();
        cameraScript.ResetY();
        GameManager.Instance.BonusLevelClouds(false);
        bonusLevel = false;

        //  Power UPs
        powerUpActive = false;
        spriteMask.transform.GetComponentInChildren<Animator>().SetBool("PowerUpActive", false);
        rigidbody.velocity = Vector3.zero;
        gameObject.layer = LayerMask.NameToLayer("Default");

        score = 0;
        if (score < 10) { scoreText.text = "00000" + score; }
        if (score >= 10) { scoreText.text = "0000" + score; }
        if (score >= 100) { scoreText.text = "000" + score; }
        if (score >= 1000) { scoreText.text = "00" + score; }
        if (score >= 10000) { scoreText.text = "0" + score; }
        if (score > 100000) { scoreText.text = "" + score; }
    }

    public void DebugTest()
    {
        CameraScript.Instance.StartShake();
        CameraScript.Instance.StartColorHit();
        PlayerPrefs.DeleteAll();
    }
} 