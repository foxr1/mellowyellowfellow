using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class OrangeGhost : MonoBehaviour, GhostInterface
{
    // Ghost properties
    public NavMeshAgent agent;
    public Vector3 startPos;
    private float startSpeed;

    // Scatter goal positions
    [SerializeField]
    Transform[] scatterPoints;
    private int destPoint = -1; // Will increment to 0 on start

    // Player
    FellowInterface player;

    // Materials
    [SerializeField]
    Material scaredMaterial;
    [SerializeField]
    Material deadMaterial;
    Material normalMaterial;

    // Booleans
    bool hiding = false;
    bool canMove = false;
    bool respawned = true;
    public bool hasDied = false; // For when the ghost has been killed by the player when powerup is active

    [SerializeField]
    GameObject ghostHouse;
    [SerializeField]
    YellowFellowGame game;
    float scatterTime, chaseTime;

    // Start is called before the first frame update
    void Start()
    {
        normalMaterial = GetComponent<Renderer>().material;
        player = GameObject.Find("Fellow").GetComponent<FellowInterface>();
        agent = GetComponent<NavMeshAgent>();
        startSpeed = agent.speed;
        SetScatterPoints(1);
    }

    // Update is called once per frame
    void Update()
    {
        // Initialise timers from game script
        scatterTime = game.scatterTime;
        chaseTime = game.chaseTime;

        // Wait for 1/3 of pellets to be collected
        canMove = player.PelletsEaten() >= (game.GetCurrentTotalPellets() / 3);

        if (game.InAnyGame() && agent.isActiveAndEnabled)
        {
            if (canMove)
            {
                agent.speed = startSpeed;

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
                        // If ghost is not currently moving towards a point, then patrol through scatter points
                        if (!agent.pathPending && agent.remainingDistance < 0.3f)
                        {
                            destPoint = (destPoint + 1) % scatterPoints.Length;
                            agent.destination = scatterPoints[destPoint].position;
                            GetComponent<Renderer>().material = normalMaterial;
                            hiding = false;
                        }
                    }
                    else
                    {
                        if (chaseTime > 0.0f)
                        {
                            /* Orange ghost will only chase if it is further than "8 tiles" away from the player.
                            When looking at the logged distance, it very rarely will be a distance of 8 away,
                            so I lowered this number to factor for this. */

                            Vector3 playerPos = player.GetPosition();
                            Vector3 ghostPos = this.transform.position;
                            float distanceAway = Vector3.Distance(playerPos, ghostPos);

                            // If further than 5 chase, else return to scatter mode
                            if (distanceAway > 5)
                            {
                                agent.destination = player.GetPosition();
                            }
                            else
                            {
                                if (!agent.pathPending && agent.remainingDistance < 0.3f)
                                {
                                    destPoint = (destPoint + 1) % scatterPoints.Length;
                                    agent.destination = scatterPoints[destPoint].position;
                                    GetComponent<Renderer>().material = normalMaterial;
                                    hiding = false;
                                }
                            }
                        }
                    }

                    hasDied = false;
                }
            }
            else
            {
                agent.speed = 0f;
            }
        }
    }

    Vector3 PickHidingPlace()
    {
        bool isDirSafe = false;
        float vRotation = 0;
        Vector3 newPos = new Vector3();

        while (!isDirSafe)
        {
            // Calculate the vector pointing from player to the ghost
            Vector3 dirToPlayer = transform.position - player.GetPosition();

            // Calculate the vector from the ghost to the direction away from the player the new point
            newPos = transform.position + dirToPlayer;

            // Rotate the direction of the ghost to move
            newPos = Quaternion.Euler(0, vRotation, 0) * newPos;

            // Shoot a Raycast out to the new direction and see if it hits an obstacle
            bool isHit = Physics.Raycast(transform.position, newPos, out RaycastHit hit, 2f);

            if (hit.transform == null)
            {
                // If the Raycast to the flee direction doesn't hit a wall then the Enemy is good to go to this direction
                return newPos;
            }

            // Change the direction of fleeing is it hits a wall by 15 degrees
            if (isHit && hit.transform.CompareTag("Wall"))
            {
                vRotation += 25;
                isDirSafe = false;
            }
            else
            {
                // If the Raycast to the flee direction doesn't hit a wall then the Enemy is good to go to this direction
                isDirSafe = true;
            }
        }

        return newPos;
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
            agent.speed = startSpeed;
            agent.acceleration = 8f;
            agent.angularSpeed = 120f;

            // Enable collisions with fellow
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), player.GetCollider(), false);
        }
        else if (game.InGame())
        {
            canMove = true;
        }
    }

    public void SetScatterPoints(int maze)
    {
        int amountOfPoints = GameObject.Find("Maze" + maze + "/ScatterPathPoints/Orange").transform.childCount;
        scatterPoints = new Transform[amountOfPoints];
        for (int i = 0; i < amountOfPoints; i++)
        {
            scatterPoints.SetValue(GameObject.Find("Maze" + maze + "/ScatterPathPoints/Orange").transform.GetChild(i), i);
        }
    }

    public void GhostDied()
    {
        hasDied = true;
        GetComponent<Renderer>().material = deadMaterial; // Transparent material

        // Disable collisions with fellow
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), player.GetCollider(), true);

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
        agent.enabled = false; // Ghost cannot move from maze to maze without NavMeshAgent first being disabled.
        GetComponent<Rigidbody>().velocity = Vector3.zero; // Stop momentum from previous movement
        transform.position = startPos;
        GetComponent<Renderer>().material = normalMaterial;
        agent.enabled = true;
        ghostHouse = GameObject.Find("Maze" + game.CurrentMaze().ToString() + "GhostHouse"); // Update ghost house to current maze
        respawned = true;

        // Enable collisions with fellow
        Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), player.GetCollider(), false);
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
