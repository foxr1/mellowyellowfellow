using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    [SerializeField]
    GameObject game;

    private float speed = 1f;
    private Vector3 startPos, startUIPos, nextLevelPos, levelUIPos;
    private GameObject userInterface;
    private bool isFP = false;

    private void Start()
    {
        userInterface = GameObject.Find("User Interface");
        startPos = transform.position;
        startUIPos = userInterface.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * speed);
    }

    public IEnumerator MoveCameraToNextLevel()
    {
        nextLevelPos = new Vector3(transform.position.x + 31.0f, transform.position.y, transform.position.z);
        levelUIPos = new Vector3(userInterface.transform.position.x + 31.0f, userInterface.transform.position.y, userInterface.transform.position.z);

        while (transform.position != nextLevelPos)
        {
            userInterface.transform.position = Vector3.Lerp(userInterface.transform.position, levelUIPos, Time.deltaTime * 3);
            transform.position = Vector3.Lerp(transform.position, nextLevelPos, Time.deltaTime * 3);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator ReturnToStart()
    {
        while (transform.position != startPos)
        {
            userInterface.transform.position = Vector3.Lerp(userInterface.transform.position, startUIPos, Time.deltaTime * 3);
            transform.position = Vector3.Lerp(transform.position, startPos, Time.deltaTime * 3);
            yield return new WaitForEndOfFrame();
        }
    }

    public IEnumerator AttachCameraToFellow()
    {
        Vector3 fpFellowPos = GameObject.Find("FPFellow").transform.position;
        isFP = true;

        while (true)
        {
            fpFellowPos = GameObject.Find("FPFellow").transform.position;
            Vector3 cameraPos = new Vector3(fpFellowPos.x, 0.4f, fpFellowPos.z);
            transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * 10);
            yield return new WaitForEndOfFrame();
        }
    }
}
