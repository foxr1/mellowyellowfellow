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
    GameObject minigameLivesUI;
    [SerializeField]
    GameObject winUI;
    [SerializeField]
    GameObject gameOverUI;
    [SerializeField]
    GameObject pauseUI;
    [SerializeField]
    GameObject saveUI;
    [SerializeField]
    GameObject countdownUI;

    // Panels and text elements
    [SerializeField]
    GameObject pausePanel;
    [SerializeField]
    GameObject pausedText;
    [SerializeField]
    GameObject countdownPanel;
    [SerializeField]
    GameObject winPanel;
    [SerializeField]
    GameObject continueText;
    [SerializeField]
    GameObject gameOverPanel;

    // Players
    [SerializeField]
    Fellow player;
    [SerializeField]
    FPFellow minigamePlayer;
    int score = 0;

    // Ghosts
    GameObject[] ghosts;
    [SerializeField]
    GameObject ghostHouse;

    // Countdown
    [SerializeField]
    GameObject countdownNumber;
    AudioSource countdownMusic;

    // Collectables
    [SerializeField]
    private GameObject[] pelletsL1, pelletsL2, pelletsL3, pelletsFP;
    public GameObject[] pellets;
    private List<GameObject[]> allPellets = new List<GameObject[]>();
    private GameObject[] powerups;
    private GameObject minimap, cherry;

    // Level
    public int level, maze, previousMaze;
    [SerializeField]
    GameObject levelText;

    // Timers
    public float scatterTime, chaseTime;
    private int stage = 1; // Determines which stage of scatter/chase mode the game should be in

    // Music
    AudioSource[] audioClips;
    AudioSource menuMusic;

    // Cameras
    [SerializeField]
    CameraMovement cameraMovement;
    [SerializeField]
    MouseLook mouseLook;
    [SerializeField]
    GameObject minimapCamera;

    private GameMode currentMode; // Used for pausing the game to hold a temporary value defining the current mode

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
        menuMusic = audioClips[4];

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

        // Declare collectables for when level needs to be reset
        powerups = GameObject.FindGameObjectsWithTag("Powerup");
        minimap = GameObject.FindGameObjectWithTag("Minimap");
        cherry = GameObject.FindGameObjectWithTag("Cherry");

        // Declare all ghost characters
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
            case GameMode.InMinigame:   UpdateMainGame(); break;
            case GameMode.Paused:       UpdatePauseMenu(); break;
        }

        // If player wins the game
        if (player.PelletsEaten() == pellets.Length || minigamePlayer.PelletsEaten() == pellets.Length)
        {
            if (gameMode == GameMode.InMinigame)
            {
                minimapCamera.SetActive(false);
                mouseLook.enabled = false;
                Cursor.lockState = CursorLockMode.None;
                winPanel.SetActive(false);

                // Only one level for minigame so change "Continue?" to "Restart?"
                continueText.GetComponent<Text>().text = "Restart?"; 
            } 
            else if (gameMode == GameMode.InGame)
            {
                winPanel.SetActive(true);
                continueText.GetComponent<Text>().text = "Continue?";
            }
            
            gameMode = GameMode.Paused;
            winUI.gameObject.SetActive(true);

            // Stop characters from moving
            Time.timeScale = 0; 
        }

        // Scatter and chase mode stages for all games.
        if (InAnyGame())
        {
            // 7 seconds of scatter mode based on documentation given on ghost behaviour
            scatterTime = Mathf.Max(0.0f, scatterTime - Time.deltaTime); 

            if (scatterTime <= 0.0f)
            {
                if (stage < 4)
                {
                    // 20 seconds of chase mode based on documentation given on ghost behaviour
                    chaseTime = Mathf.Max(0.0f, chaseTime - Time.deltaTime); 
                }
                else if (stage == 4)
                {
                    // Once "stage 4" of mode is reached, permanently stay in chase mode
                    chaseTime = 1; 
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
    }

    void UpdateMainMenu()
    {
        // Only when main menu is active can you quit game
        if (mainMenuUI.activeSelf)
        {
            // Quit game by pressing escape on main menu
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }
        }
    }

    void UpdateHighScores()
    {
        // Exit high scores by pressing escape
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            StartMainMenu();
        }
    }

    void UpdatePauseMenu()
    {
        // Game is in "paused" state when countdown is active so make sure countdown UI is not active before executing
        if (!countdownUI.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                if (currentMode == GameMode.InMinigame)
                {
                    // Lock cursor to center of screen for minigame
                    Cursor.lockState = CursorLockMode.Locked; 
                }
                RemovePauseMenu();
            }
        }
    }

    void UpdateMainGame()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            // Make cursor visible when in minigame
            Cursor.lockState = CursorLockMode.None; 
            StartCoroutine(StartPauseMenu());
        }
    }

    void StartMainMenu()
    {
        gameMode = GameMode.MainMenu;
        highScoreUI.GetComponent<UIFader>().FadeOut(0.4f, highScoreUI.gameObject, true);
        GetComponent<UIFader>().FadeIn(1.6f, mainMenuUI.gameObject);
        gameUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(false);
    }


    public void StartHighScores()
    {
        gameMode = GameMode.HighScores;
        GetComponent<UIFader>().FadeOut(0.4f, mainMenuUI.gameObject, true);
        highScoreUI.GetComponent<UIFader>().FadeIn(0.4f, highScoreUI.gameObject);
        gameUI.gameObject.SetActive(false);
        pauseUI.gameObject.SetActive(false);
        winUI.gameObject.SetActive(false);
        saveUI.gameObject.SetActive(false);
    }

    IEnumerator StartPauseMenu()
    {
        currentMode = gameMode;
        pauseUI.gameObject.SetActive(true);
        pausePanel.SetActive(!(currentMode == GameMode.InMinigame)); // Disable panel when in minigame as it is not needed

        gameMode = GameMode.Paused;

        Time.timeScale = 0;
        while (true)
        {
            /* Using "WaitForSecondsRealtime" to avoid problem of pausing game time so that the
            blinking text can still happen */
            pausedText.SetActive(true);
            yield return new WaitForSecondsRealtime(0.3f);
            pausedText.SetActive(false);
            yield return new WaitForSecondsRealtime(0.3f);
        }
    }

    void RemovePauseMenu()
    {
        gameMode = currentMode;
        pauseUI.gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    public void ShowSaveUI()
    {
        Time.timeScale = 1;

        // Stop level music and play main menu music
        audioClips[maze].Stop();
        menuMusic.Play(0);

        // If in minigame, reset minigame and animate camera back to main screen for saving score
        if (currentMode == GameMode.InGame)
        {
            score = player.GetScore();
            ResetGame();
        }
        else if (currentMode == GameMode.InMinigame)
        {
            score = minigamePlayer.GetScore();
            ResetMinigame();
            cameraMovement.SetInMinigame(false);
            cameraMovement.ResetUIPositions();
        }
        StartCoroutine(cameraMovement.ReturnToStart());

        player.pelletsEaten = 0; // Set pellets eaten to 0 to stop winUI from being shown in Update function

        winUI.gameObject.SetActive(false);
        gameOverUI.SetActive(false);
        saveUI.gameObject.SetActive(true);
    }

    public void SaveScore()
    {
        audioClips[maze].Stop();

        string name = GameObject.Find("NameText").GetComponent<Text>().text;
        string saveMode = "";
        if (currentMode == GameMode.InGame)
        {
            saveMode = "game";
        }
        else if (currentMode == GameMode.InMinigame)
        {
            saveMode = "minigame";
        }

        highScoreUI.GetComponent<HighScoreTable>().SaveScore(name, score, saveMode);

        saveUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
        GetComponent<UIFader>().FadeIn(0.4f, mainMenuUI.gameObject); // Fade in menu

        if (maze != 1)
        {
            StartCoroutine(cameraMovement.ReturnToStart());
        }

        player.SetScore(0);
        ResetCharacters(1, maze);

        // Set pellets to first maze, reset level and maze back to 1 also.
        pellets = allPellets[0];
        level = 1;
        maze = 1;

        menuMusic.Play(0);
    }

    public void ShowGameOverUI()
    {
        if (gameMode == GameMode.InMinigame)
        {
            minimapCamera.SetActive(false);
            mouseLook.enabled = false;
            Cursor.lockState = CursorLockMode.None;
            gameOverPanel.SetActive(false);
        }
        else if (gameMode == GameMode.InGame)
        {
            gameOverPanel.SetActive(true);
        }

        gameMode = GameMode.Paused;
        gameOverUI.SetActive(true);
    }

    public void StartGame()
    {
        menuMusic.Stop();
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);
        gameUI.GetComponent<UIFader>().FadeIn(0.4f, gameUI.gameObject);

        // Load highest score
        highScoreUI.GetComponent<HighScoreTable>().SetHighScore();

        // Set level
        levelText.GetComponent<Text>().text = level.ToString();

        // Fade out menu
        GetComponent<UIFader>().FadeOut(0.4f, mainMenuUI.gameObject, true);

        // Based on ghost behaviour document given, initialise timers for different modes.
        scatterTime = 7.0f;
        chaseTime = 20.0f;

        // Start countdown UI
        countdownUI.gameObject.SetActive(true);
        currentMode = GameMode.InGame;
        StartCoroutine(StartCountdown(GameMode.InGame));
    }

    public void StartFPMinigame()
    {
        menuMusic.Stop();

        Cursor.lockState = CursorLockMode.Locked; // Lock cursor to center of screen.
        StartCoroutine(cameraMovement.AttachCameraToFellow()); // Animate camera to move to first-person fellow

        // Set pellets to first person maze
        pellets = allPellets[3];
        int previousMaze = maze;
        maze = 0;

        // Change ghosts position to minigame maze
        foreach (GameObject ghost in ghosts)
        {
            GhostInterface ghostInterface = ghost.GetComponent<GhostInterface>();
            Vector3 ghostStartPos = ghost.GetComponent<GhostInterface>().GetStartPos();
            Vector3 newGhostStartPos = new Vector3(ghostStartPos.x + ((maze - previousMaze) * 31.0f), ghostStartPos.y, ghostStartPos.z);
            ghostInterface.SetStartPos(newGhostStartPos);
            ghostInterface.ResetGhost();
            ghostInterface.SetPlayerTarget(minigamePlayer.GetComponent<FellowInterface>());
            ghostInterface.SetSpeed(1.5f); // Decreased speed to make minigame more playable
            ghostInterface.SetScatterPoints(maze);
        }

        // Start countdown UI
        countdownUI.gameObject.SetActive(true);
        currentMode = GameMode.InMinigame;
        StartCoroutine(StartCountdown(currentMode));
    }

    private IEnumerator StartCountdown(GameMode mode)
    {
        /* If player is not on the first level, wait before countdown to allow time for
        camera to pan over to next level */
        if (level != 1)
        {
            yield return new WaitForSeconds(1.2f);
        }

        if (mode == GameMode.InGame)
        {
            countdownUI.transform.localScale = Vector3.one * 0.1f; // Set size to fit maze
            winUI.transform.localScale = Vector3.one * 0.1f;
            pauseUI.transform.localScale = Vector3.one * 0.1f;
            gameOverUI.transform.localScale = Vector3.one * 0.1f;
            countdownPanel.SetActive(true);
        } 
        else if (mode == GameMode.InMinigame)
        {
            // Wait for camera animation to finish starting countdown
            yield return new WaitForSeconds(3f); 

            // Set size of all UIs to fit infront of player
            countdownUI.transform.localScale = Vector3.one * 0.005f; 
            winUI.transform.localScale = Vector3.one * 0.005f;
            pauseUI.transform.localScale = Vector3.one * 0.005f;
            gameOverUI.transform.localScale = Vector3.one * 0.005f;

            // Disable panel from countdown UI for first person view
            countdownPanel.SetActive(false); 
        }
        

        UIFader[] uiFaders = countdownUI.GetComponents<UIFader>();
        UIFader number = uiFaders[0];
        UIFader background = uiFaders[1];
        background.FadeIn(0.2f, countdownUI.gameObject);

        yield return new WaitForSeconds(0.5f); // Wait for main menu to fade
        mainMenuUI.gameObject.SetActive(false);

        countdownMusic = countdownNumber.GetComponent<AudioSource>();
        countdownMusic.Play(0);

        Vector3 originalScale = Vector3.one;
        Vector3 targetScale = originalScale * 1.2f;

        // Countdown from 3 to 1 then show GO!
        for (int i = 3; i >= 0; i--)
        {
            if (i >= 1)
            {
                countdownNumber.GetComponent<Text>().text = i.ToString();
            }
            else
            {
                countdownNumber.GetComponent<Text>().text = "GO!";
                background.FadeOut(0.5f, countdownUI.gameObject, false);
            }

            float time = 0.4f;
            float originalTime = time;

            number.FadeIn(0.5f, countdownNumber.gameObject);

            // Number slowly increases in size before fading out and changing number
            while (time > 0.0f)
            {
                time -= Time.deltaTime;
                countdownNumber.transform.localScale = Vector3.Lerp(targetScale, originalScale, time / originalTime);
                yield return new WaitForEndOfFrame();
            }

            number.FadeOut(0.5f, countdownNumber.gameObject, false);

            yield return new WaitForSeconds(0.5f);
        }

        // Reset text back to nothing for next countdown
        countdownNumber.GetComponent<Text>().text = "";

        // Start the game
        gameMode = mode;
        audioClips[maze].Play(0);
        countdownUI.gameObject.SetActive(false);
    }

    public void NextLevel()
    {
        Time.timeScale = 1;

        // Check if player is in minigame and whether restart function should be called.
        if (currentMode == GameMode.InMinigame)
        {
            ResetMinigame();

            countdownUI.gameObject.SetActive(true);
            StartCoroutine(StartCountdown(GameMode.InMinigame));
        }
        else
        {
            audioClips[maze].Stop();

            winUI.gameObject.SetActive(false);
            gameMode = GameMode.Paused;

            // Reset fellow properties for next level
            if (player.lives < 3) // Give player an extra life between levels as long as it's less than 3
            {
                player.lives++;
            }
            player.pelletsEaten = 0;
            player.powerupTime = 0;
            player.GetComponent<Rigidbody>().velocity = Vector3.zero; // If player has previous momentum, reset to zero

            // Show lives UI
            Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
            for (int i = 0; i <= player.lives - 1; i++)
            {
                livesUI.transform.GetChild(i).localScale = lifeSize;
            }

            // Next maze, unless reached last maze, then return to first maze
            int previousMaze = maze; // Set previous maze before increment for camera movement
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

            // Set all collectables to be active incase maze has already been played
            foreach (GameObject pellet in pellets)
            {
                pellet.SetActive(true);
            }
            foreach (GameObject powerup in powerups)
            {
                powerup.SetActive(true);
            }
            cherry.SetActive(true);

            // Reset timers
            scatterTime = 7.0f;
            chaseTime = 20.0f;

            // Move camera to next level
            StartCoroutine(cameraMovement.MoveCameraToLevel(previousMaze, maze));

            // Move characters to next maze
            ResetCharacters(maze, previousMaze);

            StartGame();
        }
    }

    // Function for restart button
    public void Restart()
    {
        if (currentMode == GameMode.InGame)
        {
            ResetGame();

            // Reset timers
            scatterTime = 7.0f;
            chaseTime = 20.0f;

            // Move camera to next level
            StartCoroutine(cameraMovement.MoveCameraToLevel(previousMaze, maze));

            ResetCharacters(maze, previousMaze);

            StartGame();
        }
        else if (currentMode == GameMode.InMinigame)
        {
            ResetMinigame();
            Cursor.lockState = CursorLockMode.Locked;
            mouseLook.enabled = true;
            countdownUI.gameObject.SetActive(true);
            StartCoroutine(StartCountdown(GameMode.InMinigame));
        }
    }

    public void ExitGame()
    {
        audioClips[maze].Stop();

        Time.timeScale = 1;

        // If player is not on the first maze then move camera back to it
        if (maze != 1)
        {
            mouseLook.enabled = false;
            cameraMovement.SetInMinigame(false);
            StartCoroutine(cameraMovement.ReturnToStart());
        }

        if (currentMode == GameMode.InGame)
        {
            gameUI.GetComponent<UIFader>().FadeOut(0.4f, gameUI.gameObject, true);
            ResetGame();
        }
        else if (currentMode == GameMode.InMinigame)
        {
            ResetMinigame();
            minimapCamera.SetActive(false);
            cameraMovement.ResetUIPositions();
        }

        // Reset all characters to first maze
        ResetCharacters(1, previousMaze);

        level = 1;
        maze = 1;

        pauseUI.SetActive(false);
        
        mainMenuUI.SetActive(true);
        GetComponent<UIFader>().FadeIn(2f, mainMenuUI.gameObject);

        menuMusic.Play(0);

        gameMode = GameMode.MainMenu;
    }

    private void ResetCharacters(int nextMaze, int previousMaze)
    {
        // Only reset position of fellow if playing normal game
        if (currentMode == GameMode.InGame)
        {
            // Move fellow and to next maze and change start position
            Vector3 startPos = player.GetStartPos();
            Vector3 newStartPos = new Vector3(startPos.x + ((nextMaze - previousMaze) * 31.0f), startPos.y, startPos.z);
            player.SetStartPos(newStartPos);
            player.transform.position = newStartPos;
        }

        // Reset ghosts position to next maze
        foreach (GameObject ghost in ghosts)
        {
            GhostInterface ghostInterface = ghost.GetComponent<GhostInterface>();
            Vector3 ghostStartPos = ghost.GetComponent<GhostInterface>().GetStartPos();
            Vector3 newGhostStartPos = new Vector3(ghostStartPos.x + ((nextMaze - previousMaze) * 31.0f), ghostStartPos.y, ghostStartPos.z);
            ghostInterface.SetStartPos(newGhostStartPos);
            ghostInterface.ResetGhost();
            ghostInterface.SetPlayerTarget(player.GetComponent<FellowInterface>());
            ghostInterface.SetSpeed(3.5f);
            ghostInterface.SetScatterPoints(nextMaze);
        }
    }

    public void ResetGame()
    {
        audioClips[maze].Stop();

        gameOverUI.gameObject.SetActive(false);
        gameMode = GameMode.Paused;

        // Reset fellow properties
        player.ResetFellow();

        // Reset all lives to show on screen
        Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
        for (int i = 0; i <= player.lives - 1; i++)
        {
            livesUI.transform.GetChild(i).localScale = lifeSize;
        }

        // Next level, unless reached last level, then return to first maze
        previousMaze = maze; // Set previous level before increment for camera movement
        maze = 1;
        level = 1;
        pellets = allPellets[0];

        // Set all pellets and powerups to be active incase maze has already been played
        foreach (GameObject pellet in pellets)
        {
            pellet.SetActive(true);
        }
        foreach (GameObject powerup in powerups)
        {
            powerup.SetActive(true);
        }
        cherry.SetActive(true);
    }

    private void ResetMinigame()
    {
        previousMaze = 0;

        audioClips[maze].Stop();
        minigamePlayer.gameObject.SetActive(true); // Re-enable fellow

        winUI.gameObject.SetActive(false);
        gameOverUI.SetActive(false);

        // Reset timers
        scatterTime = 7.0f;
        chaseTime = 20.0f;

        // Show all lives again
        Vector3 lifeSize = new Vector3(1.87622f, 1.87622f, 0.1f);
        for (int i = 0; i <= player.lives - 1; i++)
        {
            minigameLivesUI.transform.GetChild(i).localScale = lifeSize;
        }

        // Reset all collectables for maze
        foreach (GameObject pellet in pellets)
        {
            pellet.SetActive(true);
        }
        foreach (GameObject powerup in powerups)
        {
            powerup.SetActive(true);
        }
        minimap.GetComponent<MinimapPowerup>().Init(); // Reset random position
        minimap.SetActive(true);

        // Reset ghost properties
        foreach (GameObject ghost in ghosts)
        {
            GhostInterface ghostInterface = ghost.GetComponent<GhostInterface>();
            ghostInterface.ResetGhost();
            ghostInterface.SetPlayerTarget(minigamePlayer.GetComponent<FellowInterface>());
            ghostInterface.SetSpeed(1.5f);
            ghostInterface.SetScatterPoints(maze);
        }

        // Reset fellow properties and set position back to start
        minigamePlayer.ResetFellow();
    }

    public bool InAnyGame()
    {
        return gameMode == GameMode.InGame || gameMode == GameMode.InMinigame;
    }

    public bool InGame()
    {
        return gameMode == GameMode.InGame;
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

    public int GetCurrentTotalPellets()
    {
        return pellets.Length;
    }

    public void SetVolumeOfMusic(float volume)
    {
        audioClips[maze].volume = volume;
    }
}
