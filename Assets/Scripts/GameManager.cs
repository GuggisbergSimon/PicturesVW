using System;
using System.IO;
using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private PlayerController _player;
    public PlayerController Player => _player;

    private CinemachineVirtualCamera _vCamera;
    public CinemachineVirtualCamera VCamera => _vCamera;

    private UIManager _uiManager;
    public UIManager UIManager => _uiManager;

    /*private PortableCamera _pCamera;
    public PortableCamera PCamera => _pCamera;*/
    public static GameManager Instance { get; private set; }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnLevelFinishedLoadingScene;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnLevelFinishedLoadingScene;
    }

    //this function is activated every time a scene is loaded
    private void OnLevelFinishedLoadingScene(Scene scene, LoadSceneMode mode)
    {
        Setup();
    }

    private void Setup()
    {
        //_player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();
        _player = FindObjectOfType<PlayerController>();
        _vCamera = FindObjectOfType<CinemachineVirtualCamera>();
        _uiManager = FindObjectOfType<UIManager>();
        //_pCamera = FindObjectOfType<PortableCamera>();
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        Setup();
    }

    public void RestartLevel()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void LoadLevel(string nameLevel)
    {
        SceneManager.LoadScene(nameLevel);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
		Application.Quit();
#endif
    }

    //code taken from Catlikecoding : https://catlikecoding.com/unity/tutorials/hex-map/part-12/
    public void Save()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.me");
        using (BinaryWriter writer = new BinaryWriter(File.Open(path, FileMode.Create)))
        {
            writer.Write((double) Player.transform.position.x);
            writer.Write((double) Player.transform.position.y);
            writer.Write((double) Player.transform.position.z);
        }
    }

    public void Load()
    {
        string path = Path.Combine(Application.persistentDataPath, "save.me");
        using (BinaryReader reader = new BinaryReader(File.OpenRead(path)))
        {
            Player.transform.position = Vector3.right * (float) reader.ReadDouble() +
                                        Vector3.up * (float) reader.ReadDouble() +
                                        Vector3.forward * (float) reader.ReadDouble();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            Save();
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            Load();
        }
    }
}