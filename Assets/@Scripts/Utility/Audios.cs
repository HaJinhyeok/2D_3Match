using System;
using UnityEngine;

public class Audios : MonoBehaviour
{
    [SerializeField] AudioSource ButtonSound;
    [SerializeField] AudioSource PreferenceSound;
    [SerializeField] AudioSource WinSound;
    [SerializeField] AudioSource LoseSound;
    [SerializeField] AudioSource BlockSound;

    public static Action OnButtonSoundPlay;
    public static Action OnPreferenceSoundPlay;
    public static Action OnWinSoundPlay;
    public static Action OnLoseSoundPlay;
    public static Action OnBlockSoundPlay;

    private void Awake()
    {
        OnButtonSoundPlay += ButtonSoundPlay;
        OnPreferenceSoundPlay += PreferenceSoundPlay;
        OnWinSoundPlay += WinSoundPlay;
        OnLoseSoundPlay += LoseSoundPlay;
        OnBlockSoundPlay += BlockSoundPlay;
    }

    private void OnDestroy()
    {
        OnButtonSoundPlay -= ButtonSoundPlay;
        OnPreferenceSoundPlay -= PreferenceSoundPlay;
        OnWinSoundPlay -= WinSoundPlay;
        OnLoseSoundPlay -= LoseSoundPlay;
        OnBlockSoundPlay -= BlockSoundPlay;
    }

    void ButtonSoundPlay()
    {
        ButtonSound.Play();
    }

    void PreferenceSoundPlay()
    {
        PreferenceSound.Play();
    }

    void WinSoundPlay()
    {
        WinSound.Play();
    }

    void LoseSoundPlay()
    {
        LoseSound.Play();
    }

    void BlockSoundPlay()
    {
        BlockSound.Play();
    }

}
