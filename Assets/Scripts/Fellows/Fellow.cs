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

        // Stop fellow from being able to enter the "ghost house"
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
        } 
        else
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
        if (other.gameObject.CompareTag("L" + game.CurrentMaze().ToString() + "Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            scoreText.GetComponent<Text>().text = score.ToString();
        } 
        else if (other.gameObject.CompareTag("Powerup")) 
        {
            /* If any ghosts have not changed back to a false respawn after previous death 
            while old powerup is occuring, reset to account for new powerup */
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<GhostInterface>().ResetRespawn();
            }

            powerupTime = powerupDuration;
        }
        else if (other.gameObject.CompareTag("Cherry"))
        {
            score += 500;
            scoreText.GetComponent<Text>().text = score.ToString();
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
                scoreText.GetComponent<Text>().text = score.ToString();
            }
            else
            {
                /* Disable collisions to stop accidental triggers while death is occuring,
                i.e. lives decreasing more than once */
                Physics.IgnoreCollision(GetComponent<SphereCollider>(), collision.collider, true);

                // Decrease game volume to let player hear death sound
                game.SetVolumeOfMusic(0.2f);
                deathSound.Play(0);
                lives--;
                
                if (lives <= 0)
                {
                    gameObject.SetActive(false);
                    game.ShowGameOverUI();
                }
                else
                {
                    StartCoroutine(FellowDeath(collision.collider));
                }

                // Remove life from UI
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
            ghost.GetComponent<GhostInterface>().SetSpeed(3.5f);
        }

        // Reset scatter and chase times for ghosts
        game.scatterTime = 7.0f;
        game.chaseTime = 20.0f;

        // Return music back to full volume
        game.SetVolumeOfMusic(1f);

        // Re-enable collision with ghost
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), ghostCollider, false);

        yield break;
    }

    public void ResetFellow()
    {
        gameObject.SetActive(true);
        lives = 3;
        pelletsEaten = 0;
        powerupTime = 0;
        score = 0;
        GetComponent<Rigidbody>().velocity = Vector3.zero;
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
        scoreText.GetComponent<Text>().text = score.ToString();
    }

    public Vector3 GetPosition()
    {
        return transform.position;
    }
}
