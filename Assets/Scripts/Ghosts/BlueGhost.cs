using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BlueGhost : MonoBehaviour, GhostInterface
{
    public NavMeshAgent agent;

    FellowInterface player;

    // Materials
    [SerializeField]
    Material scaredMaterial;
    [SerializeField]
    Material deadMaterial;
    Material normalMaterial;

    bool hiding = false;
    bool canMove = false;
    bool respawned = true;
    public bool hasDied = false; // For when the ghost has been killed by the player when powerup is active

    [SerializeField]
    GameObject ghostHouse;

    public Vector3 startPos;

    [SerializeField]
    GameObject game;
    float scatterTime, chaseTime;

    // Start is called before the first frame update
    void Start()
    {
        normalMaterial = GetComponent<Renderer>().material;
        player = GameObject.Find("Fellow").GetComponent<FellowInterface>();
        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Initialise timers from game script
        scatterTime = game.GetComponent<YellowFellowGame>().scatterTime;
        chaseTime = game.GetComponent<YellowFellowGame>().chaseTime;

        // Wait for player to collect at least 30 pellets before exiting the ghost house
        if (player.PelletsEaten() >= 30)
        {
            canMove = true;
        }

        if (game.GetComponent<YellowFellowGame>().InGame())
        {
            if (canMove)
            {
                if (hasDied)
                {
                    agent.destination = ghostHouse.transform.position;
                }
                else if (player.PowerupActive() && !hasDied && !respawned)
                {
                    if (!hiding || agent.remainingDistance < 0.5f)
                    {
                        hiding = true;
                        agent.destination = PickHidingPlace();
                        GetComponent<Renderer>().material = scaredMaterial;
                    }
                }
                else if (!player.PowerupActive() && respawned)
                {
                    // Once powerup has ended set respawned back to false
                    respawned = false;
                }
                else
                {
                    if (hiding)
                    {
                        GetComponent<Renderer>().material = normalMaterial;
                        hiding = false;
                    }

                    if (scatterTime > 0.0f)
                    {
                        if (agent.remainingDistance < 0.5)
                        {
                            agent.destination = PickRandomPosition();
                            hiding = false;
                            GetComponent<Renderer>().material = normalMaterial;
                        }
                    }
                    else
                    {
                        if (chaseTime > 0.0f)
                        {
                            // Blue ghost uses the red ghosts position in it's calculation for the target position,
                            // it looks "2 tiles" in front of the player and then calculates a vector from the red
                            // ghosts position to the position in front of the player and then doubles the length 
                            // to give the target position.

                            Vector3 redGhostPos = GameObject.Find("RedGhost").transform.position;
                            Vector3 inFrontOfPlayerPos = Vector3.zero;
                            Vector3 playerPos = player.GetPosition();

                            if (player.GetDirection().Equals("left"))
                            {
                                inFrontOfPlayerPos = new Vector3(playerPos.x - 2, playerPos.y, playerPos.z);
                            }
                            else if (player.GetDirection().Equals("right"))
                            {
                                inFrontOfPlayerPos = new Vector3(playerPos.x + 2, playerPos.y, playerPos.z);
                            }
                            else if (player.GetDirection().Equals("up"))
                            {
                                inFrontOfPlayerPos = new Vector3(playerPos.x, playerPos.y, playerPos.z + 2);
                            }
                            else if (player.GetDirection().Equals("down"))
                            {
                                inFrontOfPlayerPos = new Vector3(playerPos.x, playerPos.y, playerPos.z - 2);
                            }

                            Vector3 targetPos = (inFrontOfPlayerPos - redGhostPos) * 2;

                            agent.destination = targetPos;
                        }
                    }

                    hasDied = false;
                }
            }
        }
    }

    Vector3 PickRandomPosition()
    {
        Vector3 destination = transform.position;
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * 8.0f;
        destination.x += randomDirection.x;
        destination.z += randomDirection.y;

        NavMeshHit navHit;
        NavMesh.SamplePosition(destination, out navHit, 8.0f, NavMesh.AllAreas);

        return navHit.position;
    }

    Vector3 PickHidingPlace()
    {
        Vector3 directionToPlayer = (player.GetPosition() - transform.position).normalized;

        NavMeshHit navHit;
        NavMesh.SamplePosition(transform.position - (directionToPlayer * 8.0f), out navHit, 8.0f, NavMesh.AllAreas);

        return navHit.position;
    }

    bool CanSeePlayer()
    {
        Vector3 rayPos = transform.position;
        Vector3 rayDir = (player.GetPosition() - rayPos).normalized;

        RaycastHit info;
        if (Physics.Raycast(rayPos, rayDir, out info))
        {
            if (info.transform.CompareTag("Fellow"))
            {
                return true; // Ghost can see player.
            }
        }
        return false;
    }

    private void OnTriggerEnter(Collider other)
    {
        GameObject currentLeftTeleporter = GameObject.Find("Maze" + game.GetComponent<YellowFellowGame>().CurrentLevel().ToString() + "/LeftTeleporter");
        GameObject currentRightTeleporter = GameObject.Find("Maze" + game.GetComponent<YellowFellowGame>().CurrentLevel().ToString() + "/RightTeleporter");

        if (hasDied && other.gameObject.CompareTag("GhostHouse"))
        {
            hasDied = false;
            hiding = false;
            respawned = true;
            GetComponent<Renderer>().material = normalMaterial;

            // Return speed back to normal
            agent.speed = 3.5f;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;

            // Re-enable collisions for all types of fellow
            GameObject[] fellows = GameObject.FindGameObjectsWithTag("Fellow");
            foreach (GameObject fellow in fellows)
            {
                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), fellow.GetComponent<SphereCollider>(), false);
            }
        }
        else if (other.gameObject == currentLeftTeleporter)
        {
            agent.enabled = false;
            Vector3 rightPortalPos = currentRightTeleporter.transform.position;
            transform.position = new Vector3(rightPortalPos.x - 2, 0.65f, rightPortalPos.z);
            agent.enabled = true;
        }
        else if (other.gameObject == currentRightTeleporter)
        {
            agent.enabled = false;
            Vector3 leftPortalPos = currentLeftTeleporter.transform.position;
            transform.position = new Vector3(leftPortalPos.x + 2, 0.65f, leftPortalPos.z);
            agent.enabled = true;
        }
        else if (game.GetComponent<YellowFellowGame>().InGame())
        {
            canMove = true;
        }
    }

    void GhostInterface.died()
    {
        hasDied = true;
        GetComponent<Renderer>().material = deadMaterial; // Transparent material

        // Disable collisions for all types of fellow
        GameObject[] fellows = GameObject.FindGameObjectsWithTag("Fellow");
        foreach (GameObject fellow in fellows)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), fellow.GetComponent<SphereCollider>(), true);
        }

        // Increase speed so it returns to ghost house quicker
        agent.speed = 6f;
        agent.acceleration = 12f;
        agent.angularSpeed = 240f;
    }

    public Vector3 GetStartPos()
    {
        return startPos;
    }

    public void SetStartPos(Vector3 newStartPos)
    {
        startPos = newStartPos;
    }

    public void ResetGhost()
    {
        agent.enabled = false;
        transform.position = startPos;
        GetComponent<Renderer>().material = normalMaterial;
        agent.enabled = true;
    }

    public bool HasRespawned()
    {
        return respawned;
    }
    public void ResetRespawn()
    {
        respawned = false;
    }

    public void SetNavMeshAgent(bool enabled)
    {
        agent.enabled = enabled;
    }

    public void SetSpeed(float speed)
    {
        agent.speed = speed;
    }

    public void SetPlayerTarget(FellowInterface fellow)
    {
        player = fellow;
    }
}
