using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeMovingBack : MonoBehaviour
{
    float halfSize;
    private void Awake()
    {
        halfSize = transform.localScale.x * .5f;
    }
    void Update()
    {
        if (GameManager.Instance.isGamePaused) return;
        
        LavaRaycast();

        Vector3 moveVec = Vector3.back * Time.deltaTime * GameManager.Instance.playerForwardSpeed;
        transform.Translate(moveVec);

        // float playerDist = GameManager.Instance.player.transform.position.z - transform.position.z;
        // if (playerDist > 5f)
        // {
        //     Transform lastChild = transform.parent.GetChild(transform.parent.childCount - 1);
        //     transform.position = lastChild.position + Vector3.forward * GameManager.Instance.cubeSpawnMargin;
        //     transform.SetAsLastSibling();
        // }
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
            StartCoroutine(DestroyEnum());
            transform.parent = null;
            GameManager.Instance.SpawnCube();
        }
    }
    IEnumerator DestroyEnum()
    {
        if (GameManager.Instance.playerForwardSpeed == 0f)
        {
            Destroy(this);
            yield break;
        }
        yield return new WaitForSeconds(1 / GameManager.Instance.playerForwardSpeed);
        Destroy(this);
    }
}
