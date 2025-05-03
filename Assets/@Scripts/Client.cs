using UnityEngine;
using System.Net.Sockets;
using System.Threading;
using System;
using System.Text;

public class Client : MonoBehaviour
{
    TcpClient client;
    NetworkStream networkStream;
    Thread thread;
    RivalBoard rivalBoard;

    void Start()
    {
        ConnectToServer("127.0.0.1", 9000);
        GameObject parent = GetComponentInParent<Transform>().gameObject;
        DontDestroyOnLoad(parent);
        rivalBoard = FindAnyObjectByType<RivalBoard>();
    }

    void ConnectToServer(string ip, int port)
    {
        try
        {
            client = new TcpClient();
            client.Connect(ip, port);
            networkStream = client.GetStream();

            Debug.Log($"Connected to server with {port} port.");

            thread = new Thread(RecvData);
            thread.Start();
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
                //Debug.Log($"Received from server: {msg}");
                switch ((int)msg[0] - 48)
                {
                    case (int)Define.DataStatus.Start:
                        ClientReceiveProcessor.Enqueue(() =>
                        rivalBoard.StartGame(msg.Substring(2)));
                        
                        break;

                    case (int)Define.DataStatus.Swap:
                        ClientReceiveProcessor.Enqueue(()=>
                        StartCoroutine(rivalBoard.SwapBlock(msg.Substring(2))));
                        break;

                    case (int)Define.DataStatus.Destroy:
                        ClientReceiveProcessor.Enqueue(()=>
                            rivalBoard.DestroyBlock(msg.Substring(2)));
                        break;

                    case (int)Define.DataStatus.Generate:
                        ClientReceiveProcessor.Enqueue(() =>
                        rivalBoard.GenerateBlock(msg.Substring(2)));
                        break;

                    case (int)Define.DataStatus.Hide:
                        ClientReceiveProcessor.Enqueue(() =>
                        rivalBoard.HideBlock(msg.Substring(2)));
                        break;

                    default:
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Receive Error: {e.Message}");
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
}
