using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class GameManager : MonoBehaviour
{
    #region Singleton
    public static GameManager Instance { get; private set; }
    private void Singleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }
    #endregion

    [Header("Global Objects")]
    public GameObject player = null;
    public Transform cubesParent = null;
    public GameObject trail = null;
    public Transform finishStairStart = null;
    public TMP_Text scoreText = null;

    [Header("Global Settings")]
    public float playerMoveSpeed = 0.2f;

    [Header("Don't Touch")]
    public Rigidbody playerRb = null;
    public Animator playerAnim = null;
    public Camera cam;
    public Vector3 camStartPos = Vector3.zero;
    public Vector3 trailStartPos = Vector3.zero;

    [Space(10)]
    public List<Transform> cubes;
    public int diamonds = 0;

    public bool isGameOver = false;
    private void Awake()
    {
        Singleton();
        cam = Camera.main;
        playerRb = player.GetComponent<Rigidbody>();
        playerAnim = player.GetComponent<Animator>();
        cubes = new List<Transform>();
    }
    private void Start()
    {
        camStartPos = cam.transform.position;
        trailStartPos = trail.transform.position;
    }

    private void Update()
    {
        if (isGameOver)
        {
            if (Input.GetMouseButton(0))
            {
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            }
        }
    }

    public void AdjustCameraPos(float h = 0)
    {
        cam.transform.position = new Vector3(
            cam.transform.position.x,
            camStartPos.y + h,
            cam.transform.position.z
        );

        trail.transform.position = new Vector3(
            trail.transform.position.x,
            trailStartPos.y + h,
            trail.transform.position.z
        );
    }
    public void AddScore(int c = 1)
    {
        diamonds += c;
        scoreText.SetText(diamonds.ToString());
    }

    public void AddCube(Transform cube)
    {
        cube.SetParent(cubesParent);
        // playerRb.velocity = Vector3.zero;
        // playerRb.AddForce(Vector3.up * 300f);
        if (cubes.Count > 0)
            player.transform.position = cubes[0].position + Vector3.up * 3f;
        else
            player.transform.position += Vector3.up * 1.1f;

        cube.position = new Vector3(
            player.transform.position.x,
            1f,
            player.transform.position.z
        );

        foreach (var c in cubes)
        {
            
            c.position += Vector3.up * 1.1f;
        }

        cubes.Add(cube);
        cube.gameObject.tag = "MovingCube";
        cube.gameObject.AddComponent<MovingCube>();
    }
    public void DestroyCube(Transform cube)
    {
        // playerRb.velocity = Vector3.zero;
        // playerRb.AddForce(Vector3.up * 300f);
        // player.transform.position -= Vector3.up;
        cubes.Remove(cube);
        Destroy(cube.gameObject.GetComponent<BoxCollider>());
        Destroy(cube.gameObject.GetComponent<MovingCube>());
        // cube.position = new Vector3(-20, -20, -20);
        StartCoroutine("DestroyObjs", new GameObject[] { cube.gameObject });
        // Destroy(cube.gameObject);
    }

    public void GameOver()
    {
        StartCoroutine(GameOverEnum());
    }
    IEnumerator GameOverEnum()
    {
        //Print the time of when the function is first called.
        Debug.Log("Started Coroutine at timestamp : " + Time.time);
        yield return new WaitForSeconds(2f);
        //After we have waited 5 seconds print the time again.
        Debug.Log("Finished Coroutine at timestamp : " + Time.time);
        GameManager.Instance.isGameOver = true;
    }
    IEnumerator DestroyObjs(GameObject[] objs)
    {
        yield return new WaitForSeconds(3f);
        foreach (GameObject obj in objs)
            Destroy(obj);
    }


}
