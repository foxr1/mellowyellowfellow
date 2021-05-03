using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPFellow : MonoBehaviour, FellowInterface
{
    // Game
    [SerializeField]
    GameObject game;

    // Character Movement
    [SerializeField]
    float speed = 4f;

    // Score from pellets
    int score = 0;
    public int pelletsEaten = 0;
    [SerializeField]
    int pointsPerPellet = 100;
    [SerializeField]
    GameObject scoreText;

    // Powerup
    [SerializeField]
    float powerupDuration = 10.0f;
    public float powerupTime = 0.0f;

    // Lives
    public int lives = 3;
    [SerializeField]
    GameObject livesUI;

    // Position
    public Vector3 startPos;
    public string direction;

    [SerializeField]
    CharacterController controller;
    Vector3 movement;

    // Start is called before the first frame update
    void Start()
    {
        // Get start position of player to reset to when player dies
        startPos = gameObject.transform.position;
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), GameObject.Find("GhostHouse").GetComponent<BoxCollider>(), false);
    }

    // Update is called once per frame
    void Update()
    {
        powerupTime = Mathf.Max(0.0f, powerupTime - Time.deltaTime);

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        movement = transform.right * x + transform.forward * z;

        controller.Move(movement * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject currentLeftTeleporter = GameObject.Find("Maze" + game.GetComponent<YellowFellowGame>().CurrentLevel().ToString() + "/LeftTeleporter");
        GameObject currentRightTeleporter = GameObject.Find("Maze" + game.GetComponent<YellowFellowGame>().CurrentLevel().ToString() + "/RightTeleporter");

        if (other.gameObject.CompareTag("L1Pellet") || other.gameObject.CompareTag("L2Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            scoreText.GetComponent<Text>().text = score.ToString();
        }
        else if (other.gameObject.CompareTag("Powerup"))
        {
            // If any ghosts have not changed back to a false respawn after previous death 
            // while old powerup is occuring, reset to account for new powerup
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<GhostInterface>().ResetRespawn();
            }

            powerupTime = powerupDuration;
        }
        else if (other.gameObject == currentLeftTeleporter)
        {
            Vector3 rightPortalPos = currentRightTeleporter.transform.position;
            transform.position = new Vector3(rightPortalPos.x - 1.5f, rightPortalPos.y, rightPortalPos.z);
        }
        else if (other.gameObject == currentRightTeleporter)
        {
            Vector3 leftPortalPos = currentLeftTeleporter.transform.position;
            transform.position = new Vector3(leftPortalPos.x + 1.5f, leftPortalPos.y, leftPortalPos.z);
        }
    }

    public bool PowerupActive()
    {
        return powerupTime > 0.0f;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ghost"))
        {
            if (PowerupActive() && !collision.gameObject.GetComponent<GhostInterface>().HasRespawned())
            {
                collision.gameObject.GetComponent<GhostInterface>().died();
                score += 200;
            }
            else
            {
                lives--;
                if (lives <= 0)
                {
                    Debug.Log("You Died");
                    gameObject.SetActive(false);
                }
                else
                {
                    // Reset fellow back to start position
                    transform.position = startPos;

                    // Reset all ghosts back to original positions
                    GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
                    foreach (GameObject ghost in ghosts)
                    {
                        ghost.GetComponent<GhostInterface>().ResetGhost();
                        game.GetComponent<YellowFellowGame>().scatterTime = 7.0f; // Reset scatter time for ghosts
                        game.GetComponent<YellowFellowGame>().chaseTime = 20.0f; // Reset chase time for ghosts
                    }
                }

                // Remove hearts from UI each time player dies
                livesUI.transform.GetChild(lives).localScale = Vector3.zero;
            }
        }
    }

    public int PelletsEaten()
    {
        return pelletsEaten;
    }

    public Vector3 GetStartPos()
    {
        return startPos;
    }

    public void SetStartPos(Vector3 newStartPos)
    {
        startPos = newStartPos;
    }

    public int GetScore()
    {
        return score;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }

    public string GetDirection()
    {
        return direction;
    }
}
