using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using StackExchange.Redis;

namespace ChatServer
{
    class ChatRoom
    {
        public string Name { get; }
        public List<Socket> Clients { get; }

        public ChatRoom(string name)
        {
            Name = name;
            Clients = new List<Socket>();
        }

        public void AddClient(Socket client)
        {
            Clients.Add(client);
        }

        public void RemoveClient(Socket client)
        {
            Clients.Remove(client);
        }

        public void BroadcastMessage(string message, Socket sender)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            foreach (var client in Clients)
            {
                if (client != sender)
                {
                    NetworkStream stream = new NetworkStream(client);
                    stream.Write(buffer, 0, buffer.Length);
                    stream.Flush();
                }
            }
        }
    }

    class ChatServer
    {
        private Dictionary<string, ChatRoom> rooms = new Dictionary<string, ChatRoom>();
        private ConnectionMultiplexer redis;
        private IDatabase db;

        public void Start()
        {
            // Connect to Redis
            redis = ConnectionMultiplexer.Connect("localhost:6379");
            db = redis.GetDatabase();

            TcpListener serverSocket = new TcpListener(IPAddress.Any, 8888);
            serverSocket.Start();
            Console.WriteLine("Chat Server Started...");

            while (true)
            {
                Socket clientSocket = serverSocket.AcceptSocket();
                Console.WriteLine("Client Connected: " + clientSocket.RemoteEndPoint);
                Thread clientThread = new Thread(() => HandleClient(clientSocket));
                clientThread.Start();
            }
        }

        private void HandleClient(Socket clientSocket)
        {
            NetworkStream networkStream = new NetworkStream(clientSocket);
            byte[] buffer = new byte[1024];
            int bytesReceived;

            // Get client's session key from Redis
            string sessionKey = GetSessionKeyFromRedis();

            // Perform authentication using session key if needed
            if (string.IsNullOrEmpty(sessionKey))
            {
                SendMessage("Authentication failed. Session key not found.", clientSocket);
                clientSocket.Close();
                return;
            }

            // Get client's name
            bytesReceived = networkStream.Read(buffer, 0, buffer.Length);
            string clientName = Encoding.ASCII.GetString(buffer, 0, bytesReceived);

            while (true)
            {
                bytesReceived = networkStream.Read(buffer, 0, buffer.Length);
                if (bytesReceived == 0)
                    break;

                string message = Encoding.ASCII.GetString(buffer, 0, bytesReceived);
                Console.WriteLine(clientName + ": " + message);

                if (message.StartsWith("/create"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];
                        CreateRoom(clientSocket, clientName, roomName);
                    }
                    else
                    {
                        SendMessage("Invalid /create command. Usage: /create [room_name]", clientSocket);
                    }
                }
                else if (message.StartsWith("/join"))
                {
                    string[] parts = message.Split(' ');
                    if (parts.Length == 2)
                    {
                        string roomName = parts[1];
                        JoinRoom(clientSocket, clientName, roomName);
                    }
                    else
                    {
                        SendMessage("Invalid /join command. Usage: /join [room_name]", clientSocket);
                    }
                }
                else if (message.StartsWith("/list"))
                {
                    SendRoomList(clientSocket);
                }
                else
                {
                    SendMessage("Invalid command. Available commands: /create [room_name], /join [room_name], /list", clientSocket);
                }
            }

            clientSocket.Close();
        }

        private string GetSessionKeyFromRedis()
        {
            string sessionKey = db.StringGet("session_key");
            return sessionKey;
        }

        private void CreateRoom(Socket clientSocket, string clientName, string roomName)
        {
            if (!rooms.ContainsKey(roomName))
            {
                rooms.Add(roomName, new ChatRoom(roomName));
                SendMessage("Room created: " + roomName, clientSocket);
            }
            else
            {
                SendMessage("Room already exists: " + roomName, clientSocket);
            }
        }

        private void JoinRoom(Socket clientSocket, string clientName, string roomName)
        {
            if (rooms.ContainsKey(roomName))
            {
                rooms[roomName].AddClient(clientSocket);
                SendMessage("Joined room: " + roomName, clientSocket);
                rooms[roomName].BroadcastMessage(clientName + " has joined the room.", clientSocket);
            }
            else
            {
                SendMessage("Room does not exist: " + roomName, clientSocket);
            }
        }

        private void SendRoomList(Socket clientSocket)
        {
            SendMessage("Available rooms: " + string.Join(", ", rooms.Keys), clientSocket);
        }

        private void SendMessage(string message, Socket clientSocket)
        {
            byte[] buffer = Encoding.ASCII.GetBytes(message);
            NetworkStream stream = new NetworkStream(clientSocket);
            stream.Write(buffer, 0, buffer.Length);
            stream.Flush();
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            ChatServer server = new ChatServer();
            server.Start();
        }
    }
}