using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class RedGhost : MonoBehaviour, GhostInterface
{
    // Ghost properties
    public NavMeshAgent agent;
    public Vector3 startPos;
    private float startSpeed;

    // Scatter goal positions
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
    bool respawned = false; // For when the ghost returns back to ghost house so game knows not to keep ghost in powered state after returning
    public bool hasDied = false; // For when the ghost has been killed by the player when powerup is active

    // Game properties
    [SerializeField]
    YellowFellowGame game;
    float scatterTime, chaseTime;
    [SerializeField]
    GameObject ghostHouse;

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

        if (game.InAnyGame() && agent.isActiveAndEnabled)
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
                    GetComponent<Renderer>().material = scaredMaterial;
                    agent.destination = PickHidingPlace();
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

                // If in scatter mode
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
                // If in chase mode
                else if (chaseTime > 0.0f)
                {
                    agent.destination = player.GetPosition();
                }

                hasDied = false;
            }

            // Increase speed by 5% depending on pellets eaten is 1/3 or 2/3 of the total for the given maze.
            // Extra condition to check if speed has already been incremented to stop speed increasing indefinitely
            if ((agent.speed != (startSpeed * 1.05f)) && (player.PelletsEaten() >= (game.GetCurrentTotalPellets() / 3)) && (player.PelletsEaten() < (game.GetCurrentTotalPellets() * 2 / 3)))
            {
                agent.speed = startSpeed * 1.05f;
            } 
            else if ((agent.speed != (startSpeed * 1.1025f)) && (player.PelletsEaten() >= (game.GetCurrentTotalPellets() * 2 / 3)))
            {
                agent.speed = startSpeed * 1.1025f;
            }
            else if (player.PelletsEaten() < (game.GetCurrentTotalPellets() / 3))
            {
                agent.speed = startSpeed;
            }
        }
        else
        {
            agent.speed = 0f;
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

            // Enable collisions for all types of fellow
            GameObject[] fellows = GameObject.FindGameObjectsWithTag("Fellow");
            foreach (GameObject fellow in fellows)
            {
                Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), fellow.GetComponent<Collider>(), false);
            }
        }
    }

    public void SetScatterPoints(int maze)
    {
        int amountOfPoints = GameObject.Find("Maze" + maze + "/ScatterPathPoints/Red").transform.childCount;
        scatterPoints = new Transform[amountOfPoints];
        for (int i = 0; i < amountOfPoints; i++)
        {
            scatterPoints.SetValue(GameObject.Find("Maze" + maze + "/ScatterPathPoints/Red").transform.GetChild(i), i);
        }
    }

    public void GhostDied()
    {
        hasDied = true;
        GetComponent<Renderer>().material = deadMaterial; // Transparent material

        // Disable collisions for all types of fellow
        GameObject[] fellows = GameObject.FindGameObjectsWithTag("Fellow");
        foreach (GameObject fellow in fellows)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), fellow.GetComponent<Collider>(), true);
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
        ghostHouse = GameObject.Find("Maze" + game.CurrentMaze().ToString() + "GhostHouse"); // Update ghost house to current maze
        
        // Enable collisions for all types of fellow
        GameObject[] fellows = GameObject.FindGameObjectsWithTag("Fellow");
        foreach (GameObject fellow in fellows)
        {
            Physics.IgnoreCollision(GetComponent<CapsuleCollider>(), fellow.GetComponent<Collider>(), false);
        }
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
