using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuPlayerMovement : MonoBehaviour
{
    Vector3 startPos = Vector3.zero;
    Vector3 touchStart;
    Vector3 touchRelative;
    Vector3 touchStartPlayer;
    Vector3 touchStartCam;

    Camera cam;

    private void Awake()
    {
        cam = Camera.main;
    }
    private void Start()
    {
        startPos = transform.position;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mousedown");
            touchStart = Input.mousePosition;
            touchStartPlayer = transform.position;
            touchStartCam = cam.transform.position;
            Debug.Log(touchStart);
            Debug.Log(touchStartPlayer);
        }
        if (Input.GetMouseButton(0))
        {
            touchRelative = (Input.mousePosition - touchStart) / 100;
            if (touchRelative.x != 0)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(touchStartPlayer.x + touchRelative.x, -3f, +3f),
                    transform.position.y,
                    transform.position.z
                );
                cam.transform.position = new Vector3(
                    Mathf.Clamp(touchStartCam.x + touchRelative.x / 10, -3f, +3f),
                    cam.transform.position.y,
                    cam.transform.position.z
                );
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouseup");
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cube"))
        {
            if (other.transform.parent.CompareTag("Cube")) return;
            for (int i = 0; i < other.transform.childCount; i++)
            {
                Transform child = other.transform.GetChild(other.transform.childCount - 1 - i);
                MainMenuGameManager.Instance.AddCube(child);
            }
            MainMenuGameManager.Instance.AddCube(other.transform);
        }
    }
}
