using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuCube : MonoBehaviour
{
    void Update()
    {
        Vector3 moveVec = Vector3.back * Time.deltaTime * GameManager.Instance.playerMoveSpeed;
        transform.Translate(moveVec);
    }
}
