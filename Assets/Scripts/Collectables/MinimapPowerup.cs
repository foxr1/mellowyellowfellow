using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MinimapPowerup : MonoBehaviour
{
    private float speed = 3.5f;
    private Vector3 startPos, endPos, targetPos;

    [SerializeField]
    GameObject minimapCamera;

    private void Start()
    {
        startPos = transform.position;
        endPos = new Vector3(startPos.x, startPos.y - 0.05f, startPos.z);

        targetPos = endPos;
    }

    // Update is called once per frame
    void Update()
    {
        // Move between the start and end vectors
        if (Mathf.Approximately(transform.position.magnitude, endPos.magnitude))
        {
            targetPos = startPos;
        }
        else if (Mathf.Approximately(transform.position.magnitude, startPos.magnitude))
        {
            targetPos = endPos;
        }

        transform.position = Vector3.Lerp(transform.position, targetPos, Time.deltaTime * speed);
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
