using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;

public class DroneTCPServer : MonoBehaviour
{
    private TcpListener tcpListener;
    private TcpClient connectedTcpClient;
    public DroneFlyController droneController; // Ссылка на контроллер дрона
    private bool isRunning;

    private void Start()
    {
        StartServer();
    }

    private async void StartServer()
    {
        try
        {
            tcpListener = new TcpListener(IPAddress.Parse("127.0.0.1"), 12345);
            tcpListener.Start();
            isRunning = true;
            Debug.Log("Server is listening");

            while (isRunning)
            {
                TcpClient client = await tcpListener.AcceptTcpClientAsync();
                Debug.Log("Client connected");
                HandleClient(client);
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in StartServer: " + ex.Message);
        }
    }

    private async void HandleClient(TcpClient client)
    {
        connectedTcpClient = client;
        var stream = client.GetStream();
        byte[] buffer = new byte[1024];

        try
        {
            while (isRunning && client.Connected)
            {
                if (stream.DataAvailable)
                {
                    int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);
                    if (bytesRead > 0)
                    {
                        string clientMessage = Encoding.UTF8.GetString(buffer, 0, bytesRead).Trim();
                        Debug.Log("Received message from client: " + clientMessage);
                        ProcessClientMessage(clientMessage);
                    }
                }
                await System.Threading.Tasks.Task.Delay(100); // Добавляем небольшую задержку для уменьшения нагрузки
            }
        }
        catch (Exception ex)
        {
            Debug.LogError("Exception in HandleClient: " + ex.Message);
        }
        finally
        {
            client.Close();
            connectedTcpClient = null;
            Debug.Log("Client disconnected");
        }
    }

    private void ProcessClientMessage(string message)
    {
        message = message.Trim();
        if (string.IsNullOrEmpty(message)) return;  // Игнорировать пустые сообщения

        string[] tokens = message.Split(' ');
        if (tokens.Length == 0) return;

        string command = tokens[0].ToLower();
        Debug.Log("Command received: " + command);  // Отладка

        switch (command)
        {
            case "move":
                if (tokens.Length == 4)
                {
                    if (float.TryParse(tokens[1], out float moveX) &&
                        float.TryParse(tokens[2], out float moveY) &&
                        float.TryParse(tokens[3], out float moveZ))
                    {
                        Debug.Log($"Executing move command with X: {moveX}, Y: {moveY}, Z: {moveZ}");  // Отладка
                        droneController.SetMovement(moveX, moveY, moveZ);
                    }
                    else
                    {
                        Debug.LogWarning("Invalid move command arguments.");
                    }
                }
                else
                {
                    Debug.LogWarning("Move command requires 3 arguments.");
                }
                break;

            case "shit":
                Debug.Log("Executing shit command"); // Отладка
                droneController.shit();
                break;

            case "switchcamera":
                Debug.Log("Executing switchcamera command"); // Отладка
                droneController.SwitchCamera();
                break;

            case "hovermode":
                droneController.HoverMode();
                break;

            case "get_speed":
                float speed = droneController.GetCurrentSpeed();
                SendDataToClient(speed.ToString("F2"));
                break;

            case "get_altitude":
                float altitude = droneController.GetCurrentAltitude();
                SendDataToClient(altitude.ToString("F2"));
                break;

            case "get_acceleration":
                float acceleration = droneController.GetCurrentAcceleration();
                SendDataToClient(acceleration.ToString("F2"));
                break;

            case "get_roll":
                float roll = droneController.GetCurrentRollAngle();
                SendDataToClient(roll.ToString("F2"));
                break;

            case "get_pitch":
                float pitch = droneController.GetCurrentPitchAngle();
                SendDataToClient(pitch.ToString("F2"));
                break;

            case "get_yaw":
                float yaw = droneController.GetCurrentYawAngle();
                SendDataToClient(yaw.ToString("F2"));
                break;

            case "get_direction":
                float direction = droneController.GetCurrentDirection();
                SendDataToClient(direction.ToString("F2"));
                break;

            case "set_wind_strength": // Новый кейс для установки силы ветра
                if (tokens.Length == 2 && float.TryParse(tokens[1], out float newWindStrength))
                {
                    Debug.Log($"Executing set_wind_strength command with strength: {newWindStrength}");  // Отладка
                    droneController.SetWindStrength(newWindStrength);
                }
                else
                {
                    Debug.LogWarning("Invalid set_wind_strength command arguments.");
                }
                break;

            case "exit":
                SendDataToClient("exit");
                isRunning = false;
                break;

            default:
                Debug.LogWarning("Unknown command: " + message);
                break;
        }
    }

    private void SendDataToClient(string data)
    {
        if (connectedTcpClient != null && connectedTcpClient.Connected)
        {
            try
            {
                NetworkStream stream = connectedTcpClient.GetStream();
                byte[] msg = Encoding.UTF8.GetBytes(data + "\n");
                stream.Write(msg, 0, msg.Length);
            }
            catch (Exception ex)
            {
                Debug.LogError("Exception in SendDataToClient: " + ex.Message);
            }
        }
    }

    private void OnApplicationQuit()
    {
        isRunning = false;
        tcpListener?.Stop();
        if (connectedTcpClient != null)
        {
            connectedTcpClient.Close();
        }
    }
}
