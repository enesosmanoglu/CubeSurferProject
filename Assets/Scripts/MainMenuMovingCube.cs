using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuMovingCube : MonoBehaviour
{
    float halfSize;
    private void Awake()
    {
        halfSize = transform.localScale.x * .5f;
    }
    private void Update()
    {
        SyncPosWithPlayer();

        LavaRaycast();
    }

    private void SyncPosWithPlayer()
    {
        Vector3 playerPos = GameManager.Instance.player.transform.position;
        transform.position = new Vector3(
            playerPos.x,
            transform.position.y,
            playerPos.z
        );
    }

    private void LavaRaycast()
    {
        Vector3 downForwardLeft = transform.position + (Vector3.down + Vector3.forward + Vector3.left) * halfSize;
        Vector3 downForwardRight = transform.position + (Vector3.down + Vector3.forward + Vector3.right) * halfSize;
        Vector3 downBackLeft = transform.position + (Vector3.down + Vector3.back + Vector3.left) * halfSize;
        Vector3 downBackRight = transform.position + (Vector3.down + Vector3.back + Vector3.right) * halfSize;
        Vector3 direction = Vector3.down;
        LayerMask layerMask = GameManager.Instance.cubeDownRayLayerMask;

        Debug.DrawRay(downForwardLeft, direction * halfSize);
        Debug.DrawRay(downForwardRight, direction * halfSize);
        Debug.DrawRay(downBackLeft, direction * halfSize);
        Debug.DrawRay(downBackRight, direction * halfSize);

        RaycastHit hitResult;
        if (
            Physics.Raycast(downForwardLeft, direction, out hitResult, halfSize, layerMask)
            && Physics.Raycast(downForwardRight, direction, out hitResult, halfSize, layerMask)
            && Physics.Raycast(downBackLeft, direction, out hitResult, halfSize, layerMask)
            && Physics.Raycast(downBackRight, direction, out hitResult, halfSize, layerMask)
        )
        {
            Debug.Log(name + " hit a lava");
            MainMenuGameManager.Instance.DestroyCube(transform);
        }
    }

    private void OnCollisionEnter(Collision other)
    {
        Debug.Log(name + " collided with " + other.gameObject.name);

        if (other.gameObject.CompareTag("Cube"))
        {
            if (other.transform.parent.CompareTag("Cube")) return;
            // other.transform.position = transform.position;
            for (int i = 0; i < other.transform.childCount; i++)
            {
                Transform child = other.transform.GetChild(other.transform.childCount - 1 - i);
                MainMenuGameManager.Instance.AddCube(child);
            }
            MainMenuGameManager.Instance.AddCube(other.transform);
            // other.transform.position = transform.position + Vector3.up * 1.1f;
        }

    }

}
