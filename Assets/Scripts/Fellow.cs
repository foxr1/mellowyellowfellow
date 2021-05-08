using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Fellow : MonoBehaviour, FellowInterface
{
    // Game
    [SerializeField]
    YellowFellowGame game;

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
    private string direction = "up";

    // Sounds
    [SerializeField]
    AudioSource deathSound;

    // Start is called before the first frame update
    void Start()
    {
        // Get start position of player to reset to when player dies
        startPos = transform.position;
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), GameObject.Find("GhostHouse").GetComponent<BoxCollider>(), false);
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

        // Prevent player from moving until game has started
        if (game.InGame())
        {
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
        } else
        {
            b.velocity = Vector3.zero;
        }
    }

    public string GetDirection()
    {
        return direction;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject currentLeftTeleporter = GameObject.Find("Maze" + game.CurrentLevel().ToString() + "/LeftTeleporter");
        GameObject currentRightTeleporter = GameObject.Find("Maze" + game.CurrentLevel().ToString() + "/RightTeleporter");

        // Declare extra teleporters for third maze
        GameObject topLeftTeleporter = GameObject.Find("Maze3/TopLeftTeleporter");
        GameObject topRightTeleporter = GameObject.Find("Maze3/TopRightTeleporter");

        if (other.gameObject.CompareTag("L" + game.CurrentMaze().ToString() + "Pellet"))
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
        else if (other.gameObject == topLeftTeleporter)
        {
            Vector3 rightPortalPos = topRightTeleporter.transform.position;
            transform.position = new Vector3(rightPortalPos.x - 1.5f, rightPortalPos.y, rightPortalPos.z);
        }
        else if (other.gameObject == topRightTeleporter)
        {
            Vector3 leftPortalPos = topLeftTeleporter.transform.position;
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
                collision.gameObject.GetComponent<GhostInterface>().GhostDied();
                score += 200;
            }
            else
            {
                // Disable collisions to stop accidental triggers while death is occuring
                Physics.IgnoreCollision(GetComponent<SphereCollider>(), collision.collider, true);

                game.SetVolumeOfMusic(0.2f);
                deathSound.Play(0);
                lives--;
                
                if (lives <= 0)
                {
                    Debug.Log("You Died");
                    gameObject.SetActive(false);
                }
                else
                {
                    StartCoroutine(FellowDeath(collision.collider));
                }

                livesUI.transform.GetChild(lives).localScale = Vector3.zero;
            }
        }
    }

    public IEnumerator FellowDeath(Collider ghostCollider)
    {
        // Stop ghost movements
        GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<GhostInterface>().SetSpeed(0f);
        }

        // Animate scale of fellow to 0 
        while (transform.localScale != Vector3.zero)
        {
            transform.localScale = Vector3.Lerp(transform.localScale, Vector3.zero, Time.deltaTime * 5f);
            if (transform.localScale != Vector3.zero)
            {
                yield return new WaitForEndOfFrame();
            }
        }

        // Reset fellow back to start position
        transform.position = startPos;
        transform.localScale = Vector3.one * 0.8f;

        // Reset all ghosts back to original positions
        foreach (GameObject ghost in ghosts)
        {
            ghost.GetComponent<GhostInterface>().ResetGhost();
            game.scatterTime = 7.0f; // Reset scatter time for ghosts
            game.chaseTime = 20.0f; // Reset chase time for ghosts
            ghost.GetComponent<GhostInterface>().SetSpeed(3.5f);
        }

        // Return music back to full volume
        game.SetVolumeOfMusic(1f);

        // Reenable collision with ghost
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), ghostCollider, false);

        yield break;
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

    public void SetScore(int newScore)
    {
        score = newScore;
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
