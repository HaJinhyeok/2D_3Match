using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStatus
{
    public int PlayerScore;
    public string PlayerName = "Player";
    public int RivalScore;
    public string RivalName = "Rival";
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

    public void OnResultSetting(int result)
    {
        switch (result)
        {
            case 0:
                GameResult = Define.WinText;
                break;

            case 1:
                GameResult = Define.LoseText;
                break;

            case 2:
                GameResult = Define.DrawText;
                break;

            default:
                break;
        }
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    private GameManager() { }

    public GameObject BlockPrefab;
    public Sprite[] BlockImages;
    public GameStatus GameStatus = new GameStatus();

    PlayerBoard _playerBoard;
    RivalBoard _rivalBoard;

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

    void Awake()
    {
        LoadResources();
        GameInitialize();
        _playerBoard = FindAnyObjectByType<PlayerBoard>();
        _rivalBoard = FindAnyObjectByType<RivalBoard>();
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
    }
}
