using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
public class MainMenuGameManager : MonoBehaviour
{
    #region Singleton
    public static MainMenuGameManager Instance { get; private set; }
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
    public GameObject mainMenuCubePrefab;
    public TMP_Text infoText;

    [Header("Global Settings")]
    [Range(1, 50)] public int cubeCountToStartGame = 10;
    public Vector3 cubeSpawnPoint = new Vector3(2f, 1f, 10f);
    [Range(0, 3)] public float cubeSpawnRate = 0.5f;
    [Range(1, 20)] public int cubeSpawnMargin = 4;
    public int spawnCountOnStart = 20;
    public int spawnIndex = 0;


    [Header("Don't Touch")]


    [Space(10)]
    public List<Transform> cubes;

    private void Awake()
    {
        Singleton();
        cubes = new List<Transform>();
    }
    private void Start()
    {
        for (int i = 0; i < spawnCountOnStart; i++, spawnIndex++)
        {
            CreateCube();
        }
        spawnIndex--;
        StartCoroutine(CreateCubeTimer());
    }

    private void Update()
    {
        int remainingCubes = (cubeCountToStartGame - cubes.Count);
        infoText.text = "Collect <color=\"red\"><size=220%>" + remainingCubes + "</size></color> more cubes to start game";

        if (remainingCubes == 0)
        {
            SceneManager.LoadScene("Level01");
        }
    }
    public void CreateCube()
    {
        Instantiate(mainMenuCubePrefab, cubeSpawnPoint + Vector3.forward * cubeSpawnMargin * spawnIndex, Quaternion.identity, GameManager.Instance.cubesParent);
    }

    public void AddCube(Transform cube)
    {
        cube.position = new Vector3(
            GameManager.Instance.player.transform.position.x,
            1f,
            GameManager.Instance.player.transform.position.z
        );

        for (int i = 0; i < cubes.Count; i++)
        {
            Transform c = cubes[cubes.Count - 1 - i];
            c.position = cube.position + (i + 1) * 1.1f * Vector3.up;
        }

        if (cubes.Count > 0)
            GameManager.Instance.player.transform.position = cubes[0].position + Vector3.up * 1.5f;
        else
            GameManager.Instance.player.transform.position += Vector3.up * 1.01f;

        cubes.Add(cube);
        cube.gameObject.tag = "MovingCube";
        cube.gameObject.AddComponent<MainMenuMovingCube>();
    }
    public void DestroyCube(Transform cube)
    {
        // playerRb.velocity = Vector3.zero;
        // playerRb.AddForce(Vector3.up * 300f);
        // player.transform.position -= Vector3.up;
        cubes.Remove(cube);
        Destroy(cube.gameObject.GetComponent<BoxCollider>());
        Destroy(cube.gameObject.GetComponent<MainMenuMovingCube>());
        // cube.position = new Vector3(-20, -20, -20);
        StartCoroutine("DestroyObjs", new GameObject[] { cube.gameObject });
        // Destroy(cube.gameObject);
    }
    IEnumerator DestroyObjs(GameObject[] objs)
    {
        yield return new WaitForSeconds(3f);
        foreach (GameObject obj in objs)
            Destroy(obj);
    }

    IEnumerator CreateCubeTimer()
    {
        yield return new WaitForSeconds(cubeSpawnRate);
        CreateCube();
        StartCoroutine(CreateCubeTimer());
    }

}
