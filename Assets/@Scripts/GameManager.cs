using UnityEngine;

public class GameStatus
{
    public int PlayerScore;
    public string PlayerName = "Player";
    public int RivalScore;
    public string RivalName = "Rival";

    public string GameResult;

    public void OnRivalConnectionError()
    {
        RivalScore = 0;
        GameResult = Define.VictoryText;
    }
}

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    private GameManager() { }

    public GameObject BlockPrefab;
    public Sprite[] BlockImages;
    public GameStatus GameStatus=new GameStatus();

    PlayerBoard _playerBoard;
    RivalBoard _rivalBoard;

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
        _playerBoard = FindAnyObjectByType<PlayerBoard>();
        _rivalBoard = FindAnyObjectByType<RivalBoard>();
    }

    void LoadResources()
    {
        BlockPrefab = Resources.Load<GameObject>(Define.BlockPath);
        BlockImages = Resources.LoadAll<Sprite>(Define.BlockImagePath);
    }

}
