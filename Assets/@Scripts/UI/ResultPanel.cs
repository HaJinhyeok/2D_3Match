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
        if (GameManager.Instance.GameStatus.GameResult == Define.LoseText)
        {
            Audios.OnLoseSoundPlay?.Invoke();
        }
        else
        {
            Audios.OnWinSoundPlay?.Invoke();
        }
            PlayerScoreText.text = $"{GameManager.Instance.GameStatus.PlayerScore}Á¡";
        if (GameManager.s_isNetworkOn)
        {
            RivalScoreText.text = $"{GameManager.Instance.GameStatus.RivalScore}Á¡";
        }
    }

    void OnNextGameButtonClick()
    {
        Audios.OnButtonSoundPlay?.Invoke();
        PlayerBoard player = FindAnyObjectByType<PlayerBoard>();
        player.ClearBoard();
        if (!GameManager.s_isNetworkOn)
        {
            PlayerBoard.OnGameStart?.Invoke();
        }
        else
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
        Audios.OnButtonSoundPlay?.Invoke();
        if (GameManager.s_isNetworkOn)
        {
            GameManager.Client.SendMessageToServer($"{(int)Define.DataStatus.ExitMatch}");
            GameManager.s_isNetworkOn = false;
        }
        SceneManager.LoadScene(Define.MainScene);
    }
}
