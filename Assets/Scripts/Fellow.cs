using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Fellow : MonoBehaviour
{
    // Character Movement
    [SerializeField]
    float speed = 3f;

    // Score from pellets
    int score = 0;
    int pelletsEaten = 0;
    [SerializeField]
    int pointsPerPellet = 100;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

    }

    void FixedUpdate()
    {
        Rigidbody b = GetComponent<Rigidbody>();
        Vector3 velocity = b.velocity;

        if (Input.GetKey(KeyCode.A))
        {
            velocity.x = -speed;
        }
        if (Input.GetKey(KeyCode.D))
        {
            velocity.x = speed;
        }
        if (Input.GetKey(KeyCode.W))
        {
            velocity.z = speed;
        }
        if (Input.GetKey(KeyCode.S))
        {
            velocity.z = -speed;
        }
        b.velocity = velocity;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Pellet"))
        {
            pelletsEaten++;
            score += pointsPerPellet;
            Debug.Log("Score is " + score);
        }
    }

    public int PelletsEaten()
    {
        return pelletsEaten;
    }
}
