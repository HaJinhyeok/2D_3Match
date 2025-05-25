using System;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Waiting : MonoBehaviour
{
    public Camera WaitingCamera;
    public Button ExitButton;
    public static Action<bool> OnWaitingAction;

    private void Awake()
    {
        OnWaitingAction += WaitingWindowOnOff;
        ExitButton.onClick.AddListener(OnExitButtonClick);
        WaitingWindowOnOff(false);
    }

    private void OnDestroy()
    {
        OnWaitingAction -= WaitingWindowOnOff;
    }

    void WaitingWindowOnOff(bool flag)
    {
        WaitingCamera.gameObject.SetActive(flag);
        gameObject.SetActive(flag);
    }

    void OnExitButtonClick()
    {
        byte[] clientData = PacketBuilder.BuildPacketData(PacketType.PACKET_MATCH_EXIT);
        GameManager.Client.SendMessageToServer(clientData);
        //GameManager.Client.SendMessageToServer($"{(int)Define.DataStatus.ExitMatch}");
        GameManager.s_isNetworkOn = false;
    }
}
