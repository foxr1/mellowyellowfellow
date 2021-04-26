using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YellowFellowGame : MonoBehaviour
{
    [SerializeField]
    GameObject highScoreUI;

    [SerializeField]
    GameObject mainMenuUI;

    [SerializeField]
    GameObject gameUI;

    [SerializeField]
    GameObject winUI;

    [SerializeField]
    Fellow playerObject;

    [SerializeField]
    GameObject ghostHouse;

    public GameObject[] pellets;

    // Timers
    public float scatterTime, chaseTime;

    enum GameMode
    {
        InGame,
        MainMenu,
        HighScores
    }

    GameMode gameMode = GameMode.MainMenu;

    // Start is called before the first frame update
    void Start()
    {
        StartMainMenu();
        pellets = GameObject.FindGameObjectsWithTag("Pellet");
    }

    // Update is called once per frame
    void Update()
    {
        switch(gameMode)
        {
            case GameMode.MainMenu:     UpdateMainMenu(); break;
            case GameMode.HighScores:   UpdateHighScores(); break;
            case GameMode.InGame:       UpdateMainGame(); break;
        }

        if (playerObject.PelletsEaten() == pellets.Length)
        {
            Debug.Log("Level Complete!");
        }

        // Global timers for scatter and chase mode
        scatterTime = Mathf.Max(0.0f, scatterTime - Time.deltaTime); // 7 seconds of scatter mode based on documentation given on ghost behaviour

        if (gameMode == GameMode.InGame && scatterTime <= 0.0f)
        {
            chaseTime = Mathf.Max(0.0f, chaseTime - Time.deltaTime);

            if (chaseTime <= 0.0f)
            {
                scatterTime = 7.0f;
                chaseTime = 20.0f;
            }
        }
    }

    void UpdateMainMenu()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }
        else if (Input.GetKeyDown(KeyCode.Return))
        {
            StartHighScores();
        }
    }

    void UpdateHighScores()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            StartMainMenu();
        }
    }

    void UpdateMainGame()
    {
       // playerObject
    }

    void StartMainMenu()
    {
        gameMode                        = GameMode.MainMenu;
        mainMenuUI.gameObject.SetActive(true);
        GetComponent<UIFader>().FadeIn();
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(false);
    }


    void StartHighScores()
    {
        gameMode                = GameMode.HighScores;
        mainMenuUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(true);
        gameUI.gameObject.SetActive(false);
    }

    public void StartGame()
    {
        gameMode                = GameMode.InGame;
        //mainMenuUI.gameObject.SetActive(false);
        highScoreUI.gameObject.SetActive(false);
        gameUI.gameObject.SetActive(true);

        // Fade out menu
        GetComponent<UIFader>().FadeOut();

        // Only red and pink ghosts are first to move when game starts
        GameObject.Find("RedGhost").GetComponent<RedGhost>().canMove = true;
        GameObject.Find("PinkGhost").GetComponent<PinkGhost>().canMove = true;

        // Based on ghost behaviour document given
        scatterTime = 7.0f;
        chaseTime = 20.0f;
    }

    public bool inGame()
    {
        if (gameMode == GameMode.InGame)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}
