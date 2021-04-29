using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YellowFellowGame : MonoBehaviour
{
    // UI Elements
    [SerializeField]
    GameObject highScoreUI;
    [SerializeField]
    GameObject mainMenuUI;
    [SerializeField]
    GameObject gameUI;
    [SerializeField]
    GameObject livesUI;
    [SerializeField]
    GameObject winUI;
    [SerializeField]
    GameObject pauseUI;
    [SerializeField]
    GameObject saveUI;

    [SerializeField]
    Fellow playerObject;

    [SerializeField]
    GameObject ghostHouse;

    // Countdown
    [SerializeField]
    GameObject countdownUI;
    [SerializeField]
    GameObject countdownNumber;
    AudioSource countdownMusic;

    // Pellets
    [SerializeField]
    private GameObject[] pelletsL1, pelletsL2, pelletsL3;
    public GameObject[] pellets;
    private List<GameObject[]> allPellets = new List<GameObject[]>();

    // Level
    public int level;
    [SerializeField]
    GameObject levelText;

    // Timers
    public float scatterTime, chaseTime;
    private int stage = 1; // Determines which stage of mode the game should be in

    // Music
    AudioSource menuMusic, inGameMusic;

    // Camera
    [SerializeField]
    GameObject cameraObject;

    enum GameMode
    {
        InGame,
        MainMenu,
        HighScores,
        Paused
    }

    GameMode gameMode = GameMode.MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        AudioSource[] audio = GetComponents<AudioSource>();
        menuMusic = audio[0];
        inGameMusic = audio[1];

        // Declare pellets for each maze and add to list to reference when changing level
        pelletsL1 = GameObject.FindGameObjectsWithTag("L1Pellet");
        pelletsL2 = GameObject.FindGameObjectsWithTag("L2Pellet");
        pelletsL3 = GameObject.FindGameObjectsWithTag("L3Pellet");
        allPellets.Add(pelletsL1);
        allPellets.Add(pelletsL2);
        allPellets.Add(pelletsL3);

        // When game starts the user will play from level 1, so set pellets to first maze pellets
        // and set level to 1.
        pellets = pelletsL1;
        level = 1;

        StartMainMenu();
    }

    // Update is called once per frame
    void Update()
    {
        switch(gameMode)
        {
            case GameMode.MainMenu:     UpdateMainMenu(); break;
            case GameMode.HighScores:   UpdateHighScores(); break;
            case GameMode.InGame:       UpdateMainGame(); break;
            case GameMode.Paused:       UpdatePauseMenu(); break;
        }

        if (playerObject.PelletsEaten() == pellets.Length)
        {
            winUI.gameObject.SetActive(true);
        }

        if (gameMode == GameMode.InGame)
        {
            // Global timers for scatter and chase mode
            scatterTime = Mathf.Max(0.0f, scatterTime - Time.deltaTime); // 7 seconds of scatter mode based on documentation given on ghost behaviour

            if (gameMode == GameMode.InGame && scatterTime <= 0.0f)
            {
                if (stage < 4)
                {
                    chaseTime = Mathf.Max(0.0f, chaseTime - Time.deltaTime);
                }
                else if (stage == 4)
                {
                    chaseTime = 1; // Once "stage 5" of mode is reached, permanently stay in chase mode
                }

                if (chaseTime <= 0.0f)
                {
                    if (stage <= 2)
                    {
                        scatterTime = 7.0f;
                        chaseTime = 20.0f;
                    }
                    else if (stage <= 4)
                    {
                        scatterTime = 5.0f;
                        chaseTime = 20.0f;
                    }
                    stage++;
                }
            }
        }

        levelText.GetComponent<Text>().text = level.ToString();
    }

    void UpdateMainMenu()
    {
        if (!gameUI.gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                StartGame();
            }
            else if (Input.GetKeyDown(KeyCode.Return))
            {
                StartHighScores();
            }
        }
    }

    void UpdateHighScores()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartMainMenu();
        }
    }

    void UpdatePauseMenu()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            RemovePauseMenu();
        }
    }

    void UpdateMainGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartCoroutine(StartPauseMenu());
        }
    }

    void StartMainMenu()
    {
        gameMode                        = GameMode.MainMenu;
        mainMenuUI.gameObject.SetActive(true);
        GetComponent<UIFader>().FadeIn(2f);
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(false);
    }


    public void StartHighScores()
    {
        gameMode                = GameMode.HighScores;
        mainMenuUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(false);
    }

    IEnumerator StartPauseMenu()
    {
        gameMode                = GameMode.Paused;
        Time.timeScale = 0;
        while (true)
        {
            // Using "WaitForSecondsRealtime" to avoid problem of pausing game time so that the
            // blinking text can still happen
            GameObject.Find("PausedText").SetActive(true);
            yield return new WaitForSecondsRealtime(0.3f);
            GameObject.Find("PausedText").SetActive(false);
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    void RemovePauseMenu()
    {
        gameMode = GameMode.InGame;
        pauseUI.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void StartGame()
    {
        menuMusic.Stop();
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);

        // Fade out menu
        GetComponent<UIFader>().FadeOut(0.4f);

        // Based on ghost behaviour document given, initialise timers for different modes.
        scatterTime = 7.0f;
        chaseTime = 20.0f;

        // Start countdown UI
        countdownUI.gameObject.SetActive(true);
        StartCoroutine(StartCountdown());
    }

    private IEnumerator StartCountdown()
    {
        UIFader[] uiFaders = countdownUI.GetComponents<UIFader>();
        UIFader number = uiFaders[0];
        UIFader background = uiFaders[1];
        background.FadeIn(0.2f);

        yield return new WaitForSeconds(0.5f); // Wait for main menu to fade
        mainMenuUI.gameObject.SetActive(false);

        countdownMusic = countdownNumber.GetComponent<AudioSource>();
        countdownMusic.Play(0);

        Vector3 originalScale = countdownNumber.transform.localScale;
        Vector3 targetScale = countdownNumber.transform.localScale * 1.2f;

        for (int i = 3; i >= 0; i--)
        {
            if (i >= 1)
            {
                countdownNumber.GetComponent<Text>().text = i.ToString();
            }
            else
            {
                countdownNumber.GetComponent<Text>().text = "GO!";
                background.FadeOut(0.5f);
            }

            float time = 0.4f;
            float originalTime = time;

            number.FadeIn(0.5f);

            // Number slowly increases in size before fading out and changing number
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                countdownNumber.transform.localScale = Vector3.Lerp(targetScale, originalScale, time / originalTime);
                yield return new WaitForEndOfFrame();
            }

            number.FadeOut(0.5f);

            yield return new WaitForSeconds(0.5f);
        }

        // Start the game mechanics
        inGameMusic.Play(0);
        gameMode = GameMode.InGame;

        countdownUI.gameObject.SetActive(false);
    }

    public bool InGame()
    {
        return gameMode == GameMode.InGame;
    }

    public int CurrentLevel()
    {
        return level;
    }

    public void NextLevel()
    {
        winUI.gameObject.SetActive(false);

        // Reset fellow properties
        playerObject.lives = 3;
        playerObject.pelletsEaten = 0;
        playerObject.powerupTime = 0;

        // Show lives UI
        Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
        for (int i = 0; i <=2; i++)
        {
            livesUI.transform.GetChild(i).localScale = lifeSize;
        }

        // Next level
        level++;
        pellets = allPellets[level - 1];

        // Reset timers
        scatterTime = 7.0f;
        chaseTime = 20.0f;

        // Move camera to next level
        StartCoroutine(cameraObject.GetComponent<CameraMovement>().MoveCameraToNextLevel());

        // Move fellow and to next maze and change start position
        Vector3 startPos = playerObject.GetComponent<Fellow>().GetStartPos();
        Vector3 newStartPos = new Vector3(startPos.x + 31f, startPos.y, startPos.z);
        playerObject.SetStartPos(newStartPos);
        playerObject.transform.position = newStartPos;

        // Reset ghosts position to next maze
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in ghosts)
        {
            // Ghost will not move position unless nav mesh agent is inactive so temporarily disable
            ghost.GetComponent<GhostInterface>().SetNavMeshAgent(false);
            Vector3 ghostStartPos = ghost.GetComponent<GhostInterface>().GetStartPos();
            Vector3 newGhostStartPos = new Vector3(ghostStartPos.x + 31f, ghostStartPos.y, ghostStartPos.z);
            ghost.GetComponent<GhostInterface>().SetStartPos(newGhostStartPos);
            ghost.GetComponent<GhostInterface>().ResetGhost();
            ghost.GetComponent<GhostInterface>().SetNavMeshAgent(true);
        }

        StartGame();
    }

    public void ShowSaveUI()
    {
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(true);
    }

    public void SaveScore()
    {
        string name = GameObject.Find("NameText").GetComponent<Text>().text;
        int score = playerObject.GetComponent<Fellow>().GetScore();

        highScoreUI.GetComponent<HighScoreTable>().SaveScore(name, score);

        saveUI.gameObject.SetActive(false);
        mainMenuUI.gameObject.SetActive(true);

        cameraObject.GetComponent<CameraMovement>().ReturnToStart();
    }

    public void StartFPMinigame()
    {
        StartCoroutine(cameraObject.GetComponent<CameraMovement>().AttachCameraToFellow());

        cameraObject.GetComponent<MouseLook>().enabled = true;
    }
}
