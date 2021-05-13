using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Cherry : MonoBehaviour
{
    public NavMeshAgent agent;

    // Start is called before the first frame update
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.destination = PickRandomPosition();
    }

    // Update is called once per frame
    void Update()
    {
        if (agent.isOnNavMesh && agent.remainingDistance < 0.5f)
        {
            agent.destination = PickRandomPosition();
        }
    }

    Vector3 PickRandomPosition()
    {
        Vector3 destination = transform.position;
        Vector2 randomDirection = Random.insideUnitCircle * 8.0f;
        destination.x += randomDirection.x;
        destination.z += randomDirection.y;

        NavMesh.SamplePosition(destination, out NavMeshHit navHit, 8.0f, NavMesh.AllAreas);
        return navHit.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fellow"))
        {
            gameObject.SetActive(false);
        }
    }
}
