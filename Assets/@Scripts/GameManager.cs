using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager instance = null;
    private GameManager() { }

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

    int _score = 0;
    public int Score
    {
        get { return _score; }
        set
        {
            _score = value;
            UI_Game.ScoreChangeAction?.Invoke();
        }
    }

    void Initiate()
    {
        _score = 0;
    }

}
