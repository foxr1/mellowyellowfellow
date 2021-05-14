using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class LeftTeleporter : MonoBehaviour
{
    [SerializeField]
    Transform targetTeleporter;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Fellow"))
        {
            other.transform.position = new Vector3(targetTeleporter.position.x - 1.5f, other.transform.position.y, targetTeleporter.position.z);
        }
        else if (other.CompareTag("Ghost"))
        {
            // Disable NavMeshAgent before setting position for teleportation to work correctly.
            other.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            other.transform.position = new Vector3(targetTeleporter.position.x - 2, 0.65f, targetTeleporter.position.z);
            other.gameObject.GetComponent<NavMeshAgent>().enabled = true;
        }
    }
}
