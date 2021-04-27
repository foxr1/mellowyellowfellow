using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fellow : MonoBehaviour
{
    // Game
    [SerializeField]
    GameObject game;

    // Character Movement
    [SerializeField]
    float speed = 5f;

    // Score from pellets
    int score = 0;
    int pelletsEaten = 0;
    [SerializeField]
    int pointsPerPellet = 100;
    [SerializeField]
    GameObject scoreText;

    // Powerup
    [SerializeField]
    float powerupDuration = 10.0f;
    float powerupTime = 0.0f;

    // Lives
    int lives = 3;
    [SerializeField]
    GameObject livesUI;

    // Position
    Vector3 startPos;
    public string direction;

    // Start is called before the first frame update
    void Start()
    {
        // Get start position of player to reset to when player dies
        startPos = this.gameObject.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        powerupTime = Mathf.Max(0.0f, powerupTime - Time.deltaTime);
    }

    void FixedUpdate()
    {
        Rigidbody b = GetComponent<Rigidbody>();
        Vector3 velocity = b.velocity;

        if (Input.GetKey(KeyCode.A))
        {
            velocity.x = -speed;
            direction = "left";
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x = speed;
            direction = "right";
        }
        if (Input.GetKey(KeyCode.W))
        {
            velocity.z = speed;
            direction = "up";
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity.z = -speed;
            direction = "down";
        }
        b.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            scoreText.GetComponent<Text>().text = "Score:\n" + score;
        } 
        else if (other.gameObject.CompareTag("Powerup")) 
        {
            powerupTime = powerupDuration;
        }
        else if (other.gameObject.CompareTag("LeftPortal"))
        {
            Vector3 rightPortalPos = GameObject.FindGameObjectWithTag("RightPortal").transform.position;
            this.gameObject.transform.position = new Vector3(rightPortalPos.x - 1.5f, rightPortalPos.y, rightPortalPos.z);
        }
        else if (other.gameObject.CompareTag("RightPortal"))
        {
            Vector3 leftPortalPos = GameObject.FindGameObjectWithTag("LeftPortal").transform.position;
            this.gameObject.transform.position = new Vector3(leftPortalPos.x + 1.5f, leftPortalPos.y, leftPortalPos.z);
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
            if (PowerupActive() && !collision.gameObject.GetComponent<GhostInterface>().hasRespawned())
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
                    this.gameObject.transform.position = startPos;

                    // Reset all ghosts back to original positions
                    GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
                    foreach (GameObject ghost in ghosts)
                    {
                        ghost.GetComponent<GhostInterface>().resetGhost();
                        game.GetComponent<YellowFellowGame>().scatterTime = 7.0f; // Reset scatter time for ghosts
                        game.GetComponent<YellowFellowGame>().chaseTime = 20.0f; // Reset chase time for ghosts
                    }
                }

                // Remove hearts from UI each time player dies
                livesUI.transform.GetChild(lives + 1).localScale = Vector3.zero;
            }
        }
    }

    public int PelletsEaten()
    {
        return pelletsEaten;
    }
}
