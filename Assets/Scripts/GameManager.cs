using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
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
    public bool isGamePaused = false;
    public bool levelPassed = false;

    [Header("Main Menu Settings")]
    [Range(1, 50)] public int requiredCubeCountToStartGame = 8;
    public Vector3 cubeSpawnPoint = new Vector3(2f, 1f, 10f);
    [Range(1, 20)] public int cubeSpawnMargin = 4;
    public int spawnCountOnStart = 100;
    [Range(0, 16)] public float mainMenuPlayerInitForwardSpeed = 10f;

    [Header("Drag and drop from files")]
    public GameObject cubePrefab = null;
    public AudioClip pickCubeSound = null;
    public AudioClip hitWallSound = null;
    public AudioClip diamondSound = null;
    public AudioClip levelPassedSound = null;
    public AudioClip cubeDestroySound = null;

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
    public Button pauseButton;
    public Button pausePanelButton;
    public TMP_Text diamondCountText = null;
    public Button tryAgainButton;
    public Button nextLevelButton;
    public Button mainLevelButton;
    public GameObject gameOverPanel;

    [Space(5)]
    public List<Transform> cubes;

    private float playerForwardSpeedCache = -1f;
    private Vector3 sitAnimOffset = new Vector3(0, -0.41f, 0.31f);
    public void onPauseClick()
    {
        Debug.Log("You have clicked the pause button!");
        bool isPausedNow = Time.timeScale == 0;
        Time.timeScale = isPausedNow ? 1 : 0;
        pauseButton.gameObject.SetActive(isPausedNow);
        pausePanelButton.gameObject.SetActive(!isPausedNow);
        isGamePaused = !isPausedNow;
    }
    private void OnEnable()
    {
        SceneManager.sceneLoaded += (Scene scene, LoadSceneMode loadSceneMode) =>
        {
            diamonds = PlayerPrefs.GetInt("diamonds", 0);

            activeScene = scene;
            Debug.Log("Scene changed to " + scene.buildIndex);
            if (scene.buildIndex != 0)
                PlayerPrefs.SetInt("lastLevelScene", scene.buildIndex);

            player = GameObject.FindGameObjectWithTag("Player");
            platform = GameObject.FindGameObjectWithTag("Platform")?.transform;
            cubesParent = GameObject.FindGameObjectWithTag("CubesParent")?.transform;
            connectedCubesParent = GameObject.FindGameObjectWithTag("ConnectedCubesParent")?.transform;
            trail = GameObject.FindGameObjectWithTag("Trail");
            finishStairStart = GameObject.FindGameObjectWithTag("StairStart")?.transform;
            scoreText = GameObject.FindGameObjectWithTag("ScoreText")?.GetComponent<TMP_Text>();
            if (scoreText != null) scoreText.text = diamonds.ToString();
            pauseButton = GameObject.FindGameObjectWithTag("PauseButton")?.GetComponent<Button>();
            pausePanelButton = GameObject.FindGameObjectWithTag("PausePanel")?.GetComponent<Button>();
            pauseButton?.gameObject.SetActive(true);
            pausePanelButton?.gameObject.SetActive(false);
            pauseButton?.onClick.AddListener(onPauseClick);
            pausePanelButton?.onClick.AddListener(onPauseClick);
            diamondCountText = GameObject.FindGameObjectWithTag("DiamondCount")?.GetComponentInChildren<TMP_Text>();
            if (diamondCountText != null) diamondCountText.text = diamonds.ToString();
            tryAgainButton = GameObject.FindGameObjectWithTag("TryAgainButton")?.GetComponent<Button>();
            tryAgainButton?.onClick.AddListener(() =>
            {
                Debug.Log("Try again button!");
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
            });
            nextLevelButton = GameObject.FindGameObjectWithTag("NextLevelButton")?.GetComponent<Button>();
            nextLevelButton?.onClick.AddListener(() =>
            {
                Debug.Log("Next level button!");
                int buildIndex = SceneManager.GetActiveScene().buildIndex;
                int loadSceneIndex = levelPassed ? (buildIndex + 1) % SceneManager.sceneCountInBuildSettings : buildIndex;
                if (loadSceneIndex == 0) loadSceneIndex = 1;
                SceneManager.LoadScene(loadSceneIndex);
            });
            mainLevelButton = GameObject.FindGameObjectWithTag("MainLevelButton")?.GetComponent<Button>();
            mainLevelButton?.onClick.AddListener(() =>
            {
                Debug.Log("Main level button!");
                SceneManager.LoadScene("Level00");
            });
            gameOverPanel = GameObject.FindGameObjectWithTag("GameOverPanel");
            gameOverPanel?.SetActive(false);

            cam = Camera.main;
            playerRb = player.GetComponent<Rigidbody>();
            playerAnim = player.GetComponentInChildren<Animator>();
            cubes = new List<Transform>();

            camStartPos = cam.transform.position;
            trailStartPos = trail.transform.position;

            isGameOver = false;
            levelPassed = false;
            isGamePaused = false;
            Time.timeScale = 1;

            if (scene.buildIndex == 0)
            {
                playerForwardSpeedCache = playerForwardSpeed;
                playerForwardSpeed = mainMenuPlayerInitForwardSpeed;
                for (int i = 0; i < spawnCountOnStart; i++)
                    SpawnCube();
            }
            else
            {
                for (int i = 0; i < startCubeCount; i++)
                {
                    GameObject cube = Instantiate(cubePrefab, Vector3.zero, Quaternion.identity, cubesParent);
                    AddCube(cube.transform);
                }

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
    }

    private void Update()
    {
        if (activeScene.buildIndex == 0)
        {
            int nextLevel = PlayerPrefs.GetInt("lastLevelScene", 0);
            bool isFirstRun = nextLevel == 0;

            int remainingCubes = (requiredCubeCountToStartGame - cubes.Count);
            
            if (remainingCubes > 0)
                scoreText.text = "Collect <color=\"red\"><size=220%>" + remainingCubes + "</size></color> cube"
                               + (remainingCubes > 1 ? "s" : "") + " to " + (isFirstRun ? "start" : "continue");
            else
                scoreText.text = "Loading...";

            if (remainingCubes == 0)
            {
                SceneManager.LoadScene(isFirstRun ? 1 : nextLevel);
            }
            return;
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
    public void HitDiamond(GameObject diamondObject)
    {
        diamondObject?.GetComponent<Diamond>().OnCollision();
    }
    public void addScore()
    {
        diamonds += 1;
        scoreText?.SetText(diamonds.ToString());
        diamondCountText?.SetText(diamonds.ToString());
        PlayerPrefs.SetInt("diamonds", diamonds);
    }

    public void AddCube(Transform cube)
    {
        if (playerAnim?.GetBool("hasBox") == false)
        {
            playerAnim?.SetBool("hasBox", true);
            playerAnim.transform.Translate(sitAnimOffset);
        }
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
        if (cubes.Count == 0)
        {
            if (playerAnim?.GetBool("hasBox") == true)
            {
                playerAnim?.SetBool("hasBox", false);
                playerAnim?.transform.Translate(-sitAnimOffset);
            }
        }
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
        if (playerAnim?.GetBool("hasBox") == true)
            playerAnim?.transform.Translate(-sitAnimOffset);
        if (levelPassed)
        {
            PlayerPrefs.SetInt("lastLevelScene", activeScene.buildIndex + 1);
            playerAnim?.SetTrigger("levelPassed");
            AudioSource.PlayClipAtPoint(GameManager.Instance.levelPassedSound, player.transform.position);
        }
        else
        {
            playerAnim?.SetTrigger("gameOver");
            AudioSource.PlayClipAtPoint(GameManager.Instance.hitWallSound, player.transform.position);
        }
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
        gameOverPanel?.SetActive(true);
        tryAgainButton?.gameObject.SetActive(!levelPassed);
        nextLevelButton?.gameObject.SetActive(levelPassed);
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
