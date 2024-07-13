package com.example;

import java.io.*;
import java.net.*;
import java.util.Scanner;

public class DroneTCPClient {

    private Socket socket;
    private PrintWriter out;
    private BufferedReader in;
    private Scanner scanner;
    private volatile boolean running = true;

    public DroneTCPClient(String serverAddress, int serverPort) {
        try {
            socket = new Socket(serverAddress, serverPort);
            out = new PrintWriter(socket.getOutputStream(), true);
            in = new BufferedReader(new InputStreamReader(socket.getInputStream()));
            scanner = new Scanner(System.in);
            System.out.println("Connected to server at " + serverAddress + ":" + serverPort);

            new Thread(new ResponseHandler()).start();
        } catch (IOException e) {
            System.err.println("Connection error: " + e.getMessage());
        }
    }

    public void sendCommand(String command) {
        System.out.println("Sending command: " + command); // Отладка
        out.println(command);  // Добавляется \n автоматически
        out.flush();
    }

    public void close() {
        running = false;
        try {
            in.close();
            out.close();
            socket.close();
            System.out.println("Connection closed."); // Отладка
        } catch (IOException e) {
            System.err.println("Close error: " + e.getMessage());
        }
    }

    private class ResponseHandler implements Runnable {
        @Override
        public void run() {
            while (running) {
                try {
                    String response = in.readLine();
                    if (response != null) {
                        System.out.println("Server response: " + response);
                    } else {
                        break;
                    }
                } catch (IOException e) {
                    System.err.println("Read error: " + e.getMessage());
                    break;
                }
            }
        }
    }

    public static void main(String[] args) {
        DroneTCPClient client = new DroneTCPClient("127.0.0.1", 12345);

        System.out.println("Connected to drone server.");

        while (true) {
            System.out.println("Enter command (move <x> <y> <z> |" +
                    " hovermode | switchcamera | get_speed " +
                    "| get_altitude | " +
                    "get_acceleration | get_roll | get_pitch | get_yaw |" +
                    "get_direction" +
                    "set_wind_strength" +
                    " exit):");
            String command = client.scanner.nextLine();

            client.sendCommand(command);
            System.out.println("Command sent: " + command); // Отладка

            if (command.equals("exit")) {
                System.out.println("Exiting...");
                break;
            }
        }

        client.close();
    }
}

