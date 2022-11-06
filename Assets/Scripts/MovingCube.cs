using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MovingCube : MonoBehaviour
{
    private void Update()
    {
        // if (tag != "Cubez") return;
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        transform.position = new Vector3(
            playerPos.x,
            transform.position.y,
            playerPos.z
        );

        RaycastHit hitResult;
        if (Physics.Linecast(transform.position + Vector3.down * 0.2f + Vector3.right * 0.2f, transform.position + Vector3.up * 0.2f + Vector3.left * 0.2f + Vector3.forward, out hitResult))
        {
            Debug.Log($"Linecast hitted: {hitResult.collider.name}");
            if (hitResult.collider.CompareTag("Wall") || hitResult.collider.CompareTag("FinishStair"))
            {
                Destroy(this);
                tag = "Untagged";
                GameManager.Instance.cubes.Remove(transform);

                if (hitResult.collider.CompareTag("FinishStair"))
                {
                    Debug.Log(Vector3.up * (hitResult.collider.transform.position.y - GameManager.Instance.finishStairStart.position.y));
                    GameManager.Instance.AdjustCameraPos((hitResult.collider.transform.position.y - GameManager.Instance.finishStairStart.position.y));
                }

            }
        }
    }
    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(name + " collided with " + other.gameObject.name);

        if (other.gameObject.CompareTag("Cube"))
        {
            other.transform.position = transform.position;
            GameManager.Instance.AddCube(other.transform);
            // other.transform.position = transform.position + Vector3.up * 1.1f;
        }
        else if (other.gameObject.CompareTag("Finish"))
        {
            Debug.Log("YOU WIN");
            Destroy(this);
            Destroy(GameManager.Instance.player.GetComponent<PlayerMovement>());
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
            Destroy(gameObject);
        }
    }

}
