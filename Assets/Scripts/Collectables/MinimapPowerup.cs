using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class MinimapPowerup : MonoBehaviour
{
    private float speed = 3.5f;
    private Vector3 startPos, randomPos, endPos, targetPos;

    [SerializeField]
    GameObject minimapCamera;

    void Start()
    {
        Init();
    }

    public void Init()
    {
        startPos = transform.position;
        randomPos = SetRandomPosition();
        endPos = new Vector3(randomPos.x, randomPos.y - 0.05f, randomPos.z);
        targetPos = endPos;
    }

    // Update is called once per frame
    void Update()
    {
        // Move between the start and end vectors
        if (Mathf.Approximately(transform.position.magnitude, endPos.magnitude))
        {
            targetPos = randomPos;
        }
        else if (Mathf.Approximately(transform.position.magnitude, startPos.magnitude))
        {
            targetPos = endPos;
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
        transform.RotateAround(transform.position, Vector3.up, Time.deltaTime * 50);
    }

    Vector3 SetRandomPosition()
    {
        Vector3 randomDirection = Random.insideUnitSphere * 8;
        randomDirection += startPos;
        randomDirection.y = 0.45f;
        NavMesh.SamplePosition(randomDirection, out NavMeshHit hit, 8, NavMesh.AllAreas);
        return new Vector3(hit.position.x, 0.45f, hit.position.z);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Fellow"))
        {
            minimapCamera.SetActive(true);
            gameObject.SetActive(false);
        }
    }
}
