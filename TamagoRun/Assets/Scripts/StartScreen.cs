using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class StartScreen : MonoBehaviour
{
    public enum MenuType { Start, LevelSelection, Settings, Manual, Credits };
    public MenuType selectedMenu;

    public int selectedLevel;

    public GameObject startScreen, levelScreen, settingsScreen, manualScreen, creditsScreen;
    [Space(10)]

    public Button aButton;
    public Button bButton;
    public Button startButton;
    public Button dpadLeft, dpadRight;
    [Space(10)]

    public TextMeshProUGUI pressStartText;
    public Animator characterAnimation;
    [Space(10)]

    public GameObject lockedFlyAway;
    public GameObject selectIcon; private Transform target;
    public RectTransform level1, level2, level3;
    private int level1Collected, level2Collected, level3Collected;

    public TextMeshProUGUI collectedText;   //  Selected level's collectibles >  1/3
    public TextMeshProUGUI levelHighScoreText;  //  Selected level's highscore


    void Start()
    {
        Application.targetFrameRate = 60;   //Screen.SetResolution(1080, 2520, true);

        aButton.onClick.AddListener(AButtonPressed);
        bButton.onClick.AddListener(BButtonPressed);

        startButton.onClick.AddListener(StartButtonPressed);

        dpadLeft.onClick.AddListener(DPadLeftButtonPressed);
        dpadRight.onClick.AddListener(DPadRightButtonPressed);

        GameObject loadedFromLevel = GameObject.Find("LoadedFromGameLevel");
        if (loadedFromLevel)
        {
            startScreen.SetActive(false);
            levelScreen.SetActive(true);
            Destroy(loadedFromLevel);

            selectedMenu = MenuType.LevelSelection;
            selectedLevel = 2;

            Destroy(loadedFromLevel);
        }

        UpdateCollectibles();
    }

    void Update()
    {
        #if UNITY_STANDALONE_WIN || UNITY_EDITOR

        if (selectedMenu == MenuType.Start)
        {
            if (Input.anyKeyDown)
            {
                selectedMenu = MenuType.LevelSelection;

                pressStartText.gameObject.SetActive(false);
                characterAnimation.SetTrigger("Start");
                StartCoroutine(ChangeScreen(1, 1.5f));
            }
        }
        else if (selectedMenu == MenuType.LevelSelection)
        {
            if (Input.GetButtonDown("DPAD Left") && selectedLevel > 1) { selectedLevel -= 1; UpdateCollectibles(); }

            if (Input.GetButtonDown("DPAD Right") && selectedLevel < 3) { selectedLevel += 1; UpdateCollectibles(); }

            if (Input.GetButtonUp("A Button"))
            {
                AButtonPressed();
            }
            if (Input.GetButtonUp("B Button"))
            {
                BButtonPressed();
            }
        }

        #endif

        if (selectedLevel == 1) {   target = level1.transform;  }
        if (selectedLevel == 2) {   target = level2.transform;  }
        if (selectedLevel == 3) {   target = level3.transform;  }

        if (target)
        {
            selectIcon.GetComponent<RectTransform>().anchorMin = target.GetComponent<RectTransform>().anchorMin;
            selectIcon.GetComponent<RectTransform>().anchorMax = target.GetComponent<RectTransform>().anchorMax;
            selectIcon.GetComponent<RectTransform>().sizeDelta = target.GetComponent<RectTransform>().sizeDelta;

            Vector3 pos = new Vector3(0, 0, 0);
            selectIcon.transform.localPosition = pos;

            Vector3 offset = new Vector3(target.transform.localPosition.x, target.GetComponent<RectTransform>().sizeDelta.y + 45f, selectIcon.transform.localPosition.z);
            selectIcon.transform.localPosition = offset;
        }
    }

    IEnumerator ChangeScreen(int i, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);

        if (i == 0)
        {
            startScreen.SetActive(true);
            levelScreen.SetActive(false);

            selectedMenu = MenuType.Start;
        }
        if (i == 1) 
        {
            startScreen.SetActive(false);
            levelScreen.SetActive(true);

            selectedMenu = MenuType.LevelSelection; 
        }
    }

    void AButtonPressed()
    {
        if (selectedMenu == MenuType.Start)
        {
            StartCoroutine(ChangeScreen(1, 1.5f));
            pressStartText.gameObject.SetActive(false);
            characterAnimation.SetTrigger("Start");
        }
        else if (selectedMenu == MenuType.LevelSelection) { Invoke("DelayedStartGame", 0.75f); }


        /*  WORK in progress

        if (selectedLevel == 2)
        {
            target = level2.transform;
            Image locked = level2.transform.Find("Locked").transform.GetComponent<Image>();
            if (locked.transform.GetComponent<Image>().enabled == true)
            {
                lockedFlyAway.transform.parent = level2;
                lockedFlyAway.transform.position = new Vector2(0f, 0f);
                lockedFlyAway.transform.GetComponent<Animator>().SetTrigger("Fly"); lockedFlyAway.transform.GetComponent<Animator>().SetTrigger("Open");

                locked.enabled = false;
            }
        }
        
        if (selectedLevel == 3)
        {
            target = level3.transform;
            Image locked = level3.transform.Find("Locked").transform.GetComponent<Image>();
            if (locked.transform.GetComponent<Image>().enabled == true)
            {
                lockedFlyAway.transform.parent = level3;
                lockedFlyAway.transform.position = new Vector2(0f, 0f);
                lockedFlyAway.transform.GetComponent<Animator>().SetTrigger("Fly"); lockedFlyAway.transform.GetComponent<Animator>().SetTrigger("Open");

                locked.enabled = false;
            }
        }
        */
    }

    void BButtonPressed()
    {
        if (selectedMenu == MenuType.Start)
        {}
        else if (selectedMenu == MenuType.LevelSelection) { StartCoroutine(ChangeScreen(0, 0f)); pressStartText.gameObject.SetActive(true); }
    }

    void StartButtonPressed()
    {
        if (selectedMenu == MenuType.Start) 
        { 
            StartCoroutine(ChangeScreen(1, 1.5f));
            pressStartText.gameObject.SetActive(false);
            characterAnimation.SetTrigger("Start");
        }
        else if (selectedMenu == MenuType.LevelSelection) { Invoke("DelayedStartGame", 0.75f); }
    }

    void DPadLeftButtonPressed()
    {
        if (selectedLevel > 1) { selectedLevel -= 1; UpdateCollectibles(); }
    }
    void DPadRightButtonPressed()
    {
        if (selectedLevel < 3) { selectedLevel += 1; UpdateCollectibles(); }
    }

    void UpdateCollectibles()
    {
        int collectedTotal = 0;

        int a = PlayerPrefs.GetInt("Collectible_" + selectedLevel + "_1");
        int b = PlayerPrefs.GetInt("Collectible_" + selectedLevel + "_2");
        int c = PlayerPrefs.GetInt("Collectible_" + selectedLevel + "_3");
        print (selectedLevel + "- A:" + a + " B:" + b + " C:" + c);

        collectedTotal = a + b + c;
        collectedText.text = "" + collectedTotal + " / " + "3";

        levelHighScoreText.text = "Hiscore " + PlayerPrefs.GetInt("Highscore_" + selectedLevel);
    }

    void DelayedStartGame()
    {
        SceneManager.LoadScene(selectedLevel, LoadSceneMode.Single);
    }
}