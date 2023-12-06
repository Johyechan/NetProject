using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using TMPro;

public class SocketModule : Singleton<SocketModule>
{
    private TcpClient clientSocket;
    private NetworkStream serverStream = default(NetworkStream);
    private string nickName;
    bool isRunning = false;

    public void Login(string id)
    {
        if (!isRunning)
        {
            clientSocket = new TcpClient();
            IPAddress address = IPAddress.Parse("127.0.0.1");
            clientSocket.Connect(address, 8888);
            serverStream = clientSocket.GetStream();

            byte[] outStream = Encoding.UTF8.GetBytes($"{id}$");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();

            Thread ctThread = new Thread(getMessage);
            ctThread.Start();
            isRunning = true;
            nickName = id;
        }
    }
    public void SendData(string str)
    {
        if (isRunning && serverStream != null)
        {
            byte[] outStream = Encoding.UTF8.GetBytes($"${str}");
            serverStream.Write(outStream, 0, outStream.Length);
            serverStream.Flush();
        }
    }

    private void StopThread()
    {
        isRunning = false;
    }

    public void Logout()
    {
        if (isRunning)
        {
            StopThread();
            nickName = "";
        }
        if (serverStream != null)
        {
            serverStream.Close();
            serverStream = null;
        }
        clientSocket.Close();
        GameManager.Instance.TextBox("Server", "Logout");
    }

    private void getMessage()
    {
        byte[] inStream = new byte[1024];
        string returnData = "";
        try
        {
            while (isRunning)
            {
                serverStream = clientSocket.GetStream();
                int buffSize = clientSocket.ReceiveBufferSize;
                int numBytesRead;

                if (serverStream.DataAvailable)
                {
                    while (serverStream.DataAvailable)
                    {
                        numBytesRead = serverStream.Read(inStream,0,inStream.Length);
                        returnData = Encoding.UTF8.GetString(inStream, 0, numBytesRead);
                    }
                    Debug.Log($"Server-{returnData}");
                    GameManager.Instance.QueueCommand(returnData);
                }
            }
        }
        catch (Exception ex)
        {
            StopThread();
        }
    }
}
