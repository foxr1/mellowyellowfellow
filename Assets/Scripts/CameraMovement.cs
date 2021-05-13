using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraMovement : MonoBehaviour
{
    // UIs
    [SerializeField]
    GameObject userInterface;
    [SerializeField]
    GameObject fpUI;
    [SerializeField]
    GameObject countdownUI;
    [SerializeField]
    GameObject winUI;
    [SerializeField]
    GameObject pauseUI;
    [SerializeField]
    GameObject gameOverUI;

    private GameObject[] allUIs;

    [SerializeField]
    GameObject fpFellow;

    private float menuSpeed = 1f;
    private float speed = 3f;
    private Vector3 startPos, startUIPos, nextLevelPos, levelUIPos;
    private Vector3 fpUIstartPos, countdownStartPos, winStartPos, pauseStartPos, gameOverStartPos;
    private Vector3[] startPositions;
    private bool inMinigame = false;

    private void Start()
    {
        startPos = transform.position;
        startUIPos = userInterface.transform.position;

        // Only contains relevant UIs for minigame
        allUIs = new GameObject[] { fpUI, countdownUI, winUI, pauseUI, gameOverUI };

        startPositions = new Vector3[] { fpUIstartPos, countdownStartPos, winStartPos, pauseStartPos, gameOverStartPos };
        for (int i = 0; i < startPositions.Length; i++)
        {
            startPositions[i] = allUIs[i].transform.position;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // If not in the minigame, animate the camera to rotate looking down at the maze
        if (!inMinigame)
        {
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(90, 0, 0), Time.deltaTime * menuSpeed);
        }
    }

    public IEnumerator MoveCameraToLevel(int currentLevel, int nextLevel)
    {
        nextLevelPos = new Vector3(transform.position.x + ((nextLevel - currentLevel) * 31.0f), transform.position.y, transform.position.z);
        levelUIPos = new Vector3(userInterface.transform.position.x + ((nextLevel - currentLevel) * 31.0f), userInterface.transform.position.y, userInterface.transform.position.z);

        while (!V3Equal(transform.position, nextLevelPos))
        {
            userInterface.transform.position = Vector3.Lerp(userInterface.transform.position, levelUIPos, Time.deltaTime * speed);
            transform.position = Vector3.Lerp(transform.position, nextLevelPos, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    public IEnumerator ReturnToStart()
    {
        while (!V3Equal(transform.position, startPos))
        {
            userInterface.transform.position = Vector3.Lerp(userInterface.transform.position, startUIPos, Time.deltaTime * speed);
            transform.position = Vector3.Lerp(transform.position, startPos, Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        yield break;
    }

    public IEnumerator AttachCameraToFellow()
    {
        Vector3 fpFellowPos = fpFellow.transform.position;
        Vector3 cameraPos = new Vector3(fpFellowPos.x, 0.3f, fpFellowPos.z);
        Vector3 aboveMazePos = new Vector3(transform.position.x - 31.0f, transform.position.y, transform.position.z);
        inMinigame = true;

        // Move to above maze
        while (!V3Equal(transform.position, aboveMazePos))
        {
            transform.position = Vector3.Lerp(transform.position, aboveMazePos, Time.deltaTime * 6);
            yield return new WaitForEndOfFrame();
        }

        // Move camera down into minigame player and then look forward
        while (!V3Equal(transform.position, cameraPos))
        {
            transform.position = Vector3.Lerp(transform.position, cameraPos, Time.deltaTime * speed);
            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), Time.deltaTime * speed);
            yield return new WaitForEndOfFrame();
        }

        // Enable mouse movement with camera then attach camera to fellow position
        GetComponent<MouseLook>().enabled = true;
        while (inMinigame)
        {
            fpFellowPos = fpFellow.transform.position;

            // Update camera position to be fixed to player
            transform.position = fpFellowPos;

            // Move all relevant UIs infront of camera
            foreach (GameObject ui in allUIs)
            {
                ui.transform.position = transform.position + transform.forward * 0.40f;
                ui.transform.rotation = new Quaternion(0.0f, transform.rotation.y, 0.0f, transform.rotation.w);
            }

            yield return new WaitForEndOfFrame();
        }
    }

    public void SetInMinigame(bool status)
    {
        inMinigame = status;
    }

    /* Check if two vectors are approximately equal,
    adapted from https://answers.unity.com/questions/395513/vector3-comparison-efficiency-and-float-precision.html */
    public bool V3Equal(Vector3 a, Vector3 b)
    {
        return Vector3.SqrMagnitude(a - b) < 0.001;
    }

    // Used for resetting UIs back to original position after being altered when playing minigame
    public void ResetUIPositions()
    {
        for (int i = 0; i < startPositions.Length; i++)
        {
            allUIs[i].transform.position = startPositions[i];
            allUIs[i].transform.localRotation = Quaternion.Euler(90, 0, 0);
        }
    }
}
