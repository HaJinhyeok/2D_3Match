using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;
using UnityEngine.SceneManagement;

public class Client : MonoBehaviour
{
    TcpClient client;
    NetworkStream networkStream;
    Thread thread;

    public void ConnectToServer(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);
            networkStream = client.GetStream();

            Debug.Log($"Connected to server with {port} port.");

            thread = new Thread(RecvData);
            thread.Start();
            GameManager.s_isNetworkOn = true;
            SceneManager.LoadScene(Define.MatchGameScene);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Connection Error: {ex.Message}");
        }
    }

    void RecvData()
    {
        byte[] buffer = new byte[1024];
        try
        {
            while (true)
            {
                int bytes = networkStream.Read(buffer, 0, buffer.Length);
                if (bytes <= 0)
                {
                    Debug.LogWarning("Server disdconnected.");
                    break;
                }

                string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                string[] messages = msg.Split(' ');
                if (messages[0] == "MATCHED")
                {
                    Debug.Log("Matching Success!!!");
                    ClientReceiveProcessor.Enqueue(() =>
                    {
                        UI_Waiting.OnWaitingAction?.Invoke(false);
                        GameManager.Instance.Player.GameStart();
                        GameManager.Instance.Player.Name = messages[1];
                        GameManager.Instance.Rival.Name = messages[2];
                    });
                }
                else if (messages[0] == "WAITING")
                {
                    Debug.Log("Waiting...");
                    ClientReceiveProcessor.Enqueue(() =>
                    UI_Waiting.OnWaitingAction?.Invoke(true));
                }
                else if (messages[0] == Define.RivalConnectionError)
                {
                    Debug.Log(Define.RivalConnectionFailText);
                    ClientReceiveProcessor.Enqueue(() =>
                    {
                        PlayerBoard.OnRivalConnectionError?.Invoke();
                        GameManager.Instance.Rival.FinishGame();
                    });
                }
                else
                {
                    int status = (int)msg[0] - 48;
                    switch (status)
                    {
                        case (int)Define.DataStatus.Start:
                            ClientReceiveProcessor.Enqueue(() =>
                            GameManager.Instance.Rival.StartGame(msg.Substring(2)));

                            break;

                        case (int)Define.DataStatus.Swap:
                            ClientReceiveProcessor.Enqueue(() =>
                            StartCoroutine(GameManager.Instance.Rival.SwapBlock(msg.Substring(2))));
                            break;

                        case (int)Define.DataStatus.Destroy:
                            ClientReceiveProcessor.Enqueue(() =>
                                GameManager.Instance.Rival.DestroyBlock(msg.Substring(2)));
                            break;

                        case (int)Define.DataStatus.Generate:
                            ClientReceiveProcessor.Enqueue(() =>
                            GameManager.Instance.Rival.GenerateBlock(msg.Substring(2)));
                            break;

                        case (int)Define.DataStatus.Hide:
                            ClientReceiveProcessor.Enqueue(() =>
                            GameManager.Instance.Rival.HideBlock(msg.Substring(2)));
                            break;

                        case (int)Define.DataStatus.Result:
                            ClientReceiveProcessor.Enqueue(() =>
                            {
                                GameManager.Instance.GameStatus.OnResultSetting(msg[2] - '0');
                                ResultPanel.OnResultPanelOn?.Invoke();
                            });
                            break;

                        case (int)Define.DataStatus.Finish:
                            ClientReceiveProcessor.Enqueue(() =>
                            {
                                GameManager.Instance.GameStatus.IsRivalPlaying = false;
                                GameManager.Instance.GameStatus.RivalScore = int.Parse(msg.Substring(2));
                            });
                            break;

                        default:
                            break;
                    }
                }

            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Receive Error: {e.Message}");
            ClientReceiveProcessor.Enqueue(() =>
            {
                GameManager.s_isNetworkOn = false;
                SceneManager.LoadScene(Define.MainScene);
            }
            );
        }
    }

    public void SendMessageToServer(string msg)
    {
        if (networkStream == null || !networkStream.CanWrite)
            return;

        byte[] data = Encoding.UTF8.GetBytes(msg);
        networkStream.Write(data, 0, data.Length);
    }

    private void OnApplicationQuit()
    {
        thread?.Abort();
        networkStream?.Close();
        client?.Close();
    }

    public void Initialize() { }
}
