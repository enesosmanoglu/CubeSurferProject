using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMovement : MonoBehaviour
{
    Vector3 startPos = Vector3.zero;
    Vector3 touchStart;
    Vector3 touchRelative;
    Vector3 playerPosOnTouch;
    Vector3 camPosOnTouch;

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
        if (SceneManager.GetActiveScene().buildIndex != 0)
            MoveForward();

        MoveHorizontal();
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Cube"))
        {
            if (other.transform.parent.CompareTag("Cube")) return;
            for (int i = 0; i < other.transform.childCount; i++)
            {
                Transform child = other.transform.GetChild(other.transform.childCount - 1 - i);
                GameManager.Instance.AddCube(child);
            }
            GameManager.Instance.AddCube(other.transform);
        }
        else if (other.gameObject.CompareTag("Wall"))
        {
            Debug.Log("YOU LOSE");
            GameManager.Instance.playerRb.constraints = RigidbodyConstraints.None;
            GameManager.Instance.playerRb.AddForce(300f * (other.transform.position - transform.position).normalized);
            Destroy(this);
            GameManager.Instance.GameOver();
            // GameManager.Instance.playerAnim.SetBool("isGameOver", true);
        }
        else if (other.gameObject.CompareTag("Finish") || other.gameObject.CompareTag("FinishStair"))
        {
            Debug.Log("YOU WIN");
            GameManager.Instance.levelPassed = true;
            Destroy(this);
            GameManager.Instance.GameOver();
        }
    }
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Diamond"))
        {
            Destroy(other.gameObject);
            GameManager.Instance.AddScore();
        }
        else if (other.gameObject.CompareTag("Lava"))
        {
            if (SceneManager.GetActiveScene().buildIndex == 0) return;

            Debug.Log("YOU LOSE");
            GameManager.Instance.DestroyPlayer();
            Destroy(this);
            GameManager.Instance.GameOver();
        }
    }

    private void MoveForward()
    {
        Vector3 moveVec = Vector3.forward * Time.deltaTime * GameManager.Instance.playerForwardSpeed;
        transform.Translate(moveVec);
        cam.transform.Translate(moveVec, Space.World);
        GameManager.Instance.scoreText.transform.Translate(moveVec, Space.World);
        GameManager.Instance.trail.transform.position = new Vector3(
            transform.position.x,
            GameManager.Instance.trail.transform.position.y,
            transform.position.z
        );
    }
    private void MoveHorizontal()
    {
        if (Input.GetMouseButtonDown(0))
        {
            touchStart = Input.mousePosition;
            playerPosOnTouch = transform.position;
            camPosOnTouch = cam.transform.position;
        }
        if (Input.GetMouseButton(0))
        {
            touchRelative = (Input.mousePosition - touchStart) / 100;
            touchRelative.x *= GameManager.Instance.playerHorizontalSpeed;
            if (touchRelative.x != 0)
            {
                int bounds = (int)(GameManager.Instance.platform.localScale.x / 2);
                transform.position = new Vector3(
                    Mathf.Clamp(playerPosOnTouch.x + touchRelative.x, -bounds, +bounds),
                    transform.position.y,
                    transform.position.z
                );
                cam.transform.position = new Vector3(
                    Mathf.Clamp(camPosOnTouch.x + touchRelative.x / 10, -bounds, +bounds),
                    cam.transform.position.y,
                    cam.transform.position.z
                );
            }
        }
    }
}
