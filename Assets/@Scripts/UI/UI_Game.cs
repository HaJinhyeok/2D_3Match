using System;
using UnityEngine;
using UnityEngine.UI;

public class UI_Game : MonoBehaviour
{
    public Text ScoreText;

    public static Action ScoreChangeAction;

    void Start()
    {
        ScoreText.text = $"SCORE : {GameManager.Instance.Score}";
        ScoreChangeAction += OnScoreChange;
    }

    void OnScoreChange()
    {
        ScoreText.text = $"SCORE : {GameManager.Instance.Score}";
    }

    private void OnDestroy()
    {
        ScoreChangeAction -= OnScoreChange;
    }
}
