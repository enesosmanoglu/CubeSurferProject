using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    Vector3 startPos = Vector3.zero;
    Vector3 touchStart;
    Vector3 touchRelative;
    Vector3 touchStartPlayer;

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
        Vector3 moveVec = Vector3.forward * Time.deltaTime * GameManager.Instance.playerMoveSpeed;
        transform.Translate(moveVec);
        cam.transform.Translate(moveVec, Space.World);
        GameManager.Instance.scoreText.transform.Translate(moveVec, Space.World);

        if (Input.GetMouseButtonDown(0))
        {
            Debug.Log("Mousedown");
            touchStart = Input.mousePosition;
            touchStartPlayer = transform.position;
            Debug.Log(touchStart);
            Debug.Log(touchStartPlayer);
        }
        if (Input.GetMouseButton(0))
        {
            touchRelative = (Input.mousePosition - touchStart) / 100;
            if (touchRelative.x != 0)
            {
                transform.position = new Vector3(
                    Mathf.Clamp(touchStartPlayer.x + touchRelative.x, -2f, +2f),
                    transform.position.y,
                    transform.position.z
                );
            }
        }
        if (Input.GetMouseButtonUp(0))
        {
            Debug.Log("Mouseup");
        }

        RaycastHit hitResult;
        if (Physics.Linecast(transform.position + Vector3.down * 0.2f + Vector3.right * 0.2f, transform.position + Vector3.up * 0.2f + Vector3.left * 0.2f + Vector3.forward * 0.5f, out hitResult))
        {
            Debug.Log($"Player hitted: {hitResult.collider.name}");
            if (hitResult.collider.CompareTag("Wall"))
            {

            }
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            GameManager.Instance.playerRb.constraints = RigidbodyConstraints.None;
            Debug.Log((other.transform.position - GameManager.Instance.player.transform.position));
            GameManager.Instance.playerRb.AddForce(300f * (other.transform.position - GameManager.Instance.player.transform.position));
            Debug.Log("YOU LOSE");
            Destroy(this);
            GameManager.Instance.GameOver();
            // GameManager.Instance.playerAnim.SetBool("isGameOver", true);
        }
        else if (other.gameObject.CompareTag("Cube"))
        {
            GameManager.Instance.AddCube(other.transform);
            other.transform.position = new Vector3(
                transform.position.x,
                1f,
                transform.position.z
            ); // Cube
        }
        else if (other.gameObject.CompareTag("Finish") || other.gameObject.CompareTag("FinishStair"))
        {
            Debug.Log("YOU WIN");
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
            GameManager.Instance.DestroyCube(transform);
            Debug.Log("YOU LOSE");
            Destroy(this);
            GameManager.Instance.GameOver();
        }
    }
}
