using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UI_Main : MonoBehaviour
{
    public Button SoloPlayButton;
    public Button MatchPlayButton;
    public Button ExitButton;

    public Text ScreenText;

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
        SceneManager.LoadScene(Define.SoloGameScene);
    }

    public void OnMatchPlayButton()
    {
        SceneManager.LoadScene(Define.MatchGameScene);
    }

    public void OnExitButtonClick()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
