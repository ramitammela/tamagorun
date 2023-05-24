using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public int gameLevel;

    private PlayerScript player;
    private Button aButton, bButton, startButton;
    private GameObject winScreen, loseScreen, pauseScreen;
    private TextMeshProUGUI totalScoreText;
    private TextMeshProUGUI textMessage, textBonusLevelTimer;
    private bool gameOver, win, pause, bonusLevel;

    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
                Debug.Log("Game Manager is null");

            return _instance;
        }
    }


    private void Awake()
    {
        _instance = this;
    }

    void Start()
    {
        player = (PlayerScript)FindObjectOfType(typeof(PlayerScript));

        //  A & B button - Controls for Win & lose Screen
        aButton = GameObject.Find("Button_A").transform.GetComponent<Button>();
        bButton = GameObject.Find("Button_B").transform.GetComponent<Button>();
        aButton.onClick.AddListener(AButtonPressed);
        bButton.onClick.AddListener(BButtonPressed);

        //  Start button - Controls Pause Menu
        startButton = GameObject.Find("Button_Start").transform.GetComponent<Button>();
        startButton.onClick.AddListener(StartButtonPressed);

        //  Win & Lose      + Pause Screen
        winScreen = GameObject.Find("Panel_Win");
        loseScreen = GameObject.Find("Panel_GameOver");
        pauseScreen = GameObject.Find("Panel_Pause");
        totalScoreText = GameObject.Find("Text_LevelClear_Scores").transform.GetComponent<TextMeshProUGUI>();
        winScreen.SetActive(false);
        loseScreen.SetActive(false);
        pauseScreen.SetActive(false);

        textMessage = GameObject.Find("Text_Message").transform.GetComponent<TextMeshProUGUI>();
        textBonusLevelTimer = GameObject.Find("Text_BonusLevel_Timer").transform.GetComponent<TextMeshProUGUI>();
    }

    void Update()
    {
        //  Controls
        if (gameOver)
        {
            if (Input.GetButtonDown("A Button"))
            {
                AButtonPressed();
            }
            else if (Input.GetButtonDown("B Button"))
            {
                BButtonPressed();
            }
        }

        if (win)
        {
            if (Input.GetButtonDown("A Button"))
            {
                AButtonPressed();
            }
            else if (Input.GetButtonDown("B Button"))
            {
                BButtonPressed();
            }
        }

        if (pause)
        {
            if (Input.GetButtonDown("A Button"))
            {
                AButtonPressed();
            }
            else if (Input.GetButtonDown("B Button"))
            {
                BButtonPressed();
            }
        }

        if (Input.GetButtonDown("Start Button"))
        {
            if (win || gameOver)
                return;

            StartButtonPressed();
        }

        bonusLevel = player.bonusLevel;
    }

    public void GameOver ()
    {
        gameOver = true;
        loseScreen.SetActive(true);

        Time.timeScale = 0; 
    }

    public void Win()
    {
        // Scores Texts
        int timeBonus = Mathf.FloorToInt(player.timeRemaining) * 100;
        int lifesBonus = player.lifes * 10000;
        int totalScore = timeBonus + lifesBonus + player.score;
        totalScoreText.text = "Time Bonus - " + timeBonus + "\n" + "Life Bonus - " + lifesBonus + "\n" + "TOTAL - " + totalScore;

        //  Get & Set Highscore
        int previousHighscore = PlayerPrefs.GetInt("Highscore_" + gameLevel);

        if (totalScore >= previousHighscore)
        {
            PlayerPrefs.SetInt("Highscore_" + gameLevel, totalScore);

            totalScoreText.text = "Time Bonus - " + timeBonus + "\n" + "Life Bonus - " + lifesBonus + "\n" + "TOTAL - " + totalScore + " !!" + "\n" + "\n" + "*NEW HIGHSCORE!*";
        }

        win = true;
        winScreen.SetActive(true);

        Time.timeScale = 0;
    }

    public void Pause (bool p)
    {
        if (p) 
        {
            Time.timeScale = 0;
            pause = true;
            pauseScreen.SetActive(true);
        }
        else
        {
            Time.timeScale = 1;
            pause = false;
            pauseScreen.SetActive(false);
        }
    }

    public void AButtonPressed()
    {
        if (win)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Start", LoadSceneMode.Single);
        }
        if (gameOver)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
        if (pause)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
    }
    public void BButtonPressed()
    {
        if (win)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex, LoadSceneMode.Single);
        }
        if (gameOver)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Start", LoadSceneMode.Single);
        }
        if (pause)
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("Start", LoadSceneMode.Single);
        }
    }
    public void StartButtonPressed()
    {   
        if (win || gameOver)
            return;

        pause = !pause;

        if (pause) { Pause(true); }
        else { Pause(false); }
    }

    public void BonusLevelClouds(bool activate)
    {
        GameObject clouds = GameObject.Find("Clouds_BonusLevel");
        clouds.transform.GetComponent<Animator>().SetBool("Activate", activate);
    }

    public void ShowMessage(string message, float time)
    {
        textMessage.enabled = true;
        textMessage.text = "" + message;
        StartCoroutine(DelayedDisable(time));
    }
    IEnumerator DelayedDisable(float delay)
    {
        yield return new WaitForSeconds(delay);
        textMessage.enabled = false;
    }

    public void BonusLevelTimeCountdownStart(int time, float startDelay) // 3.. 2... 1... Time Out
    {
        StartCoroutine(BonusLevelTimeCountdown(time, startDelay));
    }
    IEnumerator BonusLevelTimeCountdown(int time, float startDelay)
    {
        yield return new WaitForSeconds(startDelay);

        //textMessage.enabled = true;
        textBonusLevelTimer.enabled = true;

        int counter = time;

        while (counter > 0 && bonusLevel)
        {
            textBonusLevelTimer.text = "" + counter;
            yield return new WaitForSeconds(1);

            counter--;

            if (playerReset) { textBonusLevelTimer.enabled = false; counter = 0; }
        }

        if (bonusLevel)
        {   textBonusLevelTimer.text = "BONUS LEVEL" + "\n" + "Time Out!";  }
        

        if (playerReset)
        {
            //textMessage.enabled = false;
            yield return null;
        }
        else
        {
            player.BonusLevelFail();
            yield return new WaitForSeconds(1);
        }

        textBonusLevelTimer.enabled = false;
    }


    bool playerReset;

    public void PlayerRestart()
    { 
        playerReset = true;

        StopCoroutine(BonusLevelTimeCountdown(0,0));

        StartCoroutine(PlayerResetDelay()); 
    }
    IEnumerator PlayerResetDelay()
    {
        yield return new WaitForSeconds(1);
        playerReset = false;
    }

    public void ResetAll()
    {
        BroadcastMessage("Reset", 0f);
    }
}