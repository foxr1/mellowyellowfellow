using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Ghost : MonoBehaviour
{
    NavMeshAgent agent;

    [SerializeField]
    Fellow player;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = PickRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (CanSeePlayer())
        {
            Debug.Log("I can see you!");
            agent.destination = player.transform.position;
        }
        else if (agent.remainingDistance < 0.5f)
        {
            agent.destination = PickRandomPosition();
        }
    }

    Vector3 PickRandomPosition()
    {
        Vector3 destination = transform.position;
        Vector2 randomDirection = UnityEngine.Random.insideUnitCircle * 0.8f;
        destination.x += randomDirection.x;
        destination.z += randomDirection.y;

        NavMeshHit navHit;
        NavMesh.SamplePosition(destination, out navHit, 8.0f, NavMesh.AllAreas);

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

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ghost"))
        {
            Debug.Log("You died!");
            gameObject.SetActive(false);
        }
    }
}
