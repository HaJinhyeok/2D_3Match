using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    [SerializeField] Button SoloPlayButton;
    [SerializeField] Button MatchPlayButton;
    [SerializeField] Button ExitButton;
    [SerializeField] Text ScreenText;
    [SerializeField] AudioSource ButtonSound;

    void Start()
    {
        SoloPlayButton.onClick.AddListener(OnSoloPlayButtonClick);
        MatchPlayButton.onClick.AddListener(OnMatchPlayButton);
        ExitButton.onClick.AddListener(OnExitButtonClick);
        GameManager.Instance.GameInitialize();
        GameManager.Client.Initialize();

        ScreenText.text = Screen.width + "x" + Screen.height;
    }

    public void OnSoloPlayButtonClick()
    {
        ButtonSound.Play();
        SceneManager.LoadScene(Define.SoloGameScene);
    }

    public void OnMatchPlayButton()
    {
        ButtonSound.Play();
        GameManager.Client.ConnectToServer(Define.address, Define.PORT);
    }

    public void OnExitButtonClick()
    {
        ButtonSound.Play();
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
