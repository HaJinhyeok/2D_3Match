using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ResultPanel : MonoBehaviour
{
    public Text GameResultText;
    public Text PlayerScoreText;
    public Text RivalScoreText;
    public Button BackButton;

    public static Action OnResultPanelOn;

    void Start()
    {
        OnResultPanelOn += ResultPanelOn;
        BackButton.onClick.AddListener(OnBackButtonClick);
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
        RivalScoreText.text = $"{GameManager.Instance.GameStatus.RivalScore}Á¡";
    }

    void OnBackButtonClick()
    {
        SceneManager.LoadScene(Define.MainScene);
    }
}
