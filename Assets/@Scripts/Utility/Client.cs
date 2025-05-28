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
        try
        {
            while (true)
            {
                // byte 데이터 읽다가 짤릴 수 있으므로 다 읽을 때까지 확이 작업
                byte[] headerBuffer = new byte[4];
                int headerRead = 0;
                while (headerRead < 4)
                {
                    int read = networkStream.Read(headerBuffer, headerRead, 4 - headerRead);
                    if (read <= 0) throw new Exception("Server disconnected.");
                    headerRead += read;
                }

                ushort size = BitConverter.ToUInt16(headerBuffer, 0);
                ushort type = BitConverter.ToUInt16(headerBuffer, 2);
                string data = "";
                if (size > 4)
                {
                    byte[] bodyBuffer = new byte[size - 4];
                    int bodyRead = 0;
                    while (bodyRead < bodyBuffer.Length)
                    {
                        int read = networkStream.Read(bodyBuffer, bodyRead, bodyBuffer.Length - bodyRead);
                        if (read <= 0) throw new Exception("No data");
                        bodyRead += read;
                    }
                    data = Encoding.UTF8.GetString(bodyBuffer);
                    Debug.Log($"[RECV] PacketType: {(PacketType)type}, Size: {size}, Payload Length: {data.Length}");
                }


                switch ((PacketType)type)
                {
                    case PacketType.PACKET_MATCH_REQUEST:
                        break;

                    case PacketType.PACKET_MATCH_WAITING:
                        ClientReceiveProcessor.Enqueue(() =>
                        UI_Waiting.OnWaitingAction?.Invoke(true));
                        break;

                    case PacketType.PACKET_MATCH_COMPLETE:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            UI_Waiting.OnWaitingAction?.Invoke(false);
                            GameManager.Instance.Player.GameStart();
                            GameManager.Instance.Rival.Name = data;
                        });
                        break;

                    case PacketType.PACKET_MATCH_START:
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.StartGame(data));
                        break;

                    case PacketType.PACKET_MATCH_FINISH:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.IsRivalPlaying = false;
                            GameManager.Instance.GameStatus.RivalScore = int.Parse(data);
                        });
                        break;

                    case PacketType.PACKET_MATCH_RESULT:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(ushort.Parse(data));
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_MATCH_EXIT:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.IsPaused = false;
                            SceneManager.LoadScene(Define.MainScene);
                        });
                        break;

                    case PacketType.PACKET_RESULT_WIN:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_RESULT_LOSE:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_RESULT_DRAW:
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    // 인게임 블록 이동 정보
                    case PacketType.PACKET_SWAP:
                        ClientReceiveProcessor.Enqueue(() =>
                        StartCoroutine(GameManager.Instance.Rival.SwapBlock(data)));
                        break;

                    case PacketType.PACKET_DESTROY:
                        ClientReceiveProcessor.Enqueue(() =>
                            GameManager.Instance.Rival.DestroyBlock(data));
                        break;

                    case PacketType.PACKET_GENERATE:
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.GenerateBlock(data));
                        break;

                    case PacketType.PACKET_HIDE:
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.HideBlock(data));
                        break;

                    // 에러 및 예외처리
                    case PacketType.PACKET_ERR_FULL:
                        Debug.Log("Match is already ongoing...");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            SceneManager.LoadScene(Define.MainScene);
                            GameManager.s_isNetworkOn = false;
                        });
                        break;

                    case PacketType.PACKET_ERR_DISCONNECTION:
                        Debug.Log(Define.RivalConnectionFailText);
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            PlayerBoard.OnRivalConnectionError?.Invoke();
                            GameManager.Instance.Rival.FinishGame();
                        });
                        break;
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
            });
        }
    }

    public void SendMessageToServer(byte[] data)
    {
        if (networkStream == null || !networkStream.CanWrite)
            return;
        try
        {
            networkStream.Write(data, 0, data.Length);
        }
        catch (Exception e)
        {
            Debug.LogError("Send error: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        thread?.Abort();
        networkStream?.Close();
        client?.Close();
    }

    public void Initialize() { }
}
