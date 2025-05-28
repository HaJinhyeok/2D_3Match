using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatus
{
    public int PlayerScore;
    public string PlayerName = "";
    public int RivalScore;
    public string RivalName = "";
    // 상대방이 게임 진행 중인지 여부
    public bool IsRivalPlaying = false;

    public string GameResult;

    public void OnRivalConnectionError()
    {
        RivalScore = 0;
        GameResult = Define.WinText;
    }

    public void OnGameFinish(int playerScore, int rivalScore, string result)
    {
        PlayerScore = playerScore;
        RivalScore = rivalScore;
        GameResult = result;
    }

    public void OnResultSetting(ushort result)
    {
        switch ((PacketType)result)
        {
            case PacketType.PACKET_RESULT_WIN:
                GameResult = Define.WinText;
                break;

            case PacketType.PACKET_RESULT_LOSE:
                GameResult = Define.LoseText;
                break;

            case PacketType.PACKET_RESULT_DRAW:
                GameResult = Define.DrawText;
                break;

            default:
                break;
        }
    }

    public void OnResultSetting(PacketType result)
    {
        switch (result)
        {
            case PacketType.PACKET_RESULT_WIN:
                GameResult = Define.WinText;
                break;

            case PacketType.PACKET_RESULT_LOSE:
                GameResult = Define.LoseText;
                break;

            case PacketType.PACKET_RESULT_DRAW:
                GameResult = Define.DrawText;
                break;
        }

    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    private static Client client = null;
    private GameManager() { }

    public GameObject BlockPrefab;
    public Sprite[] BlockImages;
    public GameStatus GameStatus = new GameStatus();

    public PlayerBoard Player;
    public RivalBoard Rival;

    public static bool s_isNetworkOn = false;
    public static bool s_isFinished = false;

    bool isPaused = false;
    public bool IsPaused
    {
        get { return isPaused; }
        set
        {
            isPaused = value;
            if (!isPaused)
            {
                PausePanel.OnPauseOff?.Invoke();
            }
        }
    }

    float _currentTime;

    public float CurrentTime
    {
        get { return _currentTime; }
        set
        {
            _currentTime = value;
            TimeProgressBar.OnTimeChange?.Invoke();
        }
    }

    public static GameManager Instance
    {
        get
        {
            if (instance == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject("@Managers");
                    DontDestroyOnLoad(go);
                }

                instance = FindAnyObjectByType<GameManager>();
                if (instance == null)
                {
                    GameObject gameManager = new GameObject("GameManager");
                    GameManager comp = gameManager.AddComponent<GameManager>();
                    gameManager.transform.SetParent(go.transform);
                    instance = comp;
                }
            }
            return instance;
        }
    }

    public static Client Client
    {
        get
        {
            if (client == null)
            {
                GameObject go = GameObject.Find("@Managers");
                if (go == null)
                {
                    go = new GameObject("@Managers");
                    DontDestroyOnLoad(go);
                }

                client = FindAnyObjectByType<Client>();
                if (client == null)
                {
                    GameObject gameClient = new GameObject("Client");
                    Client tmp = gameClient.AddComponent<Client>();
                    gameClient.AddComponent<ClientReceiveProcessor>();
                    gameClient.transform.SetParent(go.transform);
                    client = tmp;
                }
            }

            return client;
        }
    }

    void Awake()
    {
        LoadResources();
        GameInitialize();
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        LoadBoards();
    }

    void LoadResources()
    {
        BlockPrefab = Resources.Load<GameObject>(Define.BlockPath);
        BlockImages = Resources.LoadAll<Sprite>(Define.BlockImagePath);
    }

    public void GameInitialize()
    {
        CurrentTime = Define.TimeLimit;
        GameStatus.PlayerScore = 0;
        GameStatus.RivalScore = 0;
        s_isFinished = false;
    }

    // 씬 변경 시마다 로드해주는 게 좋음
    public void LoadBoards()
    {
        Player = FindAnyObjectByType<PlayerBoard>();
        Rival = FindAnyObjectByType<RivalBoard>();
    }
}
