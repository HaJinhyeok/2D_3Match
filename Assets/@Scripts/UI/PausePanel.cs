using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PausePanel : MonoBehaviour
{
    public Button ExitButton;
    public Button ContinueButton;
    public static Action OnPauseOff;

    void Start()
    {
        ExitButton.onClick.AddListener(OnExitButtonClick);
        ContinueButton.onClick.AddListener(OnContinueButtonClick);
        OnPauseOff += PausePanelOff;
        gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnPauseOff -= PausePanelOff;
    }

    void OnExitButtonClick()
    {
        Audios.OnPreferenceSoundPlay?.Invoke();
        // 매치 게임 중이면
        if(GameManager.s_isNetworkOn)
        {
            //GameManager.Client.SendMessageToServer($"{(int)Define.DataStatus.ExitMatch}");
            GameManager.Client.SendMessageToServer(PacketBuilder.BuildPacketData(PacketType.PACKET_MATCH_EXIT));
            GameManager.s_isNetworkOn = false;
        }
        else
        {
            Time.timeScale = 1.0f;
        }
            GameManager.Instance.IsPaused = false;
            SceneManager.LoadScene(Define.MainScene);
    }

    void OnContinueButtonClick()
    {
        Audios.OnPreferenceSoundPlay?.Invoke();
        GameManager.Instance.IsPaused = false;
    }

    void PausePanelOff()
    {
        Audios.OnPreferenceSoundPlay?.Invoke();
        // 솔로 게임 중이면
        if (!GameManager.s_isNetworkOn)
        {
            Time.timeScale = 1.0f;
        }
        gameObject.SetActive(false);
    }
}
