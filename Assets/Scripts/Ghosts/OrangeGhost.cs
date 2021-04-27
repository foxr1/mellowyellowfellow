using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OrangeGhost : MonoBehaviour, GhostInterface
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
    bool canMove = false;
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

        // Wait for 1/3 of pellets to be collected
        if (player.PelletsEaten() >= (game.GetComponent<YellowFellowGame>().pellets.Length / 3))
        {
            canMove = true;
        }

        if (canMove)
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
                        // Orange ghost will only chase if it is further than "8 tiles" away from the player
                        // When looking at the logged distance it very rarely will be a distance of 8 away,
                        // so I lowered this number to factor for this.

                        Vector3 playerPos = player.transform.position;
                        Vector3 ghostPos = this.transform.position;
                        float distanceAway = Vector3.Distance(playerPos, ghostPos);
                        Debug.Log(distanceAway);

                        if (distanceAway > 5)
                        {
                            agent.destination = player.transform.position;
                        } else
                        {
                            agent.destination = PickRandomPosition();
                        }
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
            transform.position = new Vector3(rightPortalPos.x, 0.65f, rightPortalPos.z);
        }
        else if (other.gameObject.CompareTag("RightPortal"))
        {
            Vector3 leftPortalPos = GameObject.FindGameObjectWithTag("LeftPortal").transform.position;
            transform.position = new Vector3(leftPortalPos.x, 0.65f, leftPortalPos.z);
        }
        else if (game.GetComponent<YellowFellowGame>().inGame())
        {
            canMove = true;
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
