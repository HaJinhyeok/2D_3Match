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
                    Debug.LogWarning("Server disconnected.");
                    break;
                }

                ushort size = BitConverter.ToUInt16(buffer, 0);
                ushort type = BitConverter.ToUInt16(buffer, 2);
                string data = "";
                if (bytes > 4)
                {
                    data = Encoding.UTF8.GetString(buffer, 4, size - 4);
                    if ((PacketType)type == PacketType.PACKET_GENERATE)
                    {
                        Debug.Log($"[{(PacketType)type}]");
                        Debug.Log($"data: {data}");
                    }
                }

                switch ((PacketType)type)
                {
                    case PacketType.PACKET_MATCH_REQUEST:
                        break;

                    case PacketType.PACKET_MATCH_WAITING:
                        //Debug.Log("Waiting...");
                        ClientReceiveProcessor.Enqueue(() =>
                        UI_Waiting.OnWaitingAction?.Invoke(true));
                        break;

                    case PacketType.PACKET_MATCH_COMPLETE:
                        //Debug.Log("Match Completed!");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            UI_Waiting.OnWaitingAction?.Invoke(false);
                            GameManager.Instance.Player.GameStart();
                            //GameManager.Instance.Player.Name = messages[1];
                            GameManager.Instance.Rival.Name = data;
                        });
                        break;

                    case PacketType.PACKET_MATCH_START:
                        //Debug.Log("START");
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.StartGame(data));
                        break;

                    case PacketType.PACKET_MATCH_FINISH:
                        //Debug.Log("FINISH");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.IsRivalPlaying = false;
                            GameManager.Instance.GameStatus.RivalScore = int.Parse(data);
                        });
                        break;

                    case PacketType.PACKET_MATCH_RESULT:
                        //Debug.Log("RESULT");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(ushort.Parse(data));
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_MATCH_EXIT:
                        //Debug.Log("EXIT");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.IsPaused = false;
                            SceneManager.LoadScene(Define.MainScene);
                        });
                        break;

                    case PacketType.PACKET_RESULT_WIN:
                        //Debug.Log("WIN");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_RESULT_LOSE:
                        //Debug.Log("LOSE");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    case PacketType.PACKET_RESULT_DRAW:
                        //Debug.Log("DRAW");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            GameManager.Instance.GameStatus.OnResultSetting(type);
                            ResultPanel.OnResultPanelOn?.Invoke();
                        });
                        break;

                    // 인게임 블록 이동 정보
                    case PacketType.PACKET_SWAP:
                        //Debug.Log("SWAP");
                        ClientReceiveProcessor.Enqueue(() =>
                        StartCoroutine(GameManager.Instance.Rival.SwapBlock(data)));
                        break;

                    case PacketType.PACKET_DESTROY:
                        //Debug.Log("DESTROY");
                        ClientReceiveProcessor.Enqueue(() =>
                            GameManager.Instance.Rival.DestroyBlock(data));
                        break;

                    case PacketType.PACKET_GENERATE:
                        Debug.Log("GENERATE");
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.GenerateBlock(data));
                        break;

                    case PacketType.PACKET_HIDE:
                        //Debug.Log("HIDE");
                        ClientReceiveProcessor.Enqueue(() =>
                        GameManager.Instance.Rival.HideBlock(data));
                        break;

                    // 에러 및 예외처리
                    case PacketType.PACKET_ERR_FULL:
                        //Debug.Log("Match is already ongoing...");
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            SceneManager.LoadScene(Define.MainScene);
                            GameManager.s_isNetworkOn = false;
                        });
                        break;

                    case PacketType.PACKET_ERR_DISCONNECTION:
                        //Debug.Log(Define.RivalConnectionFailText);
                        ClientReceiveProcessor.Enqueue(() =>
                        {
                            PlayerBoard.OnRivalConnectionError?.Invoke();
                            GameManager.Instance.Rival.FinishGame();
                        });
                        break;
                }


                //string msg = Encoding.UTF8.GetString(buffer, 0, bytes);
                //string[] messages = msg.Split(' ');
                //if (messages[0] == "MATCHED")
                //{
                //    Debug.Log("Matching Success!!!");
                //    ClientReceiveProcessor.Enqueue(() =>
                //    {
                //        UI_Waiting.OnWaitingAction?.Invoke(false);
                //        GameManager.Instance.Player.GameStart();
                //        GameManager.Instance.Player.Name = messages[1];
                //        GameManager.Instance.Rival.Name = messages[2];
                //    });
                //}
                //else if (messages[0] == "WAITING")
                //{
                //    Debug.Log("Waiting...");
                //    ClientReceiveProcessor.Enqueue(() =>
                //    UI_Waiting.OnWaitingAction?.Invoke(true));
                //}
                //else if (messages[0] == "MATCH_FULL")
                //{
                //    Debug.Log("Match is already ongoing...");
                //    ClientReceiveProcessor.Enqueue(() =>
                //    {
                //        SceneManager.LoadScene(Define.MainScene);
                //        GameManager.s_isNetworkOn = false;
                //    });
                //}
                //else if (messages[0] == Define.RivalConnectionError)
                //{
                //    Debug.Log(Define.RivalConnectionFailText);
                //    ClientReceiveProcessor.Enqueue(() =>
                //    {
                //        PlayerBoard.OnRivalConnectionError?.Invoke();
                //        GameManager.Instance.Rival.FinishGame();
                //    });
                //}
                //else
                //{
                //    int status = (int)msg[0] - 48;
                //    switch (status)
                //    {
                //        case (int)Define.DataStatus.Start:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            GameManager.Instance.Rival.StartGame(msg.Substring(2)));

                //            break;

                //        case (int)Define.DataStatus.Swap:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            StartCoroutine(GameManager.Instance.Rival.SwapBlock(msg.Substring(2))));
                //            break;

                //        case (int)Define.DataStatus.Destroy:
                //            ClientReceiveProcessor.Enqueue(() =>
                //                GameManager.Instance.Rival.DestroyBlock(msg.Substring(2)));
                //            break;

                //        case (int)Define.DataStatus.Generate:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            GameManager.Instance.Rival.GenerateBlock(msg.Substring(2)));
                //            break;

                //        case (int)Define.DataStatus.Hide:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            GameManager.Instance.Rival.HideBlock(msg.Substring(2)));
                //            break;

                //        case (int)Define.DataStatus.Result:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            {
                //                GameManager.Instance.GameStatus.OnResultSetting(msg[2] - '0');
                //                ResultPanel.OnResultPanelOn?.Invoke();
                //            });
                //            break;

                //        case (int)Define.DataStatus.Finish:
                //            ClientReceiveProcessor.Enqueue(() =>
                //            {
                //                GameManager.Instance.GameStatus.IsRivalPlaying = false;
                //                GameManager.Instance.GameStatus.RivalScore = int.Parse(msg.Substring(2));
                //            });
                //            break;

                //        default:
                //            break;
                //    }
                //}

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

    //public void SendMessageToServer(string msg)
    //{
    //    if (networkStream == null || !networkStream.CanWrite)
    //        return;

    //    byte[] data = Encoding.UTF8.GetBytes(msg);
    //    networkStream.Write(data, 0, data.Length);
    //}

    public void SendMessageToServer(byte[] data)
    {
        if (networkStream == null || !networkStream.CanWrite)
            return;
        try
        {
            //Debug.Log("Sending Packet: " + BitConverter.ToString(data));
            networkStream.Write(data, 0, data.Length);
            //Debug.Log("Packet sent");
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
