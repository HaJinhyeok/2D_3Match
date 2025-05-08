using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public Text GameResultText;
    public Text PlayerScoreText;
    public Text RivalScoreText;
    public Button NextGameButton;
    public Button BackButton;

    public static Action OnResultPanelOn;

    void Start()
    {
        OnResultPanelOn += ResultPanelOn;
        BackButton.onClick.AddListener(OnBackButtonClick);
        NextGameButton.onClick.AddListener(OnNextGameButtonClick);
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnResultPanelOn -= ResultPanelOn;
    }

    void ResultPanelOn()
    {
        gameObject.SetActive(true);
        GameResultText.text = GameManager.Instance.GameStatus.GameResult;
        PlayerScoreText.text = $"{GameManager.Instance.GameStatus.PlayerScore}Á¡";
        if (SceneManager.GetActiveScene().name == Define.MatchGameScene)
        {
            RivalScoreText.text = $"{GameManager.Instance.GameStatus.RivalScore}Á¡";
        }
    }

    void OnNextGameButtonClick()
    {
        PlayerBoard player = FindAnyObjectByType<PlayerBoard>();
        player.ClearBoard();
        if (SceneManager.GetActiveScene().name == Define.SoloGameScene)
        {
            PlayerBoard.OnGameStart?.Invoke();
        }
        else if (SceneManager.GetActiveScene().name == Define.MatchGameScene)
        {
            RivalBoard rival = FindAnyObjectByType<RivalBoard>();
            rival.ClearBoard();
            GameManager.Client.SendMessageToServer("MATCH");
            UI_Waiting.OnWaitingAction?.Invoke(true);
        }
        gameObject.SetActive(false);
    }

    void OnBackButtonClick()
    {
        GameManager.Client.SendMessageToServer($"{(int)Define.DataStatus.ExitMatch}");
        SceneManager.LoadScene(Define.MainScene);
    }
}
