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

    // Players
    [SerializeField]
    Fellow player;
    [SerializeField]
    FPFellow minigamePlayer;

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
    private GameObject[] pelletsL1, pelletsL2, pelletsL3, pelletsFP;
    public GameObject[] pellets;
    private List<GameObject[]> allPellets = new List<GameObject[]>();

    // Level
    public int level;
    public int maze;
    [SerializeField]
    GameObject levelText;

    // Timers
    public float scatterTime, chaseTime;
    private int stage = 1; // Determines which stage of mode the game should be in

    // Music
    AudioSource[] audioClips;
    AudioSource menuMusic;

    // Camera
    [SerializeField]
    CameraMovement cameraMovement;
    [SerializeField]
    MouseLook mouseLook;

    // Ghosts
    GameObject[] ghosts;

    enum GameMode
    {
        InGame,
        InMinigame,
        MainMenu,
        HighScores,
        Paused
    }

    GameMode gameMode = GameMode.MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        audioClips = GetComponents<AudioSource>();
        menuMusic = audioClips[0];

        menuMusic.Play(0);

        // Declare pellets for each maze and add to list to reference when changing level
        pelletsL1 = GameObject.FindGameObjectsWithTag("L1Pellet");
        pelletsL2 = GameObject.FindGameObjectsWithTag("L2Pellet");
        pelletsL3 = GameObject.FindGameObjectsWithTag("L3Pellet");
        pelletsFP = GameObject.FindGameObjectsWithTag("L0Pellet");
        allPellets.Add(pelletsL1);
        allPellets.Add(pelletsL2);
        allPellets.Add(pelletsL3);
        allPellets.Add(pelletsFP);

        // Define all ghost characters
        ghosts = GameObject.FindGameObjectsWithTag("Ghost");

        // When game starts the user will play from level 1, so set pellets to first maze pellets
        // and set level to 1.
        pellets = pelletsL1;
        level = 1;
        maze = 1;

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

        if (player.PelletsEaten() == pellets.Length || minigamePlayer.PelletsEaten() == pellets.Length)
        {
            gameMode = GameMode.Paused;
            winUI.gameObject.SetActive(true);
        }

        if (InGame())
        {
            // Global timers for scatter and chase mode
            scatterTime = Mathf.Max(0.0f, scatterTime - Time.deltaTime); // 7 seconds of scatter mode based on documentation given on ghost behaviour

            if (scatterTime <= 0.0f)
            {
                if (stage < 4)
                {
                    chaseTime = Mathf.Max(0.0f, chaseTime - Time.deltaTime);
                }
                else if (stage == 4)
                {
                    chaseTime = 1; // Once "stage 4" of mode is reached, permanently stay in chase mode
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
            if (Input.GetKeyDown(KeyCode.Return))
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
        pauseUI.gameObject.SetActive(true);
        GameObject pausedText = GameObject.Find("User Interface/PauseUI/PausedText");
        Time.timeScale = 0;
        while (true)
        {
            // Using "WaitForSecondsRealtime" to avoid problem of pausing game time so that the
            // blinking text can still happen
            pausedText.SetActive(true);
            yield return new WaitForSecondsRealtime(0.3f);
            pausedText.SetActive(false);
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
        StartCoroutine(StartCountdown(GameMode.InGame));
    }

    public void StartFPMinigame()
    {
        menuMusic.Stop();

        StartCoroutine(cameraMovement.AttachCameraToFellow());

        // Set pellets to first person maze
        pellets = allPellets[3];
        maze = 0;

        // Change ghosts position to minigame maze
        foreach (GameObject ghost in ghosts)
        {
            GhostInterface ghostInterface = ghost.GetComponent<GhostInterface>();
            // Ghost will not move position unless nav mesh agent is inactive so temporarily disable
            ghostInterface.SetNavMeshAgent(false);
            Vector3 ghostStartPos = ghost.GetComponent<GhostInterface>().GetStartPos();
            Vector3 newGhostStartPos = new Vector3(ghostStartPos.x - 31.0f, ghostStartPos.y, ghostStartPos.z);
            ghostInterface.SetStartPos(newGhostStartPos);
            ghostInterface.ResetGhost();
            ghostInterface.SetPlayerTarget(minigamePlayer.GetComponent<FellowInterface>());
            ghostInterface.SetSpeed(1.5f);
            ghostInterface.SetNavMeshAgent(true);

        }

        gameUI.gameObject.SetActive(true);
        mouseLook.enabled = true;

        // Start countdown UI
        countdownUI.gameObject.SetActive(true);
        StartCoroutine(StartCountdown(GameMode.InMinigame));
    }

    private IEnumerator StartCountdown(GameMode mode)
    {
        // If player is not on the first level, wait before countdown to allow time for
        // camera to pan over to next level
        if (level != 1)
        {
            yield return new WaitForSeconds(1.2f);
        }

        GameObject countdownPanel = GameObject.Find("CountdownPanel");
        if (mode == GameMode.InGame)
        {
            countdownUI.transform.localScale = Vector3.one * 0.1f; // Set size to fit maze
            countdownPanel.SetActive(true);
        } 
        else if (mode == GameMode.InMinigame)
        {
            countdownUI.transform.localScale = Vector3.one * 0.005f; // Set size to fit infront of player
            countdownPanel.SetActive(false); // Disable panel from countdown UI for first person view
        }
        

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
        audioClips[maze].Play(0);
        gameMode = mode;

        countdownUI.gameObject.SetActive(false);
    }

    public void NextLevel()
    {
        audioClips[maze].Stop();

        winUI.gameObject.SetActive(false);
        gameMode = GameMode.Paused;

        // Reset fellow properties
        if (player.lives < 3) // Give player an extra life between levels as long as it's less than 3
        {
            player.lives++;
        }
        player.pelletsEaten = 0;
        player.powerupTime = 0;
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Show lives UI
        Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
        for (int i = 0; i <= player.lives - 1; i++)
        {
            livesUI.transform.GetChild(i).localScale = lifeSize;
        }

        // Next level, unless reached last level, then return to first maze
        int previousMaze = maze; // Set previous level before increment for camera movement
        if ((maze % 3) == 0)
        {
            maze = 1;
        }
        else
        {
            maze++;
        }
        level++;
        pellets = allPellets[maze - 1];

        // Set all pellets to be active incase maze has already been played
        foreach (GameObject pellet in pellets)
        {
            pellet.SetActive(true);
        }

        // Reset timers
        scatterTime = 7.0f;
        chaseTime = 20.0f;

        // Move camera to next level
        StartCoroutine(cameraMovement.MoveCameraToLevel(previousMaze, maze));

        ResetCharacters(maze, previousMaze);

        StartGame();
    }

    public void ExitGame()
    {
        audioClips[maze].Stop();

        Time.timeScale = 1;

        // Reset fellow properties
        player.lives = 3;
        player.pelletsEaten = 0;
        player.powerupTime = 0;
        player.SetScore(0);
        player.GetComponent<Rigidbody>().velocity = Vector3.zero;

        // Show lives UI
        Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
        for (int i = 0; i <= player.lives - 1; i++)
        {
            livesUI.transform.GetChild(i).localScale = lifeSize;
        }

        ResetCharacters(1, maze);

        // If player is not on the first maze then move camera back
        if (maze != 1)
        {
            cameraMovement.ReturnToStart();
        }

        level = 1;
        maze = 1;

        pauseUI.SetActive(false);
        gameUI.SetActive(false);
        mainMenuUI.SetActive(true);
        GetComponent<UIFader>().FadeIn(2f);

        menuMusic.Play(0);

        gameMode = GameMode.MainMenu;
    }

    private void ResetCharacters(int nextMaze, int previousMaze)
    {
        // Move fellow and to next maze and change start position
        Vector3 startPos = player.GetComponent<Fellow>().GetStartPos();
        Vector3 newStartPos = new Vector3(startPos.x + ((nextMaze - previousMaze) * 31.0f), startPos.y, startPos.z);
        player.SetStartPos(newStartPos);
        player.transform.position = newStartPos;

        // Reset ghosts position to next maze

        foreach (GameObject ghost in ghosts)
        {
            GhostInterface ghostInterface = ghost.GetComponent<GhostInterface>();
            ghostInterface.SetNavMeshAgent(false); // Ghost will not move position unless nav mesh agent is inactive so temporarily disable
            Vector3 ghostStartPos = ghost.GetComponent<GhostInterface>().GetStartPos();
            Vector3 newGhostStartPos = new Vector3(ghostStartPos.x + ((nextMaze - previousMaze) * 31.0f), ghostStartPos.y, ghostStartPos.z);
            ghostInterface.SetStartPos(newGhostStartPos);
            ghostInterface.ResetGhost();
            ghostInterface.SetPlayerTarget(player.GetComponent<FellowInterface>());
            ghostInterface.SetSpeed(3.5f);
            ghostInterface.SetNavMeshAgent(true);
        }
    }

    public bool InGame()
    {
        return gameMode == GameMode.InGame || gameMode == GameMode.InMinigame;
    }

    public bool InMinigame()
    {
        return gameMode == GameMode.InMinigame;
    }

    public int CurrentLevel()
    {
        return level;
    }

    public int CurrentMaze()
    {
        return maze;
    }

    public void ShowSaveUI()
    {
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(true);
    }

    public void SaveScore()
    {
        string name = GameObject.Find("NameText").GetComponent<Text>().text;
        int score = player.GetScore();

        highScoreUI.GetComponent<HighScoreTable>().SaveScore(name, score);

        saveUI.gameObject.SetActive(false);
        mainMenuUI.gameObject.SetActive(true);

        cameraMovement.ReturnToStart();
    }

    public int GetCurrentTotalPellets()
    {
        return pellets.Length;
    }

    public void SetVolumeOfMusic(float volume)
    {
        audioClips[maze].volume = volume;
    }
}
