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
                instance = new GameManager();
            return instance;
        }
    }

    int _score = 0;
    public int Score
    {
        get { return _score; }
        set { _score = value; }
    }

    void Start()
    {

    }

    void Initiate()
    {
        _score = 0;
    }

}
