using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour
{
    public Board PlayerBoard;
    public Board RivalBoard;
    public Client Client;
    public GameObject ResultPanel;

    public static Action OnGameFinish;

    private void Start()
    {
        OnGameFinish += GameResult;
    }

    private void OnDestroy()
    {
        OnGameFinish -= GameResult;
    }

    void GameResult()
    {

    }
}
