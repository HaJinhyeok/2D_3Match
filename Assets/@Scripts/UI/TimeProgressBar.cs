using System;
using UnityEngine;
using UnityEngine.UI;

public class TimeProgressBar : MonoBehaviour
{
    public Image TimerFillImage;

    public static Action OnTimeChange;

    void Start()
    {
        OnTimeChange += TimeChange;
        TimerFillImage.fillAmount = GameManager.Instance.CurrentTime / Define.TimeLimit;
    }

    private void OnDestroy()
    {
        OnTimeChange -= TimeChange;
    }

    void TimeChange()
    {
        TimerFillImage.fillAmount = GameManager.Instance.CurrentTime / Define.TimeLimit;
    }
}
