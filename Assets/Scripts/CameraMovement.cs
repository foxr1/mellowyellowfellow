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
    private bool inMinigame = false;

    private void Start()
    {
        userInterface = GameObject.Find("User Interface");
        startPos = transform.position;
        startUIPos = userInterface.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        if (!inMinigame)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * speed);
        }
    }

    public IEnumerator MoveCameraToLevel(int currentLevel, int nextLevel)
    {
        nextLevelPos = new Vector3(transform.position.x + ((nextLevel - currentLevel) * 31.0f), transform.position.y, transform.position.z);
        levelUIPos = new Vector3(userInterface.transform.position.x + ((nextLevel - currentLevel) * 31.0f), userInterface.transform.position.y, userInterface.transform.position.z);

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
        GameObject fpFellow = GameObject.Find("FPFellow");
        Vector3 fpFellowPos = fpFellow.transform.position;
        GameObject fpUI = GameObject.Find("FPUI");
        GameObject countdownUI = GameObject.Find("CountdownUI");
        inMinigame = true;

        while (true)
        {
            fpFellowPos = fpFellow.transform.position;
            Vector3 cameraPos = new Vector3(fpFellowPos.x, 0.4f, fpFellowPos.z);

            // Update camera position to be fixed to player
            transform.position = cameraPos;

            // Keep first person UI in front of camera
            fpUI.transform.position = transform.position + transform.forward * 0.40f;
            fpUI.transform.rotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);

            countdownUI.transform.position = transform.position + transform.forward * 0.40f;
            countdownUI.transform.rotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);

            yield return new WaitForEndOfFrame();
        }
    }
}
