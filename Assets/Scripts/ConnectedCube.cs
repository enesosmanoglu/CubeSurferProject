using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ConnectedCube : MonoBehaviour
{
    float halfSize;
    private void Awake()
    {
        halfSize = transform.localScale.x * .5f;
        AudioSource.PlayClipAtPoint(GameManager.Instance.pickCubeSound, transform.position);
    }
    private void Update()
    {
        if (GameManager.Instance.isGamePaused) return;

        if (tag == "Untagged") return;

        SyncPosWithPlayer();

        WallAndFinishRaycast();
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

    private void WallAndFinishRaycast()
    {
        LayerMask layerMask = GameManager.Instance.cubeForwardRayLayerMask;

        Vector3 left = transform.position + Vector3.left * halfSize;
        Vector3 right = transform.position + Vector3.right * halfSize;
        Vector3 direction = Vector3.forward;
        Debug.DrawRay(left, direction * halfSize);
        Debug.DrawRay(right, direction * halfSize);

        Vector3 leftForward = transform.position + (Vector3.left + Vector3.forward) * halfSize;
        Vector3 leftBack = transform.position + (Vector3.left + Vector3.back) * halfSize;
        Vector3 direction2 = Vector3.right;
        Debug.DrawRay(leftForward, direction2 * halfSize);
        Debug.DrawRay(leftBack, direction2 * halfSize);

        Vector3 rightForward = transform.position + (Vector3.right + Vector3.forward) * halfSize;
        Vector3 rightBack = transform.position + (Vector3.right + Vector3.back) * halfSize;
        Vector3 direction3 = Vector3.left;
        Debug.DrawRay(rightForward, direction3 * halfSize);
        Debug.DrawRay(rightBack, direction3 * halfSize);

        RaycastHit hitResult;
        if (
            Physics.Raycast(left, direction, out hitResult, halfSize, layerMask)
            || Physics.Raycast(right, direction, out hitResult, halfSize, layerMask)
            || Physics.Raycast(leftForward, direction2, out hitResult, halfSize, layerMask)
            || Physics.Raycast(leftBack, direction2, out hitResult, halfSize, layerMask)
            || Physics.Raycast(rightForward, direction3, out hitResult, halfSize, layerMask)
            || Physics.Raycast(rightBack, direction3, out hitResult, halfSize, layerMask)
        )
        {
            Debug.Log(name + " hit a " + LayerMask.LayerToName(hitResult.transform.gameObject.layer));
            tag = "Untagged";
            GameManager.Instance.cubes.Remove(transform);
            StartCoroutine(DestroyObj());

            if (hitResult.transform.CompareTag("FinishStair"))
            {
                GameManager.Instance.AdjustCameraPos((hitResult.collider.transform.position.y - GameManager.Instance.finishStairStart.position.y));
            }
        }
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
            GameManager.Instance.DestroyCube(transform);
            AudioSource.PlayClipAtPoint(GameManager.Instance.cubeDestroySound, transform.position);
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
                GameManager.Instance.AddCube(child);
            }
            GameManager.Instance.AddCube(other.transform);
        }
        else if (other.gameObject.CompareTag("Finish"))
        {
            Debug.Log("YOU WIN");
            GameManager.Instance.levelPassed = true;
            foreach (Transform cube in GameManager.Instance.cubes)
            {
                StartCoroutine(cube.GetComponent<ConnectedCube>().DestroyObj());
            }
            Destroy(GameManager.Instance.player.GetComponent<PlayerMovement>());
            GameManager.Instance.GameOver();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Diamond"))
        {
            // Destroy(other.gameObject);
            GameManager.Instance.HitDiamond(other.gameObject);
        }
    }

    public IEnumerator DestroyObj()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }
}
