using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class FPFellow : MonoBehaviour, FellowInterface
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

    // Pellets Remaining
    [SerializeField]
    GameObject pelletsRemainingText;
    int pelletsRemaining = 118;

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

    // Character Controller movement
    CharacterController controller;
    Vector3 movement;

    // Sounds
    [SerializeField]
    AudioSource deathSound;

    // Start is called before the first frame update
    void Start()
    {
        startPos = transform.position; // Get start position of player to reset to when player dies
        controller = GetComponent<CharacterController>();
        controller.enabled = false; // Disable to avoid unintended movement before playing game
        Physics.IgnoreCollision(GetComponent<SphereCollider>(), GameObject.Find("GhostHouse").GetComponent<BoxCollider>(), false); // Stop fellow from being able to enter the "ghost house"
        pelletsRemainingText.GetComponent<Text>().text = pelletsRemaining.ToString();
    }

    // Update is called once per frame
    void Update()
    {
        pelletsRemaining = game.GetCurrentTotalPellets() - pelletsEaten;

        if (game.InMinigame())
        {
            controller.enabled = true;

            powerupTime = Mathf.Max(0.0f, powerupTime - Time.deltaTime);

            float x = Input.GetAxis("Horizontal");
            float z = Input.GetAxis("Vertical");

            movement = transform.right * x + transform.forward * z;

            controller.Move(movement * speed * Time.deltaTime);

            // Get rotation of player to determine which direction they're facing
            float dirFacing = transform.localRotation.eulerAngles.y;
            if (dirFacing <= 360 && dirFacing > 315 || dirFacing >= 0 && dirFacing <= 45)
            {
                direction = "up";
            }
            else if (dirFacing > 45 && dirFacing <= 135)
            {
                direction = "right";
            }
            else if (dirFacing > 135 && dirFacing <= 225)
            {
                direction = "down";
            }
            else if (dirFacing > 225 && dirFacing <= 315)
            {
                direction = "left";
            }
        }
        else
        {
            controller.enabled = false;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("L" + game.CurrentMaze().ToString() + "Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            scoreText.GetComponent<Text>().text = score.ToString();
            pelletsRemainingText.GetComponent<Text>().text = pelletsRemaining.ToString();
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
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ghost") && !PowerupActive())
        {
            Physics.IgnoreCollision(controller, collision.collider, true); // Disable collision with ghost
            StartCoroutine(FellowDeath(collision.collider));

            // Remove life from hearts
            livesUI.transform.GetChild(lives).localScale = Vector3.zero;
        }
    }

    // When using a Character Controller, must use this function for detecting collisions
    // Both teleporters have their triggers turned off so that it is a collision
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        GameObject currentLeftTeleporter = GameObject.Find("Maze" + game.CurrentMaze().ToString() + "/LeftTeleporter");
        GameObject currentRightTeleporter = GameObject.Find("Maze" + game.CurrentMaze().ToString() + "/RightTeleporter");

        if (hit.gameObject.CompareTag("Ghost") && PowerupActive() && !hit.gameObject.GetComponent<GhostInterface>().HasRespawned())
        {
            hit.gameObject.GetComponent<GhostInterface>().GhostDied();
            score += 200;
        }
        else if (hit.gameObject == currentLeftTeleporter)
        {
            Vector3 rightPortalPos = currentRightTeleporter.transform.position;
            transform.position = new Vector3(rightPortalPos.x - 1.5f, 0.4f, rightPortalPos.z);
        }
        else if (hit.gameObject == currentRightTeleporter)
        {
            Vector3 leftPortalPos = currentLeftTeleporter.transform.position;
            transform.position = new Vector3(leftPortalPos.x + 1.5f, 0.4f, leftPortalPos.z);
        }
    }

    public bool PowerupActive()
    {
        return powerupTime > 0.0f;
    }

    public IEnumerator FellowDeath(Collider ghostCollider)
    {
        lives--;
        if (lives <= 0)
        {
            game.ShowGameOverUI();
            yield break;
        }
        else
        {
            game.SetVolumeOfMusic(0.2f);
            deathSound.Play(0);
            gameObject.SetActive(false);
            // Stop ghost movements
            GameObject[] ghosts = GameObject.FindGameObjectsWithTag("Ghost");
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<GhostInterface>().SetSpeed(0f);
            }

            // Reset fellow back to start position
            transform.position = startPos;

            // Reset all ghosts back to original positions
            foreach (GameObject ghost in ghosts)
            {
                ghost.GetComponent<GhostInterface>().ResetGhost();
                game.scatterTime = 7.0f; // Reset scatter time for ghosts
                game.chaseTime = 20.0f; // Reset chase time for ghosts
                ghost.GetComponent<GhostInterface>().SetSpeed(1.5f);
            }

            // Return music back to full volume
            game.SetVolumeOfMusic(1f);
            gameObject.SetActive(true);

            // Re-enable collision with ghost
            Physics.IgnoreCollision(GetComponent<SphereCollider>(), ghostCollider, false);

            yield break;
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

    public void SetScore(int newScore)
    {
        score = newScore;
        scoreText.GetComponent<Text>().text = score.ToString();
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
