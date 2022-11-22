using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Diamond : MonoBehaviour
{
    public bool isTaken = false;
    public void OnCollision()
    {
        if (!isTaken)
        {
            isTaken = true;
            AudioSource.PlayClipAtPoint(GameManager.Instance.diamondSound, transform.position);
            GameManager.Instance.addScore();
            GetComponentInChildren<MeshRenderer>().enabled = false;
            GetComponentInChildren<ParticleSystem>().Play();
        }
    }
}
