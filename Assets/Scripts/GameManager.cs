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
            Destroy(this.gameObject);
            Destroy(this);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(this.gameObject);
        }
    }
    #endregion

    [Header("Global Settings")]
    [Range(1, 10)] public int startCubeCount = 1;
    [Range(0, 16)] public float playerForwardSpeed = 7f;
    [Range(0, 10)] public float playerHorizontalSpeed = 1f;
    public LayerMask cubeForwardRayLayerMask;
    public LayerMask cubeDownRayLayerMask;
    public int diamonds = 0;
    public bool isGameOver = false;
    public bool levelPassed = false;

    [Header("Drag and drop from files")]
    public GameObject cubePrefab = null;

    [Header("Will be assigned automatically at runtime")]
    public GameObject player = null;
    public Transform platform = null;
    public Transform cubesParent = null;
    public Transform connectedCubesParent = null;
    public GameObject trail = null;
    public Transform finishStairStart = null;
    public TMP_Text scoreText = null;
    public Rigidbody playerRb = null;
    public Animator playerAnim = null;
    public Camera cam;
    public Vector3 camStartPos = Vector3.zero;
    public Vector3 trailStartPos = Vector3.zero;
    public Scene activeScene;

    [Space(5)]
    public List<Transform> cubes;

    [Header("Main Menu Settings")]
    [Range(1, 50)] public int requiredCubeCountToStartGame = 10;
    public Vector3 cubeSpawnPoint = new Vector3(2f, 1f, 10f);
    [Range(1, 20)] public int cubeSpawnMargin = 4;
    public int spawnCountOnStart = 100;
    [Range(0, 16)] public float mainMenuPlayerInitForwardSpeed = 10f;
    private float playerForwardSpeedCache = -1f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadSceneMode) =>
        {
            activeScene = scene;
            Debug.Log("Scene changed to " + scene.buildIndex);
            player = GameObject.FindGameObjectWithTag("Player");
            platform = GameObject.FindGameObjectWithTag("Platform")?.transform;
            cubesParent = GameObject.FindGameObjectWithTag("CubesParent")?.transform;
            connectedCubesParent = GameObject.FindGameObjectWithTag("ConnectedCubesParent")?.transform;
            trail = GameObject.FindGameObjectWithTag("Trail");
            finishStairStart = GameObject.FindGameObjectWithTag("StairStart")?.transform;
            scoreText = GameObject.FindGameObjectWithTag("ScoreText")?.GetComponent<TMP_Text>();

            cam = Camera.main;
            playerRb = player.GetComponent<Rigidbody>();
            playerAnim = player.GetComponent<Animator>();
            cubes = new List<Transform>();

            camStartPos = cam.transform.position;
            trailStartPos = trail.transform.position;

            for (int i = 0; i < startCubeCount; i++)
            {
                GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, cubesParent);
                AddCube(cube.transform);
            }

            isGameOver = false;
            levelPassed = false;
            scoreText.text = diamonds.ToString();

            if (scene.buildIndex == 0)
            {
                playerForwardSpeedCache = playerForwardSpeed;
                playerForwardSpeed = mainMenuPlayerInitForwardSpeed;
                for (int i = 0; i < spawnCountOnStart; i++)
                    SpawnCube();
            }
            else
            {
                if (playerForwardSpeedCache >= 0 && playerForwardSpeedCache != playerForwardSpeed)
                {
                    playerForwardSpeed = playerForwardSpeedCache;
                    playerForwardSpeedCache = -1f;
                }
            }
        };
    }
    private void Awake()
    {
        Singleton();
    }

    public void Start()
    {
        Debug.Log("Game started");
        diamonds = PlayerPrefs.GetInt("diamonds", 0);
        scoreText.text = diamonds.ToString();
    }

    private void Update()
    {
        if (activeScene.buildIndex == 0)
        {
            int remainingCubes = (requiredCubeCountToStartGame - cubes.Count);
            scoreText.text = "Collect <color=\"red\"><size=220%>" + remainingCubes + "</size></color> more cubes to start game";

            if (remainingCubes == 0)
            {
                SceneManager.LoadScene("Level01");
            }
            return;
        }
        if (isGameOver)
        {
            if (Input.GetMouseButton(0))
            {
                int buildIndex = SceneManager.GetActiveScene().buildIndex;
                int loadSceneIndex = levelPassed ? (buildIndex + 1) % SceneManager.sceneCountInBuildSettings : buildIndex;
                if (loadSceneIndex == 0) loadSceneIndex = 1;
                SceneManager.LoadScene(loadSceneIndex);
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
        PlayerPrefs.SetInt("diamonds", diamonds);
    }

    public void AddCube(Transform cube)
    {
        cube.SetParent(connectedCubesParent);
        if (activeScene.buildIndex == 0)
            SpawnCube();
        // playerRb.velocity = Vector3.zero;
        // playerRb.AddForce(Vector3.up * 300f);
        cube.position = new Vector3(
            player.transform.position.x,
            1f,
            player.transform.position.z
        );

        for (int i = 0; i < cubes.Count; i++)
        {
            Transform c = cubes[cubes.Count - 1 - i];
            c.position = cube.position + (i + 1) * 1.1f * Vector3.up;
            c.GetComponent<Rigidbody>().velocity = Vector3.zero;
        }

        if (cubes.Count > 0)
            player.transform.position = cubes[0].position + Vector3.up * 1.5f;
        else
            player.transform.position += Vector3.up * 1.01f;

        cubes.Add(cube);
        cube.gameObject.tag = "ConnectedCube";
        cube.gameObject.AddComponent<ConnectedCube>();
    }
    public void DestroyCube(Transform cube)
    {
        // playerRb.velocity = Vector3.zero;
        // playerRb.AddForce(Vector3.up * 300f);
        // player.transform.position -= Vector3.up;
        cubes.Remove(cube);
        Destroy(cube.gameObject.GetComponent<BoxCollider>());
        Destroy(cube.gameObject.GetComponent<ConnectedCube>());
        // cube.position = new Vector3(-20, -20, -20);
        StartCoroutine("DestroyObjs", new GameObject[] { cube.gameObject });
        // Destroy(cube.gameObject);
    }
    public void DestroyPlayer()
    {
        Destroy(player.gameObject.GetComponent<CapsuleCollider>());
        StartCoroutine("DestroyObjs", new GameObject[] { player.gameObject });
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

    public GameObject SpawnCube()
    {
        Vector3 spawnPos = cubeSpawnPoint;
        if (cubesParent.childCount > 0)
        {
            Transform lastChild = cubesParent.GetChild(cubesParent.childCount - 1);
            spawnPos = lastChild.position + Vector3.forward * GameManager.Instance.cubeSpawnMargin;
        }
        GameObject cube = Instantiate(cubePrefab, spawnPos, Quaternion.identity, cubesParent);
        if (activeScene.buildIndex == 0)
            cube.AddComponent<CubeMovingBack>();
        return cube;
    }
}
