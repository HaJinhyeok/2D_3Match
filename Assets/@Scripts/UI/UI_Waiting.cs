using System;
using UnityEngine;

public class UI_Waiting : MonoBehaviour
{
    public Camera WaitingCamera;
    public static Action<bool> OnWaitingAction;

    private void Awake()
    {
        OnWaitingAction += WaitingCameraOnOff;
        WaitingCamera.gameObject.SetActive(false);
    }

    private void OnDestroy()
    {
        OnWaitingAction -= WaitingCameraOnOff;
    }

    void WaitingCameraOnOff(bool flag)
    {
        WaitingCamera.gameObject.SetActive(flag);
    }
}
