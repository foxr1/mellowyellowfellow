using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class BlueGhost : MonoBehaviour, GhostInterface
{
    NavMeshAgent agent;

    [SerializeField]
    Fellow player;

    // Materials
    [SerializeField]
    Material scaredMaterial;
    Material normalMaterial;

    bool hiding = false;
    bool canMove = false;
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

        // Wait for player to collect at least 30 pellets before exiting the ghost house
        if (player.PelletsEaten() >= 30)
        {
            canMove = true;
        }

        if (canMove)
        {
            if (player.PowerupActive() && !hasDied)
            {
                Debug.Log("Hiding from player");
                if (!hiding || agent.remainingDistance < 0.5f)
                {
                    hiding = true;
                    agent.destination = PickHidingPlace();
                    GetComponent<Renderer>().material = scaredMaterial;
                }
            }
            else if (hasDied)
            {
                agent.destination = ghostHouse.transform.position;
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
                        // Blue ghost uses the red ghosts position in it's calculation for the target position,
                        // it looks "2 tiles" in front of the player and then calculates a vector from the red
                        // ghosts position to the position in front of the player and then doubles the length 
                        // to give the target position.

                        Vector3 redGhostPos = GameObject.Find("RedGhost").transform.position;
                        Vector3 inFrontOfPlayerPos = Vector3.zero;
                        Vector3 playerPos = player.transform.position;

                        if (player.GetComponent<Fellow>().direction.Equals("left"))
                        {
                            inFrontOfPlayerPos = new Vector3(playerPos.x - 2, playerPos.y, playerPos.z);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("right"))
                        {
                            inFrontOfPlayerPos = new Vector3(playerPos.x + 2, playerPos.y, playerPos.z);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("up"))
                        {
                            inFrontOfPlayerPos = new Vector3(playerPos.x, playerPos.y, playerPos.z + 2);
                        }
                        else if (player.GetComponent<Fellow>().direction.Equals("down"))
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
        if (other.gameObject.CompareTag("GhostHouse"))
        {
            hasDied = false;
            hiding = false;
            GetComponent<Renderer>().material = normalMaterial;
            GetComponent<CapsuleCollider>().enabled = true;
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
        else if (game.GetComponent<YellowFellowGame>().inGame())
        {
            canMove = true;
        }
    }

    void GhostInterface.died()
    {
        hasDied = true;
    }

    public void resetGhost()
    {
        this.gameObject.transform.position = startPos;
    }
}