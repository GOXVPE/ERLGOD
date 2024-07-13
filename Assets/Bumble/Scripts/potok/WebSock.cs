using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Net.Sockets;
using System.Threading;

public class WebSocketClient : MonoBehaviour
{
    private TcpClient tcpClient;
    private NetworkStream stream;
    private byte[] receiveBuffer = new byte[4096];
    private StringBuilder messageBuilder = new StringBuilder();

    private bool connected = false;
    private Thread receiveThread;

    // WebSocket handshake headers
    private string handshakeRequest = "GET / HTTP/1.1\r\n" +
                                      "Host: localhost\r\n" +
                                      "Upgrade: websocket\r\n" +
                                      "Connection: Upgrade\r\n" +
                                      "Sec-WebSocket-Key: dGhlIHNhbXBsZSBub25jZQ==\r\n" +
                                      "Sec-WebSocket-Version: 13\r\n\r\n";

    void Start()
    {
        ConnectWebSocket();
    }

    void Update()
    {
        if (connected)
        {
            ReceiveMessages();
        }
    }

    void OnDestroy()
    {
        DisconnectWebSocket();
    }

    private void ConnectWebSocket()
    {
        tcpClient = new TcpClient();
        tcpClient.Connect("localhost", 2567);
        stream = tcpClient.GetStream();

        byte[] handshakeRequestBytes = Encoding.UTF8.GetBytes(handshakeRequest);
        stream.Write(handshakeRequestBytes, 0, handshakeRequestBytes.Length);

        connected = true;
        receiveThread = new Thread(new ThreadStart(ReceiveThread));
        receiveThread.Start();
    }

    private void ReceiveThread()
    {
        while (connected)
        {
            int bytesRead = stream.Read(receiveBuffer, 0, receiveBuffer.Length);
            if (bytesRead > 0)
            {
                byte[] data = new byte[bytesRead];
                Array.Copy(receiveBuffer, data, bytesRead);

                // Process incoming WebSocket frames (not fully implemented here)
                ProcessWebSocketFrame(data);
            }
        }
    }

    private void ProcessWebSocketFrame(byte[] data)
    {
        // Implement WebSocket frame processing according to RFC 6455
        // This is a simplified example and does not handle masking, continuation frames, etc.
        // For production use, consider using a WebSocket library that handles these details.

        // Extract payload data
        byte[] payloadData = new byte[data.Length - 6];
        Array.Copy(data, 6, payloadData, 0, payloadData.Length);

        string message = Encoding.UTF8.GetString(payloadData);
        Debug.Log("Received message: " + message);
    }

    private void ReceiveMessages()
    {
        // Placeholder for receiving messages if needed in Update loop
    }

    private void DisconnectWebSocket()
    {
        connected = false;
        if (receiveThread != null && receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }

        if (stream != null)
        {
            stream.Close();
        }

        if (tcpClient != null)
        {
            tcpClient.Close();
        }
    }
}
