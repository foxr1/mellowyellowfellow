using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class FPRightTeleporter : MonoBehaviour
{
    [SerializeField]
    Transform targetTeleporter;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Ghost"))
        {
            collision.gameObject.GetComponent<NavMeshAgent>().enabled = false;
            collision.transform.position = new Vector3(targetTeleporter.position.x + 2, 0.65f, targetTeleporter.position.z);
            collision.gameObject.GetComponent<NavMeshAgent>().enabled = true;
        }
    }
}