using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class PinkGhost : MonoBehaviour, GhostInterface
{
    NavMeshAgent agent;

    [SerializeField]
    Fellow player;

    // Materials
    [SerializeField]
    Material scaredMaterial;
    [SerializeField]
    Material deadMaterial;
    Material normalMaterial;

    bool hiding = false;
    bool respawned = false;
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

        agent = GetComponent<NavMeshAgent>();
    }

    // Update is called once per frame
    void Update()
    {
        // Initialise timers from game script
        scatterTime = game.GetComponent<YellowFellowGame>().scatterTime;
        chaseTime = game.GetComponent<YellowFellowGame>().chaseTime;

        if (game.GetComponent<YellowFellowGame>().inGame())
        {
            if (hasDied)
            {
                agent.destination = ghostHouse.transform.position;
            }
            else if (player.PowerupActive() && !hasDied & !respawned)
            {
                Debug.Log("Hiding from player");
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
                Debug.Log("Chasing Player!");
                if (hiding)
                {
                    GetComponent<Renderer>().material = normalMaterial;
                    hiding = false;
                }

                if (scatterTime > 0.0f)
                {
                    Debug.Log("scatter");
                    if (agent.remainingDistance < 0.5)
                    {
                        agent.destination = PickRandomPosition();
                        hiding = false;
                        GetComponent<Renderer>().material = normalMaterial;
                    }
                }
                else
                {
                    Debug.Log("chase");
                    if (chaseTime > 0.0f)
                    {
                        // Pink ghost follows player "4 tiles" in front of actual position according to
                        // ghost behaviour documentation, using the direction they are facing to determine
                        // where they should be following.

                        Vector3 targetPos = Vector3.zero;
                        Vector3 playerPos = player.transform.position;
                        if (player.GetComponent<Fellow>().direction.Equals("left"))
                        {
                            targetPos = new Vector3(playerPos.x - 4, playerPos.y, playerPos.z);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("right"))
                        {
                            targetPos = new Vector3(playerPos.x + 4, playerPos.y, playerPos.z);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("up"))
                        {
                            targetPos = new Vector3(playerPos.x, playerPos.y, playerPos.z + 4);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("down"))
                        {
                            targetPos = new Vector3(playerPos.x, playerPos.y, playerPos.z - 4);
                        }

                        agent.destination = targetPos;
                    }
                }

                hasDied = false;
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
        Vector3 directionToPlayer = (player.transform.position - transform.position).normalized;

        NavMeshHit navHit;
        NavMesh.SamplePosition(transform.position - (directionToPlayer * 8.0f), out navHit, 8.0f, NavMesh.AllAreas);

        return navHit.position;
    }

    bool CanSeePlayer()
    {
        Vector3 rayPos = transform.position;
        Vector3 rayDir = (player.transform.position - rayPos).normalized;

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
        }
        else if (other.gameObject.CompareTag("LeftPortal"))
        {
            Vector3 rightPortalPos = GameObject.FindGameObjectWithTag("RightPortal").transform.position;
            this.gameObject.transform.position = new Vector3(rightPortalPos.x, 0.65f, rightPortalPos.z);
        }
        else if (other.gameObject.CompareTag("RightPortal"))
        {
            Vector3 leftPortalPos = GameObject.FindGameObjectWithTag("LeftPortal").transform.position;
            this.gameObject.transform.position = new Vector3(leftPortalPos.x, 0.65f, leftPortalPos.z);
        }
    }

    void GhostInterface.died()
    {
        hasDied = true;
        GetComponent<Renderer>().material = deadMaterial; // Transparent material

        // Increase speed so it returns to ghost house quicker
        agent.speed = 6f;
        agent.acceleration = 12f;
        agent.angularSpeed = 240f;
    }

    public void resetGhost()
    {
        gameObject.transform.position = startPos;
    }

    public bool hasRespawned()
    {
        return respawned;
    }
}
