using Cinemachine;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    private UIManager _uiManager;
    public UIManager UIManager => _uiManager;
    private LevelManager _levelManager;
    public LevelManager LevelManager => _levelManager;

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
        _uiManager = FindObjectOfType<UIManager>();
        _levelManager = FindObjectOfType<LevelManager>();
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

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            QuitGame();
        }
        else if (Input.GetKeyDown(KeyCode.F5))
        {
            _levelManager.Save();
        }
        else if (Input.GetKeyDown(KeyCode.F9))
        {
            _levelManager.Load();
        }
    }
}